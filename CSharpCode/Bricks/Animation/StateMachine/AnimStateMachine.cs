using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.BlendTree;
using EngineNS.Bricks.StateMachine;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateMachine<S> : Bricks.StateMachine.TimedSM.TtTimedStateMachine<S, FAnimBlendTreeTickContext>
    {
        public TtBlendTree_CrossfadePose<TtLocalSpaceRuntimePose> BlendTree;
        public override void Tick(float elapseSecond, in FAnimBlendTreeTickContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, context);
        }
        public override void SetDefaultState(IState<S, FAnimBlendTreeTickContext> state)
        {
            base.SetDefaultState(state);
            if(CurrentState is TtAnimState<S> animCurrentState)
            {
                BlendTree.ToNode = animCurrentState.BlendTree;
            }    
        }
        public override void OnStateChange(IState<S, FAnimBlendTreeTickContext> preState, IState<S, FAnimBlendTreeTickContext> currentState)
        {
            base.OnStateChange(preState, currentState);
            if (preState == null)
            {
                BlendTree.ToNode = (CurrentState as TtAnimState<S>).BlendTree;
            }
            else
            {
                var cf = new TtBlendTree_CrossfadePose<TtLocalSpaceRuntimePose>();
                cf.FromNode = BlendTree;
                cf.ToNode = (CurrentState as TtAnimState<S>).BlendTree;
                BlendTree = cf;
            }
        }
        public IBlendTree<TtLocalSpaceRuntimePose> ConstructBlendTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            return BlendTree;
        }
        
    }
    public class TtAnimStateMachine : TtAnimStateMachine<TtDefaultCenterData>
    {

    }
}
