using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Find", "Find", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UFindNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UFindNode" })]
    public class TtFindNode : TtRenderGraphNode
    {
        public TtRenderGraphPin InputPinInOut = TtRenderGraphPin.CreateInputOutput("Input", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        TtRenderGraphNode mNode;
        public string mProxyNodeName = "";
        [Rtti.Meta]
        public string ProxyNodeName
        {
            get => mProxyNodeName;
            set
            {
                mProxyNodeName = value;
                mNode = RenderGraph?.FindNodeIgnore(mProxyNodeName, typeof(TtFindNode));
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
        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(0, 255, 255);
        }
        [Rtti.Meta]
        public string ProxyPinName
        {
            get; set;
        } = "";
        public TtFindNode()
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
            AddInputOutput(InputPinInOut);
            ResultPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(ResultPinOut);
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ResultAttachement);
            base.Dispose();
        }
        TtAttachBuffer ResultAttachement = new TtAttachBuffer();
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            if (string.IsNullOrEmpty(ProxyNodeName))
                return;
            if (mNode == null)
            {
                ProxyNodeName = ProxyNodeName;
            }
            if (mNode == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"{ProxyNodeName} is not found");
                return;
            }
            var pin = mNode.FindOutput(ProxyPinName);
            if (pin == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"{ProxyNodeName}:{ProxyPinName} is not found");
                return;
            }
            var refAttachement = RenderGraph.AttachmentCache.FindAttachement(pin.Attachement.AttachmentName);
            if (refAttachement == null)
                return;
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ResultPinOut, ResultAttachement);
            attachement.Srv = refAttachement.Srv;
            attachement.Uav = refAttachement.Uav;
        }
    }
}
