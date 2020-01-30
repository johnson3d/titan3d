using EngineNS.Bricks.FSM.SFSM;
using EngineNS.GamePlay.Actor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Component.StateMachine
{
    [Rtti.MetaClass]
    public class GStateMachineComponentInitializer : GComponent.GComponentInitializer
    {
        public override bool OnlyForGame
        {
            get;
            set;
        } = true;
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GStateMachineComponentInitializer), "状态机组件", "StateMachine", "StateMachineComponent")]
    public class GStateMachineComponent : GComponent
    {
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McStateMachineComponent))]
        public override RName ComponentMacross
        {
            get
            {
                return base.ComponentMacross;
            }
            set
            {
                base.ComponentMacross = value;
                if (McStateMachineComponent != null)
                {
                    McStateMachineComponent.HostComp = this;
                   if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                    {
                        McStateMachineComponent?.Construct();
                    }
                }
            }
        }
        McStateMachineComponent McStateMachineComponent
        {
            get
            {
                return mMcCompGetter?.CastGet<McStateMachineComponent>(OnlyForGame);
            }
        }
        [Browsable(false)]
        public GStateMachineComponentInitializer StateMachineComponentInitializer
        {
            get
            {
                var v = Initializer as GStateMachineComponentInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }
        public GStateMachineComponent()
        {
            Initializer = new GStateMachineComponentInitializer();
        }
        public override Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            return base.SetInitializer(rc, host, hostContainer, v);
        }
        public override void Tick(GPlacementComponent placement)
        {
            McStateMachineComponent?.Tick(placement);
        }
        public override void OnActorLoaded(GActor actor)
        {
            base.OnActorLoaded(actor);
            McStateMachineComponent?.Construct();
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    public class McStateMachineComponent : Controller.McGAIController
    {
        public StackBasedFiniteStateMachine StackBasedFSM { get; set; } = new StackBasedFiniteStateMachine();
        public McStateMachineComponent()
        {
        }
        public override void OnNewMacross()
        {
   
        }
        public override void OnActorLoaded(GComponent comp)
        {
            base.OnActorLoaded(comp);
            //Construct();
        }
        public override void Construct()
        {
            ConstructLFSMGraph();
            ConstructLFSMFunc?.Invoke();
            Constructed();
        }
        public StackBasedFiniteStateMachine CrteateStateMachine(string name)
        {
            StackBasedFSM = new StackBasedFiniteStateMachine("LFSM_" + name);
            return StackBasedFSM;
        }
        public StackBasedState CreateState(string name)
        {
            var laState = new StackBasedState(StackBasedFSM, name);
            return laState;
        }
        public StateBasedStateTransitionFunction CreateTransitionFunction(StackBasedState from, StackBasedState to)
        {
            var lat = new StateBasedStateTransitionFunction();
            lat.From = from;
            lat.To = to;
            return lat;
        }
        public void SetDefaultState(StackBasedState state, string stateMachineName)
        {
            StackBasedFSM.SetCurrentStateImmediately(state);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void Constructed()
        {
        }
        public virtual void ConstructLFSMGraph()
        {
        }
        public Action ConstructLFSMFunc { get; set; } = null;
        public override void Tick(GPlacementComponent placement)
        {
            OnTick(HostComp);
            StackBasedFSM?.Tick();
        }
    }
}
