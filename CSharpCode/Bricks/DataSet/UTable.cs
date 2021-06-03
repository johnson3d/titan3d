using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    
    public partial class UTable
    {
        public UDataProviderBinder Binder;
        private List<IDataProvider> DataProviders { get; } = new List<IDataProvider>();
        public int Count
        {
            get { return DataProviders.Count; }
        }
        public IDataProvider GetData(int index)
        {
            if (index < 0 || index >= DataProviders.Count)
                return null;
            return DataProviders[index];
        }
        public IDataProvider FindData(object key)
        {
            foreach (var i in DataProviders)
            {
                if (i.DataKey == key)
                    return i;
            }
            return null;
        }
        partial void GetCellText(int row, int col, ref string outText);
        public void CheckSheetLinks(UDataSet dataSet, int index)
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
                    var lst = Rtti.UTypeDescManager.CreateInstance(i.PropInfo.PropertyType) as System.Collections.IList;
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
