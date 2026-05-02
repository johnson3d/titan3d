using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline;
using EngineNS.Bricks.RenderPolicyEditor;

namespace EngineNS.Bricks.Procedure.Node.GpuNode
{
    public class TtGpuNodeBase : Node.UAnyTypeMonocular
    {
        protected RName mPolicyName;
        [Rtti.Meta("",Order = 2)]
        [RName.PGRName(FilterExts = TtRenderPolicyAsset.AssetExt)]
        public RName PolicyName
        {
            get
            {
                return mPolicyName;
            }
            set
            {
                var policy = value.GetAsset<TtRenderPolicyAsset>().GetResultUntilCompleted().CreateRenderPolicy(null, null);
                policy.Initialize(null).WaitCompletedAndDispose();
                Policy = policy;
                mPolicyName = value;
                GpuProcessor.Policy = policy;
            }
        }
        public bool IsCapture { get; set; } = false;
        protected Graphics.Pipeline.TtRenderPolicy Policy;
        protected TtPgcGpuProcessor GpuProcessor = new TtPgcGpuProcessor();
        protected void GpuProcess()
        {
            if (IsCapture)
            {
                TtEngine.Instance.GfxDevice.RenderQueue.CaptureRenderDocFrame = true;
                TtEngine.Instance.GfxDevice.RenderQueue.BeginFrameCapture();
            }
            GpuProcessor.Process();
            if (IsCapture)
            {
                TtEngine.Instance.GfxDevice.RenderQueue.EndFrameCapture(this.Name);
            }
        }
    }
}
