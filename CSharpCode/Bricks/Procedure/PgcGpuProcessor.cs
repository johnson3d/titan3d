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
            Policy.BeginTickLogic(null);
            Policy.TickLogic(null, OnBufferRemoved);
            Policy.EndTickLogic(null);
            //Policy.AttachmentCache.FindAttachement();
        }
    }
}
