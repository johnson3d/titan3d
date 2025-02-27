using System;
using System.Collections.Generic;
using EngineNS;
using EngineNS.DesignMacross.Base.Description;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtWeaponData", KeyName = "ItemId", HeadRow = 0, DataStartRow = 3)]
    public class TtWeaponData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemId")]
        public int  ItemId {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemType")]
        public int ItemType {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemName")]
        public string ItemName {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Level")]
        public int Level {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "LevelMax")]
        public int LevelMax {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Skill")]
        public int Skill {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Damage")]
        public float Damage {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Shape")]
        public string Shape {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "AttackRange")]
        public float AttackRange {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "LaunchCount")]
        public int LaunchCount {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Penetration")]
        public int Penetration {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "CoolDown")]
        public float CoolDown {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "AttackInterval")]
        public float AttackInterval {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Duration")]
        public float Duration {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "HitRate")]
        public float HitRate {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Knockback")]
        public bool Knockback {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Description")]
        public string Description {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Unlock")]
        public string Unlock {get;set;}
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Rarity")]
        public int Rarity { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ProjectileSpeed")]
        public float ProjectileSpeed { get; set; }

    }

    public class TtWeaponManager : EngineNS.Bricks.DataSet.TtDataManager<TtWeaponData>
    {

    }
}
