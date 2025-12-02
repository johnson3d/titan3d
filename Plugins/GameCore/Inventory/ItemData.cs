using BCnEncoder.Shared;
using EngineNS;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtItemData", KeyName= "ItemId", HeadRow = 0, DataStartRow = 3)]
    public class TtItemData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        public TtItemData()
        {
            ItemType = nameof(TtItem);
        }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemId")]
        public int ItemId { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemName")]
        public string ItemName { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemType")]
        public string ItemType { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "SkillId")]
        public int SkillId { get; set; } = -1;
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "Duration")]
        public int Duration { get; set; } = -1;
    }
    public interface IDataFactory
    {
        public V GetData<K, V>(K key) where V : class;
    }
    public class TtItem : EngineNS.IO.BaseSerializer
    {
        public TtInventory Inventory = null;
        public int IndexInInventory = -1;
        public TtItemData Data = null;
        public TtSkill Skill = null;
        //Save to Database
        public Guid UniqueId = Guid.Empty;

        public static TtItem CreateItem(IDataFactory factory, TtItemData data)
        {
            var type = EngineNS.Rtti.TtTypeDesc.TypeOf($"Survivor.{data.ItemType}@Survivor");
            if (type == null)
            {
                return null;
            }
            var result = EngineNS.Rtti.TtTypeDescManager.CreateInstance(type) as TtItem;
            result.Data = data;
            result.UniqueId = Guid.NewGuid();
            if (data.SkillId >= 0)
            {
                var skillData = factory.GetData<int, TtSkillData>(data.SkillId);
                if (skillData == null)
                    return null;
                type = EngineNS.Rtti.TtTypeDesc.TypeOf($"Survivor.{skillData.SkillType}@Survivor");
                result.Skill = EngineNS.Rtti.TtTypeDescManager.CreateInstance(type) as TtSkill;
            }
            return result;
        }

        public virtual void DestoryItem()
        {
            if (Inventory != null) 
            {
                Inventory.TakeOut(IndexInInventory);
            }
            OnDestroy();
        }
        protected virtual void OnDestroy()
        {

        }
        public virtual void UseItem(TtNode user)
        {
            if (Skill != null)
            {
                Skill.Spell(this, user);
            }
            if (Data.Duration >= 0)
            {
                Data.Duration--;
                if (Data.Duration == 0)
                {
                    DestoryItem();
                }
            }
        }

        public bool PutBack()
        {
            if (Inventory != null)
            {
                return Inventory.PutIn(this, IndexInInventory);
            }
            return false;
        }
    }
    public class TtProxyItem : TtItem
    {
        public static TtProxyItem CreateProxyItem(TtItem item)
        {
            var result = new TtProxyItem();
            result.ReferItem = item;
            return result;
        }
        public TtItem ReferItem;
        public override void DestoryItem()
        {
            ReferItem.DestoryItem();
            ReferItem = null;
            DestoryItem();
        }
        public override void UseItem(TtNode user)
        {
            ReferItem.UseItem(user);
        }
    }
    public class TtInventory : EngineNS.IO.BaseSerializer
    {
        [EngineNS.Rtti.Meta]
        public List<TtItem> Items { get; set; } = null;
        public bool Initialize(int size)
        {
            Items = new List<TtItem>(size);
            for (int i = 0; i < size; ++i)
            {
                Items.Add(null);
            }
            return true;
        }
        public virtual void PrePutIn(ref TtItem item, int index)
        {
            
        }
        public virtual bool CanPutIn(TtItem item, int index)
        {
            if (item.GetType() == typeof(TtProxyItem))
                return false;
            return true;
        }
        public bool PutIn(TtItem item, int index)
        {
            if (index >= Items.Count || index < 0)
                return false;
            if (Items[index] != null)
            {
                return false;
            }
            PrePutIn(ref item, index);
            if (CanPutIn(item, index))
            {
                return false;
            }
            Items[index].Inventory = this;
            Items[index].IndexInInventory = index;
            return true;
        }
        public TtItem TakeOut(int index)
        {
            if (index >= Items.Count || index < 0)
                return null;
            if (Items[index] != null)
            {
                var result = Items[index];
                Items[index] = null;
                return result;
            }
            return null;
        }
        public TtItem PeekItem(int index)
        {
            if (index >= Items.Count || index < 0)
                return null;
            return Items[index];
        }
        public int FindPutableIndex()
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                if (Items[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
        public virtual bool AutoPutIn(TtItem item)
        {
            var index = FindPutableIndex();
            if (index < 0)
                return false;
            return PutIn(item, index);
        }
        public TtItem SwapItem(TtItem item, int index)
        {
            if (index >= Items.Count || index < 0)
                return null;
            if (Items[index] != null)
            {
                PrePutIn(ref item, index);
                if (CanPutIn(item, index) == false)
                    return item;
                var temp = Items[index];
                Items[index] = item;
                if (item != null)
                {
                    CoreSDK.Swap(ref Items[index].Inventory, ref item.Inventory);
                    CoreSDK.Swap(ref Items[index].IndexInInventory, ref item.IndexInInventory);
                }
                return temp;
            }
            else
            {
                PrePutIn(ref item, index);
                if (CanPutIn(item, index) == false)
                    return item;
                Items[index] = item;
                Items[index].Inventory = this;
                Items[index].IndexInInventory = index;
                return null;
            }
        }
        public int FindItem(int ItemId, int start = 0)
        {
            for (int i = start; i < Items.Count; i++)
            {
                if (Items[i].Data.ItemId == ItemId)
                    return i;
            }
            return -1;
        }
    }

    public class TtGoodsInventory : TtInventory
    {
        public override bool CanPutIn(TtItem item, int index)
        {
            //if (item.Skill != null)
            //    return false;
            if (item.GetType() == typeof(TtProxyItem))
                return false;
            return true;
        }
    }
    public class TtGoodsUnlimitInventory : TtInventory
    {
        public override bool CanPutIn(TtItem item, int index)
        {
            if (item.GetType() == typeof(TtProxyItem))
                return false;
            return true;
        }
        public override bool AutoPutIn(TtItem item)
        {
            if (base.AutoPutIn(item) == false)
            {
                item.Inventory = this;
                item.IndexInInventory = Items.Count;
                Items.Add(item);
            }
            return true;
        }
    }
    public class TtProxyInventory : TtInventory
    {
        public override void PrePutIn(ref TtItem item, int index)
        {
            if (item.GetType() != typeof(TtProxyItem))
            {
                if (item.PutBack())
                {
                    item = TtProxyItem.CreateProxyItem(item);
                }
            }
        }
    }
    public class TtDrugItem : TtItem
    {
        public override void UseItem(TtNode user)
        {
            //加血
        }
    }
}
