using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public struct UNativeDictionary<K,V> : IDisposable, IEnumerable<KeyValuePair<K, V>> 
        where K : unmanaged
        where V : unmanaged
    {
        public CsDictionaryImpl mCoreObject;
        public static UNativeDictionary<K, V> CreateInstance()
        {
            var result = new UNativeDictionary<K, V>();
            unsafe
            {
                result.mCoreObject = CsDictionaryImpl.CreateInstance();
            }
            return result;
        }
        public static UNativeDictionary<K, V> CreateInstance(CsDictionaryImpl nativeObject)
        {
            var result = new UNativeDictionary<K, V>();
            result.mCoreObject = nativeObject;
            return result;
        }
        public void Dispose()
        {
            if (mCoreObject.NativePointer != IntPtr.Zero)
            {
                mCoreObject.NativeSuper.Release();
                mCoreObject.NativePointer = IntPtr.Zero;
            }
        }
        public bool Add(K key, V value)
        {
            TtAnyValue tKey = new TtAnyValue();
            TtAnyValue tValue = new TtAnyValue();
            tKey.SetValue<K>(key);
            tValue.SetValue<V>(value);
            return mCoreObject.Add(in tKey, in tValue);
        }
        public void Remove(K key)
        {
            TtAnyValue tKey = new TtAnyValue();
            tKey.SetValue<K>(key);
            mCoreObject.Remove(in tKey);
        }
        public void Clear()
        {
            mCoreObject.Clear();
        }
        public bool Find(K key, out V value)
        {
            value = default(V);
            TtAnyValue tKey = new TtAnyValue();
            tKey.SetValue<K>(key);
            TtAnyValue tValue = new TtAnyValue();
            var ret = mCoreObject.Find(in tKey, ref tValue);
            if (ret == false)
                return false;

            tValue.GetValue<V>(ref value);
            return true;
        }

        public unsafe struct UNativeDictionaryEnumerator : IEnumerator<KeyValuePair<K, V>>
        {
            UNativeDictionary<K, V> mContain;
            FCsDictionaryIterator* mIterator;
            bool IsFirst;
            public UNativeDictionaryEnumerator(UNativeDictionary<K,V> obj)
            {
                mContain = obj;
                mIterator = obj.mCoreObject.NewIterator();
                IsFirst = true;
            }
            public bool MoveNext()
            {
                if (mIterator == (FCsDictionaryIterator*)0)
                    return false;
                if (mIterator->IsEnd(mContain.mCoreObject))
                    return false;
                if (IsFirst)
                {
                    IsFirst = false;
                    return true;
                }
                IsFirst = false;
                mIterator->MoveNext();
                return true;
            }
            public void Reset()
            {
                mIterator->Reset(mContain.mCoreObject);
                IsFirst = true;
            }
            public KeyValuePair<K, V> Current
            {
                get
                {
                    TtAnyValue key = new TtAnyValue();
                    TtAnyValue value = new TtAnyValue();
                    mIterator->GetKeyValue(ref key, ref value);
                    K kk = default(K);
                    key.GetValue<K>(ref kk);
                    V vv = default(V);
                    key.GetValue<V>(ref vv);
                    return new KeyValuePair<K, V>(kk, vv);
                }
            }
            object System.Collections.IEnumerator.Current => throw new NotImplementedException();

            public void Dispose()
            {//其实不必要做
                if (mIterator != (FCsDictionaryIterator*)0)
                {
                    mIterator->Dispose();
                    mIterator = (FCsDictionaryIterator*)0;
                }
            }
        }
        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            var result = new UNativeDictionaryEnumerator(this);
            return result;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            var result = new UNativeDictionaryEnumerator(this);
            return result;
        }
    }
}
