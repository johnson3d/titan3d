using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EngineNS.Support
{
    //https://www.codeproject.com/Articles/28405/Make-the-debugger-show-the-contents-of-your-custom
    public class TtCollectionDebugView<T>
    {
        private ICollection<T> mCollection;

        public TtCollectionDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.mCollection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[this.mCollection.Count];
                this.mCollection.CopyTo(array, 0);
                return array;
            }
        }
    }
    
    [DebuggerTypeProxy(typeof(TtCollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public struct UNativeArray<T> : IDisposable, IList<T> where T : unmanaged
    {
        public CsValueList mCoreObject;
        public UNativeArray(CsValueList ptr)
        {
            mCoreObject = ptr;
        }
        public UNativeArray()
        {
            unsafe
            {
                mCoreObject = CsValueList.CreateInstance(sizeof(T));
                CoreSDK.SetMemDebugInfo(mCoreObject.CppPointer, typeof(T).FullName);
            }
        }

        public static UNativeArray<T> CreateInstance()
        {
            return new UNativeArray<T>();
        }
        public static UNativeArray<T> CreateInstance(CsValueList nativeObject)
        {
            UNativeArray<T> result = new UNativeArray<T>(nativeObject);
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
                Index++;
                if (Index >= Count)
                    return false;
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
        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }
        public int IndexOf(T item)
        {
            return IndexOf(in item);
        }
        public void Insert(int index, T item) 
        {
            unsafe
            {
                mCoreObject.InsertAt((uint)index, (byte*)&item);
            }
        }
        public Span<T> AsSpan()
        {
            unsafe
            {
                return new Span<T>(mCoreObject.GetAddressAt(0), Count);
            }
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            var src = AsSpan();
            var dst = array.AsSpan();

            for (int i = 0; i < Count && arrayIndex + i < array.Length; i++)
            {
                dst[arrayIndex + i] = src[i];
            }
        }
        public bool IsReadOnly 
        { 
            get { return false; } 
        }
        public bool Remove(T item) 
        {
            var pos = IndexOf(in item);
            if(pos == -1)
                return false;
            RemoveAt(pos);
            return true;
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
        public void Clear() 
        {
            Clear(false);
        }
        public void Clear(bool bFreeMemory = true)
        {
            mCoreObject.Clear(bFreeMemory ? 1 : 0);
        }
        public void SetSize(int Count)
        {
            mCoreObject.SetSize(Count);
        }
        public void SetCapacity(int Count)
        {
            mCoreObject.SetCapacity(Count);
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
        public bool Contains(T item) 
        { 
            return Contains(in item); 
        }
        public bool Contains(in T item)
        {
            var count = mCoreObject.GetCount();
            if (count == 0)
                return false;

            unsafe
            {
                var ptr = mCoreObject.GetAddressAt(0);
                if (((IntPtr)ptr) == IntPtr.Zero)
                    return false;

                for(int i= 0; i < count; i++)
                {
                    if (item.Equals(*(T*)(ptr + (i * sizeof(T)))))
                        return true;
                }
            }

            return false;
        }
        public int IndexOf(in T item)
        {
            var count = mCoreObject.GetCount();
            if (count == 0)
                return -1;

            unsafe
            {
                var ptr = mCoreObject.GetAddressAt(0);
                if (((IntPtr)ptr) == IntPtr.Zero)
                    return -1;

                for (int i = 0; i < count; i++)
                {
                    if (item.Equals(*(T*)(ptr + (i * sizeof(T)))))
                        return i;
                }
            }

            return -1;
        }
    }
}
