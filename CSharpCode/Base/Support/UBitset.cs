using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Support
{
    public class UBitset : AuxPtrType<FBitset>
    {
        public UBitset()
        {
            mCoreObject = FBitset.CreateInstance();
        }
        public uint BitCount
        {
            get => mCoreObject.GetBitCount();
        }
        public unsafe byte* Data
        {
            get
            {
                return mCoreObject.GetDataPtr();
            }
        }
        public uint DataByteSize
        {
            get
            {
                return mCoreObject.GetDataByteSize();
            }
        }
        public void SetBitCount(uint Count)
        {
            mCoreObject.SetBitCount(Count);
        }
        public UBitset(uint Count)
        {
            mCoreObject = FBitset.CreateInstance();
            SetBitCount(Count);
        }
        public void SetBit(uint index)
        {
            mCoreObject.SetBit(index);
        }
        public void UnsetBit(uint index)
        {
            mCoreObject.UnsetBit(index);
        }
        public bool IsSet(uint index)
        {
            return mCoreObject.IsSet(index);
        }
        public void Clear()
        {
            mCoreObject.Clear();
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
            Name,
            Struct,
            Ptr,
            V2,
            V3,
            V4,
            Bool,
        }
        public unsafe void SetValue<T>(T v) where T : unmanaged
        {
            if (typeof(T) == typeof(float))
            {
                SetF32(*(float*)&v);
            }
            else if (typeof(T) == typeof(double))
            {
                SetF64(*(double*)&v);
            }
            else if (typeof(T) == typeof(sbyte))
            {
                SetI8(*(sbyte*)&v);
            }
            else if (typeof(T) == typeof(short))
            {
                SetI16(*(short*)&v);
            }
            else if (typeof(T) == typeof(int))
            {
                SetI32(*(int*)&v);
            }
            else if (typeof(T) == typeof(long))
            {
                SetI64(*(long*)&v);
            }
            else if (typeof(T) == typeof(byte))
            {
                SetUI8(*(byte*)&v);
            }
            else if (typeof(T) == typeof(ushort))
            {
                SetUI16(*(ushort*)&v);
            }
            else if (typeof(T) == typeof(uint))
            {
                SetUI32(*(uint*)&v);
            }
            else if (typeof(T) == typeof(ulong))
            {
                SetUI64(*(ulong*)&v);
            }
            else if (typeof(T) == typeof(VNameString))
            {
                SetName(*(VNameString*)&v);
            }
            else if (typeof(T) == typeof(Vector2))
            {
                SetVector2(*(Vector2*)&v);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                SetVector3(*(Vector3*)&v);
            }
            else if(typeof(T) == typeof(Vector4))
            {
                SetVector4(*(Vector4*)&v);
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                SetQuaternion(*(Quaternion*)&v);
            }
            else if (typeof(T) == typeof(IntPtr))
            {
                SetPointer(*(IntPtr*)&v);
            }
            else if(typeof(T) == typeof(bool))
            {
                SetBool(*(bool*)&v);
            }
            else
            {
                SetStruct<T>(v);
            }
        }
        private unsafe void CopyData(void* tar, int size)
        {
            fixed (sbyte* p = &mI8Value)
            {
                CoreSDK.MemoryCopy(tar, (void*)p, (uint)size);
            }
        }
        public unsafe bool IsEqual(in UAnyValue val)
        {
            if (ValueType != val.ValueType)
                return false;
            switch(ValueType)
            {
                case EValueType.ManagedHandle:
                    return GCHandle.Target == val.GCHandle.Target;
                case EValueType.I8:
                    return mI8Value == val.mI8Value;
                case EValueType.I16:
                    return mI16Value == val.mI16Value;
                case EValueType.I32:
                    return mI32Value == val.mI32Value;
                case EValueType.I64:
                    return mI64Value == val.mI64Value;
                case EValueType.UI8:
                    return mUI8Value == val.mUI8Value;
                case EValueType.UI16:
                    return mUI16Value == val.mUI16Value;
                case EValueType.UI32:
                    return mUI32Value == val.mUI32Value;
                case EValueType.UI64:
                    return mUI64Value == val.mUI64Value;
                case EValueType.F32:
                    return mF32Value == val.mF32Value;
                case EValueType.F64:
                    return mF64Value == val.mF64Value;
                case EValueType.Name:
                    return mNameString.Text == val.mNameString.Text;
                case EValueType.Struct:
                    return (mStruct.mStructPointer == val.mStruct.mStructPointer) &&
                           (mStruct.mStructSize == val.mStruct.mStructSize) &&
                           (mStruct.mTypeName.Text == val.mStruct.mTypeName.Text);
                case EValueType.Ptr:
                    return mPointer == val.mPointer;
                case EValueType.V2:
                    return mV2 == val.mV2;
                case EValueType.V3:
                    return mV3 == val.mV3;
                case EValueType.V4:
                    return mV4 == val.mV4;
                case EValueType.Bool:
                    return mBoolValue == val.mBoolValue;
            }
            return false;
        }
        public unsafe IntPtr GetStructPointer()
        {
            if (ValueType != EValueType.Struct)
                return IntPtr.Zero;
            return mStruct.mStructPointer;
        }
        public unsafe void GetValue<T>(ref T v) where T : unmanaged
        {
            if (typeof(T) == typeof(float))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(double))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(sbyte))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(short))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(int))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(long))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(byte))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(ushort))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(ulong))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(VNameString))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(Vector2))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(Vector3))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if (typeof(T) == typeof(IntPtr))
            {
                fixed (T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else if(typeof(T) == typeof(bool))
            {
                fixed(T* p = &v)
                {
                    CopyData(p, sizeof(T));
                }
            }
            else
            {
                fixed (T* p = &v)
                {
                    CoreSDK.MemoryCopy(p, mStruct.mStructPointer.ToPointer(), (uint)sizeof(T));
                }
            }
        }
        public object ToObject()
        {
            switch (mValueType)
            {
                case EValueType.Unknown:
                    return null;
                case EValueType.ManagedHandle:
                    return this.GCHandle.Target;
                case EValueType.I8:
                    return mI8Value;
                case EValueType.I16:
                    return mI16Value;
                case EValueType.I32:
                    return mI32Value;
                case EValueType.I64:
                    return mI64Value;
                case EValueType.UI8:
                    return mUI8Value;
                case EValueType.UI16:
                    return mUI16Value;
                case EValueType.UI32:
                    return mUI32Value;
                case EValueType.UI64:
                    return mUI64Value;
                case EValueType.F32:
                    return mF32Value;
                case EValueType.F64:
                    return mF64Value;
                case EValueType.Name:
                    return mNameString.c_str();
                case EValueType.Struct:
                    return mStruct.ToObject();
                case EValueType.Ptr:
                    return mPointer;
                case EValueType.V2:
                    return mV2;
                case EValueType.V3:
                    return mV3;
                case EValueType.V4:
                    return mV4;
                case EValueType.Bool:
                    return mBoolValue;
                default:
                    return null;
            }
        }
        public override string ToString()
        {
            switch(mValueType)
            {
                case EValueType.Unknown:
                    return "null";
                case EValueType.ManagedHandle:
                    return "null";
                case EValueType.I8:
                    return mI8Value.ToString();
                case EValueType.I16:
                    return mI16Value.ToString();
                case EValueType.I32:
                    return mI32Value.ToString();
                case EValueType.I64:
                    return mI64Value.ToString();
                case EValueType.UI8:
                    return mUI8Value.ToString();
                case EValueType.UI16:
                    return mUI16Value.ToString();
                case EValueType.UI32:
                    return mUI32Value.ToString();
                case EValueType.UI64:
                    return mUI64Value.ToString();
                case EValueType.F32:
                    return mF32Value.ToString();
                case EValueType.F64:
                    return mF64Value.ToString();
                case EValueType.Name:
                    return mNameString.c_str();
                case EValueType.Struct:
                    return mStruct.ToString();
                case EValueType.Ptr:
                    return "null";
                case EValueType.V2:
                    return mV2.ToString();
                case EValueType.V3:
                    return mV3.ToString();
                case EValueType.V4:
                    return mV4.ToString();
                case EValueType.Bool:
                    return mBoolValue.ToString();
                default:
                    return null;
            }
        }
        public struct FStructDesc : IDisposable
        {
            public IntPtr mStructPointer;
            public int mStructSize;
            public VNameString mTypeName;
            public object ToObject()
            {
                unsafe
                {
                    var type = Rtti.UTypeDesc.TypeOf(mTypeName.c_str());
                    var result = Rtti.UTypeDescManager.CreateInstance(type);
                    System.Runtime.InteropServices.Marshal.PtrToStructure(mStructPointer, result);
                    return result;
                }
            }
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
            public void SetStruct2<T>(T v, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
            {
                unsafe
                {
                    mStructSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)); 
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
        private VNameString mNameString;
        public VNameString NameString
        { 
            get
            {
                return NameString;
            }
        }
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
        [FieldOffset(4)]
        private bool mBoolValue;
        public bool BoolValue { get => mBoolValue; }

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
        public void SetName(string v)
        {
            Dispose();
            mValueType = EValueType.Name;
            mNameString = VNameString.FromString(v);
        }
        public void SetName(VNameString v)
        {
            Dispose();
            mValueType = EValueType.Name;
            mNameString = v;
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
        public void SetVector4(Vector4 v)
        {
            Dispose();
            mValueType = EValueType.V4;
            mV4.X = v.X;
            mV4.Y = v.Y;
            mV4.Z = v.Z;
            mV4.W = v.W;
        }
        public void SetQuaternion(Quaternion v)
        {
            Dispose();
            mValueType = EValueType.V4;
            mV4 = v;
        }
        public void SetBool(bool v)
        {
            Dispose();
            mValueType = EValueType.Bool;
            mBoolValue = v;
        }
    }
    public struct UAnyPointer : IDisposable
    {
        public UAnyValue Value;
        public object RefObject;

        public static UAnyPointer Default = new UAnyPointer();

        public UAnyPointer(int v)
        {
            SetValue(v);
        }
        public void SetValue<T>(T v) where T : unmanaged
        {
            Value.SetValue<T>(v);
        }
        public void SetValue(object v)
        {
            RefObject = v;
        }
        public object ToObject()
        {
            if (Value.ValueType == UAnyValue.EValueType.Unknown)
                return RefObject;
            return Value.ToObject();
        }
        public void Dispose()
        {
            Value.Dispose();
        }
    }
}
