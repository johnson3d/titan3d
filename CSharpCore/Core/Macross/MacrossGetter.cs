using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Macross
{
    public interface INewMacross
    {
        void OnNewMacross();
    }

    public class MacrossGetter<T> where T : class, new()
    {
        public MacrossGetter(RName rn, T obj)
        {
            mName = rn;
            if (obj != null)
            {
                mPtr = obj;
                mDesc = Macross.MacrossFactory.Instance.GetDesc(rn);
                if (mDesc == null)
                {
                    mDesc = Macross.MacrossFactory.SetVersion(rn, Macross.MacrossFactory.Instance.GetVersion(rn));
                }
                mVersion = mDesc.Version;
                mManagerVersion = CEngine.Instance.MacrossDataManager.CollectorVersion;
            }
        }
        private RName mName;
        [Browsable(false)]
        public RName Name
        {
            get { return mName; }
        }
        [Browsable(false)]
        public int Version
        {
            get { return mVersion; }
        }
        [Browsable(false)]
        public UInt32 ManagerVersion
        {
            get { return mManagerVersion; }
        }
        T mPtr;
        public bool IsInited
        {
            get
            {
                return mPtr != null;
            }
        }
        int mVersion;
        UInt32 mManagerVersion;
        MacrossDesc mDesc;

        public void Reset()
        {
            mPtr = null;
            mVersion = 0;
            mManagerVersion = 0;
        }

        public T Get(bool OnlyForGame = true)
        {
            if (OnlyForGame && CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                return null;
            if (Name == null || CEngine.Instance.MacrossDataManager.IsValid == false)
                return null;
            if (mPtr == null || mDesc.Version != mVersion || CEngine.Instance.MacrossDataManager.CollectorVersion != mManagerVersion)
            {
                var newObj = (T)Macross.MacrossFactory.Instance.CreateMacrossObject(Name, out mDesc);
                Rtti.MetaClass.CopyData(mPtr, newObj);
#if DEBUG
                var saved = mPtr;
#endif
                mPtr = newObj;
                if (mPtr == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", $"CreateMacrossObject Type={Name} failed, new a default object");
                    mPtr = new T();
                    mDesc = new MacrossDesc();
                }
                else
                {
                    if (mPtr.GetType().IsSubclassOf(typeof(T)) == false)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", $"CreateMacrossObject Type={Name} is not match as {typeof(T).FullName}");
                        mPtr = null;
                        return null;
                    }
                }
                mVersion = mDesc.Version;
                mManagerVersion = CEngine.Instance.MacrossDataManager.CollectorVersion;
                if (mPtr.GetType().GetInterface(typeof(INewMacross).FullName) != null)
                {
                    var nmPtr = mPtr as INewMacross;
                    if (nmPtr != null)
                    {
                        nmPtr.OnNewMacross();
                    }
                }
            }
            return mPtr;
        }
        public NT CastGet<NT>(bool OnlyForGame = true) where NT : T
        {
            return (NT)Get(OnlyForGame);
        }
    }
}
