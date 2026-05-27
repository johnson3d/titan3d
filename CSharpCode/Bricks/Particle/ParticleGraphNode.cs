using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Particle.UParticleGraphNode@EngineCore", "EngineNS.Bricks.Particle.UParticleGraphNode" })]
    public class TtParticleGraphNode : TAuxRenderGraphNode<TtParticleGraphNode>
    {
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput(
            "Result", false, EPixelFormat.PXF_UNKNOWN,
            EBufferType.BFT_SRV);

        public TtParticleGraphNode()
        {
            Name = "ParticleGraphNode";
        }
        public override void InitNodePins()
        {
            AddOutput(ResultPinOut);
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
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    /// <summary>
    /// TtViewportSlate partial: 持有粒子迷你 RenderPolicy。
    /// 每个 viewport 各一个 TtParticleGraphNode，通过 CurEmitters 收集当帧 GPU emitter，
    /// 在渲染线程统一 dispatch 更新。迷你 Policy 共享主 Policy 的 DefaultCamera。
    /// </summary>
    public partial class TtViewportSlate
    {
        TtRenderPolicy mParticlePolicy;
        Bricks.Particle.TtParticleGraphNode mParticleNode;
        TtRCmdQueue mParticleCmdQueue;

        public Bricks.Particle.TtParticleGraphNode ParticleNode => mParticleNode;

        public async Thread.Async.TtTask InitParticlePolicy()
        {
            mParticlePolicy = new TtRenderPolicy();
            mParticleCmdQueue = new TtRCmdQueue();
            mParticlePolicy.CmdQueue = mParticleCmdQueue;

            mParticleNode = new Bricks.Particle.TtParticleGraphNode();
            mParticleNode.InitNodePins();

            var endingNode = new TtAssitRootNode();
            endingNode.InitNodePins();
            endingNode.Name = "ParticleEnding";

            mParticlePolicy.RegRenderNode2("ParticleCompute", mParticleNode);
            mParticlePolicy.RegRenderNode2("ParticleEnding", endingNode);

            mParticlePolicy.AddLinker(mParticleNode.ResultPinOut, endingNode.SrcPinIn);
            mParticlePolicy.RootNode = endingNode;

            bool hasInputError = false;
            mParticlePolicy.BuildGraph(ref hasInputError);

            foreach (var kvp in mParticlePolicy.GraphNodes)
            {
                await kvp.Value.Initialize(mParticlePolicy, kvp.Value.Name);
            }
        }

        /// <summary>
        /// 在 TickLogic 中调用，驱动粒子迷你 Policy 在渲染线程执行 GPU emitter 更新
        /// </summary>
        void TickParticleUpdate()
        {
            if (mParticlePolicy == null || mParticleNode == null)
                return;
            if (mParticleNode.CurEmitters.Count == 0 && mParticleNode.PrevEmitters.Count == 0)
                return;

            // 同步主 Policy 的 camera 给迷你 Policy
            //mParticlePolicy.DefaultCamera = RenderPolicy?.DefaultCamera;

            TtEngine.Instance.ThreadRender.QueueRenderAction("ParticleUpdate", static (in Thread.TtThreadRender.FRenderAction RAct) =>
            {
                var This = RAct.Arg as TtViewportSlate;
                This.mParticlePolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent("ParticleUpdate");
                }, "Begin:ParticleUpdate");

                This.mParticlePolicy.BeginTick(This.World);
                This.mParticlePolicy.Tick(This.World, null);
                This.mParticlePolicy.EndTick(This.World);

                This.mParticlePolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent("ParticleUpdate");
                }, "End:ParticleUpdate");
                This.mParticlePolicy.ExecuteCmdQueue(false);

                CoreSDK.Swap(ref This.mParticleNode.CurEmitters, ref This.mParticleNode.PrevEmitters);
            }, this);
        }

        void DisposeParticlePolicy()
        {
            if (mParticlePolicy != null)
            {
                mParticlePolicy.Dispose();
                mParticlePolicy = null;
            }
            mParticleNode = null;
            mParticleCmdQueue = null;
        }
    }
}
