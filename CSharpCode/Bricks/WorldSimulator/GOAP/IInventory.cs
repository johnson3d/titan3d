using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public class IItem
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
    public class IItemContain : IItem
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

    public class IInventory
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
