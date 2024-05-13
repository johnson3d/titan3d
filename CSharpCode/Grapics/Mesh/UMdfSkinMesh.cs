using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfSkinMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.TtSkinModifier>
    {
        public Mesh.Modifier.TtSkinModifier SkinModifier 
        { 
            get
            {
                return this.Modifiers[0] as Mesh.Modifier.TtSkinModifier;
            }
        }
        public NxRHI.UCbView PerSkinMeshCBuffer { get; set; }
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            PerSkinMeshCBuffer = (mdf as UMdfSkinMesh).PerSkinMeshCBuffer;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Pipeline.URenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);
            unsafe
            {
                var skeleton = atom.MeshPrimitives.PartialSkeleton;
                var bones = skeleton.GetLimb<Animation.SkeletonAnimation.Skeleton.Limb.TtBone>();
                int length = skeleton.GetLimb<Animation.SkeletonAnimation.Skeleton.Limb.TtBone>().Count;
                if(SkinModifier.RuntimePose == null)
                {
                    var animPose = skeleton.CreatePose() as Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                    SkinModifier.RuntimePose = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animPose);
                }
                var runtimePose = SkinModifier.RuntimePose;

                var shaderBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                if (PerSkinMeshCBuffer == null)
                {
                    if (shaderBinder.CBPerSkinMesh.UpdateFieldVar(drawcall.GraphicsEffect, "cbSkinMesh"))
                    {
                        PerSkinMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(shaderBinder.CBPerSkinMesh.Binder.mCoreObject);
                    }
                }

                var binder = drawcall.FindBinder("cbSkinMesh");
                if (binder.IsValidPointer == false)
                {
                    return;
                }
                drawcall.BindCBuffer(binder, PerSkinMeshCBuffer);

                Vector4* absPos = (Vector4*)PerSkinMeshCBuffer.mCoreObject.GetVarPtrToWrite(shaderBinder.CBPerSkinMesh.AbsBonePos, (uint)length);
                Quaternion* absQuat = (Quaternion*)PerSkinMeshCBuffer.mCoreObject.GetVarPtrToWrite(shaderBinder.CBPerSkinMesh.AbsBoneQuat, (uint)length);

                var meshSpaceRuntimePose = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(runtimePose);
                foreach (var bone in bones)
                {
                    var boneDesc = bone.Desc as Animation.SkeletonAnimation.Skeleton.Limb.TtBoneDesc;
                    var index = Animation.SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.GetIndex(boneDesc.NameHash, meshSpaceRuntimePose);
                    if(index.IsValid())
                    {
                        var trans = meshSpaceRuntimePose.Transforms[index.Value];
                        * ((Vector3*)absPos) = trans.Position.ToSingleVector3() + trans.Quat * boneDesc.InvPos;
                        absPos->W = 0;
                        *absQuat = boneDesc.InvQuat * trans.Quat;
                    }
                    
                    absPos++;
                    absQuat++;
                }

                PerSkinMeshCBuffer.mCoreObject.FlushWrite(true, UEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
            }
        }
    }
}
