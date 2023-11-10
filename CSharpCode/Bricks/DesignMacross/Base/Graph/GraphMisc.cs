using EngineNS.DesignMacross.Base.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
{
    public interface IMouseEvent
    {

    }
   
    public struct FMargin
    {
        public static readonly FMargin Default = new FMargin(0, 0, 0, 0);
        public FMargin(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
    }


}
