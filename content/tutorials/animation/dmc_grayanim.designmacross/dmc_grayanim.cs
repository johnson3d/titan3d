namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_ClipPlay_2864315307 : EngineNS.Animation.StateMachine.TtClipPlayStateAttachment<dmc_grayanim>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimationClipName = EngineNS.RName.ParseFrom("tutorials/animation/thirdpersonrun.animclip:Game");
                IsLoop = true;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimSubState_172329120 : EngineNS.Animation.StateMachine.TtAnimState<dmc_grayanim>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("ClipPlay")]
        public DMC_ClipPlay_2864315307 ClipPlay { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                ClipPlay = new DMC_ClipPlay_2864315307();
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
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_1061413898 : EngineNS.Animation.StateMachine.TtAnimStateTransition<dmc_grayanim>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2797600863 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimCompoundState_0_2987649092 : EngineNS.Animation.StateMachine.TtAnimCompoundState<dmc_grayanim>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimSubState")]
        public DMC_AnimSubState_172329120 AnimSubState { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Transition_From_AnimCompoundState_0_To_AnimSubState")]
        public DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_1061413898 Transition_From_AnimCompoundState_0_To_AnimSubState { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                AnimSubState = new DMC_AnimSubState_172329120();
                AnimSubState.CenterData = CenterData;
                await AnimSubState.Initialize(context);
                AnimSubState.StateMachine = mStateMachine;
                ((DMC_AnimStateMachine_0_4117440125)mStateMachine).SetDefaultState(AnimSubState);
                Transition_From_AnimCompoundState_0_To_AnimSubState = new DMC_Transition_From_AnimCompoundState_0_To_AnimSubState_1061413898();
                Transition_From_AnimCompoundState_0_To_AnimSubState.CenterData = CenterData;
                Transition_From_AnimCompoundState_0_To_AnimSubState.From = this;
                Transition_From_AnimCompoundState_0_To_AnimSubState.To = AnimSubState;
                this.AddTransition(Transition_From_AnimCompoundState_0_To_AnimSubState);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_AnimStateMachine_0_4117440125 : EngineNS.Animation.StateMachine.TtAnimStateMachine<dmc_grayanim>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimCompoundState_0")]
        public DMC_AnimCompoundState_0_2987649092 AnimCompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_2271966183 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.StateMachine.TtAnimStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_2271966183,mFrame_Initialize_2271966183))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_2271966183.SetWatchVariable("context", context);
                await base.Initialize(context);
                AnimCompoundState_0 = new DMC_AnimCompoundState_0_2987649092();
                AnimCompoundState_0.StateMachine = this;
                AnimCompoundState_0.CenterData = CenterData;
                await AnimCompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_PoseOutput_2965600974 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<dmc_grayanim>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_BlendTree_StateMachine_829722290 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_AnimStateMachine<dmc_grayanim>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimStateMachine_0")]
        public DMC_AnimStateMachine_0_4117440125 AnimStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_KawaiiPhysics_1383711289 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_KawaiiPhysics<dmc_grayanim>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_561274663,mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiChainSetup> TempChainSetups = new System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiChainSetup>();
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiChainSetup TempChainSetups_Item_2598714315 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiChainSetup();
                TempChainSetups_Item_2598714315.Name = "Twintail_L";
                EngineNS.Animation.LimbIndexInSkeleton RootBoneIndex_Instance_3772046834 = new EngineNS.Animation.LimbIndexInSkeleton();
                RootBoneIndex_Instance_3772046834.Name = "TwintailA_L";
                RootBoneIndex_Instance_3772046834.Index = 20;
                RootBoneIndex_Instance_3772046834.Skeleton = "tutorials/animation/graychan";
                TempChainSetups_Item_2598714315.RootBoneIndex = RootBoneIndex_Instance_3772046834;
                EngineNS.Animation.LimbIndexInSkeleton EndBoneIndex_Instance_809697957 = new EngineNS.Animation.LimbIndexInSkeleton();
                EndBoneIndex_Instance_809697957.Name = "TwintailE_L";
                EndBoneIndex_Instance_809697957.Index = 24;
                EndBoneIndex_Instance_809697957.Skeleton = "tutorials/animation/graychan";
                TempChainSetups_Item_2598714315.EndBoneIndex = EndBoneIndex_Instance_809697957;
                TempChainSetups_Item_2598714315.TailBoneLength = 0.1f;
                TempChainSetups_Item_2598714315.TailBoneAxis = EngineNS.Bricks.Animation.KawaiiPhysics.EKawaiiTailBoneAxis.X_Positive;
                TempChainSetups_Item_2598714315.ConstrainBoneLength = true;
                TempChainSetups_Item_2598714315.BoneLengthConstraintBlend = 1f;
                TempChainSetups_Item_2598714315.RootCollision = false;
                TempChainSetups_Item_2598714315.LODThreshold = -1;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings PhysicsSettings_Instance_322182137 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings();
                PhysicsSettings_Instance_322182137.Stiffness = 0.1f;
                PhysicsSettings_Instance_322182137.Damping = 0.1f;
                PhysicsSettings_Instance_322182137.WorldDampingLocation = 0.8f;
                PhysicsSettings_Instance_322182137.WorldDampingRotation = 0.8f;
                PhysicsSettings_Instance_322182137.LimitAngle = 0f;
                PhysicsSettings_Instance_322182137.Radius = 3f;
                PhysicsSettings_Instance_322182137.WindCoefficient = 1f;
                PhysicsSettings_Instance_322182137.DragCoefficient = 0f;
                PhysicsSettings_Instance_322182137.MaxFrameDisplacement = 0f;
                TempChainSetups_Item_2598714315.PhysicsSettings = PhysicsSettings_Instance_322182137;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings PhysicsSettingsRandom_Instance_2021831582 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings();
                PhysicsSettingsRandom_Instance_2021831582.Stiffness = 0f;
                PhysicsSettingsRandom_Instance_2021831582.Damping = 0f;
                PhysicsSettingsRandom_Instance_2021831582.WorldDampingLocation = 0f;
                PhysicsSettingsRandom_Instance_2021831582.WorldDampingRotation = 0f;
                PhysicsSettingsRandom_Instance_2021831582.LimitAngle = 0f;
                PhysicsSettingsRandom_Instance_2021831582.Radius = 0f;
                PhysicsSettingsRandom_Instance_2021831582.WindCoefficient = 0f;
                PhysicsSettingsRandom_Instance_2021831582.DragCoefficient = 0f;
                PhysicsSettingsRandom_Instance_2021831582.MaxFrameDisplacement = 0f;
                TempChainSetups_Item_2598714315.PhysicsSettingsRandom = PhysicsSettingsRandom_Instance_2021831582;
                TempChainSetups_Item_2598714315.PhysicsCurveMode = EngineNS.Bricks.Animation.KawaiiPhysics.EKawaiiCurveEvalMode.LengthRate;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve StiffnessCurve_Instance_193304770 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                StiffnessCurve_Instance_193304770.KeysData = "";
                StiffnessCurve_Instance_193304770.ViewYMin = 0f;
                StiffnessCurve_Instance_193304770.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.StiffnessCurve = StiffnessCurve_Instance_193304770;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve DampingCurve_Instance_295473168 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                DampingCurve_Instance_295473168.KeysData = "";
                DampingCurve_Instance_295473168.ViewYMin = 0f;
                DampingCurve_Instance_295473168.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.DampingCurve = DampingCurve_Instance_295473168;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve WorldDampingLocationCurve_Instance_1886110736 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                WorldDampingLocationCurve_Instance_1886110736.KeysData = "";
                WorldDampingLocationCurve_Instance_1886110736.ViewYMin = 0f;
                WorldDampingLocationCurve_Instance_1886110736.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.WorldDampingLocationCurve = WorldDampingLocationCurve_Instance_1886110736;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve WorldDampingRotationCurve_Instance_3685677104 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                WorldDampingRotationCurve_Instance_3685677104.KeysData = "";
                WorldDampingRotationCurve_Instance_3685677104.ViewYMin = 0f;
                WorldDampingRotationCurve_Instance_3685677104.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.WorldDampingRotationCurve = WorldDampingRotationCurve_Instance_3685677104;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve LimitAngleCurve_Instance_2882001220 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                LimitAngleCurve_Instance_2882001220.KeysData = "";
                LimitAngleCurve_Instance_2882001220.ViewYMin = 0f;
                LimitAngleCurve_Instance_2882001220.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.LimitAngleCurve = LimitAngleCurve_Instance_2882001220;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve RadiusCurve_Instance_2105010162 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                RadiusCurve_Instance_2105010162.KeysData = "";
                RadiusCurve_Instance_2105010162.ViewYMin = 0f;
                RadiusCurve_Instance_2105010162.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.RadiusCurve = RadiusCurve_Instance_2105010162;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve DragCurve_Instance_2855536007 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                DragCurve_Instance_2855536007.KeysData = "";
                DragCurve_Instance_2855536007.ViewYMin = 0f;
                DragCurve_Instance_2855536007.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.DragCurve = DragCurve_Instance_2855536007;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve WindCurve_Instance_790102878 = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();
                WindCurve_Instance_790102878.KeysData = "";
                WindCurve_Instance_790102878.ViewYMin = 0f;
                WindCurve_Instance_790102878.ViewYMax = 0f;
                TempChainSetups_Item_2598714315.WindCurve = WindCurve_Instance_790102878;
                TempChainSetups.Add(TempChainSetups_Item_2598714315);
                System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiClothSetup> TempClothSetups = new System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiClothSetup>();
                System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiRodSetup> TempRodSetups = new System.Collections.Generic.List<EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiRodSetup>();
                ChainSetups = TempChainSetups;
                ClothSetups = TempClothSetups;
                RodSetups = TempRodSetups;
                KawaiiComponent.ApplyComponentPhysicsSettings = false;
                KawaiiComponent.EnableComponentMovementPhysics = true;
                KawaiiComponent.Gravity = new EngineNS.Vector3(0f, -9.8f, 0f);
                KawaiiComponent.GravityScale = 1f;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings TempComponentPhysicsSettings = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings();
                TempComponentPhysicsSettings.Stiffness = 0.4f;
                TempComponentPhysicsSettings.Damping = 0.1f;
                TempComponentPhysicsSettings.WorldDampingLocation = 0.8f;
                TempComponentPhysicsSettings.WorldDampingRotation = 0.8f;
                TempComponentPhysicsSettings.LimitAngle = 0f;
                TempComponentPhysicsSettings.Radius = 3f;
                TempComponentPhysicsSettings.WindCoefficient = 1f;
                TempComponentPhysicsSettings.DragCoefficient = 0f;
                TempComponentPhysicsSettings.MaxFrameDisplacement = 0f;
                KawaiiComponent.PhysicsSettings = TempComponentPhysicsSettings;
                EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings TempComponentPhysicsSettingsRandom = new EngineNS.Bricks.Animation.KawaiiPhysics.TtKawaiiPhySettings();
                TempComponentPhysicsSettingsRandom.Stiffness = 0f;
                TempComponentPhysicsSettingsRandom.Damping = 0f;
                TempComponentPhysicsSettingsRandom.WorldDampingLocation = 0f;
                TempComponentPhysicsSettingsRandom.WorldDampingRotation = 0f;
                TempComponentPhysicsSettingsRandom.LimitAngle = 0f;
                TempComponentPhysicsSettingsRandom.Radius = 0f;
                TempComponentPhysicsSettingsRandom.WindCoefficient = 0f;
                TempComponentPhysicsSettingsRandom.DragCoefficient = 0f;
                TempComponentPhysicsSettingsRandom.MaxFrameDisplacement = 0f;
                KawaiiComponent.PhysicsSettingsRandom = TempComponentPhysicsSettingsRandom;
                CommandDesc.Alpha = 1f;
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Tick_877346851 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Tick_877346851 = new EngineNS.Macross.TtMacrossStackTracer();
        public override void Tick(System.Single elapseSecond,ref EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Tick = new EngineNS.Macross.TtMacrossStackGuard(mStack_Tick_877346851,mFrame_Tick_877346851))
            {
                mFrame_Tick_877346851.SetWatchVariable("elapseSecond", elapseSecond);
                mFrame_Tick_877346851.SetWatchVariable("context", context);
                base.Tick(elapseSecond,ref context);
                FromNode.Tick(elapseSecond,ref context);
            }
        }
    }
}
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_FinalBlendTree_0_1827970541 : EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<dmc_grayanim>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("PoseOutput")]
        public DMC_PoseOutput_2965600974 PoseOutput { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("BlendTree_StateMachine")]
        public DMC_BlendTree_StateMachine_829722290 BlendTree_StateMachine { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("KawaiiPhysics")]
        public DMC_KawaiiPhysics_1383711289 KawaiiPhysics { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackTracer();
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mStack_Initialize_561274663,mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                PoseOutput = new DMC_PoseOutput_2965600974();
                PoseOutput.CenterData = CenterData;
                await PoseOutput.Initialize(context);
                BlendTree_StateMachine = new DMC_BlendTree_StateMachine_829722290();
                BlendTree_StateMachine.CenterData = CenterData;
                await BlendTree_StateMachine.Initialize(context);
                KawaiiPhysics = new DMC_KawaiiPhysics_1383711289();
                KawaiiPhysics.CenterData = CenterData;
                await KawaiiPhysics.Initialize(context);
                PoseOutput.FromNode = KawaiiPhysics;
                KawaiiPhysics.FromNode = BlendTree_StateMachine;
                FromNode = PoseOutput;
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.animation
{
    [EngineNS.Rtti.Meta]
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/animation/dmc_grayanim.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class dmc_grayanim : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        //This is a Variable
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("New_Var_0")]
        public System.Boolean New_Var_0 { get; set; } = false;
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("AnimStateMachine_0")]
        public DMC_AnimStateMachine_0_4117440125 AnimStateMachine_0 { get; set; }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("FinalBlendTree_0")]
        public DMC_FinalBlendTree_0_1827970541 FinalBlendTree_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
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
                AnimStateMachine_0 = new DMC_AnimStateMachine_0_4117440125();
                AnimStateMachine_0.CenterData = this;
                await AnimStateMachine_0.Initialize(animStateMachineContextAnimStateMachine_0);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseFinalBlendTree_0 = null;
                animatableSkeletonPoseFinalBlendTree_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextFinalBlendTree_0.AnimatableSkeletonPose = animatableSkeletonPoseFinalBlendTree_0;
                FinalBlendTree_0 = new DMC_FinalBlendTree_0_1827970541();
                FinalBlendTree_0.CenterData = this;
                await FinalBlendTree_0.Initialize(blendTreeContextFinalBlendTree_0);
                EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose runtimePose = null;
                runtimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(MacrossNode);
                FinalBlendTree_0.SetPose(runtimePose);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/animation/dmc_grayanim.designmacross", EngineNS.RName.ERNameType.Game));
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
