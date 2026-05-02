using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public partial struct FDepthStencilDesc
    {
        public void SetDefault()
        {
            SetDefault(TtEngine.Instance.Config.IsReverseZ);
        }
    }
    public partial struct FGpuPipelineDesc
    {
        public void SetDefault()
        {
            SetDefault(TtEngine.Instance.Config.IsReverseZ);
        }
    }
    public partial struct FBlendDesc
    {
        public unsafe EngineNS.NxRHI.FRenderTargetBlendDesc RenderTarget0
        {
            get
            {
                return RenderTarget[0];
            }
            set
            {
                RenderTarget[0] = value;
            }
        }
        public unsafe EngineNS.NxRHI.FRenderTargetBlendDesc RenderTarget1
        {
            get
            {
                return RenderTarget[1];
            }
            set
            {
                RenderTarget[1] = value;
            }
        }
        public unsafe EngineNS.NxRHI.FRenderTargetBlendDesc RenderTarget2
        {
            get
            {
                return RenderTarget[2];
            }
            set
            {
                RenderTarget[2] = value;
            }
        }
        public unsafe EngineNS.NxRHI.FRenderTargetBlendDesc RenderTarget3
        {
            get
            {
                return RenderTarget[3];
            }
            set
            {
                RenderTarget[3] = value;
            }
        }
    }
    public class TtSampler : AuxPtrType<NxRHI.ISampler>
    {
    }
    public class TtGpuPipeline : AuxPtrType<NxRHI.IGpuPipeline>
    {
    }
}
