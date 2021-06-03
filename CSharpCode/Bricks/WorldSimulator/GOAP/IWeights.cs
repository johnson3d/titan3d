using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.WorldSimulator.GOAP
{
    public class IWeights : IComparable<IWeights>
    {
        public float GetWeights()
        {
            return 1;
        }
        public int CompareTo(IWeights other)
        {
            return GetWeights().CompareTo(other.GetWeights());
        }
    }
}
