using EngineNS.GamePlay;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("FenceIncrease", "Utility\\FenceIncrease", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtFenceIncreaseNode : TAuxRenderGraphNode<TtFenceIncreaseNode>
    {
        public TtRenderGraphPin BeforePinIn = TtRenderGraphPin.CreateInputOutput("Before", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin FencePinOut = TtRenderGraphPin.CreateOutput("Fence", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);
        public TtFenceIncreaseNode()
        {
            Name = "FenceIncrease";
        }
        public override void InitNodePins()
        {
            AddInputOutput(BeforePinIn);
            AddOutput(FencePinOut);
            FencePinOut.LinkType = "Fence";
        }
        public NxRHI.TtFence Fence { get; private set; }
        public ulong ExpectValue
        {
            get;
            set;
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            var cmdlist = TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "Fence"))
            {
                Fence = policy.FindOrCreateFence(Name);
                Fence.IncreaseExpect(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetCmdQueue(), 1, EQueueType.QU_Default);
                ExpectValue = Fence.ExpectValue;
            }
            policy.CommitCommandList(cmdlist, "Fence");
        }
        public void WaitFence()
        {
            if (Fence==null)
            {
                return;
            }
            Fence.Wait(ExpectValue);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("FenceWait", "Utility\\FenceWait", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtFenceWaitNode : TAuxRenderGraphNode<TtFenceWaitNode>
    {
        public TtRenderGraphPin FencePinIn = TtRenderGraphPin.CreateInput("Fence", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin AfterPinOut = TtRenderGraphPin.CreateOutput("After",  false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV);
        public TtFenceWaitNode()
        {
            Name = "FenceWait";
        }
        public override void InitNodePins()
        {
            AddInput(FencePinIn);
            FencePinIn.LinkType = "Fence";
            AddOutput(AfterPinOut);
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            var linker = policy.FindInLinker(FencePinIn);
            if (linker == null)
                return;
            var fenceNode = linker.OutPin.HostNode as TtFenceIncreaseNode;
            if (fenceNode == null)
                return;
            NxRHI.TtFence fence = policy.FindFence(fenceNode.Name);
            if (fence == null)
                return;
            policy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.WaitFence(fence, fenceNode.ExpectValue, EQueueType.QU_Default);
            }, "WaitFence");
        }
    }
}
