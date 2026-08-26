using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.DesignMacross.Base.Description;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtLevelUpData", KeyName = "Level", HeadRow = 0, DataStartRow = 3)]
    public class TtLevelUpData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Level")]
        public int Level { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "TotalXp")]
        public float TotalXp { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "NextLevel")]
        public float NextLevel { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Difference")]
        public float Difference { get; set; }
    }

    public class TtLevelUpDataManager : EngineNS.Bricks.DataSet.TtDataManager<TtLevelUpData>
    {

    }
}
