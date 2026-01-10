using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public partial class IItem
    {
        [Rtti.Meta("")]
        public virtual string Name
        {
            get;
            set;
        }
        [Rtti.Meta("")]
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
        [Rtti.Meta("")]
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
        [Rtti.Meta("")]
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
        [Rtti.Meta("")]
        public bool HaveItem(string name)
        {
            return Items.ContainsKey(name);
        }
        [Rtti.Meta("")]
        public IItem GetItem(string name,
            [Rtti.MetaParameter(FilterType = typeof(IItem), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            IItem result;
            if (Items.TryGetValue(name, out result))
                return result;
            return null;
        }
        [Rtti.Meta("")]
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
        [Rtti.Meta("")]
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
		public unsafe void macross_UseItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			UseItem();
		}
	}
}


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IItemContain
	{
		public unsafe IItem macross_GetItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetItem(name, type);
			return _return_value;
		}
		public unsafe IItem macross_TakeItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = TakeItem(name, type);
			return _return_value;
		}
	}
}


namespace EngineNS.Bricks.WorldSimulator.GOAP
{
	partial class IInventory
	{
		public unsafe bool macross_HaveItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = HaveItem(name);
			return _return_value;
		}
		public unsafe IItem macross_GetItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = GetItem(name, type);
			return _return_value;
		}
		public unsafe IItem macross_TakeItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, System.Type type) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = TakeItem(name, type);
			return _return_value;
		}
		public unsafe bool macross_PutItem (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, IActor actor, string name, IItem item, string bagName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = PutItem(actor, name, item, bagName);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross