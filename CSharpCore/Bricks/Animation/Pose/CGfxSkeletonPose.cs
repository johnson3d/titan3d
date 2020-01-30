using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using EngineNS.Bricks.Animation.Skeleton;

namespace EngineNS.Bricks.Animation.Pose
{
    public class CGfxSkeletonPose : AuxCoreObject<CGfxSkeletonPose.NativePointer>
    {
        CGfxSkeleton mReferenceSkeleton = null;
        public CGfxSkeleton ReferenceSkeleton { get => mReferenceSkeleton; }
        public int HashSkeleton { get => mReferenceSkeleton.GetHashCode(); }
        public void SetReferenceSkeleton(CGfxSkeleton skeleton)
        {
            mReferenceSkeleton = skeleton;
            SDK_GfxSkeletonPose_SetReferenceSkeleton(CoreObject,skeleton.CoreObject);
        }
        public Matrix WorldMatrix { get; set; } = Matrix.Identity;
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
        List<CGfxBonePose> mBones = new List<CGfxBonePose>();
        public List<CGfxBonePose> Bones
        {
            get { return mBones; }
            protected set
            {
                mBones = value;
                for (int i = 0; i < mBones.Count; ++i)
                {
                    mBonesDic.Add(mBones[i].ReferenceBone.BoneDesc.NameHash, mBones[i]);
                }
            }
        }
        Dictionary<uint, CGfxBonePose> mBonesDic = new Dictionary<uint, CGfxBonePose>();
        public void Add(CGfxBonePose bonePose)
        {
            mBones.Add(bonePose);
            mBonesDic.Add(bonePose.NameHash, bonePose);
            SDK_GfxSkeletonPose_AddBonePose(CoreObject,bonePose.CoreObject);
        }
        public CGfxSkeletonPose()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxSkeletonPose");
        }
        ~CGfxSkeletonPose()
        {

        }

        public CGfxSkeletonPose Clone()
        {
            CGfxSkeletonPose pose = new CGfxSkeletonPose();
            pose.SetReferenceSkeleton(ReferenceSkeleton);
            for (int i = 0; i < mBones.Count; ++i)
            {
                CGfxBonePose bonePose = new CGfxBonePose();
                bonePose.Transform = mBones[i].Transform;
                bonePose.MotionData = mBones[i].MotionData;
                bonePose.SetReferenceBone(mBones[i].ReferenceBone);
                pose.Add(bonePose);
            }
            return pose;
        }
        public CGfxSkeletonPose(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }


        public CGfxBonePose Root
        {
            get
            {
                if (ReferenceSkeleton == null)
                    return null;
                if (ReferenceSkeleton.Root == null)
                    return null;
                return GetBonePose(ReferenceSkeleton.Root.IndexInTable);
            }
        }
        public CGfxBonePose FindBonePose(string name)
        {
            var hash = UniHash.APHash(name);
            return FindBonePose(hash);
        }

        public CGfxBonePose FindBonePose(uint nameHash)
        {
            if (mBonesDic.ContainsKey(nameHash))
                return mBonesDic[nameHash];
            return null;
        }
        public int BoneNumber
        {
            get => mBones.Count;
        }

        public CGfxBonePose GetBonePose(uint index)
        {
            if (index >= mBones.Count)
                return null;
            return mBones[(int)index];
        }

        /// <summary>
        /// 直接操作c++ 对象，会造成C# 与 C++ 的不同步
        /// </summary>
        #region Native Call
        public CGfxBone FindBoneNative(string name)
        {
            var ptr = SDK_GfxSkeletonPose_FindBone(CoreObject, name);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone FindBoneNative(uint nameHash)
        {
            var ptr = SDK_GfxSkeletonPose_FindBoneByNameHash(CoreObject, nameHash);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone GetBoneNative(UInt32 index)
        {
            var ptr = SDK_GfxSkeletonPose_GetBone(CoreObject, index);
            if (ptr.Pointer == IntPtr.Zero)
                return null;
            return new CGfxBone(ptr);
        }
        public CGfxBone NewBoneNative(CGfxBoneDesc desc)
        {
            var result = new CGfxBone(SDK_GfxSkeletonPose_NewBone(CoreObject, desc.CoreObject));
            result.Core_Release();
            return result;
        }

        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeletonPose_GetRoot(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeletonPose_SetRoot(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonPose_SetRootByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxSkeletonPose_ExtractRootMotion(NativePointer self, vBOOL OnlyPosition);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxSkeletonPose_ExtractRootMotionPosition(NativePointer self, vBOOL ingoreY);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeletonPose_FindBone(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeletonPose_FindBoneByNameHash(NativePointer self, uint nameHash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxSkeletonPose_GetBoneNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeletonPose_GetBone(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxBone.NativePointer SDK_GfxSkeletonPose_NewBone(NativePointer self, CGfxBoneDesc.NativePointer pBone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxSkeletonPose_RemoveBone(NativePointer self, uint pBone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonPose_AddBonePose(NativePointer self,CGfxBonePose.NativePointer bonePose);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxSkeletonPose_SetReferenceSkeleton(NativePointer self,CGfxSkeleton.NativePointer skeleton);
        #endregion
    }
}
