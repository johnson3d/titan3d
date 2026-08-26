using EngineNS.Bricks.FX.Water;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure
{
    public class TtPgcGpuProcessor
    {
        public Graphics.Pipeline.TtRenderPolicy Policy;
        public Action<Graphics.Pipeline.TtRenderGraphNode, Graphics.Pipeline.TtRenderGraphPin, Graphics.Pipeline.TtAttachBuffer> OnBufferRemoved = null;
        public void Process()
        {
            TtEngine.Instance.ThreadRender.QueueRenderAction("SWEUpdate", static (in Thread.TtThreadRender.FRenderAction RAct) =>
            {
                var This = (RAct.Arg as TtPgcGpuProcessor);
                This.Policy.BeginTick(null);
                This.Policy.Tick(null, This.OnBufferRemoved);
                This.Policy.EndTick(null);
            }, this);
            TtEngine.Instance.ThreadRender.WaitFinishRenderAction(null);
            //Policy.AttachmentCache.FindAttachement();
        }
    }
}
