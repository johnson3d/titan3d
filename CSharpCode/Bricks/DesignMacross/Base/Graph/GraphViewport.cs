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
    public class TtGraphViewport
    {
        //视口 screen space
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public bool IsInViewport(Vector2 screenPos)
        {
            Rect veiwPortRect = new Rect(Location, Size);
            return veiwPortRect.Contains(screenPos);
        }

        public Vector2 ViewportTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos - cameraPos + Location;
        }
        public Vector2 ViewportInverseTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos + cameraPos - Location;
        }
    }
}
