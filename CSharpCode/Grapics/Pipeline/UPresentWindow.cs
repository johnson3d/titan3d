using System;
using System.Collections.Generic;
using System.Text;
using SDL2;

namespace EngineNS.Graphics.Pipeline
{
    public class UPresentWindow : UNativeWindow
    {
        public UPresentWindow()
        {
        }
        ~UPresentWindow()
        {
            Cleanup();
        }
        public bool IsCreatedByImGui = false;
        public RHI.CSwapChain SwapChain { get; set; }
        public UGraphicsBuffers SwapChainBuffer = new UGraphicsBuffers();
        public RHI.CRenderPass SwapChainRenderPass;
        public UDrawBuffers SwapChainPass = new UDrawBuffers();
        public override async System.Threading.Tasks.Task<bool> Initialize(string title, int x, int y, int w, int h)
        {
            if (false == await base.Initialize(title, x, y, w, h))
                return false;

            return true;
        }
        public unsafe void InitSwapChain(RHI.CRenderContext rc)
        {
            var scDesc = new ISwapChainDesc();
            scDesc.SetDefault();
            int w, h;
            SDL.SDL_GetWindowSize(Window, out w, out h);
            scDesc.Width = (uint)w;
            scDesc.Height = (uint)h;
            scDesc.WindowHandle = HWindow.ToPointer();
            SwapChain = UEngine.Instance.GfxDevice.RenderContext.CreateSwapChain(ref scDesc);

            SwapChainPass.Initialize(rc, "PresentSwapChain");

            var SwapChainPassDesc = new IRenderPassDesc();
            unsafe
            {
                SwapChainPassDesc.NumOfMRT = 1;
                SwapChainPassDesc.AttachmentMRTs[0].IsSwapChain = 1;
                SwapChainPassDesc.AttachmentMRTs[0].Format = SwapChain.mCoreObject.GetBackBuffer(0).mTextureDesc.Format;
                SwapChainPassDesc.AttachmentMRTs[0].Samples = 1;
                SwapChainPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                SwapChainPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                SwapChainPassDesc.m_AttachmentDepthStencil.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
                SwapChainPassDesc.m_AttachmentDepthStencil.Samples = 1;
                SwapChainPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                SwapChainPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                SwapChainPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                SwapChainPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //SwapChainPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //SwapChainPassDesc.mDepthClearValue = 1.0f;
                //SwapChainPassDesc.mStencilClearValue = 0u;
            }
            SwapChainRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in SwapChainPassDesc);

            SwapChainBuffer.Initialize(SwapChainRenderPass, null, 1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)w, (uint)h);
            SwapChainBuffer.BindSwapChain(0, SwapChain);
            SwapChainBuffer.UpdateFrameBuffers(w, h);
        }
        public EPixelFormat GetSwapchainFormat()
        {
            return SwapChain.mCoreObject.GetBackBuffer(0).mTextureDesc.Format;
        }
        public EPixelFormat GetSwapchainDSFormat()
        {
            return EPixelFormat.PXF_D24_UNORM_S8_UINT;
        }
        public virtual async System.Threading.Tasks.Task<bool> InitializeGraphics(RHI.CRenderContext rc, Type rpType)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            InitSwapChain(rc);

            return true;
        }
        public override void Cleanup()
        {
            SwapChain?.Dispose();
            SwapChain = null;

            base.Cleanup();
        }
        public unsafe override void OnResize(float x, float y)
        {
            base.OnResize(x, y);
            SwapChain?.OnResize(x, y);

            if (SwapChainBuffer != null)
            {
                SwapChainBuffer.OnResize(x, y);
                
                if (SwapChain != null)
                {
                    SwapChainBuffer.BindSwapChain(0, SwapChain);
                    SwapChainBuffer.UpdateFrameBuffers(x, y);
                }
            }
        }
        public virtual void TickLogic(int ellapse)
        {
            var cmdlist = SwapChainPass.DrawCmdList.mCoreObject;
            if (cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 0, 0));

                if (cmdlist.BeginRenderPass(SwapChainBuffer.FrameBuffers.mCoreObject, in passClears, "PresentSwapChain"))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public unsafe virtual void TickRender(int ellapse)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = SwapChainPass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public virtual void TickSync(int ellapse)
        {
            OnDrawSlate();

            SwapChain?.mCoreObject.Present(0, 0);
            SwapChainPass?.SwapBuffer();
        }
        public virtual void OnDrawSlate()
        {
            
        }
    }
}
