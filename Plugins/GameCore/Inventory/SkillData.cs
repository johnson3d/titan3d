using MathNet.Numerics.Differentiation;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;

namespace Inventory
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtSkillData", KeyName = "SkillId", HeadRow = 0, DataStartRow = 3)]
    public class TtSkillData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        public TtSkillData()
        {
            SkillType = nameof(TtItem);
        }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "SkillId")]
        public int ItemId { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "SkillName")]
        public string ItemName { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "SkillType")]
        public string SkillType { get; set; }
    }
    public class TtSkill
    {
        public TtSkillData Data;
        public virtual void Spell(TtItem item, TtNode user)
        {

        }
    }

    public class TtSkillInventory : TtInventory
    {
        public override bool CanPutIn(TtItem item, int index)
        {
            if (item.Skill == null)
                return false;
            return true;
        }
    }

    public class TtTeleportSkill : TtSkill
    {
        public override void Spell(TtItem item, TtNode user)
        {
            //user.Placement.Position = xxxx
        }
    }
}
