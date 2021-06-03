using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    [Flags]
    public enum EIOType : byte
    {
        None = 0,
        Normal = 1,
        File = 1 << 1,
        Network = 1 << 2,

        All = byte.MaxValue,
    }
    public interface IReader
    {
        object Tag { get; }
        EIOType IOType
        {
            get;
        }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void ReadPtr(void* p, int length);
        void OnReadError();

        void Read(out ISerializer v, object hostObject);
        void Read(out string v);
        void Read(out byte[] v);
        void Read(out VNameString v);

        void Read(out bool v);
        void Read(out sbyte v);
        void Read(out Int16 v);
        void Read(out Int32 v);
        void Read(out Int64 v);
        void Read(out byte v);
        void Read(out UInt16 v);
        void Read(out UInt32 v);
        void Read(out UInt64 v);
        void Read(out float v);
        void Read(out double v);
        void Read(out Vector2 v);
        void Read(out Vector3 v);
        void Read(out Vector4 v);
        void Read(out Quaternion v);
        void Read(out Matrix v);
        void Read(out Guid v);
        void Read(out Hash64 v);
        void Read<T>(out T v) where T : unmanaged;
    }

    public interface ICoreReader
    {
        EIOType IOType { get; }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void ReadPtr(void* p, int length);
    }
    public struct AuxReader<TR> : IReader where TR : ICoreReader
    {
        TR CoreReader;
        public AuxReader(TR cr, object tag)
        {
            CoreReader = cr;
            Tag = tag;
        }
        
        public object Tag
        {
            get;
            set;
        }
        public EIOType IOType
        {
            get { return CoreReader.IOType; }
        }
        public ulong GetPosition()
        {
            return CoreReader.GetPosition();
        }
        public void Seek(ulong pos)
        {
            CoreReader.Seek(pos);
        }
        public unsafe void ReadPtr(void* p, int length)
        {
            CoreReader.ReadPtr(p, length);
        }
        public void OnReadError()
        {

        }
        public void Read(out string v)
        {
            unsafe
            {
                unsafe
                {
                    int len = 0;
                    ReadPtr(&len, sizeof(int));
                    if (len == 0)
                    {
                        v = "";
                        return;
                    }
                    var str = new System.Char[len];
                    fixed (System.Char* pChar = &str[0])
                    {
                        ReadPtr(pChar, sizeof(System.Char) * len);
                        v = System.Runtime.InteropServices.Marshal.PtrToStringUni((IntPtr)pChar);
                    }
                }
            }
        }
        public void Read(out byte[] v)
        {
            unsafe
            {
                UInt16 len;
                ReadPtr(&len, sizeof(UInt16));
                v = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &v[0])
                    {
                        ReadPtr(p, len);
                    }
                }
            }
        }
        public void Read(out VNameString v)
        {
            unsafe
            {
                string text;
                Read(out text);
                v = new VNameString();
                v.UnsafeCallConstructor(text);
            }
        }

        public void ReadBigSize(out byte[] v)
        {
            unsafe
            {
                int len;
                ReadPtr(&len, sizeof(int));
                v = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &v[0])
                    {
                        ReadPtr(p, len);
                    }
                }
            }
        }
        public void ReadNoSize(out byte[] v, int len)
        {
            unsafe
            {
                v = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &v[0])
                    {
                        ReadPtr(p, len);
                    }
                }
            }
        }
        public void Read<T>(out T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    ReadPtr(p, sizeof(T));
                }
            }
        }
        public void Read(out ISerializer v, object hostObject = null)
        {
            bool isNull;
            this.Read(out isNull);
            if (isNull)
            {
                v = null;
                return;
            }
            Hash64 typeHash;
            this.Read(out typeHash);
            uint versionHash;
            this.Read(out versionHash);

            var savePos = this.GetPosition();
            uint ObjDataSize = 0;
            this.Read(out ObjDataSize);
            savePos += ObjDataSize;

            var meta = Rtti.UClassMetaManager.Instance.GetMeta(typeHash);
            if (meta == null)
            {
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.UMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                throw new Exception($"MetaVersion lost:{versionHash}");
            }
            v = Rtti.UTypeDescManager.CreateInstance(meta.ClassType.SystemType) as ISerializer;
            v.OnPreRead(this.Tag, hostObject, false);
            
            SerializerHelper.Read(this, v, metaVersion);
            //SerializerHelper.Read(this, out v, hostObject);
        }
        public void Read(out bool v1)
        {
            unsafe
            {
                sbyte v = 0;
                ReadPtr(&v, sizeof(sbyte));
                v1 = v == 1 ? true : false;
            }
        }
        public void Read(out sbyte v)
        {
            unsafe
            {
                fixed (sbyte* p = &v)
                {
                    ReadPtr(p, sizeof(sbyte));
                }
            }
        }
        public void Read(out Int16 v)
        {
            unsafe
            {
                fixed (Int16* p = &v)
                {
                    ReadPtr(p, sizeof(Int16));
                }
            }
        }
        public void Read(out Int32 v)
        {
            unsafe
            {
                fixed (Int32* p = &v)
                {
                    ReadPtr(p, sizeof(Int32));
                }
            }
        }
        public void Read(out Int64 v)
        {
            unsafe
            {
                fixed (Int64* p = &v)
                {
                    ReadPtr(p, sizeof(Int64));
                }
            }
        }
        public void Read(out byte v)
        {
            unsafe
            {
                fixed (byte* p = &v)
                {
                    ReadPtr(p, sizeof(byte));
                }
            }
        }
        public void Read(out UInt16 v)
        {
            unsafe
            {
                fixed (UInt16* p = &v)
                {
                    ReadPtr(p, sizeof(UInt16));
                }
            }
        }
        public void Read(out UInt32 v)
        {
            unsafe
            {
                fixed (UInt32* p = &v)
                {
                    ReadPtr(p, sizeof(UInt32));
                }
            }
        }
        public void Read(out UInt64 v)
        {
            unsafe
            {
                fixed (UInt64* p = &v)
                {
                    ReadPtr(p, sizeof(UInt64));
                }
            }
        }
        public void Read(out float v)
        {
            unsafe
            {
                fixed (float* p = &v)
                {
                    ReadPtr(p, sizeof(float));
                }
            }
        }
        public void Read(out double v)
        {
            unsafe
            {
                fixed (double* p = &v)
                {
                    ReadPtr(p, sizeof(double));
                }
            }
        }
        public void Read(out Vector2 v)
        {
            unsafe
            {
                fixed (Vector2* p = &v)
                {
                    ReadPtr(p, sizeof(Vector2));
                }
            }
        }
        public void Read(out Vector3 v)
        {
            unsafe
            {
                fixed (Vector3* p = &v)
                {
                    ReadPtr(p, sizeof(Vector3));
                }
            }
        }
        public void Read(out Vector4 v)
        {
            unsafe
            {
                fixed (Vector4* p = &v)
                {
                    ReadPtr(p, sizeof(Vector4));
                }
            }
        }
        public void Read(out Quaternion v)
        {
            unsafe
            {
                fixed (Quaternion* p = &v)
                {
                    ReadPtr(p, sizeof(Quaternion));
                }
            }
        }
        public void Read(out Matrix v)
        {
            unsafe
            {
                fixed (Matrix* p = &v)
                {
                    ReadPtr(p, sizeof(Matrix));
                }
            }
        }
        public void Read(out Guid v)
        {
            unsafe
            {
                fixed (Guid* p = &v)
                {
                    ReadPtr(p, sizeof(Guid));
                }
            }
        }
        public void Read(out Hash64 v)
        {
            unsafe
            {
                fixed (Hash64* p = &v)
                {
                    ReadPtr(p, sizeof(Hash64));
                }
            }
        }
    }
    public class CMemStreamReader : AuxPtrType<MemStreamReader>
    {
        public CMemStreamReader()
        {
            mCoreObject = MemStreamReader.CreateInstance();
        }
    }
}
