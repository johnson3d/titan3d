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
    public class TtAnimState<S> : Bricks.StateMachine.TimedSM.TtTimedState<S, FAnimBlendTreeTickContext>
    {
        public TtBlendTree_CopyPose<TtLocalSpaceRuntimePose> BlendTree = null;
        public override bool Initialize()
        {
            // BlendTree initialize now single animAttachment
            foreach (var attachment in Attachments)
            {
                if (attachment is TtAnimStateAttachment<S> animAttachment)
                {
                    attachment.Initialize();
                    BlendTree.FromNode = animAttachment.BlendTree;
                }
            }
            return base.Initialize();
        }
        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, context);
        }
        public IBlendTree<TtLocalSpaceRuntimePose> ConstructBlendTree(ref FConstructAnimationCommandTreeContext context)
        {
            return null;
        }
    }
}
