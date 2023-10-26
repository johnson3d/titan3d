using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class CSkinModifier : AuxPtrType<ISkinModifier>, Pipeline.Shader.IMeshModifier
    {
        public Animation.SkeletonAnimation.Runtime.Pose.UMeshSpaceRuntimePose RuntimeMeshSpacePose;
        public CSkinModifier()
        {
            mCoreObject = ISkinModifier.CreateInstance();
        }

        public string ModifierNameVS { get => "DoSkinModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/SkinModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_SkinIndex,
                NxRHI.EVertexStreamType.VST_SkinWeight};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
    }
}
