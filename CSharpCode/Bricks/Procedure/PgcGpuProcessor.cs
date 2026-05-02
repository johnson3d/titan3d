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
            Policy.BeginTick(null);
            Policy.Tick(null, OnBufferRemoved);
            Policy.EndTick(null);
            //Policy.AttachmentCache.FindAttachement();
        }
    }
}
