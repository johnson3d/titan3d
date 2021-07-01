using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Network.RPC
{
    //因为C#弱鸡的泛型能力不具备偏特化甚至特化能力，这里的Awaiter区分为三种
    //1.返回值为unmanaged类型，直接走Read<T>(out T) where T : unmanaged
    //2.返回值为IO.ISerializer接口，走Read(out IO.ISerializer)
    //3.字符串类型，不能走Read<T>，所以也需要一个特殊流程走Read(out string)
    public class URpcAwaiter
    {
        static TaskCompletionSource<object> source = new TaskCompletionSource<object>();
        public static async Task<T> AwaitReturn<T>(UReturnAwaiter waiter) where T : unmanaged
        {
            T rt = await source.Task.RPCWaitReturn<T>(waiter);
            return rt;
        }
        public static async Task<T> AwaitReturn_ISerializer<T>(UReturnAwaiter waiter) where T : IO.ISerializer
        {
            T rt = await source.Task.RPCWaitReturn_ISerializer<T>(waiter);
            return rt;
        }
        public static async Task<string> AwaitReturn_String(UReturnAwaiter waiter)
        {
            var rt = await source.Task.RPCWaitReturn_String(waiter);
            return rt;
        }
    }
    public class CustomTaskAwaiter<T> : INotifyCompletion where T : unmanaged
    {
        Task task;
        public UReturnAwaiter Waiter;
        public CustomTaskAwaiter(Task task, UReturnAwaiter waiter)
        {
            Waiter = waiter;
            this.task = task;
        }
        public CustomTaskAwaiter<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.RetCallBack = (ref IO.AuxReader<UMemReader> pkg, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    //throw Timeout exception : UReturnAwaiter & coneinuation;
                }
                else
                {
                    pkg.Read(out Result);
                }
                try
                {
                    continuation();
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
                return task.IsCompleted;
            }
        }
        public T Result = default(T);
        public T GetResult()
        {
            return Result;
        }
    }
    public class CustomTaskAwaiter_ISerializer<T> : INotifyCompletion where T : IO.ISerializer
    {
        Task task;
        public UReturnAwaiter Waiter;
        public CustomTaskAwaiter_ISerializer(Task task, UReturnAwaiter waiter)
        {
            Waiter = waiter;
            this.task = task;
        }
        public CustomTaskAwaiter_ISerializer<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.RetCallBack = (ref IO.AuxReader<UMemReader> pkg, bool isTimeOut) =>
            {
                EngineNS.IO.ISerializer tmp;
                if (isTimeOut)
                {
                    tmp = null;//应该给一个错误的对象用来判断超时间
                }
                else
                {
                    pkg.Read(out tmp, null);
                }
                try
                {
                    Result = (T)tmp;
                    continuation();
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
                return task.IsCompleted;
            }
        }
        public T Result = default(T);
        public T GetResult()
        {
            return Result;
        }
    }
    public class CustomTaskAwaiter_String : INotifyCompletion
    {
        Task task;
        public UReturnAwaiter Waiter;
        public CustomTaskAwaiter_String(Task task, UReturnAwaiter waiter)
        {
            Waiter = waiter;
            this.task = task;
        }
        public CustomTaskAwaiter_String GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            Waiter.RetCallBack = (ref IO.AuxReader<UMemReader> pkg, bool isTimeOut) =>
            {
                if (isTimeOut)
                {
                    Result = "@RPC_TimeOut@";
                }
                else
                {
                    pkg.Read(out Result);
                }
                try
                {
                    continuation();
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
                return task.IsCompleted;
            }
        }
        public string Result = null;
        public string GetResult()
        {
            return Result;
        }
    }
    public static class TaskExtension
    {
        public static CustomTaskAwaiter<T> RPCWaitReturn<T>(this Task task, UReturnAwaiter waiter) where T : unmanaged
        {
            return new CustomTaskAwaiter<T>(task, waiter);
        }
        public static CustomTaskAwaiter_ISerializer<T> RPCWaitReturn_ISerializer<T>(this Task task, UReturnAwaiter waiter) where T : IO.ISerializer
        {
            return new CustomTaskAwaiter_ISerializer<T>(task, waiter);
        }
        public static CustomTaskAwaiter_String RPCWaitReturn_String(this Task task, UReturnAwaiter waiter)
        {
            return new CustomTaskAwaiter_String(task, waiter);
        }
    }
}
