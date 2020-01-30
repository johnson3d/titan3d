using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Curve
{
    public class CGfxFloatCurve : CGfxICurve
    {
        public CGfxFloatCurve() : base("GfxFloatCurve")
        {

        }
        public CGfxFloatCurve(NativePointer nativePointer) : base(nativePointer)
        {
        }
    }
}
