using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class TtStackBasedState<S, T> : IState<S, T>
    {
        public S CenterData { get; set; }
        public TtStackBasedState(TtStackBasedStateMachine<S, T> stateMachine, string name = "State") 
        {
        }
        public string Name { get; set; }

        public bool AddAttachment(IAttachmentRule<S, T> attachment)
        {
            return true;
        }
        public bool RemoveAttachment(IAttachmentRule<S, T> attachment)
        {
            return false;
        }
        
        public bool AddTransition(ITransition<S, T> transition)
        {
            return false;
        }
        public bool RemoveTransition(ITransition<S, T> transition)
        {
            return false;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public bool Initialize()
        {
            return false;
        }   

        public bool ShouldUpdate()
        {
            return false;
        }

        public void Tick(float elapseSecond, in T context)
        {
        }

        public void Update(float elapseSecond, in T context)
        {
        }

        public bool TryCheckTransitions(in T context, out List<ITransition<S, T>> transitions)
        {
            throw new NotImplementedException();
        }
    }
}
