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
using NPOI.SS.Formula.Functions;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimState<S> : Bricks.StateMachine.TimedSM.TtTimedState<S, TtAnimStateMachineContext>
    {
        public TtBlendTree_CopyPose<TtLocalSpaceRuntimePose> BlendTree = null;
        public override async Thread.Async.TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            BlendTree = new TtBlendTree_CopyPose<TtLocalSpaceRuntimePose>();
            // BlendTree initialize now single animAttachment
            foreach (var attachment in Attachments)
            {
                if (attachment is TtAnimStateAttachment<S> animAttachment)
                {
                    await attachment.Initialize(context);
                    BlendTree.FromNode = animAttachment.BlendTree;
                }
            }
            return await base.Initialize(context);
        }
        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, ref context.BlendTreeContext);
        }
    }
}
