#define COMPACT_OLD
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NetCore
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct PkgHeader
    {
        [System.Flags]
        enum eHeadParam : byte
        {
            UserFlagMask = 0xF,//用户自定义数据占4bit
            HasReturn = (1 << 5),
            HashIndex = (1 << 6),
            WeakPkg = (1 << 7),//弱包，当服务器处理不过来的时候可以抛弃
        };

        public void ToDefault()
        {
            PackageSize = 0;
            PKGType = 0;
        }

        public ushort PackageSize;
        public byte PKGType;
        

        public static int SizeOf()
        {
            unsafe
            {
                return sizeof(PkgHeader);
            }
        }

        public void SetWeakPkg(bool b)
        {
            if (b)
            {
                PKGType |= (byte)eHeadParam.WeakPkg;//弱包，可以抛弃
            }
            else
            {
                unchecked
                {
                    PKGType &= (byte)(~eHeadParam.WeakPkg);
                }
            }
        }
        public bool IsWeakPkg()
        {
            if ((PKGType & (byte)eHeadParam.WeakPkg) > 0)
                return true;
            else
                return false;
        }
        public void SetHashIndex(bool b)
        {
            if (b)
            {
                PKGType |= (byte)eHeadParam.HashIndex;
            }
            else
            {
                unchecked
                {
                    PKGType &= (byte)(~eHeadParam.HashIndex);
                }
            }
        }
        public bool IsHashIndex()
        {
            if ((PKGType & (byte)eHeadParam.HashIndex) > 0)
                return true;
            else
                return false;
        }
        public void SetHasReturn(bool b)
        {
            if (b)
            {
                PKGType |= (byte)eHeadParam.HasReturn;
            }
            else
            {
                unchecked
                {
                    PKGType &= (byte)(~eHeadParam.HasReturn);
                }
            }
        }
        public bool IsHasReturn()
        {
            if ((PKGType & (byte)eHeadParam.HasReturn) > 0)
                return true;
            else
                return false;
        }

        public byte GetUserFlags()
        {//0-15
            return (byte)(PKGType & ((byte)eHeadParam.UserFlagMask));
        }
        public void SetUserFlags(byte flags)
        {//0-15
            var a = (byte)(~eHeadParam.UserFlagMask);
            PKGType &= (byte)a;
            PKGType |= (byte)(flags & ((byte)eHeadParam.UserFlagMask));
        }
    }
    public struct PkgReader : IO.Serializer.IReader
    {
        public static PkgReader NullReader = new PkgReader();
        public IO.Serializer.EIOType IOType
        {
            get
            {
                return IO.Serializer.EIOType.Network;
            }
        }
        private Support.NativeStreamReader mStreamReader;
        public PkgReader(IntPtr ptr, int size, Int64 time)
        {
            mStreamReader = new Support.NativeStreamReader(ptr, (UInt32)size);
            unsafe
            {
                PkgHeader header;
                mStreamReader.Read((byte*)&header, (UInt32)sizeof(PkgHeader));
            }
            RecvTime = time;
        }
        public PkgReader(byte[] ptr, int size, Int64 time)
        {
            unsafe
            {
                fixed (byte* p = &ptr[0])
                {
                    mStreamReader = new Support.NativeStreamReader((IntPtr)p, (UInt32)size);
                }
                PkgHeader header;
                mStreamReader.Read((byte*)&header, (UInt32)sizeof(PkgHeader));
            }

            RecvTime = time;
        }
        public void Dispose()
        {
            mStreamReader.Dispose();
        }
        public Int64 RecvTime
        {
            get;
            set;
        }

        public byte GetFlags()
        {
            unsafe
            {
                var header = (PkgHeader*)mStreamReader.Pointer;
                return header->PKGType;
            }
        }

        public bool IsHashIndex()
        {
            unsafe
            {
                var header = (PkgHeader*)mStreamReader.Pointer;
                return header->IsHashIndex();
            }
        }
        public bool IsHasReturn()
        {
            unsafe
            {
                var header = (PkgHeader*)mStreamReader.Pointer;
                return header->IsHasReturn();
            }
        }
        public byte GetUserFlags()
        {
            unsafe
            {
                var header = (PkgHeader*)mStreamReader.Pointer;
                return header->GetUserFlags();
            }
        }

        public IntPtr Ptr
        {
            get
            {
                return mStreamReader.Pointer;
            }
        }
        public int GetSize()
        {
            return (int)mStreamReader.Size;
        }
        public int GetPosition()
        {
            unsafe
            {
                return (int)mStreamReader.Position - sizeof(PkgHeader);
            }
        }

        public int DataPtr()
        {
            return PkgHeader.SizeOf();
        }
        public int CurPtr()
        {
            return PkgHeader.SizeOf() + GetPosition();
        }
        public void OnReadError()
        {
            throw new System.IndexOutOfRangeException("PkgReader Read Error");
        }
        
        public void Read(IO.Serializer.Serializer v)
        {
            v.ReadObject(this);
        }
        public T Read<T>() where T : IO.Serializer.Serializer, new()
        {
            T t = new T();
            Read(t);
            return t;
        }
        public List<T> ReadList<T>() where T : IO.Serializer.Serializer, new()
        {
            List<T> result = new List<T>();
            var sr = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(typeof(IO.Serializer.Serializer));
            sr.ReadValueList(result, this);
            return result;
        }

        public void ReadList<T>(List<T> v) where T : IO.Serializer.Serializer, new()
        {
            var sr = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(typeof(IO.Serializer.Serializer));
            sr.ReadValueList(v, this);
        }

        public void ReadPODObject(System.Type ValueType, out object Value)
        {
            var sr = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(ValueType);
            Value = sr.ReadValue(this);
        }
        #region Read POD
        public unsafe void ReadPtr(void* p, int length)
        {
            var offset = CurPtr();
            if (offset + length > GetSize())
            {
                OnReadError();
                return;
            }
            byte* tar = (byte*)p;

            mStreamReader.Read((byte*)p, (UInt32)length);
        }
        public void Read(IO.Serializer.ISerializer v)
        {

        }

        public void Read(out string v)
        {
            unsafe
            {
                unsafe
                {
                    UInt16 len = 0;
                    ReadPtr(&len, sizeof(UInt16));
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
        public void Read(out IO.Serializer.ChunkReader v)
        {
            v = new IO.Serializer.ChunkReader();
            unsafe
            {
                UInt16 len = 0;
                ReadPtr(&len, sizeof(UInt16));
                byte[] data = new byte[len];
                if (len > 0)
                {
                    fixed (byte* p = &data[0])
                    {
                        ReadPtr(p, len);
                    }
                }
                v.SetBuffer(data, 0);
            }
        }
        public void Read(out Support.BitSet v)
        {
            unsafe
            {
                int bitCount = 0;
                ReadPtr(&bitCount, sizeof(int));
                v = new Support.BitSet();
                int byteCount = 0;
                ReadPtr(&byteCount, sizeof(int));
                byte[] bitData = new byte[byteCount];
                fixed (byte* p = &bitData[0])
                {
                    ReadPtr(p, sizeof(System.Byte) * byteCount);
                }
                v.Init((UInt32)bitCount, bitData);
            }
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
        #endregion
    }
    public struct PkgWriter : IO.Serializer.IWriter
    {
        public IO.Serializer.EIOType IOType
        {
            get
            {
                return IO.Serializer.EIOType.Network;
            }
        }
        private Support.NativeStreamWriter mStreamWriter;
        public PkgWriter(int size)
        {
            Waiter = null;
            unsafe
            {
                mStreamWriter = new Support.NativeStreamWriter((UInt32)sizeof(PkgHeader) + (UInt32)size);
                PkgHeader header = new PkgHeader();
                header.ToDefault();
                WritePtr(&header, sizeof(PkgHeader));
            }
        }
        public void Dispose()
        {
            mStreamWriter.Dispose();
        }
        public object Waiter;//RemoteServices.RPCExecuter.RPCWait
        public IntPtr Ptr
        {
            get
            {
                return mStreamWriter.Pointer;
            }
        }

        public void SetFlags(byte flag)
        {
            unsafe
            {
                PkgHeader* header = (PkgHeader*)Ptr;
                header->PKGType = flag;
            }
        }

        public void SetHashIndex(bool b)
        {
            unsafe
            {
                PkgHeader* header = (PkgHeader*)Ptr;
                header->SetHashIndex(b);
            }
        }
        public void SetHasReturn(bool b)
        {
            unsafe
            {
                PkgHeader* header = (PkgHeader*)Ptr;
                header->SetHasReturn(b);
            }
        }
        public void SetUserFlags(byte flags)
        {//flags 0-15
            unsafe
            {
                PkgHeader* header = (PkgHeader*)Ptr;
                header->SetUserFlags(flags);
            }
        }

        public void Reset()
        {
            mStreamWriter = new Support.NativeStreamWriter(0);

            PkgHeader header = new PkgHeader();
            header.ToDefault();
            unsafe
            {
                WritePtr(&header, sizeof(PkgHeader));
            }
        }

        public int GetPosition()
        {
            unsafe
            {
                return (int)mStreamWriter.Size - sizeof(PkgHeader);
            }
        }
        public int CurPtr()
        {
            return PkgHeader.SizeOf() + GetPosition();
        }
        public void AppendPkg(PkgReader pkg, int offset)
        {
            //WriteByteArray(pkg.Ptr, pkg.DataPtr() + offset, pkg.GetSize() - offset); 
            unsafe
            {
                var src = (byte*)pkg.Ptr;

                WritePtr(src + pkg.DataPtr() + offset, pkg.GetSize() - offset);
            }
        }

        public void Write(IO.Serializer.Serializer v)
        {
            v.WriteObject(this);
        }
        public void WritePODObject(System.Object obj)
        {
            var tDesc = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(obj.GetType());
            if (tDesc == null)
                return;
            tDesc.WriteValue(obj, this);
        }

        public void Write<T>(List<T> v) where T : IO.Serializer.Serializer, new()
        {
            var sr = IO.Serializer.TypeDescGenerator.Instance.GetSerializer(typeof(IO.Serializer.Serializer));
            sr.WriteValueList(v, this);
        }
        #region Write POD
        public unsafe void WritePtr(void* p, int length)
        {
            if (length == 0)
                return;
        
            this.mStreamWriter.PushData((byte*)p, (UInt32)length);
        }
        private void WriteByteArray(byte[] v, int offset, int len)
        {
            if(offset+len>v.Length)
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

        public void Write(string v)
        {
            unsafe
            {
                var len = (UInt16)v.Length;
                WritePtr(&len, sizeof(UInt16));
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
        public void Write(IO.Serializer.ISerializer v)
        {
            v.WriteObject(this);
        }
        public void Write(IO.Serializer.ChunkWriter v)
        {
            this.Write(v.CurPtr());
            unsafe
            {
                fixed (byte* p = &v.Ptr[0])
                {
                    this.WritePtr(p, v.CurPtr());
                }
            }
        }
        public void Write(Support.BitSet v)
        {
            unsafe
            {
                var bitCount = v.BitCount;
                WritePtr(&bitCount, sizeof(int));
                byte[] bitData = v.Data;
                int byteCount = bitData.Length;
                WritePtr(&byteCount, sizeof(int));
                fixed (byte* p = &bitData[0])
                {
                    WritePtr(p, sizeof(System.Byte) * byteCount);
                }
            }
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
        #endregion

        public void SendBuffer(NetCore.NetConnection conn)
        {
            if (CurPtr() < 65535 - 2)
            {
                if (conn != null && conn.Connected == true)
                {
                    conn.SendBuffer(this.Ptr, CurPtr());
                }
            }
            else
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "RPC", "Error!!SendBuffer mPos>=65535-2");
            }
        }
    }
    
    public class RPCChunk
    {
        public IO.Serializer.ChunkReader Reader;
        public IO.Serializer.ChunkWriter Writer;

        public RPCChunk CloneObject()
        {
            RPCChunk cloned = new RPCChunk();
            if (Reader!=null)
            {
                cloned.Reader = new IO.Serializer.ChunkReader(Reader.Ptr());
            }
            if(Writer!=null)
            {
                cloned.Writer = new IO.Serializer.ChunkWriter(Writer.Ptr, Writer.Ptr.Length);
            }
            return cloned;
        }
    }
}
