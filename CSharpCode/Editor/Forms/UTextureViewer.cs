using SDL2;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UTextureViewer : Editor.IAssetEditor, Graphics.Pipeline.IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public RHI.CShaderResourceView TextureSRV;
        public EGui.Controls.PropertyGrid.PropertyGrid TexturePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public void Cleanup()
        {
            if (TextureID != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(TextureID);
                handle.Free();
                TextureID = IntPtr.Zero;
            }
        }
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(name);
            if (TextureSRV == null)
                return false;
            TexturePropGrid.Initialize();

            TexturePropGrid.Target = TextureSRV;
            ImageSize.X = TextureSRV.PicDesc.Width;
            ImageSize.Y = TextureSRV.PicDesc.Height;

            TextureID = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(TextureSRV));
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
        private IntPtr TextureID;
        public unsafe void OnDraw()
        {
            if (Visible == false || TextureSRV == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(ref WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(TextureSRV.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
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

        }
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("LeftView", ref sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                TexturePropGrid.OnDraw(true, false, false);
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var sz = new Vector2(-1);
            if (ImGuiAPI.BeginChild("TextureView", ref sz, true, ImGuiWindowFlags_.ImGuiWindowFlags_None))
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
                drawlist.AddImage(TextureID.ToPointer(), ref min1, ref max1, ref uv1, ref uv2, 0xFFFFFFFF);
            }
            ImGuiAPI.EndChild();
        }
        public void OnEvent(ref SDL.SDL_Event e)
        {
            
        }
    }
}

namespace EngineNS.RHI
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UTextureViewer))]
    public partial class CShaderResourceView
    {
    }
}