using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Find", "Find", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UFindNode : TtRenderGraphNode
    {
        public TtRenderGraphPin InputPinInOut = TtRenderGraphPin.CreateInputOutput("Input");
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
                mNode = RenderGraph?.FindNodeIgnore(mProxyNodeName, typeof(UFindNode));
            }
        }
        public TtRenderGraphNode GetReferNode()
        {
            if (mNode == null)
            {
                ProxyNodeName = ProxyNodeName;
            }
            return mNode;
        }
        public override Color GetTileColor()
        {
            return Color.FromRgb(0, 255, 255);
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
            InputPinInOut.IsAllowInputNull = true;
            AddInputOutput(InputPinInOut, NxRHI.EBufferType.BFT_SRV);
            ResultPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            if (string.IsNullOrEmpty(ProxyNodeName))
                return;
            if (mNode == null)
            {
                ProxyNodeName = ProxyNodeName;
            }
            if (mNode == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Graphics", $"{ProxyNodeName} is not found");
                return;
            }
            var pin = mNode.FindOutput(ProxyPinName);
            if (pin == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Graphics", $"{ProxyNodeName}:{ProxyPinName} is not found");
                return;
            }
            var refAttachement = RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
            if (refAttachement == null)
                return;
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ResultPinOut);            
            attachement.Srv = refAttachement.Srv;
            attachement.Uav = refAttachement.Uav;
        }
    }
}
