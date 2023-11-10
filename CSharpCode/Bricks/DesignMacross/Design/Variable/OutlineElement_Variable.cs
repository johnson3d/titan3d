using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.DesignMacross.TimedStateMachine;
using EngineNS.DesignMacross.TimedStateMachine.CompoundState;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross.Design;

[ImGuiElementRender(typeof(TtOutlineElementRender_Variable))]
public class TtOutlineElement_Variable : TtOutlineElement_Leaf
{

}
public class TtOutlineElementRender_Variable : IOutlineElementRender
{
    public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
    {
        TtOutlineElement_Variable outLineElement_variable = renderableElement as TtOutlineElement_Variable;
        var varibleName = outLineElement_variable.Description.Name;
        ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
        Vector2 buttonSize = new Vector2(16, 16);
        var regionSize = ImGuiAPI.GetContentRegionAvail();

        var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(varibleName, flags);
        ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
        var treeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
        var treeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
        ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
        var keyName = $"Delete Variable {varibleName}?";
        if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "Var_X_" + varibleName))
        {
            EGui.UIProxy.MessageBox.Open(keyName);
        }
        bool bIsRemoved = false;
        EGui.UIProxy.MessageBox.Draw(keyName, $"Are you sure to delete {varibleName}?", EGui.UIProxy.MessageBox.EButtonType.YesNo,
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
                context.EditorInteroperation.PGMember.Target = outLineElement_variable.Description;
                //PGMember.Target = method;
            }
        }

        if (bIsRemoved)
        {
            if (outLineElement_variable.Parent is TtOutlineElementsList_Variables parent)
            {
                var variableDesc = outLineElement_variable.Description as TtVariableDescription;
                context.CommandHistory.CreateAndExtuteCommand("RemoveVariable",
                   (data) => { parent.Descriptions.Remove(variableDesc); ; },
                   (data) => { parent.Descriptions.Add(variableDesc); });

            }
        }
    }
}

[ImGuiElementRender(typeof(TtOutlineElementsListRender_Variables))]
public class TtOutlineElementsList_Variables : TtOutlineElement_List
{
    public List<IVariableDescription> Descriptions
    {
        get => DescriptionsList as List<IVariableDescription>;
    }
}

public class TtOutlineElementsListRender_Variables : IOutlineElementsListRender
{
    public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
    {
        var elementsList = renderableElement as TtOutlineElementsList_Variables;
        Debug.Assert(elementsList != null);
        Vector2 buttonSize = new Vector2(16, 16);

        var treeNodeResult = ImGuiAPI.TreeNodeEx("Variables",
            ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap |
            ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick |
            ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
        var regionSize = ImGuiAPI.GetContentRegionAvail();
        ImGuiAPI.SameLine(regionSize.X, -1.0f);
        if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
        {
            const string variableName = "New_Var";
            var num = 0;
            while (true)
            {
                var result = elementsList.Descriptions.ToImmutableList()
                    .Find(desc => desc.Name == $"{variableName}_{num}");
                if (result == null)
                {
                    break;
                }

                num++;
            }

            var name = $"{variableName}_{num}";
            if (UTypeDescManager.CreateInstance(UTypeDesc.TypeOf<TtVariableDescription>()) is IVariableDescription
                description)
            {
                description.Name = name;
                elementsList.Descriptions.Add(description);
            }
        }

        if (treeNodeResult)
        {
            var funcRegionSize = ImGuiAPI.GetContentRegionAvail();
            var elementContext = new FOutlineElementRenderingContext();
            elementContext.CommandHistory = context.CommandHistory;
            elementContext.EditorInteroperation = context.EditorInteroperation;
            var elements = elementsList.ConstructListElements();
            foreach (var element in elements)
            {
                var render = TtElementRenderDevice.CreateOutlineElementRender(element);
                render.Draw(element, ref elementContext);
            }

            ImGuiAPI.TreePop();
        }
    }
}