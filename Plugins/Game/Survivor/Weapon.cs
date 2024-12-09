using System;
using System.Collections.Generic;
using EngineNS;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtWeaponData", HeadRow = 0, DataStartRow = 1)]
    public class TtWeaponData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Name")]
        public string Name { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Level")]
        public int Level { get; set; }
    }
    public class TtWeaponManager : EngineNS.Bricks.DataSet.TtDataManager<TtWeaponData>
    {

    }
}
