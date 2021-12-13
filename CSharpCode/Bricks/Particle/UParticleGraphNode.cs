using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class UParticleGraphNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        public async override System.Threading.Tasks.Task Initialize(Graphics.Pipeline.IRenderPolicy policy, Graphics.Pipeline.Shader.UShadingEnv shading,
                    EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
        }
        public List<GamePlay.Scene.UMeshNode> ParticleNodes = new List<GamePlay.Scene.UMeshNode>();
        public override unsafe void BeginTickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList.mCoreObject;
            cmd.BeginCommand();
        }
        public override unsafe void EndTickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList.mCoreObject;
            cmd.EndCommand();
        }
        public unsafe override void TickRender(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
