using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    public interface IDataProvider
    {
        object DataKey { get; }
        int RowInSheet { get; set; }
    }
    public class UDataTableAttribute : Attribute
    {
        public string SheetName;
    }
    public class UDataColumnAttribute : Attribute
    {
        public string SheetName;
        //public string ColumnName;
        public int ColumeIndex = -1;
        public Rtti.UTypeDesc DataConverter;
    }
    public class UDataConverter
    {
    }

    public class UDataProviderBinder
    {
        public class UDataField
        {
            public string SheetName;
            //public string Name;
            public int ColumnIndex = -1;
            public System.Reflection.PropertyInfo PropInfo;
            public UDataConverter Conveter;
        }
        public string SheetName;
        public List<UDataField> Fields = new List<UDataField>();
        public bool BuildBinder(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(UDataTableAttribute), false);
            if (attrs.Length == 0)
                return false;

            var sheet = attrs[0] as UDataTableAttribute;
            SheetName = sheet.SheetName;

            var props = type.GetProperties();
            foreach (var i in props)
            {
                attrs = i.GetCustomAttributes(typeof(UDataColumnAttribute), false);
                if (attrs.Length == 0)
                    continue;

                var tmp = new UDataField();
                tmp.PropInfo = i;
                var columns = attrs[0] as UDataColumnAttribute;
                tmp.ColumnIndex = columns.ColumeIndex;
                if (columns.DataConverter != null)
                {
                    tmp.Conveter = Rtti.UTypeDescManager.CreateInstance(columns.DataConverter.SystemType) as UDataConverter;
                }
                if (i.PropertyType.GetInterface("IDataProvider") != null)
                {
                    attrs = i.PropertyType.GetCustomAttributes(typeof(UDataTableAttribute), false);
                    if (attrs.Length != 0)
                    {
                        var sheet1 = attrs[0] as UDataTableAttribute;
                        tmp.SheetName = sheet1.SheetName;
                    }
                }
                else if (i.PropertyType.IsGenericType && i.PropertyType.GetInterface("IList") != null)
                {
                    var elemType = i.PropertyType.GetGenericArguments()[0];
                    attrs = elemType.GetCustomAttributes(typeof(UDataTableAttribute), false);
                    if (attrs.Length != 0)
                    {
                        var sheet1 = attrs[0] as UDataTableAttribute;
                        tmp.SheetName = sheet1.SheetName;
                    }
                }
                Fields.Add(tmp);
            }
            return true;
        }
    }

    public class UDataProviderBinderManager
    {
        private Dictionary<Type, UDataProviderBinder> Binders = new Dictionary<Type, UDataProviderBinder>();
        public UDataProviderBinder GetBinder(Type t)
        {
            UDataProviderBinder binder;
            if (Binders.TryGetValue(t, out binder))
            {
                return binder;
            }

            binder = new UDataProviderBinder();
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
