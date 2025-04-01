using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtDropGroup", KeyName = "DropGroupId", HeadRow = 0, DataStartRow = 3)]
    public class TtDropGroup : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "DropGroupId")]
        public int DropGroupId { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "DropGroupName")]
        public string DropGroupName { get; set; }

        [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtDropItem", KeyName = "ItemId", HeadRow = 0, DataStartRow = 3)]
        public class TtDropItem : EngineNS.Bricks.DataSet.TtDataProvider
        {
            [EngineNS.Rtti.Meta]
            [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemId")]
            public int ItemId { get; set; }
            [EngineNS.Rtti.Meta]
            [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Count")]
            public int Count { get; set; }
            [EngineNS.Rtti.Meta]
            [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "DropProbability")]
            public float DropProbability { get; set; } = 1.0f;
        }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "DropItems")]
        public List<TtDropItem> DropItems { get; set; } = new List<TtDropItem>();
    }
}
