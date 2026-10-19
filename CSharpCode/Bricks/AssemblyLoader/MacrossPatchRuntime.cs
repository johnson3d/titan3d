using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EngineNS.LeanCLR;

namespace EngineNS.Macross
{
    /// <summary>
    /// Macross 可补丁方法的稳定 ID：签名 "类全名::方法名(参数类型,...)" 的 FNV-1a 64，
    /// 每个 UTF-16 code unit 依次折入低字节、高字节。入口桩生成器与补丁加载共用。
    /// </summary>
    public static class TtMacrossPatchId
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Compute(string signature)
        {
            var hash = OffsetBasis;
            foreach (var c in signature)
            {
                hash = (hash ^ (byte)c) * Prime;
                hash = (hash ^ (byte)(c >> 8)) * Prime;
            }
            return hash;
        }
    }

    public sealed class TtMacrossPatchException : Exception
    {
        public TtMacrossPatchException(ulong methodId, string signature, string vmError, Exception inner = null)
            : base($"Macross patch 0x{methodId:X16} {signature} failed: {vmError}", inner)
        {
            MethodId = methodId;
            Signature = signature;
            VmError = vmError;
        }
        public ulong MethodId { get; }
        public string Signature { get; }
        public string VmError { get; }
    }

    /// <summary>
    /// 宿主值与 VM 栈槽（8 字节对齐）之间的编组：string 按值拷贝，类经 HostBinding 映射为 VM 对象，
    /// bool/小整数按 int32 扩展，其余值类型按字节拷贝。
    /// </summary>
    public static unsafe class TtMacrossPatchMarshal
    {
        public const int StackObjectByteSize = 8;

        public static int SlotBytes<T>()
        {
            if (!typeof(T).IsValueType)
                return StackObjectByteSize;
            return (Unsafe.SizeOf<T>() + StackObjectByteSize - 1) / StackObjectByteSize * StackObjectByteSize;
        }

        public static void Write<T>(byte* slot, T value)
        {
            new Span<byte>(slot, SlotBytes<T>()).Clear();
            if (typeof(T) == typeof(string))
                *(void**)slot = TtLeanClrHostBinding.NewVmString((string)(object)value);
            else if (!typeof(T).IsValueType)
                *(void**)slot = TtLeanClrHostBinding.GetVmObjectPointer(value);
            else if (typeof(T) == typeof(bool))
                *(int*)slot = Unsafe.As<T, bool>(ref value) ? 1 : 0;
            else if (typeof(T) == typeof(sbyte))
                *(int*)slot = Unsafe.As<T, sbyte>(ref value);
            else if (typeof(T) == typeof(short))
                *(int*)slot = Unsafe.As<T, short>(ref value);
            else if (typeof(T) == typeof(byte))
                *(int*)slot = Unsafe.As<T, byte>(ref value);
            else if (typeof(T) == typeof(ushort))
                *(int*)slot = Unsafe.As<T, ushort>(ref value);
            else if (typeof(T) == typeof(char))
                *(int*)slot = Unsafe.As<T, char>(ref value);
            else
                Unsafe.WriteUnaligned(slot, value);
        }

        public static T Read<T>(byte* slot)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)TtLeanClrHostBinding.ReadString(*(void**)slot);
            if (!typeof(T).IsValueType)
                return TtLeanClrHostBinding.ResolveHost<object>(*(void**)slot) is T host ? host : default;
            if (typeof(T) == typeof(bool))
            {
                var value = *slot != 0;
                return Unsafe.As<bool, T>(ref value);
            }
            return Unsafe.ReadUnaligned<T>(slot);
        }
    }

    /// <summary>
    /// 入口桩参数缓冲的顺序写入器；越界或未写满即为宿主与补丁签名不一致。
    /// </summary>
    public unsafe ref struct FMacrossPatchArgs
    {
        private readonly byte* mBuffer;
        private readonly int mSize;
        private int mOffset;

        public FMacrossPatchArgs(byte* buffer, int size)
        {
            mBuffer = buffer;
            mSize = size;
            mOffset = 0;
        }

        public void Push<T>(T value)
        {
            var slot = TtMacrossPatchMarshal.SlotBytes<T>();
            if (mOffset + slot > mSize)
                throw new InvalidOperationException("Macross patch arguments exceed the VM argument buffer.");
            TtMacrossPatchMarshal.Write(mBuffer + mOffset, value);
            mOffset += slot;
        }

        public void Complete()
        {
            if (mOffset != mSize)
                throw new InvalidOperationException($"Macross patch arguments occupy {mOffset} bytes, but the VM method requires {mSize} bytes.");
        }
    }

    /// <summary>
    /// 补丁方法表：MethodId -> VM 补丁实例方法；负责线程检查、调用与熔断。
    /// </summary>
    public sealed unsafe class TtMacrossPatchRuntime : IDisposable
    {
        public sealed class FEntry
        {
            internal FEntry(TtMacrossPatchRuntime owner, ulong methodId, string signature, TtLeanClrMethod method, int argBytes, int retBytes)
            {
                Owner = owner;
                MethodId = methodId;
                Signature = signature;
                Method = method;
                ArgBytes = argBytes;
                RetBytes = retBytes;
            }
            internal readonly TtMacrossPatchRuntime Owner;
            public readonly ulong MethodId;
            public readonly string Signature;
            internal readonly TtLeanClrMethod Method;
            public readonly int ArgBytes;
            public readonly int RetBytes;
            internal bool Broken;
            internal bool ThreadWarned;

            internal void Invoke(object self, byte* args, byte* ret) => Owner.Invoke(this, self, args, ret);
            internal Exception Fail(Exception ex) => Owner.Fail(this, ex);
        }

        private readonly TtLeanClrRuntime mRuntime;
        private readonly Func<bool> mIsVmThread;
        private readonly Func<bool> mEnsureOwnerThread;
        private readonly Dictionary<ulong, FEntry> mEntries = new();
        private int mThreadFallbackWarnings;

        public TtMacrossPatchRuntime(TtLeanClrRuntime runtime, Func<bool> isVmThread, Func<bool> ensureOwnerThread)
        {
            mRuntime = runtime;
            mIsVmThread = isVmThread;
            mEnsureOwnerThread = ensureOwnerThread;
        }

        public int Count => mEntries.Count;
        public int ThreadFallbackWarnings => mThreadFallbackWarnings;

        public bool Add(ulong methodId, string signature, TtLeanClrMethod method, out string error)
        {
            error = null;
            if (!method.IsValidPointer)
                error = "method is invalid";
            else if (method.IsStatic())
                error = "patch method must be an instance method";
            else if (mEntries.ContainsKey(methodId))
                error = "duplicated MethodId";
            else if (method.GetArgBufferByteSize() < TtMacrossPatchMarshal.StackObjectByteSize)
                error = "invalid argument buffer";
            if (error != null)
                return false;
            var argBytes = (int)method.GetArgBufferByteSize() - TtMacrossPatchMarshal.StackObjectByteSize;
            mEntries.Add(methodId, new FEntry(this, methodId, signature, method, argBytes, (int)method.GetRetBufferByteSize()));
            return true;
        }

        public bool IsBroken(ulong methodId)
        {
            return mEntries.TryGetValue(methodId, out var entry) && entry.Broken;
        }

        internal FEntry TryEnter(ulong methodId)
        {
            if (!mEntries.TryGetValue(methodId, out var entry) || entry.Broken)
                return null;
            if (!mIsVmThread())
            {
                if (!entry.ThreadWarned)
                {
                    entry.ThreadWarned = true;
                    System.Threading.Interlocked.Increment(ref mThreadFallbackWarnings);
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                        $"Macross patch {entry.Signature} skipped: called outside the LeanCLR VM thread, original code is used.");
                }
                return null;
            }
            if (!mEnsureOwnerThread())
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error,
                    $"Macross patch {entry.Signature} skipped: LeanCLR owner thread switch failed.");
                return null;
            }
            return entry;
        }

        private void Invoke(FEntry entry, object self, byte* args, byte* ret)
        {
            var vmSelf = TtLeanClrHostBinding.GetOrCreate(self);
            if (entry.RetBytes > 0)
                new Span<byte>(ret, entry.RetBytes).Clear();
            if (!mRuntime.InvokeInstanceWithBuffer(vmSelf, entry.Method, args, (uint)entry.ArgBytes, ret, (uint)entry.RetBytes))
                throw new TtMacrossPatchException(entry.MethodId, entry.Signature, mRuntime.GetLastError().Text);
        }

        private Exception Fail(FEntry entry, Exception ex)
        {
            entry.Broken = true;
            var patchException = ex as TtMacrossPatchException ?? new TtMacrossPatchException(entry.MethodId, entry.Signature, ex.Message, ex);
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, patchException.Message + " The method falls back to original code in this process.");
            return patchException;
        }

        public void Dispose()
        {
            foreach (var entry in mEntries.Values)
            {
                if (entry.Method.IsValidPointer)
                    CoreSDK.IUnknown_Release(entry.Method.NativePointer.ToPointer());
            }
            mEntries.Clear();
        }
    }
}
