using SDL2;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class UUvAnimEditor : Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public UUvAnim UvAnim;
        public EGui.Controls.PropertyGrid.PropertyGrid UvAnimPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UUvAnimEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            UvAnimPropGrid.Target = null;
            UvAnim = null;
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await UvAnimPropGrid.Initialize();
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            UvAnim = await UEngine.Instance.GfxDevice.UvAnimManager.CreateUVAnim(name);
            if (UvAnim == null)
                return false;

            UvAnimPropGrid.Target = UvAnim;
            if (UvAnim.mTexture != null)
            {
                ImageSize.X = UvAnim.mTexture.PicDesc.Width;
                ImageSize.Y = UvAnim.mTexture.PicDesc.Height;
            }
            return true;
        }
        public void OnCloseEditor()
        {
            Cleanup();
        }
        public float LeftWidth = 0;
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public float ScaleFactor = 1.0f;
        public unsafe void OnDraw()
        {
            if (Visible == false || UvAnim == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(UvAnim.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
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
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", in btSize))
            {
                UvAnim.SaveAssetTo(UvAnim.AssetName);
                var unused = UEngine.Instance.GfxDevice.UvAnimManager.ReloadUVAnim(UvAnim.AssetName);
            }
        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                UvAnimPropGrid.OnDraw(true, false, false);
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("TextureView", in sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows))
                {
                    if ( ImGuiAPI.GetIO().MouseWheel != 0)
                    {
                        ScaleFactor += ImGuiAPI.GetIO().MouseWheel * 0.1f;

                        if (ScaleFactor <= 0.1f)
                            ScaleFactor = 0.1f;
                        if (ScaleFactor >= 3.0f)
                            ScaleFactor = 3.0f;
                    }
                }
                var pos = ImGuiAPI.GetWindowPos();
                var drawlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                var uv1 = new Vector2(0, 0);
                var uv2 = new Vector2(1, 1);
                var min1 = ImGuiAPI.GetWindowContentRegionMin();
                var max1 = min1 + ImageSize * ScaleFactor;

                min1 = min1 + pos;
                max1 = max1 + pos;
                if (UvAnim.mTexture != null)
                    drawlist.AddImage(UvAnim.mTexture.GetTextureHandle().ToPointer(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
            }
            ImGuiAPI.EndChild();
        }
        public void OnEvent(ref SDL.SDL_Event e)
        {
            
        }
    }
}
