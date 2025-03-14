using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.RayTracing
{
    public class TtRayTracingEnv : TtRayTracingShadingEnv
    {
        Vector3ui mDispatchArg = Vector3ui.One;
        public TtRayTracingEnv() 
        {
            CodeName = RName.GetRName("Shaders/ShadingEnv/RayTracing/Raytracing.hlsl", RName.ERNameType.Engine);
            MainName = "MyRaygenShader";
        }
    }
    [Bricks.CodeBuilder.ContextMenu("RayTracing", "GI\\RayTracing", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtRayTracingNode : TtRenderGraphNode
    {
        public TtRenderGraphPin ColorPinInOut = TtRenderGraphPin.CreateInputOutput("Color", true, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin LightingPinOut = TtRenderGraphPin.CreateOutput("Lighting", true, EPixelFormat.PXF_R16_FLOAT, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRayTracingNode()
        {
            Name = "RayTracingNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut);
            ColorPinInOut.IsAllowInputNull = true;
            AddOutput(LightingPinOut);

            base.InitNodePins();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public TtRayTracingEnv mBasePassShading;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtRayTracingEnv>();

            //mCopyColorDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            //mCopyDepthDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();

            //CopyPass.Initialize(rc, debugName + ".CopyPrev");
        }
    }
}
