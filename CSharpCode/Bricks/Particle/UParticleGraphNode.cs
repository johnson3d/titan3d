using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class UParticleGraphNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin ColorPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Color");
        public Graphics.Pipeline.Common.URenderGraphPin DepthPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Depth");
        public UParticleGraphNode()
        {
            Name = "ParticleGraphNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        }
        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        public async override System.Threading.Tasks.Task Initialize(Graphics.Pipeline.URenderPolicy policy,
                    string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
        }
        public List<GamePlay.Scene.UMeshNode> ParticleNodes = new List<GamePlay.Scene.UMeshNode>();
        public override unsafe void BeginTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            cmd.BeginEvent("NebulaUpdate");
        }
        public override unsafe void EndTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.EndEvent();
            cmd.EndCommand();
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
