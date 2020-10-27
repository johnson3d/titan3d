using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO.Serializer
{
    public class ChunkReader : AuxIReader
    {
        public override EIOType IOType
        {
            get
            {
                return EIOType.Normal;
            }
        }
        protected byte[] mHandle;
        protected int mPos;
        protected int mSize;

        public ChunkReader()
        {
            mHandle = new byte[512];
            mPos = 0;
            mSize = 0;
        }
        public ChunkReader(byte[] data)
        {
            SetBuffer(data, 0);
        }
        public void SetBuffer(byte[] data, int pos)
        {
            mHandle = data;
            mPos = pos;
            mSize = data.Length;
        }
        public int CurPtr()
        {
            return mPos;
        }

        public byte[] Ptr()
        {
            return mHandle;
        }

        public override void OnReadError()
        {
            throw new System.IndexOutOfRangeException("ChunkReader Read Error");
        }

        public override void Read(ISerializer v)
        {
            v.ReadObject(this);
        }
        public void ReadPOD(System.Type type, out System.Object obj)
        {
            var tDesc = TypeDescGenerator.Instance.GetSerializer(type);
            if (tDesc == null)
            {
                obj = null;
                return;
            }
            obj = tDesc.ReadValue(this);
        }

        #region Read POD
        public unsafe override void ReadPtr(void* p, int length)
        {
            var offset = CurPtr();
            if (offset + length > mSize)
            {
                OnReadError();
                return;
            }
            byte* tar = (byte*)p;

            fixed (byte* src = &mHandle[offset])
            {
                CoreSDK.SDK_Memory_Copy(tar, src, (uint)length);
            }
            mPos += length;
        }
        #endregion
    }
    public class ChunkWriter : AuxIWriter
    {
        public override EIOType IOType
        {
            get
            {
                return EIOType.Normal;
            }
        }
        protected int mPos;
        protected int mBuffSize;
        protected byte[] mHandle;

        public ChunkWriter(int Capacity = 128, int maxGrowStep = 64 * 1024)
        {
            mHandle = new byte[Capacity];

            mBuffSize = Capacity;
            mPos = 0;
            C_MAXDATASIZE = maxGrowStep;
        }
        public ChunkWriter(byte[] data, int size)
        {
            mHandle = new byte[size];
            mBuffSize = size;
            mPos = 0;
            Buffer.BlockCopy(data, 0, mHandle, 0, size);
        }
        public int CurPtr()
        {
            return mPos;
        }
        public byte[] Ptr
        {
            get { return mHandle; }
        }

        public void CopyDataRead(ChunkReader reader)
        {
            var src = reader.Ptr();
            byte[] data = new byte[src.Length + 1];
            Buffer.BlockCopy(src, 0, data, 0, src.Length);

            mHandle = data;
            mPos = src.Length;
            mBuffSize = src.Length;
        }


        #region Write POD
        int C_MAXDATASIZE = 64 * 1024;
        private void FixSize(int growSize)
        {
            if (growSize >= C_MAXDATASIZE)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO","PackageWriter FixSIze = {0}", growSize);
                throw new Exception("OutOfMemory By PackageWriter FixSIze");
            }
            int nsize = mPos + growSize;
            if (nsize > mBuffSize)
            {
                nsize += mBuffSize / 2;
                byte[] nBuffer = new byte[nsize];
                Buffer.BlockCopy(mHandle, 0, nBuffer, 0, mPos);
                mHandle = nBuffer;
                mBuffSize = nsize;
            }
        }
        public unsafe override void WritePtr(void* p, int length)
        {
            if (length == 0)
                return;
            FixSize(length);

            byte* src = (byte*)p;
            fixed (byte* tar = &mHandle[CurPtr()])
            {
                CoreSDK.SDK_Memory_Copy(tar, src, (uint)length);
            }
            mPos += length;
        }
        private void WriteByteArray(byte[] v, int offset, int len)
        {
            if (offset + len >= v.Length)
            {
                return;
            }
            unsafe
            {
                fixed (byte* p = &v[offset])
                {
                    WritePtr(p, len);
                }
            }
        }
        #endregion
    }

    public class MemChunk
    {
        public ChunkReader Reader;
        public ChunkWriter Writer;

        public MemChunk CloneObject()
        {
            MemChunk cloned = new MemChunk();
            if (Reader != null)
            {
                cloned.Reader = new ChunkReader(Reader.Ptr());
            }
            if (Writer != null)
            {
                cloned.Writer = new ChunkWriter(Writer.Ptr, Writer.Ptr.Length);
            }
            return cloned;
        }
    }
}
