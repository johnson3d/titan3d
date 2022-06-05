using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfSkinMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public Mesh.Modifier.CSkinModifier SkinModifier { get; set; }
        public RHI.CConstantBuffer PerSkinMeshCBuffer { get; set; }
        public UMdfSkinMesh()
        {
            SkinModifier = new Mesh.Modifier.CSkinModifier();
            unsafe
            {
                mCoreObject.PushModifier(SkinModifier.mCoreObject.NativeSuper);
            }

            UpdateShaderCode();
        }
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position,
                EVertexStreamType.VST_Normal,
                EVertexStreamType.VST_SkinIndex,
                EVertexStreamType.VST_SkinWeight};
        }
        public override void CopyFrom(UMdfQueue mdf)
        {
            mCoreObject.ClearModifiers();
            SkinModifier = (mdf as UMdfSkinMesh).SkinModifier;
            PerSkinMeshCBuffer = (mdf as UMdfSkinMesh).PerSkinMeshCBuffer;
            unsafe
            {
                mCoreObject.PushModifier(SkinModifier.mCoreObject.NativeSuper);
            }
        }
        protected override string GetBaseBuilder(Bricks.CodeBuilder.Backends.UHLSLCodeGenerator codeBuilder)
        {
            var codeString = "";
            var mdfSourceName = RName.GetRName("shaders/modifier/SkinModifier.cginc", RName.ERNameType.Engine);
            codeBuilder.AddLine($"#include \"{mdfSourceName.Address}\"", ref codeString);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)", ref codeString);
            codeBuilder.PushSegment(ref codeString);
            {
                codeBuilder.AddLine("DoSkinModifierVS(output, input);", ref codeString);
            }
            codeBuilder.PopSegment(ref codeString);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref codeString);

            var code = Editor.ShaderCompiler.UShaderCodeManager.Instance.GetShaderCodeProvider(mdfSourceName);
            codeBuilder.AddLine($"//Hash for {mdfSourceName}:{UniHash.APHash(code.SourceCode.AsText)}", ref codeString);

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeString);
            return codeString;
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
                
                if (PerSkinMeshCBuffer == null)
                {
                    PerSkinMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(drawcall.Effect.ShaderProgram, "cbSkinMesh");
                }

                var binder = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbSkinMesh");
                if (CoreSDK.IsNullPointer(binder))
                {
                    return;
                }
                drawcall.mCoreObject.BindShaderCBuffer(binder, PerSkinMeshCBuffer.mCoreObject);

                List<Vector4> tempPos = new List<Vector4>();
                Vector4* absPos = (Vector4*)PerSkinMeshCBuffer.mCoreObject.GetVarPtrToWrite(0, length);
                Quaternion* absQuat = (Quaternion*)PerSkinMeshCBuffer.mCoreObject.GetVarPtrToWrite(1, length);
                
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
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            var codeString = GetBaseBuilder(codeBuilder);

            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref codeString);
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {

            }

            SourceCode.SetText(codeString);
        }
    }
}
