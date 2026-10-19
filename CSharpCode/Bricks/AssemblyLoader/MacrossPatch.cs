using System;

namespace EngineNS.Macross
{
    /// <summary>
    /// Macross 入口桩调用的补丁门面。Active 为 false 时入口桩只读一次静态字段。
    /// 返回 false 表示走原逻辑；补丁执行异常时该方法熔断并抛出 TtMacrossPatchException。
    /// 参数缓冲与返回缓冲必须在调用帧内 stackalloc，故每个元数各展开一个重载。
    /// </summary>
    public static unsafe class TtMacrossPatch
    {
        public static bool Active { get; private set; }
        public static TtMacrossPatchRuntime Runtime { get; private set; }

        internal static void Activate(TtMacrossPatchRuntime runtime)
        {
            Runtime = runtime;
            Active = runtime != null && runtime.Count > 0;
        }

        internal static void Deactivate()
        {
            Active = false;
            var runtime = Runtime;
            Runtime = null;
            runtime?.Dispose();
        }

        #region 无返回值
        public static bool TryInvoke(ulong methodId, object self)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1>(ulong methodId, object self, T1 a1)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2>(ulong methodId, object self, T1 a1, T2 a2)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3>(ulong methodId, object self, T1 a1, T2 a2, T3 a3)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3, T4>(ulong methodId, object self, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3, T4, T5>(ulong methodId, object self, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3, T4, T5, T6>(ulong methodId, object self, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7>(ulong methodId, object self, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Push(a7);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(ulong methodId, object self, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Push(a7);
                writer.Push(a8);
                writer.Complete();
                byte* ret = stackalloc byte[entry.RetBytes];
                entry.Invoke(self, args, ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }
        #endregion

        #region 有返回值
        public static bool TryInvoke<TRet>(ulong methodId, object self, out TRet result)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1>(ulong methodId, object self, out TRet result, T1 a1)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2>(ulong methodId, object self, out TRet result, T1 a1, T2 a2)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3, T4>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3, T4, T5>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3, T4, T5, T6>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3, T4, T5, T6, T7>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Push(a7);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }

        public static bool TryInvoke<TRet, T1, T2, T3, T4, T5, T6, T7, T8>(ulong methodId, object self, out TRet result, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
        {
            result = default;
            var entry = Runtime?.TryEnter(methodId);
            if (entry == null)
                return false;
            try
            {
                byte* args = stackalloc byte[entry.ArgBytes];
                var writer = new FMacrossPatchArgs(args, entry.ArgBytes);
                writer.Push(a1);
                writer.Push(a2);
                writer.Push(a3);
                writer.Push(a4);
                writer.Push(a5);
                writer.Push(a6);
                writer.Push(a7);
                writer.Push(a8);
                writer.Complete();
                byte* ret = stackalloc byte[Math.Max(entry.RetBytes, TtMacrossPatchMarshal.SlotBytes<TRet>())];
                entry.Invoke(self, args, ret);
                result = TtMacrossPatchMarshal.Read<TRet>(ret);
                return true;
            }
            catch (Exception ex)
            {
                throw entry.Fail(ex);
            }
        }
        #endregion
    }
}
