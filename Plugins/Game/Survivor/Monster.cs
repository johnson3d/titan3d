using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.DesignMacross.Base.Description;

namespace Survivor
{
    public enum EMonsterType
    {
        Enemy,
        Merchant,
        Civilian
    }
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtMonsterData", KeyName = "MonsterId", HeadRow = 0, DataStartRow = 3)]
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
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "MonsterType")]
        public EMonsterType MonsterType { get; set; } = EMonsterType.Enemy;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Missions")]
        public List<int> Missions { get; set; } = new List<int>();// 可接任务
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "SellGoods")]
        public List<int> SellGoods { get; set; } = new List<int>();//售卖物品 
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "DropGroup")]
        public int DropGroupId { get; set; } = -1;
    }

    public class TtMonsterManager : EngineNS.Bricks.DataSet.TtDataManager<TtMonsterData>
    {

    }
}
