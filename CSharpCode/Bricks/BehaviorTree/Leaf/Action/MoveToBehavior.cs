using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.BehaviorTree.Leaf.Action
{
    //[Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class MoveToBehavior : ActionBehavior
    {
        public Vector3 Target { get; set; } = Vector3.Zero;
        public Func<Vector3> TargetPositionEvaluateFunc { get; set; } = null;
        
    }
}
