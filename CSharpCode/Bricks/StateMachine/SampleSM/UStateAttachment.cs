using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class UStateAttachment : IAttachment
    {
        public string Name { get; set; } = "Attachment";

        public bool CheckCondition()
        {
            return true;
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
            return true;
        }

        public void Tick(float elapseSecond)
        {
            if(!ShouldUpdate())
                return;
            Update(elapseSecond);
        }

        public void Update(float elapseSecond)
        {
           
        }
    }
}
