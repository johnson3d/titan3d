namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_Idle_To_Dead_2672857839 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_dragonthesouleater>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_1690228518 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<animdmc_dragonthesouleater>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("survivor/monsters/dragon1/walkanim.animclip:Game");
                IsLoop = true;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Idle_2510730741 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_ClipPlay_1690228518 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_1690228518();
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_1035804480 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<animdmc_dragonthesouleater>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("survivor/monsters/dragon1/dieanim.animclip:Game");
                IsLoop = false;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Dead_3360116841 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_ClipPlay_1035804480 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_1035804480();
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimCompoundState_0_To_Idle_3200381843 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_dragonthesouleater>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimCompoundState_0_2028502536 : EngineNS.Animation.StateMachine.TtAnimCompoundState<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_Idle_2510730741 Idle { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Dead_3360116841 Dead { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_AnimCompoundState_0_To_Idle_3200381843 Transition_From_AnimCompoundState_0_To_Idle { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                Idle = new DMC_Idle_2510730741();
                Idle.CenterData = CenterData;
                await Idle.Initialize(context);
                Idle.StateMachine = mStateMachine;
                ((DMC_AnimStateMachine_0_3144312222)mStateMachine).SetDefaultState(Idle);
                Dead = new DMC_Dead_3360116841();
                Dead.CenterData = CenterData;
                await Dead.Initialize(context);
                Dead.StateMachine = mStateMachine;
                Transition_From_AnimCompoundState_0_To_Idle = new DMC_Transition_From_AnimCompoundState_0_To_Idle_3200381843();
                Transition_From_AnimCompoundState_0_To_Idle.CenterData = CenterData;
                Transition_From_AnimCompoundState_0_To_Idle.From = this;
                Transition_From_AnimCompoundState_0_To_Idle.To = Idle;
                this.AddTransition(Transition_From_AnimCompoundState_0_To_Idle);
                DMC_Transition_From_Idle_To_Dead_2672857839 Transition_From_Idle_To_Dead = new DMC_Transition_From_Idle_To_Dead_2672857839();
                Transition_From_Idle_To_Dead.CenterData = CenterData;
                Transition_From_Idle_To_Dead.From = Idle;
                Transition_From_Idle_To_Dead.To = Dead;
                Idle.AddTransition(Transition_From_Idle_To_Dead);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimStateMachine_0_3144312222 : EngineNS.Animation.StateMachine.TtAnimStateMachine<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_AnimCompoundState_0_2028502536 AnimCompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                await base.Initialize(context);
                AnimCompoundState_0 = new DMC_AnimCompoundState_0_2028502536();
                AnimCompoundState_0.StateMachine = this;
                AnimCompoundState_0.CenterData = CenterData;
                await AnimCompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_PoseOutput_4214894380 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<animdmc_dragonthesouleater>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_BlendTree_StateMachine_2511051737 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_AnimStateMachine<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_AnimStateMachine_0_3144312222 AnimStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_FinalBlendTree_0_2686687017 : EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<animdmc_dragonthesouleater>
    {
        [EngineNS.Rtti.Meta]
        public DMC_PoseOutput_4214894380 PoseOutput { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_BlendTree_StateMachine_2511051737 BlendTree_StateMachine { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                PoseOutput = new DMC_PoseOutput_4214894380();
                PoseOutput.CenterData = CenterData;
                await PoseOutput.Initialize(context);
                BlendTree_StateMachine = new DMC_BlendTree_StateMachine_2511051737();
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
namespace NS_survivor.monsters.dragon1
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class animdmc_dragonthesouleater : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        //This is a Variable
        [EngineNS.Rtti.Meta]
        public System.Boolean IsDead { get; set; } = false;
        [EngineNS.Rtti.Meta]
        public DMC_AnimStateMachine_0_3144312222 AnimStateMachine_0 { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_FinalBlendTree_0_2686687017 FinalBlendTree_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_79843837 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
        public override void Tick(System.Single elapseSecond)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Tick_79843837))
            {
                mFrame_Tick_79843837.SetWatchVariable("elapseSecond", elapseSecond);
                Survivor.TtMonsterNode result_Cast_2791454737;
                if ((this.MacrossNode.Parent.Parent.Parent is Survivor.TtMonsterNode))
                {
                    result_Cast_2791454737 = ((Survivor.TtMonsterNode)this.MacrossNode.Parent.Parent.Parent);
                    IsDead = result_Cast_2791454737.StateNode.IsDead;
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
                AnimStateMachine_0 = new DMC_AnimStateMachine_0_3144312222();
                AnimStateMachine_0.CenterData = this;
                await AnimStateMachine_0.Initialize(animStateMachineContextAnimStateMachine_0);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseFinalBlendTree_0 = null;
                animatableSkeletonPoseFinalBlendTree_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextFinalBlendTree_0.AnimatableSkeletonPose = animatableSkeletonPoseFinalBlendTree_0;
                FinalBlendTree_0 = new DMC_FinalBlendTree_0_2686687017();
                FinalBlendTree_0.CenterData = this;
                await FinalBlendTree_0.Initialize(blendTreeContextFinalBlendTree_0);
                EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose runtimePose = null;
                runtimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(MacrossNode);
                FinalBlendTree_0.SetPose(runtimePose);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/dragon1/animdmc_dragonthesouleater.designmacross", EngineNS.RName.ERNameType.Game));
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
