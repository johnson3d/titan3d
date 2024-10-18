using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Render;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Base.Outline
{
    [ImGuiElementRender(typeof(TtOutlineRender))]
    public class TtOutline : IOutline
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();
        public IDescription Description { get; set; } = null;
        public IOutlineElement Parent { get; set; } = null;

        public List<IOutlineElement> ConstructElements()
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
    }

    public class TtOutlineRender : IOutlineRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineRenderingContext context)
        {
            var outline = renderableElement as TtOutline;
            var elementContext = new FOutlineElementRenderingContext();
            elementContext.CommandHistory = context.CommandHistory;
            elementContext.EditorInteroperation = context.EditorInteroperation;
            var elements = outline.ConstructElements();
            foreach (var element in elements)
            {
                var elementRender = TtElementRenderDevice.CreateOutlineElementRender(element);
                elementRender.Draw(element, ref elementContext);
            }
            TtOutlineContextMenuHandler.Instance.HandleContextMenu(ref elementContext);
        }       
    }
}
