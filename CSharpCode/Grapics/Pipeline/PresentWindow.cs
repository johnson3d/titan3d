using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtPresentWindow : TtNativeWindow
    {
        public TtPresentWindow()
        {
        }
        ~TtPresentWindow()
        {
            Cleanup();
        }
        public bool IsCreatedByImGui = false;
        public bool IsClosed = false;
        public NxRHI.TtSwapChain SwapChain { get; set; }
        private bool mHasPendingResize = false;
        private uint mPendingResizeWidth = 0;
        private uint mPendingResizeHeight = 0;
        public bool CanRender
        {
            get => SwapChain != null && IsRenderable;
        }
        
        public void BeginFrame()
        {
            if (CanRender == false)
                return;
            ApplyPendingResize();
            SwapChain.BeginFrame();
        }
        public void EndFrame()
        {
        }
        public override async Thread.Async.TtTask<bool> Initialize(string title, int x, int y, int w, int h)
        {
            if (false == await base.Initialize(title, x, y, w, h))
                return false;

            return true;
        }
        public unsafe void InitSwapChain(NxRHI.TtGpuDevice rc)
        {
            var scDesc = new NxRHI.FSwapChainDesc();
            scDesc.SetDefault();

            if (rc.mCoreObject.IsSupportSwapchainFormat(EPixelFormat.PXF_R8G8B8A8_UNORM))
            {
                scDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            }
            else if (rc.mCoreObject.IsSupportSwapchainFormat(EPixelFormat.PXF_B8G8R8A8_UNORM))
            {
                scDesc.Format = EPixelFormat.PXF_B8G8R8A8_UNORM;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            //scDesc.BufferCount = 1;
            var size = GetWindowSize();
            scDesc.Width = (uint)size.X;
            scDesc.Height = (uint)size.Y;
            scDesc.OutputWindow = HWindow.ToPointer();
            SwapChain = TtEngine.Instance.GfxDevice.RenderContext.CreateSwapChain(in scDesc);
            mHasPendingResize = false;
            mPendingResizeWidth = scDesc.Width;
            mPendingResizeHeight = scDesc.Height;
        }
        public EPixelFormat GetSwapchainFormat()
        {
            return SwapChain.mCoreObject.GetBackBuffer(0).Desc.Format;
        }
        public EPixelFormat GetSwapchainDSFormat()
        {
            return EPixelFormat.PXF_D24_UNORM_S8_UINT;
        }
        public virtual async System.Threading.Tasks.Task<bool> InitializeGraphics(NxRHI.TtGpuDevice rc, Type rpType)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            InitSwapChain(rc);

            return true;
        }
        public override void Cleanup()
        {
            if (SwapChain != null)
            {
                SwapChain.RenderPass = null;
                SwapChain.Dispose();
                SwapChain = null;
            }

            base.Cleanup();
        }
        public unsafe override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

            if (SwapChain == null)
                return;
            if (x < 1 || y < 1)
                return;

            var width = (uint)x;
            var height = (uint)y;
            if (width < 1 || height < 1)
                return;

            if (SwapChain.mCoreObject.Desc.Width == width && SwapChain.mCoreObject.Desc.Height == height)
            {
                mHasPendingResize = false;
                return;
            }

            mPendingResizeWidth = width;
            mPendingResizeHeight = height;
            mHasPendingResize = true;
            //TtEngine.Instance.EventPoster.PostTickSyncEvent(() =>
            //{
            //    if (SwapChain == null)
            //        return true;
            //    TtEngine.Instance.GfxDevice.RenderCmdQueue.Reset();
            //    SwapChain.OnResize(x, y);
            //    return true;
            //});
        }
        private void ApplyPendingResize()
        {
            if (mHasPendingResize == false || SwapChain == null)
                return;
            if (mPendingResizeWidth < 1 || mPendingResizeHeight < 1)
            {
                mHasPendingResize = false;
                return;
            }
            if (SwapChain.mCoreObject.Desc.Width == mPendingResizeWidth && SwapChain.mCoreObject.Desc.Height == mPendingResizeHeight)
            {
                mHasPendingResize = false;
                return;
            }

            var width = mPendingResizeWidth;
            var height = mPendingResizeHeight;
            mHasPendingResize = false;

            TtEngine.Instance.GfxDevice.RenderQueue.Flush(true);
            SwapChain.OnResize(width, height);
        }
    }
}
