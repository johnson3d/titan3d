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
