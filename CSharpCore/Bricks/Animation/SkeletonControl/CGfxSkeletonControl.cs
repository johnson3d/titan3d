using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS.Bricks.Animation.Pose;

namespace EngineNS.Bricks.Animation.SkeletonControl
{
    public class CGfxSkeletonControl : AuxCoreObject<CGfxSkeletonControl.NativePointer>, ILogicTick
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
        public RName Name { get; set; }
        protected CGfxSkeletonPose mPose;
        public CGfxSkeletonPose Pose
        {
            get
            {
                if (mPose == null)
                {
                    mPose = new CGfxSkeletonPose(SDK_GfxSkeletonControl_GetAnimationPose(CoreObject));
                }
                return mPose;
            }
            set
            {
                mPose = value;
                //mAnimationPose.GenerateHierarchy();
                SDK_GfxSkeletonControl_SetAnimationPose(CoreObject, value.CoreObject);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Alpha
        {
            set { SDK_GfxSkeletonControl_SetAlpha(CoreObject, value); }
            get { return SDK_GfxSkeletonControl_GetAlpha(CoreObject); }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Enable
        {
            set { SDK_GfxSkeletonControl_SetEnable(CoreObject, value); }
            get { return SDK_GfxSkeletonControl_GetEnable(CoreObject); }
        }
        protected CGfxSkeletonControl(string deriveClass)
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>(deriveClass);
        }
        private CGfxSkeletonControl()
        {
            // NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSkeletonControl");
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool EnableTick
        {
            get;
            set;
        }
        public virtual void TickLogic()
        {
            //time = 0;
            if (!EnableTick)
                return;
            SDK_GfxSkeletonControl_Update(CoreObject, CEngine.Instance.EngineElapseTime);
        }

        #region SDK

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonControl_Update(NativePointer self, Int64 time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxSkeletonPose.NativePointer SDK_GfxSkeletonControl_GetAnimationPose(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonControl_SetAnimationPose(NativePointer self, CGfxSkeletonPose.NativePointer animationPose);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxSkeletonControl_GetAlpha(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonControl_SetAlpha(NativePointer self, float alpha);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxSkeletonControl_GetEnable(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonControl_SetEnable(NativePointer self, bool enable);
        #endregion
    }
}
