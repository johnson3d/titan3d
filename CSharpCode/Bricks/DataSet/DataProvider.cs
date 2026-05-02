using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    public class TtDataProvider : IO.BaseSerializer
    {
        [Category("Excel")]
        [Rtti.Meta("")]
        public int RowInSheet { get; set; }
    }
    public class TtDataTableAttribute : Attribute
    {
        public string SheetName;
        public string KeyName;
        public int HeadRow = -1;
        public int DataStartRow = 0;
    }
    public class TtDataColumnAttribute : Attribute
    {
        public string SheetName;
        public string HeadName;
        public Rtti.TtTypeDesc DataConverter;
    }
    public class TtDataConverter
    {
    }

    public class TtDataProviderBinder
    {
        public class TtDataField
        {
            public string SheetName;
            public string HeadName;
            public int ColumnIndex = -1;
            public System.Reflection.PropertyInfo PropInfo;
            public TtDataConverter Conveter;
        }
        public string SheetName;
        public int HeadRow = -1;
        public int DataStartRow = 0;
        public List<TtDataField> Fields = new List<TtDataField>();
        public TtDataField FindFiled(string name)
        {
            foreach (var fld in Fields)
            {
                if (fld.HeadName == name)
                    return fld;
            }
            return null;
        }
        public bool BuildBinder(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(TtDataTableAttribute), false);
            if (attrs.Length == 0)
                return false;

            var sheet = attrs[0] as TtDataTableAttribute;
            SheetName = sheet.SheetName;
            HeadRow = sheet.HeadRow;
            DataStartRow = sheet.DataStartRow;

            var props = type.GetProperties();
            foreach (var i in props)
            {
                attrs = i.GetCustomAttributes(typeof(TtDataColumnAttribute), false);
                if (attrs.Length == 0)
                    continue;

                var tmp = new TtDataField();
                tmp.PropInfo = i;
                var columns = attrs[0] as TtDataColumnAttribute;
                tmp.HeadName = columns.HeadName;
                if (columns.DataConverter != null)
                {
                    tmp.Conveter = Rtti.TtTypeDescManager.CreateInstance(columns.DataConverter) as TtDataConverter;
                }
                if (i.PropertyType.IsSubclassOf(typeof(TtDataProvider)))
                {
                    attrs = i.PropertyType.GetCustomAttributes(typeof(TtDataTableAttribute), false);
                    if (attrs.Length != 0)
                    {
                        var sheet1 = attrs[0] as TtDataTableAttribute;
                        tmp.SheetName = sheet1.SheetName;
                    }
                }
                else if (i.PropertyType.IsGenericType && i.PropertyType.GetInterface("IList") != null)
                {
                    var elemType = i.PropertyType.GetGenericArguments()[0];
                    attrs = elemType.GetCustomAttributes(typeof(TtDataTableAttribute), false);
                    if (attrs.Length != 0)
                    {
                        var sheet1 = attrs[0] as TtDataTableAttribute;
                        tmp.SheetName = sheet1.SheetName;
                    }
                }
                Fields.Add(tmp);
            }
            return true;
        }
    }

    public class TtDataProviderBinderManager
    {
        internal Dictionary<Type, TtDataProviderBinder> Binders = new Dictionary<Type, TtDataProviderBinder>();
        public TtDataProviderBinder GetBinder(Type t)
        {
            TtDataProviderBinder binder;
            if (Binders.TryGetValue(t, out binder))
            {
                return binder;
            }

            binder = new TtDataProviderBinder();
            if (binder.BuildBinder(t) == false)
                return null;
            Binders.Add(t, binder);
            return binder;
        }
        public void CollectSheetTypes(Type type, List<Type> outTypes)
        {
            if (outTypes.Contains(type))
                return;
            
            var binder = GetBinder(type);
            if (binder == null)
                return;

            outTypes.Add(type);
            foreach (var i in binder.Fields)
            {
                if (i.SheetName != null)
                {
                    CollectSheetTypes(i.PropInfo.PropertyType, outTypes);
                }
            }
        }
    }
}
