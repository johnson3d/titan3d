using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    //[OutlineElement(typeof(TtOutlineElement_DesignableVariable))]
    //public class TtDesignableVariableDescription : IDescription
    //{
    //    public Guid Id { get; set; } = Guid.NewGuid();
    //    public IDescription Class { get; set; }
    //    public TtVariableDescription Variable { get; set; }
    //}

    //[ImGuiElementRender(typeof(TtOutlineElementRender_DesignableVariable))]
    //public class TtOutlineElement_DesignableVariable : IOutlineElement
    //{
    //    public string Name { get; set; }
    //    public List<IOutlineElement> Elements { get; set; }
    //    public IDescription Description { get; set; }

    //    public void Construct()
    //    {

    //    }
    //}
    public class TtOutlineElementRender_DesignableVariable : IOutlineElementRender
    {
        public IOutlineElement Element { get; set; }

        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {

        }
    }
    [ImGuiElementRender(typeof(TtOutlineElementsListRender_DesignableVariables))]
    public class TtOutlineElementsList_DesignableVariables : IOutlineElementsList
    {
        public string Name { get; set; }
        public IDescription Description { get; set; } = null;
        public INotifyCollectionChanged NotifiableDescriptions { get; set; }
        public ObservableCollection<IDesignableVariableDescription> Descriptions { get => NotifiableDescriptions as ObservableCollection<IDesignableVariableDescription>; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();

        public void Construct()
        {
            Children.Clear();

            System.Diagnostics.Debug.Assert(Descriptions != null);

            foreach (var description in Descriptions)
            {
                var outlinerElementAttribute = description.GetType().GetCustomAttribute<OutlineElementAttribute>();
                if (outlinerElementAttribute != null)
                {
                    var instance = UTypeDescManager.CreateInstance(outlinerElementAttribute.ClassType) as IOutlineElement;
                    instance.Description = description;
                    instance.Construct();
                    Children.Add(instance);
                }
                var outlinerElementTreeAttribute = description.GetType().GetCustomAttribute<OutlineElementsListAttribute>();
                if (outlinerElementTreeAttribute != null)
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
        }
    }

    public class TtOutlineElementsListRender_DesignableVariables : IOutlineElementsListRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            var elementsList = renderableElement as TtOutlineElementsList_DesignableVariables;
            Vector2 buttonSize = new Vector2(16, 16);

            var treeNodeResult = ImGuiAPI.TreeNodeEx("DesignableVariables", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            ImGuiAPI.SameLine(regionSize.X, -1.0f);
            if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            {
                ImGuiAPI.OpenPopup("MacrossDesignableVariablesSelectPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("MacrossDesignableVariablesSelectPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                foreach (var designableType in TtDesignableClassTypes.DesignTypePairs)
                {
                    var typeShowName = designableType.ShowClassName;
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    var menuData = new Support.UAnyPointer();
                    EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
                    newMethodMenuState.Reset();
                    //newMethodMenuState.Opened =true;
                    if (EGui.UIProxy.MenuItemProxy.MenuItem("New" + typeShowName, null, false, null, in drawList, in menuData, ref newMethodMenuState))
                    {
                        var num = 0;
                        while (true)
                        {
                            var result = elementsList.Descriptions.ToImmutableList().Find(desc => desc.Name == $"{typeShowName}_{num}");
                            if (result == null)
                            {
                                break;
                            }
                            num++;
                        }
                        var name = $"{typeShowName}_{num}";
                        if (UTypeDescManager.CreateInstance(designableType.TypeForDesign) is IDesignableVariableDescription description)
                        {
                            description.Name = name;
                            elementsList.Descriptions.Add(description);
                            elementsList.Construct();
                        }
                    }
                }
                ImGuiAPI.EndPopup();
            }
            if (treeNodeResult)
            {
                var funcRegionSize = ImGuiAPI.GetContentRegionAvail();
                var elementContext = new FOutlineElementRenderingContext();
                elementContext.CommandHistory = context.CommandHistory;
                elementContext.EditorInteroperation = context.EditorInteroperation;
                foreach (var element in elementsList.Children)
                {
                    var render = TtElementRenderDevice.CreateOutlineElementRender(element);
                    render.Draw(element, ref elementContext);
                }
                ImGuiAPI.TreePop();
            }

        }
    }


}
