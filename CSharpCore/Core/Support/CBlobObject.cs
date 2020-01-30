using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.IO.Serializer;

namespace EngineNS.Support
{
    public class CBlobObject : AuxCoreObject<CBlobObject.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CBlobObject()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("IBlobObject");
        }
        public CBlobObject(NativePointer ptr)
        {
            mCoreObject = ptr;
            Core_AddRef();
        }
        public UInt32 Size
        {
            get
            {
                return SDK_IBlobObject_GetSize(CoreObject);
            }
        }
        public IntPtr Data
        {
            get
            {
                return SDK_IBlobObject_GetData(CoreObject);
            }
        }
        public void PushData(IntPtr ptr, UInt32 size)
        {
            SDK_IBlobObject_PushData(CoreObject, ptr, size);
        }
        public void ReSize(UInt32 size)
        {
            SDK_IBlobObject_ReSize(CoreObject, size);
        }
        public byte[] ToBytes()
        {
            var ret = new byte[Size];
            unsafe
            {
                byte* src = (byte*)Data.ToPointer();
                for (int i=0;i<ret.Length;i++)
                {
                    ret[i] = src[i];
                }
            }
            return ret;
        }
        public uint[] ToUInts()
        {
            if (Size < 4)
                return null;
            var ret = new uint[Size/4];
            unsafe
            {
                uint* src = (uint*)Data.ToPointer();
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = src[i];
                }
            }
            return ret;
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IBlobObject_GetSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_IBlobObject_GetData(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IBlobObject_PushData(NativePointer self, IntPtr ptr, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IBlobObject_ReSize(NativePointer self, UInt32 size);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IBlobObject_ReadFromXnd(NativePointer self, IO.XndAttrib.NativePointer attrib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IBlobObject_Write2Xnd(NativePointer self, IO.XndAttrib.NativePointer attrib);
        #endregion
    }

    public struct CBlobProxy2 : IDisposable, IReader
    {
        public CBlobObject.NativePointer CoreObject;
        public static CBlobProxy2 CreateBlobProxy()
        {
            var result = new CBlobProxy2();
            //result.CoreObject.Pointer = CoreObjectBase.SDK_CoreRttiManager_NewObjectByName("Titan3D::IBlobObject");
            result.CoreObject = CoreObjectBase.NewNativeObjectByNativeName<CBlobObject.NativePointer>("IBlobObject");
            return result;
        }
        public void Dispose()
        {
            if(CoreObject.Pointer!=IntPtr.Zero)
            {
                CoreObjectBase.SDK_VIUnknown_Release(CoreObject.Pointer);
                CoreObject.Pointer = IntPtr.Zero;
            }
        }
        public void BeginRead()
        {
            DataPtr = CBlobObject.SDK_IBlobObject_GetData(CoreObject);
            DataLength = CBlobObject.SDK_IBlobObject_GetSize(CoreObject);
            Current = 0;
        }
        IntPtr DataPtr;
        public UInt32 DataLength;
        int Current;

        public unsafe void ReadPtr(void* p, int length)
        {
            if (Current + length > DataLength)
            {
                OnReadError();
                return;
            }
            byte* ptr = (byte*)(DataPtr + Current);
            for (int i = 0; i < length; i++)
            {
                ((byte*)p)[i] = ptr[i];
            }

            Current += length;
        }

        #region IReader
        public EIOType IOType
        {
            get { return EIOType.Normal; }
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
        public void Read(out byte[] v, int len)
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
        public void Read(out ChunkReader v)
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
        public void Read(ISerializer v)
        {
            v.ReadObject(this);
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

        public void ReadVector3s(out Vector3[] v, int size)
        {
            unsafe
            {
                v = new Vector3[size];
                for (int i = 0; i < size; i++)
                {
                    Read(out v[i]);
                }
            }
        }
    }

    public class CBlobProxy : IO.Serializer.AuxIReader
    {
        IntPtr DataPtr;
        UInt32 DataLength;
        int Current;
        public CBlobProxy(CBlobObject blob)
        {
            DataPtr = blob.Data;
            DataLength = blob.Size;
            Current = 0;
        }
        public override EIOType IOType
        {
            get { return EIOType.File; }
        }
        
        public override void Read(ISerializer v)
        {
            throw new NotImplementedException();
        }
        public void ReadFloats(out float[] v)
        {
            int size = (int)DataLength / sizeof(float);
            v = new float[size];
            for (int i = 0; i < size; i++)
            {
                Read(out v[i]);
            }
        }
        public void ReadVector3s(out Vector3[] v)
        {
            unsafe
            {
                int size = (int)DataLength / sizeof(Vector3);
                v = new Vector3[size];
                for (int i = 0; i < size; i ++)
                {
                    Read(out v[i]);
                }
            }
        }
        
        public unsafe override void ReadPtr(void* p, int length)
        {
            if(Current + length>DataLength)
            {
                OnReadError();
                return;
            }
            byte* ptr = (byte*)(DataPtr + Current);
            for(int i=0;i<length;i++)
            {
                ((byte*)p)[i] = ptr[i];
            }

            Current += length;
        }
    }
}
