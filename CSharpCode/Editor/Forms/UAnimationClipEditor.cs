using EngineNS.Graphics.Pipeline;
using SDL2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class UAnimationClipEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public Animation.Asset.UAnimationClip AnimationClip;
        public Editor.UPreviewViewport PreviewViewport = new Editor.UPreviewViewport();
        public EGui.Controls.PropertyGrid.PropertyGrid AnimationClipPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UAnimationClipEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            AnimationClip = null;
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            AnimationClipPropGrid.Target = null;
        }
        #region IAssetEditor
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public IRootForm GetRootForm()
        {
            return this;
        }

        public async Task<bool> Initialize()
        {
            await AnimationClipPropGrid.Initialize();
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
        public void OnDraw()
        {
            if(Visible == false || AnimationClip == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(AnimationClip.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
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
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
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
        protected unsafe void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", in btSize))
            {
                AnimationClip.SaveAssetTo(AnimationClip.AssetName);
                var unused = UEngine.Instance.GfxDevice.MaterialMeshManager.ReloadMaterialMesh(AnimationClip.AssetName);

                USnapshot.Save(AnimationClip.AssetName, AnimationClip.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Reload", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Redo", in btSize))
            {

            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    AnimationClipPropGrid.OnDraw(true, false, false);
                }
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.ChildWindow;
            PreviewViewport.OnDraw();
        }
        public void OnEvent(ref SDL.SDL_Event e)
        {
            //throw new NotImplementedException();
        }

        public async Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            AnimationClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(name);
            if (AnimationClip == null)
                return false;

            PreviewViewport.Title = $"MaterialMesh:{name}";
            //PreviewViewport.OnInitialize = Initialize_PreviewMesh;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.MainWindow, UEngine.Instance.Config.MainRPolicyName, Rtti.UTypeDesc.TypeOf(UEngine.Instance.Config.MainWindowRPolicy), 0, 1);

            AnimationClipPropGrid.Target = AnimationClip;
            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }


        #endregion IAssetEditor

        #region ITickable
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
        #endregion ITickable
    }
}
namespace EngineNS.Animation.Asset
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UAnimationClipEditor))]
    public partial class UAnimationClip
    {

    }
}