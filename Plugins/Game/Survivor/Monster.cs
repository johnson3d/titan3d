using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.DesignMacross.Base.Description;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtMonsterData", HeadRow = 0, DataStartRow = 1)]
    public class TtMonsterData : EngineNS.Bricks.DataSet.TtDataProvider
    {

        public float Speed = 5;
        public float AttackRange = 1.5f;
        public float Health = 100;
    }

    public class TtMonsterManager : EngineNS.Bricks.DataSet.TtDataManager<TtMonsterData>
    {

    }
}
