using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread.Async
{
    public enum EForEachResult
    {
        FER_Continue,
        FER_Stop,
        FER_Erase,
    };

    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public class TSafeDictionary<K, V>
    {
        public TSafeDictionary(IEqualityComparer<K> equalityComparer)
        {
            mObjects = new Dictionary<K, V>(equalityComparer);
        }
        public TSafeDictionary()
        {
            mObjects = new Dictionary<K, V>();
        }
        [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
        Dictionary<K, V> mObjects;
        protected Dictionary<K, V> Objects
        {
            get 
            {
                return mObjects; 
            }
        }
        List<KeyValuePair<K, V>> mAddList = new List<KeyValuePair<K, V>>();
        public List<KeyValuePair<K, V>> AddList
        {
            get { return mAddList; }
        }
        List<K> mRemoveList = new List<K>();
        public List<K> RemoveList
        {
            get { return mRemoveList; }
        }

        //bool ICollection<KeyValuePair<K, V>>.IsReadOnly
        //{
        //    get { return false; }
        //}

        public int Count
        {
            get
            {
                lock (this)
                {
                    var retValue = mAddList.Count + mObjects.Count - mRemoveList.Count;
                    return retValue < 0 ? 0 : retValue;
                }
            }
        }

        public ICollection<K> Keys
        {
            get
            {
                lock (this)
                {
                    if (Count <= 0)
                        return new List<K>().ToArray();
                    K[] ret = new K[Count];
                    int i = 0;
                    foreach (var key in Objects.Keys)
                    {
                        if (mRemoveList.Contains(key))
                            continue;

                        ret[i] = key;
                        i++;
                    }
                    foreach (var value in mAddList)
                    {
                        ret[i] = value.Key;
                        i++;
                    }
                    return ret;
                }
            }
        }

        public ICollection<V> Values
        {
            get
            {
                lock (this)
                {
                    if (Count <= 0)
                        return new List<V>().ToArray();
                    V[] ret = new V[Count];
                    int i = 0;
                    foreach (var value in Objects)
                    {
                        if (mRemoveList.Contains(value.Key))
                            continue;

                        ret[i] = value.Value;
                        i++;
                    }
                    foreach (var value in mAddList)
                    {
                        ret[i] = value.Value;
                        i++;
                    }
                    return ret;
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                mRemoveList.Clear();
                mAddList.Clear();
                mObjects.Clear();
            }
        }

        public V this[K key]
        {
            get { return FindObj(key); }
            set
            {
                lock (this)
                {
                    for (var i = 0; i < mRemoveList.Count; i++)
                    {
                        if (EqualityComparer<K>.Default.Equals(mRemoveList[i], key))
                        {
                            mRemoveList.RemoveAt(i);
                            i--;
                        }
                    }

                    bool added = false;
                    foreach (var i in mAddList)
                    {
                        if (EqualityComparer<K>.Default.Equals(i.Key, key))
                        {
                            mAddList.Remove(i);
                            mAddList.Add(new KeyValuePair<K, V>(key, value));
                            added = true;
                            break;
                        }
                    }

                    if (!added)
                        mAddList.Add(new KeyValuePair<K, V>(key, value));
                }
            }
        }

        public bool Add(K key, V value)
        {
            lock (this)
            {
                for (var i = 0; i < mRemoveList.Count; i++)
                {
                    if (EqualityComparer<K>.Default.Equals(mRemoveList[i], key))
                    {
                        mRemoveList.RemoveAt(i);
                        i--;
                    }
                }

                V fv;
                if (mObjects.TryGetValue(key, out fv))
                    return false;

                mAddList.Add(new KeyValuePair<K, V>(key, value));
                return true;
            }
        }

        public bool Remove(K key, out V value)
        {
            lock (this)
            {
                for (var i = 0; i < mAddList.Count; i++)
                {
                    if (EqualityComparer<K>.Default.Equals(mAddList[i].Key, key))
                    {
                        value = mAddList[i].Value;
                        mAddList.RemoveAt(i);
                        return true;
                    }
                }

                if (Objects.ContainsKey(key))
                {
                    value = Objects[key];
                    foreach (var item in mRemoveList)
                    {
                        if (EqualityComparer<K>.Default.Equals(item, key))
                            return true;
                    }
                    mRemoveList.Add(key);
                    return true;
                }
            }
            value = default(V);
            return false;
        }
        public bool Remove(K key)
        {
            lock (this)
            {
                for (var i = 0; i < mAddList.Count; i++)
                {
                    if (EqualityComparer<K>.Default.Equals(mAddList[i].Key, key))
                    {
                        mAddList.RemoveAt(i);
                        return true;
                    }
                }
                for (var i = 0; i < mRemoveList.Count; i++)
                {
                    if (EqualityComparer<K>.Default.Equals(mRemoveList[i], key))
                    {
                        return true;
                    }
                }

                if (Objects.ContainsKey(key))
                {
                    mRemoveList.Add(key);
                    return true;
                }
            }

            return false;
        }

        public V FindObj(K Key)
        {
            lock (this)
            {
                // 这里加BeforeTick可能会导致Foreach时mObjects被修改
                //BeforeTick();

                for (int i = 0; i < mAddList.Count; i++)
                {
                    if (EqualityComparer<K>.Default.Equals(mAddList[i].Key, Key))
                        return mAddList[i].Value;
                }

                if (mRemoveList.Contains(Key))
                    return default(V);

                V fv;
                if (mObjects.TryGetValue(Key, out fv))
                    return fv;
                return default(V);
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            lock (this)
            {
                for (int i = 0; i < mAddList.Count; i++)
                {
                    if (mObjects.Comparer != null)
                    {
                        if (mObjects.Comparer.Equals(mAddList[i].Key, key))
                        {
                            value = mAddList[i].Value;
                            return true;
                        }
                    }
                    else
                    {
                        if (EqualityComparer<K>.Default.Equals(mAddList[i].Key, key))
                        {
                            value = mAddList[i].Value;
                            return true;
                        }
                    }
                }

                V fv;
                if (mObjects.TryGetValue(key, out fv))
                {
                    value = fv;
                    return true;
                }
                value = default(V);
                return false;
            }
        }

        public bool ContainsKey(K key)
        {
            lock (this)
            {
                if (mObjects.Comparer != null)
                {
                    for (int i = 0; i < mRemoveList.Count; i++)
                    {
                        if (mObjects.Comparer.Equals(mRemoveList[i], key))
                            return false;
                    }
                }
                else
                {
                    for (int i = 0; i < mRemoveList.Count; i++)
                    {
                        if (EqualityComparer<K>.Default.Equals(mRemoveList[i], key))
                            return false;
                    }
                }
                if (mObjects.Comparer!=null)
                {
                    for (int i = 0; i < mAddList.Count; i++)
                    {
                        if (mObjects.Comparer.Equals(mAddList[i].Key, key))
                            return true;
                    }
                }
                else
                {
                    for(int i=0; i<mAddList.Count; i++)
                    {
                        if (EqualityComparer<K>.Default.Equals(mAddList[i].Key, key))
                            return true;
                    }
                }

                return mObjects.ContainsKey(key);
            }
        }

        public void BeforeTick()
        {
            _BeforeForEach();
        }
        private void _BeforeForEach()
        {
            lock (this)
            {
                if (mAddList.Count > 0)
                {
                    foreach (var i in mAddList)
                    {
                        mObjects[i.Key] = i.Value;
                    }
                    mAddList.Clear();
                }

                if (mRemoveList.Count > 0)
                {
                    foreach (var i in mRemoveList)
                    {
                        mObjects.Remove(i);
                    }
                    mRemoveList.Clear();
                }
            }
        }
        public void AfterTick()
        {
            _AfterForEach();
        }
        private void _AfterForEach()
        {
            lock (this)
            {
                if (mRemoveList.Count > 0)
                {
                    foreach (var i in mRemoveList)
                    {
                        mObjects.Remove(i);
                    }
                    mRemoveList.Clear();
                }
            }
        }

        public Dictionary<K, V>.Enumerator GetEnumerator()
        {
            BeforeTick();
            return this.Objects.GetEnumerator();
        }
    }
}
