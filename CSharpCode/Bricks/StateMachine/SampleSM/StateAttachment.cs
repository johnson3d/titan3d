using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.StateMachine.SampleSM
{
    public class TtStateAttachment<T> : IAttachment<T>
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

        public void Tick(float elapseSecond, in T context)
        {
            if(!ShouldUpdate())
                return;
            Update(elapseSecond, context);
        }

        public void Update(float elapseSecond, in T context)
        {
           
        }
    }
}
