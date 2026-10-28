using System;
using System.Collections.Generic;

namespace EngineNS.LeanCLR
{
    /// <summary>
    /// LeanCLR VM 与 intrinsic 分发模块，跟随 TtEngine 模块生命周期。
    /// 进程内只有一个 VM，初始化后保活到进程退出（LeanCLR 不支持 shutdown 后重建）。
    /// Register 必须在目标方法首次被调用前、并在 VM 初始化线程上调用；
    /// Unregister 之后该签名的调用会失败（native 劫持不可撤销），再次 Register 可替换 handler。
    /// </summary>
    public unsafe class TtLeanClrIntrinsicDispatcher : TtModule<TtEngine>
    {
        public delegate bool FIntrinsicHandler(FLeanClrIntrinsicCall* call);

        private readonly Dictionary<int, FIntrinsicHandler> mHandlers = new();
        private readonly List<FIntrinsicHandler> mHandlersByToken = new();
        private readonly Dictionary<System.Reflection.Assembly, ITtLeanClrRegistry> mRegistries = new();
        private TtLeanClrRuntime mRuntime;
        private int mOwnerManagedThreadId;
        private readonly TtLeanClrRuntime.FDelegate_FLeanClrIntrinsicDispatcher mNativeInvoke;

        public TtLeanClrIntrinsicDispatcher()
        {
            mNativeInvoke = Invoke;
        }

        public TtLeanClrRuntime Runtime => mRuntime;
        public bool IsRuntimeReady => mRuntime.IsValidPointer;
        public EngineNS.Macross.TtMacrossPatchLoader MacrossPatch { get; private set; }

        public override int GetOrder()
        {
            return 0;
        }

