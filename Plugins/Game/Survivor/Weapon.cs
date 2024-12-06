using System;
using System.Collections.Generic;
using EngineNS;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtWeaponData")]
    public class TtWeaponData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(ColumeIndex = 0)]
        public string Name { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(ColumeIndex = 1)]
        public int Level { get; set; }
    }
    public class TtWeaponManager : EngineNS.Bricks.DataSet.TtDataManager<TtWeaponData>
    {

    }
}
