using EngineNS;
using EngineNS.Bricks.DataSet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    public partial class TtDatabase
    {
        private static TtDatabase mInstance = null;
        public static TtDatabase Instance
        {
            get 
            { 
                if (mInstance == null)
                    mInstance = new TtDatabase();
                return mInstance; 
            }
        }
        TtWeaponDataManager WeaponManager { get; } = new TtWeaponDataManager();
        TtHeroDataManager HeroManager { get; } = new TtHeroDataManager();
        TtMonsterDataManager MonsterManager { get; } = new TtMonsterDataManager();
        public TtItemDataManager ItemManager { get; } = new TtItemDataManager();
        TtSkillDataManager SkillManager { get; } = new TtSkillDataManager();
        public void LoadWeapons(RName name)
        {
            WeaponManager.LoadDataSet(name);
        }
        public void LoadHeros(RName name)
        {
            HeroManager.LoadDataSet(name);
        }
        public void LoadMonsters(RName name)
        {
            MonsterManager.LoadDataSet(name);
        }
        public void LoadItems(RName name)
        {
            ItemManager.LoadDataSet(name);
        }
        public TtWeaponData GetWeaponData(int weaponId)
        {
            if (WeaponManager == null)
                return null;

            return WeaponManager.GetData("ItemId", weaponId);
        }
        public TtRoleData GetHeroData(int roleId)
        {
            if (HeroManager == null)
                return null;

            return HeroManager.GetData("RoleId", roleId);
        }
        public TtMonsterData GetMonsterData(int monsterId)
        {
            if(MonsterManager == null)
                return null;

            return MonsterManager.GetData("MonsterId", monsterId);
        }
        public TtItemData GetItemData(int itemId)
        {
            if (ItemManager == null)
                return null;

            return ItemManager.GetData("ItemId", itemId);
        }
        public TtSkillData GetSkillData(int skillId)
        {
            if (SkillManager == null)
                return null;

            return SkillManager.GetData("SkillId", skillId);
        }
    }
}