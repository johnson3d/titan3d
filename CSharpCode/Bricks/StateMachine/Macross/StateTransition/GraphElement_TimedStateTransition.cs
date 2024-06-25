using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.EGui.Controls;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.Bricks.StateMachine.Macross.SubState;

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

        public override bool HitCheck(Vector2 pos)
        {
            Rect rect = new Rect(AbsLocation, Size);
            //冗余一点
            Rect mouseRect = new Rect(pos - Vector2.One, new SizeF(1.0f, 1.0f));
            return rect.IntersectsWith(mouseRect);
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
            var nodeStart = context.ViewPortTransform(transitionElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(transitionElement.AbsLocation + new Vector2(size.Width, 10));

            var clolr = new Color4f(233f / 255, 234 / 255f, 236f / 255);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(clolr), transitionElement.TimeDurationBarRounding, transitionElement.TimeDurationBarRoundCorner);

            if(ImGuiAPI.IsMouseDoubleClickedInRectInCurrentWindow(nodeStart, nodeEnd, ImGuiMouseButton_.ImGuiMouseButton_Left, true))
            {
                context.EditorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(transitionElement.TimedStateTransition.CheckConditionMethodDescription);
            }

            if (transitionElement.From is IStateTransitionInitial initiable && transitionElement.To is IStateTransitionAcceptable acceptable)
            {
                var lines = ManhattanConnectionRouter.GetLines((transitionElement.AbsLocation * 2 + new Vector2(size.Width, 10)) / 2, ELineDirection.South, acceptable.GetTransitionLinkPosition(ELineDirection.East), ELineDirection.East, null);
                foreach(var line in lines)
                {
                    cmdlist.AddLine(context.ViewPortTransform(line.Start), context.ViewPortTransform(line.End), ImGuiAPI.ColorConvertFloat4ToU32(clolr), 5);
                }
            }
        }
    }
}
