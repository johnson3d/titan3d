using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UCopy2SwapChainNode : URenderGraphNode
    {
        public Common.URenderGraphPin ColorPinInOut = Common.URenderGraphPin.CreateInputOutput("Color");
        public Common.URenderGraphPin HitIdPinIn = Common.URenderGraphPin.CreateInput("HitId");
        public Common.URenderGraphPin HzbPinIn = Common.URenderGraphPin.CreateInput("Hzb");
        public UCopy2SwapChainNode()
        {
            Name = "Copy2SwapChainNode";
        }
        public override void Cleanup()
        {
            base.Cleanup();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinInOut, EGpuBufferViewType.GBVT_Srv);
            HitIdPinIn.IsAllowInputNull = true;
            AddInput(HitIdPinIn, EGpuBufferViewType.GBVT_Srv);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn, EGpuBufferViewType.GBVT_Srv);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
        public override void FrameBuild()
        {
            
        }
    }
}
