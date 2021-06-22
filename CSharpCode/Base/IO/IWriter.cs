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

        void Write(bool v);
        void Write(sbyte v);
        void Write(Int16 v);
        void Write(Int32 v);
        void Write(Int64 v);
        void Write(byte v);
        void Write(UInt16 v);
        void Write(UInt32 v);
        void Write(UInt64 v);
        void Write(float v);
        void Write(double v);
        void Write(Vector2 v);
        void Write(Vector3 v);
        void Write(Vector4 v);
        void Write(Quaternion v);
        void Write(Matrix v);
        void Write(Guid v);
        void Write(Hash64 v);
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

        public void Write(bool v)
        {
            unsafe
            {
                sbyte v1 = v ? (sbyte)1 : (sbyte)0;
                WritePtr(&v1, sizeof(sbyte));
            }
        }
        public void Write(sbyte v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(sbyte));
            }
        }
        public void Write(Int16 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int16));
            }
        }
        public void Write(Int32 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int32));
            }
        }
        public void Write(Int64 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Int64));
            }
        }
        public void Write(byte v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(byte));
            }
        }
        public void Write(UInt16 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt16));
            }
        }
        public void Write(UInt32 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt32));
            }
        }
        public void Write(UInt64 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(UInt64));
            }
        }
        public void Write(float v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(float));
            }
        }
        public void Write(double v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(double));
            }
        }
        public void Write(Vector2 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector2));
            }
        }
        public void Write(Vector3 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector3));
            }
        }
        public void Write(Vector4 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Vector4));
            }
        }
        public void Write(Quaternion v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Quaternion));
            }
        }
        public void Write(Matrix v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Matrix));
            }
        }
        public void Write(Guid v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Guid));
            }
        }
        public void Write(Hash64 v)
        {
            unsafe
            {
                WritePtr(&v, sizeof(Hash64));
            }
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
                    return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)mCoreObject.GetDataPointer(), (int)mCoreObject.Tell());
                }
            }
        }
    }
}
