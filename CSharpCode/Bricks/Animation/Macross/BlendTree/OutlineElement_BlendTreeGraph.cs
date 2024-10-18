using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Macross.BlendTree
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_BlendTreeGraph))]
    public class TtOutlineElement_BlendTreeGraph : TtOutlineElement_Leaf
    {
    }
    public class TtOutlineElementRender_BlendTreeGraph : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_BlendTreeGraph outlineElement_BlendTreeGraph = renderableElement as TtOutlineElement_BlendTreeGraph;
            var varibleName = outlineElement_BlendTreeGraph.Description.Name;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(varibleName, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var treeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var treeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
            var keyName = $"Delete BlendTree {varibleName}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, varibleName))
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
                    context.EditorInteroperation.GraphEditPanel.EditGraph(outlineElement_BlendTreeGraph.Description);
                }
                else if (treeNodeIsItemClicked)
                {
                    context.EditorInteroperation.PGMember.Target = outlineElement_BlendTreeGraph.Description;
                    //PGMember.Target = method;
                }
            }

            if (bIsRemoved)
            {
                if (outlineElement_BlendTreeGraph.Parent is TtOutlineElement_List parent)
                {
                    var desc = outlineElement_BlendTreeGraph.Description;
                    context.CommandHistory.CreateAndExtuteCommand("RemoveBlendTree",
                       (data) => { parent.DescriptionsList.Remove(desc); ; },
                       (data) => { parent.DescriptionsList.Add(desc); });

                }
            }
        }

    }
}
