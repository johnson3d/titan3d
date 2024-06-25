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
        public IBlendTree<TtLocalSpaceRuntimePose> BlendTree = null;

    }
    public class TtClipPlayStateAttachment<S> : TtAnimStateAttachment<S>
    {
        public RName AnimationClipName;
        public override async TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            var animClipBlendTree = new TtBlendTree_AnimationClip();
            animClipBlendTree.Clip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(AnimationClipName);
            BlendTree = animClipBlendTree;
            BlendTree.Initialize(ref context.BlendTreeContext);
            return await base.Initialize(context);
        }

        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);
            //(BlendTree as TtBlendTree_AnimationClip).Time = 
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
