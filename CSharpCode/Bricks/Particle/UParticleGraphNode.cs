using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Bricks.CodeBuilder.ContextMenu("Particle", "Particle\\Particle", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UParticleGraphNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color");
        public Graphics.Pipeline.TtRenderGraphPin DepthPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Depth");
        public UParticleGraphNode()
        {
            Name = "ParticleGraphNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public async override System.Threading.Tasks.Task Initialize(Graphics.Pipeline.URenderPolicy policy,
                    string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");
        }
        public List<GamePlay.Scene.UMeshNode> ParticleNodes = new List<GamePlay.Scene.UMeshNode>();
        public override unsafe void BeginTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            cmd.BeginEvent("NebulaUpdate");
        }
        public override unsafe void EndTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.FlushDraws();
            cmd.EndEvent();
            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }
        public override void FrameBuild(URenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
        }
    }
}
