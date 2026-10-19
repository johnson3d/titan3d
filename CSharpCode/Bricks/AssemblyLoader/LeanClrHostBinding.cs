using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EngineNS.LeanCLR
{
    /// <summary>
    /// 标记游戏插件中允许被 LeanCLR 热更脚本调用的类。
    /// 只有该类上带 [Rtti.Meta] 且未设置 NoMacrossUseable 的方法会生成 intrinsic 代理。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TtLeanClrAttribute : Attribute
    {
    }

    /// <summary>
    /// 由 LeanClrGenerator 生成，指向插件程序集内的 ITtLeanClrRegistry 实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class TtLeanClrRegistryAttribute : Attribute
    {
        public TtLeanClrRegistryAttribute(Type registryType)
        {
            RegistryType = registryType;
        }
        public Type RegistryType { get; }
    }

    public interface ITtLeanClrRegistry
    {
        string StubAssemblyName { get; }
        void Register(TtLeanClrIntrinsicDispatcher dispatcher);
        void Unregister(TtLeanClrIntrinsicDispatcher dispatcher);
        // 宿主 Type -> VM 桩类全名
        void CollectHostTypes(List<KeyValuePair<Type, string>> hostTypes);
    }

    /// <summary>
    /// 宿主对象与 VM 桩对象的映射（方案 A：VM 桩对象的 HostHandle 字段保存宿主对象的弱 GCHandle）。
    /// 除 FBinding 终结器外，所有 VM 调用必须串行；运行期属主为 Logic 线程（见 TtLeanClrIntrinsicDispatcher.EnsureOwnerThread）。
    /// </summary>
    public static unsafe class TtLeanClrHostBinding
    {
        public const string EngineStubAssemblyName = "Titan.LeanClr.EngineStub";
        public const string HostObjectClassName = "EngineNS.LeanCLR.TtLeanClrHostObject";
        public const string HostHandleFieldName = "HostHandle";

        private readonly struct FHostType
        {
            public FHostType(string assemblyName, string className)
            {
                AssemblyName = assemblyName;
                ClassName = className;
            }
            public readonly string AssemblyName;
            public readonly string ClassName;
        }

        private sealed class FBinding
        {
            public FBinding(TtLeanClrObject vmObject, GCHandle hostHandle)
            {
                VmObject = vmObject;
                HostHandle = hostHandle;
            }
            public readonly TtLeanClrObject VmObject;
            public readonly GCHandle HostHandle;

            // 宿主对象已回收；VM 对象可能仍被脚本引用，必须回到 VM 线程先清 handle 字段再释放
            ~FBinding()
            {
                sReleased.Enqueue((VmObject, HostHandle));
            }
        }

        private static TtLeanClrRuntime sRuntime;
        private static int sHostHandleOffset = -1;
        private static int sStringLengthOffset;
        private static int sStringCharsOffset;
        private static readonly Dictionary<Type, FHostType> sHostTypes = new();
        private static readonly Dictionary<Type, FHostType?> sResolvedHostTypes = new();
        private static readonly ConditionalWeakTable<object, FBinding> sBindings = new();
        private static readonly ConcurrentQueue<(TtLeanClrObject VmObject, GCHandle HostHandle)> sReleased = new();

        internal static void Attach(TtLeanClrRuntime runtime)
        {
            sRuntime = runtime;
            sHostHandleOffset = -1;
            sStringLengthOffset = TtLeanClrRuntime.GetStringLengthOffset();
            sStringCharsOffset = TtLeanClrRuntime.GetStringCharsOffset();
        }

        internal static void Detach()
        {
            FlushReleased();
            sRuntime = default;
            sHostTypes.Clear();
            sResolvedHostTypes.Clear();
        }

        internal static bool RegisterHostTypes(string stubAssemblyName, List<KeyValuePair<Type, string>> hostTypes)
        {
            if (hostTypes.Count == 0)
                return true;
            if (!EnsureHostObjectClassBound())
                return false;
            // 桩程序集须在 VM 中已加载，GetOrCreate 才能在脚本首次引用前创建桩对象
            if (!sRuntime.LoadAssembly(stubAssemblyName))
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                    $"LeanCLR host types skipped: {stubAssemblyName} must be listed in LeanClrAssemblies.");
                return false;
            }
            foreach (var i in hostTypes)
                sHostTypes[i.Key] = new FHostType(stubAssemblyName, i.Value);
            sResolvedHostTypes.Clear();
            return true;
        }

        internal static void UnregisterHostTypes(List<KeyValuePair<Type, string>> hostTypes)
        {
            foreach (var i in hostTypes)
                sHostTypes.Remove(i.Key);
            sResolvedHostTypes.Clear();
        }

        /// <summary>
        /// 把已注册宿主类型改映射到另一个 VM 类（Macross 补丁类）；须在该类型首次 GetOrCreate 之前调用。
        /// </summary>
        internal static bool OverrideHostType(Type type, string assemblyName, string className)
        {
            if (!sHostTypes.ContainsKey(type))
                return false;
            sHostTypes[type] = new FHostType(assemblyName, className);
            sResolvedHostTypes.Clear();
            return true;
        }

        internal static bool IsRegisteredHostType(Type type)
        {
            return sHostTypes.ContainsKey(type);
        }

        private static bool EnsureHostObjectClassBound()
        {
            if (sHostHandleOffset >= 0)
                return true;
            if (!sRuntime.IsValidPointer)
                return false;
            if (!sRuntime.BindHostObjectClass(EngineStubAssemblyName, HostObjectClassName, HostHandleFieldName))
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error,
                    $"LeanCLR host binding failed: {EngineStubAssemblyName} must be listed in LeanClrAssemblies.");
                return false;
            }
            sHostHandleOffset = TtLeanClrRuntime.GetHostHandleOffset();
            return sHostHandleOffset >= 0;
        }

        private static FHostType? FindHostType(Type type)
        {
            if (sResolvedHostTypes.TryGetValue(type, out var result))
                return result;
            // 未标注的派生类映射到最近的已注册基类桩
            for (var cur = type; cur != null; cur = cur.BaseType)
            {
                if (sHostTypes.TryGetValue(cur, out var hostType))
                {
                    result = hostType;
                    break;
                }
            }
            sResolvedHostTypes[type] = result;
            return result;
        }

        /// <summary>
        /// 由 VM 桩对象指针取回宿主对象；宿主已回收或类型不符时返回 null。
        /// </summary>
        public static T ResolveHost<T>(void* vmObject) where T : class
        {
            if (vmObject == null || sHostHandleOffset < 0)
                return null;
            var handle = *(IntPtr*)((byte*)vmObject + sHostHandleOffset);
            if (handle == IntPtr.Zero)
                return null;
            return GCHandle.FromIntPtr(handle).Target as T;
        }

        /// <summary>
        /// 取宿主对象对应的 VM 桩对象指针；返回的 TtLeanClrObject 由映射表持有，调用方不得 Release。
        /// </summary>
        public static TtLeanClrObject GetOrCreate(object host)
        {
            if (host == null)
                return default;
            FlushReleased();
            if (sBindings.TryGetValue(host, out var binding))
                return binding.VmObject;

            var hostType = FindHostType(host.GetType());
            if (hostType == null)
                throw new InvalidOperationException($"{host.GetType().FullName} is not a registered [TtLeanClr] host type.");
            if (!sRuntime.IsValidPointer || sHostHandleOffset < 0)
                throw new InvalidOperationException("LeanCLR host binding is not ready.");

            var vmObject = sRuntime.CreateObject(hostType.Value.AssemblyName, hostType.Value.ClassName);
            if (!vmObject.IsValidPointer)
                throw new InvalidOperationException($"LeanCLR failed to create {hostType.Value.ClassName} in {hostType.Value.AssemblyName}.");

            var hostHandle = GCHandle.Alloc(host, GCHandleType.Weak);
            if (!vmObject.SetHostHandle(GCHandle.ToIntPtr(hostHandle).ToPointer()))
            {
                hostHandle.Free();
                CoreSDK.IUnknown_Release(vmObject.NativePointer.ToPointer());
                throw new InvalidOperationException($"{hostType.Value.ClassName} does not derive from {HostObjectClassName}.");
            }
            sBindings.Add(host, new FBinding(vmObject, hostHandle));
            return vmObject;
        }

        public static void* GetVmObjectPointer(object host)
        {
            return host == null ? null : GetOrCreate(host).GetTarget();
        }

        /// <summary>
        /// 拷贝 VM System.String 为宿主 string；不经 P/Invoke，直接按布局读 UTF-16 字符。
        /// </summary>
        public static string ReadString(void* vmString)
        {
            if (vmString == null)
                return null;
            var p = (byte*)vmString;
            return new string((char*)(p + sStringCharsOffset), 0, *(int*)(p + sStringLengthOffset));
        }

        /// <summary>
        /// 由宿主 string 创建 VM System.String；返回未入根的裸指针，只能立即写入 intrinsic 返回缓冲交给 VM。
        /// </summary>
        public static void* NewVmString(string value)
        {
            if (value == null)
                return null;
            if (!sRuntime.IsValidPointer)
                throw new InvalidOperationException("LeanCLR runtime is not attached.");
            void* result;
            fixed (char* chars = value)
                result = sRuntime.NewString(chars, value.Length);
            if (result == null)
                throw new InvalidOperationException("LeanCLR failed to create System.String.");
            return result;
        }

        public static void FlushReleased()
        {
            while (sReleased.TryDequeue(out var released))
            {
                if (released.VmObject.IsValidPointer)
                {
                    released.VmObject.SetHostHandle(null);
                    CoreSDK.IUnknown_Release(released.VmObject.NativePointer.ToPointer());
                }
                if (released.HostHandle.IsAllocated)
                    released.HostHandle.Free();
            }
        }
    }
}
