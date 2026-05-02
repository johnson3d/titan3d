using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtBasePassNode : TAuxRenderGraphNode<TtBasePassNode>
    {
        public TtGraphicsBuffers GBuffers { get; protected set; } = new TtGraphicsBuffers();
    }
}
