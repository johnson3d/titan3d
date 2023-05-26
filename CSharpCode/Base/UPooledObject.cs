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

    public class TtObjectPool<T> : IObjectPoolBase where T : IPooledObject, new()
    {
        public TtObjectPool()
        {
            TtObjectPoolManager.Instance.RegPoolManager(this);
        }
        public static TtObjectPool<T> DefaultPool = new TtObjectPool<T>();
        public int GrowStep
        {
            get;
            set;
        } = 10;
        Stack<T> mPool = new Stack<T>();
        public int PoolSize
        {
            get
            {
                return mPool.Count;
            }
        }
        public int AliveNumber
        {
            get;
            private set;
        } = 0;
        protected virtual T CreateObjectSync()
        {
            return new T();
        }
        public T QueryObjectSync()
        {
            lock (this)
            {
                if (mPool.Count == 0)
                {
                    for (int i = 0; i < GrowStep; i++)
                    {
                        var t = CreateObjectSync();
                        t.IsAlloc = false;
                        mPool.Push(t);
                    }
                }
                var result = mPool.Peek();
                mPool.Pop();
                System.Diagnostics.Debug.Assert(result.IsAlloc == false);
                OnObjectQuery(result);
                result.IsAlloc = true;
                AliveNumber++;
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
                return true;
            }
        }
        public bool ReleaseObject(IPooledObject tObj)
        {
            if (tObj == null)
                return false;
            return ReleaseObject(tObj);
        }
        protected virtual void OnFinalObject(T obj)
        {

        }
        public void Cleanup()
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
