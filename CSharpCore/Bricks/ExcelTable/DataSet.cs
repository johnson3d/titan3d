using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.ExcelTable;

namespace EngineNS.Bricks.DataProvider
{
    public partial class GDataSet
    {
        partial void LoadDataSetFromExcel(Type objType, RName name, ref bool result)
        {
            var imp = new ExcelImporter();
            if (false == imp.Init(name.Address))
            {
                result = false;
                return;
            }
            mDataRows = imp.Table2Objects2(objType);
            result = true;
        }
    }
    public partial class DataSet<T>
    {
        partial void LoadDataSetFromExcel(RName name, ref bool result)
        {
            var imp = new ExcelImporter();
            if (false == imp.Init(name.Address))
            {
                result = true;
                return;
            }
            mDataRows = imp.Table2Objects<T>();
            result = true;
        }
    }
}
