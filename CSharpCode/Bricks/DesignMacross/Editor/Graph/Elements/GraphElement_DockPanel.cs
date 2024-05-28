using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Editor
{
    public enum EDockPosition
    {
        Left,
        Right,
        Top,
        Down,
    }
    [ImGuiElementRender(typeof(TtGraphElementRender_DockPanel))]
    public class TtGraphElement_DockPanel : TtWidgetGraphElement, ILayoutable
    {
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0);
        public Dictionary<EDockPosition, IGraphElement> Children { get; set; } = new();
        public FMargin Margin { get; set; } = FMargin.Default;
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Stretch;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Stretch;

        public void AddElement(EDockPosition dockPosition, IGraphElement element)
        {
            Children.Add(dockPosition, element);
            element.Parent = this;
        }
        public void RemoveElement(EDockPosition dockPosition)
        {
            Children[dockPosition].Parent = null;
            Children.Remove(dockPosition);
        }

        public void Clear()
        {
            foreach (var element in Children)
            {
                element.Value.Parent = null;
            }
            Children.Clear();
        }

        public override bool CanDrag()
        {
            return false;
        }

        public override bool HitCheck(Vector2 pos)
        {
            return false;
        }

        public override void OnDragging(Vector2 delta)
        {

        }


        public override void OnSelected(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnUnSelected()
        {

        }
        public override void OnMouseLeave(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnMouseLeftButtonDown(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnMouseLeftButtonUp(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnMouseOver(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnMouseRightButtonDown(ref FGraphElementRenderingContext context)
        {

        }

        public override void OnMouseRightButtonUp(ref FGraphElementRenderingContext context)
        {

        }

        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public SizeF Measuring(SizeF availableSize)
        {
            ChildrenMeasuringSize.Clear();
            Dictionary<EDockPosition, Rect> ChildrenRect = new Dictionary<EDockPosition, Rect>();
            foreach (var element in Children)
            {
                if (element.Value is ILayoutable layoutable)
                {
                    var childDesireSize = layoutable.Measuring(availableSize);

                    if (ChildrenMeasuringSize.ContainsKey(layoutable))
                    {
                        ChildrenMeasuringSize[layoutable] = childDesireSize;
                    }
                    else
                    {
                        ChildrenMeasuringSize.Add(layoutable, childDesireSize);
                    }
                    var startLocation = Vector2.Zero;
                    switch (element.Key)
                    {
                        case EDockPosition.Left:
                            {

                            }
                            break;
                        case EDockPosition.Right:
                            {
                                startLocation.X = availableSize.Width - childDesireSize.Width;
                            }
                            break;
                        case EDockPosition.Top:
                            {

                            }
                            break;
                        case EDockPosition.Down:
                            {
                                startLocation.Y = availableSize.Height - childDesireSize.Height;
                            }
                            break;
                    }
                    Rect rect = new Rect(startLocation, childDesireSize);
                    ChildrenRect.Add(element.Key, rect);
                }
            }
            Rect bound = new Rect();
            foreach (var childRect in ChildrenRect)
            {
                if (bound.X > childRect.Value.X)
                {
                    bound.X = childRect.Value.X;
                }
                if (bound.Y > childRect.Value.Y)
                {
                    bound.Y = childRect.Value.Y;
                }
                switch (childRect.Key)
                {
                    case EDockPosition.Left:
                    case EDockPosition.Right:
                        {
                            bound.Width += childRect.Value.Width;
                            bound.Height = Math.Max(childRect.Value.Height, bound.Height);
                        }
                        break;
                    case EDockPosition.Top:
                    case EDockPosition.Down:
                        {
                            bound.Width += Math.Max(childRect.Value.Width, bound.Width);
                            bound.Height += childRect.Value.Height;
                        }
                        break;
                }
            }
            return new SizeF(bound.Width + Margin.Left + Margin.Right, bound.Height + Margin.Top + Margin.Bottom);
        }


        public SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            Location = finalRect.Location + new Vector2(Margin.Left, Margin.Top);

            foreach (var element in Children)
            {
                if (element.Value is ILayoutable layoutableChild)
                {
                    var startLocation = Vector2.Zero;
                    switch (element.Key)
                    {
                        case EDockPosition.Left:
                            {

                            }
                            break;
                        case EDockPosition.Right:
                            {
                                startLocation.X = finalRect.Width - ChildrenMeasuringSize[layoutableChild].Width;
                            }
                            break;
                        case EDockPosition.Top:
                            {

                            }
                            break;
                        case EDockPosition.Down:
                            {
                                startLocation.Y = finalRect.Height - ChildrenMeasuringSize[layoutableChild].Height;
                            }
                            break;
                    }
                    Rect rect = new Rect(startLocation, ChildrenMeasuringSize[layoutableChild]);
                    layoutableChild.Arranging(rect);
                }
            }

            return finalRect.Size;
        }
    }

    public class TtGraphElementRender_DockPanel : IGraphElementRender
    {
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            TtGraphElement_DockPanel dockPanel = renderableElement as TtGraphElement_DockPanel;
            var cmd = ImGuiAPI.GetWindowDrawList();
            foreach (var child in dockPanel.Children)
            {
                var render = TtElementRenderDevice.CreateGraphElementRender(child.Value);
                if (render != null)
                {
                    render.Draw(child.Value, ref context);
                }
            }
        }
    }
}
