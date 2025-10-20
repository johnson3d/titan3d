using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Thread.Async;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateAttachment<S> : Bricks.StateMachine.TimedSM.TtTimedStateAttachment<S, TtAnimStateMachineContext>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> BlendTree = null;

    }
    public class TtClipPlayStateAttachment<S> : TtAnimStateAttachment<S>
    {
        public RName AnimationClipName { get; set; }
        public bool IsLoop { get; set; } = false;
        public override async TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            var animClipBlendTree = new TtBlendTree_AnimationClip<S>();
            animClipBlendTree.Clip = await AnimationClipName.GetAsset<Animation.Asset.TtAnimationClip>();
            animClipBlendTree.IsLoop = IsLoop;
            BlendTree = animClipBlendTree;
            await BlendTree.Initialize(context.BlendTreeContext);
            return await base.Initialize(context);
        }

        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);

            BlendTree.Tick(elapseSecond, ref context.BlendTreeContext);
        }
    }
    public class TtBlendSpacePlayStateAttachment<S> : TtAnimStateAttachment<S>
    {
        public RName AnimationName { get; set; }
        public Vector3 Input = Vector3.Zero;
        public override async TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            var blendSpaceBlendTree = new TtBlendTree_BlendSpace2D<S>();
            blendSpaceBlendTree.BlendSpace = await AnimationName.GetAsset<Animation.Asset.BlendSpace.TtBlendSpace2D>();
            BlendTree = blendSpaceBlendTree;
            await BlendTree.Initialize(context.BlendTreeContext);
            return await base.Initialize(context);
        }

        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);
            var blendSpace = BlendTree as TtBlendTree_BlendSpace2D<S>;
            blendSpace.Input = Input;
            BlendTree.Tick(elapseSecond, ref context.BlendTreeContext);
        }
    }
    /// <summary>
    /// Using for construct the BlendTree from graph
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class TtAnimBlendTreeStateAttachment<S> : TtAnimStateAttachment<S>
    {
        public override async TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            //construct the blend tree

            return await base.Initialize(context);
        }

        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, ref context.BlendTreeContext);
        }

    }
}
