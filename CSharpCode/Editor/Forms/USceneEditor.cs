using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Editor.Forms
{
    public class USceneEditor : Editor.IAssetEditor, ITickable, Graphics.Pipeline.IRootForm
    {
        public class USceneEditorViewport : EGui.Slate.UWorldViewportSlate
        {
            protected override void OnHitproxySelected(Graphics.Pipeline.IProxiable proxy)
            {
                base.OnHitproxySelected(proxy);

                if (proxy == null)
                {
                    this.ShowBoundVolumes(false, null);
                    return;
                }
                var node = proxy as GamePlay.Scene.UNode;
                if (node != null)
                {
                    this.ShowBoundVolumes(true, node);
                }
            }
        }
        public class USceneEditorOutliner : UWorldOutliner
        {
            public USceneEditorOutliner(bool regRoot)
                : base(regRoot)
            {

            }            
            protected override void OnNodeUI_LClick(INodeUIProvider provider)
            {
                base.OnNodeUI_LClick(provider);
            }
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public GamePlay.Scene.UScene Scene;
        public USceneEditorViewport PreviewViewport = new USceneEditorViewport();
        public USceneEditorOutliner mWorldOutliner = new USceneEditorOutliner(false);
        public EGui.Controls.PropertyGrid.PropertyGrid ScenePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();        
        ~USceneEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            Scene = null;
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            ScenePropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await ScenePropGrid.Initialize();
            return true;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewScene(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.IRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.RenderPolicy.Initialize(1, 1);

            (viewport as EGui.Slate.UWorldViewportSlate).CameraController.Camera = viewport.RenderPolicy.GetBasePassNode().GBuffers.Camera;

            Scene.Parent = PreviewViewport.World.Root;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            Scene = await UEngine.Instance.SceneManager.GetScene(name);
            if (Scene == null)
                return false;

            PreviewViewport.Title = $"Scene:{name}";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.MainWindow, new Graphics.Pipeline.Mobile.UMobileEditorFSPolicy(), 0, 1);

            ScenePropGrid.Target = Scene;

            mWorldOutliner.mWorld = PreviewViewport.World;
            mWorldOutliner.Title = $"Outliner:{name}";

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
            if (Visible == false || Scene == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
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
        protected void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", in btSize))
            {
                Action action = async () =>
                {
                    Scene.ClearChildren();
                    await EngineNS.Editor.MetaViewEditor.TestCreateScene(Scene);
                    Scene.SaveAssetTo(AssetName);
                };
                action();
                
                USnapshot.Save(AssetName, Scene.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
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
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ScenePropGrid.OnDraw(true, false, false);
                }
                mWorldOutliner.DrawAsChildWindow(ref sz);
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

namespace EngineNS.GamePlay.Scene
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.USceneEditor))]
    public partial class UScene
    {
    }
}