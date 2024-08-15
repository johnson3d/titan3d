using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UStateManager<T> where T : class
    {
        public Dictionary<Hash64, T> States
        {
            get;
        } = new Dictionary<Hash64, T>(new Hash64.EqualityComparer());
        public T GetPipelineState<D>(NxRHI.UGpuDevice rc, in D desc) where D : unmanaged
        {
            Hash64 hash = new Hash64();
            unsafe
            {
                fixed (D* p = &desc)
                {
                    Hash64.CalcHash64(&hash, (byte*)p, sizeof(D));

                    T state;
                    if (States.TryGetValue(hash, out state) == false)
                    {
                        state = CreateState(rc, p);
                        if (state == null)
                            return null;
                        States.Add(hash, state);
                    }
                    return state;
                }
            }
        }
        public void Cleanup()
        {
            foreach (var i in States.Values)
            {
                ReleaseState(i);
            }
            States.Clear();
        }
        protected unsafe virtual T CreateState(NxRHI.UGpuDevice rc, void* desc)
        {
            return null;
        }
        protected virtual void ReleaseState(T state)
        {

        }
    }
    public class UGpuPipelineManager : UStateManager<NxRHI.UGpuPipeline>
    {
        NxRHI.UGpuPipeline mDefaultState;
        public NxRHI.UGpuPipeline DefaultState
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
        protected unsafe override NxRHI.UGpuPipeline CreateState(NxRHI.UGpuDevice rc, void* desc)
        {
            return rc.CreatePipeline(in *(NxRHI.FGpuPipelineDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.UGpuPipeline state)
        {
            state?.Dispose();
        }
    }
    public class USamplerStateManager : UStateManager<NxRHI.USampler>
    {
        NxRHI.USampler mDefaultState;
        public NxRHI.USampler DefaultState
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
        NxRHI.USampler mLinearClampState;
        public NxRHI.USampler LinearClampState
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
        NxRHI.USampler mPointState;
        public NxRHI.USampler PointState
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
        protected unsafe override NxRHI.USampler CreateState(NxRHI.UGpuDevice rc, void* desc)
        {
            return rc.CreateSampler(in *(NxRHI.FSamplerDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.USampler state)
        {
            state.Dispose();
        }
    }
    public class URenderPassManager : UStateManager<NxRHI.URenderPass>
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
        protected unsafe override NxRHI.URenderPass CreateState(NxRHI.UGpuDevice rc, void* desc)
        {
            return rc.CreateRenderPass(in *(NxRHI.FRenderPassDesc*)desc);
        }
        protected override void ReleaseState(NxRHI.URenderPass state)
        {
            state.Dispose();
        }
    }

    public class UInputLayoutManager
    {
        public Dictionary<UInt64, NxRHI.UInputLayoutDesc> States
        {
            get;
        } = new Dictionary<UInt64, NxRHI.UInputLayoutDesc>();
        public NxRHI.UInputLayoutDesc GetPipelineState(NxRHI.UGpuDevice rc, NxRHI.UInputLayoutDesc desc)
        {
            unsafe
            {
                var key = desc.GetLayoutHash64();
                {
                    NxRHI.UInputLayoutDesc state;
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
