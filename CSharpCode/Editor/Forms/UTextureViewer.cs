using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UTextureViewer : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public NxRHI.USrView TextureSRV;
        public EGui.Controls.PropertyGrid.PropertyGrid TexturePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        ~UTextureViewer()
        {
            Dispose();
        }
        public void Dispose()
        {
            TexturePropGrid.Target = null;
            if (TextureSRV != null)
            {
                TextureSRV.FreeTextureHandle();
                TextureSRV = null;
            }
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await TexturePropGrid.Initialize();
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetOrNewTexture(name);
            if (TextureSRV == null)
                return false;

            TexturePropGrid.Target = TextureSRV;
            ImageSize.X = TextureSRV.PicDesc.Width;
            ImageSize.Y = TextureSRV.PicDesc.Height;
            return true;
        }
        public void OnCloseEditor()
        {
            Dispose();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Left", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextureView", mDockKeyClass), rightId);
            ImGuiAPI.DockBuilderFinish(id);
        }
        public Vector2 WindowSize = new Vector2(800, 600);
        public Vector2 ImageSize = new Vector2(512, 512);
        public float ScaleFactor = 1.0f;
        public unsafe void OnDraw()
        {
            if (Visible == false || TextureSRV == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (EGui.UIProxy.DockProxy.BeginMainForm(TextureSRV.AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                DrawToolBar();
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm();

            DrawLeft();
            DrawRight();
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                var imgType = NxRHI.USrView.GetOriginImageType(AssetName);

                switch(imgType)
                {
                    case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                        {
                            StbImageSharp.ImageResult image = NxRHI.USrView.LoadOriginImage(AssetName);
                            using (var xnd = new IO.TtXndHolder("USrView", 0, 0))
                            {
                                NxRHI.USrView.SaveTexture(AssetName, xnd.RootNode.mCoreObject, image, this.TextureSRV.PicDesc);
                                xnd.SaveXnd(AssetName.Address);
                            }
                        }
                        break;
                    case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                        {
                            StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                            NxRHI.USrView.LoadOriginImage(AssetName, ref imageFloat);
                            using (var xnd = new IO.TtXndHolder("USrView", 0, 0))
                            {
                                NxRHI.USrView.SaveTexture(AssetName, xnd.RootNode.mCoreObject, imageFloat, this.TextureSRV.PicDesc);
                                xnd.SaveXnd(AssetName.Address);
                            }
                        }
                        break;
                }
                if(imgType == EngienNS.Bricks.ImageDecoder.UImageType.PNG)
                {
                }
                
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("SaveOriginFile", in btSize))
            {
                USrView.SaveOriginPng(AssetName);
            }
        }
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var sz = new Vector2(-1);
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                TexturePropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        bool mTextureViewShow = true;
        protected unsafe void DrawRight()
        {
            if (EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextureView", ref mTextureViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None))
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
                drawlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                drawlist.AddRect(in min1, in max1, 0xFF00FF00, 0, ImDrawFlags_.ImDrawFlags_None, 0);
            }
            EGui.UIProxy.DockProxy.EndPanel();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            
        }
    }
}

namespace EngineNS.NxRHI
{
    [Editor.UAssetEditor(EditorType = typeof(Editor.Forms.UTextureViewer))]
    public partial class USrView
    {
    }
}