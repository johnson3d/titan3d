using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Animation.SkeletonControl
{
    public class CGfxLookAt : CGfxSkeletonControl
    {
        string mModifyBoneName;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public string ModifyBoneName
        {
            get => mModifyBoneName;
            set
            {
                mModifyBoneName = value;
                SDK_GfxLookAt_SetModifyBoneName(CoreObject, value);
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
                SDK_GfxLookAt_SetTargetBoneName(CoreObject, value);
            }
        }
        Vector3 mTargetPosition = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 TargetPosition
        {
            get => mTargetPosition;
            set
            {
                mTargetPosition = value;
                SDK_GfxLookAt_SetTargetPosition(CoreObject, value);
            }
        }
        Vector3 mLookAtAxis = Vector3.UnitZ;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 LookAtAxis
        {
            get => mLookAtAxis;
            set
            {
                mLookAtAxis = value;
                SDK_GfxLookAt_SetLookAtAxis(CoreObject, value);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static CGfxLookAt CreateLookAt(GamePlay.Actor.GActor actor, string modifyBone, string targetBone, Vector3 loolAtAxis)
        {
            return null;
            //var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            //if (meshComp == null)
            //    return null;
            //var animationCom = actor.GetComponent<EngineNS.GamePlay.Component.GMacrossAnimationComponent>();
            //if (animationCom == null)
            //    return null;
            //var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            //if (skinModifier == null)
            //    return null;
            //var FinalPose = skinModifier.AnimationPose;
            //var lookAt = new EngineNS.Bricks.Animation.SkeletonControl.CGfxLookAt();
            //lookAt.ModifyBoneName = modifyBone;
            //if (targetBone != null)
            //    lookAt.TargetBoneName = targetBone;
            //lookAt.LookAtAxis = Vector3.UnitX;
            //lookAt.Pose = FinalPose;
            //lookAt.Alpha = 1;
            //animationCom.skeletonControls.Add(lookAt);

            //return lookAt;
        }

        public CGfxLookAt():base($"GfxLookAt")
        {

        }


        public override void TickLogic()
        {
            //time = 0;
            base.TickLogic();
        }

        #region SDK

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxLookAt_SetTargetBoneName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxLookAt_SetModifyBoneName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxLookAt_SetTargetPosition(NativePointer self, Vector3 position);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxLookAt_SetLookAtAxis(NativePointer self, Vector3 axis);
        #endregion
    }
}
