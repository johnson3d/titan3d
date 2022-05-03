using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct IRenderContextDesc
    {
        public string GPUName
        {
            get
            {
                return this.GetDeviceName();
            }
        }
    }
    public unsafe partial struct IRenderContextCaps
    {
        public void SetShaderGlobalEnv(IShaderDefinitions defPtr)
        {
            if (MaxVertexShaderStorageBlocks > 0)
            {
                defPtr.AddDefine("HW_VS_STRUCTUREBUFFER", "1");
            }
        }
        public override string ToString()
        {
            string result = "";
            if(MaxVertexShaderStorageBlocks > 0)
                result += "HW_VS_STRUCTUREBUFFER=1\n";
            return result;
        }
    }
}


namespace EngineNS.RHI
{
    public class CRenderContext : AuxPtrType<IRenderContext>
    {
        public ERHIType RHIType
        {
            get
            {
                return mCoreObject.GetRHIType();
            }
        }
        public int ShaderModel
        {
            get
            {
                return mCoreObject.GetShaderModel();
            }
        }
        public CSwapChain CreateSwapChain(ref ISwapChainDesc desc)
        {
            unsafe
            {
                var result = new CSwapChain();
                result.mCoreObject = mCoreObject.CreateSwapChain(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CCommandList CreateCommandList(ref ICommandListDesc desc)
        {
            unsafe
            {
                var result = new CCommandList();
                result.mCoreObject = mCoreObject.CreateCommandList(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CDrawCall CreateDrawCall(Graphics.Pipeline.Shader.UEffect effect)
        {
            var result = new CDrawCall();
            unsafe
            {
                result.mCoreObject = mCoreObject.CreateDrawCall();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;

                var desc = new IRenderPipelineDesc();
                desc.SetDefault();
                //desc.Blend = UEngine.Instance.GfxDevice.BlendStateManager.DefaultState.mCoreObject;
                //desc.DepthStencil = UEngine.Instance.GfxDevice.DepthStencilStateManager.DefaultState.mCoreObject;
                //desc.Rasterizer = UEngine.Instance.GfxDevice.RasterizerStateManager.DefaultState.mCoreObject;
                var pl = UEngine.Instance.GfxDevice.RenderContext.CreateRenderPipeline(in desc);
                pl.mCoreObject.BindGpuProgram(effect.ShaderProgram.mCoreObject);
                pl.mCoreObject.BindBlendState(UEngine.Instance.GfxDevice.BlendStateManager.DefaultState.mCoreObject);
                pl.mCoreObject.BindDepthStencilState(UEngine.Instance.GfxDevice.DepthStencilStateManager.DefaultState.mCoreObject);
                pl.mCoreObject.BindRasterizerState(UEngine.Instance.GfxDevice.RasterizerStateManager.DefaultState.mCoreObject);
                result.mCoreObject.BindPipeline(pl.mCoreObject);
            }
            return result;
        }
        public async System.Threading.Tasks.Task<CDrawCall> CreateDrawCall(Graphics.Pipeline.Shader.UShadingEnv shading, Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            var result = new CDrawCall();
            unsafe
            {
                result.mCoreObject = mCoreObject.CreateDrawCall();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;

                var desc = new IRenderPipelineDesc();
                desc.SetDefault();
                //desc.Blend = UEngine.Instance.GfxDevice.BlendStateManager.DefaultState.mCoreObject;
                //desc.DepthStencil = UEngine.Instance.GfxDevice.DepthStencilStateManager.DefaultState.mCoreObject;
                //desc.Rasterizer = UEngine.Instance.GfxDevice.RasterizerStateManager.DefaultState.mCoreObject;
                var pl = UEngine.Instance.GfxDevice.RenderContext.CreateRenderPipeline(in desc);
                pl.mCoreObject.BindBlendState(UEngine.Instance.GfxDevice.BlendStateManager.DefaultState.mCoreObject);
                pl.mCoreObject.BindDepthStencilState(UEngine.Instance.GfxDevice.DepthStencilStateManager.DefaultState.mCoreObject);
                pl.mCoreObject.BindRasterizerState(UEngine.Instance.GfxDevice.RasterizerStateManager.DefaultState.mCoreObject);
                result.mCoreObject.BindPipeline(pl.mCoreObject);
            }
            await result.UpdateShaderProgram(shading, material, mdf);
            return result;
        }
        public CComputeDrawcall CreateComputeDrawcall()
        {
            var result = new CComputeDrawcall();
            unsafe
            {
                result.mCoreObject = mCoreObject.CreateComputeDrawcall();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
            }
            return result;
        }
        public CCopyDrawcall CreateCopyDrawcall()
        {
            var result = new CCopyDrawcall();
            unsafe
            {
                result.mCoreObject = mCoreObject.CreateCopyDrawcall();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
            }
            return result;
        }
        public CRenderPipeline CreateRenderPipeline(in IRenderPipelineDesc desc)
        {
            unsafe
            {
                var result = new CRenderPipeline();
                result.mCoreObject = mCoreObject.CreateRenderPipeline(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CVertexBuffer CreateVertexBuffer(in IVertexBufferDesc desc)
        {
            unsafe
            {
                var result = new CVertexBuffer();
                result.mCoreObject = mCoreObject.CreateVertexBuffer(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CIndexBuffer CreateIndexBuffer(in IIndexBufferDesc desc)
        {
            unsafe
            {
                var result = new CIndexBuffer();
                result.mCoreObject = mCoreObject.CreateIndexBuffer(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CInputLayout CreateInputLayout(IInputLayoutDesc desc)
        {
            unsafe
            {
                var result = new CInputLayout();
                result.mCoreObject = mCoreObject.CreateInputLayout(desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CGeometryMesh CreateGeometryMesh()
        {
            unsafe
            {
                var result = new CGeometryMesh();
                result.mCoreObject = mCoreObject.CreateGeometryMesh();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CIndexBuffer CreateIndexBufferFromBuffer(in IIndexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var result = new CIndexBuffer();
                result.mCoreObject = mCoreObject.CreateIndexBufferFromBuffer(in desc, pBuffer.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CVertexBuffer CreateVertexBufferFromBuffer(in IVertexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var result = new CVertexBuffer();
                result.mCoreObject = mCoreObject.CreateVertexBufferFromBuffer(in desc, pBuffer.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CRenderPass CreateRenderPass(in IRenderPassDesc desc)
        {
            unsafe
            {
                var result = new CRenderPass();
                result.mCoreObject = mCoreObject.CreateRenderPass(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CFrameBuffers CreateFrameBuffers(in IFrameBuffersDesc desc)
        {
            unsafe
            {
                var result = new CFrameBuffers();
                result.mCoreObject = mCoreObject.CreateFrameBuffers(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CRenderTargetView CreateRenderTargetView(in IRenderTargetViewDesc desc)
        {
            unsafe
            {
                var result = new CRenderTargetView();
                result.mCoreObject = mCoreObject.CreateRenderTargetView(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CDepthStencilView CreateDepthRenderTargetView(in IDepthStencilViewDesc desc)
        {
            unsafe
            {
                var result = new CDepthStencilView();
                result.mCoreObject = mCoreObject.CreateDepthRenderTargetView(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CTexture2D CreateTexture2D(in ITexture2DDesc desc)
        {
            unsafe
            {
                var result = new CTexture2D();
                result.mCoreObject = mCoreObject.CreateTexture2D(in desc).NativeSuper;
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderResourceView CreateShaderResourceView(in EngineNS.IShaderResourceViewDesc desc)
        {
            unsafe
            {
                var result = new CShaderResourceView();
                result.mCoreObject = mCoreObject.CreateShaderResourceView(in desc);
                if (desc.ViewDimension == SRV_DIMENSION.SRV_DIMENSION_TEXTURE2D)
                {
                    var tex2d = new ITexture2D(desc.mGpuBuffer);
                    result.PicDesc = new CShaderResourceView.UPicDesc();
                    result.PicDesc.Desc.Width = (int)tex2d.mTextureDesc.Width;
                    result.PicDesc.Desc.Height = (int)tex2d.mTextureDesc.Height;
                    result.PicDesc.Desc.MipLevel = (int)tex2d.mTextureDesc.MipLevels;
                }
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CGpuBuffer CreateGpuBuffer(in IGpuBufferDesc desc, IntPtr pInitData)
        {
            unsafe
            {
                var result = new CGpuBuffer();
                result.mCoreObject = mCoreObject.CreateGpuBuffer(in desc, pInitData.ToPointer());
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CUnorderedAccessView CreateUnorderedAccessView(IGpuBuffer pBuffer, in IUnorderedAccessViewDesc desc)
        {
            unsafe
            {
                var result = new CUnorderedAccessView();
                result.mCoreObject = mCoreObject.CreateUnorderedAccessView(pBuffer, in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CUnorderedAccessView CreateUnorderedAccessView(CGpuBuffer pBuffer, in IUnorderedAccessViewDesc desc)
        {
            unsafe
            {
                var result = new CUnorderedAccessView();
                result.mCoreObject = mCoreObject.CreateUnorderedAccessView(pBuffer.mCoreObject, in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderResourceView LoadShaderResourceView(string file)
        {
            unsafe
            {
                var result = new CShaderResourceView();
                result.mCoreObject = mCoreObject.LoadShaderResourceView(file);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CSamplerState CreateSamplerState(in ISamplerStateDesc desc)
        {
            unsafe
            {
                var result = new CSamplerState();
                result.mCoreObject = mCoreObject.CreateSamplerState(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CRasterizerState CreateRasterizerState(in IRasterizerStateDesc desc)
        {
            unsafe
            {
                var result = new CRasterizerState();
                result.mCoreObject = mCoreObject.CreateRasterizerState(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CDepthStencilState CreateDepthStencilState(in IDepthStencilStateDesc desc)
        {
            unsafe
            {
                var result = new CDepthStencilState();
                result.mCoreObject = mCoreObject.CreateDepthStencilState(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CBlendState CreateBlendState(in IBlendStateDesc desc)
        {
            unsafe
            {
                var result = new CBlendState();
                result.mCoreObject = mCoreObject.CreateBlendState(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderProgram CreateShaderProgram(ref IShaderProgramDesc desc)
        {
            unsafe
            {
                var result = new CShaderProgram();
                result.mCoreObject = mCoreObject.CreateShaderProgram(in desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CVertexShader CreateVertexShader(CShaderDesc desc)
        {
            unsafe
            {
                var result = new CVertexShader();
                result.mCoreObject = mCoreObject.CreateVertexShader(desc.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CPixelShader CreatePixelShader(CShaderDesc desc)
        {
            unsafe
            {
                var result = new CPixelShader();
                result.mCoreObject = mCoreObject.CreatePixelShader(desc.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CComputeShader CreateComputeShader(CShaderDesc desc)
        {
            unsafe
            {
                var result = new CComputeShader();
                result.mCoreObject = mCoreObject.CreateComputeShader(desc.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CConstantBuffer CreateConstantBuffer(CShaderProgram program, string name)
        {
            unsafe
            {
                var result = new CConstantBuffer();
                result.mCoreObject = mCoreObject.CreateConstantBuffer(program.mCoreObject, name);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CConstantBuffer CreateConstantBuffer(CShaderProgram program, UInt32 index)
        {
            unsafe
            {
                var result = new CConstantBuffer();
                result.mCoreObject = mCoreObject.CreateConstantBuffer(program.mCoreObject, index);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CConstantBuffer CreateConstantBuffer2(CShaderDesc program, UInt32 index)
        {
            unsafe
            {
                var result = new CConstantBuffer();
                result.mCoreObject = mCoreObject.CreateConstantBuffer2(program.mCoreObject, index);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CFence CreateFence()
        {
            unsafe
            {
                var result = new CFence();
                result.mCoreObject = mCoreObject.CreateFence();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CSemaphore CreateSemaphore()
        {
            unsafe
            {
                var result = new CSemaphore();
                result.mCoreObject = mCoreObject.CreateGpuSemaphore();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderDesc CreateShaderDesc(RName shader, string entry, EShaderType type, CShaderDefinitions defines, Editor.ShaderCompiler.UHLSLInclude incProvider)
        {
            string sm = "5_0";
            switch (type)
            {
                case EShaderType.EST_ComputeShader:
                    sm = "5_0";
                    if (ShaderModel == 3)
                    {
                        return null;
                    }
                    break;
                case EShaderType.EST_VertexShader:
                    sm = "5_0";
                    break;
                case EShaderType.EST_PixelShader:
                    sm = "5_0";
                    break;
            }
            var compilier = new Editor.ShaderCompiler.UHLSLCompiler();
            return compilier.CompileShader(shader.Address, entry, type, sm, null, null, null, defines, UEngine.Instance.Config.IsDebugShader, incProvider);
        }
    }
}
