using EngineNS.Thread;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public interface IPooledObject
    {
        public bool IsAlloc { get; set; }
    }

    public interface IObjectPoolBase
    {
        bool ReleaseObject(IPooledObject obj);
        void Cleanup();
    }
    public class TtObjectPoolManager
    {
        static TtObjectPoolManager mInstance = new TtObjectPoolManager();
        public static TtObjectPoolManager Instance
        {
            get => mInstance;
        }
        public List<IObjectPoolBase> Pools { get; } = new List<IObjectPoolBase>();
        public void RegPoolManager(IObjectPoolBase manager)
        {
            lock (Pools)
            {
                Pools.Add(manager);
            }
        }
        public void Cleanup()
        {
            lock (Pools)
            {
                foreach(var i in Pools)
                {
                    i.Cleanup();
                }
            }
        }
    }
    public abstract class TtObjectPoolBase : IObjectPoolBase
    {
        public abstract bool ReleaseObject(IPooledObject obj);
        public abstract void Cleanup();

        public int GrowStep
        {
            get;
            set;
        } = 10;
        public abstract int PoolSize { get; }
        public int AliveNumber
        {
            get;
            protected set;
        } = 0;
        public int TotalQueryTimes = 0;
        public int TotalReleaseTimes = 0;
        public abstract Type ObjectType { get; }
        public abstract string ShowName { get; }
    }
    public class TtObjectPool<T> : TtObjectPoolBase where T : IPooledObject, new()
    {
        public TtObjectPool()
        {
            TtObjectPoolManager.Instance.RegPoolManager(this);
        }
        public static TtObjectPool<T> DefaultPool = new TtObjectPool<T>();
        public override Type ObjectType 
        {
            get => typeof(T);
        }
        public override string ShowName 
        {
            get => ObjectType.FullName;
        }
        protected virtual bool IsAsyncCreate { get => false; }
        Stack<T> mPool = new Stack<T>();
        public override int PoolSize
        {
            get
            {
                return mPool.Count;
            }
        }
        protected virtual T CreateObjectSync()
        {
            return new T();
        }
        protected virtual async Thread.Async.TtTask<T> CreateObjectAsync()
        {
            return default(T);
        }
        public void InternalPush(T t)
        {
            lock (mPool)
            {
                t.IsAlloc = false;
                mPool.Push(t);
            }
        }
        public T QueryObjectSync()
        {
            lock (this)
            {
                TtPooledSemaphore smp = null;
                if (mPool.Count == 0)
                {    
                    if (IsAsyncCreate)
                    {
                        smp = TtEngine.Instance.EventPoster.ParrallelForSmpAllocator.QueryObjectSync();
                        smp.Reset(GrowStep);
                    }
                    for (int i = 0; i < GrowStep; i++)
                    {
                        if (IsAsyncCreate)
                        {
                            var task = CreateObjectAsync();
                            if (task.IsCompleted)
                            {
                                InternalPush(task.DirectResult);
                                smp.Semaphore.Release();
                                task.Dispose();
                            }
                            else
                            {
                                TtEngine.Instance.TaskCollector.AddWaitTask(task, (ft) =>
                                {
                                    InternalPush(((TtTask<T>)ft).DirectResult);
                                    smp.Semaphore.Release();
                                });
                            }
                        }
                        else
                        {
                            var t = CreateObjectSync();
                            InternalPush(t);
                        }
                    }
                }
                if (mPool.Count == 0 && IsAsyncCreate)
                {
                    Thread.TtContextThread.CurrentContext.FlushToSemephore(smp.Semaphore);
                    TtEngine.Instance.EventPoster.ParrallelForSmpAllocator.ReleaseObject(smp);
                    //Thread.TtContextThread.CurrentContext.FlushAllThreadEvents();
                }
                var result = mPool.Peek();
                mPool.Pop();
                System.Diagnostics.Debug.Assert(result.IsAlloc == false);
                OnObjectQuery(result);
                result.IsAlloc = true;
                AliveNumber++;
                TotalQueryTimes++;
                return result;
            }
        }
        protected virtual void OnObjectQuery(T obj)
        {

        }
        protected virtual bool OnObjectRelease(T obj)
        {
            return true;
        }
        public bool ReleaseObject(T obj)
        {
            lock (this)
            {
                if (OnObjectRelease(obj) == false)
                    return false;

                System.Diagnostics.Debug.Assert(obj.IsAlloc == true);
                obj.IsAlloc = false;
                mPool.Push(obj);
                AliveNumber--;
                TotalReleaseTimes++;
                return true;
            }
        }
        public override bool ReleaseObject(IPooledObject tObj)
        {
            if (tObj == null)
                return false;
            return ReleaseObject(tObj);
        }
        protected virtual void OnFinalObject(T obj)
        {

        }
        public override void Cleanup()
        {
            lock (this)
            {
                foreach (var i in mPool)
                {
                    OnFinalObject(i);
                }
                mPool.Clear();
            }
        }
        public bool IsContains(T obj)
        {
            return mPool.Contains(obj);
        }
        public void Shrink(int poolSize)
        {
            lock (this)
            {
                var num = PoolSize - poolSize;
                for (int i = 0; i < num; i++)
                {
                    var t = mPool.Peek();
                    OnFinalObject(t);
                    t.IsAlloc = false;
                    mPool.Pop();
                }
            }
        }
    }
}

namespace EngineNS.UTest
{
    public class TtTestAsyncObject : IPooledObject
    {
        public bool IsAlloc { get; set; }
        public string A;
    }
    public class TtTestAsyncObjectPool : TtObjectPool<TtTestAsyncObject>
    {
        protected override bool IsAsyncCreate => true;
        protected override async TtTask<TtTestAsyncObject> CreateObjectAsync()
        {
            await TtEngine.Instance.EventPoster.Post((state) =>
            {
                System.Threading.Thread.Sleep(1000);
                return true;
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return new TtTestAsyncObject();
        }
    }
    [UTest.UTest]
    public partial class UTest_TestAsyncObjectPool
    {
        TtTestAsyncObjectPool Pool = new TtTestAsyncObjectPool();
        public void UnitTestEntrance()
        {
            //var o = Pool.QueryObjectSync();
            //o.A = "?";
        }
    }
}