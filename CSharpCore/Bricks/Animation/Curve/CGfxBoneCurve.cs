using EngineNS.Bricks.Animation.Skeleton;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Animation.Curve
{
    public class CGfxBoneCurve : CGfxICurve
    {
        public CGfxBoneCurve() : base("GfxBoneCurve")
        {

        }
        public CGfxBoneCurve(NativePointer nativePointer) : base(nativePointer)
        {
        }
        public CGfxMotionState EvaluateMotionState(float curveT)
        {
            CGfxMotionState data = new CGfxMotionState();
            unsafe
            {
                SDK_GfxBoneCurve_EvaluateMotionState(CoreObject, curveT, &data);
            }
            return data;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static void SDK_GfxBoneCurve_EvaluateMotionState(NativePointer self, float curveT, CGfxMotionState* motionData);
        #endregion
    }
}
