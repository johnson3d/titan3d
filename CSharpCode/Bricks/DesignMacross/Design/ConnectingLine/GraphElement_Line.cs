using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Bricks.NodeGraph;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    public class TtGraphElement_Pin : TtDescriptionGraphElement
    {
        public override FMargin Margin { get; set; } = new FMargin(2, 2, 2, 2);
        public TtGraphElement_TextBlock NameTextBlock = new TtGraphElement_TextBlock();
        public TtGraphElement_StackPanel ElementContainer = new();
        public TtGraphElement_Icon Icon = new();
        public float Rounding { get; set; } = 5;
        public Color4f BackgroundColor
        {
            get => Style.BackgroundColor;
            set => Style.BackgroundColor = value;
        }
        public TtGraphElement_Pin(IDescription description, IGraphElementStyle style) : base(description, style)
        {

        }
        public override SizeF Measuring(SizeF availableSize)
        {
            return SizeF.Empty;
        }

        public override SizeF Arranging(Rect finalRect)
        {
            return SizeF.Empty;
        }
    }
    public class TtGraphElement_Line : TtDescriptionGraphElement
    {
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

        public TtGraphElement_Line(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }

        public override SizeF Measuring(SizeF availableSize)
        {
            return SizeF.Empty;
        }

        public override SizeF Arranging(Rect finalRect)
        {
            return SizeF.Empty;
        }

        public override bool HitCheck(ref FMouseEventContext context)
        {
            var elementRenderingContext = context.GraphElementRenderingContext;
            var fromPin = From as TtGraphElement_Pin;
            var toPin = To as TtGraphElement_Pin;
            if (fromPin == null || toPin == null)
            {
                return false;
            }
            var nodeStart = elementRenderingContext.ViewportTransform(fromPin.Icon.AbsCenter);
            var nodeEnd = elementRenderingContext.ViewportTransform(toPin.Icon.AbsCenter);
            var p1 = nodeStart;
            var p4 = nodeEnd;
            var delta = p4 - p1;
            var ctDelta = Math.Min(TtDesignMacrossGraphStyles.LineBezierMaxDelta, Math.Max(TtDesignMacrossGraphStyles.LineBezierMinDelta, Math.Max(Math.Abs(delta.X), Math.Abs(delta.Y)) * 0.5f));

            var p2 = new Vector2(p1.X + ctDelta, p1.Y);
            var p3 = new Vector2(p4.X - ctDelta, p4.Y);

            if (IsMouseHoveringBezierCubic(ref context, p1, p2, p3, p4, 8))
            {
                return true;
            }
            return false;
        }
        public float PointToSegmentDistance(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = point - a;
            float t = Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab);
            t = Math.Clamp(t, 0f, 1f);
            Vector2 closest = a + ab * t;
            return Vector2.Distance(point, closest);
        }

        public bool IsMouseHoveringBezierCubic(ref FMouseEventContext context, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float hoverThreshold, int numSegments = 15)
        {
            Vector2 mousePos = context.MouseAbsPos;

            // 采样点
            var points = new Vector2[numSegments + 1];
            for (int i = 0; i <= numSegments; i++)
            {
                float t = i / (float)numSegments;
                float u = 1f - t;
                float w1 = u * u * u;
                float w2 = 3 * u * u * t;
                float w3 = 3 * u * t * t;
                float w4 = t * t * t;
                points[i] = p1 * w1 + p2 * w2 + p3 * w3 + p4 * w4;
            }

            for (int i = 0; i < numSegments; i++)
            {
                float dist = PointToSegmentDistance(mousePos, points[i], points[i + 1]);
                if (dist < hoverThreshold)
                    return true;
            }
            return false;
        }
        public override void OnSelected(ref FMouseEventContext context)
        {
            
        }
        public override void OnUnSelected(ref FMouseEventContext context)
        {
            
        }

        public override void OnMouseOver(ref FMouseEventContext context)
        {
            context.GraphElementRenderingContext.DesignedGraph.HighLightLine(this);
        }
        public override void OnMouseLeave(ref FMouseEventContext context)
        {
            context.GraphElementRenderingContext.DesignedGraph.UnHighLightLine(this);
        }
        
    }

    
}
