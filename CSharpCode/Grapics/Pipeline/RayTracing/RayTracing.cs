using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.RayTracing
{
    public class TtRayTracingEnv : TtRayTracingShadingEnv
    {
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
        public TtRayTracingDraw mRayTracingDraw;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName + ".BasePass");

            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtRayTracingEnv>();

            //mRayTracingDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateRayTracingDraw();
            //mBasePassShading.SetDispatchRay(this, policy, mRayTracingDraw, 1, 1, 1);
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            if (mRayTracingDraw == null)
            {
                return;
            }
            var cmdlist = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                cmdlist.PushGpuDraw(mRayTracingDraw);
                cmdlist.FlushDraws();
            }
            policy.CommitCommandList(cmdlist);
        }
    }
}
