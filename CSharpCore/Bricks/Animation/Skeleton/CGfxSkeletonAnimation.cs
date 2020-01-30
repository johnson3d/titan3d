using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.Animation.Skeleton
{
    public class CGfxSkeletonAnimation : AuxCoreObject<CGfxSkeletonAnimation.NativePointer>
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
        public CGfxSkeletonAnimation()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSkeletonAnimation");
        }
        public void GetAnimaPose(float time, ref CGfxSkeletonPose pose, bool evaluateMotionState = true)
        {
            SDK_GfxSkeletonAnimation_EvaluatePose(CoreObject, time, pose.CoreObject, vBOOL.FromBoolean(evaluateMotionState));
        }
        public void GetAnimaPose(float time, ref CGfxAnimationPoseProxy poseProxy, bool evaluateMotionState = true)
        {
            SDK_GfxSkeletonAnimation_EvaluatePose(CoreObject, time, poseProxy.Pose.CoreObject, vBOOL.FromBoolean(evaluateMotionState));
        }
        public void InitBySkeletonActioin(CGfxSkeletonAction action)
        {
            SDK_GfxSkeletonAnimation_InitBySkeletonActioin(mCoreObject, action.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAnimation_EvaluatePose(NativePointer self, float time, CGfxSkeletonPose.NativePointer animPose, vBOOL evaluateMotionState);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonAnimation_InitBySkeletonActioin(NativePointer self, CGfxSkeletonAction.NativePointer animPose);
        #endregion
    }
}
