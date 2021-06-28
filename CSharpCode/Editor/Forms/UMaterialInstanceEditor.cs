using EngineNS.Graphics.Pipeline;
using SDL2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class UMaterialInstanceEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public Graphics.Pipeline.Shader.UMaterialInstance Material;
        public Bricks.CodeBuilder.ShaderNode.UPreviewViewport PreviewViewport = new Bricks.CodeBuilder.ShaderNode.UPreviewViewport();
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
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterialInstance(Bricks.CodeBuilder.ShaderNode.UPreviewViewport viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.RenderPolicy.Initialize(1, 1);

            viewport.CameraController.Camera = viewport.RenderPolicy.GBuffers.Camera;

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = Material;
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.CMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mesh.SetWorldMatrix(ref Matrix.mIdentity);
                viewport.RenderPolicy.VisibleMeshes.Add(mesh);
            }

            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();
        }
        public async Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            Material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(name);
            if (Material == null)
                return false;

            PreviewViewport.Title = "MaterialInstancePreview";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterialInstance;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.MainWindow, new Graphics.Pipeline.Mobile.UMobileFSPolicy(), 0, 1);

            MaterialPropGrid.SingleTarget = Material;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public unsafe void OnDraw()
        {
            if (Visible == false || Material == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(ref WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(Material.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                ImGuiAPI.Separator();
                ImGuiAPI.Columns(2, null, true);
                if (LeftWidth == 0)
                {
                    ImGuiAPI.SetColumnWidth(0, 300);
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMin();

                DrawLeft(ref min, ref max);
                ImGuiAPI.NextColumn();

                DrawRight(ref min, ref max);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            ImGuiAPI.End();
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (ImGuiAPI.CollapsingHeader("MaterialProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
            {
                MaterialPropGrid.OnDraw(true, false, false);
            }
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            PreviewViewport.VieportType = UViewportSlate.EVieportType.ChildWindow;            
            PreviewViewport.OnDraw();
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

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UMaterialInstanceEditor))]
    public partial class UMaterialInstance
    {
    }
}