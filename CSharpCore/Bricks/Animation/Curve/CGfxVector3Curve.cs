using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Curve
{
    public class CGfxVector3Curve : CGfxICurve
    {
        public CGfxVector3Curve() : base("GfxVector3Curve")
        {

        }
        public CGfxVector3Curve(NativePointer nativePointer) : base(nativePointer)
        {
        }
    }
}
