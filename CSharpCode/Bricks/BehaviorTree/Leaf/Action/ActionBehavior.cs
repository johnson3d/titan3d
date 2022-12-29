using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Leaf.Action
{
    ////[Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ActionBehavior : LeafBehavior
    {
        public Func<long, GamePlay.UCenterData, BehaviorStatus> Func { get; set; } = null;
        public override BehaviorStatus Update(long timeElapse, GamePlay.UCenterData context)        {            if (Func == null)                return BehaviorStatus.Failure;            return Func.Invoke(timeElapse, context);
        }
    }
}
