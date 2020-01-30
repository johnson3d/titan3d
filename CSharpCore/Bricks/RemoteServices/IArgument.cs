using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RemoteServices
{
    public interface IArgument
    {
        int GetPkgSize();
    }
    public interface IReturnValue
    {
        int GetPkgSize();
    }

    public struct NullReturnValue : IReturnValue
    {
        public int GetPkgSize()
        {
            return 0;
        }
        public RPCError ResultState
        {
            get { return RPCError.Unknown; }
            set { }
        }
    }

    public class AuxProcessor<T> : RPCProcessor
            where T : struct, IArgument
    {
        public void DoCall(ref T arg, 
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            try
            {
                WritePkgHeader<T>(ref pkg, ref arg, 0, null, route, conn, router);

                OnWriteArugment(ref arg, ref pkg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        public void DoHashCall(ref T arg,
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            try
            {
                WritePkgHeader_Hash<T>(ref pkg, ref arg, 0, null, route, conn, router);

                OnWriteArugment(ref arg, ref pkg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        #region IO
        protected virtual void OnWriteArugment(ref T obj, ref NetCore.PkgWriter pkg)
        {
            IO.Serializer.SerializerHelper.WriteObject<T>(ref obj, pkg);
        }
        protected virtual void OnReadArugment(ref T obj, ref NetCore.PkgReader pkg)
        {
            object temp = new T();
            IO.Serializer.SerializerHelper.ReadObject(temp, pkg);
            obj = (T)temp;
        }
        protected virtual object OnCallMethod(object host, byte userFlags, ref T arg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var args = new object[] { userFlags, arg, serialId, connect, routeInfo };
            return Method.Invoke(host, args);
        }
        public sealed override object Execute(object host, ref NetCore.PkgReader pkg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            unsafe
            {
                T arg = new T();
                OnReadArugment(ref arg, ref pkg);

                return OnCallMethod(host, pkg.GetUserFlags(), ref arg, serialId, connect, ref routeInfo);
            }
        }
        #endregion
    }
    public class AuxUnmanagedProcessor<T> : RPCProcessor
            where T : unmanaged, IArgument
    {
        public void DoCall(ref T arg,
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            try
            {
                WritePkgHeader<T>(ref pkg, ref arg, 0, null, route, conn, router);

                OnWriteArgument(ref pkg, ref arg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        public void DoHashCall(ref T arg,
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            try
            {
                var waiter = WritePkgHeader_Hash<T>(ref pkg, ref arg, 0, null, route, conn, router);

                OnWriteArgument(ref pkg, ref arg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        #region IO
        protected virtual void OnWriteArgument(ref NetCore.PkgWriter pkg, ref T data)
        {
            unsafe
            {
                fixed (T* p = &data)
                {
                    pkg.WritePtr(p, sizeof(T));
                }
            }
        }
        protected virtual void OnReadArgument(ref NetCore.PkgReader pkg, ref T data)
        {
            unsafe
            {
                fixed (T* p = &data)
                {
                    pkg.ReadPtr(p, sizeof(T));
                }
            }
        }
        protected virtual object OnCallMethod(object host, byte userFlags, ref T arg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var args = new object[] { userFlags, arg, serialId, connect, routeInfo };
            return Method.Invoke(host, args);
        }
        public sealed override object Execute(object host, ref NetCore.PkgReader pkg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            unsafe
            {
                T arg = new T();
                OnReadArgument(ref pkg, ref arg);

                return OnCallMethod(host, pkg.GetUserFlags(), ref arg, serialId, connect, ref routeInfo);
            }
        }
        #endregion
    }
    public class AuxReturnProcessor<R, T> : RPCProcessor
            where R : struct, IReturnValue
            where T : struct, IArgument
    {
        public async System.Threading.Tasks.Task<R> DoAwaitCall(T arg, long timeOut, 
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            RPCExecuter.RPCWait waiter = null;
            try
            {
                waiter = WritePkgHeader<T>(ref pkg, ref arg, timeOut, RPCAwaiter.NullCB, route, conn, router);

                OnWriteArugment(ref arg, ref pkg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }

            waiter.Processor = this;
            var result = await RPCAwaiter.RPCWaitReturn<R>(waiter);
            return result;
        }
        public async System.Threading.Tasks.Task<R> DoAwaitHashCall(T arg, 
                    long timeOut, 
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            RPCExecuter.RPCWait waiter = null;
            try
            {
                waiter = WritePkgHeader_Hash<T>(ref pkg, ref arg, timeOut, RPCAwaiter.NullCB, route, conn, router);

                OnWriteArugment(ref arg, ref pkg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }

            waiter.Processor = this;
            var result = await RPCAwaiter.RPCWaitReturn<R>(waiter);
            return result;
        }
        public void DoReturn(ref R arg, UInt16 serialId, NetCore.NetConnection connect)
        {
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            try
            {
                byte route = (byte)NetCore.ERouteTarget.Self | (byte)NetCore.ERouteTarget.ReturnFlag;
                pkg.Write((byte)route);
                pkg.Write(serialId);
                OnWriteReturn(ref arg, ref pkg);
                pkg.SendBuffer(connect);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        public void DoReturn(ref R arg, UInt16 serialId, NetCore.NetConnection connect, NetCore.RPCRouter.RouteData router)
        {
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            try
            {
                byte route = (byte)NetCore.ERouteTarget.Client | (byte)NetCore.ERouteTarget.ReturnFlag;
                pkg.Write((byte)route);
                router.Save(ref pkg, NetCore.ERouteTarget.Self);
                pkg.Write(serialId);
                OnWriteReturn(ref arg, ref pkg);
                pkg.SendBuffer(connect);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        #region IO
        protected virtual void OnWriteArugment(ref T obj, ref NetCore.PkgWriter pkg)
        {
            IO.Serializer.SerializerHelper.WriteObject<T>(ref obj, pkg);
        }
        protected virtual void OnReadArugment(ref T obj, ref NetCore.PkgReader pkg)
        {
            object temp = new T();
            IO.Serializer.SerializerHelper.ReadObject(temp, pkg);
            obj = (T)temp;
        }
        protected virtual void OnWriteReturn(ref R obj, ref NetCore.PkgWriter pkg)
        {
            IO.Serializer.SerializerHelper.WriteObject<R>(ref obj, pkg);
        }
        protected virtual void OnReadReturn(ref R obj, ref NetCore.PkgReader pkg)
        {
            object temp = new R();
            IO.Serializer.SerializerHelper.ReadObject(temp, pkg);
            obj = (R)temp;
        }
        public sealed override object ReadReturn(ref NetCore.PkgReader pkg)
        {
            R temp = new R();
            OnReadReturn(ref temp, ref pkg);
            return temp;
        }
        protected virtual object OnCallMethod(object host, byte userFlags, ref T arg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var args = new object[] { userFlags, arg, serialId, connect, routeInfo };
            return Method.Invoke(host, args);
        }
        public sealed override object Execute(object host, ref NetCore.PkgReader pkg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            unsafe
            {
                T arg = new T();
                OnReadArugment(ref arg, ref pkg);

                return OnCallMethod(host, pkg.GetUserFlags(), ref arg, serialId, connect, ref routeInfo);
            }
        }
        #endregion
    }
    public class AuxUnmangedReturnProcessor<R, T> : RPCProcessor
            where R : unmanaged, IReturnValue
            where T : unmanaged, IArgument
    {
        public async System.Threading.Tasks.Task<R> DoAwaitCall(T arg, 
                    long timeOut,
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            RPCExecuter.RPCWait waiter = null;
            try
            {
                waiter = WritePkgHeader<T>(ref pkg, ref arg, timeOut, RPCAwaiter.NullCB, route, conn, router);

                OnWriteArgument(ref pkg, ref arg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }

            waiter.Processor = this;
            var result = await RPCAwaiter.RPCWaitReturn_Unmanaged<R>(waiter);
            return result;
        }
        public async System.Threading.Tasks.Task<R> DoAwaitHashCall(T arg, 
                    long timeOut,
                    NetCore.ERouteTarget route = NetCore.ERouteTarget.Self,
                    NetCore.NetConnection conn = null,
                    NetCore.RPCRouter router = null,
                    byte userFlags = 0)
        {
            if (conn == null)
                conn = RPCProcessor.DefaultConnection;
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            pkg.SetUserFlags(userFlags);
            RPCExecuter.RPCWait waiter = null;
            try
            {
                waiter = WritePkgHeader_Hash<T>(ref pkg, ref arg, timeOut, RPCAwaiter.NullCB, route, conn, router);

                OnWriteArgument(ref pkg, ref arg);

                if (conn != null)
                    pkg.SendBuffer(conn);
            }
            finally
            {
                pkg.Dispose();
            }

            waiter.Processor = this;
            var result = await RPCAwaiter.RPCWaitReturn_Unmanaged<R>(waiter);
            return result;
        }
        public void DoReturn(ref R arg, UInt16 serialId, NetCore.NetConnection connect)
        {
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            try
            {
                byte route = (byte)NetCore.ERouteTarget.Self | (byte)NetCore.ERouteTarget.ReturnFlag;
                pkg.Write((byte)route);
                pkg.Write(serialId);
                OnWriteReturn(ref pkg, ref arg);
                pkg.SendBuffer(connect);
            }
            finally
            {
                pkg.Dispose();
            }
        }
        public void DoReturn(ref R arg, UInt16 serialId, NetCore.NetConnection connect, NetCore.RPCRouter.RouteData router)
        {
            var pkg = new NetCore.PkgWriter(arg.GetPkgSize());
            try
            {
                byte route = (byte)NetCore.ERouteTarget.Client | (byte)NetCore.ERouteTarget.ReturnFlag;
                pkg.Write((byte)route);
                router.Save(ref pkg, NetCore.ERouteTarget.Self);
                pkg.Write(serialId);
                OnWriteReturn(ref pkg, ref arg);
                pkg.SendBuffer(connect);
            }
            finally
            {
                pkg.Dispose();
            }
        }

        #region IO
        protected virtual void OnWriteArgument(ref NetCore.PkgWriter pkg, ref T data)
        {
            unsafe
            {
                fixed (T* p = &data)
                {
                    pkg.WritePtr(p, sizeof(T));
                }
            }
        }
        protected virtual void OnReadArgument(ref NetCore.PkgReader pkg, ref T data)
        {
            unsafe
            {
                fixed (T* p = &data)
                {
                    pkg.ReadPtr(p, sizeof(T));
                }
            }
        }
        protected virtual void OnWriteReturn(ref NetCore.PkgWriter pkg, ref R data)
        {
            unsafe
            {
                fixed (R* p = &data)
                {
                    pkg.WritePtr(p, sizeof(R));
                }
            }
        }
        protected virtual void OnReadReturn(ref NetCore.PkgReader pkg, ref R data)
        {
            unsafe
            {
                fixed (R* p = &data)
                {
                    pkg.ReadPtr(p, sizeof(R));
                }
            }
        }
        public sealed override unsafe void UnsafeReadReturn(ref NetCore.PkgReader pkg, void* arg)
        {
            R* pArg = (R*)arg;
            OnReadReturn(ref pkg, ref *pArg);
        }
        protected virtual object OnCallMethod(object host, byte userFlags, ref T arg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            var args = new object[] { userFlags, arg, serialId, connect, routeInfo };
            return Method.Invoke(host, args);
        }
        public sealed override object Execute(object host, ref NetCore.PkgReader pkg, UInt16 serialId, NetCore.NetConnection connect, ref NetCore.RPCRouter.RouteData routeInfo)
        {
            unsafe
            {
                T arg = new T();

                OnReadArgument(ref pkg, ref arg);

                return OnCallMethod(host, pkg.GetUserFlags(), ref arg, serialId, connect, ref routeInfo);
            }
        }
        
        #endregion
    }

    public class AuxProcessorEx<T> : AuxProcessor<T>
        where T : struct, IArgument
    {
        public static AuxProcessorEx<T> Instance;
        public AuxProcessorEx()
        {
            ArgumentType = typeof(T);
            if (Instance == null)
                Instance = this;
        }
    }
    public class AuxUnmanagedProcessorEx<T> : AuxUnmanagedProcessor<T>
        where T : unmanaged, IArgument
    {
        public static AuxUnmanagedProcessorEx<T> Instance;
        public AuxUnmanagedProcessorEx()
        {
            ArgumentType = typeof(T);
            if(Instance==null)
                Instance = this;
        }
    }
    public class AuxReturnProcessorEx<R, T> : AuxReturnProcessor<R, T>
        where R : struct, IReturnValue
        where T : struct, IArgument
    {
        public static AuxReturnProcessorEx<R, T> Instance;
        public AuxReturnProcessorEx()
        {
            ArgumentType = typeof(T);
            ReturnType = typeof(R);
            if (Instance == null)
                Instance = this;
        }
    }
    public class AuxUnmangedReturnProcessorEx<R, T> : AuxUnmangedReturnProcessor<R, T>
        where R : unmanaged, IReturnValue
        where T : unmanaged, IArgument
    {
        public static AuxUnmangedReturnProcessorEx<R, T> Instance;
        public AuxUnmangedReturnProcessorEx()
        {
            ArgumentType = typeof(T);
            ReturnType = typeof(R);
            if (Instance == null)
                Instance = this;
        }
    }
}
