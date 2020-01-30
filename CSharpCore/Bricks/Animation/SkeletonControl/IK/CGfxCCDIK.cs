using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.SkeletonControl.IK
{
    public class CGfxCCDIK : CGfxSkeletonControl
    {
        string mEndEffecterBoneName;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string EndEffecterBoneName
        {
            get => mEndEffecterBoneName;
            set
            {
                mEndEffecterBoneName = value;
                SDK_GfxCCDIK_SetEndEffecterBoneByName(CoreObject, value);
            }
        }
        string mTargetBoneName;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string TargetBoneName
        {
            get => mTargetBoneName;
            set
            {
                mTargetBoneName = value;

                SDK_GfxCCDIK_SetTargetBoneByName(CoreObject, value);
            }
        }

        public bool SetEndEffecterBoneByNameHash(uint nameHash)
        {
            return SDK_GfxCCDIK_SetEndEffecterBoneByNameHash(CoreObject,nameHash);
        }
        public bool SetTargetBoneByNameHash(uint nameHash)
        {
            return SDK_GfxCCDIK_SetTargetBoneByNameHash(CoreObject, nameHash);
        }
        public bool SetEndEffecterBoneByIndex(uint index)
        {
            return SDK_GfxCCDIK_SetEndEffecterBoneByIndex(CoreObject, index);
        }
        public bool SetTargetBoneByIndex(uint index)
        {
            return SDK_GfxCCDIK_SetTargetBoneByIndex(CoreObject, index);
        }
        public bool SetEndEffecterBone(Skeleton.CGfxBone bone)
        {
            return SDK_GfxCCDIK_SetEndEffecterBone(CoreObject, bone.CoreObject);
        }
        public bool SetTargetBone(Skeleton.CGfxBone bone)
        {
            return SDK_GfxCCDIK_SetTargetBone(CoreObject, bone.CoreObject);
        }
        Vector3 mTargetPosition = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 TargetPosition
        {
            get => mTargetPosition;
            set
            {
                mTargetPosition = value;
                SDK_GfxCCDIK_SetTargetPosition(CoreObject, value);
            }
        }
        uint mDepth = 1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public uint Depth
        {
            get => mDepth;
            set
            {
                mDepth = value;
                SDK_GfxCCDIK_SetDepth(CoreObject, value);
            }
        }
        uint mIteration = 1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public uint Iteration
        {
            get => mIteration;
            set
            {
                mIteration = value;
                SDK_GfxCCDIK_SetIteration(CoreObject, value);
            }
        }
        float mLimitAngle = 1;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float LimitAngle
        {
            get => mLimitAngle;
            set
            {
                mLimitAngle = value;
                //c++ 弧度
                var rad = value * (EngineNS.MathHelper.V_PI / 180.0f);
                SDK_GfxCCDIK_SetLimitAngle(CoreObject, rad);
            }
        }
        public CGfxCCDIK() : base($"GfxCCDIK")
        {

        }
        ~CGfxCCDIK()
        {

        }
        public override void TickLogic()
        {
            //time = 0;
            base.TickLogic();
        }
        #region SDK

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetEndEffecterBoneByName(NativePointer self,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TextMarshaler))]
            string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetTargetBoneByName(NativePointer self,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TextMarshaler))]
            string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetEndEffecterBoneByNameHash(NativePointer self, uint nameHash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetTargetBoneByNameHash(NativePointer self, uint nameHash);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetEndEffecterBoneByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetTargetBoneByIndex(NativePointer self, uint index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetEndEffecterBone(NativePointer self, Skeleton.CGfxBone.NativePointer bone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxCCDIK_SetTargetBone(NativePointer self, Skeleton.CGfxBone.NativePointer bone);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCCDIK_SetTargetPosition(NativePointer self, Vector3 position);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCCDIK_SetDepth(NativePointer self, uint depth);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCCDIK_SetIteration(NativePointer self, uint iteration);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxCCDIK_SetLimitAngle(NativePointer self, float limitAngle);
        #endregion
    }
}
