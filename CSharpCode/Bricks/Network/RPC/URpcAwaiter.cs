using EngineNS.Thread.Async;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Network.RPC
{
    //因为C#弱鸡的泛型能力不具备偏特化甚至特化能力，这里的Awaiter区分为三种
    //1.返回值为unmanaged类型，直接走Read<T>(out T) where T : unmanaged
    //2.返回值为IO.ISerializer接口，走Read(out IO.ISerializer)
    //3.字符串类型，不能走Read<T>，所以也需要一个特殊流程走Read(out string)
    public class TtRpcAwaiterBase
    {
        
    }

    public class TtRpcAwaiter : TtRpcAwaiterBase
    {
        public static async TtTask<T> AwaitReturn<T>(TtReturnAwaiter<T> waiter) where T : unmanaged
        {
            var task = TaskExtension.RPCWaitReturn<T>(waiter);
            T rt = await task;
            return rt;
        }
        public static async TtTask<T> AwaitReturn<T>(TtReturnAwaiter<T> waiter, int noused = 0) where T : class, IO.ISerializer
        {
            T rt = await TaskExtension.RPCWaitReturn_ISerializer<T>(waiter);
            return rt;
        }
        public static async TtTask<T> AwaitReturn_ISerializer<T>(TtReturnAwaiter<T> waiter) where T : IO.ISerializer
        {
            T rt = await TaskExtension.RPCWaitReturn_ISerializer<T>(waiter);
            return rt;
        }
        public static async TtTask<string> AwaitReturn_String(TtReturnAwaiter<string> waiter)
        {
            var rt = await TaskExtension.RPCWaitReturn_String(waiter);
            return rt;
        }
    }
    public struct FRpcTaskAwaiter<T> : INotifyCompletion where T : unmanaged
    {
        //Task task;
        public TtReturnAwaiter<T> Waiter;
        public FRpcTaskAwaiter(TtReturnAwaiter<T> waiter)
        {
            Waiter = waiter;
            //this.task = task;
        }
        public FRpcTaskAwaiter<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.ContinuationAction = continuation;
            Waiter.RetCallBack = static (ref IO.AuxReader<EngineNS.IO.UMemReader> pkg, bool isTimeOut, TtReturnAwaiterBase awaiter) =>
            {
                var typedAwaiter = (TtReturnAwaiter<T>)awaiter;
                if (isTimeOut)
                {
                    //throw Timeout exception : UReturnAwaiter & coneinuation;
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{typedAwaiter.ContinuationAction.ToString()} timeout");
                }
                else
                {
                    pkg.Read(out ((TtReturnAwaiter<T>)awaiter).Result);
                }
                try
                {
                    if (typedAwaiter.ReturnContext != null)
                        typedAwaiter.ReturnContext.IsTimeout = isTimeOut;
                    typedAwaiter.ContinuationAction();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            };
            //RPCAwaiter.PushContinueAction<T>(continuation, this);
            // ContinueWith sets the scheduler to use for the continuation action
            //task.ContinueWith(x => continuation(), scheduler);
        }
        public bool IsCompleted
        {
            get
            {
                //return task.IsCompleted;
                return Waiter.IsCompleted;
            }
        }
        public T GetResult()
        {
            return Waiter.Result;
        }
    }
    public struct FRpcTaskAwaiter_ISerializer<T> : INotifyCompletion where T : IO.ISerializer
    {
        //Task task;
        public TtReturnAwaiter<T> Waiter;
        public FRpcTaskAwaiter_ISerializer(TtReturnAwaiter<T> waiter)
        {
            Waiter = waiter;
            //this.task = task;
        }
        public FRpcTaskAwaiter_ISerializer<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.ContinuationAction = continuation;
            Waiter.RetCallBack = static (ref IO.AuxReader<EngineNS.IO.UMemReader> pkg, bool isTimeOut, TtReturnAwaiterBase awaiter) =>
            {
                var typedAwaiter = (TtReturnAwaiter<T>)awaiter;
                EngineNS.IO.ISerializer tmp;
                if (isTimeOut)
                {
                    tmp = null;//应该给一个错误的对象用来判断超时间
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{typedAwaiter.ContinuationAction.ToString()} timeout");
                }
                else
                {
                    pkg.Read(out tmp);
                }
                try
                {
                    ((TtReturnAwaiter<T>)awaiter).Result = (T)tmp;
                    if (typedAwaiter.ReturnContext != null)
                        typedAwaiter.ReturnContext.IsTimeout = isTimeOut;
                    typedAwaiter.ContinuationAction();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            };
            //RPCAwaiter.PushContinueAction<T>(continuation, this);
            // ContinueWith sets the scheduler to use for the continuation action
            //task.ContinueWith(x => continuation(), scheduler);
        }
        public bool IsCompleted
        {
            get
            {
                //return task.IsCompleted;
                return Waiter.IsCompleted;
            }
        }
        public T GetResult()
        {
            return Waiter.Result;
        }
    }
    public struct FRpcTaskAwaiter_String : INotifyCompletion
    {
        //Task task;
        public TtReturnAwaiter<string> Waiter;
        public FRpcTaskAwaiter_String(TtReturnAwaiter<string> waiter)
        {
            Waiter = waiter;
            //this.task = task;
        }
        public FRpcTaskAwaiter_String GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.ContinuationAction = continuation;
            Waiter.RetCallBack = static (ref IO.AuxReader<EngineNS.IO.UMemReader> pkg, bool isTimeOut, TtReturnAwaiterBase awaiter) =>
            {
                var typedAwaiter = (TtReturnAwaiter<string>)awaiter;
                if (isTimeOut)
                {
                    ((TtReturnAwaiter<string>)awaiter).Result = "@RPC_TimeOut@";
                    Profiler.Log.WriteLine<Profiler.TtNetCategory>(Profiler.ELogTag.Warning, $"{typedAwaiter.ContinuationAction.ToString()} timeout");
                }
                else
                {
                    pkg.Read(out ((TtReturnAwaiter<string>)awaiter).Result);
                }
                try
                {
                    typedAwaiter.ContinuationAction();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            };
            //RPCAwaiter.PushContinueAction<T>(continuation, this);
            // ContinueWith sets the scheduler to use for the continuation action
            //task.ContinueWith(x => continuation(), scheduler);
        }
        public bool IsCompleted
        {
            get
            {
                //return task.IsCompleted;
                return Waiter.IsCompleted;
            }
        }
        public string GetResult()
        {
            return Waiter.Result;
        }
    }
    public static class TaskExtension
    {
        public static FRpcTaskAwaiter<T> RPCWaitReturn<T>(TtReturnAwaiter<T> waiter) where T : unmanaged
        {
            return new FRpcTaskAwaiter<T>(waiter);
        }
        public static FRpcTaskAwaiter_ISerializer<T> RPCWaitReturn_ISerializer<T>(TtReturnAwaiter<T> waiter) where T : IO.ISerializer
        {
            return new FRpcTaskAwaiter_ISerializer<T>(waiter);
        }
        public static FRpcTaskAwaiter_String RPCWaitReturn_String(TtReturnAwaiter<string> waiter)
        {
            return new FRpcTaskAwaiter_String(waiter);
        }
    }
}
