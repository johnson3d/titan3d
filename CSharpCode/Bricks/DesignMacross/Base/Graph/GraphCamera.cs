using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.Rtti;
using NPOI.OpenXml4Net.OPC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtGraphCamera
    {
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public Vector2 Scale { get; set; } = Vector2.One;
    }
}
