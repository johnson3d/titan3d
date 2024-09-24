using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    public partial class TtDataSet
    {
        public UDataProviderBinderManager BinderManager = new UDataProviderBinderManager();
        public Dictionary<string, TtTable> Tables { get; } = new Dictionary<string, TtTable>();
        public TtTable MainTable;
        public bool LoadDataSet(RName name, Type objType)
        {
            bool isOk = false;
            LoadDataSet_Exel(ref isOk, name, objType);
            if (isOk)
            {
                return true;
            }
            //todo: load xnd
            return false;
        }
        public bool LoadFromDatabase(Type objType)
        {
            return false;
        }
        partial void LoadDataSet_Exel(ref bool isOk, RName name, Type objType);
        public TtTable GetTable(string name)
        {
            TtTable result;
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
            var dataSet = new Bricks.DataSet.TtDataSet();
            if(dataSet.LoadDataSet(RName.GetRName("UTest/dataset/testdatatype.xls"), typeof(TestDataType)))
            {
                var obj = dataSet.MainTable.GetData(0) as TestDataType;
                UTest.UnitTestManager.TAssert(obj.A == 1, "");
            }

            //dataSet.SaveDataSetToExcel(RName.GetRName("UTest/dataset/testdatatype_1.xls").Address);
        }
    }
}