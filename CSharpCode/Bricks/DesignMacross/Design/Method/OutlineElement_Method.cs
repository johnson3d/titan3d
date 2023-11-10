using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace EngineNS.DesignMacross.Design
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_Method))]
    public class TtOutlineElement_Method : TtOutlineElement_Leaf
    {
    }
    public class TtOutlineElementRender_Method : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_Method outlineElement_Method = renderableElement as TtOutlineElement_Method;
            var varibleName = outlineElement_Method.Description.Name;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(varibleName, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var treeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var treeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
            var keyName = $"Delete Method {varibleName}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "Method_X_" + varibleName))
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
                    context.EditorInteroperation.PGMember.Target = outlineElement_Method.Description;
                    //PGMember.Target = method;
                }
            }

            if (bIsRemoved)
            {
                if (outlineElement_Method.Parent is TtOutlineElementsList_Methods parent)
                {
                    var methodDesc = outlineElement_Method.Description as TtMethodDescription;
                    context.CommandHistory.CreateAndExtuteCommand("RemoveMethod",
                       (data) => { parent.Descriptions.Remove(methodDesc); ; },
                       (data) => { parent.Descriptions.Add(methodDesc); });

                }
            }
        }

    }

    [ImGuiElementRender(typeof(TtOutlineElementsListRender_Methods))]
    public class TtOutlineElementsList_Methods : TtOutlineElement_List
    {
        public List<IMethodDescription> Descriptions
        {
            get => DescriptionsList as List<IMethodDescription>;
        }
    }
    public class TtOutlineElementsListRender_Methods : IOutlineElementsListRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            var elementsList = renderableElement as TtOutlineElementsList_Methods;
            Debug.Assert(elementsList != null);
            Vector2 buttonSize = new Vector2(16, 16);

            var treeNodeResult = ImGuiAPI.TreeNodeEx("Methods",
                ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap |
                ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick |
                ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen);
            var regionSize = ImGuiAPI.GetContentRegionAvail();
            ImGuiAPI.SameLine(regionSize.X, -1.0f);
            if (EGui.UIProxy.CustomButton.ToolButton("+", in buttonSize, 0xFF00FF00))
            {
                const string methodName = "New_Method";
                var num = 0;
                while (true)
                {
                    var result = elementsList.Descriptions.ToImmutableList()
                        .Find(desc => desc.Name == $"{methodName}_{num}");
                    if (result == null)
                    {
                        break;
                    }

                    num++;
                }

                var name = $"{methodName}_{num}";
                if (UTypeDescManager.CreateInstance(UTypeDesc.TypeOf<TtMethodDescription>()) is IMethodDescription
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
}
