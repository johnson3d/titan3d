namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Transition_From_Up_To_Down_267312077 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<smtest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2304976243 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean CheckCondition(in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mFrame_CheckCondition_2304976243))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2304976243.SetWatchVariable("context", context);
                EngineNS.Vector3 result_ToSingleVector3_3552239884 = default(EngineNS.Vector3);
                result_ToSingleVector3_3552239884 = CenterData.MacrossNode.Parent.Placement.Position.ToSingleVector3();
                returnedValue = (result_ToSingleVector3_3552239884.y > 2f);
                return returnedValue;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Script_934227224 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateAttachment<smtest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Update_3355887419 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override void Update(System.Single elapseSecond,in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Update = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Update_3355887419))
            {
                mFrame_Update_3355887419.SetWatchVariable("elapseSecond", elapseSecond);
                mFrame_Update_3355887419.SetWatchVariable("context", context);
                EngineNS.Vector3 result_CreateVector3f_1867382767 = default(EngineNS.Vector3);
                result_CreateVector3f_1867382767 = EngineNS.MathHelper.CreateVector3f(0f,0.1f,0f);
                EngineNS.DVector3 result_AsDVector_528613241 = default(EngineNS.DVector3);
                result_AsDVector_528613241 = result_CreateVector3f_1867382767.AsDVector();
                CenterData.MacrossNode.Parent.Placement.Position = (CenterData.MacrossNode.Parent.Placement.Position + result_AsDVector_528613241);
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Up_3275865331 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedState<smtest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_Up_To_Down_267312077 Transition_From_Up_To_Down { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Script_934227224 Script { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                Script = new DMC_Script_934227224();
                Script.CenterData = CenterData;
                await Script.Initialize(context);
                this.AddAttachment(Script);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Transition_From_Down_To_Up_2059917023 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<smtest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2304976243 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override System.Boolean CheckCondition(in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_CheckCondition = new EngineNS.Macross.TtMacrossStackGuard(mFrame_CheckCondition_2304976243))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_CheckCondition_2304976243.SetWatchVariable("context", context);
                EngineNS.Vector3 result_ToSingleVector3_1023095188 = default(EngineNS.Vector3);
                result_ToSingleVector3_1023095188 = CenterData.MacrossNode.Parent.Placement.Position.ToSingleVector3();
                returnedValue = (result_ToSingleVector3_1023095188.y < 0f);
                return returnedValue;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Script_3879801902 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateAttachment<smtest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Update_3355887419 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override void Update(System.Single elapseSecond,in EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Update = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Update_3355887419))
            {
                mFrame_Update_3355887419.SetWatchVariable("elapseSecond", elapseSecond);
                mFrame_Update_3355887419.SetWatchVariable("context", context);
                EngineNS.Vector3 result_CreateVector3f_1601524131 = default(EngineNS.Vector3);
                result_CreateVector3f_1601524131 = EngineNS.MathHelper.CreateVector3f(0f,-0.1f,0f);
                EngineNS.DVector3 result_AsDVector_2767448039 = default(EngineNS.DVector3);
                result_AsDVector_2767448039 = result_CreateVector3f_1601524131.AsDVector();
                CenterData.MacrossNode.Parent.Placement.Position = (CenterData.MacrossNode.Parent.Placement.Position + result_AsDVector_2767448039);
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Down_294180657 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedState<smtest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_Down_To_Up_2059917023 Transition_From_Down_To_Up { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Script_3879801902 Script { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                Script = new DMC_Script_3879801902();
                Script.CenterData = CenterData;
                await Script.Initialize(context);
                this.AddAttachment(Script);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_Transition_From_CompoundState_0_To_Up_2649117070 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateTransition<smtest>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_CheckCondition_2304976243 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
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
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_CompoundState_0_2567573323 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedCompoundState<smtest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_Up_3275865331 Up { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Down_294180657 Down { get; set; }
        [EngineNS.Rtti.Meta]
        public DMC_Transition_From_CompoundState_0_To_Up_2649117070 Transition_From_CompoundState_0_To_Up { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                Up = new DMC_Up_3275865331();
                Up.CenterData = CenterData;
                await Up.Initialize(context);
                Up.StateMachine = mStateMachine;
                ((DMC_TimedStateMachine_0_3138747184)mStateMachine).SetDefaultState(Up);
                Down = new DMC_Down_294180657();
                Down.CenterData = CenterData;
                await Down.Initialize(context);
                Down.StateMachine = mStateMachine;
                Transition_From_CompoundState_0_To_Up = new DMC_Transition_From_CompoundState_0_To_Up_2649117070();
                Transition_From_CompoundState_0_To_Up.CenterData = CenterData;
                Transition_From_CompoundState_0_To_Up.From = this;
                Transition_From_CompoundState_0_To_Up.To = Up;
                this.AddTransition(Transition_From_CompoundState_0_To_Up);
                DMC_Transition_From_Up_To_Down_267312077 Transition_From_Up_To_Down = new DMC_Transition_From_Up_To_Down_267312077();
                Transition_From_Up_To_Down.CenterData = CenterData;
                Transition_From_Up_To_Down.From = Up;
                Transition_From_Up_To_Down.To = Down;
                Up.AddTransition(Transition_From_Up_To_Down);
                DMC_Transition_From_Down_To_Up_2059917023 Transition_From_Down_To_Up = new DMC_Transition_From_Down_To_Up_2059917023();
                Transition_From_Down_To_Up.CenterData = CenterData;
                Transition_From_Down_To_Up.From = Down;
                Transition_From_Down_To_Up.To = Up;
                Down.AddTransition(Transition_From_Down_To_Up);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class DMC_TimedStateMachine_0_3138747184 : EngineNS.Bricks.StateMachine.TimedSM.TtTimedStateMachine<smtest>
    {
        [EngineNS.Rtti.Meta]
        public DMC_CompoundState_0_2567573323 CompoundState_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_1869935397 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_1869935397))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_1869935397.SetWatchVariable("context", context);
                CompoundState_0 = new DMC_CompoundState_0_2567573323();
                CompoundState_0.StateMachine = this;
                CompoundState_0.CenterData = CenterData;
                await CompoundState_0.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_tutorials.statemachine
{
    [EngineNS.Macross.TtMacross]
    public partial class smtest : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        [EngineNS.Rtti.Meta]
        public DMC_TimedStateMachine_0_3138747184 TimedStateMachine_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize()
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_3062514605))
            {
                System.Boolean returnedValue = default(System.Boolean);
                EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext stateMachineContextTimedStateMachine_0 = new EngineNS.Bricks.StateMachine.TimedSM.TtStateMachineContext();
                TimedStateMachine_0 = new DMC_TimedStateMachine_0_3138747184();
                TimedStateMachine_0.CenterData = this;
                await TimedStateMachine_0.Initialize(stateMachineContextTimedStateMachine_0);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/statemachine/smtest.designmacross", EngineNS.RName.ERNameType.Game));
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
