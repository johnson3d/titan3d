using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Slate
{
    public class BaseRenderer
    {
        public RHI.CRenderPipeline Pipeline;
        public RHI.CSamplerState SamplerState;
        public RHI.CConstantBuffer FontCBuffer;
        public RHI.CTexture2D FontTexture;
        public RHI.CShaderResourceView FontSRV;
        public unsafe virtual void Initialize(RName vs, RName ps)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var shaderCompiler = new Editor.ShaderCompiler.UHLSLCompiler();
            var vsDesc = shaderCompiler.CompileShader(vs.Address, "VS", EShaderType.EST_VertexShader, "5_0", null, null, null, true, true, false, false);

            var VertexShader = rc.CreateVertexShader(vsDesc);

            shaderCompiler = new Editor.ShaderCompiler.UHLSLCompiler();
            var psDesc = shaderCompiler.CompileShader(ps.Address, "FS", EShaderType.EST_PixelShader, "5_0", null, null, null, true, true, false, false);
            if (psDesc == null)
                return;
            var PixelShader = rc.CreatePixelShader(psDesc);

            var iptDesc = new RHI.CInputLayoutDesc();
            iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
            iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
            iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
            iptDesc.mCoreObject.SetShaderDesc(vsDesc.mCoreObject.Ptr);
            var InputLayout = UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc.mCoreObject);

            var spDesc = new IShaderProgramDesc();
            spDesc.InputLayout = InputLayout.mCoreObject.Ptr;
            spDesc.VertexShader = VertexShader.mCoreObject.Ptr;
            spDesc.PixelShader = PixelShader.mCoreObject.Ptr;
            var ShaderProgram = rc.CreateShaderProgram(ref spDesc);
            ShaderProgram.mCoreObject.LinkShaders(rc.mCoreObject.Ptr);

            var cbIndex = ShaderProgram.mCoreObject.FindCBuffer("ProjectionMatrixBuffer");
            FontCBuffer = rc.CreateConstantBuffer(ShaderProgram, cbIndex);

            var rstDesc = new IRasterizerStateDesc();
            rstDesc.SetDefault();
            rstDesc.ScissorEnable = 1;
            rstDesc.FillMode = EFillMode.FMD_SOLID;
            rstDesc.CullMode = ECullMode.CMD_NONE;
            var RasterizerState = UEngine.Instance.GfxDevice.RasterizerStateManager.GetPipelineState(rc, ref rstDesc);

            var dssDesc = new IDepthStencilStateDesc();
            dssDesc.SetDefault();
            dssDesc.DepthFunc = EComparisionMode.CMP_ALWAYS;
            var DepthStencilState = UEngine.Instance.GfxDevice.DepthStencilStateManager.GetPipelineState(rc, ref dssDesc);

            var bldDesc = new IBlendStateDesc();
            bldDesc.SetDefault();
            EngineNS.RenderTargetBlendDesc* pRenderTarget = bldDesc.RenderTarget;
            pRenderTarget[0].SetDefault();
            pRenderTarget[0].SrcBlendAlpha = EBlend.BLD_INV_SRC_ALPHA;
            pRenderTarget[0].DestBlendAlpha = EBlend.BLD_ONE;
            pRenderTarget[0].BlendEnable = 1;
            var BlendState = UEngine.Instance.GfxDevice.BlendStateManager.GetPipelineState(rc, ref bldDesc);

            var sdProgDesc = new IShaderProgramDesc();
            sdProgDesc.InputLayout = InputLayout.mCoreObject.Ptr;
            sdProgDesc.VertexShader = VertexShader.mCoreObject.Ptr;
            sdProgDesc.PixelShader = PixelShader.mCoreObject.Ptr;
            var gpuProgram = rc.CreateShaderProgram(ref sdProgDesc);

            var pipelineDesc = new IRenderPipelineDesc();
            pipelineDesc.SetDefault();
            pipelineDesc.Blend = BlendState.mCoreObject.Ptr;
            pipelineDesc.GpuProgram = gpuProgram.mCoreObject.Ptr;
            pipelineDesc.Rasterizer = RasterizerState.mCoreObject.Ptr;
            pipelineDesc.DepthStencil = DepthStencilState.mCoreObject.Ptr;
            Pipeline = rc.CreateRenderPipeline(ref pipelineDesc);

            var splDesc = new ISamplerStateDesc();
            splDesc.SetDefault();
            splDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
            splDesc.AddressU = EAddressMode.ADM_WRAP;
            splDesc.AddressV = EAddressMode.ADM_WRAP;
            splDesc.AddressW = EAddressMode.ADM_WRAP;
            splDesc.MipLODBias = 0;
            splDesc.MaxAnisotropy = 0;
            splDesc.CmpMode = EComparisionMode.CMP_ALWAYS;
            SamplerState = UEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(rc, ref splDesc);
        }
        public void Cleanup()
        {
            Pipeline?.Dispose();
            Pipeline = null;
            SamplerState = null;
            FontSRV?.Dispose();
            FontSRV = null;
            FontTexture?.Dispose();
            FontTexture = null;

            for(int i=0; i< mFontDataList.Count; ++i)
            {
                mFontDataList[i].FontSRV?.Dispose();
                mFontDataList[i].FontSRV = null;
                mFontDataList[i].FontTexture?.Dispose();
                mFontDataList[i].FontTexture = null;
                mFontDataList[i].SRCGCHandle.Free();
            }

            FontCBuffer?.Dispose();
            FontCBuffer = null;
        }

        public enum enFont
        {
            Font_15px        = 0,
            Font_Bold_13px   = 1,
            Font_13px        = 2,
        }

        class FontDatas
        {
            public ImFont Font;
            public RHI.CTexture2D FontTexture;
            public RHI.CShaderResourceView FontSRV;
            public System.Runtime.InteropServices.GCHandle SRCGCHandle;
        }
        List<FontDatas> mFontDataList = new List<FontDatas>();
        public unsafe void RecreateFontDeviceTexture()
        {
            //var io = ImGuiAPI.GetIO();
            //ImFontConfig fontConfig = new ImFontConfig();
            //fontConfig.UnsafeCallConstructor();
            //fontConfig.MergeMode = true;
            ////io.FontsWrapper.AddFontDefault(ref mFontConfig);
            //Font_15px = io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 15.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());
            //Font_Bold_13px = io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Bold.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //Font_13px = io.Fonts.AddFontFromFileTTF(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //// Build
            //byte* pixels;
            //int width = 0, height = 0, bytesPerPixel = 0;
            //io.Fonts.GetTexDataAsRGBA32(&pixels, ref width, ref height, ref bytesPerPixel);
            //// Store our identifier
            //io.Fonts.SetTexID((void*)0);

            //ImageInitData initData;
            //initData.pSysMem = pixels;
            //initData.SysMemPitch = (uint)(width * bytesPerPixel);

            //var rc = UEngine.Instance.GfxDevice.RenderContext;
            //var txDesc = new ITexture2DDesc();
            //txDesc.SetDefault();
            //txDesc.Width = (uint)width;
            //txDesc.Height = (uint)height;
            //txDesc.MipLevels = 1;
            //txDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //txDesc.InitData = &initData;
            //FontTexture = rc.CreateTexture2D(ref txDesc);

            //var srvDesc = new IShaderResourceViewDesc();
            //srvDesc.mFormat = txDesc.Format;
            //srvDesc.m_pTexture2D = FontTexture.mCoreObject.Ptr;
            //FontSRV = rc.CreateShaderResourceView(ref srvDesc);

            //io.Fonts.ClearTexData();

            var io = ImGuiAPI.GetIO();
            CreateFontTexture(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 15.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());

            //ImFontConfig fontConfig = new ImFontConfig();
            //fontConfig.UnsafeCallConstructor();
            //fontConfig.MergeMode = true;
            //CreateFontTexture(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Bold.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            //CreateFontTexture(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 13.0f, &fontConfig, io.Fonts.GetGlyphRangesDefault());
            CreateFontTexture(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Bold.ttf", 13.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());
            CreateFontTexture(UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Engine) + "fonts/Roboto-Regular.ttf", 13.0f, (ImFontConfig*)0, io.Fonts.GetGlyphRangesDefault());
        }

        unsafe void CreateFontTexture(string absFontFile, float size_pixels, ImFontConfig* fontConfig, Wchar16* glyph_ranges)
        {
            var fontData = new FontDatas();

            var io = ImGuiAPI.GetIO();
            fontData.Font = io.Fonts.AddFontFromFileTTF(absFontFile, size_pixels, fontConfig, glyph_ranges);
            byte* pixels;
            int width = 0, height = 0, bytesPerPixel = 0;
            io.Fonts.GetTexDataAsRGBA32(&pixels, ref width, ref height, ref bytesPerPixel);

            ImageInitData initData;
            initData.pSysMem = pixels;
            initData.SysMemPitch = (uint)(width * bytesPerPixel);

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var txDesc = new ITexture2DDesc();
            txDesc.SetDefault();
            txDesc.Width = (uint)width;
            txDesc.Height = (uint)height;
            txDesc.MipLevels = 1;
            txDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            txDesc.InitData = &initData;
            fontData.FontTexture = rc.CreateTexture2D(ref txDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.mFormat = txDesc.Format;
            srvDesc.m_pTexture2D = fontData.FontTexture.mCoreObject.Ptr;
            fontData.FontSRV = rc.CreateShaderResourceView(ref srvDesc);

            var texId = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(fontData.FontSRV));
            io.Fonts.SetTexID(texId.ToPointer());

            io.Fonts.ClearTexData();

            mFontDataList.Add(fontData);
        }

        public unsafe void PushFont(int fontIdx)
        {
            if (fontIdx < 0 || (int)fontIdx >= mFontDataList.Count)
                return;

            if (mFontDataList[(int)fontIdx] == null)
                return;

            ImGuiAPI.PushFont(mFontDataList[(int)fontIdx].Font.Ptr);
        }
        public void PopFont()
        {
            ImGuiAPI.PopFont();
        }
    }
}
