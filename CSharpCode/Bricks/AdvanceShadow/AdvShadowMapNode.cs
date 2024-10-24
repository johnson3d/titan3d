using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AdvanceShadow
{
    [Bricks.CodeBuilder.ContextMenu("AdvShadow", "Shadow\\AdvShadow", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtAdvShadowMapNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin AdvDepthPinOut = TtRenderGraphPin.CreateOutput("AdvDepth", false, EPixelFormat.PXF_D16_UNORM);//or D32
        public TtAdvanceShadow mAdvanceShadow = new TtAdvanceShadow();
        public TtAdvShadowMapNode()
        {
            Name = "AdvShadowMapNode";
        }
        public override void InitNodePins()
        {
            AddOutput(AdvDepthPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            //2048 shadowmap * 2
            mAdvanceShadow.Initialize(5, 1024.0f, 32 * 2, 128.0f);
        }
        public override void TickLogic(TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            
        }
    }
}
