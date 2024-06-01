using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.TimedSM;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree.Node;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateAttachment<S> : Bricks.StateMachine.TimedSM.TtTimedStateAttachment<S, FAnimBlendTreeTickContext>
    {
        public IBlendTree<TtLocalSpaceRuntimePose> BlendTree = null;

    }
    public class TtClipPlayStateAttachment<S> : TtAnimStateAttachment<S>
    {
        TtAnimationClip AnimationClip;
        public override bool Initialize()
        {
            BlendTree = new TtBlendTree_AnimationClip();
            return base.Initialize();
        }

        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, context);
        }

    }
    /// <summary>
    /// Using for construct the BlendTree from graph
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class TtAnimBlendTreeStateAttachment<S> : TtAnimStateAttachment<S>
    {
        public override bool Initialize()
        {
            //construct the blend tree

            return base.Initialize();
        }

        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, context);
        }

    }
}
