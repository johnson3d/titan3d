using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class UGpuSystem : AuxPtrType<NxRHI.IGpuSystem>
    {
        public static UGpuSystem CreateGpuSystem(ERhiType type, in FGpuSystemDesc desc)
        {
            var result = new UGpuSystem();
            result.mCoreObject = NxRHI.IGpuSystem.CreateGpuSystem(type, in desc);
            return result;
        }
        public UGpuDevice CreateGpuDevice(in FGpuDeviceDesc desc)
        {
            var result = new UGpuDevice();
            result.mCoreObject = mCoreObject.CreateDevice(in desc);
            result.mCmdQueue = new UCmdQueue(result.mCoreObject.GetCmdQueue());
            return result;
        }
        public int NumOfContext
        {
            get
            {
                return mCoreObject.GetNumOfGpuDevice();
            }
        }
    }
    public unsafe partial struct FGpuDeviceCaps
    {
        public void SetShaderGlobalEnv(IShaderDefinitions defPtr)
        {
            if (IsSupportSSBO_VS)
            {
                defPtr.AddDefine("HW_VS_STRUCTUREBUFFER", "1");
            }
        }
        public override string ToString()
        {
            string result = "";
            if (IsSupportSSBO_VS)
                result += "HW_VS_STRUCTUREBUFFER=1\n";
            return result;
        }
    }
    public class UGpuDevice : AuxPtrType<NxRHI.IGpuDevice>
    {
        public ERhiType RhiType
        {
            get
            {
                return mCoreObject.Desc.RhiType;
            }
        }
        public FGpuDeviceCaps DeviceCaps
        {
            get
            {
                return mCoreObject.mCaps;
            }
        }
        public UCommandList CreateCommandList()
        {
            var result = new UCommandList();
            result.mCoreObject = mCoreObject.CreateCommandList();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UBuffer CreateBuffer(in FBufferDesc desc)
        {
            var result = new UBuffer();
            result.mCoreObject = mCoreObject.CreateBuffer(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UCbView CreateCBV(UEffectBinder binder)
        {
            if (binder == null)
                return null;
            return CreateCBV(binder.mCoreObject.GetShaderBinder(EShaderType.SDT_Unknown));
        }
        public UCbView CreateCBV(FShaderBinder binder)
        {
            return CreateCBV(null, binder);
        }
        public UCbView CreateCBV(UBuffer buffer, FShaderBinder binder)
        {
            var cbvDesc = new FCbvDesc();
            cbvDesc.ShaderBinder = binder;
            var result = new UCbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateCBV(new IBuffer(), in cbvDesc);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateCBV(buffer.mCoreObject, in cbvDesc);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UVbView CreateVBV(UBuffer buffer, in FVbvDesc desc)
        {
            var result = new UVbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateVBV(new IBuffer(), in desc);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateVBV(buffer.mCoreObject, in desc);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UIbView CreateIBV(UBuffer buffer, in FIbvDesc desc)
        {
            var result = new UIbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateIBV(new IBuffer(), in desc);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateIBV(buffer.mCoreObject, in desc);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public unsafe UGpuResource CreateTextureToCpuBuffer(in FTextureDesc texDesc, in FSubResourceFootPrint desc)
        {
            switch (mCoreObject.Desc.RhiType)
            {
                case ERhiType.RHI_D3D11:
                    {
                        return UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
                    }
                case ERhiType.RHI_D3D12:
                case ERhiType.RHI_VK:
                    {
                        var pAlignment = UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetGpuResourceAlignment();
                        var bfDesc = new FBufferDesc();
                        bfDesc.CpuAccess = ECpuAccess.CAS_READ;
                        bfDesc.Usage = EGpuUsage.USAGE_STAGING;
                        bfDesc.RowPitch = desc.RowPitch;

                        //bfDesc.RowPitch = desc.Width * (uint)CoreSDK.GetPixelFormatByteWidth(desc.Format);
                        //if (bfDesc.RowPitch % pAlignment->TexturePitchAlignment > 0)
                        //{
                        //    bfDesc.RowPitch = (bfDesc.RowPitch / pAlignment->TexturePitchAlignment) * pAlignment->TexturePitchAlignment + pAlignment->TexturePitchAlignment;
                        //}
                        bfDesc.DepthPitch = bfDesc.RowPitch * texDesc.Height;
                        if (bfDesc.DepthPitch % pAlignment->TextureAlignment > 0)
                        {
                            bfDesc.DepthPitch = (bfDesc.DepthPitch / pAlignment->TextureAlignment) * pAlignment->TextureAlignment + pAlignment->TextureAlignment;
                        }
                        if (desc.Depth == 0)
                            bfDesc.Size = bfDesc.DepthPitch;
                        else
                            bfDesc.Size = bfDesc.DepthPitch * desc.Depth;

                        bfDesc.StructureStride = (uint)CoreSDK.GetPixelFormatByteWidth(desc.Format);
                        bfDesc.Type = (EBufferType)0;
                        return UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                    }
                default:
                    return null;
            }
        }
        public UTexture CreateTexture(in FTextureDesc desc)
        {
            var ptr = mCoreObject.CreateTexture(in desc);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new UTexture(ptr);
            ptr.NativeSuper.NativeSuper.Release();
            return result;
        }
        public USrView CreateSRV(UBuffer buffer, in FSrvDesc desc)
        {
            if (desc.Type == ESrvType.ST_BufferSRV)
            {
                var result = new USrView();
                result.mCoreObject = mCoreObject.CreateSRV(buffer.mCoreObject.NativeSuper, in desc);
                if (result.mCoreObject.IsValidPointer == false)
                    return null;
                return result;
            }
            return null;
        }
        public USrView CreateSRV(UTexture texture, in FSrvDesc desc)
        {
            if (desc.Type == ESrvType.ST_BufferSRV)
                return null;
            var result = new USrView();
            result.mCoreObject = mCoreObject.CreateSRV(texture.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UUaView CreateUAV(UBuffer buffer, in FUavDesc desc)
        {
            var result = new UUaView();
            result.mCoreObject = mCoreObject.CreateUAV(buffer.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UUaView CreateUAV(UTexture texture, in FUavDesc desc)
        {
            var result = new UUaView();
            result.mCoreObject = mCoreObject.CreateUAV(texture.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public URenderTargetView CreateRTV(UTexture buffer, in FRtvDesc desc)
        {
            var result = new URenderTargetView();
            result.mCoreObject = mCoreObject.CreateRTV(buffer.mCoreObject, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UDepthStencilView CreateDSV(UTexture buffer, in FDsvDesc desc)
        {
            var result = new UDepthStencilView();
            result.mCoreObject = mCoreObject.CreateDSV(buffer.mCoreObject, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public USampler CreateSampler(in FSamplerDesc desc)
        {
            var result = new USampler();
            result.mCoreObject = mCoreObject.CreateSampler(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public USwapChain CreateSwapChain(in FSwapChainDesc desc)
        {
            var result = new USwapChain();
            result.mCoreObject = mCoreObject.CreateSwapChain(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.InitRenderPass();
            return result;
        }
        public URenderPass CreateRenderPass(in FRenderPassDesc desc)
        {
            var result = new URenderPass();
            result.mCoreObject = mCoreObject.CreateRenderPass(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UFrameBuffers CreateFrameBuffers(URenderPass rpass)
        {
            var result = new UFrameBuffers();
            result.mCoreObject = mCoreObject.CreateFrameBuffers(rpass.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UShader CreateShader(UShaderDesc desc)
        {
            var result = new UShader();
            result.mCoreObject = mCoreObject.CreateShader(desc.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UShaderEffect CreateShaderEffect(UShader vs, UShader ps)
        {
            var result = new UShaderEffect();
            result.mCoreObject = mCoreObject.CreateShaderEffect();
            result.mCoreObject.BindVS(vs.mCoreObject);
            result.mCoreObject.BindPS(ps.mCoreObject);
            result.mCoreObject.LinkShaders();
            result.mCoreObject.BuildState(mCoreObject);
            return result;
        }
        public UComputeEffect CreateComputeEffect(UShader cs)
        {
            var result = new UComputeEffect();
            result.mComputeShader = cs;
            result.mCoreObject = mCoreObject.CreateComputeEffect();
            result.mCoreObject.BindCS(cs.mCoreObject);
            result.mCoreObject.BuildState(mCoreObject);
            return result;
        }
        public UGpuPipeline CreatePipeline(in FGpuPipelineDesc desc)
        {
            var result = new UGpuPipeline();
            result.mCoreObject = mCoreObject.CreatePipeline(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UInputLayout CreateInputLayout(UInputLayoutDesc desc)
        {
            var result = new UInputLayout();
            result.mCoreObject = mCoreObject.CreateInputLayout(desc.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UGraphicDraw CreateGraphicDraw()
        {
            var result = new UGraphicDraw();
            result.mCoreObject = mCoreObject.CreateGraphicDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UComputeDraw CreateComputeDraw()
        {
            var result = new UComputeDraw();
            result.mCoreObject = mCoreObject.CreateComputeDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UCopyDraw CreateCopyDraw()
        {
            var result = new UCopyDraw();
            result.mCoreObject = mCoreObject.CreateCopyDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UEvent CreateGpuEvent(in FEventDesc desc, string name)
        {
            var result = new UEvent();
            result.mCoreObject = mCoreObject.CreateGpuEvent(in desc, name);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UFence CreateFence(in FFenceDesc desc, string name)
        {
            var result = new UFence();
            result.mCoreObject = mCoreObject.CreateFence(in desc, name);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }

        internal UCmdQueue mCmdQueue;
        public UCmdQueue CmdQueue { get => mCmdQueue; }

        public void TickPostEvents()
        {
            mCoreObject.TickPostEvents();
        }
    }
    public class UCmdQueue : AuxPtrType<NxRHI.ICmdQueue>
    {
        public UCmdQueue(ICmdQueue ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public void Flush()
        {
            mCoreObject.Flush();
        }
        public void ExecuteCommandList(UCommandList Cmdlist)
        {
            mCoreObject.ExecuteCommandList(Cmdlist.mCoreObject);
        }
        public void ExecuteCommandList(ICommandList Cmdlist)
        {
            mCoreObject.ExecuteCommandList(Cmdlist);
        }
        public ulong SignalFence(UFence fence, ulong value = ulong.MaxValue)
        {
            return mCoreObject.SignalFence(fence.mCoreObject, value);
        }
		public ICommandList GetIdleCmdlist(EQueueCmdlist type)
        {
            return mCoreObject.GetIdleCmdlist(type);
        }
		public void ReleaseIdleCmdlist(ICommandList cmd, EQueueCmdlist type)
        {
            mCoreObject.ReleaseIdleCmdlist(cmd, type);
        }
    }
}
