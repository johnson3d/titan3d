using EngineNS.Bricks.NodeGraph;
using EngineNS.NxRHI;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtDesignGraphUtil
    {
        public static Vector2 CalculateAbsLocation(IGraphElement element)
        {
            if (element.Parent != null)
            {
                return element.Location + CalculateAbsLocation(element.Parent);
            }
            return element.Location;
        }
    }
}
