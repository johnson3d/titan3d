using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UBasePassNode : TtRenderGraphNode
    {
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
    }
}
