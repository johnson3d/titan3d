using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System.Collections;

namespace EngineNS.DesignMacross.Base.Outline
{
    public class OutlineElement_LeafAttribute : Attribute
    {
        public TtTypeDesc ClassType { get; set; }
        public OutlineElement_LeafAttribute(Type type)
        {
            ClassType = TtTypeDesc.TypeOf(type);
        }
    }
    public class OutlineElement_ListAttribute : Attribute
    {
        public TtTypeDesc ClassType { get; set; }
        public bool IsHideTitle { get; set; }
        public OutlineElement_ListAttribute(Type type, bool isHideTitle = false)
        {
            ClassType = TtTypeDesc.TypeOf(type);
            IsHideTitle = isHideTitle;
        }
    }
    public class OutlineElement_BranchAttribute : Attribute
    {
        public TtTypeDesc ClassType { get; set; }
        public OutlineElement_BranchAttribute(Type type)
        {
            ClassType = TtTypeDesc.TypeOf(type);
        }
    }
    public interface IOutlilneElementDraggable
    {
        public bool CanDrag();
        public void OnDragging(Vector2 delta);
    }
    public interface IOutlilneElementSelectable : IOutlilneElementDraggable
    {
        public void OnSelected(ref FOutlineElementRenderingContext context);
        public void OnUnSelected();
    }
    public interface IOutlineElement : IRenderableElement
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public IOutlineElement Parent { get; set; }
    }
    //the leaf node of tree
    public interface IOutlineElement_Leaf : IOutlineElement, IOutlilneElementSelectable, IOutlineContextMeunable
    {
        public IDescription Description { get; set; }
    }
    //the inner node of tree
    public interface IOutlineElement_Branch : IOutlineElement, IOutlilneElementSelectable, IOutlineContextMeunable
    {
        public IDescription Description { get; set; }
        public List<IOutlineElement> ConstructChildrenElements();
    }
    //the inner node of tree
    public interface IOutlineElement_List : IOutlineElement
    {
        public IList DescriptionsList { get; set; }
        public bool IsHideTitle { get; set; }
        public List<IOutlineElement> ConstructListElements();
    }
    //the root node of tree
    public interface IOutline : IOutlineElement
    {

    }
}
