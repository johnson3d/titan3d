using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.IO
{
    public interface ICoreWriter
    {
        EIOType IOType { get; }
        ulong GetPosition();
        void Seek(ulong pos);
        unsafe void WritePtr(void* p, int length);
        unsafe void* Ptr { get; }
    }

    public interface IWriter : ICoreWriter
    {
        void Write(ISerializer v);
        void Write(string v);
        void Write(byte[] v);
        void Write(VNameString v);
        void Write(RName v);
        void Write(Support.TtBitset v);
        void Write(Rtti.TtTypeDesc v); 
        void Write<T>(T v) where T : unmanaged;
        void Write<T>(T v, bool dummy = true) where T : struct;

        void WriteWithType(Type type, object value);
    }

    public partial struct TtMemWriter : IO.ICoreWriter, IDisposable
    {
        private unsafe static CoreSDK.FDelegate_FSaveMemStream NativeSaveMemStream = NativeSaveMemStreamCB;
        private unsafe static void NativeSaveMemStreamCB(EngineNS.MemStreamWriter arg0, sbyte* arg1, sbyte* arg2)
        {
            var name = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg1);
            var type = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)arg2);

            if (type == "DX12GpuDrawState")
            {
                var path = TtEngine.Instance.FileManager.GetPath(TtFileManager.ERootDir.Cache, TtFileManager.ESystemDir.PSO) + "dx12/";
                using (var xnd = new TtXndHolder("Dx12PSO", 0, 0))
                {
                    var attr = xnd.RootNode.GetOrAddAttribute("Blob", 0, 0);
                    using (var ar = attr.GetWriter((uint)arg0.GetLength()))
                    {
                        ar.WritePtr(arg0.GetPointer(), (int)arg0.GetLength());
                    }
                    xnd.SaveXnd(path + name + ".pso");
                }   
            }
        }
        public static void InitNativeCallback()
        {
            CoreSDK.SetSaveMemStreamCallback(NativeSaveMemStream);
        }
        public static void FinalNativeCallback()
        {
            CoreSDK.SetSaveMemStreamCallback(null);
        }
        public static TtMemWriter CreateInstance()
        {
            TtMemWriter result = new TtMemWriter();
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
        public void Write<T>(T v, bool dummy = true) where T : struct
        {
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.TtTypeDescGetter<T>.TypeDesc.TypeString);
            if (meta != null)
            {
                Write(meta.TypeHash);
            }
            else
            {
                Write(Hash64.Empty);
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
        public unsafe void Write(byte[] v)
        {
            var len = (UInt16)v.Length;
            WritePtr(&len, sizeof(UInt16));
            if (len>0)
            {
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
            Write(RName.CurrentVersion);
            if (v== null)
            {
                Write(RName.ERNameType.Unkown);
                return;
            }
            else
            {
                Write(v.RNameType);
            }
            Write(v.Name);
            Write(v.AssetId);
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
        public unsafe void Write(TtMemWriter v)
        {
            Write((uint)v.GetPosition());
            WritePtr(v.Ptr, (int)v.GetPosition());
        }
        public void Write(Rtti.TtTypeDesc v)
        {
            if(v == null)
                this.Write("");
            else
                this.Write(v.TypeString);
        }

        public void WriteWithType(Type type, object value)
        {
            if(type == typeof(byte))
            {
                Write((byte)value);
            }
            else if(type == typeof(short))
            {
                Write((short)value);
            }
            else if(type == typeof(ushort))
            {
                Write((ushort)value);
            }
            else if(type == typeof(int))
            {
                Write((int)value);
            }
            else if(type == typeof(uint))
            {
                Write((uint)value);
            }
            else if(type == typeof(long))
            {
                Write((long)value);
            }
            else if(type == typeof(ulong))
            {
                Write((ulong)value);
            }
            else if(type == typeof(float))
            {
                Write((float)value);
            }
            else if(type == typeof(double))
            {
                Write((double)value);
            }
            else if(type == typeof(bool))
            {
                Write((bool)value);
            }
            else if(type == typeof(string))
            {
                Write((string)value);
            }
            else if (type == typeof(VNameString))
            {
                Write((RName)value);
            }
            else if (type == typeof(Support.TtBitset))
            {
                Write((Support.TtBitset)value);
            }
            else if (type == typeof(byte[]))
            {
                Write((byte[])value);
            }
            else if (type == typeof(RName))
            {
                Write((RName)value);
            }
            else if(typeof(ISerializer).IsAssignableFrom(type))
            {
                Write((ISerializer)value);
            }
            else
            {
                throw new InvalidOperationException($"Unsupport type serialize {type.FullName}");
            }
        }
    }
}
