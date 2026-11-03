namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimSubState_To_Dead_293878152 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_belica>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackTracer();
        public override System.Boolean CheckCondition(in EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mStack_CheckCondition_2797600863,mFrame_CheckCondition_2797600863))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2797600863.SetWatchVariable("context", context);
                returnedValue = CenterData.IsDead;
                return returnedValue;
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_BlendSpacePlay_1996511248 : EngineNS.Animation.StateMachine.TtBlendSpacePlayStateAttachment<animdmc_belica>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationName = EngineNS.RName.ParseFrom("project_factory/belica/locomotion.blendspace2d:Game");
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_2613193649 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Tick_2613193649 = new EngineNS.Macross.TtMacrossStackTracer();
        public override void Tick(System.Single elapseSecond,in EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mStack_Tick_2613193649,mFrame_Tick_2613193649))
            {
                mFrame_Tick_2613193649.SetWatchVariable("elapseSecond", elapseSecond);
                mFrame_Tick_2613193649.SetWatchVariable("context", context);
                Input.Y = CenterData.MoveSpeed;
                base.Tick(elapseSecond,in context);
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimSubState_1500684542 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("BlendSpacePlay")]
        public DMC_BlendSpacePlay_1996511248 BlendSpacePlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                BlendSpacePlay = new DMC_BlendSpacePlay_1996511248();
                BlendSpacePlay.CenterData = CenterData;
                await BlendSpacePlay.Initialize(context);
                this.AddAttachment(BlendSpacePlay);
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_1254788028 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<animdmc_belica>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("project_factory/belica/animations/death_a.animclip:Game");
                IsLoop = false;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Dead_1111980249 : EngineNS.Animation.StateMachine.TtAnimState<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("ClipPlay")]
        public DMC_ClipPlay_1254788028 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_1254788028();
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
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_216445893 : EngineNS.Animation.StateMachine.TtAnimStateTransition<animdmc_belica>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackTracer();
        public override System.Boolean CheckCondition(in EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mStack_CheckCondition_2797600863,mFrame_CheckCondition_2797600863))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2797600863.SetWatchVariable("context", context);
                return returnedValue;
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimCompoundState_0_1422827071 : EngineNS.Animation.StateMachine.TtAnimCompoundState<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimSubState")]
        public DMC_AnimSubState_1500684542 AnimSubState { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Dead")]
        public DMC_Dead_1111980249 Dead { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Transition_From_AnimCompoundState_0_To_AnimSubState")]
        public DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_216445893 Transition_From_AnimCompoundState_0_To_AnimSubState { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimSubState = new DMC_AnimSubState_1500684542();
                AnimSubState.CenterData = CenterData;
                await AnimSubState.Initialize(context);
                AnimSubState.StateMachine = mStateMachine;
                ((DMC_AnimStateMachine_0_228312668)mStateMachine).SetDefaultState(AnimSubState);
                Dead = new DMC_Dead_1111980249();
                Dead.CenterData = CenterData;
                await Dead.Initialize(context);
                Dead.StateMachine = mStateMachine;
                Transition_From_AnimCompoundState_0_To_AnimSubState = new DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_216445893();
                Transition_From_AnimCompoundState_0_To_AnimSubState.CenterData = CenterData;
                Transition_From_AnimCompoundState_0_To_AnimSubState.From = this;
                Transition_From_AnimCompoundState_0_To_AnimSubState.To = AnimSubState;
                this.AddTransition(Transition_From_AnimCompoundState_0_To_AnimSubState);
                DMC_Transition_From_AnimSubState_To_Dead_293878152 Transition_From_AnimSubState_To_Dead = new DMC_Transition_From_AnimSubState_To_Dead_293878152();
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
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimStateMachine_0_228312668 : EngineNS.Animation.StateMachine.TtAnimStateMachine<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimCompoundState_0")]
        public DMC_AnimCompoundState_0_1422827071 AnimCompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                await base.Initialize(context);
                AnimCompoundState_0 = new DMC_AnimCompoundState_0_1422827071();
                AnimCompoundState_0.StateMachine = this;
                AnimCompoundState_0.CenterData = CenterData;
                await AnimCompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_PoseOutput_3069185515 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<animdmc_belica>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_561274663,mFrame_Initialize_561274663))
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
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_BlendTree_StateMachine_1318257668 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_AnimStateMachine<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimStateMachine_0")]
        public DMC_AnimStateMachine_0_228312668 AnimStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_561274663,mFrame_Initialize_561274663))
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
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_FinalBlendTree_0_1557498772 : EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<animdmc_belica>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("PoseOutput")]
        public DMC_PoseOutput_3069185515 PoseOutput { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("BlendTree_StateMachine")]
        public DMC_BlendTree_StateMachine_1318257668 BlendTree_StateMachine { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_561274663,mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                PoseOutput = new DMC_PoseOutput_3069185515();
                PoseOutput.CenterData = CenterData;
                await PoseOutput.Initialize(context);
                BlendTree_StateMachine = new DMC_BlendTree_StateMachine_1318257668();
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
namespace NS_project_factory.belica
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "project_factory/belica/animdmc_belica.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class animdmc_belica : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        //This is a Variable
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("MoveSpeed")]
        public System.Single MoveSpeed { get; set; } = 0f;
        //This is a Variable
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("IsDead")]
        public System.Boolean IsDead { get; set; } = false;
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimStateMachine_0")]
        public DMC_AnimStateMachine_0_228312668 AnimStateMachine_0 { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("FinalBlendTree_0")]
        public DMC_FinalBlendTree_0_1557498772 FinalBlendTree_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_79843837 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Tick_79843837 = new EngineNS.Macross.TtMacrossStackTracer();
        public override void Tick(System.Single elapseSecond)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mStack_Tick_79843837,mFrame_Tick_79843837))
            {
                mFrame_Tick_79843837.SetWatchVariable("elapseSecond", elapseSecond);
                EngineNS.GamePlay.Scene.TtNode result_FindFirstChild_4157295633 = default(EngineNS.GamePlay.Scene.TtNode);
                result_FindFirstChild_4157295633 = this.MacrossNode.Parent.Parent.FindFirstChild("Movement");
                EngineNS.GamePlay.Movemnet.TtMovement result_Cast_3930248843;
                if ((result_FindFirstChild_4157295633 is EngineNS.GamePlay.Movemnet.TtMovement))
                {
                    result_Cast_3930248843 = ((EngineNS.GamePlay.Movemnet.TtMovement)result_FindFirstChild_4157295633);
                    System.Single result_Length_1938986086 = default(System.Single);
                    result_Length_1938986086 = result_Cast_3930248843.LinearVelocity.Length();
                    this.MoveSpeed = result_Length_1938986086;
                    EngineNS.GamePlay.Scene.TtNode result_FindFirstChild_4081583513 = default(EngineNS.GamePlay.Scene.TtNode);
                    result_FindFirstChild_4081583513 = this.MacrossNode.Parent.Parent.FindFirstChild("StateNode");
                    Survivor.TtStateNode result_Cast_3905671104;
                    if ((result_FindFirstChild_4081583513 is Survivor.TtStateNode))
                    {
                        result_Cast_3905671104 = ((Survivor.TtStateNode)result_FindFirstChild_4081583513);
                        IsDead = result_Cast_3905671104.IsDead;
                    }
                }
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize()
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_3062514605,mFrame_Initialize_3062514605))
            {
                System.Boolean returnedValue = default(System.Boolean);
                EngineNS.Animation.StateMachine.TtAnimStateMachineContext animStateMachineContextAnimStateMachine_0 = new EngineNS.Animation.StateMachine.TtAnimStateMachineContext();
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextAnimStateMachine_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseAnimStateMachine_0 = null;
                animatableSkeletonPoseAnimStateMachine_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextAnimStateMachine_0.AnimatableSkeletonPose = animatableSkeletonPoseAnimStateMachine_0;
                animStateMachineContextAnimStateMachine_0.BlendTreeContext = blendTreeContextAnimStateMachine_0;
                AnimStateMachine_0 = new DMC_AnimStateMachine_0_228312668();
                AnimStateMachine_0.CenterData = this;
                await AnimStateMachine_0.Initialize(animStateMachineContextAnimStateMachine_0);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseFinalBlendTree_0 = null;
                animatableSkeletonPoseFinalBlendTree_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextFinalBlendTree_0.AnimatableSkeletonPose = animatableSkeletonPoseFinalBlendTree_0;
                FinalBlendTree_0 = new DMC_FinalBlendTree_0_1557498772();
                FinalBlendTree_0.CenterData = this;
                await FinalBlendTree_0.Initialize(blendTreeContextFinalBlendTree_0);
                EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose runtimePose = null;
                runtimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(MacrossNode);
                FinalBlendTree_0.SetPose(runtimePose);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("project_factory/belica/animdmc_belica.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackTracer();
        public override void AfterTick(System.Single elapseSecond)
        {
            using(var guard_AfterTick = new EngineNS.Macross.TtMacrossStackGuard(mStack_AfterTick_4051512107,mFrame_AfterTick_4051512107))
            {
                mFrame_AfterTick_4051512107.SetWatchVariable("elapseSecond", elapseSecond);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                FinalBlendTree_0.Tick(elapseSecond,ref blendTreeContextFinalBlendTree_0);
            }
        }
    }
}
