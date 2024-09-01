﻿using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public unsafe partial struct FGpuDeviceDesc
    {
    }

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
            result.mGpuQueue = new UGpuQueue(result, result.mCoreObject.GetCmdQueue());
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
    public class UGpuDevice : AuxPtrType<NxRHI.IGpuDevice>
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
        public UShaderDefinitions GlobalEnvDefines { get; } = new UShaderDefinitions();
        Hash160 mGlobalEnvHash;
        public Hash160 GlobalEnvHash
        {
            get => mGlobalEnvHash;
        }
        internal void InitShaderGlobalEnv()
        {
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
        public UBuffer CreateBuffer(in FBufferDesc desc)
        {
            var result = new UBuffer();
            result.mCoreObject = mCoreObject.CreateBuffer(in desc);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UCbView CreateCBV(TtEffectBinder binder)
        {
            if (binder == null)
                return null;
            return CreateCBV(binder.mCoreObject.GetShaderBinder(EShaderType.SDT_Unknown));
        }
        public UCbView CreateCBV(FEffectBinder binder)
        {
            return CreateCBV(binder.GetShaderBinder(EShaderType.SDT_Unknown));
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
        public UTexture CreateTexture(in FTextureDesc desc)
        {
            var ptr = mCoreObject.CreateTexture(in desc);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new UTexture(ptr);
            ptr.NativeSuper.NativeSuper.Release();
            return result;
        }
        public TtSrView CreateSRV(UBuffer buffer, in FSrvDesc desc)
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
        public TtSrView CreateSRV(UTexture texture, in FSrvDesc desc)
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
            TtStatistic.Instance.GraphicsDrawcall++;
            return result;
        }
        public UComputeDraw CreateComputeDraw()
        {
            var result = new UComputeDraw();
            result.mCoreObject = mCoreObject.CreateComputeDraw();
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.ComputeDrawcall++;
            return result;
        }
        public UCopyDraw CreateCopyDraw()
        {
            var result = new UCopyDraw();
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
        public UEvent CreateGpuEvent(in FEventDesc desc, string name)
        {
            var result = new UEvent();
            result.mCoreObject = mCoreObject.CreateGpuEvent(in desc, name);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public UVertexArray CreateVertexArray()
        {
            var ptr = mCoreObject.CreateVertexArray();
            if (ptr.IsValidPointer == false)
                return null;
            var result = new UVertexArray(ptr);
            return result;
        }
        public UGeomMesh CreateGeomMesh()
        {
            var ptr = mCoreObject.CreateGeomMesh();
            if (ptr.IsValidPointer == false)
                return null;
            var result = new UGeomMesh(ptr);
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
        public void SetBreakOnId(int id, bool open)
        {
            mCoreObject.SetBreakOnID(id, open);
        }

        internal UGpuQueue mGpuQueue;
        public UGpuQueue GpuQueue { get => mGpuQueue; }

        public void TickPostEvents()
        {
            mCoreObject.TickPostEvents();
        }
    }
    public class UGpuQueue : AuxPtrType<NxRHI.ICmdQueue>
    {
        public override void Dispose()
        {
            //FramePostCmdList = null;
            base.Dispose();
        }
        //public UCommandList FramePostCmdList { get; set; } = null;
        public UGpuQueue(UGpuDevice device, ICmdQueue ptr)
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
        public ulong IncreaseSignal(UFence fence, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
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
