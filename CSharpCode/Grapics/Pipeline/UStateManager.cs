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
        public T GetPipelineState<D>(RHI.CRenderContext rc, in D desc) where D : unmanaged
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
        protected unsafe virtual T CreateState(RHI.CRenderContext rc, void* desc)
        {
            return null;
        }
        protected virtual void ReleaseState(T state)
        {

        }
    }
    public class UBlendStateManager : UStateManager<RHI.CBlendState>
    {
        RHI.CBlendState mDefaultState;
        public RHI.CBlendState DefaultState 
        { 
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new IBlendStateDesc();
                    desc.SetDefault();
                    mDefaultState = UEngine.Instance.GfxDevice.BlendStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        protected unsafe override RHI.CBlendState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateBlendState(in *(IBlendStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CBlendState state)
        {
            state.Dispose();
        }
    }
    public class URasterizerStateManager : UStateManager<RHI.CRasterizerState>
    {
        RHI.CRasterizerState mDefaultState;
        public RHI.CRasterizerState DefaultState
        {
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new IRasterizerStateDesc();
                    desc.SetDefault();
                    mDefaultState = UEngine.Instance.GfxDevice.RasterizerStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        protected unsafe override RHI.CRasterizerState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateRasterizerState(in *(IRasterizerStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CRasterizerState state)
        {
            state.Dispose();
        }
    }
    public class UDepthStencilStateManager : UStateManager<RHI.CDepthStencilState>
    {
        RHI.CDepthStencilState mDefaultState;
        public RHI.CDepthStencilState DefaultState
        {
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new IDepthStencilStateDesc();
                    desc.SetDefault();
                    mDefaultState = UEngine.Instance.GfxDevice.DepthStencilStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        protected unsafe override RHI.CDepthStencilState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateDepthStencilState(in *(IDepthStencilStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CDepthStencilState state)
        {
            state.Dispose();
        }
    }
    public class USamplerStateManager : UStateManager<RHI.CSamplerState>
    {
        RHI.CSamplerState mDefaultState;
        public RHI.CSamplerState DefaultState
        {
            get
            {
                if (mDefaultState == null)
                {
                    var desc = new ISamplerStateDesc();
                    desc.SetDefault();
                    desc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    desc.CmpMode = EComparisionMode.CMP_NEVER;
                    desc.AddressU = EAddressMode.ADM_WRAP;
                    desc.AddressV = EAddressMode.ADM_WRAP;
                    desc.AddressW = EAddressMode.ADM_WRAP;
                    desc.MaxAnisotropy = 0;
                    desc.MipLODBias = 0;
                    desc.MinLOD = 0;
                    desc.MaxLOD = 3.402823466e+38f;
                    mDefaultState = UEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(
                        UEngine.Instance.GfxDevice.RenderContext, in desc);
                }
                return mDefaultState;
            }
        }
        protected unsafe override RHI.CSamplerState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateSamplerState(in *(ISamplerStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CSamplerState state)
        {
            state.Dispose();
        }
    }
    public class URenderPassManager : UStateManager<RHI.CRenderPass>
    {
        protected unsafe override RHI.CRenderPass CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateRenderPass(in *(IRenderPassDesc*)desc);
        }
        protected override void ReleaseState(RHI.CRenderPass state)
        {
            state.Dispose();
        }
    }

    public class UInputLayoutManager
    {
        public Dictionary<UInt64, RHI.CInputLayout> States
        {
            get;
        } = new Dictionary<UInt64, RHI.CInputLayout>();
        public RHI.CInputLayout GetPipelineState(RHI.CRenderContext rc, IInputLayoutDesc desc)
        {
            unsafe
            {
                var key = desc.GetLayoutHash64();
                {

                    RHI.CInputLayout state;
                    if (States.TryGetValue(key, out state) == false)
                    {
                        state = rc.CreateInputLayout(desc);
                        States.Add(key, state);
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
