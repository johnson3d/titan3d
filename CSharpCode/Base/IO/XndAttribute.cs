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
        public IO.AuxReader<IO.TtXndAttributeReader> GetReader(object tag)
        {
            unsafe
            {
                this.BeginRead();
                var result = new IO.AuxReader<IO.TtXndAttributeReader>(new IO.TtXndAttributeReader(this), tag);
                var This = this;
                result.DisposeAction = () =>
                {
                    This.EndRead();
                };

                return result;
            }
        }
        //public void ReleaseReader(ref IO.AuxReader<IO.TtXndAttributeReader> reader)
        //{
        //    this.EndRead();
        //}
        public IO.AuxWriter<IO.TtXndAttributeWriter> GetWriter(ulong length)
        {
            unsafe
            {
                this.BeginWrite(length);
                var result = new IO.AuxWriter<IO.TtXndAttributeWriter>(new IO.TtXndAttributeWriter(this));
                var This = this;
                result.DisposeAction = () =>
                {
                    This.EndWrite();
                };

                return result;
            }
        }
        //public void ReleaseWriter(ref IO.AuxWriter<IO.TtXndAttributeWriter> writer)
        //{
        //    this.EndWrite();
        //}
    }
}

namespace EngineNS.IO
{
    public struct TtXndAttributeReader : ICoreReader
    {
        XndAttribute mAttribute;
        public TtXndAttributeReader(XndAttribute attr)
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
    public struct TtXndAttributeWriter : ICoreWriter
    {
        XndAttribute mAttribute;
        public TtXndAttributeWriter(XndAttribute attr)
        {
            mAttribute = attr;
        }
        public EIOType IOType
        {
            get { return EIOType.File; }
        }
        public unsafe void* Ptr
        {
            get
            {
                return mAttribute.GetWriterPtr();
            }
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
