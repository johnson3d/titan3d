using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtBasePassNode : TtRenderGraphNode
    {
        public TtGraphicsBuffers GBuffers { get; protected set; } = new TtGraphicsBuffers();
    }
}
