namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Script_3017065879 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateAttachment<dmc_skill_bullet>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Update_3355887419 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override void Update(System.Single elapseSecond,in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Update = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Update_3355887419))
            {
                mFrame_Update_3355887419.SetWatchVariable("elapseSecond", elapseSecond);
                mFrame_Update_3355887419.SetWatchVariable("context", context);
                if ((CenterData.accumulateTime >= CenterData.bulletCooldown))
                {
                    CenterData.accumulateTime = 0f;
                }
                else
                {
                    CenterData.accumulateTime = (elapseSecond + CenterData.accumulateTime);
                }
            }
        }
    }
}
namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_TimedSubState_1470609185 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedState<dmc_skill_bullet>
    {
        [EngineNS.Rtti.Meta]
        public DMC_Script_3017065879 Script { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                Script = new DMC_Script_3017065879();
                Script.CenterData = CenterData;
                await Script.Initialize(context);
                this.AddAttachment(Script);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Transition_From_CompoundState_0_To_TimedSubState_2311555624 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<dmc_skill_bullet>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2304976243 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean CheckCondition(in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mFrame_CheckCondition_2304976243))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2304976243.SetWatchVariable("context", context);
                return returnedValue;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_CompoundState_0_3413297639 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedCompoundState<dmc_skill_bullet>
    {
        [EngineNS.Rtti.Meta]
        public DMC_TimedSubState_1470609185 TimedSubState { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_CompoundState_0_To_TimedSubState_2311555624 Transition_From_CompoundState_0_To_TimedSubState { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                TimedSubState = new DMC_TimedSubState_1470609185();
                TimedSubState.CenterData = CenterData;
                await TimedSubState.Initialize(context);
                TimedSubState.StateMachine = mStateMachine;
                Transition_From_CompoundState_0_To_TimedSubState = new DMC_Transition_From_CompoundState_0_To_TimedSubState_2311555624();
                Transition_From_CompoundState_0_To_TimedSubState.CenterData = CenterData;
                Transition_From_CompoundState_0_To_TimedSubState.From = this;
                Transition_From_CompoundState_0_To_TimedSubState.To = TimedSubState;
                this.AddTransition(Transition_From_CompoundState_0_To_TimedSubState);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_TimedStateMachine_0_1731219335 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateMachine<dmc_skill_bullet>
    {
        [EngineNS.Rtti.Meta]
        public DMC_CompoundState_0_3413297639 CompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                CompoundState_0 = new DMC_CompoundState_0_3413297639();
                CompoundState_0.StateMachine = this;
                CompoundState_0.CenterData = CenterData;
                await CompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    public partial class dmc_skill_bullet : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        //This is a Variable
        [EngineNS.Rtti.Meta]
        public System.Single bulletCooldown { get; set; } = 1f;
        //This is a Variable
        [EngineNS.Rtti.Meta]
        public System.Single accumulateTime { get; set; } = 0f;
        [EngineNS.Rtti.Meta]
        public DMC_TimedStateMachine_0_1731219335 TimedStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize()
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_3062514605))
            {
                System.Boolean returnedValue = default(System.Boolean);
                EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext stateMachineContextTimedStateMachine_0 = new EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext();
                TimedStateMachine_0 = new DMC_TimedStateMachine_0_1731219335();
                TimedStateMachine_0.CenterData = this;
                await TimedStateMachine_0.Initialize(stateMachineContextTimedStateMachine_0);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/dmc_skill_bullet.designmacross", EngineNS.RName.ERNameType.Game));
        public override void AfterTick(System.Single elapseSecond)
        {
            using(var guard_AfterTick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_AfterTick_4051512107))
            {
                mFrame_AfterTick_4051512107.SetWatchVariable("elapseSecond", elapseSecond);
                EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext stateMachineTimedStateMachine_0 = new EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext();
                TimedStateMachine_0.Tick(elapseSecond,in stateMachineTimedStateMachine_0);
            }
        }
    }
}
