using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class PooledObjectBase
    {
        public virtual bool ReleaseObject(object obj)
        {
            return true;
        }
    }
    public class PooledObject<T> : PooledObjectBase where T : class, new()
    {
        public static PooledObject<T> DefaultPool = new PooledObject<T>();
        public int GrowStep
        {
            get;
            set;
        } = 10;
        List<T> mPool = new List<T>();
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
#pragma warning disable 1998
        protected virtual async System.Threading.Tasks.Task<T> CreateObject(RName name)
        {
            return new T();
        }
#pragma warning restore 1998
        public T QueryObjectSync()
        {
            lock(this)
            {
                if (mPool.Count == 0)
                {
                    for (int i = 0; i < GrowStep; i++)
                    {
                        mPool.Add(new T());
                    }
                }
                var result = mPool[mPool.Count - 1];
                mPool.RemoveAt(mPool.Count - 1);
                OnObjectQuery(result);
                AliveNumber++;
                return result;
            }
        }
        public async System.Threading.Tasks.Task<T> QueryObject(RName name)
        {
            if (mPool.Count == 0)
            {
                for (int i = 0; i < GrowStep; i++)
                {
                    var obj = await CreateObject(name);
                    lock (mPool)
                    {
                        mPool.Add(obj);
                    }
                }
            }
            lock (this)
            {
                var result = mPool[mPool.Count - 1];
                mPool.RemoveAt(mPool.Count - 1);
                OnObjectQuery(result);
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
            lock(this)
            {
                if (OnObjectRelease(obj) == false)
                    return false;
                mPool.Add(obj);
                AliveNumber--;
                return true;
            }
        }
        public override bool ReleaseObject(object obj)
        {
            var tObj = obj as T;
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
    }

}
