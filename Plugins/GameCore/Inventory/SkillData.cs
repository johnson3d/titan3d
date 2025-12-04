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
        public class TtSkillEffect
        {
            public bool Tick(IHostActor actor, TtSkill skill)
            {
                return true;
            }
        }
        public List<TtSkillEffect> SkillEffects = new List<TtSkillEffect>();
        public void Spell(TtItem item, TtNode user)
        {
            var se = OnSpell(item, user);
            if (se != null)
            {
                SkillEffects.Add(se);
            }
        }
        public void Tick(IHostActor actor)
        {
            for (int i = 0; i<SkillEffects.Count; i++)
            {
                var s = SkillEffects[i];
                if (s.Tick(actor, this) == false)
                {
                    SkillEffects.RemoveAt(i);
                    i--;
                }
            }
        }
        public virtual TtSkillEffect OnSpell(TtItem item, TtNode user)
        {
            return null;
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
        public void Tick(IHostActor actor)
        {
            if (Items==null)
                return;
            foreach(var i in Items)
            {
                if (i==null || i.Skill==null)
                    continue;
                i.Skill.Tick(actor);
            }
        }
    }

    public class TtTeleportSkill : TtSkill
    {
        public override TtSkillEffect OnSpell(TtItem item, TtNode user)
        {
            //user.Placement.Position = xxxx
            return null;
        }
    }
}
