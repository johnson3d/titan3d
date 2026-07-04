using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.EGui.Controls;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.Macross.SubState;
using Sprache;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    //可发起 Transition 的节点
    public interface IStateTransitionInitial 
    {

    }

    //可接受 Transition 的节点
    public interface IStateTransitionAcceptable
    {
        public Vector2 GetTransitionLinkPosition(ELineDirection lineDirection);
    }
    public enum ELineDirection
    {
        None,
        East,
        West,
        South,
        North,
    }
    public class AStarPoint : IEquatable<AStarPoint>
    {
        public AStarPoint Parent;
        public Vector2 Position;
        public float F;
        public float G;
        public float H;

        public bool Equals(AStarPoint other)
        {
            return other.Position == Position;
        }
        public static bool operator ==(in AStarPoint left, in AStarPoint right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(in AStarPoint left, in AStarPoint right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj is AStarPoint && Equals((AStarPoint)obj);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public struct AStarCollision
    {
        public Rect Bounds;
        public bool IsCollide(AStarPoint point)
        {
            return Bounds.Contains(point.Position.X, point.Position.Y);
        }
    }
    public class ManhattanAStart
    {
        AStarPoint StartPoint;
        AStarPoint EndPoint;
        List<AStarCollision> Collisions = new List<AStarCollision>();
        System.Collections.Generic.PriorityQueue<AStarPoint, float> OpenQueue = new();
        List<AStarPoint> CloseList = new List<AStarPoint>();
        public List<Vector2> GetPath(Vector2 startPosition, ELineDirection startDirection, Vector2 endPosition, ELineDirection endDirection, List<Rect> collisioins)
        {
            OpenQueue.Clear();
            CloseList.Clear();
            Collisions.Clear();
            StartPoint = CreatePoint(startPosition + new Vector2(0, 50), startPosition + new Vector2(0, 50), endPosition);
            EndPoint = CreatePoint(endPosition + new Vector2(-50, 0), startPosition, endPosition + new Vector2(-50, 0));
            foreach(var collision in collisioins)
            {
                Collisions.Add(new AStarCollision() { Bounds = collision });
            }

            OpenQueue.Enqueue(StartPoint, StartPoint.F);
            while(OpenQueue.Count > 0)
            {
                var current = OpenQueue.Dequeue();
                CloseList.Add(current);
                if(current.Position == EndPoint.Position)
                {

                }
                var neighbors = GetNeighbors(current);
                foreach(var next in neighbors)
                {
            
                    if(IsCollided(next) || CloseList.Contains(next))
                    {
                        continue;
                    }
                    if(NeedAddToOpenList(next))
                    {
                        OpenQueue.Enqueue(next, next.F);
                    }
                }
            }

            return null;
        }
        public bool NeedAddToOpenList(AStarPoint point)
        {
            foreach(var item in OpenQueue.UnorderedItems)
            {
                if(point == item.Element && point.G > item.Element.G)
                {
                    return false;
                }
            }
            return true;
        }
        public AStarPoint CreatePoint(Vector2 position, Vector2 startPosition, Vector2 endPosition, AStarPoint parent = null, float g = 0)
        {
            AStarPoint point = new AStarPoint();
            point.Parent = parent;
            point.Position = position;
            point.G = g;
            point.H = GetManhattanLength(position, endPosition);
            point.F = point.G + point.H;
            return point;
        }
        public float GetManhattanLength(Vector2 a, Vector2 b)
        {
            return MathF.Sqrt(Math.Abs(a.X - b.X) * Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) * Math.Abs(a.Y - b.Y));
        }
        List<AStarPoint> GetNeighbors(AStarPoint point)
        {
            List<AStarPoint> neighbors = new List<AStarPoint>();
            neighbors.Add(CreatePoint(point.Position + new Vector2(1, 0), StartPoint.Position, EndPoint.Position, point, point.G + 10));
            neighbors.Add(CreatePoint(point.Position + new Vector2(-1, 0), StartPoint.Position, EndPoint.Position, point, point.G + 10));
            neighbors.Add(CreatePoint(point.Position + new Vector2(0, 1), StartPoint.Position, EndPoint.Position, point, point.G + 10));
            neighbors.Add(CreatePoint(point.Position + new Vector2(0, -1), StartPoint.Position, EndPoint.Position, point, point.G + 10));
            return neighbors;
        }
        bool IsCollided(AStarPoint point)
        {
            bool isCollide = false;
            foreach (var collision in Collisions)
            {
                if (collision.IsCollide(point))
                {
                    isCollide = true;
                }
            }
            return isCollide;
        }
    }
    public class ManhattanConnectionRouter
    {
        //static float Threshold = 10;
        public static List<(Vector2 Start, Vector2 End)> GetLines(Vector2 startPosition, ELineDirection startDirection,Vector2 endPosition, ELineDirection endDirection, List<Rect> collisioins)
        {
            List<(Vector2 Start, Vector2 End)> lines = new List<(Vector2 Start, Vector2 End)>();
            //状态连线
            if (startDirection == ELineDirection.South)
            {
                var firstCorner = new Vector2(startPosition.X, endPosition.Y);
                lines.Add((startPosition, firstCorner));
                lines.Add((firstCorner, endPosition));
            }
            return lines;
        }
        static ELineDirection GetLineDirection(Vector2 start, Vector2 end)
        {
            System.Diagnostics.Debug.Assert((start.X == end.X) || (start.Y == end.Y));
            if((start.X == end.X) && (start.Y == end.Y))
            {
                return ELineDirection.None;
            }
            if(start.X == end.X)
            {
                if(start.Y > end.Y)
                {
                    return ELineDirection.South;
                }
                else
                {
                    return ELineDirection.North;
                }
            }
            if(start.Y == end.Y)
            {
                if(start.X > end.X)
                {
                    return ELineDirection.East;
                }
                else
                {
                    return ELineDirection.West;
                }
            }
            return ELineDirection.None;
        }
    }
    public class TtTimeDurationSlilder
    {

    }

    [ImGuiElementRender(typeof(TtGraphElement_TimedStateTransitionRender))]
    public class TtGraphElement_TimedStateTransition : TtDescriptionGraphElement
    {
        public TtTimedStateTransitionClassDescription TimedStateTransition { get => Description as TtTimedStateTransitionClassDescription; }
        public TtTimeDurationSlilder TimeDurationSlilder { get; set; }
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

        public ImDrawFlags_ TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_Closed;
        public float TimeDurationBarRounding = 10;
        public TtGraphElement_TimedStateTransition(IDescription description, IGraphElementStyle style) : base(description, style)
        {
            Size = SizeF.Empty;
        }


        public override SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(availableSize.Width, 10);
        }
        public override SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, 10);
            Location = finalRect.Location;
            return new SizeF();
        }

        public override void ConstructContextMenu(ref FGraphElementRenderingContext context, TtPopupMenu popupMenu)
        {
            popupMenu.bHasSearchBox = false;
            var parentMenu = popupMenu.Menu;
            var cmdHistory = context.CommandHistory;
            parentMenu.AddMenuSeparator("GENERAL");

            if(TimedStateTransition.Parent is TtTimedCompoundStateEntryClassDescription entry)
            {
                parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                {
                    cmdHistory.CreateAndExtuteCommand("Break Transition",
                        (data) => { entry.RemoveTransition(TimedStateTransition); },
                        (data) => { entry.AddTransition(TimedStateTransition); }
                        );
                });
                
            }

            if (TimedStateTransition.Parent is TtTimedSubStateClassDescription subState)
            {
                parentMenu.AddMenuItem("Break Link", null, (TtMenuItem item, object sender) =>
                {
                    cmdHistory.CreateAndExtuteCommand("Break Transition",
                        (data) => { subState.RemoveTransition(TimedStateTransition); },
                        (data) => { subState.AddTransition(TimedStateTransition); }
                        );
                });

            }
            var editorInteroperation = context.EditorInteroperation;
            parentMenu.AddMenuItem("Open Transition Graph", null, (TtMenuItem item, object sender) =>
            {
                editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(TimedStateTransition.CheckConditionMethodDescription);
            });
        }

        public override void ConstructElements(ref FGraphElementRenderingContext context)
        {
            From = context.DescriptionsElement[TimedStateTransition.FromId];
            To = context.DescriptionsElement[TimedStateTransition.ToId];
            base.ConstructElements(ref context);
        }

        public override bool HitCheck(ref FMouseEventContext context)
        {
            var renderingContext = context.GraphElementRenderingContext;
            var start = renderingContext.ViewportTransform(AbsLocation);
            var end = renderingContext.ViewportTransform(AbsLocation + new Vector2(Size.Width, Size.Height));
            Rect rect = new Rect(start.X, start.Y, end.X - start.X, end.Y - start.Y);
            //冗余一点
            //Rect mouseRect = new Rect(context.MouseAbsPos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(context.MouseAbsRect);
        }
    }

    public class TtGraphElement_TimedStateTransitionRender : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            TtGraphElement_TimedStateTransition transitionElement = renderableElement as TtGraphElement_TimedStateTransition;
            transitionElement.ConstructElements(ref context);
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var size = transitionElement.From.Size;
            var nodeStart = context.ViewportTransform(transitionElement.AbsLocation);
            var nodeEnd = context.ViewportTransform(transitionElement.AbsLocation + new Vector2(size.Width, 10));

            var clolr = new Color4f(233f / 255, 234 / 255f, 236f / 255);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(clolr), transitionElement.TimeDurationBarRounding, transitionElement.TimeDurationBarRoundCorner);

            if(ImGuiAPI.IsMouseDoubleClickedInRectInCurrentWindow(nodeStart, nodeEnd, ImGuiMouseButton_.ImGuiMouseButton_Left, true))
            {
                context.EditorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(transitionElement.TimedStateTransition.CheckConditionMethodDescription);
            }

            if (transitionElement.From is IStateTransitionInitial initiable && transitionElement.To is IStateTransitionAcceptable acceptable)
            {
                List<Rect> rects = new List<Rect>();
                foreach(var element in context.DesignedGraph.Elements)
                {
                    Rect rect = new Rect();
                    rect.Location = element.Location;
                    rect.Size = element.Size;
                    rects.Add(rect);
                }
                var lines = ManhattanConnectionRouter.GetLines((transitionElement.AbsLocation * 2 + new Vector2(size.Width, 10)) / 2, ELineDirection.South, acceptable.GetTransitionLinkPosition(ELineDirection.East), ELineDirection.East, rects);
                foreach(var line in lines)
                {
                    cmdlist.AddLine(context.ViewportTransform(line.Start), context.ViewportTransform(line.End), ImGuiAPI.ColorConvertFloat4ToU32(clolr), TtDesignMacrossGraphStyles.LineNormalThickness * context.Camera.Scale);
                }
            }
        }
    }
}
