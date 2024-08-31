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
        public async override System.Threading.Tasks.Task Initialize(Graphics.Pipeline.TtRenderPolicy policy,
                    string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");
        }
        public List<GamePlay.Scene.TtMeshNode> ParticleNodes = new List<GamePlay.Scene.TtMeshNode>();
        [ThreadStatic]
        private static Profiler.TimeScope mScopeBeginTickLogic;
        private static Profiler.TimeScope ScopeBeginTickLogic
        {
            get
            {
                if (mScopeBeginTickLogic == null)
                    mScopeBeginTickLogic = new Profiler.TimeScope(typeof(UParticleGraphNode), nameof(BeginTickLogic));
                return mScopeBeginTickLogic;
            }
        }
        public override unsafe void BeginTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeBeginTickLogic))
            {
                var cmd = BasePass.DrawCmdList;
                cmd.BeginCommand();
                cmd.BeginEvent("NebulaUpdate");
            }   
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeEndTickLogic;
        private static Profiler.TimeScope ScopeEndTickLogic
        {
            get
            {
                if (mScopeEndTickLogic == null)
                    mScopeEndTickLogic = new Profiler.TimeScope(typeof(UParticleGraphNode), nameof(EndTickLogic));
                return mScopeEndTickLogic;
            }
        }
        public override unsafe void EndTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeEndTickLogic))
            {
                var cmd = BasePass.DrawCmdList;
                cmd.FlushDraws();
                cmd.EndEvent();
                cmd.EndCommand();
                policy.CommitCommandList(cmd);
            }   
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            base.TickLogic(world, policy, bClear);
        }
    }
}
