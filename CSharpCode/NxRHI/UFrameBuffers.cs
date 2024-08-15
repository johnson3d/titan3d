using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public class URenderPass : AuxPtrType<NxRHI.IRenderPass>
    {
    }
    public class UFrameBuffers : AuxPtrType<NxRHI.IFrameBuffers>
    {
        public void BindRenderTargetView(uint index, URenderTargetView rt)
        {
            if (rt == null)
            {
                mCoreObject.BindRenderTargetView(index, new IRenderTargetView());
                return;
            }
            mCoreObject.BindRenderTargetView(index, rt.mCoreObject);
        }
        public void BindDepthStencilView(UDepthStencilView ds)
        {
            if(ds == null)
            {
                mCoreObject.BindDepthStencilView(new IDepthStencilView());
                return;
            }
            mCoreObject.BindDepthStencilView(ds.mCoreObject);
        }
        public void FlushModify()
        {
            mCoreObject.FlushModify();
        }
        public IRenderTargetView GetRtv(uint index)
        {
            return mCoreObject.GetRtv(index);
        }
        public IDepthStencilView GetDsv()
        {
            return mCoreObject.GetDsv();
        }
    }
    public class USwapChain : AuxPtrType<NxRHI.ISwapChain>
    {
        public FViewPort Viewport;
        public unsafe void InitRenderPass()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var SwapChainPassDesc = new NxRHI.FRenderPassDesc();
            SwapChainPassDesc.NumOfMRT = 1;
            SwapChainPassDesc.AttachmentMRTs[0].IsSwapChain = 1;
            SwapChainPassDesc.AttachmentMRTs[0].Format = mCoreObject.Desc.Format;
            SwapChainPassDesc.AttachmentMRTs[0].Samples = 1;
            SwapChainPassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            SwapChainPassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            SwapChainPassDesc.m_AttachmentDepthStencil.Format = EPixelFormat.PXF_UNKNOWN;
            //no dsv for it

            //SwapChainPassDesc.m_AttachmentDepthStencil.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            //SwapChainPassDesc.m_AttachmentDepthStencil.Samples = 1;
            //SwapChainPassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //SwapChainPassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //SwapChainPassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //SwapChainPassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //SwapChainPassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
            //SwapChainPassDesc.mDepthClearValue = 1.0f;
            //SwapChainPassDesc.mStencilClearValue = 0u;

            Viewport.TopLeftX = 0;
            Viewport.TopLeftY = 0;
            Viewport.Width = mCoreObject.Desc.Width;
            Viewport.Height = mCoreObject.Desc.Height;
            Viewport.MinDepth = 0;
            Viewport.MaxDepth = 1.0f;

            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in SwapChainPassDesc);
        }
        public void OnResize(float x, float y)
        {
            Viewport.TopLeftX = 0;
            Viewport.TopLeftY = 0;
            Viewport.Width = x;
            Viewport.Height = y;
            Viewport.MinDepth = 0;
            Viewport.MaxDepth = 1.0f;

            mBackFrameBuffers = null;
            mCoreObject.Resize(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, (uint)x, (uint)y);
        }
        public FViewPort GetViewport()
        {
            FViewPort tmp;
            tmp.m_TopLeftX = 0;
            tmp.m_TopLeftY = 0;
            tmp.m_Width = mCoreObject.Desc.Width;
            tmp.m_Height = mCoreObject.Desc.Height;
            tmp.m_MinDepth = 0;
            tmp.m_MaxDepth = 1;
            return tmp;
        }
        public uint BackBufferCount
        {
            get
            {
                return mCoreObject.GetBackBufferCount();
            }
        }
        public uint CurrentBackBuffer
        {
            get
            {
                return mCoreObject.GetCurrentBackBuffer();
            }
        }
        public void BeginFrame()
        {
            mCoreObject.BeginFrame();
        }
        public void Present(uint SyncInterval, uint Flags)
        {
            mCoreObject.Present(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, SyncInterval, Flags);
        }

        UFrameBuffers[] mBackFrameBuffers;
        public URenderPass RenderPass { get; set; }
        public UFrameBuffers GetBackFrameBuffers(uint index)
        {
            if (mBackFrameBuffers == null)
            {
                var Num = mCoreObject.GetBackBufferCount();
                mBackFrameBuffers = new UFrameBuffers[Num];
                for (uint i = 0; i < Num; i++)
                {
                    mBackFrameBuffers[i] = TtEngine.Instance.GfxDevice.RenderContext.CreateFrameBuffers(RenderPass);
                    mBackFrameBuffers[i].mCoreObject.BindRenderTargetView(0, mCoreObject.GetBackRTV(i));

                    mBackFrameBuffers[i].FlushModify();
                }
            }
            return mBackFrameBuffers[index];
        }
        public UFrameBuffers BeginFrameBuffers(ICommandList cmd)
        {
            var index = CurrentBackBuffer;
            var result = GetBackFrameBuffers(index);
            result.mCoreObject.GetRtv(0).GetTexture().NativeSuper.TransitionTo(cmd, EGpuResourceState.GRS_RenderTarget);
            return result;
        }
        public void EndFrameBuffers(ICommandList cmd)
        {
            var index = CurrentBackBuffer;
            var result = GetBackFrameBuffers(index);
            result.mCoreObject.GetRtv(0).GetTexture().NativeSuper.TransitionTo(cmd, EGpuResourceState.GRS_Present);
        }
    }
}
