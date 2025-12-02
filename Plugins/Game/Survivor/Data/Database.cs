using EngineNS;
using EngineNS.Bricks.DataSet;
using Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    public class TtItemDataManager : EngineNS.Bricks.DataSet.TtDataManager<Inventory.TtItemData>
    {

    }
    public class TtSkillDataManager : EngineNS.Bricks.DataSet.TtDataManager<Inventory.TtSkillData>
    {

    }
    public class TtMissionDataManager : EngineNS.Bricks.DataSet.TtDataManager<TtMissionData>
    {

    }
    public partial class TtDatabase : Inventory.IDataFactory
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
        public TtItemDataManager ItemManager { get; } = new ();
        TtSkillDataManager SkillManager { get; } = new();
        public V GetData<K, V>(K key) where V : class
        {
            if(typeof(V)==typeof(TtWeaponData))
            {
                return GetWeaponData((int)(object)key) as V;
            }
            else if(typeof(V) == typeof(TtRoleData))
            {
                return GetHeroData((int)(object)key) as V;
            }
            else if(typeof(V) == typeof(TtMonsterData))
            {
                return GetMonsterData((int)(object)key) as V;
            }
            else if(typeof(V) == typeof(Inventory.TtItemData))
            {
                return GetItemData((int)(object)key) as V;
            }
            else if(typeof(V) == typeof(Inventory.TtSkillData))
            {
                return GetSkillData((int)(object)key) as V;
            }
            return null;
        }
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
        public Inventory.TtItemData GetItemData(int itemId)
        {
            if (ItemManager == null)
                return null;

            return ItemManager.GetData("ItemId", itemId);
        }
        public Inventory.TtSkillData GetSkillData(int skillId)
        {
            if (SkillManager == null)
                return null;

            return SkillManager.GetData("SkillId", skillId);
        }
    }
}