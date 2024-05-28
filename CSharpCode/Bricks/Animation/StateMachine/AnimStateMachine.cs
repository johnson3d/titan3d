using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.BlendTree;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateMachine<S> : Bricks.StateMachine.TimedSM.TtTimedStateMachine<S>
    {
        public TtBlendTree_BlendPose<TtLocalSpaceRuntimePose> Root;
        public override void Update(float elapseSecond, in TtStateMachineContext context)
        {
            Root.WeightedTree.Add((CurrentState as TtAnimState<S>).Root);
            base.Update(elapseSecond, context);
        }
        public TtAnimationCommand<TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return (CurrentState as TtAnimState<S>).Root.ConstructAnimationCommandTree(parentNode, ref context);
        }
    }
    public class TtAnimStateMachine : TtAnimStateMachine<TtDefaultCenterData>
    {

    }
}
