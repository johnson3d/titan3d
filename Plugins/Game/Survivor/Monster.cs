using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.DesignMacross.Base.Description;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtMonsterData", HeadRow = 0, DataStartRow = 3)]
    public class TtMonsterData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MonsterId")]
        public int MonsterId { get; set; } = 0;
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MonsterName ")]
        public string MonsterName { get; set; } = "";
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Level")]
        public int Level { get; set; } = 1;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Speed")]
        public float Speed { get; set; } = 1;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "AttackRange")]
        public float AttackRange { get; set; } = 1f;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Health")]
        public float Health { get; set; } = 1;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Damage")]
        public float Damage { get; set; } = 1;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Prefab")]
        public string Prefab { get; set; } = "";
    }

    public class TtMonsterManager : EngineNS.Bricks.DataSet.TtDataManager<TtMonsterData>
    {

    }
}
