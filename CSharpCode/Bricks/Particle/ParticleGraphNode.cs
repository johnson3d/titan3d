using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Bricks.CodeBuilder.ContextMenu("Particle", "Particle\\Particle", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Particle.UParticleGraphNode@EngineCore", "EngineNS.Bricks.Particle.UParticleGraphNode" })]
    public class TtParticleGraphNode : TAuxRenderGraphNode<TtParticleGraphNode>
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin DepthPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Depth", NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        public TtParticleGraphNode()
        {
            Name = "ParticleGraphNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut);
            AddInputOutput(DepthPinInOut);
        }
        public async override Thread.Async.TtTask Initialize(Graphics.Pipeline.TtRenderPolicy policy,
                    string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
        }
        public List<TtEmitter> CurEmitters = new List<TtEmitter>();
        public List<TtEmitter> PrevEmitters = new List<TtEmitter>();
        public override void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "ParticleUpdate"))
            {
                foreach (var e in PrevEmitters)
                {
                    e.UpdateGPU(cmdlist, this, 0);
                }
                PrevEmitters.Clear();
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist, "ParticleUpdate");
            
            policy.RegisterAfterPolicyFinishedCallback(static (rp, arg) =>
            {
                CoreSDK.Swap(ref ((TtParticleGraphNode)arg).CurEmitters, ref ((TtParticleGraphNode)arg).PrevEmitters);
            }, this);
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
    }
}
