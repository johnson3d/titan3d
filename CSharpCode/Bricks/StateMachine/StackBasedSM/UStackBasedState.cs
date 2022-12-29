using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.StackBasedSM
{
    public class StackBasedState : IState
    {
        public StackBasedState(UStackBasedStateMachine context, string name = "State") 
        {
        }
        public string Name { get; set; }

        public bool AddAttachment(IAttachmentRule attachment)
        {
            return true;
        }
        public bool RemoveAttachment(IAttachmentRule attachment)
        {
            return false;
        }
        
        public bool AddTransition(ITransition transition)
        {
            return false;
        }
        public bool RemoveTransition(ITransition transition)
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

        public void Tick(float elapseSecond)
        {
        }

        public void Update(float elapseSecond)
        {
        }
    }
}
