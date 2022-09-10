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
        public Common.URenderGraphPin SavedPinIn0 = Common.URenderGraphPin.CreateInput("Save0");
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
            AddInputOutput(ColorPinInOut, NxRHI.EBufferType.BFT_SRV);
            HitIdPinIn.IsAllowInputNull = true;
            AddInput(HitIdPinIn, NxRHI.EBufferType.BFT_SRV);
            HzbPinIn.IsAllowInputNull = true;
            AddInput(HzbPinIn, NxRHI.EBufferType.BFT_SRV);
            SavedPinIn0.IsAllowInputNull = true;
            AddInput(SavedPinIn0, NxRHI.EBufferType.BFT_SRV);
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
