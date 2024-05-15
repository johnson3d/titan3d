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
            var styles = UNodeGraphStyles.DefaultStyles;

            var result = new LinkDesc();
            result.Icon.TextureName = RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine);
            result.Icon.Size = new Vector2(15, 11);
            result.DisconnectIcon.TextureName = RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine);
            result.DisconnectIcon.Size = new Vector2(15, 11);

            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
    }
    public class UPgcEditor : Editor.IAssetEditor, IO.ISerializer, IRootForm, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        uint mDockId = uint.MaxValue;
        public uint DockId { get => mDockId; set => mDockId = value; }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
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
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PreviewViewport);
            NodePropGrid.Target = null;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            await mUnionNodeConfigRenderer.Initialize();
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
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterial(Graphics.Pipeline.UViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
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
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
        #region IAssetEditor
        bool IsStarting = false;
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
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

            mDockKeyClass = new ImGuiWindowClass()
            {
                ClassId = 0,
                ParentViewportId = uint.MaxValue,
                ViewportFlagsOverrideSet = ImGuiViewportFlags_.ImGuiViewportFlags_TopMost,
                ViewportFlagsOverrideClear = ImGuiViewportFlags_.ImGuiViewportFlags_None,
                TabItemFlagsOverrideSet = ImGuiTabItemFlags_.ImGuiTabItemFlags_None,
                DockNodeFlagsOverrideSet = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None,
                DockingAlwaysTabBar = false,
                DockingAllowUnclassed = true,
            };

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        #endregion

        #region DrawUI
        //public Vector2 WindowPos;
        //public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawPgcGraph();

            DrawCameralDetails();
            DrawPreview();
            
            DrawGraphDetails();
            DrawNodeDetails();
            DrawUnionNodeConfig();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("PgcGraph", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("CameralDetails", mDockKeyClass), rightUpId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("GraphDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("UnionNodeConfig", mDockKeyClass), rightDownId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                var noused = Save();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Compile", in btSize))
            {
                var noused = Compile();
            }
        }
        bool ShowNodeGraph = true;
        protected void DrawPgcGraph()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "PgcGraph", ref ShowNodeGraph, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                GraphRenderer.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowGrapPropGrid = true;
        protected void DrawGraphDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "GraphDetails", ref ShowGrapPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                GraphPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowNodePropGrid = true;
        protected void DrawNodeDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref ShowNodePropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (GraphRenderer.Graph != null && GraphRenderer.Graph.SelectedNodesDirty)
                {
                    NodePropGrid.Target = GraphRenderer.Graph.SelectedNodes.ToArray();
                    GraphRenderer.Graph.SelectedNodesDirty = false;
                }
                NodePropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowCameralPropGrid = true;
        protected unsafe void DrawCameralDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "CameralDetails", ref ShowCameralPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
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
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.UViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            this.PreviewViewport.Visible = show;
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        NodeGraph.UnionNodeConfigRenderer mUnionNodeConfigRenderer = new NodeGraph.UnionNodeConfigRenderer();
        bool mUnionNodeConfigShow = false;
        protected void DrawUnionNodeConfig()
        {
            if (!mUnionNodeConfigShow)
                return;
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "UnionNodeConfig", ref mUnionNodeConfigShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mUnionNodeConfigRenderer.DrawConfigPanel();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void SetConfigUnionNode(NodeGraph.IUnionNode node)
        {
            mUnionNodeConfigRenderer?.SetUnionNode(node);
            mUnionNodeConfigShow = (node != null);
        }

        //uint PreviewDockId = 0;
        private async System.Threading.Tasks.Task Save()
        {
            EditAsset.SaveAssetTo(AssetName);
        }
        private async System.Threading.Tasks.Task Compile()
        {
            await UEngine.Instance.EventPoster.Post((state) =>
            {
                EditAsset.Compile(EditAsset.AssetGraph.Root);
                return true;
            }, Thread.Async.EAsyncTarget.Logic);
        }
        #endregion
    }
}
