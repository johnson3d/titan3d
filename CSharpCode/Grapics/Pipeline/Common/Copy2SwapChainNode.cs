using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.Threading.Tasks;
using EngineNS.GamePlay;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UNodePinDefine : Bricks.NodeGraph.UNodePinDefineBase
    {
        [Rtti.Meta("")]
        public override string Name { get; set; } = "UserPin";
        [Rtti.Meta("")]
        public override string TypeValue { get; set; } = "Value";

        protected override void InitFromPin<T>(T pin)
        {
            Name = pin.Name;
        }
    }

    public class TtEndingNode : TAuxRenderGraphNode<TtEndingNode>
    {
        public TtAttachBuffer ColorAttachement = null;

        public virtual bool IsMainRoot { get => true; }
    }
    [Bricks.CodeBuilder.ContextMenu("AssitRoot", "AssitRoot", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("")]
    public class TtAssitRootNode : TtEndingNode
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);

        public override bool IsMainRoot { get => false; }

        public TtAssitRootNode()
        {
            Name = "AssitRootNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
        }
        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(128, 128, 255);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Copy2SwapChain", "Copy2SwapChain", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UCopy2SwapChainNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UCopy2SwapChainNode" })]
    public class TtCopy2SwapChainNode : TtEndingNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin HitIdPinIn = TtRenderGraphPin.CreateInput("HitId", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin HzbPinIn = TtRenderGraphPin.CreateInput("Hzb", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin SavedPinIn0 = TtRenderGraphPin.CreateInput("Save0", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin ColorPinOut = TtRenderGraphPin.CreateOutput("Color", true, EPixelFormat.PXF_R8G8B8A8_UNORM, NxRHI.EBufferType.BFT_SRV);

        public NxRHI.TtCopyDraw mCopyDrawcall;

        public TtCopy2SwapChainNode()
        {
            ColorAttachement = new TtAttachBuffer();
            Name = "Copy2SwapChainNode";
            //NodeDefine.HostNode = this;
            //UpdateInputOutputs();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn);
            HitIdPinIn.IsAllowInputNull = true;
            AddInput(HitIdPinIn);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn);
            SavedPinIn0.IsAllowInputNull = true;
            AddInput(SavedPinIn0);

            AddOutput(ColorPinOut);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            await base.Initialize(policy, debugName);
            
            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ColorAttachement);
            CoreSDK.DisposeObject(ref ColorOutAttachement);
            CoreSDK.DisposeObject(ref mCopyDrawcall);
            base.Dispose();
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            ColorPinOut.Attachement.Width = (uint)x;
            ColorPinOut.Attachement.Height = (uint)y;
            CoreSDK.DisposeObject(ref ColorAttachement);
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        TtAttachBuffer ColorOutAttachement = new TtAttachBuffer();
        public override void BeforeTick(TtRenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"TtCopy2SwapChainNode can't find attach buffer for pin({policy.RPolicyName}:{ColorPinIn.Name}), RenderPolicy will create one ");
                buffer = this.GetAttachBuffer(ColorPinIn);
            }
            if (ColorAttachement == null || ColorAttachement.BufferDesc.IsMatch(in buffer.BufferDesc) == false)
            {
                CoreSDK.DisposeObject(ref ColorAttachement);
                ColorAttachement = new TtAttachBuffer();
                ColorAttachement.CreateBufferViews(in buffer.BufferDesc);
            }

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ColorPinOut, ColorOutAttachement);

            attachement.BufferDesc = ColorAttachement.BufferDesc;
            attachement.GpuResource = ColorAttachement.GpuResource;
            attachement.Srv = ColorAttachement.Srv;
            attachement.Rtv = ColorAttachement.Rtv;
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = NxRHI.TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "Copy2SwapChain"))
            {
                var srcPin = GetAttachBuffer(ColorPinIn);
                var tarPin = GetAttachBuffer(ColorPinOut);
                mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                mCopyDrawcall.BindSrc(srcPin.GpuResource);
                mCopyDrawcall.BindDest(tarPin.GpuResource);

                //mCopyDrawcall.Commit(cmdlist);
                cmdlist.PushGpuDraw(mCopyDrawcall);
                cmdlist.BeginEvent(Name);
                cmdlist.FlushDraws();
                cmdlist.EndEvent();
            }
            policy.CommitCommandList(cmdlist, "Copy2SwapChain");
        }
    }
}
