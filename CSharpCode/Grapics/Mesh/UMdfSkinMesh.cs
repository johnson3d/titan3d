using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfSkinMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public Mesh.Modifier.CSkinModifier SkinModifier { get; set; }
        public UMdfSkinMesh()
        {
            SkinModifier = new Mesh.Modifier.CSkinModifier();
            unsafe
            {
                mCoreObject.PushModifier(SkinModifier.mCoreObject.NativeSuper);
            }

            UpdateShaderCode();
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position,
                EVertexSteamType.VST_Normal,
                EVertexSteamType.VST_SkinIndex,
                EVertexSteamType.VST_SkinWeight};
        }
        public override void CopyFrom(UMdfQueue mdf)
        {
            mCoreObject.ClearModifiers();
            SkinModifier = (mdf as UMdfSkinMesh).SkinModifier;
            unsafe
            {
                mCoreObject.PushModifier(SkinModifier.mCoreObject.NativeSuper);
            }
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/modifier/SkinModifier.cginc", RName.ERNameType.Engine).Address}\"");
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {
                codeBuilder.AddLine("DoSkinModifierVS(output, input);");
            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Pipeline.URenderPolicy policy, Mesh.UMesh mesh)
        {
            unsafe
            {
                var bones = mesh.MaterialMesh.Mesh.PartialSkeleton.GetLimb<Animation.SkeletonAnimation.Skeleton.Limb.UBone>();
                int length = mesh.MaterialMesh.Mesh.PartialSkeleton.GetLimb<Animation.SkeletonAnimation.Skeleton.Limb.UBone>().Count;
                var runtimePose = SkinModifier.RuntimeMeshSpacePose;
                if(runtimePose == null)
                {
                    var animPose = mesh.MaterialMesh.Mesh.PartialSkeleton.CreatePose() as Animation.SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
                    runtimePose = Animation.SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.CreateMeshSpaceRuntimePose(animPose);
                }
                List<Vector4> tempPos = new List<Vector4>();
                Vector4* absPos = (Vector4*)mesh.PerMeshCBuffer.mCoreObject.GetVarPtrToWrite(mesh.PerMeshCBuffer.PerMeshIndexer.AbsBonePos, length);
                Quaternion* absQuat = (Quaternion*)mesh.PerMeshCBuffer.mCoreObject.GetVarPtrToWrite(mesh.PerMeshCBuffer.PerMeshIndexer.AbsBoneQuat, length);
                
                foreach (var bone in bones)
                {
                    var boneDesc = bone.Desc as Animation.SkeletonAnimation.Skeleton.Limb.UBoneDesc;
                    var index = Animation.SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.GetIndex(boneDesc.NameHash, runtimePose);
                    if(index.IsValid())
                    {
                        //var trans = limPose.Transtorm;
                        var trans = runtimePose.Transforms[index.Value];
                        
                        * ((Vector3*)absPos) = trans.Position.ToSingleVector3() + trans.Quat * boneDesc.InvPos;
                        tempPos.Add(*absPos);
                        absPos->W = 0;
                        *absQuat = boneDesc.InvQuat * trans.Quat;
                    }
                    
                    absPos++;
                    absQuat++;
                }

            }
        }
        public override Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            if (features.Contains("UMdf_NoShadow"))
            {
                return Rtti.UTypeDescGetter<UMdfSkinMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc;
            }
            else
            {
                return Rtti.UTypeDescGetter<UMdfSkinMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc;
            }
        }
    }

    public class UMdfSkinMeshPermutation<PermutationType> : UMdfSkinMesh
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/modifier/SkinModifier.code", RName.ERNameType.Engine).Address}\"");
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {
                codeBuilder.AddLine("DoSkinModifierVS(output, input);");
            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");
            
            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1");
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
    }
}
