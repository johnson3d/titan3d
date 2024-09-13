using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public unsafe partial struct FGpuDeviceDesc
    {
    }

    public class TtGpuSystem : AuxPtrType<NxRHI.IGpuSystem>
    {
        public static TtGpuSystem CreateGpuSystem(ERhiType type, in FGpuSystemDesc desc)
        {
            var result = new TtGpuSystem();
            result.mCoreObject = NxRHI.IGpuSystem.CreateGpuSystem(type, in desc);
            return result;
        }
        public TtGpuDevice CreateGpuDevice(in FGpuDeviceDesc desc)
        {
            var result = new TtGpuDevice();
            result.mCoreObject = mCoreObject.CreateDevice(in desc);
            result.mGpuQueue = new TtGpuQueue(result, result.mCoreObject.GetCmdQueue());
            result.InitShaderGlobalEnv();
            return result;
        }
        public int NumOfContext
        {
            get
            {
                return mCoreObject.GetNumOfGpuDevice();
            }
        }
        public void GetDeviceDesc(int index, ref EngineNS.NxRHI.FGpuDeviceDesc desc)
        {
            mCoreObject.GetDeviceDesc(index, ref desc);
        }
        public int GetAdapterScore(in EngineNS.NxRHI.FGpuDeviceDesc desc)
        {
            var memMB = (uint)desc.DedicatedVideoMemory / (1024 * 1024);
            if (desc.GetName().Contains("NVIDIA"))
            {
                memMB += 100;
            }
            return (int)memMB;
        }
    }
    public unsafe partial struct FGpuDeviceCaps
    {
        public override string ToString()
        {
            string result = "";
            if (IsSupportSSBO_VS)
                result += "HW_VS_STRUCTUREBUFFER=1\n";
            return result;
        }
    }
    public class TtGpuDevice : AuxPtrType<NxRHI.IGpuDevice>
    {
        public override void Dispose()
        {
            this.GpuQueue.Dispose();
            base.Dispose();
        }
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

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public UShaderDefinitions GlobalEnvDefines { get; } = new UShaderDefinitions();
        Hash160 mGlobalEnvHash;
在此之后:
        public TtShaderDefinitions GlobalEnvDefines { get; } = new UShaderDefinitions();
        Hash160 mGlobalEnvHash;
*/
        public TtShaderDefinitions GlobalEnvDefines { get; } = new TtShaderDefinitions();
        Hash160 mGlobalEnvHash;
        public Hash160 GlobalEnvHash
        {
            get => mGlobalEnvHash;
        }
        internal void InitShaderGlobalEnv()
        {
            GlobalEnvDefines.AddDefine("USE_INVERSE_Z", TtEngine.Instance.Config.IsReverseZ ? "1" : "0");
            
            var caps = DeviceCaps;
            if (caps.IsSupportSSBO_VS)
            {
                GlobalEnvDefines.AddDefine("HW_VS_STRUCTUREBUFFER", "1");
                if (TtEngine.Instance.Config.Feature_UseRVT)
                {
                    GlobalEnvDefines.AddDefine("FEATURE_USE_RVT", "1");
                }
            }
            if (RhiType == ERhiType.RHI_VK)
            {
                GlobalEnvDefines.AddDefine("USE_VS_DrawIndex", "1");
            }
            mGlobalEnvHash = Hash160.CreateHash160(GlobalEnvDefines.ToString());
        }
        public UCommandList CreateCommandList()
        {
            var result = new UCommandList();
            result.mCoreObject = mCoreObject.CreateCommandList();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtGpuScope CreateGpuScope()
        {
            var result = new TtGpuScope();
            result.mCoreObject = mCoreObject.CreateGpuScope();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtBuffer CreateBuffer(in FBufferDesc desc)
        {
            var result = new TtBuffer();
            result.mCoreObject = mCoreObject.CreateBuffer(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtCbView CreateCBV(TtEffectBinder binder)
        {
            if (binder == null)
                return null;
            return CreateCBV(binder.mCoreObject.GetShaderBinder(EShaderType.SDT_Unknown));
        }
        public TtCbView CreateCBV(FEffectBinder binder)
        {
            return CreateCBV(binder.GetShaderBinder(EShaderType.SDT_Unknown));
        }
        public TtCbView CreateCBV(FShaderBinder binder)
        {
            return CreateCBV(null, binder);
        }
        public TtCbView CreateCBV(TtBuffer buffer, FShaderBinder binder)
        {
            var cbvDesc = new FCbvDesc();
            cbvDesc.ShaderBinder = binder;
            var result = new TtCbView();
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
        public TtVbView CreateVBV(TtBuffer buffer, in FVbvDesc desc)
        {
            var result = new TtVbView();
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
        public TtIbView CreateIBV(TtBuffer buffer, in FIbvDesc desc)
        {
            var result = new TtIbView();
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
        public TtTexture CreateTexture(in FTextureDesc desc)
        {
            var ptr = mCoreObject.CreateTexture(in desc);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtTexture(ptr);
            ptr.NativeSuper.NativeSuper.Release();
            return result;
        }
        public TtSrView CreateSRV(TtBuffer buffer, in FSrvDesc desc)
        {
            if (buffer == null)
                return null;
            if (desc.Type == ESrvType.ST_BufferSRV)
            {
                var result = new TtSrView();
                result.mCoreObject = mCoreObject.CreateSRV(buffer.mCoreObject.NativeSuper, in desc);
                if (result.mCoreObject.IsValidPointer == false)
                    return null;
                return result;
            }
            return null;
        }
        public TtSrView CreateSRV(TtTexture texture, in FSrvDesc desc)
        {
            if (texture == null)
                return null;
            if (desc.Type == ESrvType.ST_BufferSRV)
                return null;
            var result = new TtSrView();
            result.mCoreObject = mCoreObject.CreateSRV(texture.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtUaView CreateUAV(TtBuffer buffer, in FUavDesc desc)
        {
            var result = new TtUaView();
            result.mCoreObject = mCoreObject.CreateUAV(buffer.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtUaView CreateUAV(TtTexture texture, in FUavDesc desc)
        {
            var result = new TtUaView();
            result.mCoreObject = mCoreObject.CreateUAV(texture.mCoreObject.NativeSuper, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtRenderTargetView CreateRTV(TtTexture buffer, in FRtvDesc desc)
        {
            var result = new TtRenderTargetView();
            result.mCoreObject = mCoreObject.CreateRTV(buffer.mCoreObject, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtDepthStencilView CreateDSV(TtTexture buffer, in FDsvDesc desc)
        {
            var result = new TtDepthStencilView();
            result.mCoreObject = mCoreObject.CreateDSV(buffer.mCoreObject, in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtSampler CreateSampler(in FSamplerDesc desc)
        {
            var result = new TtSampler();
            result.mCoreObject = mCoreObject.CreateSampler(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtSwapChain CreateSwapChain(in FSwapChainDesc desc)
        {
            var result = new TtSwapChain();
            result.mCoreObject = mCoreObject.CreateSwapChain(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.InitRenderPass();
            return result;
        }
        public TtRenderPass CreateRenderPass(in FRenderPassDesc desc)
        {
            var result = new TtRenderPass();
            result.mCoreObject = mCoreObject.CreateRenderPass(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtFrameBuffers CreateFrameBuffers(TtRenderPass rpass)
        {
            var result = new TtFrameBuffers();
            result.mCoreObject = mCoreObject.CreateFrameBuffers(rpass.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtShader CreateShader(TtShaderDesc desc)
        {
            var result = new TtShader();
            result.PermutationId = desc.PermutationId;
            result.mCoreObject = mCoreObject.CreateShader(desc.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtShaderEffect CreateShaderEffect(TtShader vs, TtShader ps)
        {
            var result = new TtShaderEffect();
            result.mCoreObject = mCoreObject.CreateShaderEffect();
            result.mCoreObject.BindVS(vs.mCoreObject);
            result.mCoreObject.BindPS(ps.mCoreObject);
            result.mCoreObject.LinkShaders();
            result.mCoreObject.BuildState(mCoreObject);
            return result;
        }
        public TtComputeEffect CreateComputeEffect(TtShader cs)
        {
            var result = new TtComputeEffect();
            result.mComputeShader = cs;
            result.mCoreObject = mCoreObject.CreateComputeEffect();
            result.mCoreObject.BindCS(cs.mCoreObject);
            result.mCoreObject.BuildState(mCoreObject);
            return result;
        }
        public TtGpuPipeline CreatePipeline(in FGpuPipelineDesc desc)
        {
            var result = new TtGpuPipeline();
            var cp = desc;
            if (TtEngine.Instance.Config.IsReverseZ)
                cp.m_DepthStencil.m_DepthFunc = EComparisionMode.CMP_GREATER_EQUAL;
            else
                cp.m_DepthStencil.m_DepthFunc = EComparisionMode.CMP_LESS_EQUAL;
            result.mCoreObject = mCoreObject.CreatePipeline(in cp);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtInputLayout CreateInputLayout(TtInputLayoutDesc desc)
        {
            var result = new TtInputLayout();
            result.mCoreObject = mCoreObject.CreateInputLayout(desc.mCoreObject);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtGraphicDraw CreateGraphicDraw()
        {
            var result = new TtGraphicDraw();
            result.mCoreObject = mCoreObject.CreateGraphicDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.GraphicsDrawcall++;
            return result;
        }
        public TtComputeDraw CreateComputeDraw()
        {
            var result = new TtComputeDraw();
            result.mCoreObject = mCoreObject.CreateComputeDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.ComputeDrawcall++;
            return result;
        }
        public TtCopyDraw CreateCopyDraw()
        {
            var result = new TtCopyDraw();
            result.mCoreObject = mCoreObject.CreateCopyDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.TransferDrawcall++;
            return result;
        }
        public TtActionDraw CreateActionDraw()
        {
            var result = new TtActionDraw();
            result.mCoreObject = mCoreObject.CreateActionDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.ActionDrawcall++;
            return result;
        }
        public TtEvent CreateGpuEvent(in FEventDesc desc, string name)
        {
            var result = new TtEvent();
            result.mCoreObject = mCoreObject.CreateGpuEvent(in desc, name);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtVertexArray CreateVertexArray()
        {
            var ptr = mCoreObject.CreateVertexArray();
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtVertexArray(ptr);
            return result;
        }
        public TtGeomMesh CreateGeomMesh()
        {
            var ptr = mCoreObject.CreateGeomMesh();
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtGeomMesh(ptr);
            return result;
        }
        public TtFence CreateFence(in FFenceDesc desc, string name)
        {
            var result = new TtFence();
            result.mCoreObject = mCoreObject.CreateFence(in desc, name);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public void SetBreakOnId(int id, bool open)
        {
            mCoreObject.SetBreakOnID(id, open);
        }

        internal TtGpuQueue mGpuQueue;
        public TtGpuQueue GpuQueue { get => mGpuQueue; }

        public void TickPostEvents()
        {
            mCoreObject.TickPostEvents();
        }
    }
    public class TtGpuQueue : AuxPtrType<NxRHI.ICmdQueue>
    {
        public override void Dispose()
        {
            //FramePostCmdList = null;
            base.Dispose();
        }
        //public UCommandList FramePostCmdList { get; set; } = null;
        public TtGpuQueue(TtGpuDevice device, ICmdQueue ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
            //FramePostCmdList = device.CreateCommandList();
        }
        public void Flush(EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            mCoreObject.Flush(type);
        }
        public void ExecuteCommandList(UCommandList Cmdlist, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            ExecuteCommandList(Cmdlist.mCoreObject, type);
        }
        public void ExecuteCommandList(ICommandList Cmdlist, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            if (Cmdlist.IsValidPointer == false)
                return;
            mCoreObject.ExecuteCommandListSingle(Cmdlist, type);
        }
        public ulong IncreaseSignal(TtFence fence, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            return mCoreObject.IncreaseSignal(fence.mCoreObject, type);
        }

        public ICommandList GetIdleCmdlist()
        {
            return mCoreObject.GetIdleCmdlist();
        }
		public void ReleaseIdleCmdlist(ICommandList cmd)
        {
            mCoreObject.ReleaseIdleCmdlist(cmd);
        }

    }

    public struct FTransientCmd : IDisposable
    {
        ICommandList mCmdList;
        EQueueType mType;
        public ICommandList CmdList { get => mCmdList; }
        public FTransientCmd(EQueueType type, string debugName)
        {
            mType = type;
            mCmdList = TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.GetIdleCmdlist();
            mCmdList.BeginCommand();
        }
        public void Dispose()
        {
            mCmdList.FlushDraws();
            mCmdList.EndCommand();
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, mType);
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ReleaseIdleCmdlist(mCmdList);
        }
    }
}
