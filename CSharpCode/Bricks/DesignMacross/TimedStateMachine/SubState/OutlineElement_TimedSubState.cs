using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.DesignMacross.TimedStateMachine.CompoundState;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_TimedSubState))]
    public class TtOutlineElement_TimedSubState : TtOutlineElement_Leaf
    {
        public TtOutlineElement_TimedCompoundStateGraph CompoundStateGraph { get; set; }


    }
    public class TtOutlineElementRender_TimedSubState : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_TimedSubState state = renderableElement as  TtOutlineElement_TimedSubState;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(state.Name, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var treeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var treeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
            var keyName = $"Delete DesignableVariable {state.Name}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + state.Name))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }
            bool bIsRemoved = false;
            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {state.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                bIsRemoved = true;
            }, null);
            if (treeNodeIsOpen)
            {
                if (treeNodeDoubleClicked)
                {
                    //context.DefinitionGraphEditPanel.EditDefinitionGraph(variable);
                }
                else if (treeNodeIsItemClicked)
                {
                    //PGMember.Target = method;
                }
            }

            if (bIsRemoved)
            {
                //state.StatesHub.RemoveState(state);
            }
        }

    }
}
