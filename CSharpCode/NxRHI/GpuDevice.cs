using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
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
            var ptr = result.mCoreObject.GetDescriptorPoolManager();
            if (ptr.IsValidPointer)
            {
                result.mDescriptorPoolManager = new TtDescriptorPoolManager(ptr);
            }
            else
            {
                result.mDescriptorPoolManager = null;
            }
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
    public partial class TtGpuDevice : AuxPtrType<NxRHI.IGpuDevice>
    {
        internal TtDescriptorPoolManager mDescriptorPoolManager;
        public TtDescriptorPoolManager DescriptorPoolManager
        {
            get => mDescriptorPoolManager;
        }
        public override void Dispose()
        {
            if (GlobalEnvDefines!=null)
            {
                GlobalEnvDefines.Dispose();
                GlobalEnvDefines = null;
            }
            this.CmdListManager.Dispose();
            this.GpuQueue.Dispose();
            this.mDescriptorPoolManager?.Dispose();
            base.Dispose();
        }
        public unsafe NxRHI.DX12GpuDevice AsDX12Deivce
        {
            get
            {
                if (RhiType == ERhiType.RHI_D3D12)
                    return new NxRHI.DX12GpuDevice(mCoreObject.CppPointer);
                return new NxRHI.DX12GpuDevice();
            }
        }
        public ERhiType RhiType
        {
            get
            {
                return mCoreObject.Desc.RhiType;
            }
        }
        FGpuDeviceCaps? mCaps = null;
        public FGpuDeviceCaps DeviceCaps
        {
            get
            {
                if (mCaps == null)
                {
                    mCaps = mCoreObject.mCaps;
                }
                return mCaps.Value;
            }
        }
        public TtShaderDefinitions GlobalEnvDefines { get; private set; } = new TtShaderDefinitions();
        Hash160 mGlobalEnvHash;
        public Hash160 GlobalEnvHash
        {
            get => mGlobalEnvHash;
        }
        internal void InitShaderGlobalEnv(TtEngine engine)
        {
            var graphicsConfig = engine.ConfigManager.GetConfig<Graphics.Pipeline.TtGfxDeviceConfig>();
            GlobalEnvDefines.AddDefine("USE_INVERSE_Z", graphicsConfig.IsReverseZ ? "1" : "0");
            
            var caps = DeviceCaps;
            if (caps.IsSupportSSBO_VS)
            {
                GlobalEnvDefines.AddDefine("HW_VS_STRUCTUREBUFFER", "1");
                if (engine.Config.Feature_UseRVT)
                {
                    GlobalEnvDefines.AddDefine("FEATURE_USE_RVT", "1");
                }
            }
            if (RhiType == ERhiType.RHI_VK)
            {
                GlobalEnvDefines.AddDefine("USE_VS_DrawIndex", "1");
            }
            if (graphicsConfig.UseOctahedronNormal)
            {
                GlobalEnvDefines.AddDefine("USE_OCTAHEDRON_NORMAL", "1");
            }
            else
            {
                GlobalEnvDefines.AddDefine("USE_OCTAHEDRON_NORMAL", "0");
            }
            mGlobalEnvHash = Hash160.CreateHash160(GlobalEnvDefines.ToString());
        }
        public void BeginFrame()
        {
            mCoreObject.BeginFrame();
            DescriptorPoolManager?.mCoreObject.AllocFramePool();
        }
        public void EndFrame()
        {
            mCoreObject.EndFrame();
            if (DescriptorPoolManager!=null)
            {
                var save = DescriptorPoolManager.mCoreObject.GetCurrentFramePool();
                CoreSDK.PtrType_Add(save);
                DescriptorPoolManager.mCoreObject.NullCurrentFramePool();
                TtEngine.Instance.GfxDevice?.RenderQueue?.QueueCmd((TtRCmdQueue queue, ref NxRHI.FRCmdInfo info) =>
                {
                    DescriptorPoolManager.mCoreObject.FreePool(this.GpuQueue.mCoreObject, save);
                    CoreSDK.PtrType_Release(save);
                }, "#EndFrame#", save);
            }
        }
        public TtCommandList CreateCommandList([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtCommandList();
            result.mCoreObject = mCoreObject.CreateCommandList(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtGpuScope CreateGpuScope([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtGpuScope();
            result.mCoreObject = mCoreObject.CreateGpuScope(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtBuffer CreateBuffer(in FBufferDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var result = new TtBuffer();
                result.mCoreObject = mCoreObject.CreateBuffer(in desc, filePath, lineNumber);
                if (result.mCoreObject.IsValidPointer == false)
                    return null;
                return result;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(ELogTag.Error, $"CreateBuffer failed, desc: {desc}, ex: {ex}");
                return null;
            }
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
        public TtCbView CreateCBV(TtShaderBinder binder)
        {
            if (binder == null)
                return null;
            return CreateCBV(binder.mCoreObject);
        }
        public TtCbView CreateCBV(FShaderBinder binder)
        {
            return CreateCBV(null, binder);
        }
        public TtCbView CreateCBV(TtBuffer buffer, FShaderBinder binder, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var cbvDesc = new FCbvDesc();
            cbvDesc.ShaderBinder = binder;
            var result = new TtCbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateCBV(new IBuffer(), in cbvDesc, filePath, lineNumber);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateCBV(buffer.mCoreObject, in cbvDesc, filePath, lineNumber);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtVbView CreateVBV(TtBuffer buffer, in FVbvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtVbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateVBV(new IBuffer(), in desc, filePath, lineNumber);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateVBV(buffer.mCoreObject, in desc, filePath, lineNumber);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtIbView CreateIBV(TtBuffer buffer, in FIbvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtIbView();
            if (buffer == null)
            {
                result.mCoreObject = mCoreObject.CreateIBV(new IBuffer(), in desc, filePath, lineNumber);
            }
            else
            {
                result.mCoreObject = mCoreObject.CreateIBV(buffer.mCoreObject, in desc, filePath, lineNumber);
            }
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtTexture CreateTexture(in FTextureDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateTexture(in desc, filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtTexture(ptr);
            ptr.NativeSuper.NativeSuper.Release();
            return result;
        }
        public TtSrView CreateSRV(NxRHI.IBuffer buffer, in FSrvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            if (desc.Type == ESrvType.ST_BufferSRV)
            {
                var result = new TtSrView();
                result.mCoreObject = mCoreObject.CreateSRV(buffer.NativeSuper, in desc, filePath, lineNumber);
                if (result.mCoreObject.IsValidPointer == false)
                    return null;
                return result;
            }
            return null;
        }
        public TtSrView CreateSRV(TtBuffer buffer, in FSrvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            if (buffer == null)
                return null;
            if (desc.Type == ESrvType.ST_BufferSRV)
            {
                var result = new TtSrView();
                result.mCoreObject = mCoreObject.CreateSRV(buffer.mCoreObject.NativeSuper, in desc, filePath, lineNumber);
                if (result.mCoreObject.IsValidPointer == false)
                    return null;
                return result;
            }
            return null;
        }
        public TtSrView CreateSRV(TtTexture texture, in FSrvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            if (texture == null)
                return null;
            if (desc.Type == ESrvType.ST_BufferSRV)
                return null;
            var result = new TtSrView();
            result.mCoreObject = mCoreObject.CreateSRV(texture.mCoreObject.NativeSuper, in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtUaView CreateUAV(TtBuffer buffer, in FUavDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtUaView();
            result.mCoreObject = mCoreObject.CreateUAV(buffer.mCoreObject.NativeSuper, in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtUaView CreateUAV(TtTexture texture, in FUavDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtUaView();
            result.mCoreObject = mCoreObject.CreateUAV(texture.mCoreObject.NativeSuper, in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtRenderTargetView CreateRTV(TtTexture buffer, in FRtvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtRenderTargetView();
            result.mCoreObject = mCoreObject.CreateRTV(buffer.mCoreObject, in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtDepthStencilView CreateDSV(TtTexture buffer, in FDsvDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtDepthStencilView();
            result.mCoreObject = mCoreObject.CreateDSV(buffer.mCoreObject, in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtSampler CreateSampler(in FSamplerDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtSampler();
            result.mCoreObject = mCoreObject.CreateSampler(in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtSwapChain CreateSwapChain(in FSwapChainDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtSwapChain();
            result.mCoreObject = mCoreObject.CreateSwapChain(in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.InitRenderPass();
            return result;
        }
        public TtRenderPass CreateRenderPass(in FRenderPassDesc desc, string identifier, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtRenderPass();
            result.mCoreObject = mCoreObject.CreateRenderPass(in desc, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.mCoreObject.SetIdentifier(identifier);
            return result;
        }
        public TtFrameBuffers CreateFrameBuffers(TtRenderPass rpass, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtFrameBuffers();
            result.mCoreObject = mCoreObject.CreateFrameBuffers(rpass.mCoreObject, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtAccelerationStructure CreateAccelerationStructure(in FAccelerationStructureDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateAccelerationStructure(in desc, filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtAccelerationStructure(ptr);
        }
        public TtAStructureInstance CreateAccelerationStructureInstance(in FAStructureInstanceDesc desc, TtAccelerationStructure pAStructrure, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateAccelerationStructureInstance(in desc, pAStructrure.mCoreObject, filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtAStructureInstance(ptr);
        }
        public TtTopAccelerationStructure CreateTopAccelerationStructure(in FTopAccelerationStructureDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateTopAccelerationStructure(in desc, filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            return new TtTopAccelerationStructure(ptr);
        }
        public TtShader CreateShader(TtShaderDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtShader();
            result.ShaderDesc = desc;
            result.PermutationId = desc.PermutationId;
            result.mCoreObject = mCoreObject.CreateShader(desc.mCoreObject, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtNativeGraphicsEffect CreateShaderEffect(TtShader ams, TtShader ms, TtShader vs, TtShader ps, string identifier, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtNativeGraphicsEffect();
            if(TtEngine.Instance.GfxDevice.RenderContext.DeviceCaps.IsSupportMeshShader == false &&
                ms!=null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(ELogTag.Error, $"Mesh Shader is not supported");
            }
            result.mCoreObject = mCoreObject.CreateShaderEffect(filePath, lineNumber);
            if (ams != null)
                result.mCoreObject.BindAS(ams.mCoreObject);
            if (ms != null)
                result.mCoreObject.BindMS(ms.mCoreObject);
            if (vs != null)
                result.mCoreObject.BindVS(vs.mCoreObject);            
            result.mCoreObject.BindPS(ps.mCoreObject);

            result.mCoreObject.LinkShaders();
            result.mCoreObject.BuildState(mCoreObject);
            result.mCoreObject.SetIdentifier(identifier);
            return result;
        }
        public TtComputeEffect CreateComputeEffect(TtShader cs, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtComputeEffect();
            result.mComputeShader = cs;
            result.mCoreObject = mCoreObject.CreateComputeEffect(filePath, lineNumber);
            result.mCoreObject.BindCS(cs.mCoreObject);
            result.mCoreObject.BuildState(mCoreObject);
            return result;
        }
        public unsafe TtRayTracingEffect CreateRayTracingEffect(TtShaderDesc shader, TtRayTracingEffect.TtRTShaderLibDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtRayTracingEffect();
            result.ShaderDesc = shader;
            result.ShaderLibDesc = desc;
            result.mCoreObject = mCoreObject.CreateRayTracingEffect(filePath, lineNumber);
            result.mCoreObject.SetShaderLibDesc(shader.mCoreObject);
            if (desc != null)
            {
                result.mCoreObject.mMaxRecursionDepth = desc.MaxRecursionDepth;
                result.mCoreObject.mPayloadSize = desc.PayloadSize;
                result.mCoreObject.mAttributeSize = desc.AttributeSize;
                foreach (var i in desc.Functions)
                {
                    result.mCoreObject.AddFunctions(VNameString.FromString(i));
                }
                result.mCoreObject.SetRayGenShader(VNameString.FromString(desc.RayGenShader));
                result.mCoreObject.SetMissShader(VNameString.FromString(desc.MissShader));

                foreach (var i in desc.GlobalSignatures)
                {
                    result.mCoreObject.AddGlobalSignature(VNameString.FromString(i));
                }
                VNameString* localSignatures = stackalloc VNameString[64];
                foreach (var i in desc.HitGroups)
                {   
                    for (int j = 0; j < i.LocalSignatures.Count; ++j)
                    {
                        localSignatures[j] = VNameString.FromString(i.LocalSignatures[j]);
                    }

                    result.mCoreObject.AddHitGroup(VNameString.FromString(i.Name),
                        VNameString.FromString(i.AnyHitShader),
                        VNameString.FromString(i.ClosestHitShader),
                        VNameString.FromString(i.IntersectionShader), localSignatures, i.LocalSignatures.Count);
                }
            }
            if (false == result.mCoreObject.BuildEffect(mCoreObject))
                return null;
            return result;
        }
        public TtGpuPipeline CreatePipeline(in FGpuPipelineDesc desc, string identifier, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtGpuPipeline();
            var cp = desc;
            if (TtEngine.Instance.GfxDevice.Config.IsReverseZ)
                cp.m_DepthStencil.m_DepthFunc = EComparisionMode.CMP_GREATER_EQUAL;
            else
                cp.m_DepthStencil.m_DepthFunc = EComparisionMode.CMP_LESS_EQUAL;
            result.mCoreObject = mCoreObject.CreatePipeline(in cp, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            result.mCoreObject.SetIdentifier(identifier);
            return result;
        }
        public TtInputLayout CreateInputLayout(TtInputLayoutDesc desc, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtInputLayout();
            result.mCoreObject = mCoreObject.CreateInputLayout(desc.mCoreObject, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtGraphicDraw CreateGraphicDraw([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtGraphicDraw();
            result.mCoreObject = mCoreObject.CreateGraphicDraw(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.GraphicsDrawcall++;
            return result;
        }
        public TtComputeDraw CreateComputeDraw([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtComputeDraw();
            result.mCoreObject = mCoreObject.CreateComputeDraw(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.ComputeDrawcall++;
            return result;
        }
        public TtRayTracingDraw CreateRayTracingDraw([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtRayTracingDraw();
            result.mCoreObject = mCoreObject.CreateRayTracingDraw(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.RayTracingDrawcall++;
            return result;
        }
        public TtCopyDraw CreateCopyDraw([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtCopyDraw();
            result.mCoreObject = mCoreObject.CreateCopyDraw(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.TransferDrawcall++;
            return result;
        }
        public TtActionDraw CreateActionDraw([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtActionDraw();
            result.mCoreObject = mCoreObject.CreateActionDraw(filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            TtStatistic.Instance.ActionDrawcall++;
            return result;
        }
        public TtEvent CreateGpuEvent(in FEventDesc desc, string name, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtEvent();
            result.mCoreObject = mCoreObject.CreateGpuEvent(in desc, name, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public TtVertexArray CreateVertexArray([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateVertexArray(filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtVertexArray(ptr);
            return result;
        }
        public TtGeomMesh CreateGeomMesh([CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var ptr = mCoreObject.CreateGeomMesh(filePath, lineNumber);
            if (ptr.IsValidPointer == false)
                return null;
            var result = new TtGeomMesh(ptr);
            return result;
        }
        public TtFence CreateFence(in FFenceDesc desc, string name, [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        {
            var result = new TtFence();
            result.mCoreObject = mCoreObject.CreateFence(in desc, name, filePath, lineNumber);
            if (result.mCoreObject.IsValidPointer == false)
                return null;
            return result;
        }
        public void SetDX12BreakOnId(NxRHI.EDx12MessageId id, bool open)
        {
            mCoreObject.SetBreakOnID((int)id, open);
        }
        public void ShowDX12DeviceMessage(NxRHI.EDx12MessageId id, bool show)
        {//id->D3D12_MESSAGE_ID 
            mCoreObject.ShowDeviceMessage((int)id, show);
        }

        internal TtGpuQueue mGpuQueue;
        public TtGpuQueue GpuQueue { get => mGpuQueue; }

        [ThreadStatic]
        private static Profiler.TimeScope mScopeTickPostEvents;
        private static Profiler.TimeScope ScopeTickPostEvents
        {
            get
            {
                if (mScopeTickPostEvents == null)
                    mScopeTickPostEvents = new Profiler.TimeScope(typeof(TtGpuDevice), nameof(TickPostEvents));
                return mScopeTickPostEvents;
            }
        }
        public void TickPostEvents()
        {
            using (new Profiler.TimeScopeHelper(ScopeTickPostEvents))
            {
                mCoreObject.TickPostEvents();
                CmdListManager.Tick();
            }   
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
            mCoreObject.NativeSuper.NativeSuper.AddRef();
            //FramePostCmdList = device.CreateCommandList();
        }
        public void Flush(EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            mCoreObject.Flush(type);
        }
        public void ExecuteCommandList(TtCommandList Cmdlist, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            ExecuteCommandList(Cmdlist.mCoreObject, type);
            Cmdlist.CommandListState = ECommandListState.Committed;
        }
        public void ExecuteCommandList(ICommandList Cmdlist, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            if (Cmdlist.IsValidPointer == false)
                return;
            mCoreObject.ExecuteCommandListSingle(Cmdlist, type);
        }
        public unsafe void ExecuteCommandLists(List<TtCommandList> list, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            EngineNS.NxRHI.ICommandList** nativeList = stackalloc EngineNS.NxRHI.ICommandList*[list.Count];
            mCoreObject.ExecuteCommandList((uint)list.Count, nativeList, 0, (EngineNS.NxRHI.ICommandList**)0, type);
        }
        public UInt64 ExecuteCommandList(ICommandList Cmdlist, NxRHI.TtFence fence, EngineNS.NxRHI.EQueueType type = EQueueType.QU_Default)
        {
            if (Cmdlist.IsValidPointer == false)
                return 0;
            Cmdlist.GetCommitFence();
            mCoreObject.ExecuteCommandListSingle(Cmdlist, type);
            fence.IncreaseExpect(mCoreObject, 1, type);
            return fence.ExpectValue;
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

        public void BeginEvent(string info, uint color = 0)
        {
            mCoreObject.BeginEvent(info, color);
        }
        public void EndEvent(string info)
        {
            mCoreObject.EndEvent(info);
        }
        public void WaitFence(TtFence fence, ulong value, EngineNS.NxRHI.EQueueType type)
        {
            mCoreObject.WaitFence(fence.mCoreObject, value, type);
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

    public class TtDescriptorPoolManager : AuxPtrType<NxRHI.NxDesriptorPoolManager>
    {
        public TtDescriptorPoolManager(NxRHI.NxDesriptorPoolManager self)
        {
            mCoreObject = self;
            mCoreObject.NativeSuper.AddRef();
        }
    }
}
