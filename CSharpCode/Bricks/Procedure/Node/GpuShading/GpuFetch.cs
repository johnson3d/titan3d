using System;
using System.Collections.Generic;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.Procedure.Node.GpuShading
{
    [Bricks.CodeBuilder.ContextMenu("GpuFetch", "PGC\\GpuFetch", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtGpuFetchNode : Graphics.Pipeline.Common.TtEndingNode
    {
        public Graphics.Pipeline.TtRenderGraphPin SrcPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Src");
        public TtGpuFetchNode()
        {
            Name = "GpuFetch";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn, NxRHI.EBufferType.BFT_SRV);
            SrcPinIn.IsAllowInputNull = true;
        }
        public override void Dispose()
        {
            CoreSDK.DisposePtr(ref ReadableTexture);
            CoreSDK.DisposeObject(ref mFinishFence);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override async Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            mFinishFence = rc.CreateFence(in fenceDesc, "GpuFetch");
            mCmdList = rc.CreateCommandList();
        }
        public NxRHI.UFence mFinishFence;
        public NxRHI.UCommandList mCmdList;
        public NxRHI.IBuffer ReadableTexture;
        public override void TickLogic(UWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList))
            {
                var cpDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                var texture = policy.AttachmentCache.FindAttachement(SrcPinIn).GpuResource;
                CoreSDK.DisposePtr(ref ReadableTexture);
                ReadableTexture = texture.CreateReadable(0, cpDraw.mCoreObject);
                mCmdList.PushGpuDraw(cpDraw);
                mCmdList.FlushDraws();
            }
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(mFinishFence, NxRHI.EQueueType.QU_Compute);
        }
    }
}
