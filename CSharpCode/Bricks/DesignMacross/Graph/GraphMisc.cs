using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Graph
{
    public class TtGraphMisc
    {
        public static Vector2 CalculateAbsLocation(IGraphElement element)
        {
            if(element.Parent != null)
            {
               return element.Location + CalculateAbsLocation(element.Parent);
            }
            return element.Location;
        }

    }
}
