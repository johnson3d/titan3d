namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimSubState_To_Dead_4120560633 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_barghest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean CheckCondition(in EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mFrame_CheckCondition_2797600863))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2797600863.SetWatchVariable("context", context);
                returnedValue = CenterData.IsDead;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_1262808213 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<animdmc_barghest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("survivor/monsters/barghest/anims/barghest_run.animclip:Game");
                IsLoop = true;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimSubState_2805292630 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_ClipPlay_1262808213 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_1262808213();
                ClipPlay.CenterData = CenterData;
                await ClipPlay.Initialize(context);
                this.AddAttachment(ClipPlay);
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_1171191897 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<animdmc_barghest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("survivor/monsters/barghest/anims/barghest_deathnormal.animclip:Game");
                IsLoop = false;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Dead_3272594922 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_ClipPlay_1171191897 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_1171191897();
                ClipPlay.CenterData = CenterData;
                await ClipPlay.Initialize(context);
                this.AddAttachment(ClipPlay);
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_2944434644 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_barghest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean CheckCondition(in EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mFrame_CheckCondition_2797600863))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2797600863.SetWatchVariable("context", context);
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimCompoundState_0_3447725427 : EngineNS.Animation.StateMachine.TtAnimCompoundState<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_AnimSubState_2805292630 AnimSubState { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Dead_3272594922 Dead { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_2944434644 Transition_From_AnimCompoundState_0_To_AnimSubState { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimSubState = new DMC_AnimSubState_2805292630();
                AnimSubState.CenterData = CenterData;
                await AnimSubState.Initialize(context);
                AnimSubState.StateMachine = mStateMachine;
                ((DMC_AnimStateMachine_0_1575471455)mStateMachine).SetDefaultState(AnimSubState);
                Dead = new DMC_Dead_3272594922();
                Dead.CenterData = CenterData;
                await Dead.Initialize(context);
                Dead.StateMachine = mStateMachine;
                Transition_From_AnimCompoundState_0_To_AnimSubState = new DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_2944434644();
                Transition_From_AnimCompoundState_0_To_AnimSubState.CenterData = CenterData;
                Transition_From_AnimCompoundState_0_To_AnimSubState.From = this;
                Transition_From_AnimCompoundState_0_To_AnimSubState.To = AnimSubState;
                this.AddTransition(Transition_From_AnimCompoundState_0_To_AnimSubState);
                DMC_Transition_From_AnimSubState_To_Dead_4120560633 Transition_From_AnimSubState_To_Dead = new DMC_Transition_From_AnimSubState_To_Dead_4120560633();
                Transition_From_AnimSubState_To_Dead.CenterData = CenterData;
                Transition_From_AnimSubState_To_Dead.From = AnimSubState;
                Transition_From_AnimSubState_To_Dead.To = Dead;
                AnimSubState.AddTransition(Transition_From_AnimSubState_To_Dead);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimStateMachine_0_1575471455 : EngineNS.Animation.StateMachine.TtAnimStateMachine<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_AnimCompoundState_0_3447725427 AnimCompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                await base.Initialize(context);
                AnimCompoundState_0 = new DMC_AnimCompoundState_0_3447725427();
                AnimCompoundState_0.StateMachine = this;
                AnimCompoundState_0.CenterData = CenterData;
                await AnimCompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_PoseOutput_3721581790 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<animdmc_barghest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_BlendTree_StateMachine_367281382 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_AnimStateMachine<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_AnimStateMachine_0_1575471455 AnimStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                AnimStateMachine_0 = CenterData.AnimStateMachine_0;
                AnimStateMachine_0.CenterData = CenterData;
                AnimStateMachine = AnimStateMachine_0;
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_FinalBlendTree_0_1468431646 : EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<animdmc_barghest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_PoseOutput_3721581790 PoseOutput { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_BlendTree_StateMachine_367281382 BlendTree_StateMachine { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                PoseOutput = new DMC_PoseOutput_3721581790();
                PoseOutput.CenterData = CenterData;
                await PoseOutput.Initialize(context);
                BlendTree_StateMachine = new DMC_BlendTree_StateMachine_367281382();
                BlendTree_StateMachine.CenterData = CenterData;
                await BlendTree_StateMachine.Initialize(context);
                PoseOutput.FromNode = BlendTree_StateMachine;
                FromNode = PoseOutput;
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.barghest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/barghest/animdmc_barghest.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class animdmc_barghest : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        //This is a Variable
        [EngineNS.Rtti.Meta]
        public System.Boolean IsDead { get; set; } = false;
        [EngineNS.Rtti.Meta]
        public DMC_AnimStateMachine_0_1575471455 AnimStateMachine_0 { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_FinalBlendTree_0_1468431646 FinalBlendTree_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_79843837 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override void Tick(System.Single elapseSecond)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Tick_79843837))
            {
                mFrame_Tick_79843837.SetWatchVariable("elapseSecond", elapseSecond);
                Survivor.TtMonsterNode result_Cast_4216252265;
                if ((this.MacrossNode.Parent.Parent.Parent is Survivor.TtMonsterNode))
                {
                    result_Cast_4216252265 = ((Survivor.TtMonsterNode)this.MacrossNode.Parent.Parent.Parent);
                    IsDead = result_Cast_4216252265.StateNode.IsDead;
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize()
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_3062514605))
            {
                System.Boolean returnedValue = default(System.Boolean);
                EngineNS.Animation.StateMachine.TtAnimStateMachineContext animStateMachineContextAnimStateMachine_0 = new EngineNS.Animation.StateMachine.TtAnimStateMachineContext();
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextAnimStateMachine_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseAnimStateMachine_0 = null;
                animatableSkeletonPoseAnimStateMachine_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextAnimStateMachine_0.AnimatableSkeletonPose = animatableSkeletonPoseAnimStateMachine_0;
                animStateMachineContextAnimStateMachine_0.BlendTreeContext = blendTreeContextAnimStateMachine_0;
                AnimStateMachine_0 = new DMC_AnimStateMachine_0_1575471455();
                AnimStateMachine_0.CenterData = this;
                await AnimStateMachine_0.Initialize(animStateMachineContextAnimStateMachine_0);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseFinalBlendTree_0 = null;
                animatableSkeletonPoseFinalBlendTree_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextFinalBlendTree_0.AnimatableSkeletonPose = animatableSkeletonPoseFinalBlendTree_0;
                FinalBlendTree_0 = new DMC_FinalBlendTree_0_1468431646();
                FinalBlendTree_0.CenterData = this;
                await FinalBlendTree_0.Initialize(blendTreeContextFinalBlendTree_0);
                EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose runtimePose = null;
                runtimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(MacrossNode);
                FinalBlendTree_0.SetPose(runtimePose);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/barghest/animdmc_barghest.designmacross", EngineNS.RName.ERNameType.Game));
        public override void AfterTick(System.Single elapseSecond)
        {
            using(var guard_AfterTick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_AfterTick_4051512107))
            {
                mFrame_AfterTick_4051512107.SetWatchVariable("elapseSecond", elapseSecond);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                FinalBlendTree_0.Tick(elapseSecond,ref blendTreeContextFinalBlendTree_0);
            }
        }
    }
}