        public override async Thread.Async.TtTask<bool> Initialize(TtEngine host)
        {
            // content\LeanCLR 递归下的 BCL、EngineStub 与各插件桩自动注册；LeanClrAssemblies 只作补充
            var extraFiles = new List<string>();
            if (host.Config.LeanClrAssemblies != null)
            {
                foreach (var rn in host.Config.LeanClrAssemblies)
                {
                    if (rn != null)
                        extraFiles.Add(rn.Address);
                }
            }
            var scanDirectory = host.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game) + "LeanCLR/";
            var assemblies = TtLeanClrAssemblyScanner.Collect(scanDirectory, extraFiles);
            foreach (var warning in assemblies.Warnings)
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, warning);
            if (assemblies.Errors.Count > 0)
            {
                foreach (var error in assemblies.Errors)
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, error);
                return false;
            }
            if (!assemblies.ShouldStartRuntime)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"LeanCLR runtime is disabled: no {TtLeanClrHostBinding.EngineStubAssemblyName} in {scanDirectory} and no LeanClrAssemblies configured.");
                return true;
            }

            var runtime = TtLeanClrRuntime.CreateInstance();
            if (!runtime.IsValidPointer)
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, "LeanCLR runtime creation failed.");
                return false;
            }

            try
            {
                foreach (var assembly in assemblies.Assemblies)
                {
                    if (!RegisterAssembly(runtime, assembly.Key, assembly.Value))
                    {
                        CoreSDK.IUnknown_Release(runtime.NativePointer.ToPointer());
                        return false;
                    }
                }
                // 补丁程序集只能在 VM Initialize 前注册；校验失败整包拒绝，引擎走原逻辑
                MacrossPatch = new EngineNS.Macross.TtMacrossPatchLoader(host.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "macrosspatch/");
                MacrossPatch.StageA(assemblies.Assemblies, (name, path) => RegisterAssembly(runtime, name, path));
                mRuntime = runtime;
                bool previousNoReportLeaks = Profiler.TtNativeMemory.IsNoReportLeaks;
                bool initialized;
                try
                {
                    // 仅覆盖同步 VM 初始化；ThreadStatic 作用域中不能 await。
                    Profiler.TtNativeMemory.IsNoReportLeaks = true;
                    initialized = mRuntime.Initialize();
                }
                finally
                {
                    Profiler.TtNativeMemory.IsNoReportLeaks = previousNoReportLeaks;
                }
                if (!initialized)
                {
                    mRuntime = default;
                    CoreSDK.IUnknown_Release(runtime.NativePointer.ToPointer());
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, "LeanCLR runtime initialization failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                if (!runtime.IsInitialized())
                {
                    mRuntime = default;
                    CoreSDK.IUnknown_Release(runtime.NativePointer.ToPointer());
                }
                return false;
            }

            AttachRuntime(runtime);
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, $"LeanCLR runtime initialized with {assemblies.Assemblies.Count} assemblies from {scanDirectory}.");
            return true;
        }

        /// <summary>
        /// 接管已初始化的 VM：安装 native dispatcher 并注册当前已加载的带 TtLeanClrRegistry 的程序集。
        /// </summary>
        public bool AttachRuntime(TtLeanClrRuntime runtime)
        {
            if (!runtime.IsValidPointer || !runtime.IsInitialized())
                return false;
            mRuntime = runtime;
            mOwnerManagedThreadId = 0;
            if (!EnsureOwnerThread())
                return false;
            TtLeanClrHostBinding.Attach(runtime);
            TtLeanClrRuntime.SetIntrinsicDispatcher(mNativeInvoke);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                RegisterAssembly(assembly);
            return true;
        }

        /// <summary>
        /// 运行期 VM 属主为 Logic 线程；当前线程不是属主时切换 native 属主。
        /// 调用方保证与上一属主线程上的 VM 调用已串行。
        /// </summary>
        public bool EnsureOwnerThread()
        {
            if (!mRuntime.IsValidPointer)
                return false;
            var current = Environment.CurrentManagedThreadId;
            if (mOwnerManagedThreadId == current)
                return true;
            if (!mRuntime.AcquireOwnerThread())
                return false;
            mOwnerManagedThreadId = current;
            return true;
        }

        /// <summary>
        /// 注册插件程序集生成的 intrinsic 代理与宿主类型；必须早于脚本首次调用对应方法。
        /// VM 未就绪或程序集没有 TtLeanClrRegistry 时返回 false。
        /// </summary>
        public bool RegisterAssembly(System.Reflection.Assembly assembly)
        {
            if (assembly == null || !mRuntime.IsValidPointer)
                return false;
            if (mRegistries.ContainsKey(assembly))
                return true;
            var attribute = (TtLeanClrRegistryAttribute)Attribute.GetCustomAttribute(assembly, typeof(TtLeanClrRegistryAttribute));
            if (attribute == null)
                return false;

            try
            {
                var registry = (ITtLeanClrRegistry)Activator.CreateInstance(attribute.RegistryType);
                var hostTypes = new List<KeyValuePair<Type, string>>();
                registry.CollectHostTypes(hostTypes);
                if (!TtLeanClrHostBinding.RegisterHostTypes(registry.StubAssemblyName, hostTypes))
                    return false;
                registry.Register(this);
                mRegistries.Add(assembly, registry);
                TtLeanClrHostBinding.FlushReleased();
                return true;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return false;
            }
        }

        public bool UnregisterAssembly(System.Reflection.Assembly assembly)
        {
            if (assembly == null || !mRegistries.Remove(assembly, out var registry))
                return false;
            registry.Unregister(this);
            var hostTypes = new List<KeyValuePair<Type, string>>();
            registry.CollectHostTypes(hostTypes);
            TtLeanClrHostBinding.UnregisterHostTypes(hostTypes);
            return true;
        }

        private static bool RegisterAssembly(TtLeanClrRuntime runtime, string assemblyName, string path)
        {
            using (var dll = CreateBlob(path))
            {
                var pdbPath = System.IO.Path.ChangeExtension(path, ".pdb");
                using (var pdb = System.IO.File.Exists(pdbPath) ? CreateBlob(pdbPath) : null)
                {
                    if (!runtime.RegisterAssembly(assemblyName, dll.mCoreObject, pdb == null ? default : pdb.mCoreObject))
                    {
                        Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"LeanCLR assembly registration failed: {assemblyName}({path})");
                        return false;
                    }
                }
            }
            return true;
        }

        private static Support.TtBlobObject CreateBlob(string path)
        {
            var bytes = System.IO.File.ReadAllBytes(path);
            var blob = new Support.TtBlobObject();
            if (bytes.Length > 0)
            {
                fixed (byte* p = &bytes[0])
                {
                    blob.PushData(p, (uint)bytes.Length);
                }
            }
            return blob;
        }

        public override void Cleanup(TtEngine host)
        {
            TtLeanClrRuntime.SetIntrinsicDispatcher(null);
            TtLeanClrHostBinding.Detach();
            // 已初始化的 VM 属于进程级单例；LeanCLR 不支持 shutdown，不能在模块清理时销毁。
            // 想了一下Cleanup已经是系统退出了，LeanCLR的Runtime也应该随之退出，所以这里还是释放掉吧，虽然目前LeanClr的释放不会完整，但是以后他应该要做到vm是可以释放，并重新加载的吧
            // 后续可以给mRuntime调用都做一下null判断，确保不会再调用到已经释放的Runtime
            CoreSDK.IUnknown_Release(mRuntime);
            mRuntime = default;
            mOwnerManagedThreadId = 0;
            mRegistries.Clear();
            mHandlers.Clear();
            for (var index = 0; index < mHandlersByToken.Count; ++index)
                mHandlersByToken[index] = null;
        }

        public void Register(VNameString signature, FIntrinsicHandler handler)
        {
            if (!signature.IsValidString() || handler == null)
                throw new ArgumentException("An intrinsic signature and handler are required.");
            if (!mRuntime.IsValidPointer)
                throw new InvalidOperationException("LeanCLR runtime is not initialized.");
            // native 侧重复注册同一签名会 assert，只在首次注册时劫持 VM 方法
            if (!mHandlers.ContainsKey(signature.Index))
            {
                if (!mRuntime.RegisterIntrinsicFunc(signature.Text))
                    throw new InvalidOperationException($"Failed to register LeanCLR intrinsic: {signature.Text}");
            }
            mHandlers[signature.Index] = handler;
        }

        public bool Unregister(VNameString signature)
        {
            if (!signature.IsValidString() || !mHandlers.TryGetValue(signature.Index, out var handler))
                return false;
            // native 劫持无法撤销，保留 key 以免再次注册时重复劫持
            mHandlers[signature.Index] = null;
            // 已缓存到 VM 方法上的 token 也要失效，否则卸载后的插件代理仍会被调用
            if (handler != null)
            {
                for (var index = 0; index < mHandlersByToken.Count; ++index)
                {
                    if (ReferenceEquals(mHandlersByToken[index], handler))
                        mHandlersByToken[index] = null;
                }
            }
            return true;
        }

        public void Invoke(FLeanClrIntrinsicCall* call)
        {
            try
            {
                if (call == null)
                    return;

                var token = (int)(nint)call->GetUserData();
                FIntrinsicHandler handler = null;
                if (token != 0 && token <= mHandlersByToken.Count)
                {
                    handler = mHandlersByToken[token - 1];
                    if (handler != null)
                    {
                        if (!handler(call))
                            call->SetFailed();
                        return;
                    }
                }

                var signature = call->GetSignature().Index;
                if (!mHandlers.TryGetValue(signature, out handler) || handler == null)
                {
                    call->SetFailed();
                    return;
                }

                mHandlersByToken.Add(handler);
                token = mHandlersByToken.Count;
                call->SetUserData((void*)(nint)token);
                if (!handler(call))
                    call->SetFailed();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                if (call != null)
                    call->SetFailed();
            }
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public LeanCLR.TtLeanClrIntrinsicDispatcher LeanClrIntrinsicDispatcher { get; } = new LeanCLR.TtLeanClrIntrinsicDispatcher();
    }
}
