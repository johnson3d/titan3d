using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public struct UNativeArray<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public CsValueList mCoreObject;
        public static UNativeArray<T> CreateInstance()
        {
            UNativeArray<T> result = new UNativeArray<T>();
            unsafe
            {
                result.mCoreObject = CsValueList.CreateInstance(sizeof(T));
            }
            return result;
        }
        public static UNativeArray<T> CreateInstance(CsValueList nativeObject)
        {
            UNativeArray<T> result = new UNativeArray<T>();
            result.mCoreObject = nativeObject;
            return result;
        }
        public struct UNativeArrayEnumerator : IEnumerator<T>
        {
            int Index;
            int Count;
            unsafe T* ArrayAddress;
            public UNativeArrayEnumerator(UNativeArray<T> obj)
            {
                Index = -1;
                Count = obj.Count;
                unsafe
                {
                    ArrayAddress = obj.UnsafeGetElementAddress(0);
                }
            }
            public bool MoveNext()
            {
                if (Index >= Count)
                    return false;
                Index++;
                return true;
            }
            public void Reset()
            {
                Index = -1;
            }
            public T Current
            { 
                get
                {
                    unsafe
                    {
                        return ArrayAddress[Index];
                    }
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    unsafe
                    {
                        return ArrayAddress[Index];
                    }
                }
            }
            public void Dispose()
            {//其实不必要做
                //Index = -1;
                //Count = 0;
                //unsafe
                //{
                //    ArrayAddress = (T*)0;
                //}
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var result = new UNativeArrayEnumerator(this);            
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var result = new UNativeArrayEnumerator(this);
            return result;
        }
        public int Count
        {
            get
            {
                return (int)mCoreObject.GetCount();
            }
        }
        public void Dispose()
        {
            if (mCoreObject.NativePointer != IntPtr.Zero)
            {
                mCoreObject.NativeSuper.Release();
                mCoreObject.NativePointer = IntPtr.Zero;
            }
        }
        public void Add(T item)
        {
            unsafe
            {
                mCoreObject.AddValue((byte*)(&item));
            }
        }
        public void Append(UNativeArray<T> src)
        {
            unsafe
            {
                mCoreObject.Append(src.mCoreObject);
            }
        }
        public unsafe void Append(T* scr, int count)
        {
            unsafe
            {
                mCoreObject.AppendArray((byte*)scr, count);
            }
        }
        public void RemoveAt(int index)
        {
            mCoreObject.RemoveAt((uint)index);
        }
        public void Clear(bool bFreeMemory = true)
        {
            mCoreObject.Clear(bFreeMemory ? 1 : 0);
        }
        public IntPtr UnsafeAddressAt(int index)
        {
            unsafe
            {
                return (IntPtr)mCoreObject.GetAddressAt((uint)index);
            }
        }
        public unsafe T* UnsafeGetElementAddress(int index)
        {
            return (T*)mCoreObject.GetAddressAt((uint)index);
        }
        public void UnsafeSetDatas(IntPtr ptr, int count)
        {
            unsafe
            {
                mCoreObject.SetDatas((byte*)ptr.ToPointer(), count);
            }
        }
        public T this[int index]
        {
            get
            {
                T result = default(T);
                unsafe
                {
                    var ptr = (IntPtr)mCoreObject.GetAddressAt((uint)index);
                    if (ptr == IntPtr.Zero)
                        return result;
                    CoreSDK.MemoryCopy(&result, ptr.ToPointer(), (UInt32)sizeof(T));
                }
                return result;
            }
            set
            {
                unsafe
                {
                    var ptr = (IntPtr)mCoreObject.GetAddressAt((uint)index);
                    if (ptr == IntPtr.Zero)
                        return;
                    CoreSDK.MemoryCopy(ptr.ToPointer(), &value, (UInt32)sizeof(T));
                }
            }
        }
    }
}
