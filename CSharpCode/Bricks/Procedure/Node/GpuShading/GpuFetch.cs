using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Procedure.Node.GpuShading
{
    [Bricks.CodeBuilder.ContextMenu("GpuFetch", "PGC\\GpuFetch", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtGpuFetchNode : Graphics.Pipeline.Common.TtEndingNode
    {
        public Graphics.Pipeline.TtRenderGraphPin SrcPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);
        public TtGpuFetchNode()
        {
            Name = "GpuFetch";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
            SrcPinIn.IsAllowInputNull = true;
        }
        public override void Dispose()
        {
            CoreSDK.DisposePtr(ref ReadableTexture);
            CoreSDK.DisposeObject(ref mFinishFence);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            mFinishFence = rc.CreateFence(in fenceDesc, "GpuFetch");
            mCmdList = rc.CreateCommandList();
        }
        public NxRHI.TtFence mFinishFence;
        public NxRHI.TtCommandList mCmdList;
        public NxRHI.IBuffer ReadableTexture;
        public override void Tick(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList, "PCG.GpuFetch"))
            {
                var cpDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                var texture = policy.AttachmentCache.FindAttachement(SrcPinIn).GpuResource;
                CoreSDK.DisposePtr(ref ReadableTexture);
                ReadableTexture = texture.CreateReadable(0, cpDraw.mCoreObject);
                mCmdList.PushGpuDraw(cpDraw);
                mCmdList.FlushDraws();
                CoreSDK.DisposeObject(ref cpDraw);
            }
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmdlist(mCmdList, "PCG.GpuFetch", NxRHI.EQueueType.QU_Compute, true);
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(mFinishFence, info.QueueType);
            }, "PCG.GpuFetch.FenceSignal", null, NxRHI.EQueueType.QU_Compute, ERCmdType.Cmd, true); ;
        }
    }
}
