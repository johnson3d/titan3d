using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UFindNode : URenderGraphNode
    {
        public Common.URenderGraphPin ResultPinOut = Common.URenderGraphPin.CreateOutput("Result", false, EPixelFormat.PXF_A8_UNORM);
        [Rtti.Meta]
        public string ProxyNodeName
        {
            get; set;
        }
        [Rtti.Meta]
        public string ProxyPinName
        {
            get; set;
        }
        public UFindNode()
        {
            
        }
        public override string Name
        {
            get
            {
                return $"Find_{ProxyNodeName}:{ProxyPinName}";
            }
            set
            {

            }
        }
        public override void InitNodePins()
        {
            ResultPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public unsafe override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            var node = RenderGraph.FindNode(ProxyNodeName);
            if (node == null)
                return;
            var pin = node.FindOutput(ProxyPinName);
            if (pin == null)
                return;
            var refAttachement = RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
            if (refAttachement == null)
                return;
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ResultPinOut);            
            attachement.Srv = refAttachement.Srv;
            attachement.Uav = refAttachement.Uav;
        }
    }
}
