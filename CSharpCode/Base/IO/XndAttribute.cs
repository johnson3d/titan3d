using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial struct XndAttribute
    {
        public string Name
        {
            get
            {
                return NativeSuper.GetName();
            }
            set
            {
                var obj = NativeSuper;
                obj.SetName(value);
            }
        }
        public UInt32 Version
        {
            get
            {
                return NativeSuper.mVersion;
            }
            set
            {
                var obj = NativeSuper;
                obj.mVersion = value;
            }
        }
        public UInt32 Flags
        {
            get
            {
                return NativeSuper.mFlags;
            }
            set
            {
                var super = NativeSuper;
                super.mFlags = value;
            }
        }
        public void Write<T>(T v) where T : unmanaged
        {
            unsafe
            {
                this.Write(&v, (uint)sizeof(T));
            }
        }
        public unsafe uint Read<T>(ref T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    return this.Read(p, (uint)sizeof(T));
                }
            }
            
        }
        public IO.AuxReader<IO.XndAttributeReader> GetReader(object tag)
        {
            unsafe
            {
                this.BeginRead();
                return new IO.AuxReader<IO.XndAttributeReader>(new IO.XndAttributeReader(this), tag);
            }
        }
        public void ReleaseReader(ref IO.AuxReader<IO.XndAttributeReader> reader)
        {
            this.EndRead();
        }
        public IO.AuxWriter<IO.XndAttributeWriter> GetWriter(ulong length)
        {
            unsafe
            {
                this.BeginWrite(length);
                return new IO.AuxWriter<IO.XndAttributeWriter>(new IO.XndAttributeWriter(this));
            }
        }
        public void ReleaseWriter(ref IO.AuxWriter<IO.XndAttributeWriter> writer)
        {
            this.EndWrite();
        }
    }
}

namespace EngineNS.IO
{
    public struct XndAttributeReader : ICoreReader
    {
        XndAttribute mAttribute;
        public XndAttributeReader(XndAttribute attr)
        {
            mAttribute = attr;
        }
        public EIOType IOType
        {
            get { return EIOType.File; }
        }
        public ulong GetPosition()
        {
            return mAttribute.GetReaderPosition();
        }
        public void Seek(ulong offset)
        {
            mAttribute.ReaderSeek(offset);
        }
        public unsafe void ReadPtr(void* p, int length)
        {
            mAttribute.Read(p, (uint)length);
        }
    }
    public struct XndAttributeWriter : ICoreWriter
    {
        XndAttribute mAttribute;
        public XndAttributeWriter(XndAttribute attr)
        {
            mAttribute = attr;
        }
        public EIOType IOType
        {
            get { return EIOType.File; }
        }
        public ulong GetPosition()
        {
            return mAttribute.GetWriterPosition();
        }
        public void Seek(ulong offset)
        {
            mAttribute.WriterSeek(offset);
        }
        public unsafe void WritePtr(void* p, int length)
        {
            mAttribute.Write(p, (uint)length);
        }
    }
}
