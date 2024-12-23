using EngineNS.Support;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    
    public partial class TtTable : IO.BaseSerializer
    {
        public TtDataProviderBinder Binder;
        [Rtti.Meta]
        private List<TtDataProvider> DataProviders { get; set; } = new List<TtDataProvider>();
        private Dictionary<string, List<TtDataProvider>> SortedDataProviders { get; } = new Dictionary<string, List<TtDataProvider>>();
        public int Count
        {
            get { return DataProviders.Count; }
        }
        public TtDataProvider GetData(int index)
        {
            if (index < 0 || index >= DataProviders.Count)
                return null;
            return DataProviders[index];
        }
        protected static int CmpKeyEqual(Type objType, object lh, object rh)
        {
            switch (objType.FullName)
            {
                case "System.String":
                    return ((string)lh).CompareTo((string)rh);
                case "System.Int32":
                    return ((System.Int32)lh).CompareTo((System.Int32)rh);
                case "System.UInt32":
                    return ((System.UInt32)lh).CompareTo((System.UInt32)rh);
                default:
                    System.Diagnostics.Debug.Assert(false);
                    return 0;
            }
        }
        public TtDataProvider FindByKey(string propName, object key, 
            bool bSorted = true,
            [Rtti.MetaParameter(FilterType = typeof(TtDataProvider), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            var prop = DataProviders[0].GetType().GetProperty(propName);
            if (bSorted == false)
            {
                var result = DataProviders.Find((TtDataProvider obj) =>
                {
                    return CmpKeyEqual(prop.PropertyType, prop.GetValue(obj), key) == 0;
                });
                return result;
            }
            else
            {
                List<TtDataProvider> dp;
                if (SortedDataProviders.TryGetValue(propName, out dp) == false)
                {
                    dp = new List<TtDataProvider>();
                    dp.AddRange(DataProviders);
                    dp.Sort((lh, rh) =>
                    {
                        return CmpKeyEqual(prop.PropertyType, prop.GetValue(lh), prop.GetValue(rh));
                    });
                    SortedDataProviders.Add(propName, dp);
                }
                var index = TtBinarySearchExtension.BinarySearch<TtDataProvider, object>(dp, key, (lh, rh)=>
                {
                    return CmpKeyEqual(prop.PropertyType, prop.GetValue(lh), rh);
                });
                if (index < 0)
                    return null;
                return dp[index];
            }
        }
        public TtDataProvider FindByKey<KeyType>(string propName, KeyType key,
            bool bSorted = true) where KeyType : IComparable<KeyType>
        {
            var prop = DataProviders[0].GetType().GetProperty(propName);
            if (bSorted == false)
            {
                var result = DataProviders.Find((TtDataProvider obj) =>
                {
                    return ((KeyType)prop.GetValue(obj)).CompareTo(key) == 0;
                });
                return result;
            }
            else
            {
                List<TtDataProvider> dp;
                if (SortedDataProviders.TryGetValue(propName, out dp) == false)
                {
                    dp = new List<TtDataProvider>();
                    dp.AddRange(DataProviders);
                    dp.Sort((lh, rh) =>
                    {
                        return ((KeyType)prop.GetValue(lh)).CompareTo((KeyType)prop.GetValue(rh));
                    });
                    SortedDataProviders.Add(propName, dp);
                }
                var index = TtBinarySearchExtension.BinarySearch<TtDataProvider, KeyType>(dp, key, (lh, rh) =>
                {
                    return ((KeyType)prop.GetValue(lh)).CompareTo(rh);
                });
                if (index < 0)
                    return null;
                return dp[index];
            }
        }
        partial void GetCellText(int row, int col, ref string outText);
        public void CheckSheetLinks(TtDataSet dataSet, int index)
        {
            var data = GetData(index);
            if (data == null)
                return;

            var binder = dataSet.BinderManager.GetBinder(data.GetType());
            foreach (var i in binder.Fields)
            {
                if (i.SheetName == null)
                    continue;

                string linkInfo = null;
                GetCellText(index, i.ColumnIndex, ref linkInfo);
                if (string.IsNullOrEmpty(linkInfo))
                    continue;

                var refTab = dataSet.GetTable(i.SheetName);

                if (i.PropInfo.PropertyType.IsGenericType && i.PropInfo.PropertyType.GetInterface("IList") != null)
                {
                    var elemType = i.PropInfo.PropertyType.GetGenericArguments()[0];
                    
                    var segs = linkInfo.Split(',');
                    var lst = Rtti.TtTypeDescManager.CreateInstance(i.PropInfo.PropertyType) as System.Collections.IList;
                    foreach (var j in segs)
                    {
                        var k = System.Convert.ToInt32(j);
                        var tObj = refTab.GetData(k);
                        lst.Add(tObj);
                    }
                    i.PropInfo.SetValue(data, lst);
                }
                else
                {
                    var refIndex = System.Convert.ToInt32(linkInfo);
                    var refObj = refTab.GetData(refIndex);
                    i.PropInfo.SetValue(data, refObj);
                    if (refObj != null)
                    {
                        refTab.CheckSheetLinks(dataSet, refIndex);
                    }
                }
            }
        }

    }
}
