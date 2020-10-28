using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Support
{
    internal struct NativeListImpl : IDisposable
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
        NativePointer CoreObject;
        public static NativeListImpl CreateNativeList(UInt32 stride, int capacity)
        {
            NativeListImpl result;
            result.CoreObject = CoreObjectBase.NewNativeObjectByNativeName<NativePointer>("CsValueList");
            SDK_CsValueList_SetStride(result.CoreObject, stride);
            SDK_CsValueList_SetCapacity(result.CoreObject, capacity);
            return result;
        }
        public void Dispose()
        {
            if (CoreObject.Pointer != IntPtr.Zero)
            {
                CoreObjectBase.SDK_VIUnknown_Release(CoreObject.Pointer);
                CoreObject.Pointer = IntPtr.Zero;
            }
        }
        public void SetCapacity(int cap)
        {
            SDK_CsValueList_SetCapacity(CoreObject, cap);
        }

        public UInt32 Stride
        {
            get
            {
                return SDK_CsValueList_GetStride(CoreObject);
            }
        }

        public int Count
        {
            get
            {
                return (int)SDK_CsValueList_GetCount(CoreObject);
            }
        }
        public void Add(IntPtr ptr)
        {
            SDK_CsValueList_AddValue(CoreObject, ptr);
        }

        public void Append(NativeListImpl scr)
        {
            SDK_CsValueList_Append(CoreObject, scr.CoreObject);
        }
        public unsafe void Append(byte* src, int count)
        {
            SDK_CsValueList_AppendArray(CoreObject, src, count);
        }
        public void RemoveAt(int index)
        {
            SDK_CsValueList_RemoveAt(CoreObject, (UInt32)index);
        }
        public void Clear(bool bFreeMemory)
        {
            SDK_CsValueList_Clear(CoreObject, vBOOL.FromBoolean(bFreeMemory));
        }
        public IntPtr AddressAt(int index)
        {
            return SDK_CsValueList_GetAddressAt(CoreObject, (UInt32)index);
        }
        public void UnsafeSetDatas(IntPtr ptr, int count)
        {
            SDK_CsValueList_SetDatas(CoreObject, ptr, count);
        }
        #region SDK
        public const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsValueList_GetStride(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_SetStride(NativePointer self, UInt32 stride);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_SetCapacity(NativePointer self, int capacity); 
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_CsValueList_GetCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_AddValue(NativePointer self, IntPtr stride);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_Append(NativePointer self, NativePointer src);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static void SDK_CsValueList_AppendArray(NativePointer self, byte* src, int count);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_RemoveAt(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_CsValueList_Clear(NativePointer self, vBOOL bFreeMemory);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_CsValueList_GetAddressAt(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_CsValueList_SetDatas(NativePointer self, IntPtr ptr, int countOfObj);
        #endregion
    }

    public class NativeList<T> where T : unmanaged
    {
        private NativeListImpl mImpl;
        private static int mStructSize;
        static NativeList()
        {
            unsafe
            {
                mStructSize = sizeof(T);
            }
        }
        public NativeList(int capacity = -1)
        {
            mImpl = NativeListImpl.CreateNativeList((UInt32)mStructSize, capacity);
        }
        ~NativeList()
        {
            mImpl.Dispose();
        }
        public void SetCapacity(int cap)
        {
            mImpl.SetCapacity(cap);
        }
        public int Count
        {
            get { return mImpl.Count; }
        }
        public int Stride
        {
            get { return mStructSize; }
        }
        public void Add(T item)
        {
            lock (this)
            {
                unsafe
                {
                    mImpl.Add((IntPtr)(&item));
                }
            }
        }
        public void Append(NativeList<T> scr)
        {
            lock (this)
            {
                unsafe
                {
                    mImpl.Append(scr.mImpl);
                }
            }
        }
        public unsafe void Append(T* scr, int count)
        {
            lock (this)
            {
                unsafe
                {
                    mImpl.Append((byte*)scr, count);
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (this)
            {
                mImpl.RemoveAt(index);
            }
        }
        public void Clear(bool bFreeMemory = true)
        {
            lock (this)
            {
                mImpl.Clear(bFreeMemory);
            }
        }

        public IntPtr UnsafeAddressAt(int index)
        {
            return mImpl.AddressAt(index);
        }

        public unsafe T* UnsafeGetElementAddress(int index)
        {
            return (T*)mImpl.AddressAt(index);
        }
        public IntPtr GetBufferPtr()
        {
            return mImpl.AddressAt(0);
        }
        public void UnsafeSetDatas(IntPtr ptr, int count)
        {
            mImpl.UnsafeSetDatas(ptr, count);
        }
        public T this[int index]
        {
            get
            {
                T result = default(T);
                var ptr = mImpl.AddressAt(index);
                if (ptr == IntPtr.Zero)
                    return result;
                unsafe
                {
                    CoreSDK.SDK_Memory_Copy(&result, ptr.ToPointer(), (UInt32)mStructSize);
                }
                return result;
            }
            set
            {
                var ptr = mImpl.AddressAt(index);
                if (ptr == IntPtr.Zero)
                    return;
                unsafe
                {
                    lock (this)
                    {
                        CoreSDK.SDK_Memory_Copy(ptr.ToPointer(), &value, (UInt32)mStructSize);
                    }
                }
            }
        }
    }

    public struct NativeListProxy<T> : IDisposable where T : unmanaged
    {
        private NativeListImpl mImpl;
        private static int mStructSize;
        static NativeListProxy()
        {
            unsafe
            {
                mStructSize = sizeof(T);
            }
        }
        public static NativeListProxy<T> CreateNativeList(int capacity = -1)
        {
            var result = new NativeListProxy<T>();
            result.mImpl = NativeListImpl.CreateNativeList((UInt32)mStructSize, capacity);
            return result;
        }
        public void Dispose()
        {
            mImpl.Dispose();
        }
        public void SetCapacity(int cap)
        {
            mImpl.SetCapacity(cap);
        }
        public int Count
        {
            get { return mImpl.Count; }
        }
        public int Stride
        {
            get { return mStructSize; }
        }
        public void Add(T item)
        {
            unsafe
            {
                mImpl.Add((IntPtr)(&item));
            }
        }
        public void Append(NativeListProxy<T> scr)
        {
            unsafe
            {
                mImpl.Append(scr.mImpl);
            }
        }
        public unsafe void Append(T* scr, int count)
        {
            unsafe
            {
                mImpl.Append((byte*)scr, count);
            }
        }

        public void RemoveAt(int index)
        {
            mImpl.RemoveAt(index);
        }
        public void Clear(bool bFreeMemory = true)
        {
            mImpl.Clear(bFreeMemory);
        }

        public IntPtr UnsafeAddressAt(int index)
        {
            return mImpl.AddressAt(index);
        }
        public unsafe T* UnsafeGetElementAddress(int index)
        {
            return (T*)mImpl.AddressAt(index);
        }
        public IntPtr GetBufferPtr()
        {
            return mImpl.AddressAt(0);
        }
        public void UnsafeSetDatas(IntPtr ptr, int count)
        {
            mImpl.UnsafeSetDatas(ptr, count);
        }
        public T this[int index]
        {
            get
            {
                T result = default(T);
                var ptr = mImpl.AddressAt(index);
                if (ptr == IntPtr.Zero)
                    return result;
                unsafe
                {
                    CoreSDK.SDK_Memory_Copy(&result, ptr.ToPointer(), (UInt32)mStructSize);
                }
                return result;
            }
            set
            {
                var ptr = mImpl.AddressAt(index);
                if (ptr == IntPtr.Zero)
                    return;
                unsafe
                {
                    CoreSDK.SDK_Memory_Copy(ptr.ToPointer(), &value, (UInt32)mStructSize);
                }
            }
        }
    }

    public class NativeQueueForArray<T> where T : unmanaged
    {//用List模拟Queue接口，保证内存连续，没有用循环队列实现，所以dequeue性能不好
        private NativeList<T> mList = new NativeList<T>();
        public int Count
        {
            get { return mList.Count; }
        }
        public void Clear()
        {
            mList.Clear();
        }
        public void Dequeue()
        {
            mList.RemoveAt(0);
        }
        public void Enqueue(T item)
        {
            mList.Add(item);
        }
        public T Peek()
        {
            return mList[0];
        }
        public IntPtr UnsafePeekPtr()
        {
            return mList.UnsafeAddressAt(0);
        }
        public IntPtr GetBufferPtr()
        {
            return mList.GetBufferPtr();
        }
        public T this[int index]
        {
            get
            {
                return mList[index];
            }
            set
            {
                mList[index] = value;
            }
        }
    }
}
