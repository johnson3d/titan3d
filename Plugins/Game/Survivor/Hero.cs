using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtHeroData", HeadRow = 0, DataStartRow = 1)]
    public class TtHeroData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Name")]
        public string Name { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Weapon")]
        public string Weapon { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MaxLife")]
        public int MaxLife { get; set; }
    }
    public class TtHeroManager : EngineNS.Bricks.DataSet.TtDataManager<TtHeroData>
    {

    }
}
