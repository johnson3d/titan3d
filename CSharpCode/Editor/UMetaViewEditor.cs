using EngineNS.GamePlay.Character;
using System;
using System.Collections.Generic;

namespace EngineNS.Editor
{
    public class UMetaVersionViewer : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public EGui.Controls.PropertyGrid.PropertyGrid VersionPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Rtti.UMetaVersion CurrentMetaVersion;
        public void Dispose()
        {
            VersionPropGrid.Target = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await VersionPropGrid.Initialize();
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;

            var absFile = name.Address;
            var dir = IO.TtFileManager.GetBaseDirectory(absFile);
            var pureName = IO.TtFileManager.GetPureName(absFile);
            var version = System.Convert.ToUInt32(pureName);
            var descName = IO.TtFileManager.CombinePath(dir, "typedesc.txt");
            var typeStr = IO.TtFileManager.ReadAllText(descName);
            if (typeStr == null)
            {
                descName = IO.TtFileManager.CombinePath(dir, "typename.txt");
                typeStr = IO.TtFileManager.ReadAllText(descName);
                if (typeStr == null)
                    return false;
            }
            var typeDesc = Rtti.UTypeDesc.TypeOf(typeStr);
            if (typeDesc == null)
                return false;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
            if (meta == null)
                return false;

            CurrentMetaVersion = meta.GetMetaVersion(version);
            VersionPropGrid.Target = CurrentMetaVersion;
            return true;
        }
        public void OnCloseEditor()
        {

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
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("LeftView", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextureView", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public float ScaleFactor = 1.0f;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                DrawToolBar();
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawLeft();
            DrawRight();
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {

            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Load", in btSize))
            {

            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "LeftView", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                VersionPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool mRightShow = true;
        protected unsafe void DrawRight()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextureView", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}
