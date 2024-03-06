using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline;
using EngineNS.Bricks.RenderPolicyEditor;

namespace EngineNS.Bricks.Procedure.Node.GpuNode
{
    public class TtGpuNodeBase : Node.UAnyTypeMonocular
    {
        protected RName mPolicyName;
        [Rtti.Meta(Order = 2)]
        [RName.PGRName(FilterExts = URenderPolicyAsset.AssetExt)]
        public RName PolicyName
        {
            get
            {
                return mPolicyName;
            }
            set
            {
                var action = async () =>
                {
                    var policy = URenderPolicyAsset.LoadAsset(value).CreateRenderPolicy(null, null);
                    await policy.Initialize(null);
                    Policy = policy;
                    mPolicyName = value;
                    GpuProcessor.Policy = policy;
                };
                action();
            }
        }
        public bool IsCapture { get; set; } = false;
        protected Graphics.Pipeline.URenderPolicy Policy;
        protected TtPgcGpuProcessor GpuProcessor = new TtPgcGpuProcessor();
        protected void GpuProcess()
        {
            if (IsCapture)
            {
                UEngine.Instance.GfxDevice.RenderCmdQueue.CaptureRenderDocFrame = true;
                UEngine.Instance.GfxDevice.RenderCmdQueue.BeginFrameCapture();
            }
            GpuProcessor.Process();
            if (IsCapture)
            {
                UEngine.Instance.GfxDevice.RenderCmdQueue.EndFrameCapture(this.Name);
            }
        }
    }
}
