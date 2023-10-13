using EngineNS.DesignMacross.Base.Graph;
using System.Collections.Generic;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Base.Description;
using System;

namespace EngineNS.DesignMacross.TimedStateMachine
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
    public class TtGraphElement_TimedStateTransition : IGraphElement, IContextMeunable, IDraggable, ILayoutable
    {
        public Guid Id { get; set; } = Guid.Empty;
        public TtTimeDurationSlilder TimeDurationSlilder { get; set; }
        public IGraphElement From { get; set; } = null;
        public IGraphElement To { get; set; } = null;

        public string Name { get; set; }
        public Vector2 Location { get => Description.Location; set => Description.Location = value; }

        public Vector2 AbsLocation { get => TtGraphMisc.CalculateAbsLocation(this); }

        public IGraphElement Parent { get; set; } = null;
        public IDescription Description { get; set; } = null;
        public FMargin Margin { get; set; } = FMargin.Default;
        public SizeF Size { get; set; } = SizeF.Empty;
        public ImDrawFlags_ TimeDurationBarRoundCorner = ImDrawFlags_.ImDrawFlags_Closed;
        public float TimeDurationBarRounding = 10;
        public TtPopupMenu PopupMenu { get; set; } = null;
        public TtGraphElement_TimedStateTransition(IDescription description)
        {
            Id = description.Id;
            Description = description;
        }
        public void Construct()
        {
        }
        public bool CanDrag()
        {
            return false;
        }
        public void OnDragging(Vector2 delta)
        {

        }
        public void DrawContextMenu(ref FGraphElementRenderingContext context)
        {
           
        }

        public bool HitCheck(Vector2 pos)
        {
            return true;
        }

        public SizeF Measuring(SizeF availableSize)
        {
            return new SizeF(availableSize.Width, 10);
        }
        public SizeF Arranging(Rect finalRect)
        {
            Location = finalRect.Location;
            return new SizeF();
        }

        public void OnSelected()
        {
           
        }

        public void OnUnSelected()
        {
           
        }

        public void OpenContextMeun()
        {
        }

        public void UpdateContextMenu(ref FGraphElementRenderingContext context)
        {
            //PopupMenu.StringId = Name + "_ContextMenu";
            //PopupMenu.Reset();
            //var parentMenu = PopupMenu.Menu;
            //var editorInteroperation = context.EditorInteroperation;
            //parentMenu.AddMenuItem("Open Transition Graph", null, (Bricks.NodeGraph.UMenuItem item, object sender) =>
            //{
            //    editorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(Description);
            //});
        }
    }

    public class TtGraphElement_TimedStateTransitionRender : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            TtGraphElement_TimedStateTransition transitionElement = renderableElement as TtGraphElement_TimedStateTransition;
            var cmdlist = ImGuiAPI.GetWindowDrawList();
            var size = transitionElement.From.Size;
            var nodeStart = context.ViewPortTransform(transitionElement.AbsLocation);
            var nodeEnd = context.ViewPortTransform(transitionElement.AbsLocation + new Vector2(size.Width, 10));

            var clolr = new Color4f(233f / 255, 234 / 255f, 236f / 255);
            cmdlist.AddRectFilled(nodeStart, nodeEnd, ImGuiAPI.ColorConvertFloat4ToU32(clolr), transitionElement.TimeDurationBarRounding, transitionElement.TimeDurationBarRoundCorner);

            if(ImGuiAPI.IsMouseDoubleClickedInRectInCurrentWindow(nodeStart, nodeEnd, ImGuiMouseButton_.ImGuiMouseButton_Left, true))
            {
                context.EditorInteroperation.GraphEditPanel.ActiveGraphNavigatedPanel.OpenSubGraph(transitionElement.Description);
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
