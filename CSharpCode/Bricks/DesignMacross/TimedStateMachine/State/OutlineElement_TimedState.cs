using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_TimedState))]
    public class TtOutlineElement_TimedState : IDesignableVariableOutlineElement
    {
        public TtOutlineElement_TimedStatesHubGraph StatesHub { get; set; }
        public string Name { get => VariableDeclaration.VariableName; set => VariableDeclaration.VariableName = value; }
        public UVariableDeclaration VariableDeclaration { get; set; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();
        public IDescription Description { get; set; } = null;

        public void Construct()
        {
            throw new NotImplementedException();
        }
    }
    public class TtOutlineElementRender_TimedState : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_TimedState state = renderableElement as  TtOutlineElement_TimedState;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var designVarTreeNodeIsOpen = ImGuiAPI.TreeNodeEx(state.Name, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var designVarTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var designVarTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
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
            if (designVarTreeNodeIsOpen)
            {
                if (designVarTreeNodeDoubleClicked)
                {
                    //context.DefinitionGraphEditPanel.EditDefinitionGraph(variable);
                }
                else if (designVarTreeNodeIsItemClicked)
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
