using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Editor.Forms
{
    public partial class UAssetReferNode : UNodeBase
    {
        [Browsable(false)]
        public PinIn InPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutPin { get; set; } = new PinOut();
        public IO.IAssetMeta AMeta;

        public UAssetReferNode()
        {
            PrevSize = new Vector2(60, 50);
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InPin, "In");
            AddOutput(OutPin, "Out");
        }
        private LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
        public void AddInput(PinIn pin, string name)
        {
            pin.Name = name;
            pin.LinkDesc = NewInOutPinDesc();
            pin.MultiLinks = true;

            AddPinIn(pin);
        }
        public void AddOutput(PinOut pin, string name)
        {
            pin.Name = name;
            pin.LinkDesc = NewInOutPinDesc();
            pin.MultiLinks = true;

            AddPinOut(pin);
        }
        public override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            var ctrlPos = prevStart;
            ctrlPos -= ImGuiAPI.GetWindowPos();
            ImGuiAPI.SetCursorPos(in ctrlPos);
            ImGuiAPI.PushID($"{this.NodeId.ToString()}");
            if (ImGuiAPI.Button("In"))
            {
                if (AMeta != null)
                {
                    var linkers = new List<UPinLinker>();
                    this.ParentGraph.FindInLinker(InPin, linkers);
                    var pos = Position;
                    pos.X -= 600;
                    var holders = new List<IO.IAssetMeta>();
                    TtEngine.Instance.AssetMetaManager.GetAssetHolder(AMeta, holders);
                    foreach (var i in holders)
                    {
                        bool bFind = false;
                        foreach (var j in linkers)
                        {
                            if (j.InNode.Name == i.ToString())
                            {
                                bFind = true;
                            }
                        }
                        if (bFind == false)
                        {
                            var node = new UAssetReferNode();
                            node.Name = i.GetAssetName().ToString();
                            node.UserData = this;
                            node.AMeta = i;
                            var curPos = pos;
                            TtEngine.Instance.EventPoster.RunOn((state) =>
                            {
                                this.ParentGraph.AddNode(node);
                                node.Position = curPos;
                                this.ParentGraph.AddLink(node.OutPin, InPin, true);
                                return true;
                            }, Thread.Async.EAsyncTarget.Main);

                            pos.Y += 100;
                        }
                    }
                }
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.Button("Out"))
            {
                if (AMeta != null)
                {
                    var linkers = new List<UPinLinker>();
                    this.ParentGraph.FindOutLinker(OutPin, linkers);
                    var pos = Position;
                    pos.X += 600;
                    foreach (var i in AMeta.RefAssetRNames)
                    {
                        bool bFind = false;
                        foreach (var j in linkers)
                        {
                            if (j.InNode.Name == i.ToString())
                            {
                                bFind = true;
                            }
                        }
                        if (bFind == false)
                        {
                            var node = new UAssetReferNode();
                            node.Name = i.ToString();
                            node.UserData = this;
                            node.AMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(i);
                            var curPos = pos;
                            TtEngine.Instance.EventPoster.RunOn((state) =>
                            {
                                this.ParentGraph.AddNode(node);
                                node.Position = curPos;
                                this.ParentGraph.AddLink(OutPin, node.InPin, true);
                                return true;
                            }, Thread.Async.EAsyncTarget.Main);

                            pos.Y += 100;
                        }
                    }
                }
            }
            ImGuiAPI.PopID();
        }
    }

    public partial class UAssetReferGraph : UNodeGraph
    {
    }

    class UAssetReferViewer : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public IO.IAssetMeta EditAsset { get; private set; }
        public UAssetReferGraph ReferGraph = new UAssetReferGraph();
        public TtGraphRenderer GraphRenderer { get; } = new TtGraphRenderer();

        public float LeftWidth = 0;
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public void Dispose()
        {
            //NodePropGrid.Target = null;
        }
        #region IAssetEditor
        bool IsStarting = false;
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            IsStarting = true;

            EditAsset = arg as IO.IAssetMeta;

            //EditAsset = UPgcAsset.LoadAsset(name);
            //EditAsset.AssetGraph.GraphEditor = this;

            AssetName = name;
            IsStarting = false;

            //await NodePropGrid.Initialize();
            //NodePropGrid.Target = EditAsset;

            //await GraphPropGrid.Initialize();
            //GraphPropGrid.Target = this;

            var node = new UAssetReferNode();
            node.Name = EditAsset.GetAssetName().ToString();
            node.UserData = this;
            node.Position = Vector2.Zero;
            node.AMeta = EditAsset;
            ReferGraph.AddNode(node);

            GraphRenderer.SetGraph(ReferGraph);

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
        #endregion
        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
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
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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

            DrawRefGraph();
            DrawEditorDetails();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("RefGraph", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightDownId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("ExportRefs", in btSize))
            {

            }
        }

        bool ShowEditorPropGrid = true;
        protected void DrawEditorDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "EditorDetails", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawRefGraph()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "RefGraph", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                GraphRenderer.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        #endregion
    }
}
