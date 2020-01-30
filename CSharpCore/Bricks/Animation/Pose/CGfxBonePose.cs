using EngineNS.Bricks.Animation.Skeleton;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.Pose
{
    public class CGfxBonePose : AuxCoreObject<CGfxBonePose.NativePointer>, IScocket
    {
        public event MeshSpaceMatrixChange OnMeshSpaceMatrixChange;
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
        public string Name
        {
            get
            {
                if (mReferenceBone == null)
                    return "";
                return mReferenceBone.BoneDesc.Name;
            }
        }
        public uint NameHash
        {
            get
            {
                if (mReferenceBone == null)
                    return 0;
                return mReferenceBone.BoneDesc.NameHash;
            }
        }
        public string ParentName
        {
            get
            {
                if (mReferenceBone == null)
                    return "";
                return mReferenceBone.BoneDesc.Parent;
            }
        }
        public uint ParentNameHash
        {
            get
            {
                if (mReferenceBone == null)
                    return 0;
                return mReferenceBone.BoneDesc.ParentHash;
            }
        }
        public Matrix MeshSpaceMatrix
        {
            get
            {
                //return Matrix.Transformation(Transform.Scale, Transform.Rotation, Transform.Position);
                return Matrix.Transformation(Vector3.UnitXYZ, Transform.Rotation, Transform.Position);
            }
        }
        CGfxBone mReferenceBone = null;
        public CGfxBone ReferenceBone { get => mReferenceBone; }
        public void SetReferenceBone(CGfxBone bone)
        {
            mReferenceBone = bone;
            SDK_GfxBonePose_SetReferenceBone(CoreObject, bone.CoreObject);
        }
        #region transform and MotionData
        public CGfxBoneTransform Transform
        {
            get
            {
                CGfxBoneTransform trans = CGfxBoneTransform.Identify;
                GetTransform(ref trans);
                return trans;
            }
            set
            {
                SetTransform(ref value);
            }
        }
        public void GetTransform(ref CGfxBoneTransform transform)
        {
            unsafe
            {
                fixed (CGfxBoneTransform* p = &transform)
                {
                    SDK_GfxBonePose_GetTransform(CoreObject, p);
                }
            }
        }
        public void SetTransform(ref CGfxBoneTransform transform)
        {
            unsafe
            {
                fixed (CGfxBoneTransform* p = &transform)
                {
                    SDK_GfxBonePose_SetTransform(CoreObject, p);
                }
            }
        }
        public CGfxMotionState MotionData
        {
            get
            {
                CGfxMotionState data = new CGfxMotionState();
                GetMotionData(ref data);
                return data;
            }
            set
            {
                SetMotionData(ref value);
            }
        }
        public void GetMotionData(ref CGfxMotionState motionData)
        {
            unsafe
            {
                fixed (CGfxMotionState* p = &motionData)
                {
                    SDK_GfxBonePose_GetMotionData(CoreObject, p);
                }
            }
        }
        public bool IsMotionDataSeted = false; // 临时
        public void SetMotionData(ref CGfxMotionState motionData)
        {
            unsafe
            {
                fixed (CGfxMotionState* p = &motionData)
                    SDK_GfxBonePose_SetMotionData(CoreObject, p);
            }
            IsMotionDataSeted = true;
        }
        #endregion 

        public CGfxBonePose()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxBonePose");
        }
        ~CGfxBonePose()
        {
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxBonePose_GetTransform(NativePointer self, CGfxBoneTransform* transform);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxBonePose_SetTransform(NativePointer self, CGfxBoneTransform* transform);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxBonePose_GetMotionData(NativePointer self, CGfxMotionState* motionData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxBonePose_SetMotionData(NativePointer self, CGfxMotionState* motionData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxBonePose_SetReferenceBone(NativePointer self, CGfxBone.NativePointer bone);
        #endregion
    }
}
