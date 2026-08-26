using EngineNS.Animation.Animatable;
using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.Montage;
using EngineNS.Animation.RootMotion;
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
        /// <summary>
        /// Montage宿主, 每帧先于状态机推进
        /// </summary>
        public TtAnimMontageHost MontageHost { get; } = new TtAnimMontageHost();
        /// <summary>
        /// RootMotion过滤模式, 由持有本播放器的节点设置
        /// </summary>
        public ERootMotionMode RootMotionMode
        {
            get => mRootMotionMode;
            set
            {
                mRootMotionMode = value;
                MontageHost.RootMotionMode = value;
                StateMachineContext.BlendTreeContext.RootMotionMode = value;
            }
        }
        ERootMotionMode mRootMotionMode = ERootMotionMode.Ignore;

        /// <summary>
        /// 顶层Montage Slot节点: 把状态机输出作为Source, Montage按权重覆盖在上面。
        /// 图Source已由状态机驱动, 所以置TickSourceNode=false。
        /// </summary>
        BlendTree.Node.TtBlendTree_Slot<TestData> mMontageSlot = null;
        /// <summary>
        /// 顶层Slot节点响应的Slot名, 需与Montage资产里Slot轨道的名字一致。
        /// 必须在BindingPose之前设置。
        /// </summary>
        public string MontageSlotName { get; set; } = "DefaultSlot";
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
            var runtimeBindingPose = bindedPose.Clone() as TtAnimatableSkeletonPose;
            StateMachineContext.BlendTreeContext.AnimatableSkeletonPose = runtimeBindingPose;
            StateMachineContext.BlendTreeContext.RootMotionMode = mRootMotionMode;
            MontageHost.BindingPose(runtimeBindingPose);
            MontageHost.RootMotionMode = mRootMotionMode;
            StateMachineContext.BlendTreeContext.MontageHost = MontageHost;
            await StateMachine.Initialize(StateMachineContext);

            mMontageSlot = new BlendTree.Node.TtBlendTree_Slot<TestData>();
            mMontageSlot.SlotName = MontageSlotName;
            mMontageSlot.TickSourceNode = false;
            await mMontageSlot.Initialize(StateMachineContext.BlendTreeContext);
        }

        public void Update(float elapse)
        {
            if (StateMachine.BlendTree == null)
                return;

            // Montage必须先推进: Slot节点求值时需要本帧的权重与位置
            MontageHost.Tick(elapse);
            StateMachine.Tick(elapse, in StateMachineContext);
            var context = StateMachineContext.BlendTreeContext;
            mMontageSlot?.Tick(elapse, ref context);
        }

        public void Evaluate()
        {
            //make command
            //if(IsImmediate)
            var blendTree = StateMachine.BlendTree;
            if (blendTree == null)
                return;

            IBlendTree<TestData, TtLocalSpaceRuntimePose> rootNode = blendTree;
            if (mMontageSlot != null)
            {
                mMontageSlot.SourceNode = blendTree;
                rootNode = mMontageSlot;
            }

            FConstructAnimationCommandTreeContext context = new();
            context.CmdExecuteStack = new();
            var commandTree = rootNode.ConstructAnimationCommandTree(null, ref context);
            var stack = context.CmdExecuteStack;
            stack.Execute();
            
            TtRuntimePoseUtility.CopyPose(ref OutPose, commandTree.OutPose);
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }

        #region Montage

        public TtAnimMontageInstance Montage_Play(Asset.TtAnimMontage montage, float playRate = 1.0f, float blendInTime = -1.0f, string startSectionName = null)
        {
            return MontageHost.Play(montage, playRate, blendInTime, startSectionName);
        }
        public void Montage_Stop(Asset.TtAnimMontage montage, float blendOutTime = -1.0f)
        {
            MontageHost.Stop(montage, blendOutTime);
        }
        public void Montage_StopAll(float blendOutTime = -1.0f)
        {
            MontageHost.StopAll(blendOutTime);
        }
        public bool Montage_IsPlaying(Asset.TtAnimMontage montage)
        {
            return MontageHost.IsPlaying(montage);
        }
        public bool Montage_JumpToSection(Asset.TtAnimMontage montage, string sectionName)
        {
            return MontageHost.JumpToSection(montage, sectionName);
        }
        public void Montage_SetNextSection(Asset.TtAnimMontage montage, string sectionName, string nextSectionName)
        {
            MontageHost.SetNextSectionName(montage, sectionName, nextSectionName);
        }
        public string Montage_GetCurrentSection(Asset.TtAnimMontage montage)
        {
            return MontageHost.GetCurrentSection(montage);
        }
        #endregion Montage
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
