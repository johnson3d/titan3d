using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace EngineNS.Bricks.RemoteServices
{
    public class RPCAwaiter
    {
        static TaskCompletionSource<object> source = new TaskCompletionSource<object>();
        private static Task AsyncTask()
        {
            return source.Task;
        }
        public static async Task<T> RPCWaitReturn<T>(RPCExecuter.RPCWait waiter) where T : struct, IReturnValue
        {
            T rt = await RPCAwaiter.AsyncTask().RPCWaitReturn<T>(waiter);
            return rt;
        }
        public static async Task<T> RPCWaitReturn_Unmanaged<T>(RPCExecuter.RPCWait waiter) where T : unmanaged, IReturnValue
        {
            T rt = await RPCAwaiter.AsyncTask().RPCWaitReturn_Unmanaged<T>(waiter);
            return rt;
        }
        public static void PushContinueAction<T>(Action act, CustomTaskAwaiter<T> awaiter) where T : struct, IReturnValue
        {
            RPCExecuter.RPCWait w = awaiter.Waiter;
            if(w==null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC awaiter's waiter is null:{act.Method.DeclaringType}.{act.Method.Name}");
                try
                {
                    act();
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                return;
            }
            w.RetCallBack = (ref NetCore.PkgReader pkg, bool isTimeOut) =>
            {
                if (isTimeOut == false)
                {
                    object ret = w.Processor.ReadReturn(ref pkg);
                    awaiter.Result = (T)ret;
                }
                else
                {
                    awaiter.Result = default(T);
                }
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            };
        }
        public static void PushContinueAction_Unmanaged<T>(Action act, CustomTaskAwaiter_Unmanaged<T> awaiter) where T : unmanaged, IReturnValue
        {
            RPCExecuter.RPCWait w = awaiter.Waiter;
            if (w == null)
            {
                System.Diagnostics.Debug.WriteLine($"RPC awaiter's waiter is null:{act.Method.DeclaringType}.{act.Method.Name}");
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                return;
            }
            w.RetCallBack = (ref NetCore.PkgReader pkg, bool isTimeOut) =>
            {
                if (isTimeOut == false)
                {
                    unsafe
                    {
                        fixed (T* p = &awaiter.Result)
                        {
                            w.Processor.UnsafeReadReturn(ref pkg, p);
                        }
                    }
                }
                else
                {
                    awaiter.Result = default(T);
                }
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            };
        }
        public static void NullCB(ref NetCore.PkgReader data, bool isTimeOut)
        {

        }
    }

    public class CustomTaskAwaiter<T> : INotifyCompletion where T : struct, IReturnValue
    {
        Task task;
        public RPCExecuter.RPCWait Waiter;
        public CustomTaskAwaiter(Task task, RPCExecuter.RPCWait waiter)
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
            RPCAwaiter.PushContinueAction<T>(continuation, this);
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
    

    public class CustomTaskAwaiter_Unmanaged<T> : INotifyCompletion where T : unmanaged, IReturnValue
    {
        Task task;
        public RPCExecuter.RPCWait Waiter;
        public CustomTaskAwaiter_Unmanaged(Task task, RPCExecuter.RPCWait waiter)
        {
            Waiter = waiter;
            this.task = task;
        }
        public CustomTaskAwaiter_Unmanaged<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            RPCAwaiter.PushContinueAction_Unmanaged<T>(continuation, this);
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
    public static class TaskExtension
    {
        public static CustomTaskAwaiter<T> RPCWaitReturn<T>(this Task task, RPCExecuter.RPCWait waiter) where T : struct, IReturnValue
        {
            return new CustomTaskAwaiter<T>(task, waiter);
        }
        public static CustomTaskAwaiter_Unmanaged<T> RPCWaitReturn_Unmanaged<T>(this Task task, RPCExecuter.RPCWait waiter) where T : unmanaged, IReturnValue
        {
            return new CustomTaskAwaiter_Unmanaged<T>(task, waiter);
        }
    }
}
