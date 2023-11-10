using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine.CompoundState
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_TimedCompoundStateGraph))]
    public class TtOutlineElement_TimedCompoundStateGraph : TtOutlineElement_Branch
    {
        //public override List<IOutlineElement> ConstructChildrenElements()
        //{
        //    var childrenElements = new List<IOutlineElement>();
        //    if(Description is TtTimedCompoundStateClassDescription compoundState)
        //    {
        //        foreach(var state in compoundState.States)
        //        {
        //            var instance = TtOutlineElementsPoolManager.Instance.Get(UTypeDesc.TypeOf(state.GetType())) as IOutlineElement_Leaf;
        //            instance.Description = state;
        //            instance.Parent = this;
        //            childrenElements.Add(instance);
        //        }
        //    }
        //    return childrenElements;
        //}
    }
    public class TtOutlineElementRender_TimedCompoundStateGraph : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_TimedCompoundStateGraph graph = renderableElement as TtOutlineElement_TimedCompoundStateGraph; ;
            Vector2 buttonSize = new Vector2(16, 16);
            //float buttonOffset = 16;
            var sz = new Vector2(-1, 0);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            ImGuiTreeNodeFlags_ flags;
            bool bIsLeaf = true;
            if(bIsLeaf)
            {
                flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            }
            else
            {
                flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
            }
            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(graph.Description.Name, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var isTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var isTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X, -1.0f);
            var keyName = $"Delete DesignableVariable {graph.Description.Name}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + graph.Description.Name))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }
            bool bIsRemoved = false;
            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {graph.Description.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                bIsRemoved = true;
                
            }, null);
            if(bIsRemoved)
            {
                if (graph.Parent is TtOutlineElementsList_TimedCompoundStates parent)
                {
                    var compoundStateDesc = graph.Description as TtTimedCompoundStateClassDescription;
                    context.CommandHistory.CreateAndExtuteCommand("RemoveCompoundState",
                       (data) => { parent.Descriptions.Remove(compoundStateDesc); ; },
                       (data) => { parent.Descriptions.Add(compoundStateDesc); });
                    
                }
            }
            if (treeNodeIsOpen)
            {
                if (isTreeNodeDoubleClicked)
                {
                    context.EditorInteroperation.GraphEditPanel.EditGraph(graph.Description);
                }
                else if (isTreeNodeIsItemClicked)
                {
                    graph.OnSelected(ref context);
                }
                var elements = graph.ConstructChildrenElements();
                foreach (var state in elements)
                {
                    var stateRender = TtElementRenderDevice.CreateOutlineElementRender(state);
                    stateRender.Draw(state, ref context);
                }
                if(!bIsLeaf)
                {
                    ImGuiAPI.TreePop();
                }
            }
        }
    }

    #region OutlineElementsListRender_TimedCompoundStates
    [ImGuiElementRender(typeof(TtOutlineElementsListRender_TimedCompoundStates))]
    public class TtOutlineElementsList_TimedCompoundStates : TtOutlineElement_List
    {
        public List<TtTimedCompoundStateClassDescription> Descriptions { get => DescriptionsList as List<TtTimedCompoundStateClassDescription>; }
        public TtOutlineElementsList_TimedCompoundStates()
        {

        }

    }
    public class TtOutlineElementsListRender_TimedCompoundStates : IOutlineElementsListRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElementsList_TimedCompoundStates elementsList = renderableElement as TtOutlineElementsList_TimedCompoundStates;
            TtOutlineElement_TimedStateMachine stateMachine = elementsList.Parent as TtOutlineElement_TimedStateMachine;
            Vector2 buttonSize = new Vector2(16, 16);
            float buttonOffset = 16;
            var sz = new Vector2(-1, 0);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            bool treeNodeResult = true;
            if (!elementsList.IsHideTitle)
            {
                treeNodeResult = ImGuiAPI.TreeNodeEx("CompoundStates", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick);
            }
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X - buttonOffset, -1.0f);
            if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            {
                ImGuiAPI.OpenPopup("CompoundStatesAddPopup", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            if (ImGuiAPI.BeginPopup("CompoundStatesAddPopup", ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                var stateVarName = "CompoundState";
                var drawList = ImGuiAPI.GetWindowDrawList();
                var menuData = new Support.UAnyPointer();
                EGui.UIProxy.MenuItemProxy.MenuState newMethodMenuState = new EGui.UIProxy.MenuItemProxy.MenuState();
                newMethodMenuState.Reset();
                if (EGui.UIProxy.MenuItemProxy.MenuItem("New" + stateVarName, null, false, null, in drawList, in menuData, ref newMethodMenuState))
                {
                    var num = 0;
                    while (true)
                    {
                        var result = elementsList.Descriptions.Find(child => child.Name == $"{stateVarName}_{num}");
                        if (result == null)
                        {
                            break;
                        }
                        num++;
                    }
                    var desc = new TtTimedCompoundStateClassDescription();
                    desc.Name = $"{stateVarName}_{num}";
                    desc.Parent = stateMachine.Description as TtTimedStateMachineClassDescription;
                    context.CommandHistory.CreateAndExtuteCommand("AddCompoundState",
                        (data) => { elementsList.Descriptions.Add(desc); desc.Parent = stateMachine.Description; },
                        (data) => { elementsList.Descriptions.Remove(desc); desc.Parent = null; });

                }
                ImGuiAPI.EndPopup();
            }
            if (treeNodeResult)
            {
                var elements = elementsList.ConstructListElements();
                foreach (var element in elements)
                {
                    var elementRender = TtElementRenderDevice.CreateOutlineElementRender(element);
                    if (elementRender != null)
                    {
                        elementRender.Draw(element, ref context);
                    }
                }
                if (!elementsList.IsHideTitle)
                {
                    ImGuiAPI.TreePop();
                }
            }
        }
    }

    #endregion OutlineElementsListRender_TimedCompoundStates
}
