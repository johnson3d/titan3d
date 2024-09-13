using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtHdrShading : Shader.TtGraphicsShadingEnv
    {
        public TtHdrShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/screenspace/hdr.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var node = drawcall.TagObject as TtHdrNode;
            var lightSRV = node.GetAttachBuffer(node.ColorPinIn).Srv;
            var gpuSceneDescSRV = node.GetAttachBuffer(node.GpuScenePinIn).Srv;
            
            var index = drawcall.FindBinder("GSourceTarget");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, lightSRV);
            index = drawcall.FindBinder("Samp_GSourceTarget");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GpuSceneDescSRV");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, gpuSceneDescSRV);

            index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
                drawcall.BindCBuffer(index, policy.GetGpuSceneNode().PerGpuSceneCbv);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Hdr", "Post\\Hdr", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UHdrNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UHdrNode" })]
    public class TtHdrNode : TtSceenSpaceNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color");
        public TtRenderGraphPin GpuScenePinIn = TtRenderGraphPin.CreateInput("GpuScene");
        public TtHdrNode()
        {
            Name = "HdrNode";            
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(GpuScenePinIn, NxRHI.EBufferType.BFT_SRV);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            base.InitNodePins();
            //base Result
        }
        public TtHdrShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtHdrShading>();
        }
    }
}
