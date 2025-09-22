using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Utility
{
    public class TtMaterialToTextureShading : Shader.TtGraphicsShadingEnv
    {
        public TtMaterialToTextureShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Utility/MaterialToTexture.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var aaNode = drawcall.TagObject as TtMaterialToTextureNode;

        }
    }
    [Bricks.CodeBuilder.ContextMenu("Mat2Texture", "Utility\\Mat2Texture", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtMaterialToTextureNode : Common.TAuxSceenSpaceNode<TtMaterialToTextureNode>
    {
        public TtMaterialToTextureNode()
        {
            Name = "Mat2Texture";
        }
        public TtMaterialToTextureShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(Mesh.TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtMaterialToTextureShading>();
        }
        [RName.PGRName(FilterExts = TtMaterial.AssetExt + "," + TtMaterialInstance.AssetExt)]
        [Category("Option")]
        public RName MaterialName
        {
            get
            {
                return ScreenMesh?.MaterialMesh.SubMeshes[0].Materials[0].AssetName;
            }
            set
            {
                TtMaterial material = null;
                if (value.ExtName == TtMaterial.AssetExt)
                {
                    material =TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value).GetResultUntilCompleted();
                }
                else if (value.ExtName == TtMaterial.AssetExt)
                {
                    material =TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(value).GetResultUntilCompleted();
                }
                ScreenMesh = CreateScreenMesh(material);
            }
        }
        public void SetMaterial(TtMaterial material)
        {
            ScreenMesh = CreateScreenMesh(material);
        }
    }
}
