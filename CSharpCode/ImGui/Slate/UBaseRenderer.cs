using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Slate
{
    public class UBaseRenderer
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
            var vsDesc = shaderCompiler.CompileShader(vs.Address, "VS", EShaderType.EST_VertexShader, "5_0", null, null, null, true);

            var VertexShader = rc.CreateVertexShader(vsDesc);

            shaderCompiler = new Editor.ShaderCompiler.UHLSLCompiler();
            var psDesc = shaderCompiler.CompileShader(ps.Address, "FS", EShaderType.EST_PixelShader, "5_0", null, null, null, true);
            if (psDesc == null)
                return;
            var PixelShader = rc.CreatePixelShader(psDesc);

            var iptDesc = new RHI.CInputLayoutDesc();
            iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
            iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
            iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
            iptDesc.mCoreObject.SetShaderDesc(vsDesc.mCoreObject);
            var InputLayout = UEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc.mCoreObject);

            var spDesc = new IShaderProgramDesc();
            spDesc.InputLayout = InputLayout.mCoreObject;
            spDesc.VertexShader = VertexShader.mCoreObject;
            spDesc.PixelShader = PixelShader.mCoreObject;
            var ShaderProgram = rc.CreateShaderProgram(ref spDesc);
            ShaderProgram.mCoreObject.LinkShaders(rc.mCoreObject);

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
            sdProgDesc.InputLayout = InputLayout.mCoreObject;
            sdProgDesc.VertexShader = VertexShader.mCoreObject;
            sdProgDesc.PixelShader = PixelShader.mCoreObject;
            var gpuProgram = rc.CreateShaderProgram(ref sdProgDesc);
            gpuProgram.mCoreObject.LinkShaders(rc.mCoreObject);

            var pipelineDesc = new IRenderPipelineDesc();
            pipelineDesc.SetDefault();
            pipelineDesc.Blend = BlendState.mCoreObject;
            pipelineDesc.GpuProgram = gpuProgram.mCoreObject;
            pipelineDesc.Rasterizer = RasterizerState.mCoreObject;
            pipelineDesc.DepthStencil = DepthStencilState.mCoreObject;
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
            FontCBuffer?.Dispose();
            FontCBuffer = null;
        }
        public unsafe void RecreateFontDeviceTexture()
        {
            var io = ImGuiAPI.GetIO();
            ImFontConfig mFontConfig = new ImFontConfig();
            mFontConfig.UnsafeCallConstructor();
            io.FontsWrapper.AddFontDefault(ref mFontConfig);
            // Build
            byte* pixels;
            int width = 0, height = 0, bytesPerPixel = 0;
            io.FontsWrapper.GetTexDataAsRGBA32(&pixels, ref width, ref height, ref bytesPerPixel);
            // Store our identifier
            io.FontsWrapper.SetTexID((void*)0);

            ImageInitData initData = new ImageInitData();
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
            FontTexture = rc.CreateTexture2D(ref txDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.mFormat = txDesc.Format;
            srvDesc.m_pTexture2D = FontTexture.mCoreObject;
            FontSRV = rc.CreateShaderResourceView(ref srvDesc);

            io.FontsWrapper.ClearTexData();
        }
    }
}
