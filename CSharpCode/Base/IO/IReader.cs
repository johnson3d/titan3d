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
    public interface ICoreReader
    {
        EIOType IOType { get; }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void ReadPtr(void* p, int length);
    }
    public interface IReader : ICoreReader
    {
        object Tag { get; }
        void OnReadError();

        void Read(out ISerializer v, object hostObject);
        void Read(out string v);
        void Read(out byte[] v);
        void Read(out RName v);
        void Read(out VNameString v);
        void Read(out Rtti.TtTypeDesc v); 
        void Read(ref Support.TtBitset v);
        void Read(out TtMemWriter v);
        void Read<T>(out T v) where T : unmanaged;
        T Read<T>() where T : unmanaged;

        public object ReadWithType(Type type);
    }

    public struct TtMemReader : IO.ICoreReader, IDisposable
    {
        private unsafe static CoreSDK.FDelegate_FGetMemStream NativeGetMemStream = NativeGetMemStreamCB;
        private unsafe static bool NativeGetMemStreamCB(EngineNS.MemStreamReader arg0, sbyte* arg1, sbyte* arg2)
        {
            var name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg1);
            var type = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg2);
            if (type == "DX12GpuDrawState")
            {
                var path = TtEngine.Instance.FileManager.GetPath(TtFileManager.ERootDir.Cache, TtFileManager.ESystemDir.PSO) + "dx12/";
                using (var xnd = TtXndHolder.LoadXnd(path + name + ".pso"))
                {
                    if (xnd == null)
                        return false;
                    var attr = xnd.RootNode.TryGetAttribute("Blob");
                    using (var ar = attr.GetReader(null))
                    {
                        arg0.ResetData(attr.GetReaderLength());
                        ar.ReadPtr(arg0.GetPointer(), (int)attr.GetReaderLength());
                    }
                }
                return true;
            }
            return false;
        }
        public static void InitNativeCallback()
        {
            CoreSDK.SetGetMemStreamCallback(NativeGetMemStream);
        }
        public static void FinalNativeCallback()
        {
            CoreSDK.SetGetMemStreamCallback(null);
        }
        public unsafe static TtMemReader CreateInstance(byte* ptr, ulong len)
        {
            TtMemReader result = new TtMemReader();
            result.Reader = MemStreamReader.CreateInstance();
            result.Reader.ProxyPointer(ptr, len);
            return result;
        }
        public unsafe static TtMemReader CreateInstance(in TtMemWriter writer)
        {
            TtMemReader result = new TtMemReader();
            result.Reader = MemStreamReader.CreateInstance();
            result.Reader.ProxyPointer((byte*)writer.Ptr, writer.GetPosition());
            return result;
        }
        public MemStreamReader Reader;
        public IO.EIOType IOType { get => IO.EIOType.Network; }
        public ulong GetPosition()
        {
            return Reader.Tell();
        }
        public void Seek(ulong pos)
        {
            Reader.Seek(pos);
        }
        public unsafe void ReadPtr(void* p, int length)
        {
            Reader.Read(p, (uint)length);
        }
        public void Dispose()
        {
            Reader.Dispose();
        }
        public unsafe void Read<T>(out T v) where T : unmanaged
        {
            fixed(T* p = &v)
            {
                ReadPtr(p, sizeof(T));
            }
        }
    }

    public struct AuxReader<TR> : IReader, IDisposable where TR : ICoreReader
    {
        public System.Action DisposeAction = null;
        public TR CoreReader;
        public AuxReader(TR cr, object tag)
        {
            CoreReader = cr;
            Tag = tag;
        }
        public void Dispose()
        {
            if (DisposeAction != null)
            {
                DisposeAction();
            }
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
                var str = new System.Char[len + 1];
                str[len] = '\0';
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
        public void Read(out RName v)
        {
            RName.ERNameType type;
            ushort Version;
            Read(out Version);
            if (Version < RName.MinVersion || Version == (ushort)RName.ERNameType.Unkown)
            {
                type = (RName.ERNameType)Version;
                if (type == RName.ERNameType.Unkown)
                {
                    v = null;
                    return;
                }
                string name;
                Read(out name);
                v = RName.GetRName(name, type);
            }
            else
            {
                if (Version == RName.Version100)
                {
                    Read(out type);
                    if (type == RName.ERNameType.Unkown)
                    {
                        v = null;
                        return;
                    }
                    string name;
                    Read(out name);
                    v = RName.GetRName(name, type);
                }
                else if (Version == RName.Version101)
                {
                    Read(out type);
                    if (type == RName.ERNameType.Unkown)
                    {
                        v = null;
                        return;
                    }
                    string name;
                    Read(out name);
                    Guid assetId;
                    Read(out assetId);
                    var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(assetId);
                    if (meta!=null)
                    {
                        v = meta.AssetName;
                        if (v.RNameType != type || v.Name != name)
                        {
                            Profiler.Log.WriteLine<Profiler.TtAssetGategory>(Profiler.ELogTag.Warning, $"RName info not match AssetMeta:{v},read type:{type},name:{name}");
                        }
                    }
                    else
                    {
                        v = RName.GetRName(name, type);
                    }
                }
                else
                {
                    throw new Exception("RName Version not support:" + Version);
                }
            }
        }
        public void Read(ref Support.TtBitset v)
        {
            uint bitCount = 0;
            Read(out bitCount);
            if (v == null)
            {
                v = new Support.TtBitset(bitCount);
            }
            else if(v.BitCount != bitCount)
            {
                v.SetBitCount(bitCount);
            }
            unsafe
            {
                ReadPtr(v.Data, (int)v.DataByteSize);
            }
        }
        public unsafe void Read(out TtMemWriter v)
        {
            uint len = 0;
            Read(out len);
            v = TtMemWriter.CreateInstance();
            v.ResetSize(len + 1);
            v.Seek(len);
            ReadPtr(v.Ptr, (int)len);
        }
        public void Read(out Rtti.TtTypeDesc v)
        {
            string typeStr;
            Read(out typeStr);
            v = Rtti.TtTypeDesc.TypeOf(typeStr);
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
        public void ReadNoSize(byte[] v, int len)
        {
            unsafe
            {
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
        public void Read<T>(out T v, bool noused = false) where T : class, IO.ISerializer
        {
            ReadObject<T>(out v);
        }
        public void ReadObject<T>(out T v) where T : class, IO.ISerializer
        {
            IO.ISerializer tmp;
            Read(out tmp);
            v = tmp as T;
        }
        public T Read<T>() where T : unmanaged
        {
            unsafe
            {
                T v;
                ReadPtr(&v, sizeof(T));
                return v;
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

            uint magic;
            this.Read(out magic);
            UInt64 versionHash;
            if (magic != SerializerHelper.HashMagic)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"读取到老的资产格式请重新保存成最新版本");
                versionHash = magic;
            }
            else
            {
                this.Read(out versionHash);
            }

            var savePos = this.GetPosition();
            uint ObjDataSize = 0;
            this.Read(out ObjDataSize);
            savePos += ObjDataSize;

            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeHash);
            if (meta == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"Meta Type lost:{typeHash}");
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.TtMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"Meta Type lost in direct:{meta.ClassMetaName}");
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"MetaVersion lost:{versionHash}");
                throw new Exception($"MetaVersion lost:{versionHash}");
            }
            v = Rtti.TtTypeDescManager.CreateInstance(meta.ClassType) as ISerializer;
            v.OnPreRead(this.Tag, hostObject, false);
            
            SerializerHelper.Read(this, v, metaVersion);
            //SerializerHelper.Read(this, out v, hostObject);
            v.OnPostRead(this.Tag, hostObject, false);
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
            uint magic;
            this.Read(out magic);
            UInt64 versionHash;
            if (magic != SerializerHelper.HashMagic)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"读取到老的资产格式请重新保存成最新版本");
                versionHash = magic;
            }
            else
            {
                this.Read(out versionHash);
            }

            //var savePos = this.GetPosition();
            //uint ObjDataSize = 0;
            //this.Read(out ObjDataSize);
            //savePos += ObjDataSize;

            var savePos = IO.SerializerHelper.GetSkipOffset(this);

            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeHash);
            if (meta == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"Meta Type lost:{typeHash}");
                throw new Exception($"Meta Type lost:{typeHash}");
            }
            Rtti.TtMetaVersion metaVersion = meta.GetMetaVersion(versionHash);
            if (metaVersion == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"Meta Type lost in direct:{meta.ClassMetaName}");
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"MetaVersion lost:{versionHash}");
                this.Seek(savePos);
                return false;
                //throw new Exception($"MetaVersion lost:{versionHash}");
            }
            if (!meta.ClassType.IsEqual(v.GetType()))
                return false;
            //v = Rtti.UTypeDescManager.CreateInstance(meta.ClassType.SystemType) as ISerializer;
            v.OnPreRead(this.Tag, hostObject, false);

            SerializerHelper.Read(this, v, metaVersion);
            //SerializerHelper.Read(this, out v, hostObject);
            v.OnPostRead(this.Tag, hostObject, false);
            return true;
        }

        public object ReadWithType(Type type)
        {
            if (type == typeof(byte))
            {
                byte value;
                Read(out value);
                return value;
            }
            else if (type == typeof(short))
            {
                short value;
                Read(out value);
                return value;
            }
            else if (type == typeof(ushort))
            {
                ushort value;
                Read(out value);
                return value;
            }
            else if (type == typeof(int))
            {
                int value;
                Read(out value);
                return value;
            }
            else if (type == typeof(uint))
            {
                uint value;
                Read(out value);
                return value;
            }
            else if (type == typeof(long))
            {
                long value;
                Read(out value);
                return value;
            }
            else if (type == typeof(ulong))
            {
                ulong value;
                Read(out value);
                return value;
            }
            else if (type == typeof(float))
            {
                float value;
                Read(out value);
                return value;
            }
            else if (type == typeof(double))
            {
                double value;
                Read(out value);
                return value;
            }
            else if (type == typeof(bool))
            {
                bool value;
                Read(out value);
                return value;
            }
            else if (type == typeof(string))
            {
                string value;
                Read(out value);
                return value;
            }
            else if (type == typeof(VNameString))
            {
                VNameString value;
                Read(out value);
                return value;
            }
            else if (type == typeof(Support.TtBitset))
            {
                Support.TtBitset value = new Support.TtBitset();
                Read(ref value);
                return value;
            }
            else if (type == typeof(byte[]))
            {
                byte[] value;
                Read(out value);
                return value;
            }
            else if (type == typeof(RName))
            {
                RName value;
                Read(out value);
                return value;
            }
            else if (typeof(ISerializer).IsAssignableFrom(type))
            {
                ISerializer value;
                Read(out value);
                return value;
            }
            else
            {
                throw new InvalidOperationException($"Unsupport type serialize {type.FullName}");
            }
        }
    }
}
