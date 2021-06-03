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
        public T GetPipelineState<D>(RHI.CRenderContext rc, ref D desc) where D : unmanaged
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
        protected unsafe override RHI.CBlendState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateBlendState(ref *(IBlendStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CBlendState state)
        {
            state.Dispose();
        }
    }
    public class URasterizerStateManager : UStateManager<RHI.CRasterizerState>
    {
        protected unsafe override RHI.CRasterizerState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateRasterizerState(ref *(IRasterizerStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CRasterizerState state)
        {
            state.Dispose();
        }
    }
    public class UDepthStencilStateManager : UStateManager<RHI.CDepthStencilState>
    {
        protected unsafe override RHI.CDepthStencilState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateDepthStencilState(ref *(IDepthStencilStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CDepthStencilState state)
        {
            state.Dispose();
        }
    }
    public class USamplerStateManager : UStateManager<RHI.CSamplerState>
    {
        protected unsafe override RHI.CSamplerState CreateState(RHI.CRenderContext rc, void* desc)
        {
            return rc.CreateSamplerState(ref *(ISamplerStateDesc*)desc);
        }
        protected override void ReleaseState(RHI.CSamplerState state)
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
