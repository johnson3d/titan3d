using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    
    public class AnimatedClassDesc
    {
        public Rtti.UTypeDesc ClassType;
        public string ObjectTypeStr { get; set; }
        public string NodeName { get; set; }
        public List<AnimatedPropertyDesc> Properties { get; set; }
    }
    public class AnimatedPropertyDesc
    {
        public string PropertyName { get; set; }
        public int CurveIndex { get; set; }
    }

    public class AnimatedTreeNode 
    {
        public AnimatedClassDesc Current { get; set; }

        public AnimatedTreeNode Root { get; set; }

        public AnimatedTreeNode Parent { get; set; }
        public List<AnimatedTreeNode> Children { get; set; }
    }
}
