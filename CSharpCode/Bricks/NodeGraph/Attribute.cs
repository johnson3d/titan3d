using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class ExpandableAttribute : Attribute
    {
        public string[] KeyStrings;

        public ExpandableAttribute(params string[] keyStrings)
        {
            KeyStrings = keyStrings;
        }
        public bool HasKeyString(string keyString)
        {
            for(int i=0; i < KeyStrings.Length; i++)
            {
                if(KeyStrings[i] == keyString)
                    return true;
            }
            return false;
        }
    }
}
