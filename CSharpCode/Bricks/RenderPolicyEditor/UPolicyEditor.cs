using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    public class UPolicyEditor : Editor.IAssetEditor, IO.ISerializer, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
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
            Cleanup();
        }
        public void Cleanup()
        {
            NodePropGrid.Target = null;
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
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
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
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
                    
                }
            }
        }
        protected void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", in btSize))
            {
                var noused = Save();
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Compile", in btSize))
            {
                var noused = Compile();
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Set", in btSize))
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

            UEngine.Instance.Config.MainRPolicyName = AssetName;
        }
        private async System.Threading.Tasks.Task Compile()
        {
            var policy = new Graphics.Pipeline.URenderPolicy();
            foreach (UPolicyNode i in PolicyGraph.PolicyGraph.Nodes)
            {
                if (false == policy.RegRenderNode(i.Name, i.GraphNode))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "PolicyEditor", $"Node{i.Name} regist failed");
                }
            }
            foreach (var i in PolicyGraph.PolicyGraph.Linkers)
            {
                var inNode = policy.FindNode(i.InNode.Name);
                if (inNode == null)
                    continue;
                var outNode = policy.FindNode(i.OutNode.Name);
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
            root.Cleanup();
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = ImGuiAPI.GetWindowSize();
                sz.Y = sz.X;
                ImGuiAPI.DockSpace(PreviewDockId, in sz, dockspace_flags, in winClass);
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen))
                {
                    NodePropGrid.OnDraw(true, false, false);
                }
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                GraphRenderer.OnDraw();
            }
            ImGuiAPI.EndChild();
        }
        #endregion
    }
}
