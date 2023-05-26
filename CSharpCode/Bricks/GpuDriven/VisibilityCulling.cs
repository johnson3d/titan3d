using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GpuDriven
{
    public class TtVisibilityCullingNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin BoundingInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Bounding");
        public Graphics.Pipeline.Common.URenderGraphPin VisibilityOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("Visibility", false, EPixelFormat.PXF_UNKNOWN);
        public Graphics.Pipeline.Common.URenderGraphPin DrawArgumentOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("DrawArg", false, EPixelFormat.PXF_UNKNOWN);
    }
    //nanite: https://zhuanlan.zhihu.com/p/382687738
    //barycentric: https://blog.csdn.net/qq_38065509/article/details/105446756
    //raster: http://richbabe.top/2018/06/22/Bresenham%E7%AE%97%E6%B3%95%E4%B8%8E%E4%B8%89%E8%A7%92%E5%BD%A2%E5%85%89%E6%A0%85%E5%8C%96/
    //raster: https://zhuanlan.zhihu.com/p/64006712
    public class TtVisibilityBuffer
    {
    }
}
