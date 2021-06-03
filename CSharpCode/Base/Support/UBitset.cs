using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Support
{
    public class UBitset
    {
        byte[] mData;
        uint mBitCount;
        public uint BitCount
        {
            get => mBitCount;
        }
        public UBitset(uint Count)
        {
            mBitCount = Count;
            if (Count % 8 == 0)
                mData = new byte[Count / 8];
            else
                mData = new byte[Count / 8 + 1];
        }
        public void SetBit(uint index)
        {
            if (index >= mBitCount)
                return;
            uint a = index / 8;
            int b = (int)index % 8;
            mData[a] |= (byte)(1 << b);
        }
        public void UnsetBit(uint index)
        {
            if (index >= mBitCount)
                return;
            uint a = index / 8;
            int b = (int)index % 8;
            byte v =  ((byte)(1 << b));
            mData[a] &= (byte)(~v);
        }
        public bool IsSet(uint index)
        {
            if (index >= mBitCount)
                return false;
            uint a = index / 8;
            int b = (int)index % 8;
            return (mData[a] & (byte)(1 << b))!=0;
        }
        public void Clear()
        {
            for (int i = 0; i < mData.Length; i++)
            {
                mData[i] = 0;
            }
        }
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 4)]
    public struct UAnyValue : IDisposable
    {
        public enum EValueType : sbyte
        {
            Unknown,
            ManagedHandle,
            I8,
            I16,
            I32,
            I64,
            UI8,
            UI16,
            UI32,
            UI64,
            F32,
            F64,
            Struct,
            Ptr,
            V2,
            V3,
            V4,
        }
        public struct FStructDesc : IDisposable
        {
            public IntPtr mStructPointer;
            public int mStructSize;
            public VNameString mTypeName;
            public void SetStruct<T>(T v, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) where T : unmanaged
            {
                unsafe
                {
                    mStructSize = sizeof(T);
                    mStructPointer = (IntPtr)CoreSDK.Alloc((uint)mStructSize, sourceFilePath, sourceLineNumber);
                    var typeStr = Rtti.UTypeDesc.TypeStr(typeof(T));
                    mTypeName.SetString(typeStr);
                }
            }
            public void SetStruct(object v, System.Type type,
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
            {
                unsafe
                {
                    mStructSize = System.Runtime.InteropServices.Marshal.SizeOf(type);
                    mStructPointer = (IntPtr)CoreSDK.Alloc((uint)mStructSize, sourceFilePath, sourceLineNumber);
                    var typeStr = Rtti.UTypeDesc.TypeStr(type);
                    mTypeName.SetString(typeStr);
                }
            }
            public void Dispose()
            {
                if (mStructPointer != IntPtr.Zero)
                {
                    unsafe
                    {
                        CoreSDK.Free(mStructPointer.ToPointer());
                    }
                    mStructPointer = IntPtr.Zero;
                }
                mStructSize = 0;
            }
        }
        [FieldOffset(0)]
        private EValueType mValueType;
        public EValueType ValueType { get => mValueType; }
        [FieldOffset(1)]
        private byte mUnused;//3 bytes padding
        [FieldOffset(4)]
        private sbyte mI8Value;
        public sbyte I8Value { get => mI8Value; }
        [FieldOffset(4)]
        private short mI16Value;
        public short I16Value { get => mI16Value; }
        [FieldOffset(4)]
        private int mI32Value;
        public int I32Value { get => mI32Value; }
        [FieldOffset(4)]
        private long mI64Value;
        public long I64Value { get => mI64Value; }
        [FieldOffset(4)]
        private byte mUI8Value;
        public byte UI8Value { get => mUI8Value; }
        [FieldOffset(4)]
        private ushort mUI16Value;
        public ushort UI16Value { get => mUI16Value; }
        [FieldOffset(4)]
        private uint mUI32Value;
        public uint UI32Value { get => mUI32Value; }
        [FieldOffset(4)]
        private ulong mUI64Value;
        public ulong UI64Value { get => mUI64Value; }
        [FieldOffset(4)]
        private float mF32Value;
        public float F32Value { get => mF32Value; }
        [FieldOffset(4)]
        private double mF64Value;
        public double F64Value { get => mF64Value; }
        [FieldOffset(4)]
        private IntPtr mPointer;
        public IntPtr Pointer { get => mPointer; }
        [FieldOffset(4)]
        private FStructDesc mStruct;
        public FStructDesc Struct { get => mStruct; }
        [FieldOffset(4)]
        private IntPtr mGCHandle;
        public System.Runtime.InteropServices.GCHandle GCHandle 
        { 
            get
            {
                System.Diagnostics.Debug.Assert(mValueType == EValueType.ManagedHandle);
                return System.Runtime.InteropServices.GCHandle.FromIntPtr(mGCHandle);
            }
        }
        [FieldOffset(4)]
        private Vector2 mV2;
        public Vector2 V2 { get => mV2; }
        [FieldOffset(4)]
        private Vector3 mV3;
        public Vector3 V3 { get => mV3; }
        [FieldOffset(4)]
        private Quaternion mV4;
        public Quaternion V4 { get => mV4; }

        public void Dispose()
        {
            if (ValueType == EValueType.Struct)
            {
                mStruct.Dispose();
            }
            else if (ValueType == EValueType.ManagedHandle)
            {
                if (this.mGCHandle != IntPtr.Zero)
                {
                    this.GCHandle.Free();
                    this.mGCHandle = IntPtr.Zero;
                }
            }
            mValueType = EValueType.Unknown;
        }

        public void SetStruct<T>(T v,
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0) where T : unmanaged
        {
            Dispose();
            mValueType = EValueType.Struct;
            mStruct.SetStruct(v, sourceFilePath, sourceLineNumber);
        }
        public void SetStruct(object v,
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Dispose();
            mValueType = EValueType.Struct;
            mStruct.SetStruct(v, v.GetType(), sourceFilePath, sourceLineNumber);
        }
        public void SetPointer(IntPtr v)
        {
            Dispose();
            mValueType = EValueType.Ptr;
            mPointer = v;
        }
        public void SetObject(object obj)
        {
            Dispose();
            mValueType = EValueType.ManagedHandle;
            var gcHandle = System.Runtime.InteropServices.GCHandle.Alloc(obj);
            mGCHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gcHandle);
        }
        public void SetManagedHandle(System.Runtime.InteropServices.GCHandle gcHandle)
        {
            Dispose();
            mValueType = EValueType.ManagedHandle;
            mGCHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gcHandle);
        }
        public void SetManagedHandle(IntPtr gcHandle)
        {
            Dispose();
            mValueType = EValueType.ManagedHandle;
            mGCHandle = gcHandle;
        }
        public void SetI8(sbyte v)
        {
            Dispose();
            mValueType = EValueType.I8;
            mI8Value = v;
        }
        public void SetI16(short v)
        {
            Dispose();
            mValueType = EValueType.I16;
            mI16Value = v;
        }
        public void SetI32(int v)
        {
            Dispose();
            mValueType = EValueType.I32;
            mI32Value = v;
        }
        public void SetI64(long v)
        {
            Dispose();
            mValueType = EValueType.I64;
            mI64Value = v;
        }
        public void SetUI8(byte v)
        {
            Dispose();
            mValueType = EValueType.UI8;
            mUI8Value = v;
        }
        public void SetUI16(ushort v)
        {
            Dispose();
            mValueType = EValueType.UI16;
            mUI16Value = v;
        }
        public void SetUI32(uint v)
        {
            Dispose();
            mValueType = EValueType.UI32;
            mUI32Value = v;
        }
        public void SetUI64(ulong v)
        {
            Dispose();
            mValueType = EValueType.UI64;
            mUI64Value = v;
        }
        public void SetF32(float v)
        {
            Dispose();
            mValueType = EValueType.F32;
            mF32Value = v;
        }
        public void SetF64(double v)
        {
            Dispose();
            mValueType = EValueType.F64;
            mF64Value = v;
        }
        public void SetVector2(Vector2 v)
        {
            Dispose();
            mValueType = EValueType.V2;
            mV2 = v;
        }
        public void SetVector3(Vector3 v)
        {
            Dispose();
            mValueType = EValueType.V3;
            mV3 = v;
        }
        public void SetQuaternion(Quaternion v)
        {
            Dispose();
            mValueType = EValueType.V4;
            mV4 = v;
        }
    }
    public struct UAnyPointer : IDisposable
    {
        public UAnyValue Value;
        public object RefObject;
        public void Dispose()
        {
            Value.Dispose();
        }
    }
}
