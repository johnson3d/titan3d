using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Outline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Editor.DeclarationPanel
{

    public class TtMethodOutlinerElement : IOutlineElement
    {
        public string Name { get => VariableDeclaration.VariableName; set => VariableDeclaration.VariableName = value; }
        //For code generate
        public UVariableDeclaration VariableDeclaration { get; set; }
        public List<IOutlineElement> Children { get; set; } = new List<IOutlineElement>();
        public IDescription Description { get; set; } = null;

        public void Construct()
        {
            throw new NotImplementedException();
        }

        public void Draw(ref FDesignMacrossEditorRenderingContext context)
        {
            var render = new TtMethodOutlinerElementRender();
            render.Draw(this, ref context);
        }
    }
    public class TtMethodOutlinerElementRender
    {
        public void Draw(TtMethodOutlinerElement variable, ref FDesignMacrossEditorRenderingContext context)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            TtMethodOutlinerElement removeVar = null;
            var designVarTreeNodeIsOpen = ImGuiAPI.TreeNodeEx(variable.Name, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var designVarTreeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var designVarTreeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
            var keyName = $"Delete DesignableVariable {variable.Name}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "DesignVar_X_" + variable.Name))
            {
                EGui.UIProxy.MessageBox.Open(keyName);
            }
            EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {variable.Name}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
            () =>
            {
                removeVar = variable;
            }, null);
            if (designVarTreeNodeIsOpen)
            {
                if (designVarTreeNodeDoubleClicked)
                {
                    //context.GraphEditingZone.EditDesignableVariable(designableVar);
                }
                else if (designVarTreeNodeIsItemClicked)
                {
                    //PGMember.Target = method;
                }
            }

            if (removeVar != null)
            {
                //classDeclarationEditingZoneUI.DesignableVariables.Remove(removeVar);
            }


        }
    }
}
