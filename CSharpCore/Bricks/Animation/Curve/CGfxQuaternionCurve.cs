using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.Curve
{
    public class CGfxQuaternionCurve : CGfxICurve
    {
        public CGfxQuaternionCurve() : base("GfxQuaternionCurve")
        {

        }
        public CGfxQuaternionCurve(NativePointer nativePointer) : base(nativePointer)
        {
        }
    }
}
