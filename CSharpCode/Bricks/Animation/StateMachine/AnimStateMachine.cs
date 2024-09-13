using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.BlendTree.Node;
using EngineNS.Animation.Command;
using EngineNS.Animation.BlendTree;
using EngineNS.Bricks.StateMachine;
using EngineNS.Thread.Async;

namespace EngineNS.Animation.StateMachine
{
    public class TtAnimStateMachineContext
    {
        public FAnimBlendTreeContext BlendTreeContext;
    }
    public class TtAnimStateMachine<S> : Bricks.StateMachine.TimedSM.TtTimedStateMachine<S, TtAnimStateMachineContext>
    {
        public TtBlendTree_CrossfadePose<S, TtLocalSpaceRuntimePose> BlendTree;
        public override async TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            BlendTree = new();
            await BlendTree.Initialize(context.BlendTreeContext);
            return await base.Initialize(context);
        }
        public override void Tick(float elapseSecond, in TtAnimStateMachineContext context)
        {
            base.Tick(elapseSecond, context);
            BlendTree.Tick(elapseSecond, ref context.BlendTreeContext);
        }
        public override void SetDefaultState(IState<S, TtAnimStateMachineContext> state)
        {
            base.SetDefaultState(state);
            if(CurrentState is TtAnimState<S> animCurrentState)
            {
                BlendTree.ToNode = animCurrentState.BlendTree;
            }    
        }
        public override void OnStateChange(IState<S, TtAnimStateMachineContext> preState, IState<S, TtAnimStateMachineContext> currentState)
        {
            base.OnStateChange(preState, currentState);
            if (preState == null)
            {
                BlendTree.ToNode = (CurrentState as TtAnimState<S>).BlendTree;
            }
            else
            {
                var cf = new TtBlendTree_CrossfadePose<S, TtLocalSpaceRuntimePose>();
                cf.FromNode = BlendTree;
                cf.ToNode = (CurrentState as TtAnimState<S>).BlendTree;
                BlendTree = cf;
            }
        }
        
    }
    public class TtAnimStateMachine : TtAnimStateMachine<TtDefaultCenterData>
    {

    }
}
