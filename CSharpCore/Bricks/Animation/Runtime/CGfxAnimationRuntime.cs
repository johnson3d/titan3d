using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.Skeleton;

namespace EngineNS.Bricks.Animation.Runtime
{
    public class CGfxAnimationRuntime : AuxCoreObject<CGfxAnimationRuntime.NativePointer>
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

        public static void BlendPose(CGfxSkeletonPose outPose, CGfxSkeletonPose aPose, CGfxSkeletonPose bPose, float weight)
        {
            if (outPose == null || aPose == null || bPose == null)
                return;
            if (outPose.HashSkeleton == aPose.HashSkeleton && outPose.HashSkeleton == bPose.HashSkeleton)
            {
                SDK_GfxAnimationRuntime_FastBlendPose(outPose.CoreObject, aPose.CoreObject, bPose.CoreObject, weight);
            }
            else
            {
                SDK_GfxAnimationRuntime_BlendPose(outPose.CoreObject, aPose.CoreObject, bPose.CoreObject, weight);
            }
            
        }
        public static void AddPose(CGfxSkeletonPose outPose, CGfxSkeletonPose basePose, CGfxSkeletonPose additivePose, float alpha)
        {
            if (outPose == null || basePose == null || additivePose == null)
                return;
            if (outPose.HashSkeleton == basePose.HashSkeleton && outPose.HashSkeleton == additivePose.HashSkeleton)
            {
                SDK_GfxAnimationRuntime_FastAddPose(outPose.CoreObject, basePose.CoreObject, additivePose.CoreObject, alpha);
            }
            else
            {
                SDK_GfxAnimationRuntime_AddPose(outPose.CoreObject, basePose.CoreObject, additivePose.CoreObject, alpha);
            }
            
        }
        public static void MinusPose(CGfxSkeletonPose outPose, CGfxSkeletonPose minusPose, CGfxSkeletonPose minuendPose)
        {
            if (outPose == null || minusPose == null || minuendPose == null)
                return;
            if (outPose.HashSkeleton == minusPose.HashSkeleton && outPose.HashSkeleton == minuendPose.HashSkeleton)
            {
                SDK_GfxAnimationRuntime_FastMinusPose(outPose.CoreObject, minusPose.CoreObject, minuendPose.CoreObject);
            }
            else
            {
                SDK_GfxAnimationRuntime_MinusPose(outPose.CoreObject, minusPose.CoreObject, minuendPose.CoreObject);
            }
        }
        //public static void MinusPoseMeshSpace(CGfxSkeletonPose outPose, CGfxSkeletonPose minusPose, CGfxSkeletonPose minuendPose)
        //{
        //    if (outPose == null || minusPose == null || minuendPose == null)
        //        return;
        //    SDK_GfxAnimationRuntime_MinusPoseMeshSpace(outPose.CoreObject, minusPose.CoreObject, minuendPose.CoreObject);
        //}
        public static bool IsZeroPose(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return true;
            return SDK_GfxAnimationRuntime_IsZeroPose(pose.CoreObject);
        }
        public static void ZeroPose(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ZeroPose(pose.CoreObject);
        }
        public static void ZeroTransition(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ZeroTransition(pose.CoreObject);
        }
        public static void CopyPose(CGfxSkeletonPose desPose, CGfxSkeletonPose srcPose)
        {
            if (desPose == null || srcPose == null)
                return;
            if (desPose.ReferenceSkeleton.GetHashCode() == srcPose.ReferenceSkeleton.GetHashCode())
            {
                SDK_GfxAnimationRuntime_FastCopyPose(desPose.CoreObject, srcPose.CoreObject);
            }
            else
            {
                SDK_GfxAnimationRuntime_CopyPose(desPose.CoreObject, srcPose.CoreObject);
            }
        }
        public static void FastCopyPose(CGfxSkeletonPose desPose, CGfxSkeletonPose srcPose)
        {
            if (desPose == null || srcPose == null)
                return;
            SDK_GfxAnimationRuntime_FastCopyPose(desPose.CoreObject, srcPose.CoreObject);
        }
        public static void CopyPoseAndConvertMeshSpace(CGfxSkeletonPose desPose, CGfxSkeletonPose srcPose)
        {
            if (desPose == null || srcPose == null)
                return;
            CopyPose(desPose, srcPose);
            ConvertToMeshSpace(desPose);
        }
        public static void CopyPoseAndConvertRotationToMeshSpace(CGfxSkeletonPose desPose, CGfxSkeletonPose srcPose)
        {
            if (desPose == null || srcPose == null)
                return;
            CopyPose(desPose, srcPose);
            ConvertRotationToMeshSpace(desPose);
        }
        public static void CopyPoseAndConvertRotationToLocalSpace(CGfxSkeletonPose desPose, CGfxSkeletonPose srcPose)
        {
            if (desPose == null || srcPose == null)
                return;
            CopyPose(desPose, srcPose);
            ConvertRotationToLocalSpace(desPose);
        }
        public static void ConvertToLocalSpace(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ConvertToLocalSpace(pose.CoreObject);
        }
        public static void ConvertToMeshSpace(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ConvertToMeshSpace(pose.CoreObject);
        }
        public static void ConvertRotationToMeshSpace(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ConvertRotationToMeshSpace(pose.CoreObject);
        }
        public static void ConvertRotationToLocalSpace(CGfxSkeletonPose pose)
        {
            if (pose == null)
                return;
            SDK_GfxAnimationRuntime_ConvertRotationToLocalSpace(pose.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxAnimationRuntime_IsZeroPose(CGfxSkeletonPose.NativePointer pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ZeroPose(CGfxSkeletonPose.NativePointer pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ZeroTransition(CGfxSkeletonPose.NativePointer pose);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_BlendPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer aPose, CGfxSkeletonPose.NativePointer bPose, float weight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_AddPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer basePose, CGfxSkeletonPose.NativePointer additivePose, float alpha);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_MinusPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer minusPose, CGfxSkeletonPose.NativePointer minuendPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_MinusPoseMeshSpace(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer minusPose, CGfxSkeletonPose.NativePointer minuendPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_FastBlendPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer aPose, CGfxSkeletonPose.NativePointer bPose, float weight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_FastAddPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer basePose, CGfxSkeletonPose.NativePointer additivePose, float alpha);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_FastMinusPose(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer minusPose, CGfxSkeletonPose.NativePointer minuendPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_FastMinusPoseMeshSpace(CGfxSkeletonPose.NativePointer outPose, CGfxSkeletonPose.NativePointer minusPose, CGfxSkeletonPose.NativePointer minuendPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_CopyPose(CGfxSkeletonPose.NativePointer desPose, CGfxSkeletonPose.NativePointer srcPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_FastCopyPose(CGfxSkeletonPose.NativePointer desPose, CGfxSkeletonPose.NativePointer srcPose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ConvertToMeshSpace(CGfxSkeletonPose.NativePointer pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ConvertToLocalSpace(CGfxSkeletonPose.NativePointer pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ConvertRotationToMeshSpace(CGfxSkeletonPose.NativePointer pose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxAnimationRuntime_ConvertRotationToLocalSpace(CGfxSkeletonPose.NativePointer pose);
        #endregion
    }
}
