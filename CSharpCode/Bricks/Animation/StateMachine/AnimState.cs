using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Animation.Asset;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.StateMachine.Macross.StateAttachment;
using EngineNS.Bricks.StateMachine.TimedSM;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimState<S> : Bricks.StateMachine.TimedSM.TtTimedState<S>
    {
        public TtBlendTree_CopyPose<TtLocalSpaceRuntimePose> Root = null;
        public override void Update(float elapseSecond, in TtStateMachineContext context)
        {
            foreach (var attachment in Attachments)
            {
                if (attachment is TtAnimStateAttachment<S> animPlay)
                {
                    Root.FromNode = animPlay.Root;
                }
            }
            base.Update(elapseSecond, context);
        }

    }
}
