using BCnEncoder.Shared;
using EngineNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Survivor
{
    [EngineNS.Bricks.DataSet.TtDataTable(SheetName = "TtItemData", KeyName= "ItemId", HeadRow = 0, DataStartRow = 3)]
    public class TtItemData : EngineNS.Bricks.DataSet.TtDataProvider
    {
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemId")]
        public int ItemId { get; set; }
        [EngineNS.Rtti.Meta]
        [EngineNS.Bricks.DataSet.TtDataColumn(HeadName = "ItemName")]
        public string ItemName { get; set; }
    }
    public class TtItemManager : EngineNS.Bricks.DataSet.TtDataManager<TtItemData>
    {

    }
    public class TtItem : EngineNS.IO.BaseSerializer
    {
        public TtItemInventory Inventory = null;
        public short IndexInInventory = -1;
        public TtItemData Data = null;
        
        public static TtItem CreateItem(TtItemData data)
        {
            var result = new TtItem();
            result.Data = data;
            return result;
        }
    }
    public class TtItemInventory : EngineNS.IO.BaseSerializer
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
        public bool PutIn(TtItem item, short index)
        {
            if (index >= Items.Count || index < 0)
                return false;
            if (Items[index] != null)
            {
                return false;
            }
            Items[index].Inventory = this;
            Items[index].IndexInInventory = index;
            return true;
        }
        public TtItem TakeOut(short index)
        {
            if (index >= Items.Count || index < 0)
                return null;
            if (Items[index] != null)
            {
                return Items[index];
            }
            return null;
        }
        public short FindPutableIndex()
        {
            for (short i = 0; i < Items.Count; ++i)
            {
                if (Items[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
        public bool AutoPutIn(TtItem item)
        {
            var index = FindPutableIndex();
            if (index < 0)
                return false;
            return PutIn(item, index);
        }
        public TtItem SwapItem(TtItem item, short index)
        {
            if (index >= Items.Count || index < 0)
                return null;
            if (Items[index] != null)
            { 
                var temp = Items[index];
                Items[index] = item;
                CoreSDK.Swap(ref Items[index].Inventory, ref item.Inventory);
                CoreSDK.Swap(ref Items[index].IndexInInventory, ref item.IndexInInventory);
                return temp;
            }
            else
            {
                Items[index] = item;
                Items[index].Inventory = this;
                Items[index].IndexInInventory = index;
                return null;
            }
        }
    }
}
