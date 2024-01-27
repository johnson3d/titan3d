using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UFindNode : TtRenderGraphNode
    {
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", false, EPixelFormat.PXF_UNKNOWN);
        TtRenderGraphNode mNode;
        public string mProxyNodeName = "";
        [Rtti.Meta]
        public string ProxyNodeName
        {
            get => mProxyNodeName;
            set
            {
                mProxyNodeName = value;
                mNode = RenderGraph?.FindNode(ProxyNodeName);
            }
        }
        [Rtti.Meta]
        public string ProxyPinName
        {
            get; set;
        } = "";
        public UFindNode()
        {
            
        }
        //public override string Name
        //{
        //    get
        //    {
        //        return $"Find_{ProxyNodeName}:{ProxyPinName}";
        //    }
        //    set
        //    {

        //    }
        //}
        public override void InitNodePins()
        {
            ResultPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            if (mNode == null)
                mNode = RenderGraph.FindNode(ProxyNodeName);
            if (mNode == null)
                return;
            var pin = mNode.FindOutput(ProxyPinName);
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
