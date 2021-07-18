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
        public RenderPassDesc SwapChainPassDesc = new RenderPassDesc();
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

            SwapChainBuffer.SwapChainIndex = 0;
            SwapChainBuffer.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)w, (uint)h);
            SwapChainBuffer.CreateGBuffer(0, SwapChain.mCoreObject.GetTexture2D());
            SwapChainBuffer.FrameBuffers.mCoreObject.SetSwapChain(SwapChain.mCoreObject);

            SwapChainPassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            SwapChainPassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            SwapChainPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            SwapChainPassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            SwapChainPassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            SwapChainPassDesc.mDepthClearValue = 1.0f;
            SwapChainPassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            SwapChainPassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            SwapChainPassDesc.mStencilClearValue = 0u;
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
                SwapChainBuffer.SwapChainIndex = 0;
                SwapChainBuffer.OnResize(x, y);
                
                if (SwapChain != null)
                {
                    SwapChainBuffer.CreateGBuffer(0, SwapChain.mCoreObject.GetTexture2D());
                    SwapChainBuffer.FrameBuffers.mCoreObject.SetSwapChain(SwapChain.mCoreObject);
                }
            }
        }
        public virtual void TickLogic(int ellapse)
        {
            var cmdlist = SwapChainPass.DrawCmdList.mCoreObject;
            cmdlist.BeginCommand();
            unsafe
            {
                cmdlist.BeginRenderPass(ref SwapChainPassDesc, SwapChainBuffer.FrameBuffers.mCoreObject, "PresentSwapChain");
                cmdlist.BuildRenderPass(0);
                cmdlist.EndRenderPass();
            }
            cmdlist.EndCommand();
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
