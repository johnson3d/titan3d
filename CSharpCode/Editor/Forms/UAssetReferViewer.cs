using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Editor.Forms
{
    public partial class UAssetReferNode : UNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
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

            AddPinIn(pin);
        }
        public void AddOutput(PinOut pin, string name)
        {
            pin.Name = name;
            pin.LinkDesc = NewInOutPinDesc();

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
                var linkers = new List<UPinLinker>();
                this.ParentGraph.FindInLinker(InPin, linkers);
                var pos = Position;
                pos.X -= 600;
                var holders = new List<IO.IAssetMeta>();
                UEngine.Instance.AssetMetaManager.GetAssetHolder(AMeta, holders);
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
                        UEngine.Instance.EventPoster.RunOn(() =>
                        {
                            this.ParentGraph.AddNode(node);
                            node.Position = curPos;
                            this.ParentGraph.AddLink(node.OutPin, InPin, true);
                            return null;
                        }, Thread.Async.EAsyncTarget.Main);

                        pos.Y += 100;
                    }
                }
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.Button("Out"))
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
                        node.AMeta = UEngine.Instance.AssetMetaManager.GetAssetMeta(i);
                        var curPos = pos;
                        UEngine.Instance.EventPoster.RunOn(() =>
                        {
                            this.ParentGraph.AddNode(node);
                            node.Position = curPos;
                            this.ParentGraph.AddLink(OutPin, node.InPin, true);
                            return null;
                        }, Thread.Async.EAsyncTarget.Main);

                        pos.Y += 100;
                    }
                }
            }
            ImGuiAPI.PopID();
        }
    }

    public partial class UAssetReferGraph : UNodeGraph
    {
    }

    class UAssetReferViewer : Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public IO.IAssetMeta EditAsset { get; private set; }
        public UAssetReferGraph ReferGraph = new UAssetReferGraph();
        public UGraphRenderer GraphRenderer { get; } = new UGraphRenderer();

        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);

        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public void Cleanup()
        {
            //NodePropGrid.Target = null;
        }
        #region IAssetEditor
        bool IsStarting = false;
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
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

                }
            }
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("ExportRefs", in btSize))
            {
                
            }
        }
        uint PreviewDockId = 0;
        
        private async System.Threading.Tasks.Task Compile()
        {
            
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = Vector2.MinusOne;
            if (ImGuiAPI.BeginChild("LeftWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.CollapsingHeader("Preview", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                    var winClass = new ImGuiWindowClass();
                    winClass.UnsafeCallConstructor();
                    var sz = ImGuiAPI.GetWindowSize();
                    sz.Y = sz.X;
                    ImGuiAPI.DockSpace(PreviewDockId, in sz, dockspace_flags, in winClass);
                    winClass.UnsafeCallDestructor();
                }
                //if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                //{
                //    NodePropGrid.OnDraw(true, false, false);
                //}
                //if (ImGuiAPI.CollapsingHeader("EditorProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                //{
                //    GraphPropGrid.OnDraw(true, false, false);
                //}
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = Vector2.MinusOne;
            if (ImGuiAPI.BeginChild("RightWindow", in size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                GraphRenderer.OnDraw();
            }
            ImGuiAPI.EndChild();
        }
        #endregion
    }
}
