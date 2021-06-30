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
                result.mCoreObject = mCoreObject.CreateSwapChain(ref desc);
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
                result.mCoreObject = mCoreObject.CreateCommandList(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public async System.Threading.Tasks.Task<CDrawCall> CreateDrawCall(Graphics.Pipeline.Shader.UShadingEnv shading, Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            var result = new CDrawCall();
            unsafe
            {
                result.mCoreObject = mCoreObject.CreateDrawCall();
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
            }
            await result.UpdateShaderProgram(shading, material, mdf);
            return result;
        }
        public CRenderPipeline CreateRenderPipeline(ref IRenderPipelineDesc desc)
        {
            unsafe
            {
                var result = new CRenderPipeline();
                result.mCoreObject = mCoreObject.CreateRenderPipeline(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CVertexBuffer CreateVertexBuffer(ref IVertexBufferDesc desc)
        {
            unsafe
            {
                var result = new CVertexBuffer();
                result.mCoreObject = mCoreObject.CreateVertexBuffer(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CIndexBuffer CreateIndexBuffer(ref IIndexBufferDesc desc)
        {
            unsafe
            {
                var result = new CIndexBuffer();
                result.mCoreObject = mCoreObject.CreateIndexBuffer(ref desc);
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
        public CIndexBuffer CreateIndexBufferFromBuffer(ref IIndexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var result = new CIndexBuffer();
                result.mCoreObject = mCoreObject.CreateIndexBufferFromBuffer(ref desc, pBuffer.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CVertexBuffer CreateVertexBufferFromBuffer(ref IVertexBufferDesc desc, CGpuBuffer pBuffer)
        {
            unsafe
            {
                var result = new CVertexBuffer();
                result.mCoreObject = mCoreObject.CreateVertexBufferFromBuffer(ref desc, pBuffer.mCoreObject);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CFrameBuffers CreateFrameBuffers(ref IFrameBuffersDesc desc)
        {
            unsafe
            {
                var result = new CFrameBuffers();
                result.mCoreObject = mCoreObject.CreateFrameBuffers(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CRenderTargetView CreateRenderTargetView(ref IRenderTargetViewDesc desc)
        {
            unsafe
            {
                var result = new CRenderTargetView();
                result.mCoreObject = mCoreObject.CreateRenderTargetView(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CDepthStencilView CreateDepthRenderTargetView(ref IDepthStencilViewDesc desc)
        {
            unsafe
            {
                var result = new CDepthStencilView();
                result.mCoreObject = mCoreObject.CreateDepthRenderTargetView(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CTexture2D CreateTexture2D(ref ITexture2DDesc desc)
        {
            unsafe
            {
                var result = new CTexture2D();
                result.mCoreObject = mCoreObject.CreateTexture2D(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderResourceView CreateShaderResourceView(ref EngineNS.IShaderResourceViewDesc desc)
        {
            unsafe
            {
                var result = new CShaderResourceView();
                result.mCoreObject = mCoreObject.CreateShaderResourceView(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CGpuBuffer CreateGpuBuffer(ref IGpuBufferDesc desc, IntPtr pInitData)
        {
            unsafe
            {
                var result = new CGpuBuffer();
                result.mCoreObject = mCoreObject.CreateGpuBuffer(ref desc, pInitData.ToPointer());
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CShaderResourceView CreateShaderResourceViewFromBuffer(CGpuBuffer pBuffer, ref ISRVDesc desc)
        {
            unsafe
            {
                var result = new CShaderResourceView();
                result.mCoreObject = mCoreObject.CreateShaderResourceViewFromBuffer(pBuffer.mCoreObject, ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CUnorderedAccessView CreateUnorderedAccessView(CGpuBuffer pBuffer, ref IUnorderedAccessViewDesc desc)
        {
            unsafe
            {
                var result = new CUnorderedAccessView();
                result.mCoreObject = mCoreObject.CreateUnorderedAccessView(pBuffer.mCoreObject, ref desc);
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
        public CSamplerState CreateSamplerState(ref ISamplerStateDesc desc)
        {
            unsafe
            {
                var result = new CSamplerState();
                result.mCoreObject = mCoreObject.CreateSamplerState(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CRasterizerState CreateRasterizerState(ref IRasterizerStateDesc desc)
        {
            unsafe
            {
                var result = new CRasterizerState();
                result.mCoreObject = mCoreObject.CreateRasterizerState(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CDepthStencilState CreateDepthStencilState(ref IDepthStencilStateDesc desc)
        {
            unsafe
            {
                var result = new CDepthStencilState();
                result.mCoreObject = mCoreObject.CreateDepthStencilState(ref desc);
                if (result.mCoreObject.NativePointer == IntPtr.Zero)
                    return null;
                return result;
            }
        }
        public CBlendState CreateBlendState(ref IBlendStateDesc desc)
        {
            unsafe
            {
                var result = new CBlendState();
                result.mCoreObject = mCoreObject.CreateBlendState(ref desc);
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
                result.mCoreObject = mCoreObject.CreateShaderProgram(ref desc);
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
    }
}
