using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class TtStackBasedState<T> : IState<T>
    {
        public TtStackBasedState(TtStackBasedStateMachine<T> stateMachine, string name = "State") 
        {
        }
        public string Name { get; set; }

        public bool AddAttachment(IAttachmentRule<T> attachment)
        {
            return true;
        }
        public bool RemoveAttachment(IAttachmentRule<T> attachment)
        {
            return false;
        }
        
        public bool AddTransition(ITransition<T> transition)
        {
            return false;
        }
        public bool RemoveTransition(ITransition<T> transition)
        {
            return false;
        }

        public void Enter()
        {
        }

        public void Exit()
        {
        }

        public void Initialize()
        {
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

        public bool TryCheckTransitions(out List<ITransition<T>> transitions)
        {
            throw new NotImplementedException();
        }
    }
}
