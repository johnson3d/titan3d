using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.Rtti;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Base.Outline
{

    //the leaf node of tree
    public class TtOutlineElement_Leaf : IOutlineElement_Leaf
    {
        public string Name { get; set; } = "Default_OutlineElement_Leaf";
        public IDescription Description { get; set; } = null;
        public IOutlineElement Parent { get; set; } = null;

        public bool CanDrag()
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {
            
        }

        public void OnSelected(ref FOutlineElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description;
        }

        public void OnUnSelected()
        {
            
        }
    }
    //the inner node of tree
    public class TtOutlineElement_Branch : IOutlineElement_Branch
    {
        public string Name { get; set; } = "Default_OutlineElement_Branch";
        public IDescription Description { get; set; }
        public IOutlineElement Parent { get; set; }
        public virtual List<IOutlineElement> ConstructChildrenElements()
        {
            var childrenElements = new List<IOutlineElement>();
            foreach (var property in Description.GetType().GetProperties())
            {
                var singleAttribute = property.GetCustomAttribute<OutlineElement_LeafAttribute>();
                if (singleAttribute != null)
                {
                    var instance = TtOutlineElementsPoolManager.Instance.Get(singleAttribute.ClassType) as IOutlineElement_Leaf;
                    var desc = property.GetValue(Description) as IDescription;
                    instance.Description = desc;
                    instance.Parent = this;
                    childrenElements.Add(instance);
                }
                var listAttribute = property.GetCustomAttribute<OutlineElement_ListAttribute>();
                if (listAttribute != null)
                {
                    var instance = TtOutlineElementsPoolManager.Instance.Get(listAttribute.ClassType) as IOutlineElement_List;
                    instance.IsHideTitle = listAttribute.IsHideTitle;
                    var list = property.GetValue(Description) as IList;
                    Debug.Assert(list != null);
                    instance.DescriptionsList = list;
                    instance.Parent = this;
                    childrenElements.Add(instance);
                }
                var branchAttribute = property.GetCustomAttribute<OutlineElement_BranchAttribute>();
                if (branchAttribute != null)
                {
                    var instance = TtOutlineElementsPoolManager.Instance.Get(branchAttribute.ClassType) as IOutlineElement_Branch;
                    var desc = property.GetValue(Description) as IDescription;
                    instance.Description = desc;
                    instance.Parent = this;
                    childrenElements.Add(instance);
                }
            }
            return childrenElements;
        }

        public bool CanDrag()
        {
            return false;
        }

        public void OnDragging(Vector2 delta)
        {

        }

        public void OnSelected(ref FOutlineElementRenderingContext context)
        {
            context.EditorInteroperation.PGMember.Target = Description;
        }

        public void OnUnSelected()
        {

        }
    }
    //the inner node of tree
    public class TtOutlineElement_List : IOutlineElement_List
    {
        public string Name { get; set; } = "Default_OutlineElement_List";
        public IList DescriptionsList { get; set; } = null;
        public IOutlineElement Parent { get; set; } = null;
        public bool IsHideTitle { get; set; } = false;

        public virtual List<IOutlineElement> ConstructListElements()
        {
            var childrenElements = new List<IOutlineElement>();
            foreach (var description in DescriptionsList)
            {
                var outlinerElementAttribute = description.GetType().GetCustomAttribute<OutlineElement_LeafAttribute>();
                if (outlinerElementAttribute != null)
                {
                    var instance = TtOutlineElementsPoolManager.Instance.Get(outlinerElementAttribute.ClassType) as IOutlineElement_Leaf;
                    instance.Description = description as IDescription;
                    instance.Parent = this;
                    childrenElements.Add(instance);
                }
                var outlinerElementTreeAttribute = description.GetType().GetCustomAttribute<OutlineElement_ListAttribute>();
                if (outlinerElementTreeAttribute != null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                var branchAttribute = description.GetType().GetCustomAttribute<OutlineElement_BranchAttribute>();
                if (branchAttribute != null)
                {
                    var instance = TtOutlineElementsPoolManager.Instance.Get(branchAttribute.ClassType) as IOutlineElement_Branch;
                    instance.Description = description as IDescription;
                    instance.Parent = this;
                    childrenElements.Add(instance);
                }
            }
            return childrenElements;
        }
    }

    public class TtOutlineElementsPoolManager
    {
        public static TtOutlineElementsPoolManager Instance { get; } = new TtOutlineElementsPoolManager();
        protected Dictionary<UTypeDesc, TtOutlineElementsPool> ElementsPools = new Dictionary<UTypeDesc, TtOutlineElementsPool>();
        public IOutlineElement Get(UTypeDesc type)
        {
            if (!ElementsPools.ContainsKey(type))
            {
                ElementsPools.Add(type, new TtOutlineElementsPool(type));
            }
            var result = ElementsPools[type].Get();
            Debug.Assert(UTypeDesc.TypeOf(result.GetType()) == type);
            return result;
        }
        public void Return(IOutlineElement graphElement)
        {
            var elementType = UTypeDesc.TypeOf(graphElement.GetType());
            if (ElementsPools.ContainsKey(elementType))
            {
                ElementsPools[elementType].Return(graphElement);
            }
        }
    }

    public class TtOutlineElementsPool
    {
        protected Stack<IOutlineElement> mPool = new Stack<IOutlineElement>();
        protected UTypeDesc ElementType;
        public TtOutlineElementsPool(UTypeDesc type)
        {
            ElementType = type;
        }
        public int GrowStep
        {
            get;
            set;
        } = 10;
        public int PoolSize
        {
            get
            {
                return mPool.Count;
            }
        }
        public int AliveNumber
        {
            get;
            private set;
        } = 0;
        protected IOutlineElement Create()
        {
            return UTypeDescManager.CreateInstance(ElementType) as IOutlineElement;
        }
        public IOutlineElement Get()
        {
            lock (this)
            {
                if (mPool.Count == 0)
                {
                    for (int i = 0; i < GrowStep; i++)
                    {
                        var t = Create();
                        mPool.Push(t);
                    }
                }
                var result = mPool.Peek();
                mPool.Pop();
                AliveNumber++;
                return result;
            }
        }
        public bool Return(IOutlineElement outlineElement)
        {
            lock (this)
            {
                mPool.Push(outlineElement);
                AliveNumber--;
                return true;
            }
        }
    }
}
