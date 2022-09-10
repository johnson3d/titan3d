using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public class UPgcEditorStyles
    {
        public static UPgcEditorStyles Instance = new UPgcEditorStyles();
        public EGui.UUvAnim FunctionIcon = new EGui.UUvAnim(0xFF00FF00, 25);
        public uint FunctionTitleColor = 0xFF204020;
        public uint FunctionBGColor = 0x80808080;
        public LinkDesc NewInOutPinDesc(string linkType = "FloatBuffer")
        {
            var result = new LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
    }
    public class UPgcEditor : Editor.IAssetEditor, IO.ISerializer, Graphics.Pipeline.IRootForm, ITickable
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public UPgcAsset EditAsset { get; private set; }
        public UGraphRenderer GraphRenderer { get; } = new UGraphRenderer();
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid GraphPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Editor.UPreviewViewport PreviewViewport;
        [RName.PGRName(FilterExts = UPgcAsset.AssetExt)]
        public RName PreviewPGC { get; set; }
        public NxRHI.UGpuSystem GpuSystem { get; private set; }
        public NxRHI.UGpuDevice GpuDevice { get; private set; }
        public UPgcEditor()
        {
            PreviewViewport = new Editor.UPreviewViewport();
            PreviewPGC = RName.GetRName("template/emptyterrain.pgc", RName.ERNameType.Engine);
        }
        ~UPgcEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            NodePropGrid.Target = null;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var gpuDesc = new NxRHI.FGpuSystemDesc();
            unsafe
            {
                gpuDesc.CreateDebugLayer = 0;
                gpuDesc.WindowHandle = IntPtr.Zero.ToPointer();
            }
            GpuSystem = NxRHI.UGpuSystem.CreateGpuSystem(NxRHI.ERhiType.RHI_D3D11, in gpuDesc);
            NxRHI.FGpuDeviceDesc desc = new NxRHI.FGpuDeviceDesc();
            GpuDevice = GpuSystem.CreateGpuDevice(in desc);
            
            return true;
        }
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterial(Graphics.Pipeline.UViewportSlate viewport, Graphics.Pipeline.USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.UPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;

            PreviewRoot = await viewport.World.Root.NewNode(viewport.World, typeof(GamePlay.Scene.USubTreeRootNode), 
                new GamePlay.Scene.UNodeData() { Name = "PreviewRoot" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            PreviewRoot.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
            PreviewRoot.Parent = viewport.World.Root;
        }
        public GamePlay.Scene.USceneActorNode PreviewRoot { get; private set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion
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
        #region IAssetEditor
        bool IsStarting = false;
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            IsStarting = true;

            EditAsset = UPgcAsset.LoadAsset(name);
            EditAsset.AssetGraph.GraphEditor = this;

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();
            NodePropGrid.Target = EditAsset;

            await GraphPropGrid.Initialize();
            GraphPropGrid.Target = this;

            GraphRenderer.SetGraph(this.EditAsset.AssetGraph);

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialPreview:{AssetName}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterial;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }
        public void OnEvent(ref SDL2.SDL.SDL_Event e)
        {
        }
        #endregion

        #region DrawUI
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            bool drawing = true;
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
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
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
            else
            {
                drawing = false;
            }
            ImGuiAPI.End();

            if (drawing)
            {
                if (PreviewDockId != 0)
                {
                    PreviewViewport.DockId = PreviewDockId;
                    PreviewViewport.DockCond = ImGuiCond_.ImGuiCond_Always;
                    PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.Window;
                    PreviewViewport.OnDraw();
                }
            }
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (ImGuiAPI.Button("Save", in btSize))
            {
                var noused = Save();
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Compile", in btSize))
            {
                var noused = Compile();
            }
        }
        uint PreviewDockId = 0;
        private async System.Threading.Tasks.Task Save()
        {
            EditAsset.SaveAssetTo(AssetName);
        }
        private async System.Threading.Tasks.Task Compile()
        {
            await UEngine.Instance.EventPoster.Post(() =>
            {
                EditAsset.Compile(EditAsset.AssetGraph.Root);
                return true;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Preview", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    if (ImGuiAPI.CollapsingHeader("Camera", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                    {
                        float v = PreviewViewport.CameraMoveSpeed;
                        ImGuiAPI.SliderFloat("KeyMove", ref v, 1.0f, 150.0f, "%.3f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                        PreviewViewport.CameraMoveSpeed = v;

                        v = PreviewViewport.CameraMouseWheelSpeed;
                        ImGuiAPI.InputFloat("WheelMove", ref v, 0.1f, 1.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        PreviewViewport.CameraMouseWheelSpeed = v;

                        var zN = PreviewViewport.CameraController.Camera.mCoreObject.mZNear;
                        var zF = PreviewViewport.CameraController.Camera.mCoreObject.mZFar;
                        ImGuiAPI.SliderFloat("ZNear", ref zN, 0.1f, 100.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                        ImGuiAPI.SliderFloat("ZFar", ref zF, 10.0f, 10000.0f, "%.1f", ImGuiSliderFlags_.ImGuiSliderFlags_None);
                        if (zN != PreviewViewport.CameraController.Camera.mCoreObject.mZNear ||
                            zF != PreviewViewport.CameraController.Camera.mCoreObject.mZFar)
                        {
                            PreviewViewport.CameraController.Camera.SetZRange(zN, zF);
                        }

                        var camPos = PreviewViewport.CameraController.Camera.mCoreObject.GetPosition();
                        var saved = camPos;
                        ImGuiAPI.InputDouble($"X", ref camPos.X, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        ImGuiAPI.InputDouble($"Y", ref camPos.Y, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        ImGuiAPI.InputDouble($"Z", ref camPos.Z, 0.1, 10.0, "%.2f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        if (saved != camPos)
                        {
                            var lookAt = PreviewViewport.CameraController.Camera.mCoreObject.GetLookAt();
                            var up = PreviewViewport.CameraController.Camera.mCoreObject.GetUp();
                            PreviewViewport.CameraController.Camera.mCoreObject.LookAtLH(in camPos, lookAt - saved + camPos, up);
                        }
                    }
                    ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                    var winClass = new ImGuiWindowClass();
                    winClass.UnsafeCallConstructor();
                    var sz = ImGuiAPI.GetWindowSize();
                    sz.Y = sz.X;
                    ImGuiAPI.DockSpace(PreviewDockId, in sz, dockspace_flags, in winClass);
                    winClass.UnsafeCallDestructor();

                    this.PreviewViewport.Visible = true;
                }
                else
                {
                    this.PreviewViewport.Visible = false;
                }
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    NodePropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("EditorProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    GraphPropGrid.OnDraw(true, false, false);
                }
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", in Vector2.MinusOne, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                GraphRenderer.OnDraw();
            }
            ImGuiAPI.EndChild();
        }
        #endregion
    }
}
