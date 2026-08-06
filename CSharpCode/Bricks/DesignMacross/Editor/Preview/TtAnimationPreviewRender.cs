using EngineNS.EGui;
using System;

namespace EngineNS.DesignMacross.Editor.Preview
{
    /// <summary>
    /// 动画预览渲染器 - 绘制模型属性PropertyGrid和3D预览视口
    /// </summary>
    public struct TtAnimationPreviewRender
    {
        public void Draw(TtAnimationPreviewPanel previewPanel, FDesignMacrossEditorRenderingContext context)
        {
            if (previewPanel == null || !previewPanel.IsShow)
                return;

            try
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.Text("PreviewModel:");
                    ImGuiAPI.SameLine(0, -1);
                    if (previewPanel.ModelSelector.OnDraw(previewPanel.GetCurrentModelRName(), out var newModelRName))
                    {
                        previewPanel.LoadModel(newModelRName);
                    }
                }

                previewPanel.PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                previewPanel.PreviewViewport.OnDraw();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
        }
    }
}
