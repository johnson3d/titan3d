using EngineNS.GamePlay.Character;
using EngineNS.IO;
using EngineNS.Rtti;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using static EngineNS.Editor.Forms.TtCpuProfiler.TtTimeScopeTree;

namespace EngineNS.Editor
{
    public class TtMetaVersionViewer : IRootForm
    {
        public TtMetaVersionViewer() 
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }
        public RName AssetName { get; set; }
        protected bool mVisible = false;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public EGui.Controls.PropertyGrid.PropertyGrid VersionPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Rtti.TtMetaVersion CurrentMetaVersion;
        public void Dispose()
        {
            VersionPropGrid.Target = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            return await VersionPropGrid.Initialize();
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID("MetaViewer_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var graphId = id;
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref graphId);
            uint propertyId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Right, 0.2f, ref propertyId, ref graphId);
            uint unionConfigId = 0;
            ImGuiAPI.DockBuilderSplitNode(graphId, ImGuiDir.ImGuiDir_Right, 0.4f, ref unionConfigId, ref graphId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MetaTree", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MetaVersion", mDockKeyClass), graphId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public float ScaleFactor = 1.0f;
        bool IsDrawing = false;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm("MetaViewer", this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawMetaTree();
            DrawMetaVersion();
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {

            }
        }
        public class TtMetaTree : Editor.TtTreeNodeDrawer
        {
            public class TtMetaNode : Editor.INodeUIProvider
            {
                public string AbsPath;
                public List<TtMetaNode> Children = null;
                public Rtti.TtClassMeta Meta;
                public Rtti.TtMetaVersion MetaVersion;
                public int NumOfChildUI()
                {
                    if (AbsPath == null)
                        return 0;
                    if (Children == null)
                    {
                        Children = new List<TtMetaNode>();
                        var dirs = IO.TtFileManager.GetDirectories(AbsPath, "*.*", false);
                        foreach (var i in dirs)
                        {
                            var tmp = new TtMetaNode();
                            tmp.AbsPath = i;
                            tmp.NodeName = IO.TtFileManager.GetLastestPathName(i);
                            Children.Add(tmp);
                        }
                        var txtFilepath = EngineNS.IO.TtFileManager.CombinePath(AbsPath, $"typedesc.txt");
                        var text = EngineNS.IO.TtFileManager.ReadAllText(txtFilepath);
                        if (text != null)
                        {
                            string assembly;
                            string typeStr;
                            Rtti.TtClassMeta.TypeDescText(text, out assembly, out typeStr);
                            Meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeStr);
                            NodeName = Meta.ClassType.Name;

                            foreach (var i in Meta.MetaVersions)
                            {
                                var tmp = new TtMetaNode();
                                tmp.AbsPath = null;
                                tmp.MetaVersion = i.Value;
                                tmp.NodeName = i.Value.MetaHash.ToString();
                                Children.Add(tmp);
                            }
                        }
                    }
                    return Children.Count;
                }
                public INodeUIProvider GetChildUI(int index)
                {
                    return Children[index];
                }
                public string NodeName
                {
                    get;
                    set;
                }
                public bool Selected { get; set; } = false;
                public GamePlay.TtWorld GetWorld()
                {
                    return null;
                }
                public bool DrawNode(INodeUIProvider parent, TtTreeNodeDrawer tree, int index, int NumOfChild)
                {
                    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanFullWidth;
                    if (this.Selected)
                        flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
                    bool ret = false;
                    string hit = "";
                    if (MetaVersion != null)
                    {
                        hit = $"({MetaVersion.HitCount})";
                    }

                    var name = (string.IsNullOrEmpty(NodeName) ? "EmptyName" : NodeName + hit) + "##" + index;
                    if (NumOfChild == 0)
                    {
                        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
                    }
                    ret = ImGuiAPI.TreeNodeEx(name, flags);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        tree.OnNodeUI_LClick(this);
                    }
                    return ret;
                }
            }
            public int TotalVersionCount { get; set; } = 0;
            public int UsedVersionCount { get; set; } = 0;

            public TtMetaNode RootNode = new TtMetaNode();
            public Rtti.TtMetaVersion mCurMetaVersion = null;
            public TtMetaTree(IO.TtFileManager.ERootDir rootType)
            {
                RootNode.AbsPath = TtEngine.Instance.FileManager.GetPath(rootType, TtFileManager.ESystemDir.MetaData);
                RootNode.NodeName = "Root";
            }
            public unsafe void OnDraw()
            {
                TotalVersionCount = 0;
                UsedVersionCount = 0;

                foreach (var i in TtClassMetaManager.Instance.Metas)
                {
                    foreach (var j in i.Value.MetaVersions)
                    {
                        if(j.Value.HitCount > 0)
                        {
                            UsedVersionCount++;
                        }
                    }
                    TotalVersionCount+= i.Value.MetaVersions.Count;
                }

                DrawTree(null,RootNode, 0);

                ImGuiAPI.Separator();
                ImGuiAPI.Text($"Total Version Count: {TotalVersionCount}");
                ImGuiAPI.Text($"Used Version Count: {UsedVersionCount}");
            }
            public override void OnNodeUI_LClick(INodeUIProvider provider)
            {
                var meta = provider as TtMetaNode;
                if (meta != null)
                {
                    if (meta.MetaVersion != null)
                    {
                        mCurMetaVersion = meta.MetaVersion;
                    }
                }
            }
        }
        public TtMetaTree mMetaTree = new TtMetaTree(IO.TtFileManager.ERootDir.Engine);
        bool ShowEditorPropGrid = true;
        protected void DrawMetaTree()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MetaTree", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mMetaTree.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowMeshPropGrid = true;
        protected void DrawMetaVersion()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MetaVersion", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (mMetaTree.mCurMetaVersion != null)
                {
                    ImGuiAPI.Columns(3, "SettingColumns", true);
                    foreach (var i in mMetaTree.mCurMetaVersion.Propertys)
                    {
                        ImGuiAPI.Text(i.PropertyName);
                        ImGuiAPI.NextColumn();
                        ImGuiAPI.Text(i.FieldType.FullName);
                        ImGuiAPI.NextColumn();
                        ImGuiAPI.Text(i.Order.ToString());
                        ImGuiAPI.NextColumn();
                    }
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}
