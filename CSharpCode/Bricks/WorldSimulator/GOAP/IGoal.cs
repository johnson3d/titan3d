using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public class IGoal
    {
        //下列行为可以达成目标，事先做好消耗排序
        public List<IAction> Actions { get; } = new List<IAction>();
    }
    public class IArrivedGoal : IGoal
    {
        public Vector3 TargetPosition { get; set; }
    }
}
