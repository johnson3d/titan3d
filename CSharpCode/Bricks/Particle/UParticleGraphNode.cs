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
            AddInputOutput(ColorPinInOut, EGpuBufferViewType.GBVT_Rtv | EGpuBufferViewType.GBVT_Srv);
            AddInputOutput(DepthPinInOut, EGpuBufferViewType.GBVT_Dsv | EGpuBufferViewType.GBVT_Srv);
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
        }
        public override unsafe void EndTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            cmd.EndCommand();
        }
        public unsafe override void TickRender(Graphics.Pipeline.URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
