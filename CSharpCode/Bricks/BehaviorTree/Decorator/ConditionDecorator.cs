using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Decorator
{
    public class ConditionDecorator : DecoratorBehavior
    {
        public bool Inverse = false;
    }
}
