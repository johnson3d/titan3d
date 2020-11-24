using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.Controls.NodeGraph
{
    public class PinLinker
    {
        public class Pin
        {
            public Guid NodeId { get; set; }
            public string Name { get; set; }
            public NodePin NodePin { get; set;}
        }
        public Pin OutPin { get; } = new Pin();
        public Pin InPin { get; } = new Pin();
        public PinOut Out
        {
            get
            {
                return OutPin.NodePin as PinOut;
            }
        }
        public PinIn In
        {
            get
            {
                return InPin.NodePin as PinIn;
            }
        }
        public void OnDraw(NodeGraph graph, NodeGraphStyles styles)
        {
            var cmdlist = new ImDrawList_PtrType(ImGuiAPI.GetWindowDrawList().NativePointer);

            var p1 = OutPin.NodePin.DrawPosition + styles.PinInStyle.Image.Size * graph.ScaleFactor / 2;
            var p4 = InPin.NodePin.DrawPosition + styles.PinOutStyle.Image.Size * graph.ScaleFactor / 2;
            var delta = p4 - p1;
            delta.X = Math.Abs(delta.X);

            var p2 = new Vector2(p1.X + delta.X * 0.5f, p1.Y);
            var p3 = new Vector2(p4.X - delta.X * 0.5f, p4.Y);

            int num_segs = (int)(delta.Length() / styles.BezierPixelPerSegement + 1);
            cmdlist.AddBezierCurve(ref p1, ref p2, ref p3, ref p4, styles.LinkerColor, 3, num_segs);
        }
    }
    public class LinkingLine
    {
        public NodePin StartPin { get; set; }
        public NodePin HoverPin { get; set; }
        public void OnDraw(NodeGraph graph, ref Vector2 endPoint, NodeGraphStyles styles = null)
        {
            var p4 = endPoint;
            Vector2 p1;
            float ControlLength = 0;
            bool IsPinIn = true;
            if(StartPin.GetType() == typeof(PinIn))
            {
                p1 = StartPin.DrawPosition + styles.PinInStyle.Image.Size * graph.ScaleFactor / 2;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * 0.5f;
            }
            else
            {
                IsPinIn = false;
                p1 = StartPin.DrawPosition + styles.PinOutStyle.Image.Size * graph.ScaleFactor / 2;
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * (-0.5f);
            }
            var p2 = new Vector2(p1.X - ControlLength, p1.Y);
            var p3 = new Vector2(p4.X + ControlLength, p4.Y);

            var cmdlist = new ImDrawList_PtrType(ImGuiAPI.GetWindowDrawList().NativePointer);
            if (HoverPin != null)
            {
                var min = HoverPin.DrawPosition;
                Vector2 max;
                if (IsPinIn)
                    max = min + styles.PinInStyle.Image.Size * graph.ScaleFactor;
                else
                    max = min + styles.PinOutStyle.Image.Size * graph.ScaleFactor;
                cmdlist.AddRect(ref min, ref max, styles.HighLightColor, 0, ImDrawCornerFlags_.ImDrawCornerFlags_All, 2);
            }
            cmdlist.AddBezierCurve(ref p1, ref p2, ref p3, ref p4, styles.LinkerColor, 3, 30);
        }
    }
}
