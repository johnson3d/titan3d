using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtCullingNode : TtRenderGraphNode
    {
        public TtRenderGraphPin HzbPinInOut = TtRenderGraphPin.CreateInputOutput("Hzb", false, EPixelFormat.PXF_UNKNOWN);
        public TtCullingNode()
        {
            Name = "GpuCulling";
        }
        public override void InitNodePins()
        {
            AddInputOutput(HzbPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_DSV);
        }
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            var cmd = BasePass.DrawCmdList;
            using (new NxRHI.TtCmdListScope(cmd))
            {
                Culling(policy, cmd);

                cmd.FlushDraws();
            }
            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd, "GpuCulling");
        }
        private void Culling(URenderPolicy policy, NxRHI.UCommandList cmd)
        {
            foreach (var i in policy.VisibleMeshes)
            {
                var mdfQueue = i.MdfQueue as Mesh.UMdfInstanceStaticMesh;
                if (mdfQueue != null)
                {
                    var modifier = mdfQueue.InstanceModifier;
                    if (modifier.IsGpuCulling)
                        modifier.InstanceBuffers.InstanceCulling(modifier, cmd.mCoreObject, policy, modifier.DrawArgsBuffer.Uav, 0);
                }
            }
        }
    }
}
