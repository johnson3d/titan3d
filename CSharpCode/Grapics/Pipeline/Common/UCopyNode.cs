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
            AddInput(SrcPinIn, EGpuBufferViewType.GBVT_Srv);
            AddOutput(DestPinOut, EGpuBufferViewType.GBVT_Srv);
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            BasePass.Initialize(rc, debugName);

            mCopyDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDrawcall();
        }
        public UAttachBuffer ResultBuffer;
        public bool IsCpuAceesResult { get; set; } = false;
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CCopyDrawcall mCopyDrawcall;
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
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = BasePass.DrawCmdList;            
            if (cmdlist.BeginCommand())
            {
                var srcPin = GetAttachBuffer(SrcPinIn);
                var tarPin = GetAttachBuffer(DestPinOut);
                if (SrcPinIn.Attachement.Format == EPixelFormat.PXF_UNKNOWN)
                {
                    mCopyDrawcall.SetCopyBuffer(srcPin.Buffer.mCoreObject, 0, tarPin.Buffer.mCoreObject, 0, SrcPinIn.Attachement.Width * SrcPinIn.Attachement.Height);
                }
                else
                {   
                    mCopyDrawcall.SetCopyTexture2D(srcPin.Buffer.mCoreObject, 0, 0, 0, tarPin.Buffer.mCoreObject, 0, 0, 0, SrcPinIn.Attachement.Width, SrcPinIn.Attachement.Height);
                }

                mCopyDrawcall.BuildPass(cmdlist);


                cmdlist.EndCommand();
            }
        }
        public override unsafe void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public override unsafe void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
