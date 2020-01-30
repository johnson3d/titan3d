using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public class CGfxBoneAnim : AuxCoreObject<CGfxBoneAnim.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        public CGfxBoneAnim(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        public CGfxBoneDesc BoneDesc
        {
            get
            {
                return new CGfxBoneDesc(SDK_GfxBoneAnim_GetBoneDesc(CoreObject));
            }
        }
        public void GetBoneCurve(ref Curve.CGfxBoneCurve boneCurve)
        {
            SDK_GfxBoneAnim_GetBoneCurve(mCoreObject, boneCurve.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBoneDesc.NativePointer SDK_GfxBoneAnim_GetBoneDesc(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxBoneAnim_GetBoneCurve(NativePointer self,Animation.Curve.CGfxBoneCurve.NativePointer boneCurve);
        #endregion
    }
}
