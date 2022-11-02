using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UCopyNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin SrcPinIn = Common.URenderGraphPin.CreateInput("Src");
        public Common.URenderGraphPin DestPinOut = Common.URenderGraphPin.CreateOutput("Dest", false, EPixelFormat.PXF_UNKNOWN);
        public UCopyNode()
        {
            Name = "CopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn, NxRHI.EBufferType.BFT_SRV);
            AddOutput(DestPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            BasePass.Initialize(rc, debugName);

            mCopyDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public UAttachBuffer ResultBuffer;
        public bool IsCpuAceesResult { get; set; } = false;
        public UDrawBuffers BasePass = new UDrawBuffers();
        public NxRHI.UCopyDraw mCopyDrawcall;
        public override void FrameBuild()
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(DestPinOut);
            if (SrcPinIn.Attachement.Format != DestPinOut.Attachement.Format ||
                SrcPinIn.Attachement.Width != DestPinOut.Attachement.Width ||
                SrcPinIn.Attachement.Height != DestPinOut.Attachement.Height)
            {
                ResultBuffer = attachement.Clone(IsCpuAceesResult, SrcPinIn.Attachement);
                attachement.Buffer = ResultBuffer.Buffer;
                attachement.Srv = ResultBuffer.Srv;
                attachement.Rtv = ResultBuffer.Rtv;
                attachement.Dsv = ResultBuffer.Dsv;
                attachement.Uav = ResultBuffer.Uav;
                DestPinOut.Attachement.Format = SrcPinIn.Attachement.Format;
                DestPinOut.Attachement.Width = SrcPinIn.Attachement.Width;
                DestPinOut.Attachement.Height = SrcPinIn.Attachement.Height;
            }
            if (ResultBuffer != null)
            {
                attachement.Buffer = ResultBuffer.Buffer;
                attachement.Srv = ResultBuffer.Srv;
                attachement.Rtv = ResultBuffer.Rtv;
                attachement.Dsv = ResultBuffer.Dsv;
                attachement.Uav = ResultBuffer.Uav;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UCopyNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mCopyDrawcall == null)
                    return;
                var cmdlist = BasePass.DrawCmdList;
                if (cmdlist.BeginCommand())
                {
                    var srcPin = GetAttachBuffer(SrcPinIn);
                    var tarPin = GetAttachBuffer(DestPinOut);

                    mCopyDrawcall.BindSrc(srcPin.Buffer);
                    mCopyDrawcall.BindSrc(tarPin.Buffer);

                    //if (SrcPinIn.Attachement.Format == EPixelFormat.PXF_UNKNOWN)
                    //{
                    //    //SetCopyBuffer(srcPin.Buffer.mCoreObject, 0, tarPin.Buffer.mCoreObject, 0, SrcPinIn.Attachement.Width * SrcPinIn.Attachement.Height);
                    //}
                    //else
                    //{   
                    //    mCopyDrawcall.SetCopyTexture2D(srcPin.Buffer.mCoreObject, 0, 0, 0, tarPin.Buffer.mCoreObject, 0, 0, 0, SrcPinIn.Attachement.Width, SrcPinIn.Attachement.Height);
                    //}

                    mCopyDrawcall.Commit(cmdlist);

                    cmdlist.EndCommand();
                }
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
            }   
        }
        
        public override unsafe void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
