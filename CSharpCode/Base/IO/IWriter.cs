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
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void WritePtr(void* p, int length);

        void Write(ISerializer v);
        void Write(string v);
        void Write(byte[] v);
        void Write(VNameString v);

        void Write<T>(T v) where T : unmanaged;
    }

    public interface ICoreWriter
    {
        EIOType IOType { get; }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void WritePtr(void* p, int length);
    }

    public struct AuxWriter<TR> : IWriter where TR : ICoreWriter
    {
        public TR CoreWriter;
        public AuxWriter(TR cr)
        {
            CoreWriter = cr;
        }
        public EIOType IOType
        {
            get { return CoreWriter.IOType; }
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
        public void WriteIntSize(byte[] v)
        {
            unsafe
            {
                var len = (UInt16)v.Length;
                fixed (byte* p = &v[0])
                {
                    WritePtr(p, len);
                }
            }
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
    }

    public class CMemStreamWriter : AuxPtrType<MemStreamWriter>
    {
        public CMemStreamWriter()
        {
            mCoreObject = MemStreamWriter.CreateInstance();
        }
        public void SetText(string txt)
        {
            unsafe
            {
                var ptr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(txt);
                mCoreObject.Seek(0);
                mCoreObject.Write(ptr.ToPointer(), CoreSDK.SDK_StrLen(ptr.ToPointer()));
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            }
        }
        public string AsText
        {
            get
            {
                unsafe
                {
                    if (mCoreObject.Tell() == 0)
                        return null;
                    return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)mCoreObject.GetPointer(), (int)mCoreObject.Tell());
                }
            }
        }
    }
}
