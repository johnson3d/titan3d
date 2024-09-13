﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UPresentWindow : TtNativeWindow
    {
        public UPresentWindow()
        {
        }
        ~UPresentWindow()
        {
            Cleanup();
        }
        public bool IsCreatedByImGui = false;
        public bool IsClosed = false;
        public NxRHI.TtSwapChain SwapChain { get; set; }
        
        public void BeginFrame()
        {
            SwapChain.BeginFrame();
        }
        public void EndFrame()
        {
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(string title, int x, int y, int w, int h)
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
            TtEngine.Instance.GfxDevice.RenderSwapQueue.Reset();
            SwapChain.OnResize(x, y);
            //TtEngine.Instance.EventPoster.PostTickSyncEvent(() =>
            //{
            //    if (SwapChain == null)
            //        return true;
            //    TtEngine.Instance.GfxDevice.RenderCmdQueue.Reset();
            //    SwapChain.OnResize(x, y);
            //    return true;
            //});
        }
        public virtual void TickLogic(int ellapse)
        {
            //var cmdlist = SwapChainPass.DrawCmdList;
            //if (cmdlist.BeginCommand())
            //{
            //    var passClears = new NxRHI.FRenderPassClears();
            //    passClears.SetDefault();
            //    passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));
            //    SwapChainBuffer.BuildFrameBuffers(null);
            //    cmdlist.BeginPass(SwapChainBuffer.FrameBuffers, in passClears, "PresentSwapChain");
            //    {
            //        cmdlist.FlushDraws();
            //    }
            //    cmdlist.EndPass();
            //    cmdlist.EndCommand();
            //}
            //policy.CommitCommandList(cmdlist);
        }
        public unsafe virtual void TickRender(int ellapse)
        {
            //var rc = TtEngine.Instance.GfxDevice.RenderContext;
            //var cmdlist = SwapChainPass.CommitCmdList.mCoreObject;
            //cmdlist.Commit(rc.mCoreObject);
        }
        public virtual void TickSync(int ellapse)
        {
            OnDrawSlate();

            SwapChain?.Present(0, 0);
        }
        public virtual void OnDrawSlate()
        {
            
        }
    }
}
