using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UBasePassNode : Common.URenderGraphNode
    {
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UGraphicsBuffers GGizmosBuffers { get; protected set; } = new UGraphicsBuffers();
    }
}
