using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.NodeGraph
{
    public partial class PinLinker : IO.ISerializer
    {
        public class Pin
        {
            public Guid NodeId { get; set; }
            public string Name
            {
                get
                {
                    if (NodePin == null)
                        return "??";
                    return NodePin.Name;
                }
            }
            public NodePin NodePin { get; set;}
        }
        public NodeBase OutNode
        {
            get
            {
                if (Out == null)
                    return null;
                return Out.HostNode;
            }
        }
        public NodeBase InNode
        {
            get
            {
                if (In == null)
                    return null;
                return In.HostNode;
            }
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
        public unsafe void OnDraw(NodeGraph graph, NodeGraphStyles styles)
        {
            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());

            var p1 = OutPin.NodePin.DrawPosition + OutPin.NodePin.GetIconSize(styles, graph.ScaleFactor / 2);
            var p4 = InPin.NodePin.DrawPosition + InPin.NodePin.GetIconSize(styles, graph.ScaleFactor / 2);
            var delta = p4 - p1;
            delta.X = Math.Abs(delta.X);

            var p2 = new Vector2(p1.X + delta.X * 0.5f, p1.Y);
            var p3 = new Vector2(p4.X - delta.X * 0.5f, p4.Y);

            int num_segs = (int)(delta.Length() / styles.BezierPixelPerSegement + 1);

            var lineColor = styles.LinkerColor;
            if (Out.Link != null)
                lineColor = Out.Link.LineColor;
            else if (In.Link != null)
                lineColor = In.Link.LineColor;

            var thinkness = styles.LinkerThinkness;
            if (Out.Link != null)
                thinkness = Out.Link.LineThinkness;
            else if (In.Link != null)
                thinkness = In.Link.LineThinkness;
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, lineColor, thinkness, num_segs);
        }
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public Guid InNodeId { get; set; }
            [Rtti.Meta]
            public Guid OutNodeId { get; set; }
            [Rtti.Meta]
            public string InPinName { get; set; }
            [Rtti.Meta]
            public string OutPinName { get; set; }
        }
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as NodeGraph;
            if (graph == null)
                return;
            mGraph = graph;
        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        NodeGraph mGraph;
        [Rtti.Meta]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.InNodeId = InPin.NodeId;
                tmp.OutNodeId = OutPin.NodeId;
                tmp.InPinName = InPin.Name;
                tmp.OutPinName = OutPin.Name;
                return tmp;
            }
            set
            {
                InPin.NodeId = value.InNodeId;
                OutPin.NodeId = value.OutNodeId;

                if (mGraph != null)
                {
                    var inNode = mGraph.FindNode(InPin.NodeId);
                    if (inNode != null)
                        InPin.NodePin = inNode.FindPinIn(value.InPinName);

                    var outNode = mGraph.FindNode(OutPin.NodeId);
                    if (outNode != null)
                        OutPin.NodePin = outNode.FindPinOut(value.OutPinName);

                    if (InPin.NodePin == null || OutPin.NodePin == null)
                    {
                        mGraph.Linkers.Remove(this);
                    }
                    else
                    {
                        inNode.OnLoadLinker(this);
                        outNode.OnLoadLinker(this);
                    }
                }
            }
        }
    }
    public class LinkingLine
    {
        public NodePin StartPin { get; set; }
        public NodePin HoverPin { get; set; }
        public bool IsBlocking { get; set; } = false;
        private Vector2 BlockingEnd;
        public unsafe void OnDraw(NodeGraph graph, ref Vector2 endPoint, NodeGraphStyles styles = null)
        {
            if (IsBlocking == false)
            {
                BlockingEnd = endPoint;
            }
            var p4 = BlockingEnd;
            Vector2 p1;
            float ControlLength = 0;
            if(StartPin.GetType() == typeof(PinIn))
            {
                p1 = StartPin.DrawPosition + StartPin.GetIconSize(styles, graph.ScaleFactor / 2);
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * 0.5f;
            }
            else
            {
                p1 = StartPin.DrawPosition + StartPin.GetIconSize(styles, graph.ScaleFactor / 2);
                var delta = p4 - p1;
                delta.X = Math.Abs(delta.X);
                ControlLength = delta.X * (-0.5f);
            }
            var p2 = new Vector2(p1.X - ControlLength, p1.Y);
            var p3 = new Vector2(p4.X + ControlLength, p4.Y);

            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
            if (HoverPin != null)
            {
                var min = HoverPin.DrawPosition;
                Vector2 max;
                max = min + HoverPin.GetIconSize(styles, graph.ScaleFactor);
                cmdlist.AddRect(in min, in max, styles.HighLightColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
            }
            cmdlist.AddBezierCubic(in p1, in p2, in p3, in p4, styles.LinkerColor, 3, 30);
        }
    }
}
