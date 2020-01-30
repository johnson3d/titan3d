using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace EditorCommon
{
    public class PropertyMultiValue : EngineNS.Editor.IEditorInstanceObject
    {
        public List<object> Values
        {
            get;
            protected set;
        } = new List<object>();

        public void FinalCleanup()
        {
        }

        public override string ToString()
        {
            if(HasDifferentValue())
                return "---";
            return System.Convert.ToString(Values[0]);
        }

        public bool HasDifferentValue()
        {
            if (Values.Count == 0)
                return true;
            var firstVal = Values[0];
            for(int i=1; i<Values.Count; i++)
            {
                if (!object.Equals(firstVal, Values[i]))
                    return true;
            }
            return false;
        }

        public object GetValue()
        {
            if (HasDifferentValue())
            {
                return this;
            }
            return Values[0];
        }
    }

    public class CustomPropertyDescriptor
    {
        public Dictionary<Type, PropertyDescriptor> Propertys
        {
            get;
            private set;
        } = new Dictionary<Type, PropertyDescriptor>();

        public Dictionary<Type, System.Reflection.FieldInfo> Fields
        {
            get;
            private set;
        } = new Dictionary<Type, System.Reflection.FieldInfo>();

        public string Name
        {
            get;
            private set;
        }
        //public string DisplayName
        //{
        //    get;
        //    private set;
        //}
        string mDisplayName;
        public string GetDisplayName(object ins)
        {
            if (PropertyProvider == null || ins == null)
                return mDisplayName;
            var proIns = GetValue(ins, false);
            return PropertyProvider.GetName(proIns);
        }
        public string Description
        {
            get;
            private set;
        }

        //public Type PropertyType
        //{
        //    get;
        //    private set;
        //}
        Type mPropertyType;
        public Type GetPropertyType(object ins)
        {
            if (PropertyProvider == null || ins == null)
                return mPropertyType;
            var proIns = GetValue(ins, false);
            return PropertyProvider.GetUIType(proIns);
        }

        public bool IsBrowsable
        {
            get;
            private set;
        }

        public AttributeCollection Attributes
        {
            get;
            private set;
        }

        public TypeConverter Converter
        {
            get;
            private set;
        }
        protected bool mIsReadOnly = false;
        public bool GetIsReadOnly(object ins)
        {
            if (PropertyProvider == null || ins == null)
                return mIsReadOnly;
            var proIns = GetValue(ins, false);
            return PropertyProvider.IsReadOnly(proIns);
        }
        public string Category
        {
            get;
            private set;
        }

        public bool IsNotifyValueChange
        {
            get;
            private set;
        }
        public EngineNS.Editor.Editor_PropertyGridUIProvider PropertyProvider
        {
            get;
            private set;
        }
        public bool ParentIsValueType
        {
            get;
            private set;
        }
        public void SetPropertyProvider(EngineNS.Editor.Editor_PropertyGridUIProvider provider, object propertyValue)
        {
            PropertyProvider = provider;
        }

        public CustomPropertyDescriptor(Type insType, PropertyDescriptor property, bool parentIsValueType)
        {
            Propertys[insType] = property;
            Name = property.Name;
            mDisplayName = property.DisplayName;
            mPropertyType = property.PropertyType;
            IsBrowsable = property.IsBrowsable;
            var atts = new Attribute[property.Attributes.Count];
            property.Attributes.CopyTo(atts, 0);
            Attributes = new AttributeCollection(atts);
            Converter = property.Converter;
            if (string.IsNullOrEmpty(property.Description))
            {
                for (int i = 0; i < Attributes.Count; i++)
                {
                    var att = Attributes[i] as EngineNS.Editor.Editor_RNameMacrossType;
                    if (att != null)
                    {
                        Description = att.MacrossBaseType.FullName;
                        break;
                    }
                }
            }
            else
            {
                Description = property.Description;
            }
           
            mIsReadOnly = property.IsReadOnly;
            Category = property.Category;
            foreach (var att in Attributes)
            {
                if (att is EngineNS.Editor.Editor_NotifyMemberValueChangedAttribute)
                    IsNotifyValueChange = true;
            }

            if (parentIsValueType)
                ParentIsValueType = parentIsValueType;
            else
                ParentIsValueType = insType.IsValueType;
        }
        public CustomPropertyDescriptor(Type insType, System.Reflection.FieldInfo field, bool parentIsValueType)
        {
            Fields[insType] = field;
            Name = field.Name;
            var disNameAtt = field.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (disNameAtt != null && disNameAtt.Length > 0)
                mDisplayName = ((DisplayNameAttribute)disNameAtt[0]).DisplayName;
            mPropertyType = field.FieldType;
            var browsableAtt = field.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (browsableAtt != null && browsableAtt.Length > 0)
                IsBrowsable = ((BrowsableAttribute)browsableAtt[0]).Browsable;
            else
                IsBrowsable = true;
            var atts = field.GetCustomAttributes(true);
            Attribute[] tAtts = new Attribute[atts.Length];
            for (int i = 0; i < atts.Length; i++)
                tAtts[i] = atts[i] as Attribute;
            Attributes = new AttributeCollection(tAtts);
            var desAtt = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (desAtt != null && desAtt.Length > 0)
                Description = ((DescriptionAttribute)desAtt[0]).Description;
            else
            {
                var MacrossType = field.GetCustomAttributes(typeof(EngineNS.Editor.Editor_RNameMacrossType), true);
                if (MacrossType.Length > 0)
                {
                    Description = ((EngineNS.Editor.Editor_RNameMacrossType)MacrossType[0]).MacrossBaseType.FullName;
                }
            }
            var readOnlyAtt = field.GetCustomAttributes(typeof(ReadOnlyAttribute), true);
            if (readOnlyAtt != null && readOnlyAtt.Length > 0)
                mIsReadOnly = ((ReadOnlyAttribute)readOnlyAtt[0]).IsReadOnly;
            var categoryAtt = field.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (categoryAtt != null && categoryAtt.Length > 0)
                Category = ((CategoryAttribute)categoryAtt[0]).Category;
            else
                Category = "杂项";
            var notifyAtt = field.GetCustomAttributes(typeof(EngineNS.Editor.Editor_NotifyMemberValueChangedAttribute), true);
            if (notifyAtt != null && notifyAtt.Length > 0)
                IsNotifyValueChange = true;
            var converAtts = field.FieldType.GetCustomAttributes(typeof(System.ComponentModel.TypeConverterAttribute), true);
            if(converAtts != null && converAtts.Length > 0)
            {
                var convertName = ((System.ComponentModel.TypeConverterAttribute)converAtts[0]).ConverterTypeName;
                Type converType = null;
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    converType = assembly.GetType(convertName);
                    if (converType != null)
                        break;
                }
                Converter = System.Activator.CreateInstance(converType) as TypeConverter;
            }

            if (parentIsValueType)
                ParentIsValueType = parentIsValueType;
            else
                ParentIsValueType = insType.IsValueType;
        }
        public override bool Equals(object obj)
        {
            var tagPro = obj as CustomPropertyDescriptor;
            if (tagPro == null)
                return false;
            return ((tagPro.Name == this.Name) &&
                    (tagPro.mPropertyType == this.mPropertyType) &&
                    (tagPro.Converter == this.Converter) &&
                    (tagPro.mDisplayName == this.mDisplayName) &&
                    (tagPro.Description == this.Description) &&
                    (tagPro.mIsReadOnly == this.mIsReadOnly) &&
                    (tagPro.Category == this.Category));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //public void Add(Type insType, PropertyDescriptor property)
        //{
        //    Propertys[insType] = property;

        //    if (!property.IsBrowsable)
        //        IsBrowsable = false;
        //    // todo: Attributes收集
        //    //var proAtts = property.Attributes;
        //    //for(int i=Attributes.Count - 1; i>=0; i--)
        //    //{
        //    //}
        //}

        public bool Add(CustomPropertyDescriptor property)
        {
            if ((property.Name != Name) || (property.mPropertyType != mPropertyType))
                return false;
            foreach(var pro in property.Propertys)
            {
                Propertys[pro.Key] = pro.Value;
                if (!pro.Value.IsBrowsable)
                    IsBrowsable = false;
            }
            return true;
        }

        object _ProGetValue(PropertyDescriptor pro, object ins, bool useProvider)
        {
            if (PropertyProvider != null && useProvider)
            {
                var proIns = pro.GetValue(ins);
                return PropertyProvider.GetValue(proIns);
            }
            else
                return pro.GetValue(ins);
        }
        object _FieldGetValue(System.Reflection.FieldInfo field, object ins, bool useProvider)
        {
            if(PropertyProvider != null && useProvider)
            {
                var fieldIns = field.GetValue(ins);
                return PropertyProvider.GetValue(fieldIns);
            }
            else
            {

            }
                return field.GetValue(ins);
        }

        public object GetValue(object ins, bool useProvider = true)
        {
            if (ins == null)
                return null;
            if(ins.GetType().GetInterface(typeof(IEnumerable).FullName) != null)
            {
                var multiVals = new PropertyMultiValue();
                foreach(var objIns in (IEnumerable)ins)
                {
                    PropertyDescriptor dicPro;
                    if (Propertys.TryGetValue(objIns.GetType(), out dicPro))
                    {
                        var val = _ProGetValue(dicPro, objIns, useProvider);
                        multiVals.Values.Add(val);
                    }
                    System.Reflection.FieldInfo field;
                    if(Fields.TryGetValue(objIns.GetType(), out field))
                    {
                        var val = _FieldGetValue(field, objIns, useProvider);
                        multiVals.Values.Add(val);
                    }
                }
                return multiVals.GetValue();
            }
            else
            {
                PropertyDescriptor dicPro;
                if (Propertys.TryGetValue(ins.GetType(), out dicPro))
                {
                    return _ProGetValue(dicPro, ins, useProvider);
                }
                System.Reflection.FieldInfo field;
                if(Fields.TryGetValue(ins.GetType(), out field))
                {
                    return _FieldGetValue(field, ins, useProvider);
                }
            }
            return null;
        }
        void _ProSetValue(PropertyDescriptor pro, ref object ins, ref object value)
        {
            if (PropertyProvider != null)
            {
                var proIns = pro.GetValue(ins);
                PropertyProvider.SetValue(proIns, value);
            }
            else
                pro.SetValue(ins, value);
        }
        void _FieldSetValue(System.Reflection.FieldInfo field, ref object ins, ref object value)
        {
            if (PropertyProvider != null)
            {
                var fieldIns = field.GetValue(ins);
                PropertyProvider.SetValue(fieldIns, value);
            }
            else
                field.SetValue(ins, value);
        }
        public void SetValue(ref object ins, ref object value)
        {
            if (ins == null)
                return;
            if(ins.GetType().GetInterface(typeof(IEnumerable).FullName) != null)
            {
                foreach(var objIns in (IEnumerable)ins)
                {
                    var obj = objIns;
                    PropertyDescriptor dicPro;
                    if (Propertys.TryGetValue(obj.GetType(), out dicPro))
                        _ProSetValue(dicPro, ref obj, ref value);
                    System.Reflection.FieldInfo field;
                    if (Fields.TryGetValue(ins.GetType(), out field))
                        _FieldSetValue(field, ref obj, ref value);
                }
            }
            else
            {
                PropertyDescriptor dicPro;
                if (Propertys.TryGetValue(ins.GetType(), out dicPro))
                    _ProSetValue(dicPro, ref ins, ref value);
                System.Reflection.FieldInfo field;
                if (Fields.TryGetValue(ins.GetType(), out field))
                    _FieldSetValue(field, ref ins, ref value);
            }
        }

        public void AddValueChanged(object component, EventHandler handler)
        {
            var objType = component.GetType();
            if(objType.GetInterface(typeof(IEnumerable).FullName) != null)
            {
                foreach(var objIns in (IEnumerable)component)
                {
                    if (objIns == null)
                        continue;
                    var tempType = objIns.GetType();
                    PropertyDescriptor pd;
                    if (Propertys.TryGetValue(tempType, out pd))
                        pd.AddValueChanged(objIns, handler);
                }
            }
            else
            {
                PropertyDescriptor pd;
                if (Propertys.TryGetValue(objType, out pd))
                    pd.AddValueChanged(component, handler);
            }
        }
        public void RemoveValueChanged(object component, EventHandler handler)
        {
            var objType = component.GetType();
            if(objType.GetInterface(typeof(IEnumerable).FullName) != null)
            {
                foreach(var objIns in (IEnumerable)component)
                {
                    PropertyDescriptor pd;
                    if (Propertys.TryGetValue(objType, out pd))
                        pd.RemoveValueChanged(objIns, handler);
                }
            }
            else
            {
                PropertyDescriptor pd;
                if(Propertys.TryGetValue(objType, out pd))
                    pd.RemoveValueChanged(component, handler);
            }
        }
        public object GetPropertyOwner(ICustomTypeDescriptor ins)
        {
            PropertyDescriptor pd;
            if (Propertys.TryGetValue(ins.GetType(), out pd))
                return ins.GetPropertyOwner(pd);
            return null;
        }
        //public PropertyDescriptor GetProperty(object ins)
        //{
        //    PropertyDescriptor pd;
        //    if (Propertys.TryGetValue(ins.GetType(), out pd))
        //        return pd;
        //    return null;
        //}
    }

    public class CustomPropertyDescriptorCollection : ICollection, IEnumerable, IList, IDictionary
    {
        public static readonly CustomPropertyDescriptorCollection Empty = new CustomPropertyDescriptorCollection(null, true);
        private IDictionary cachedFoundProperties;
        private bool cachedIgnoreCase;
        private CustomPropertyDescriptor[] properties;
        public CustomPropertyDescriptor[] Properties => properties;
        private int propCount;
        private string[] namedSort;
        private IComparer comparer;
        private bool propsOwned;
        private bool needSort;
        private bool readOnly;
        
        public CustomPropertyDescriptorCollection()
        {
            this.propsOwned = true;
            this.properties = new CustomPropertyDescriptor[0];
            this.propCount = 0;
            this.propsOwned = true;
        }
        public CustomPropertyDescriptorCollection(Type ins, PropertyDescriptorCollection collection, bool parentIsValueType)
        {
            bool notShowBaseTypeProperties = false;
            var atts = ins.GetCustomAttributes(typeof(EngineNS.Editor.Editor_DoNotShowBaseClassProperties), false);
            if (atts.Length > 0)
                notShowBaseTypeProperties = true;

            List<CustomPropertyDescriptor> cpd = null;
            if (collection.Count > 0)
            {
                cpd = new List<CustomPropertyDescriptor>(collection.Count);
                for (int i = 0; i < collection.Count; i++)
                {
                    var pro = collection[i];
                    if(notShowBaseTypeProperties)
                    {
                        if (pro.ComponentType != ins)
                            continue;
                    }
                    cpd.Add(new CustomPropertyDescriptor(ins, pro, parentIsValueType));
                }
            }
            this.propsOwned = true;
            this.properties = cpd?.ToArray();
            if (properties == null)
            {
                this.properties = new CustomPropertyDescriptor[0];
                this.propCount = 0;
            }
            else
            {
                this.propCount = properties.Length;
            }
        }
        public CustomPropertyDescriptorCollection(Type ins, System.Reflection.FieldInfo[] fields, bool parentIsValueType)
        {
            List<CustomPropertyDescriptor> cpd = null;
            if(fields.Length > 0)
            {
                cpd = new List<CustomPropertyDescriptor>();
                foreach(var field in fields)
                {
                    var atts = field.GetCustomAttributes(typeof(EngineNS.Editor.Editor_ShowInPropertyGridAttribute), true);
                    if (atts == null || atts.Length == 0)
                        continue;
                    cpd.Add(new CustomPropertyDescriptor(ins, field, parentIsValueType));
                }
            }
            this.propsOwned = true;
            if(cpd != null)
                this.properties = cpd.ToArray();
            if (properties == null)
            {
                this.properties = new CustomPropertyDescriptor[0];
                this.propCount = 0;
            }
            else
                this.propCount = properties.Length;
        }
        public CustomPropertyDescriptorCollection(CustomPropertyDescriptor[] properties)
        {
            this.propsOwned = true;
            this.properties = properties;
            if (properties == null)
            {
                this.properties = new CustomPropertyDescriptor[0];
                this.propCount = 0;
            }
            else
            {
                this.propCount = properties.Length;
            }
            this.propsOwned = true;
        }
        public CustomPropertyDescriptorCollection(CustomPropertyDescriptor[] properties, bool readOnly) : this(properties)
        {
            this.readOnly = readOnly;
        }
        private CustomPropertyDescriptorCollection(CustomPropertyDescriptor[] properties, int propCount, string[] namedSort, IComparer comparer)
        {
            this.propsOwned = true;
            this.propsOwned = false;
            if (namedSort != null)
            {
                this.namedSort = (string[])namedSort.Clone();
            }
            this.comparer = comparer;
            this.properties = properties;
            this.propCount = propCount;
            this.needSort = true;
        }

        public int Add(CustomPropertyDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.propCount + 1);
            int propCount = this.propCount;
            this.propCount = propCount + 1;
            this.properties[propCount] = value;
            return (this.propCount - 1);
        }

        public void Clear()
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.propCount = 0;
            this.cachedFoundProperties = null;
        }

        public bool Contains(CustomPropertyDescriptor value)
        {
            return (this.IndexOf(value) >= 0);
        }

        public void CopyTo(Array array, int index)
        {
            this.EnsurePropsOwned();
            Array.Copy(this.properties, 0, array, index, this.Count);
        }

        private void EnsurePropsOwned()
        {
            if (!this.propsOwned)
            {
                this.propsOwned = true;
                if (this.properties != null)
                {
                    CustomPropertyDescriptor[] destinationArray = new CustomPropertyDescriptor[this.Count];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.Count);
                    this.properties = destinationArray;
                }
            }
            if (this.needSort)
            {
                this.needSort = false;
                this.InternalSort(this.namedSort);
            }
        }

        private void EnsureSize(int sizeNeeded)
        {
            if (sizeNeeded > this.properties.Length)
            {
                if ((this.properties == null) || (this.properties.Length == 0))
                {
                    this.propCount = 0;
                    this.properties = new CustomPropertyDescriptor[sizeNeeded];
                }
                else
                {
                    this.EnsurePropsOwned();
                    CustomPropertyDescriptor[] destinationArray = new CustomPropertyDescriptor[Math.Max(sizeNeeded, this.properties.Length * 2)];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                    this.properties = destinationArray;
                }
            }
        }

        public virtual CustomPropertyDescriptor Find(string name, bool ignoreCase)
        {
            CustomPropertyDescriptorCollection descriptors = this;
            lock (descriptors)
            {
                CustomPropertyDescriptor descriptor = null;
                if ((this.cachedFoundProperties == null) || (this.cachedIgnoreCase != ignoreCase))
                {
                    this.cachedIgnoreCase = ignoreCase;
                    this.cachedFoundProperties = new HybridDictionary(ignoreCase);
                }
                object obj2 = this.cachedFoundProperties[name];
                if (obj2 != null)
                {
                    return (CustomPropertyDescriptor)obj2;
                }
                for (int i = 0; i < this.propCount; i++)
                {
                    if (ignoreCase)
                    {
                        if (!string.Equals(this.properties[i].Name, name, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        this.cachedFoundProperties[name] = this.properties[i];
                        descriptor = this.properties[i];
                        break;
                    }
                    if (this.properties[i].Name.Equals(name))
                    {
                        this.cachedFoundProperties[name] = this.properties[i];
                        descriptor = this.properties[i];
                        break;
                    }
                }
                return descriptor;
            }
        }

        public IEnumerator GetEnumerator()
        {
            this.EnsurePropsOwned();
            if (this.properties.Length != this.propCount)
            {
                CustomPropertyDescriptor[] destinationArray = new CustomPropertyDescriptor[this.propCount];
                Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                return destinationArray.GetEnumerator();
            }
            return this.properties.GetEnumerator();
        }

        public int IndexOf(CustomPropertyDescriptor value)
        {
            return Array.IndexOf<CustomPropertyDescriptor>(this.properties, value, 0, this.propCount);
        }

        public void Insert(int index, CustomPropertyDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            this.EnsureSize(this.propCount + 1);
            if (index < this.propCount)
            {
                Array.Copy(this.properties, index, this.properties, index + 1, this.propCount - index);
            }
            this.properties[index] = value;
            this.propCount++;
        }

        protected void InternalSort(string[] names)
        {
            if ((this.properties != null) && (this.properties.Length != 0))
            {
                this.InternalSort(this.comparer);
                if ((names != null) && (names.Length != 0))
                {
                    ArrayList list = new ArrayList(this.properties);
                    int num = 0;
                    int length = this.properties.Length;
                    for (int i = 0; i < names.Length; i++)
                    {
                        for (int k = 0; k < length; k++)
                        {
                            CustomPropertyDescriptor descriptor = (CustomPropertyDescriptor)list[k];
                            if ((descriptor != null) && descriptor.Name.Equals(names[i]))
                            {
                                this.properties[num++] = descriptor;
                                list[k] = null;
                                break;
                            }
                        }
                    }
                    for (int j = 0; j < length; j++)
                    {
                        if (list[j] != null)
                        {
                            this.properties[num++] = (CustomPropertyDescriptor)list[j];
                        }
                    }
                }
            }
        }

        protected void InternalSort(IComparer sorter)
        {
            if (sorter == null)
            {
                TypeDescriptor.SortDescriptorArray(this);
            }
            else
            {
                Array.Sort(this.properties, sorter);
            }
        }

        public void Remove(CustomPropertyDescriptor value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            int index = this.IndexOf(value);
            if (index != -1)
            {
                this.RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException();
            }
            if (index < (this.propCount - 1))
            {
                Array.Copy(this.properties, index + 1, this.properties, index, (this.propCount - index) - 1);
            }
            this.properties[this.propCount - 1] = null;
            this.propCount--;
        }

        public virtual CustomPropertyDescriptorCollection Sort()
        {
            return new CustomPropertyDescriptorCollection(this.properties, this.propCount, this.namedSort, this.comparer);
        }

        public virtual CustomPropertyDescriptorCollection Sort(string[] names)
        {
            return new CustomPropertyDescriptorCollection(this.properties, this.propCount, names, this.comparer);
        }

        public virtual CustomPropertyDescriptorCollection Sort(IComparer comparer)
        {
            return new CustomPropertyDescriptorCollection(this.properties, this.propCount, this.namedSort, comparer);
        }

        public virtual CustomPropertyDescriptorCollection Sort(string[] names, IComparer comparer)
        {
            return new CustomPropertyDescriptorCollection(this.properties, this.propCount, names, comparer);
        }

        void IDictionary.Add(object key, object value)
        {
            CustomPropertyDescriptor descriptor = value as CustomPropertyDescriptor;
            if (descriptor == null)
            {
                throw new ArgumentException("value");
            }
            this.Add(descriptor);
        }

        void IDictionary.Clear()
        {
            this.Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return ((key is string) && (this[(string)key] != null));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new PropertyDescriptorEnumerator(this);
        }

        void IDictionary.Remove(object key)
        {
            if (key is string)
            {
                CustomPropertyDescriptor descriptor = this[(string)key];
                if (descriptor != null)
                {
                    ((IList)this).Remove(descriptor);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return this.Add((CustomPropertyDescriptor)value);
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((CustomPropertyDescriptor)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((CustomPropertyDescriptor)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (CustomPropertyDescriptor)value);
        }

        void IList.Remove(object value)
        {
            this.Remove((CustomPropertyDescriptor)value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public int Count
        {
            get
            {
                return this.propCount;
            }
        }

        public virtual CustomPropertyDescriptor this[int index]
        {
            get
            {
                if (index >= this.propCount)
                {
                    throw new IndexOutOfRangeException();
                }
                this.EnsurePropsOwned();
                return this.properties[index];
            }
        }

        public virtual CustomPropertyDescriptor this[string name]
        {
            get
            {
                return this.Find(name, false);
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return this.readOnly;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (key is string)
                {
                    return this[(string)key];
                }
                return null;
            }
            set
            {
                if (this.readOnly)
                {
                    throw new NotSupportedException();
                }
                if ((value != null) && !(value is CustomPropertyDescriptor))
                {
                    throw new ArgumentException("value");
                }
                int index = -1;
                if (key is int)
                {
                    index = (int)key;
                    if ((index < 0) || (index >= this.propCount))
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                else
                {
                    if (!(key is string))
                    {
                        throw new ArgumentException("key");
                    }
                    for (int i = 0; i < this.propCount; i++)
                    {
                        if (this.properties[i].Name.Equals((string)key))
                        {
                            index = i;
                            break;
                        }
                    }
                }
                if (index == -1)
                {
                    this.Add((CustomPropertyDescriptor)value);
                }
                else
                {
                    this.EnsurePropsOwned();
                    this.properties[index] = (CustomPropertyDescriptor)value;
                    if ((this.cachedFoundProperties != null) && (key is string))
                    {
                        this.cachedFoundProperties[key] = value;
                    }
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                string[] strArray = new string[this.propCount];
                for (int i = 0; i < this.propCount; i++)
                {
                    strArray[i] = this.properties[i].Name;
                }
                return strArray;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                if (this.properties.Length != this.propCount)
                {
                    CustomPropertyDescriptor[] destinationArray = new CustomPropertyDescriptor[this.propCount];
                    Array.Copy(this.properties, 0, destinationArray, 0, this.propCount);
                    return destinationArray;
                }
                return (ICollection)this.properties.Clone();
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return this.readOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (this.readOnly)
                {
                    throw new NotSupportedException();
                }
                if (index >= this.propCount)
                {
                    throw new IndexOutOfRangeException();
                }
                if ((value != null) && !(value is CustomPropertyDescriptor))
                {
                    throw new ArgumentException("value");
                }
                this.EnsurePropsOwned();
                this.properties[index] = (CustomPropertyDescriptor)value;
            }
        }

        private class PropertyDescriptorEnumerator : IDictionaryEnumerator, IEnumerator
        {
            private CustomPropertyDescriptorCollection owner;
            private int index = -1;

            public PropertyDescriptorEnumerator(CustomPropertyDescriptorCollection owner)
            {
                this.owner = owner;
            }

            public bool MoveNext()
            {
                if (this.index < (this.owner.Count - 1))
                {
                    this.index++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                this.index = -1;
            }

            public object Current
            {
                get
                {
                    return this.Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    CustomPropertyDescriptor descriptor = this.owner[this.index];
                    return new DictionaryEntry(descriptor.Name, descriptor);
                }
            }

            public object Key
            {
                get
                {
                    return this.owner[this.index].Name;
                }
            }

            public object Value
            {
                get
                {
                    return this.owner[this.index].Name;
                }
            }
        }
    }
}
