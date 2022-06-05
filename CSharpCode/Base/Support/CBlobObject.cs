using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class CBlobObject : AuxPtrType<IBlobObject>
    {
        public CBlobObject()
        {
            mCoreObject = IBlobObject.CreateInstance();
        }
        public unsafe T* PushValue<T>(in T v) where T : unmanaged
        {
            fixed (T* p = &v)
            {
                var pos = mCoreObject.GetSize();
                mCoreObject.PushData(p, (uint)sizeof(T));
                return (T*)((byte*)mCoreObject.GetData() + pos);
            }
        }
    }
}
