using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public unsafe partial struct FSamplerDesc : IHash64
    { 
        public unsafe Hash64 GetHash64()
        {
            Hash64 result = new Hash64();
            fixed (FSamplerDesc* p = &this)
            {
                Hash64.CalcHash64(&result, (byte*)p, sizeof(FSamplerDesc));
            }
            return result;
        }
    }
    public unsafe partial struct FRenderPassDesc : IHash64
    {
        public unsafe Hash64 GetHash64()
        {
            Hash64 result = new Hash64();
            fixed (FRenderPassDesc* p = &this)
            {
                Hash64.CalcHash64(&result, (byte*)p, sizeof(FRenderPassDesc));
            }
            return result;
        }
    }
    public unsafe partial struct FGpuPipelineDesc : IHash64
    {
        public unsafe Hash64 GetHash64()
        {
            Hash64 result = new Hash64();
            fixed (FGpuPipelineDesc* p = &this)
            {
                Hash64.CalcHash64(&result, (byte*)p, sizeof(FGpuPipelineDesc));
            }
            return result;
        }
    }
    
}

namespace EngineNS.Graphics.Pipeline
{
    public class TtStateManager<T> where T : class
    {
        public Dictionary<Hash64, T> States
        {
            get;
        } = new Dictionary<Hash64, T>(new Hash64.EqualityComparer());
        public unsafe T GetPipelineState<D>(NxRHI.TtGpuDevice rc, in D desc) where D : unmanaged, IHash64
        {
            Hash64 hash = desc.GetHash64();
            T state;
            if (States.TryGetValue(hash, out state) == false)
            {
                var t = desc;
                state = CreateState(rc, &t);
                if (state == null)
                    return null;
                States.Add(hash, state);
            }
            return state;
        }
        public void Cleanup()
        {
            foreach (var i in States.Values)
            {
                ReleaseState(i);
            }
            States.Clear();
        }
        protected unsafe virtual T CreateState(NxRHI.TtGpuDevice rc, void* desc)
        {
            return null;
        }
        protected virtual void ReleaseState(T state)
        {

        }
    }
    public class TtGpuPipelineManager : TtStateManager<NxRHI.TtGpuPipeline>
    {
        NxRHI.TtGpuPipeline mDefaultState;
        public NxRHI.TtGpuPipeline DefaultState
        {
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new NxRHI.FGpuPipelineDesc();
                    desc.SetDefault();
                    mDefaultState = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        protected unsafe override NxRHI.TtGpuPipeline CreateState(NxRHI.TtGpuDevice rc, void* desc)
        {
            return rc.CreatePipeline(in *(NxRHI.FGpuPipelineDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.TtGpuPipeline state)
        {
            state?.Dispose();
        }
    }
    public class TtSamplerStateManager : TtStateManager<NxRHI.TtSampler>
    {
        NxRHI.TtSampler mDefaultState;
        public NxRHI.TtSampler DefaultState
        {
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new NxRHI.FSamplerDesc();
                    desc.SetDefault();
                    desc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    desc.CmpMode = NxRHI.EComparisionMode.CMP_NEVER;
                    desc.AddressU = NxRHI.EAddressMode.ADM_WRAP;
                    desc.AddressV = NxRHI.EAddressMode.ADM_WRAP;
                    desc.AddressW = NxRHI.EAddressMode.ADM_WRAP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mDefaultState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        NxRHI.TtSampler mLinearClampState;
        public NxRHI.TtSampler LinearClampState
        {
            get
            {
                if (mLinearClampState == null)
                {
                    var desc = new NxRHI.FSamplerDesc();
                    desc.SetDefault();
                    desc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    desc.CmpMode = NxRHI.EComparisionMode.CMP_NEVER;
                    desc.AddressU = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressV = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressW = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mLinearClampState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mLinearClampState;
            }
        }
        NxRHI.TtSampler mPointState;
        public NxRHI.TtSampler PointState
        {
            get
            {
                if (mPointState == null)
                {
                    var desc = new NxRHI.FSamplerDesc();
                    desc.SetDefault();
                    desc.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                    desc.CmpMode = NxRHI.EComparisionMode.CMP_NEVER;
                    desc.AddressU = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressV = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.AddressW = NxRHI.EAddressMode.ADM_CLAMP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mPointState = TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        TtEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mPointState;
            }
        }
        protected unsafe override NxRHI.TtSampler CreateState(NxRHI.TtGpuDevice rc, void* desc)
        {
            return rc.CreateSampler(in *(NxRHI.FSamplerDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.TtSampler state)
        {
            state.Dispose();
        }
    }
    public class TtRenderPassManager : TtStateManager<NxRHI.TtRenderPass>
    {
        //public NxRHI.URenderPass DefaultRenderPass { get; private set; }
        public void Initialize(TtEngine engine)
        {
            //var desc = new NxRHI.FRenderPassDesc();
            //desc.SetDefault();
            //unsafe
            //{
            //    desc.NumOfMRT = 1;
            //    desc.AttachmentMRTs[0].Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            //    desc.AttachmentMRTs[0].Samples = 1;
            //    desc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //    desc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //    desc.m_AttachmentDepthStencil.Format = EPixelFormat.PXF_D16_UNORM;
            //    desc.m_AttachmentDepthStencil.Samples = 1;
            //    desc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //    desc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //    desc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //    desc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //}
            //DefaultRenderPass = this.GetPipelineState(engine.GfxDevice.RenderContext, in desc);
        }
        protected unsafe override NxRHI.TtRenderPass CreateState(NxRHI.TtGpuDevice rc, void* desc)
        {
            return rc.CreateRenderPass(in *(NxRHI.FRenderPassDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.TtRenderPass state)
        {
            state.Dispose();
        }
    }

    public class TtInputLayoutManager
    {
        public Dictionary<UInt64, NxRHI.TtInputLayoutDesc> States
        {
            get;
        } = new Dictionary<UInt64, NxRHI.TtInputLayoutDesc>();
        public NxRHI.TtInputLayoutDesc GetPipelineState(NxRHI.TtGpuDevice rc, NxRHI.TtInputLayoutDesc desc)
        {
            unsafe
            {
                var key = desc.GetLayoutHash64();
                {
                    NxRHI.TtInputLayoutDesc state;
                    if (States.TryGetValue(key, out state) == false)
                    {
                        States.Add(key, desc);
                        return desc;
                    }
                    return state;
                }
            }
        }
        public void Cleanup()
        {
            foreach (var i in States.Values)
            {
                i.Dispose();
            }
            States.Clear();
        }
    }
}
