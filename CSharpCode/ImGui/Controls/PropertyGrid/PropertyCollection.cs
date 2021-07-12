using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class CustomPropertyDescriptor : IPooledObjectBase
    {
        public CustomPropertyDescriptor ParentPropertyDesc;

        public object ObjectInstance;   // 包含此属性的对象的值

        //public HashSet<Rtti.UTypeDesc> Propertys
        //{
        //    get;
        //    private set;
        //} = new HashSet<Rtti.UTypeDesc>();
        //public HashSet<Rtti.UTypeDesc> Fields
        //{
        //    get;
        //    private set;
        //} = new HashSet<Rtti.UTypeDesc>();

        public string Name
        {
            get;
            private set;
        }

        string mDisplayName;
        public string GetDisplayName(object objIns)
        {
            if(CustomValueEditor != null && CustomValueEditor.Provider != null)
            {
                var proIns = GetValue(objIns, false);
                return CustomValueEditor.Provider.GetDisplayName(proIns);
            }
            if (string.IsNullOrEmpty(mDisplayName))
                return Name;
            return mDisplayName;
        }

        Rtti.UTypeDesc mPropertyType;
        public Type GetPropertyType(object objIns)
        {
            var proIns = GetValue(objIns, false);
            if (CustomValueEditor != null && CustomValueEditor.Provider != null)
            {
                return CustomValueEditor.Provider.GetPropertyType(proIns).SystemType;
            }
            else if (proIns is PropertyMultiValue)
                return mPropertyType.SystemType;
            else if (proIns != null)
                return proIns.GetType();
            return mPropertyType.SystemType;
        }

        bool mIsReadonly = false;
        public bool GetIsReadonly(object objIns)
        {
            if(CustomValueEditor != null && CustomValueEditor.Provider != null)
            {
                var proIns = GetValue(objIns, false);
                return CustomValueEditor.Provider.IsReadOnly(proIns);
            }
            return mIsReadonly;
        }

        public string Description
        {
            get;
            private set;
        }

        public AttributeCollection Attributes
        {
            get;
            private set;
        }

        public string Category
        {
            get;
            private set;
        }

        public bool ParentIsValueType
        {
            get;
            private set;
        }

        public bool IsBrowsable
        {
            get;
            private set;
        }

        public Rtti.UTypeDesc DeclaringType
        {
            get;
            private set;
        }

        public PGCustomValueEditorAttribute CustomValueEditor
        {
            get;
            private set;
        }

        PropertyMultiValue mMultiValue;

        #region UI
        public Vector2 RowRectMin;
        public Vector2 RowRectMax;
        public float RowHeight;
        public bool IsMouseHovered = false;
        #endregion

        public CustomPropertyDescriptor()
        {

        }
        public void InitValue(object objIns, Rtti.UTypeDesc insType, PropertyDescriptor property, bool parentIsValueType)
        {
            //Propertys.Add(insType);
            Name = property.Name;
            mDisplayName = property.DisplayName;
            mPropertyType = Rtti.UTypeDesc.TypeOf(property.PropertyType);
            var atts = new Attribute[property.Attributes.Count];
            property.Attributes.CopyTo(atts, 0);
            Attributes = new AttributeCollection(atts); 
            Description = property.Description;
            mIsReadonly = property.IsReadOnly;
            IsBrowsable = property.IsBrowsable;
            foreach(var att in property.Attributes)
            {
                if(att is PGCustomValueEditorAttribute)
                {
                    var tAtt = att as PGCustomValueEditorAttribute;
                    if (!tAtt.Initialized)
                        _ = tAtt.Initialize();
                    IsBrowsable = !tAtt.HideInPG;
                    mIsReadonly = tAtt.ReadOnly;
                    CustomValueEditor = tAtt;
                }
            }
            Category = property.Category;
            if (parentIsValueType)
                ParentIsValueType = parentIsValueType;
            else
                ParentIsValueType = insType.IsValueType;
            DeclaringType = Rtti.UTypeDesc.TypeOf(property.ComponentType);
        }

        public void InitValue(object objIns, Rtti.UTypeDesc insType, System.Reflection.FieldInfo field, bool parentIsValueType)
        {
            //Fields.Add(insType);
            Name = field.Name;
            var disNameAtt = field.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (disNameAtt != null && disNameAtt.Length > 0)
                mDisplayName = ((DisplayNameAttribute)disNameAtt[0]).DisplayName;
            mPropertyType = Rtti.UTypeDesc.TypeOf(field.FieldType);
            var browsableAtt = field.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (browsableAtt != null && browsableAtt.Length > 0)
                IsBrowsable = ((BrowsableAttribute)browsableAtt[0]).Browsable;
            else
                IsBrowsable = true;
            var atts = field.GetCustomAttributes(true);
            Attribute[] tAtts = new Attribute[atts.Length];
            for (int i = 0; i < atts.Length; i++)
            {
                tAtts[i] = atts[i] as Attribute;

                if(atts[i] is PGCustomValueEditorAttribute)
                {
                    var tAtt = atts[i] as PGCustomValueEditorAttribute;
                    IsBrowsable = !tAtt.HideInPG;
                    mIsReadonly = tAtt.ReadOnly;
                    CustomValueEditor = tAtt;
                }
            }
            Attributes = new AttributeCollection(tAtts);
            var desAtt = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (desAtt != null && desAtt.Length > 0)
                Description = ((DescriptionAttribute)desAtt[0]).Description;
            else
            {
            }
            //var readOnlyAtt = field.GetCustomAttributes(typeof(ReadOnlyAttribute), true);
            //if (readOnlyAtt != null && readOnlyAtt.Length > 0)
            //    mIsReadonly = ((ReadOnlyAttribute)readOnlyAtt[0]).IsReadOnly;
            var categoryAtt = field.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (categoryAtt != null && categoryAtt.Length > 0)
                Category = ((CategoryAttribute)categoryAtt[0]).Category;
            else
                Category = "Misc";
            if (parentIsValueType)
                ParentIsValueType = parentIsValueType;
            else
                ParentIsValueType = insType.IsValueType;
            DeclaringType = Rtti.UTypeDesc.TypeOf(field.DeclaringType);
        }

        public override bool Equals(object obj)
        {
            var tagPro = obj as CustomPropertyDescriptor;
            if (tagPro == null)
                return false;
            return ((tagPro.Name == this.Name) &&
                    (tagPro.mPropertyType == this.mPropertyType) &&
                    (tagPro.mDisplayName == this.mDisplayName) &&
                    (tagPro.Description == this.Description) &&
                    (tagPro.mIsReadonly == this.mIsReadonly) &&
                    (tagPro.Category == this.Category));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //public bool Add(CustomPropertyDescriptor propertyDesc)
        //{
        //    if ((propertyDesc.Name != Name) || (propertyDesc.mPropertyType != mPropertyType))
        //        return false;
        //    //foreach(var pro in propertyDesc.Propertys)
        //    //{
        //    //    Propertys.Add(pro);
        //    //}
        //    return true;
        //}

        object _ProGetValue(PropertyInfo pro, object objIns, bool useProvider)
        {
            if (pro == null)
                return null;

            if (CustomValueEditor != null && CustomValueEditor.Provider != null && useProvider)
            {
                var proIns = pro.GetValue(objIns);
                return CustomValueEditor.Provider.GetValue(proIns);
            }
            else
                return pro.GetValue(objIns);
        }
        object _FieldGetValue(FieldInfo field, object objIns, bool useProvider)
        {
            if (CustomValueEditor != null && CustomValueEditor.Provider != null && useProvider)
            {
                var fieldIns = field.GetValue(objIns);
                return CustomValueEditor.Provider.GetValue(fieldIns);
            }
            else
                return field.GetValue(objIns);
        }

        public object GetValue(object objIns, bool useProvider = true)
        {
            if (objIns == null)
                return null;

            var enumerable = objIns as IEnumerable;
            if (enumerable != null)
            {
                if(mMultiValue == null)
                    mMultiValue = new PropertyMultiValue();
                mMultiValue.Cleanup();
                foreach(var elem in (IEnumerable)objIns)
                {
                    var elemType = elem.GetType();
                    var propertyInfo = elemType.GetProperty(Name);
                    if (propertyInfo != null)
                    {
                        var val = _ProGetValue(propertyInfo, elem, useProvider);
                        mMultiValue.Values.Add(val);
                    }

                    var fieldInfo = elemType.GetField(Name);
                    if (fieldInfo != null)
                    {
                        var val = _FieldGetValue(fieldInfo, elem, useProvider);
                        mMultiValue.Values.Add(val);
                    }
                }
                return mMultiValue.GetValue();
            }
            else
            {
                var insType = objIns.GetType();
                var propertyInfo = insType.GetProperty(Name);
                if (propertyInfo != null)
                {
                    return _ProGetValue(propertyInfo, objIns, useProvider);
                }

                var fieldInfo = insType.GetField(Name);
                if (fieldInfo != null)
                {
                    return _FieldGetValue(fieldInfo, objIns, useProvider);
                }
            }

            return null;
        }

        void _ProSetValue(PropertyInfo pro, object objIns, object value, bool useProvider)
        {
            if (CustomValueEditor != null && CustomValueEditor.Provider != null && useProvider)
            {
                var proIns = pro.GetValue(objIns);
                CustomValueEditor.Provider.SetValue(proIns, value);
            }
            else
                pro.SetValue(objIns, value);
        }
        void _FieldSetValue(FieldInfo field, object objIns, object value, bool useProvider)
        {
            if (CustomValueEditor != null && CustomValueEditor.Provider != null && useProvider)
            {
                var fieldIns = field.GetValue(objIns);
                CustomValueEditor.Provider.SetValue(fieldIns, value);
            }
            else
                field.SetValue(objIns, value);
        }
        public void SetValue(ref object objIns, object value, bool useProvider = true)
        {
            if (objIns == null)
                return;

            var enumerable = objIns as IEnumerable;
            if(enumerable != null)
            {
                var multiValue = value as PropertyMultiValue;
                if(multiValue != null)
                {
                    int index = 0;
                    foreach(var elem in (IEnumerable)objIns)
                    {
                        if (index >= multiValue.Values.Count)
                            continue;

                        var elemType = elem.GetType();
                        var propertyInfo = elemType.GetProperty(Name);
                        if (propertyInfo != null)
                            _ProSetValue(propertyInfo, elem, multiValue.Values[index], useProvider);
                        var fieldInfo = elemType.GetField(Name);
                        if (fieldInfo != null)
                            _FieldSetValue(fieldInfo, elem, multiValue.Values[index], useProvider);

                        index++;
                    }
                }
                else
                {
                    foreach (var elem in (IEnumerable)objIns)
                    {
                        var elemType = elem.GetType();
                        var propertyInfo = elemType.GetProperty(Name);
                        if (propertyInfo != null)
                        {
                            _ProSetValue(propertyInfo, elem, value, useProvider);
                        }
                        var fieldInfo = elemType.GetField(Name);
                        if (fieldInfo != null)
                        {
                            _FieldSetValue(fieldInfo, elem, value, useProvider);
                        }
                    }
                }
            }
            else
            {
                var insType = objIns.GetType();
                var propertyInfo = insType.GetProperty(Name);
                if (propertyInfo != null)
                {
                    _ProSetValue(propertyInfo, objIns, value, useProvider);
                }
                var fieldInfo = insType.GetField(Name);
                if (fieldInfo != null)
                {
                    _FieldSetValue(fieldInfo, objIns, value, useProvider);
                }
            }

            if(ParentIsValueType)
            {
                if(ParentPropertyDesc != null)
                {
                    ParentPropertyDesc.SetValue(ref ParentPropertyDesc.ObjectInstance, objIns);
                }
                else
                {
                    // root是值类型，改不回去了
                }
            }
        }

        public override string ToString()
        {
            var type = GetPropertyType(ObjectInstance);
            return type.ToString();
        }

        public bool ReleaseObject(object obj = null)
        {
            ParentPropertyDesc = default;
            ObjectInstance = default;
            //Propertys.Clear();
            //Fields.Clear();
            Name = default;
            mDisplayName = default;
            mPropertyType = default;
            mIsReadonly = false;
            Description = default;
            Attributes = default;
            Category = default;
            ParentIsValueType = default;
            IsBrowsable = default;
            DeclaringType = default;            
            CustomValueEditor?.Cleanup();
            CustomValueEditor = default;
            mMultiValue?.Cleanup();

            PropertyCollection.PropertyDescPool.ReleaseObject(this);

            return true;
        }
        public void CopyFrom(CustomPropertyDescriptor proDesc)
        {
            if (proDesc.ParentPropertyDesc != null)
            {
                if(ParentPropertyDesc == null)
                    ParentPropertyDesc = PropertyCollection.PropertyDescPool.QueryObjectSync();
                ParentPropertyDesc.CopyFrom(proDesc);
            }
            ObjectInstance = proDesc.ObjectInstance;
            Name = proDesc.Name;
            mDisplayName = proDesc.mDisplayName;
            mPropertyType = proDesc.mPropertyType;
            mIsReadonly = proDesc.mIsReadonly;
            Description = proDesc.Description;
            Attributes = proDesc.Attributes;
            Category = proDesc.Category;
            ParentIsValueType = proDesc.ParentIsValueType;
            IsBrowsable = proDesc.IsBrowsable;
            DeclaringType = proDesc.DeclaringType;
            CustomValueEditor?.Cleanup();
            CustomValueEditor = proDesc.CustomValueEditor;
            var noUse = CustomValueEditor?.Initialize();
        }
    }

    public class PropertyMultiValue
    {
        public List<object> Values
        {
            get;
            protected set;
        } = new List<object>(10);

        string mMultiValueString = "---";
        public string MultiValueString { get => mMultiValueString; }

        public void Cleanup()
        {
            Values.Clear();
        }

        public override string ToString()
        {
            if (HasDifferentValue())
                return mMultiValueString;
            return System.Convert.ToString(Values[0]);
        }

        public bool HasDifferentValue()
        {
            if (Values.Count == 0)
                return true;
            var firstVal = Values[0];
            for (int i = 1; i < Values.Count; i++)
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
        public void SetValue(object value)
        {
            for (int i = 0; i < Values.Count; i++)
                Values[i] = value;
        }

        byte[] mTextBuffer = new byte[128];
        bool mInvalidCast = false;
        public unsafe bool Draw(string name, out object newOutValue, bool readOnly, Func<string, object> castAction)
        {
            bool retValue = false;
            newOutValue = null;
            fixed (byte* pBuffer = &mTextBuffer[0])
            {
                var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(mMultiValueString);
                var len = (uint)mMultiValueString.Length;
                CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), len);
                if (mInvalidCast)
                    ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.ErrorStringColor);
                var changed = ImGuiAPI.InputText(name, pBuffer, len, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                changed = changed && !readOnly;
                if (mInvalidCast)
                    ImGuiAPI.PopStyleColor(1);
                if (changed)
                {
                    var newValue = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                    try
                    {
                        newOutValue = castAction?.Invoke(newValue);
                        mInvalidCast = false;
                        retValue = true;
                    }
                    catch (System.Exception)
                    {
                        mInvalidCast = true;
                        retValue = false;
                    }
                }
                System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
            }
            return retValue;
        }

        public unsafe bool DrawVector<T>(in PGCustomValueEditorAttribute.EditorInfo info) where T : unmanaged
        {
            bool retValue = false;
            var minValue = float.MinValue;
            var maxValue = float.MaxValue;

            if (info.Expand)
            {
                ImGuiTableRowData rowData = new ImGuiTableRowData()
                {
                    IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer().ToPointer(),
                    MinHeight = 0,
                    CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                    CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                    IndentImageWidth = info.HostPropertyGrid.Indent,
                    IndentTextureUVMin = Vector2.Zero,
                    IndentTextureUVMax = Vector2.UnitXY,
                    IndentColor = info.HostPropertyGrid.IndentColor,
                    HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                    Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                };
                Span<float> valueArray = stackalloc float[Values.Count];

                for (var dimIdx = 0; dimIdx < sizeof(T)/sizeof(float); dimIdx++)
                {
                    bool valuesDifferent = false;
                    float firstValue = 0;
                    for (int i = 0; i < Values.Count; i++)
                    {
                        var v = (T)Values[i];
                        valueArray[i] = ((float*)&v)[dimIdx];
                        if (i == 0)
                            firstValue = valueArray[i];
                        else if (firstValue != valueArray[i])
                            valuesDifferent = true;
                    }

                    ImGuiAPI.TableNextRow(ref rowData);
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.AlignTextToFramePadding();
                    string dimName = "";
                    switch (dimIdx)
                    {
                        case 0:
                            dimName = "X";
                            break;
                        case 1:
                            dimName = "Y";
                            break;
                        case 2:
                            dimName = "Z";
                            break;
                        case 3:
                            dimName = "W";
                            break;
                    }
                    ImGuiAPI.Indent(15);
                    ImGuiAPI.Text(dimName);
                    ImGuiAPI.Unindent(15);
                    ImGuiAPI.TableNextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    if (valuesDifferent)
                    {
#pragma warning disable CA2014
                        Span<byte> textBuffer = stackalloc byte[8];
#pragma warning restore CA2014
                        var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(mMultiValueString);
                        var len = (uint)mMultiValueString.Length;
                        fixed (byte* pBuffer = &textBuffer[0])
                        {
                            CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), len);

                            var changed = ImGuiAPI.InputText(dimName, pBuffer, len, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                            if (changed)
                            {
                                var newValueStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                                try
                                {
                                    var v = System.Convert.ToSingle(newValueStr);
                                    for (int i = 0; i < Values.Count; i++)
                                    {
                                        var vv = (T)Values[i];
                                        ((float*)&vv)[dimIdx] = v;
                                        Values[i] = vv;
                                    }
                                    retValue = true;
                                }
                                catch (System.Exception)
                                {
                                    retValue = false;
                                }
                            }
                        }
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
                    }
                    else
                    {
                        var v = valueArray[0];
                        var changed = ImGuiAPI.DragScalar2(dimName, ImGuiDataType_.ImGuiDataType_Float, &v, 0.1f, &minValue, &maxValue, "%0.6f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                        if (changed)
                        {
                            for (int i = 0; i < Values.Count; i++)
                            {
                                var vv = (T)Values[i];
                                ((float*)&vv)[dimIdx] = v;
                                Values[i] = vv;
                            }
                            retValue = true;
                        }
                    }
                }
            }
            return retValue;
        }
    }

    public class CustomPropertyDescriptorCollection : IPooledObjectBase
    {
        List<CustomPropertyDescriptor> mProperties;

        public int Count { get; private set; } = 0;

        public void Cleanup()
        {
            for(int i=0; i<Count; ++i)
            {
                if(mProperties[i] != null)
                {
                    mProperties[i].ReleaseObject(null);
                    mProperties[i] = null;
                }
            }
            Count = 0;
        }

        void ResizeList(int newCount, bool fixSize = true)
        {
            if(fixSize)
            {
                newCount = (int)(Math.Ceiling(newCount / 16.0f) * 16);
            }
            if(mProperties.Count > newCount)
            {
                for(int i=newCount; i<mProperties.Count; i++)
                {
                    if (mProperties[i] != null)
                        mProperties[i].ReleaseObject();
                }
                mProperties.RemoveRange(newCount, mProperties.Count - newCount);
            }
            else if(mProperties.Count < newCount)
            {
                mProperties.Capacity = newCount;
                var count = newCount - mProperties.Count;
                for(int i=0; i<count; i++)
                {
                    mProperties.Add(null);
                }
            }
        }

        public void InitValue(object objIns, Rtti.UTypeDesc ins, PropertyDescriptorCollection collection, bool parentIsValueType)
        {
            Cleanup();

            bool notShowBaseTypeProperties = false;
            var atts = ins.SystemType.GetCustomAttributes(typeof(PGHideBaseClassPropertiesAttribute), false);
            if (atts != null && atts.Length > 0)
                notShowBaseTypeProperties = true;

            if(mProperties == null)
                mProperties = new List<CustomPropertyDescriptor>(collection.Count);
            else
            {
                ResizeList(collection.Count);
            }
            var count = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                var pro = collection[i];
                if(notShowBaseTypeProperties)
                {
                    if (pro.ComponentType != ins.SystemType)
                        continue;
                }
                var proDesc = PropertyCollection.PropertyDescPool.QueryObjectSync(); //new CustomPropertyDescriptor(objIns, ins, pro, parentIsValueType);
                proDesc.InitValue(objIns, ins, pro, parentIsValueType);
                if(!proDesc.IsBrowsable)
                {
                    proDesc.ReleaseObject();
                    continue;
                }
                mProperties[count] = proDesc;
                count++;
            }
            Count = count;
        }
        public void InitValue(object objIns, Rtti.UTypeDesc ins, System.Reflection.FieldInfo[] fields, bool parentIsValueType)
        {
            Cleanup();

            if(mProperties == null)
                mProperties = new List<CustomPropertyDescriptor>(fields.Length);
            else
            {
                ResizeList(fields.Length);
            }
            var count = 0;
            for(int i=0; i<fields.Length; i++)
            {
                var atts = fields[i].GetCustomAttributes(typeof(PGShowInPropertyGridAttribute), true);
                if (atts == null || atts.Length == 0)
                    continue;

                var proDesc = PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(objIns, ins, fields[i], parentIsValueType);
                mProperties[count] = proDesc;
                count++;
            }
            Count = count;
        }

        public CustomPropertyDescriptorCollection()
        {
            mProperties = new List<CustomPropertyDescriptor>(16);
            Count = 0;
        }

        public int IndexOf(CustomPropertyDescriptor value)
        {
            if (mProperties == null)
                return -1;
            return mProperties.IndexOf(value);
        }

        public CustomPropertyDescriptor this[int index]
        {
            get
            {
                if (mProperties == null)
                    return null;
                if (index >= Count)
                    throw new IndexOutOfRangeException("index is out of properties range");
                return mProperties[index];
            }
        }

        public void Add(CustomPropertyDescriptorCollection collection)
        {
            if(mProperties.Count < Count + collection.Count)
            {
                ResizeList(Count + collection.Count);
            }

            for(int i=0; i<collection.Count; ++i)
            {
                if (mProperties[Count] != null)
                    mProperties[Count].ReleaseObject();
                mProperties[Count] = PropertyCollection.PropertyDescPool.QueryObjectSync();
                mProperties[Count].CopyFrom(collection[i]);
                Count++;
            }
        }

        public void Add(CustomPropertyDescriptor propertyDesc)
        {
            if (mProperties.Count < Count + 1)
            {
                ResizeList(Count + 1);
            }

            if (mProperties[Count] != null)
                mProperties[Count].ReleaseObject();
            mProperties[Count] = PropertyCollection.PropertyDescPool.QueryObjectSync();
            mProperties[Count].CopyFrom(propertyDesc);
            Count++;
        }

        public void RemoveAt(int index)
        {
            if (index < (Count - 1))
            {
                mProperties[index].ReleaseObject();
                for(int i=index + 1; i<Count; i++)
                {
                    mProperties[i - 1] = mProperties[i];
                }
            }
            if (mProperties[Count - 1] != null)
            {
                //mProperties[Count - 1].ReleaseObject();
                mProperties[Count - 1] = null;
            }
            Count--;
        }

        public enum enSortType
        {

        }
        class PropertyDisplayNameComparer : IComparer<CustomPropertyDescriptor>
        {
            public int Compare(CustomPropertyDescriptor proX, CustomPropertyDescriptor proY)
            {
                if (proX == null && proY == null)
                    return 0;
                if (proX == null && proY != null)
                    return 1;
                if (proX != null && proY == null)
                    return -1;

                var nameX = proX.GetDisplayName(null);
                var nameY = proY.GetDisplayName(null);

                var minLength = Math.Min(nameX.Length, nameY.Length);
                for(int i=0; i<minLength; i++)
                {
                    if (nameX[i] > nameY[i])
                        return 1;
                    if (nameX[i] < nameY[i])
                        return -1;
                }

                if (nameX.Length == nameY.Length)
                    return 0;
                return (nameX.Length > nameY.Length) ? 1 : -1;
            }
        }
        public readonly static IComparer<CustomPropertyDescriptor> CompareByDisplayName = new PropertyDisplayNameComparer();
        public void Sort(IComparer<CustomPropertyDescriptor> comparer)
        {
            mProperties.Sort(comparer);
        }

        public bool ReleaseObject(object obj = null)
        {
            Cleanup();
            PropertyCollection.PropertyDescCollectionPool.ReleaseObject(this);

            return true;
        }

        public void CopyFrom(CustomPropertyDescriptorCollection descCollection)
        {
            if (descCollection.mProperties.Count > mProperties.Count)
                ResizeList(descCollection.mProperties.Count);
            for(int i=0; i<descCollection.Count; i++)
            {
                if (mProperties[i] != null)
                    mProperties[i].ReleaseObject();
                mProperties[i] = PropertyCollection.PropertyDescPool.QueryObjectSync();
                mProperties[i].CopyFrom(descCollection.mProperties[i]);
            }
            Count = descCollection.Count;
        }
    }

    public class PropertyCollection
    {
        public static UPooledObject<CustomPropertyDescriptor> PropertyDescPool = new UPooledObject<CustomPropertyDescriptor>();
        public static UPooledObject<CustomPropertyDescriptorCollection> PropertyDescCollectionPool = new UPooledObject<CustomPropertyDescriptorCollection>();

        public static Dictionary<string, CustomPropertyDescriptorCollection> CollectionProperties(object instance, bool withCategoryGroup, bool parentIsValueType)
        {
            Dictionary<string, CustomPropertyDescriptorCollection> categoryGroups = new Dictionary<string, CustomPropertyDescriptorCollection>();
            CustomPropertyDescriptorCollection properties = null;
            CustomPropertyDescriptorCollection fields = null;
            var getFieldsFlag = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            if(instance != null)
            {
                var enumrableInterface = instance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    // 多个对象
                    foreach (var objIns in (IEnumerable)instance)
                    {
                        if (objIns == null)
                            continue;

                        var objType = objIns.GetType();
                        var objTypeDesc = Rtti.UTypeDesc.TypeOf(objType);
                        PropertyDescriptorCollection pros;
                        var tc = TypeDescriptor.GetConverter(objIns);
                        if (tc == null || !tc.GetPropertiesSupported())
                        {
                            if (objIns is ICustomTypeDescriptor)
                                pros = ((ICustomTypeDescriptor)instance).GetProperties();
                            else
                                pros = TypeDescriptor.GetProperties(objIns);
                        }
                        else
                            pros = tc.GetProperties(objIns);
                        var tempProperties = PropertyDescCollectionPool.QueryObjectSync();
                        tempProperties.InitValue(objIns, objTypeDesc, pros, parentIsValueType);

                        // 相同属性合并
                        if (properties == null)
                        {
                            properties = PropertyDescCollectionPool.QueryObjectSync();
                            properties.CopyFrom(tempProperties);
                        }
                        else
                        {
                            for (int i = properties.Count - 1; i >= 0; i--)
                            {
                                var idx = tempProperties.IndexOf(properties[i]);
                                if (idx >= 0)
                                {
                                    //properties[i].Add(tempProperties[idx]);
                                    tempProperties.RemoveAt(idx);
                                }
                                else
                                    properties.RemoveAt(i);
                            }
                        }

                        PropertyDescCollectionPool.ReleaseObject(tempProperties);
                        tempProperties.ReleaseObject(null);

                        var fls = objType.GetFields(getFieldsFlag);
                        var tempFields = PropertyDescCollectionPool.QueryObjectSync();
                        tempFields.InitValue(objIns, objTypeDesc, fls, parentIsValueType);

                        // 相同成员合并
                        if (fields == null)
                        {
                            fields = PropertyDescCollectionPool.QueryObjectSync();
                            fields.CopyFrom(tempFields);
                        }
                        else
                        {
                            for(int i=fields.Count - 1; i>=0; i--)
                            {
                                var idx = tempFields.IndexOf(fields[i]);
                                if (idx >= 0)
                                {
                                    //fields[i].Add(tempFields[idx]);
                                    tempProperties.RemoveAt(idx);
                                }
                                else
                                    fields.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    // 单个对象
                    var insType = instance.GetType();
                    var insTypeDesc = Rtti.UTypeDesc.TypeOf(insType);
                    PropertyDescriptorCollection pros;
                    var tc = TypeDescriptor.GetConverter(instance);
                    if (tc == null || !tc.GetPropertiesSupported())
                    {
                        if (instance is ICustomTypeDescriptor)
                            pros = ((ICustomTypeDescriptor)instance).GetProperties();
                        else
                            pros = TypeDescriptor.GetProperties(instance);
                    }
                    else
                        pros = tc.GetProperties(instance);
                    properties = PropertyDescCollectionPool.QueryObjectSync();
                    properties.InitValue(instance, insTypeDesc, pros, parentIsValueType);

                    var fls = insType.GetFields(getFieldsFlag);
                    fields = PropertyDescCollectionPool.QueryObjectSync();
                    fields.InitValue(instance, insTypeDesc, fls, parentIsValueType);
                }
            }

            if (properties == null)
                properties = PropertyDescCollectionPool.QueryObjectSync();// new CustomPropertyDescriptorCollection(null);
            if (fields == null)
                fields = PropertyDescCollectionPool.QueryObjectSync();// new CustomPropertyDescriptorCollection(null);

            properties.Add(fields);
            properties.Sort(CustomPropertyDescriptorCollection.CompareByDisplayName);

            if(withCategoryGroup)
            {
                for(int i=0; i<properties.Count; i++)
                {
                    CustomPropertyDescriptorCollection categoryPropertyCollection = null;
                    var category = properties[i].Category ?? string.Empty;
                    if(categoryGroups.ContainsKey(category))
                    {
                        categoryPropertyCollection = categoryGroups[category];
                    }
                    else
                    {
                        categoryPropertyCollection = new CustomPropertyDescriptorCollection();
                        categoryGroups[category] = categoryPropertyCollection;
                    }

                    categoryPropertyCollection.Add(properties[i]);
                }
            }
            else
            {
                categoryGroups[string.Empty] = properties;
            }

            properties.ReleaseObject();
            fields.ReleaseObject();

            return categoryGroups;
        }
    }
}
