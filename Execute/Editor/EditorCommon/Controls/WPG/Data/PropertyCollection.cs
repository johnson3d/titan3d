using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WPG.Data
{
    public class PropertyCollection : CompositeItem
    {
        #region Initialization

        public PropertyCollection() { }

        //public PropertyCollection(object instance)
        //    : this(instance, false)
        //{ }
        static Dictionary<Type, EditorCommon.CustomPropertyDescriptorCollection> mPropertyDesColDic = new Dictionary<Type, EditorCommon.CustomPropertyDescriptorCollection>();
        static Dictionary<Type, EditorCommon.CustomPropertyDescriptorCollection> mFieldInfosDic = new Dictionary<Type, EditorCommon.CustomPropertyDescriptorCollection>();

        public static void RemoveCache(Type type)
        {
            if (type != null)
            {
                mPropertyDesColDic.Remove(type);
                mFieldInfosDic.Remove(type);
            }
        }

        public PropertyCollection(object instance, bool noCategory, bool alphabetical, bool automaticlyExpandObjects, string filter, Property parentProperty, bool parentIsValueType, PropertyGrid hostGrid, bool mergeMultiValue = false)
        {
            //bool useCustomTypeConverter = false;
            List<EditorCommon.CustomPropertyDescriptorCollection> propertyCollections = new List<EditorCommon.CustomPropertyDescriptorCollection>();
            List<EditorCommon.CustomPropertyDescriptorCollection> fieldCollections = new List<EditorCommon.CustomPropertyDescriptorCollection>();
            EditorCommon.CustomPropertyDescriptorCollection properties = null;
            EditorCommon.CustomPropertyDescriptorCollection fields = null;
            System.Reflection.BindingFlags getFieldsFlag = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            if (instance != null)
            {
                var enumrableInterface = instance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    // 显示多个对象
                    foreach (var objIns in (IEnumerable)instance)
                    {
                        if (objIns == null)
                            continue;
                        var objType = objIns.GetType();
                        EditorCommon.CustomPropertyDescriptorCollection tempProperties;
                        if (!mPropertyDesColDic.TryGetValue(objType, out tempProperties))
                        {
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
                            {
                                pros = tc.GetProperties(objIns);
                            }
                            tempProperties = new EditorCommon.CustomPropertyDescriptorCollection(objType, pros, parentIsValueType);
                            mPropertyDesColDic[objType] = tempProperties;
                            if(objType.Name == "CGfxMeshImportOption")
                            {
                                int s = 0;
                                s = 1;
                            }
                        }
                        if (objType.Name == "CGfxMeshImportOption")
                        {
                            int s = 0;
                            s = 1;
                        }
                        if(mergeMultiValue)
                        {
                            if (properties == null)
                            {
                                var pros = new EditorCommon.CustomPropertyDescriptor[tempProperties.Count];
                                tempProperties.CopyTo(pros, 0);
                                properties = new EditorCommon.CustomPropertyDescriptorCollection(pros);
                                //properties = tempProperties;
                            }
                            else if (properties.Properties == tempProperties.Properties)
                            {
                                for (int i = properties.Count - 1; i >= 0; i--)
                                {
                                    var idx = tempProperties.IndexOf(properties[i]);
                                    if (idx >= 0)
                                    {
                                        var item = tempProperties[idx];
                                        properties[i].Add(item);
                                    }
                                    else
                                        properties.RemoveAt(i);
                                }
                            }
                        }
                        else
                            propertyCollections.Add(tempProperties);
                        

                        EditorCommon.CustomPropertyDescriptorCollection tempFields;
                        if (!mFieldInfosDic.TryGetValue(objType, out tempFields))
                        {
                            var fls = objType.GetFields(getFieldsFlag);
                            tempFields = new EditorCommon.CustomPropertyDescriptorCollection(objType, fls, parentIsValueType);
                            mFieldInfosDic[objType] = tempFields;
                        }
                        if (mergeMultiValue)
                        {
                            if (fields == null)
                                fields = tempFields;
                            else
                            {
                                for (int i = fields.Count - 1; i >= 0; i--)
                                {
                                    var idx = tempFields.IndexOf(fields[i]);
                                    if (idx >= 0)
                                    {
                                        var item = tempFields[idx];
                                        fields[i].Add(item);
                                    }
                                    else
                                        fields.RemoveAt(i);
                                }
                            }
                        }
                        else
                            fieldCollections.Add(tempFields);
                    }
                }
                else
                {
                    // 显示单个对象
                    var insType = instance.GetType();
                    if (!mPropertyDesColDic.TryGetValue(insType, out properties))
                    {
                        PropertyDescriptorCollection pros;
                        TypeConverter tc = TypeDescriptor.GetConverter(instance);
                        if (tc == null || !tc.GetPropertiesSupported())
                        {
                            if (instance is ICustomTypeDescriptor)
                                pros = ((ICustomTypeDescriptor)instance).GetProperties();
                            else
                                pros = TypeDescriptor.GetProperties(insType);  //I changed here from instance to instance.GetType, so that only the Direct Properties are shown!
                        }
                        else
                        {
                            pros = tc.GetProperties(instance);
                            //useCustomTypeConverter = true;
                        }
                        properties = new EditorCommon.CustomPropertyDescriptorCollection(insType, pros, parentIsValueType);
                        mPropertyDesColDic[insType] = properties;
                        if (insType.Name == "CGfxMeshImportOption")
                        {
                            int s = 0;
                            s = 1;
                        }
                    }

                    if (!mFieldInfosDic.TryGetValue(insType, out fields))
                    {
                        var flds = insType.GetFields(getFieldsFlag);
                        fields = new EditorCommon.CustomPropertyDescriptorCollection(insType, flds, parentIsValueType);
                        mFieldInfosDic[insType] = fields;
                    }
                }
            }

            if (properties == null)
                properties = new EditorCommon.CustomPropertyDescriptorCollection(new EditorCommon.CustomPropertyDescriptor[] { });
            if (fields == null)
                fields = new EditorCommon.CustomPropertyDescriptorCollection(new EditorCommon.CustomPropertyDescriptor[] { });

            if (mergeMultiValue || instance == null)
            {
                List<Property> propertyCollection = new List<Property>();
                Dictionary<string, PropertyCategory> groups = new Dictionary<string, PropertyCategory>();
                foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in properties)
                {
                    CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                }
                foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in fields)
                {
                    CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                }

                ProcessPropertyCollection(parentProperty, propertyCollection, noCategory, alphabetical, filter, groups, Items);
            }
            else
            {
                var enumrableInterface = instance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    int idxShow = 1;  // 从1开始算
                    foreach (var objIns in (IEnumerable)instance)
                    {
                        Dictionary<string, PropertyCategory> groups = new Dictionary<string, PropertyCategory>();
                        List<Property> tempProCollection = new List<Property>();
                        if(propertyCollections.Count >= idxShow)
                        {
                            var pros = propertyCollections[idxShow - 1];
                            foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in pros)
                            {
                                CollectProperties(objIns, propertyDescriptor, tempProCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                            }
                            var field = fieldCollections[idxShow - 1];
                            foreach (EditorCommon.CustomPropertyDescriptor fieldDescriptor in field)
                            {
                                CollectProperties(objIns, fieldDescriptor, tempProCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                            }
                            var atts = objIns.GetType().GetCustomAttributes(typeof(EngineNS.Editor.Editor_DisplayNameInEnumerable), true);
                            var nameString = idxShow.ToString();
                            if (atts.Length > 0)
                            {
                                var att = atts[0] as EngineNS.Editor.Editor_DisplayNameInEnumerable;
                                nameString += " " + att.DisplayName;
                            }
                            var category = new PropertyCategory(nameString);
                            ProcessPropertyCollection(parentProperty, tempProCollection, noCategory, alphabetical, filter, groups, category.Items);
                            Items.Add(category);
                            idxShow++;
                        }
                    }
                }
                else
                {
                    List<Property> propertyCollection = new List<Property>();
                    Dictionary<string, PropertyCategory> groups = new Dictionary<string, PropertyCategory>();
                    foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in properties)
                    {
                        CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                    }
                    foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in fields)
                    {
                        CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, mergeMultiValue);
                    }

                    ProcessPropertyCollection(parentProperty, propertyCollection, noCategory, alphabetical, filter, groups, Items);
                }
            }
        }

        void ProcessPropertyCollection(Property parentProperty, List<Property> propertyCollection, bool noCategory, bool alphabetical, string filter, Dictionary<string, PropertyCategory> groups, ObservableCollection<Item> items)
        {
            EditorCommon.Editor_PropertyGridSortTypeAttribute sortAtt = null;
            if (parentProperty != null)
            {
                foreach (var valueAtt in parentProperty.ValueAttributes)
                {
                    var att = valueAtt as EditorCommon.Editor_PropertyGridSortTypeAttribute;
                    if (att != null)
                    {
                        sortAtt = att;
                        break;
                    }
                }
            }

            // 筛选GPlacementComponent
            var placementCompType = typeof(EngineNS.GamePlay.Component.GPlacementComponent);
            List<Property> placementPros = new List<Property>();
            for (int i = propertyCollection.Count - 1; i >= 0; i--)
            {
                var pro = propertyCollection[i];
                if (pro.PropertyType == placementCompType ||
                   pro.PropertyType.IsSubclassOf(placementCompType))
                {
                    placementPros.Add(pro);
                    propertyCollection.RemoveAt(i);
                }
            }

            if (noCategory)
            {
                if (sortAtt != null)
                {
                    switch (sortAtt.SortType)
                    {
                        case EditorCommon.Editor_PropertyGridSortTypeAttribute.enSortType.NoSort:
                            break;
                        case EditorCommon.Editor_PropertyGridSortTypeAttribute.enSortType.Custom:
                            propertyCollection.Sort(sortAtt.Comparer);
                            break;
                    }
                }
                else
                {
                    if (alphabetical)
                        propertyCollection.Sort(Property.CompareByName);
                    else
                        propertyCollection.Sort(Property.CompareBySortIndex);
                }

                foreach (var pro in placementPros)
                {
                    if (filter == "" || pro.Name.ToLower().Contains(filter))
                        items.Add(pro);
                }
                foreach (Property property in propertyCollection)
                {
                    if (filter == "" || property.Name.ToLower().Contains(filter))
                        items.Add(property);
                }
            }
            else
            {
                if (sortAtt != null)
                {
                    switch (sortAtt.SortType)
                    {
                        case EditorCommon.Editor_PropertyGridSortTypeAttribute.enSortType.NoSort:
                            break;
                        case EditorCommon.Editor_PropertyGridSortTypeAttribute.enSortType.Custom:
                            propertyCollection.Sort(sortAtt.Comparer);
                            break;
                    }
                }
                else
                {
                    if (alphabetical)
                        propertyCollection.Sort(Property.CompareByCategoryThenByName);
                    else
                        propertyCollection.Sort(Property.CompareBySortIndex);
                }

                foreach (var pro in placementPros)
                {
                    if (filter == "" || pro.Name.ToLower().Contains(filter))
                        items.Add(pro);
                }
                foreach (Property property in propertyCollection)
                {
                    if (filter == "" || property.Name.ToLower().Contains(filter))
                    {
                        PropertyCategory propertyCategory = null;
                        var category = property.Category ?? string.Empty; // null category handled here

                        if (groups.ContainsKey(category))
                        {
                            propertyCategory = groups[category];
                        }
                        else
                        {
                            var noCa = false;
                            foreach (var att in property.PGProperty.Attributes)
                            {
                                if (att is EngineNS.Editor.Editor_NoCategoryAttribute)
                                {
                                    noCa = true;
                                    break;
                                }
                            }
                            if(noCa)
                            {
                                items.Add(property);
                            }
                            else
                            {
                                propertyCategory = new PropertyCategory(property.Category);
                                groups[category] = propertyCategory;
                                items.Add(propertyCategory);
                            }
                        }
                        propertyCategory?.Items.Add(property);
                    }
                }
            }
        }

        void CollectPropertiesWithInstance(object instance, EditorCommon.CustomPropertyDescriptor descriptor, List<Property> propertyCollection, bool noCategory,
                                        bool alphabetical, bool automaticlyExpandObjects, string filter, Property parentProperty, bool parentIsValueType, PropertyGrid hostGrid)
        {
            bool useCustomCtrl = false;
            bool showInnerProperties = false;
            var insType = instance.GetType();
            var enumrableInterface = insType.GetInterface(typeof(IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                foreach(var insObj in (IEnumerable)instance)
                {
                    foreach (var att in descriptor.Attributes)
                    {
                        if (att is EngineNS.Editor.Editor_CustomEditorAttribute)
                        {
                            useCustomCtrl = true;
                        }
                        if (att is EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute)
                        {
                            if (descriptor.PropertyProvider == null)
                            {
                                var pgUIAtt = (EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute)att;
                                var ins = System.Activator.CreateInstance(pgUIAtt.PropertyGridUIProviderType) as EngineNS.Editor.Editor_PropertyGridUIProvider;
                                var proIns = descriptor.GetValue(insObj, false);
                                descriptor.SetPropertyProvider(ins, proIns);
                            }
                            useCustomCtrl = descriptor.PropertyProvider.UseCustomCtrl;
                        }
                        else if(att is EngineNS.Editor.Editor_ShowOnlyInnerProperties)
                        {
                            showInnerProperties = true;
                            useCustomCtrl = false;
                        }
                        else if (att is EngineNS.Editor.Editor_CustomEditorAttribute)
                        {
                            useCustomCtrl = true;
                        }
                    }
                    break;
                }
            }
            else
            {
                foreach (var att in descriptor.Attributes)
                {
                    if (att is EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute)
                    {
                        if (descriptor.PropertyProvider == null)
                        {
                            var pgUIAtt = (EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute)att;
                            var ins = System.Activator.CreateInstance(pgUIAtt.PropertyGridUIProviderType) as EngineNS.Editor.Editor_PropertyGridUIProvider;
                            var proIns = descriptor.GetValue(instance, false);
                            descriptor.SetPropertyProvider(ins, proIns);
                        }
                        useCustomCtrl = descriptor.PropertyProvider.UseCustomCtrl;
                    }
                    else if (att is EngineNS.Editor.Editor_ShowOnlyInnerProperties)
                    {
                        showInnerProperties = true;
                        useCustomCtrl = false;
                    }
                    else if (att is EngineNS.Editor.Editor_CustomEditorAttribute)
                        useCustomCtrl = true;
                }
            }

            //Add a property with Name: AutomaticlyExpandObjects
            if (useCustomCtrl)
            {
                var property = new Property(instance, descriptor, parentProperty, hostGrid);
                propertyCollection.Add(property);
            }
            else if(showInnerProperties)
            {
                CollectProperties(instance, descriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, false, false);
            }
            else
            {
                Type propertyType = descriptor.GetPropertyType(instance);
                if (propertyType == typeof(EngineNS.GamePlay.Component.GPlacementComponent) ||
                    propertyType.IsSubclassOf(typeof(EngineNS.GamePlay.Component.GPlacementComponent)))
                {
                    var property = new Property_HideName(instance, descriptor, parentProperty, hostGrid);
                    propertyCollection.Add(property);
                }
                else if (propertyType.IsClass && !propertyType.IsArray && propertyType != typeof(string) &&
                    (propertyType.GetInterface(typeof(IEnumerable).FullName) == null))
                {
                    var property = new Property_Class(instance, descriptor, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, hostGrid);
                    propertyCollection.Add(property);
                    //if(automaticlyExpandObjects)
                    //    propertyCollection.Add(new ExpandableProperty(instance, descriptor, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, hostGrid));
                    //else
                    //{
                    //    var property = new Property(instance, descriptor, parentProperty, hostGrid);
                    //    propertyCollection.Add(property);
                    //}
                }
                else if (propertyType.GetInterface(typeof(IEnumerable).FullName) != null && propertyType != typeof(string))
                {
                    // List等如果要用自定义属性框控件就要打上EngineNS.Editor.Editor_CustomEditorAttribute
                    propertyCollection.Add(new ExpandableProperty(instance, descriptor, true, alphabetical, automaticlyExpandObjects, filter, parentProperty, hostGrid));
                }
                else if (descriptor.Converter != null && descriptor.Converter.GetType() == typeof(ExpandableObjectConverter))
                {
                    propertyCollection.Add(new ExpandableProperty(instance, descriptor, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, hostGrid));
                }
                else if(automaticlyExpandObjects && propertyType.IsValueType && !propertyType.IsPrimitive && propertyType != typeof(string) && !propertyType.IsEnum)
                {
                    propertyCollection.Add(new ExpandableProperty(instance, descriptor, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, hostGrid));
                }
                else
                {
                    var property = new Property(instance, descriptor, parentProperty, hostGrid);
                    propertyCollection.Add(property);
                }
            }
        }
        private void CollectProperties(object instance, EditorCommon.CustomPropertyDescriptor descriptor, List<Property> propertyCollection, bool noCategory, 
                                        bool alphabetical, bool automaticlyExpandObjects, string filter, Property parentProperty, bool parentIsValueType, PropertyGrid hostGrid, bool mergeMultiValue, bool check = true)
        {
            if (descriptor.Attributes[typeof(FlatAttribute)] == null && check)
            {
                if (descriptor.IsBrowsable)
                {
                    if(mergeMultiValue)
                        CollectPropertiesWithInstance(instance, descriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid);
                    else
                    {
                        var insType = instance.GetType();
                        var enumrableInterface = insType.GetInterface(typeof(IEnumerable).FullName, false);
                        if (enumrableInterface != null)
                        {
                            foreach (var objIns in (IEnumerable)instance)
                            {
                                CollectPropertiesWithInstance(objIns, descriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid);
                            }
                        }
                        else
                        {
                            CollectPropertiesWithInstance(instance, descriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid);
                        }
                    }

                }
            }
            else
            {
                instance = descriptor.GetValue(instance);
                if (instance == null)
                    return;
                var insType = instance.GetType();
                var enumrableInterface = insType.GetInterface(typeof(IEnumerable).FullName, false);
                if (enumrableInterface != null)
                {
                    if(mergeMultiValue)
                    {
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                        foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in properties)
                        {
                            CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, false);
                        }
                    }
                    else
                    {
                        foreach (var objIns in (IEnumerable)instance)
                        {
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                            foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in properties)
                            {
                                CollectProperties(objIns, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, false);
                            }
                        }
                    }
                }
                else
                {
                    var properties = new EditorCommon.CustomPropertyDescriptorCollection(insType, TypeDescriptor.GetProperties(instance), parentIsValueType);
                    foreach (EditorCommon.CustomPropertyDescriptor propertyDescriptor in properties)
                    {
                        CollectProperties(instance, propertyDescriptor, propertyCollection, noCategory, alphabetical, automaticlyExpandObjects, filter, parentProperty, parentIsValueType, hostGrid, false);
                    }
                }
            }
        }

        #endregion
    }
}
