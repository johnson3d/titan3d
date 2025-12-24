using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Bricks.CodeBuilder.ContextMenu("Particle", "Particle\\Particle", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class UParticleGraphNode : TAuxRenderGraphNode<UParticleGraphNode>
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin DepthPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Depth", NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        public UParticleGraphNode()
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
        public NxRHI.TtCommandList mCmdList;
        public override unsafe void BeginTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeBeginTickLogic))
            {
                mCmdList = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
                mCmdList.BeginCommand();
                mCmdList.BeginEvent("NebulaUpdate");
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
                mCmdList.FlushDraws();
                mCmdList.EndEvent();
                mCmdList.EndCommand();
                policy.CommitCommandList(mCmdList);
                mCmdList = null;
            }   
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
    }
}
