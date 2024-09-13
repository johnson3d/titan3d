using EngineNS.Animation.Animatable;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Player
{
    public class TtAnimStateMachinePlayer : IAnimationPlayer
    {
        public TtAnimStateMachine<TestData> StateMachine { get; protected set; } = null;
        TtAnimStateMachineContext StateMachineContext = new TtAnimStateMachineContext();
        public TtLocalSpaceRuntimePose OutPose = null;
        //protected TtAnimationPropertiesSetter AnimationPropertiesSetter = null;
        public TtAnimStateMachinePlayer()
        {

        }
        public virtual async void Initialize()
        {
            StateMachine = new TtTestAnimStateMachine();
            
        }
        public async TtTask BindingPose(TtAnimatableSkeletonPose bindedPose)
        {
            System.Diagnostics.Debug.Assert(bindedPose != null);
            StateMachineContext.BlendTreeContext.AnimatableSkeletonPose = bindedPose.Clone() as TtAnimatableSkeletonPose;
            await StateMachine.Initialize(StateMachineContext);
        }

        public void Update(float elapse)
        {
            if (StateMachine.BlendTree == null)
                return;

            StateMachine.Tick(elapse, in StateMachineContext);
        }

        public void Evaluate()
        {
            //make command
            //if(IsImmediate)
            var blendTree = StateMachine.BlendTree;
            if (blendTree == null)
                return;
            FConstructAnimationCommandTreeContext context = new();
            context.CmdExecuteStack = new();
            var commandTree = blendTree.ConstructAnimationCommandTree(null, ref context);
            var stack = context.CmdExecuteStack;
            stack.Execute();
            
            TtRuntimePoseUtility.CopyPose(ref OutPose, commandTree.OutPose);
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }
    }

    #region Test

    public class TestData
    {

    }
    public class TestState : TtAnimState<TestData>
    {
        public override async Thread.Async.TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            var clip = new TtClipPlayStateAttachment<TestData>();
            clip.AnimationClipName = RName.GetRName("utest/puppet/animation/w2_run_f_loop_ip.animclip");
            await clip.Initialize(context);
            Attachments.Add(clip);
            return await base.Initialize(context);
        }
    }
    public class TtTestAnimStateMachine : TtAnimStateMachine<TestData>
    {
        public override async Thread.Async.TtTask<bool> Initialize(TtAnimStateMachineContext context)
        {
            var ss = new TestState();
            await ss.Initialize(context);
            BlendTree = new BlendTree.Node.TtBlendTree_CrossfadePose<TestData, TtLocalSpaceRuntimePose>();
            await BlendTree.Initialize(context.BlendTreeContext);
            SetDefaultState(ss);
            return await base.Initialize(context);
        }
    }
    #endregion
}
