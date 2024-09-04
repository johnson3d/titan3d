using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public interface IWriter
    {
        EIOType IOType
        {
            get;
        }
        unsafe void* Ptr
        {
            get;
        }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void WritePtr(void* p, int length);

        void Write(ISerializer v);
        void Write(string v);
        void Write(byte[] v);
        void Write(VNameString v);
        void Write(RName v);
        void Write(Support.TtBitset v);

        void Write<T>(T v) where T : unmanaged;
    }

    public interface ICoreWriter
    {
        EIOType IOType { get; }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void WritePtr(void* p, int length);
        unsafe void* Ptr { get; }
    }

    public partial struct UMemWriter : IO.ICoreWriter, IDisposable
    {
        public static UMemWriter CreateInstance()
        {
            UMemWriter result = new UMemWriter();
            result.Writer = MemStreamWriter.CreateInstance();
            return result;
        }
        public object Tag;
        public MemStreamWriter Writer;
        public unsafe void* Ptr
        {
            get
            {
                return Writer.GetPointer();
            }
        }
        public void ResetSize(ulong size)
        {
            Writer.ResetBufferSize(size);
        }
        public IO.EIOType IOType { get => IO.EIOType.Network; }
        public ulong GetPosition()
        {
            return Writer.Tell();
        }
        public void Seek(ulong pos)
        {
            Writer.Seek(pos);
        }
        public unsafe void WritePtr(void* p, int length)
        {
            Writer.Write(p, (uint)length);
        }
        public void Dispose()
        {
            Writer.Dispose();
        }
    }

    public struct AuxWriter<TR> : IWriter, IDisposable where TR : ICoreWriter
    {
        public System.Action DisposeAction = null;
        public TR CoreWriter;
        public AuxWriter(TR cr)
        {
            CoreWriter = cr;
        }
        public void Dispose()
        {
            if (DisposeAction != null)
                DisposeAction();
        }
        public EIOType IOType
        {
            get { return CoreWriter.IOType; }
        }
        public unsafe void* Ptr
        {
            get
            {
                return CoreWriter.Ptr;
            }
        }
        public ulong GetPosition()
        {
            return CoreWriter.GetPosition();
        }
        public void Seek(ulong pos)
        {
            CoreWriter.Seek(pos);
        }
        public unsafe void WritePtr(void* p, int length)
        {
            CoreWriter.WritePtr(p, length);
        }
        public void Write<T>(T v) where T : unmanaged
        {
            unsafe
            {
                WritePtr(&v, sizeof(T));
            }
        }
        public unsafe void Write<T>(in T v) where T : unmanaged
        {
            fixed(T* p = &v)
            {
                WritePtr(p, sizeof(T));
            }
        }
        public void Write(string v)
        {
            unsafe
            {
                int len;
                if (v == null)
                {
                    len = 0;
                }
                else
                {
                    len = (int)v.Length;
                }
                WritePtr(&len, sizeof(int));
                if (len > 0)
                {
                    fixed (System.Char* pPtr = v)
                    {
                        WritePtr(pPtr, sizeof(System.Char) * len);
                    }
                }
            }
        }
        public void Write(byte[] v)
        {
            unsafe
            {
                var len = (UInt16)v.Length;
                WritePtr(&len, sizeof(UInt16));
                fixed (byte* p = &v[0])
                {
                    WritePtr(p, len);
                }
            }
        }
        public void Write(VNameString v)
        {
            Write(v.Text);
        }
        public void Write(RName v)
        {
            Write(v.RNameType);
            Write(v.Name);
        }
        public void WriteNoSize(byte[] v, int len)
        {
            System.Diagnostics.Debug.Assert(len <= v.Length);
            unsafe
            {
                fixed (byte* p = &v[0])
                {
                    WritePtr(p, len);
                }
            }
        }
        public void Write(ISerializer v)
        {
            SerializerHelper.Write(this, v);
        }
        public void Write(Support.TtBitset v)
        {
            Write(v.BitCount);
            unsafe
            {
                WritePtr(v.Data, (int)v.DataByteSize);
            }
        }
        public unsafe void Write(UMemWriter v)
        {
            Write((uint)v.GetPosition());
            WritePtr(v.Ptr, (int)v.GetPosition());
        }
    }

}
