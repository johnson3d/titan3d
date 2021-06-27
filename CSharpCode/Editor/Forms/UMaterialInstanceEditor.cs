using EngineNS.Graphics.Pipeline;
using SDL2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    class UMaterialInstanceEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public Graphics.Pipeline.Shader.UMaterialInstance Material;
        public Bricks.CodeBuilder.ShaderNode.UPreviewViewport PreviewViewport = new Bricks.CodeBuilder.ShaderNode.UPreviewViewport(false);
        public EGui.Controls.PropertyGrid.PropertyGrid MaterialPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public void Cleanup()
        {
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            MaterialPropGrid.SingleTarget = null;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            Material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(name);
            if (Material == null)
                return false;
            
            MaterialPropGrid.SingleTarget = Material;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }

        public void OnDraw()
        {
            if (Material == null)
                return;

            if (Visible == false)
                return;
        }

        public void OnEvent(ref SDL.SDL_Event e)
        {
            //throw new NotImplementedException();
        }
        #region Tickable
        public void TickLogic(int ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
    }
}
