using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.Threading.Tasks;
using EngineNS.GamePlay;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UNodePinDefine : Bricks.NodeGraph.UNodePinDefineBase
    {
        [Rtti.Meta]
        public override string Name { get; set; } = "UserPin";
        [Rtti.Meta]
        public override string TypeValue { get; set; } = "Value";

        protected override void InitFromPin<T>(T pin)
        {
            Name = pin.Name;
        }
    }

    public class UCopy2SwapChainNode : URenderGraphNode
    {
        public Common.URenderGraphPin ColorPinIn = Common.URenderGraphPin.CreateInput("Color");
        public Common.URenderGraphPin HitIdPinIn = Common.URenderGraphPin.CreateInput("HitId");
        public Common.URenderGraphPin HzbPinIn = Common.URenderGraphPin.CreateInput("Hzb");
        public Common.URenderGraphPin SavedPinIn0 = Common.URenderGraphPin.CreateInput("Save0");
        public Common.URenderGraphPin ColorPinOut = Common.URenderGraphPin.CreateOutput("Color", true, EPixelFormat.PXF_R8G8B8A8_UNORM);

        public UAttachBuffer ColorAttachement = new UAttachBuffer();
        public NxRHI.UCopyDraw mCopyDrawcall;

        public UCopy2SwapChainNode()
        {
            Name = "Copy2SwapChainNode";
            //NodeDefine.HostNode = this;
            //UpdateInputOutputs();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            HitIdPinIn.IsAllowInputNull = true;
            AddInput(HitIdPinIn, NxRHI.EBufferType.BFT_SRV);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn, NxRHI.EBufferType.BFT_SRV);
            SavedPinIn0.IsAllowInputNull = true;
            AddInput(SavedPinIn0, NxRHI.EBufferType.BFT_SRV);

            AddOutput(ColorPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            await base.Initialize(policy, debugName);
            BasePass.Initialize(rc, debugName + ".BasePass");

            mCopyDrawcall = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            ColorPinOut.Attachement.Width = (uint)x;
            ColorPinOut.Attachement.Height = (uint)y;
            CoreSDK.DisposeObject(ref ColorAttachement);
        }
        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if(ColorAttachement == null || ColorAttachement.BufferDesc.IsMatch(in buffer.BufferDesc) == false)
                {
                    CoreSDK.DisposeObject(ref ColorAttachement);
                    ColorAttachement = new UAttachBuffer();
                    ColorAttachement.CreateBufferViews(in buffer.BufferDesc);
                }

                var attachement = RenderGraph.AttachmentCache.ImportAttachment(ColorPinOut);

                attachement.Buffer = ColorAttachement.Buffer;
                attachement.Srv = ColorAttachement.Srv;
                attachement.Rtv = ColorAttachement.Rtv;
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = BasePass.DrawCmdList;
            cmdlist.BeginCommand();
            {
                var srcPin = GetAttachBuffer(ColorPinIn);
                var tarPin = GetAttachBuffer(ColorPinOut);
                mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                mCopyDrawcall.BindSrc(srcPin.Buffer);
                mCopyDrawcall.BindDest(tarPin.Buffer);

                //mCopyDrawcall.Commit(cmdlist);
                cmdlist.PushGpuDraw(mCopyDrawcall);
            }
            cmdlist.FlushDraws();
            cmdlist.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
        }
    }
}
