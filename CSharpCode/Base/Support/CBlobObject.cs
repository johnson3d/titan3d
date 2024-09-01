using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class TtBlobObject : AuxPtrType<IBlobObject>
    {
        public TtBlobObject()
        {
            mCoreObject = IBlobObject.CreateInstance();
        }
        public uint Size
        {
            get => mCoreObject.GetSize();
        }
        public unsafe void* DataPointer
        {
            get => mCoreObject.GetData();
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
        public unsafe void PushData(void* data, uint size)
        {
            mCoreObject.PushData(data, size);
        }
        public unsafe IO.UMemReader CreateReader()
        {
            var result = IO.UMemReader.CreateInstance((byte*)mCoreObject.GetData(), (ulong)mCoreObject.GetSize());
            return result;
        }
    }
}
