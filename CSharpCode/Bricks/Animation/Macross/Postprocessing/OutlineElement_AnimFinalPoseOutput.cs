using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Macross.Postprocessing
{
    [ImGuiElementRender(typeof(TtOutlineElementRender_AnimFinalPoseOutput))]
    public class TtOutlineElement_AnimFinalPoseOutput : TtOutlineElement_Leaf
    {
    }
    public class TtOutlineElementRender_AnimFinalPoseOutput : IOutlineElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FOutlineElementRenderingContext context)
        {
            TtOutlineElement_AnimFinalPoseOutput outlineElement_AnimFinalPoseOutput = renderableElement as TtOutlineElement_AnimFinalPoseOutput;
            var varibleName = outlineElement_AnimFinalPoseOutput.Description.Name;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;// | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_AllowItemOverlap;
            Vector2 buttonSize = new Vector2(16, 16);
            var regionSize = ImGuiAPI.GetContentRegionAvail();

            var treeNodeIsOpen = ImGuiAPI.TreeNodeEx(varibleName, flags);
            ImGuiAPI.SameLine(0, EGui.UIProxy.StyleConfig.Instance.ItemSpacing.X);
            var treeNodeDoubleClicked = ImGuiAPI.IsItemDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            var treeNodeIsItemClicked = ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left);
            ImGuiAPI.SameLine(regionSize.X - buttonSize.X, -1.0f);
            var keyName = $"Delete Method {varibleName}?";
            if (EGui.UIProxy.CustomButton.ToolButton("x", in buttonSize, 0xFF0000FF, "AnimFinalPoseOutput"))
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
                    context.EditorInteroperation.GraphEditPanel.EditGraph(outlineElement_AnimFinalPoseOutput.Description);
                }
                else if (treeNodeIsItemClicked)
                {
                    context.EditorInteroperation.PGMember.Target = outlineElement_AnimFinalPoseOutput.Description;
                    //PGMember.Target = method;
                }
            }

            if (bIsRemoved)
            {
                if (outlineElement_AnimFinalPoseOutput.Parent is TtOutlineElement_List parent)
                {
                    var desc = outlineElement_AnimFinalPoseOutput.Description as TtMethodDescription;
                    context.CommandHistory.CreateAndExtuteCommand("RemoveAnimFinalPoseOutput",
                       (data) => { parent.DescriptionsList.Remove(desc); ; },
                       (data) => { parent.DescriptionsList.Add(desc); });

                }
            }
        }

    }
}
