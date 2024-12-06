using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public partial class IItem
    {
        [Rtti.Meta]
        public virtual string Name
        {
            get;
            set;
        }
        [Rtti.Meta]
        public virtual void UseItem()
        {

        }
    }
    public class IValueItem<T> : IItem
    {
        public T Value;
        public T MaxValue;
        public T MinValue;
    }
    public partial class IItemContain : IItem
    {
        public List<IItem> Items { get; } = new List<IItem>();
        [Rtti.Meta]
        public IItem GetItem(string name,
            [Rtti.MetaParameter(FilterType = typeof(IItem), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            foreach(var i in Items)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        [Rtti.Meta]
        public IItem TakeItem(string name,
            [Rtti.MetaParameter(FilterType = typeof(IItem), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Name == name)
                {
                    var result = Items[i];
                    Items.RemoveAt(i);
                    return result;
                }
            }
            return null;
        }
    }

    public partial class IInventory
    {
        public Dictionary<string, IItem> Items { get; } = new Dictionary<string, IItem>();
        [Rtti.Meta]
        public bool HaveItem(string name)
        {
            return Items.ContainsKey(name);
        }
        [Rtti.Meta]
        public IItem GetItem(string name,
            [Rtti.MetaParameter(FilterType = typeof(IItem), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            IItem result;
            if (Items.TryGetValue(name, out result))
                return result;
            return null;
        }
        [Rtti.Meta]
        public IItem TakeItem(string name,
            [Rtti.MetaParameter(FilterType = typeof(IItem), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            IItem result;
            if (Items.TryGetValue(name, out result))
            {
                Items.Remove(name);
                return result;
            }
            return null;
        }
        [Rtti.Meta]
        public bool PutItem(IActor actor, string name, IItem item, string bagName)
        {
            if (Items.ContainsKey(name))
                return false;

            var bag = GetItem(bagName) as IItemContain;
            if (bag != null)
            {
                if (bag.Items.Contains(item))
                    return false;
                bag.Items.Add(item);
            }
            else
            {
                Items.Add(name, item);
            }
            actor.OnPickedItem(item, this, bag);
            return true;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IItem
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_UseItem_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IItem->void UseItem()");
		public unsafe void macross_UseItem (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			UseItem();
			macross_break_UseItem_2609910045.TryBreak();
		}
	}
}


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IItemContain
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetItem_3440503648 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IItemContain->IItem GetItem(string name, System.Type type)");
		public unsafe IItem macross_GetItem (string nodeName, string name, System.Type type) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":type", type);
				}
			}
			var _return_value = GetItem(name, type);
			macross_break_GetItem_3440503648.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TakeItem_3440503648 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IItemContain->IItem TakeItem(string name, System.Type type)");
		public unsafe IItem macross_TakeItem (string nodeName, string name, System.Type type) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":type", type);
				}
			}
			var _return_value = TakeItem(name, type);
			macross_break_TakeItem_3440503648.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IInventory
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_HaveItem_107167771 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IInventory->bool HaveItem(string name)");
		public unsafe bool macross_HaveItem (string nodeName, string name) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
				}
			}
			var _return_value = HaveItem(name);
			macross_break_HaveItem_107167771.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetItem_3440503648 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IInventory->IItem GetItem(string name, System.Type type)");
		public unsafe IItem macross_GetItem (string nodeName, string name, System.Type type) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":type", type);
				}
			}
			var _return_value = GetItem(name, type);
			macross_break_GetItem_3440503648.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TakeItem_3440503648 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IInventory->IItem TakeItem(string name, System.Type type)");
		public unsafe IItem macross_TakeItem (string nodeName, string name, System.Type type) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":type", type);
				}
			}
			var _return_value = TakeItem(name, type);
			macross_break_TakeItem_3440503648.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_PutItem_1685709441 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.WorldSimulator.GOAP.IInventory->bool PutItem(IActor actor, string name, IItem item, string bagName)");
		public unsafe bool macross_PutItem (string nodeName, IActor actor, string name, IItem item, string bagName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":actor", actor);
					stackframe.SetWatchVariable(nodeName + ":name", name);
					stackframe.SetWatchVariable(nodeName + ":item", item);
					stackframe.SetWatchVariable(nodeName + ":bagName", bagName);
				}
			}
			var _return_value = PutItem(actor, name, item, bagName);
			macross_break_PutItem_1685709441.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross