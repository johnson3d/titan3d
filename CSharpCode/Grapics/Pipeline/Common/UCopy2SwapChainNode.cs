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

    public class TtEndingNode : TtRenderGraphNode
    {
        public TtAttachBuffer ColorAttachement = null;
    }
    [Bricks.CodeBuilder.ContextMenu("Copy2SwapChain", "Copy2SwapChain", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UCopy2SwapChainNode : TtEndingNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin HitIdPinIn = TtRenderGraphPin.CreateInput("HitId");
        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput("Hzb");
        public TtRenderGraphPin SavedPinIn0 = TtRenderGraphPin.CreateInput("Save0");
        public TtRenderGraphPin ColorPinOut = TtRenderGraphPin.CreateOutput("Color", true, EPixelFormat.PXF_R8G8B8A8_UNORM);

        public NxRHI.UCopyDraw mCopyDrawcall;

        public UCopy2SwapChainNode()
        {
            ColorAttachement = new TtAttachBuffer();
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
                    ColorAttachement = new TtAttachBuffer();
                    ColorAttachement.CreateBufferViews(in buffer.BufferDesc);
                }

                var attachement = RenderGraph.AttachmentCache.ImportAttachment(ColorPinOut);

                attachement.GpuResource = ColorAttachement.GpuResource;
                attachement.Srv = ColorAttachement.Srv;
                attachement.Rtv = ColorAttachement.Rtv;
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                var srcPin = GetAttachBuffer(ColorPinIn);
                var tarPin = GetAttachBuffer(ColorPinOut);
                mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                mCopyDrawcall.BindSrc(srcPin.GpuResource);
                mCopyDrawcall.BindDest(tarPin.GpuResource);

                //mCopyDrawcall.Commit(cmdlist);
                cmdlist.PushGpuDraw(mCopyDrawcall);
                cmdlist.FlushDraws();
            }
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist);
        }
    }
}
