using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    public partial class UDataSet
    {
        public UDataProviderBinderManager BinderManager = new UDataProviderBinderManager();
        public Dictionary<string, UTable> Tables { get; } = new Dictionary<string, UTable>();
        public UTable MainTable;
        public bool LoadDataSet(RName name, Type objType)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(name.Address);
            if (fs == null)
                return false;

            var workbook = new NPOI.HSSF.UserModel.HSSFWorkbook(fs);
            return LoadDataSetFromExcel(workbook, objType);
        }
        public UTable GetTable(string name)
        {
            UTable result;
            if (Tables.TryGetValue(name, out result))
                return result;
            return null;
        }
        private void CheckSheetLinks()
        {
            if (MainTable == null)
                return;
            for (int i = 0; i < MainTable.Count; i++)
            {
                MainTable.CheckSheetLinks(this, i);
            }
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_UDataSet
    {
        [Bricks.DataSet.UDataTable(SheetName = "TestDataType")]
        public class TestDataType : Bricks.DataSet.IDataProvider
        {
            public object DataKey 
            { 
                get
                {
                    return A;
                }
            }
            public int RowInSheet { get; set; }
            [Bricks.DataSet.UDataColumn(ColumeIndex = 0)]
            public int A { get; set; }
            [Bricks.DataSet.UDataColumn(ColumeIndex = 1)]
            public float B { get; set; }
            [Bricks.DataSet.UDataColumn(ColumeIndex = 2)]
            public string C { get; set; }

            [Bricks.DataSet.UDataTable(SheetName = "SubDataType")]
            public class SubDataType : Bricks.DataSet.IDataProvider
            {
                public object DataKey
                {
                    get
                    {
                        return A;
                    }
                }
                public int RowInSheet { get; set; }
                [Bricks.DataSet.UDataColumn(ColumeIndex = 0)]
                public int A { get; set; }
                [Bricks.DataSet.UDataColumn(ColumeIndex = 1)]
                public float B { get; set; }
            }
            [Bricks.DataSet.UDataColumn(ColumeIndex = 3)]
            public SubDataType D { get; set; }
            [Bricks.DataSet.UDataColumn(ColumeIndex = 4)]
            public List<int> E { get; set; }
        }
        public void UnitTestEntrance()
        {
            var dataSet = new Bricks.DataSet.UDataSet();
            dataSet.LoadDataSet(RName.GetRName("UTest/dataset/testdatatype.xls"), typeof(TestDataType));

            var obj = dataSet.MainTable.GetData(0) as TestDataType;
            UTest.UnitTestManager.TAssert(obj.A == 1, "");

            //dataSet.SaveDataSetToExcel(RName.GetRName("UTest/dataset/testdatatype_1.xls").Address);
        }
    }
}