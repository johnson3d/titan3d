using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    public class UPolicyEditor : Editor.IAssetEditor, IO.ISerializer, IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        uint mDockId = uint.MaxValue;
        public uint DockId { get => mDockId; set => mDockId = value; }
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public URenderPolicyAsset PolicyGraph { get; private set; }
        public UGraphRenderer GraphRenderer { get; } = new UGraphRenderer();
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public UPolicyEditor()
        {

        }
        ~UPolicyEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            NodePropGrid.Target = null;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

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

            PolicyGraph = URenderPolicyAsset.LoadAsset(name);
            PolicyGraph.PolicyGraph.PolicyEditor = this;

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();
            NodePropGrid.Target = PolicyGraph;

            GraphRenderer.SetGraph(this.PolicyGraph.PolicyGraph);

            return true;
        }
        public void OnCloseEditor()
        {
            Dispose();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        #endregion

        #region DrawUI
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
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Right", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            bool drawing = true;
            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
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
                ImGuiAPI.Separator();
            }
            else
            {
                drawing = false;
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawLeft();
            DrawRight();

            if (drawing)
            {
                if (PreviewDockId != 0)
                {
                    
                }
            }
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
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Set", in btSize))
            {
                var noused = SetCurrentPolicy();
            }
        }
        uint PreviewDockId = 0;
        private async System.Threading.Tasks.Task Save()
        {
            PolicyGraph.SaveAssetTo(AssetName);

            //Editor.USnapshot.Save(AssetName, PolicyGraph.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
        }
        private async System.Threading.Tasks.Task SetCurrentPolicy()
        {
            /*
            var mainWindow = UEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
            var saved = mainWindow.WorldViewportSlate.RenderPolicy;
            //var policy = new Graphics.Pipeline.Mobile.UMobileEditorFSPolicy();                                
            //await policy.Initialize(saved.DefaultCamera);
            //policy.OnResize(this.WorldViewportSlate.ClientSize.X, this.WorldViewportSlate.ClientSize.Y);
            Graphics.Pipeline.URenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.URenderPolicyAsset.LoadAsset(AssetName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy();
            }
            await policy.Initialize(saved.DefaultCamera);
            policy.OnResize(mainWindow.WorldViewportSlate.ClientSize.X, mainWindow.WorldViewportSlate.ClientSize.Y);
            policy.AddCamera("MainCamera", saved.DefaultCamera);
            policy.SetDefaultCamera("MainCamera");

            mainWindow.WorldViewportSlate.RenderPolicy = policy;
            saved.Cleanup();
            */

            UEngine.Instance.Config.MainRPolicyName = AssetName;
        }
        private async System.Threading.Tasks.Task Compile()
        {
            var policy = new Graphics.Pipeline.URenderPolicy();
            foreach (UPolicyNode i in PolicyGraph.PolicyGraph.Nodes)
            {
                if (false == policy.RegRenderNode(i.NodeId, i.GraphNode))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "PolicyEditor", $"Node{i.Name} regist failed");
                }
            }
            foreach (var i in PolicyGraph.PolicyGraph.Linkers)
            {
                var inNode = policy.FindNode(i.InNode.NodeId);
                if (inNode == null)
                    continue;
                var outNode = policy.FindNode(i.OutNode.NodeId);
                if (outNode == null)
                    continue;
                var inPin = inNode.FindInput(i.InPin.Name);
                if (inPin == null)
                    continue;
                var outPin = outNode.FindOutput(i.OutPin.Name);
                if (outPin == null)
                    continue;
                policy.AddLinker(outPin, inPin);
            }
            var root = policy.FindFirstNode<Graphics.Pipeline.Common.UCopy2SwapChainNode>();
            policy.RootNode = root;
            bool hasInputError = false;
            policy.BuildGraph(ref hasInputError);
            if (hasInputError == false)
            {

            }
            root.Dispose();
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = new Vector2(-1, -1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Preview", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    //ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                    //var winClass = new ImGuiWindowClass();
                    //winClass.UnsafeCallConstructor();
                    //var sz = ImGuiAPI.GetWindowSize();
                    //sz.Y = sz.X;
                    //ImGuiAPI.DockSpace(PreviewDockId, in sz, dockspace_flags, in winClass);
                    //winClass.UnsafeCallDestructor();
                }
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    NodePropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            var size = new Vector2(-1, -1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Right", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                GraphRenderer.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        #endregion
    }
}
