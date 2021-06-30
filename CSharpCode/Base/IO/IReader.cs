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
        public TR CoreReader;
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
                v.Index = VNameString.GetIndexFromString(text);
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
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"Meta Type lost:{typeHash}");
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.UMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"Meta Type lost in direct:{meta.MetaDirectoryName}");
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"MetaVersion lost:{versionHash}");
                throw new Exception($"MetaVersion lost:{versionHash}");
            }
            v = Rtti.UTypeDescManager.CreateInstance(meta.ClassType.SystemType) as ISerializer;
            v.OnPreRead(this.Tag, hostObject, false);
            
            SerializerHelper.Read(this, v, metaVersion);
            //SerializerHelper.Read(this, out v, hostObject);
        }

        public bool ReadTo(ISerializer v, object hostObject = null)
        {
            bool isNull;
            this.Read(out isNull);
            if (isNull)
            {
                v = null;
                return false;
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
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"Meta Type lost:{typeHash}");
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.UMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"Meta Type lost in direct:{meta.MetaDirectoryName}");
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"MetaVersion lost:{versionHash}");
                throw new Exception($"MetaVersion lost:{versionHash}");
            }
            if (meta.ClassType.SystemType != v.GetType())
                return false;
            //v = Rtti.UTypeDescManager.CreateInstance(meta.ClassType.SystemType) as ISerializer;
            v.OnPreRead(this.Tag, hostObject, false);

            SerializerHelper.Read(this, v, metaVersion);
            //SerializerHelper.Read(this, out v, hostObject);
            return true;
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
