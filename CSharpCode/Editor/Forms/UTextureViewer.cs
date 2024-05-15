using EngineNS.NxRHI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Editor.Forms
{
    public class USlateTextureViewerShading : Graphics.Pipeline.Shader.UGraphicsShadingEnv
    {
        public USlateTextureViewerShading()
        {
            CodeName = RName.GetRName("shaders/slate/Slate_TextureViewer.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
            };
        }
    }

    public class UTextureViewer : Editor.IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        TtTextureViewerCmdParams CmdParameters;
        public Graphics.Pipeline.Shader.UEffect SlateEffect;
        public NxRHI.USrView TextureSRV;
        public NxRHI.USrView ShowTextureSRV;
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
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await TexturePropGrid.Initialize();
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg)
        {
            AssetName = name;
            TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetOrNewTexture(name);
            if (TextureSRV == null)
                return false;

            TexturePropGrid.Target = TextureSRV;
            ImageSize.X = TextureSRV.PicDesc.Width;
            ImageSize.Y = TextureSRV.PicDesc.Height;
            if(Math.Min(ImageSize.X, ImageSize.Y) < 256)
            {
                ScaleFactor = (float)256 / Math.Min(ImageSize.X, ImageSize.Y);
                ScaleSpeed = 0.1f * ScaleFactor;
            }    

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            SlateEffect = await UEngine.Instance.GfxDevice.EffectManager.GetEffect(
                UEngine.Instance.ShadingEnvManager.GetShadingEnv<USlateTextureViewerShading>(),
                UEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.UMdfStaticMesh());
            var iptDesc = new NxRHI.UInputLayoutDesc();
            unsafe
            {
                iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
            }
            iptDesc.mCoreObject.SetShaderDesc(SlateEffect.DescVS.mCoreObject);
            var InputLayout = rc.CreateInputLayout(iptDesc); //UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
            SlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

            
            var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<TtTextureViewerCmdParams>();
            var cbBinder = SlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
            cmdParams.CBuffer = rc.CreateCBV(cbBinder);
            cmdParams.Drawcall.BindShaderEffect(SlateEffect);
            cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
            cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
            cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, UEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            cmdParams.IsNormalMap = 0;
            if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                cmdParams.IsNormalMap = 1;

            CmdParameters = cmdParams;

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
        public float ScaleSpeed = 0.1f;
        public unsafe void OnDraw()
        {
            if (Visible == false || TextureSRV == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(TextureSRV.AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
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
                var imgType = NxRHI.USrView.GetOriginImageType(AssetName);

                switch (imgType)
                {
                    case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                        {
                            StbImageSharp.ImageResult image = NxRHI.USrView.LoadOriginPng(AssetName);
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
                            NxRHI.USrView.LoadOriginHdr(AssetName, ref imageFloat);
                            using (var xnd = new IO.TtXndHolder("USrView", 0, 0))
                            {
                                NxRHI.USrView.SaveTexture(AssetName, xnd.RootNode.mCoreObject, imageFloat, this.TextureSRV.PicDesc);
                                xnd.SaveXnd(AssetName.Address);
                            }
                        }
                        break;
                    case EngienNS.Bricks.ImageDecoder.UImageType.EXR:
                        {
                            System.IO.Stream outStream = null;
                            var file = NxRHI.USrView.LoadOriginExr(AssetName, ref outStream);
                            using (var xnd = new IO.TtXndHolder("USrView", 0, 0))
                            {
                                NxRHI.USrView.SaveTexture(AssetName, xnd.RootNode.mCoreObject, file, this.TextureSRV.PicDesc);
                                xnd.SaveXnd(AssetName.Address);
                            }
                        }
                        break;
                }
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("SaveOriginFile", in btSize))
            {
                USrView.SaveOriginImage(AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("R", ref mShowR, in btSize, 0))
            {
                CmdParameters.ColorMask.X = mShowR ? 1 : 0;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("G", ref mShowG, in btSize, 0))
            {
                CmdParameters.ColorMask.Y = mShowG ? 1 : 0;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("B", ref mShowB, in btSize, 0))
            {
                CmdParameters.ColorMask.Z = mShowB ? 1 : 0;
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.ToggleButton("A", ref mShowA, in btSize, 0))
            {
                CmdParameters.ColorMask.W = mShowA ? 1 : 0;
            }
        }
        bool mShowR = true;
        bool mShowG = true;
        bool mShowB = true;
        bool mShowA = false;
        bool mLeftShow = true;
        protected unsafe void DrawLeft()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Left", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                TexturePropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool mTextureViewShow = true;
        protected unsafe void DrawRight()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextureView", ref mTextureViewShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_ChildWindows))
                {
                    if (ImGuiAPI.GetIO().MouseWheel != 0)
                    {
                        ScaleFactor += ImGuiAPI.GetIO().MouseWheel * ScaleSpeed;

                        if (ScaleFactor <= 0.1f)
                            ScaleFactor = 0.1f;
                        if (ScaleFactor >= 256.0f)
                            ScaleFactor = 256.0f;
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

                if (CmdParameters != null)
                    drawlist.AddImage(CmdParameters.GetHandle(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                //drawlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in min1, in max1, in uv1, in uv2, 0xFFFFFFFF);
                drawlist.AddRect(in min1, in max1, 0xFF00FF00, 0, ImDrawFlags_.ImDrawFlags_None, 0);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }

    public class TtTextureViewerCmdParams : EGui.TtImDrawCmdParameters
    {
        public NxRHI.UCbView CBuffer;
        public Vector4i ColorMask = new Vector4i(1,1,1,0);
        public int IsNormalMap = 0; 
        public override void OnDraw(in Matrix mvp)
        {
            CBuffer.SetValue("ProjectionMatrix", in mvp);
            CBuffer.SetValue("ColorMask", in ColorMask);
            CBuffer.SetValue("IsNormalMap", in IsNormalMap);
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