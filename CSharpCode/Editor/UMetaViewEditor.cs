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
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public EGui.Controls.PropertyGrid.PropertyGrid VersionPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Rtti.UMetaVersion CurrentMetaVersion;
        public void Cleanup()
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
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;

            var absFile = name.Address;
            var dir = IO.FileManager.GetBaseDirectory(absFile);
            var pureName = IO.FileManager.GetPureName(absFile);
            var version = System.Convert.ToUInt32(pureName);
            var descName = IO.FileManager.CombinePath(dir, "typedesc.txt");
            var typeStr = IO.FileManager.ReadAllText(descName);
            if (typeStr == null)
            {
                descName = IO.FileManager.CombinePath(dir, "typename.txt");
                typeStr = IO.FileManager.ReadAllText(descName);
                if (typeStr == null)
                    return false;
            }
            var typeDesc = Rtti.UTypeDesc.TypeOf(typeStr);
            if (typeDesc == null)
                return false;
            var meta = Rtti.UClassMetaManager.Instance.GetMeta(typeDesc);
            if (meta == null)
                return false;

            CurrentMetaVersion = meta.GetMetaVersion(version);
            VersionPropGrid.Target = CurrentMetaVersion;
            return true;
        }
        public void OnCloseEditor()
        {

        }
        public float LeftWidth = 0;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public float ScaleFactor = 1.0f;
        public unsafe void OnDraw()
        {
            if (Visible == false)
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
            ImGuiAPI.End();
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
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                VersionPropGrid.OnDraw(true, false, false);
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("TextureView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
            }
            ImGuiAPI.EndChild();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}
