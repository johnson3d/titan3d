using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;

namespace EngineNS.DesignMacross.Editor
{
    public enum EOrientation
    {
        Horizontal,
        Vertical,
    }
    public struct TtDesignStackPanelStyle
    {
        public TtDesignStackPanelStyle()
        {

        }


    }
    [ImGuiElementRender(typeof(TtGraphElementRender_StackPanel))]
    public class TtGraphElement_StackPanel : TtWidgetGraphElement, ILayoutable
    {
        public EOrientation Orientation { get; set; } = EOrientation.Vertical;
        public Color4f BackgroundColor { get; set; } = new Color4f(0, 0, 0);
        public List<IGraphElement> Children { get; set; } = new List<IGraphElement>();
        public FMargin Margin { get; set; } = FMargin.Default;
        public void AddElement(IGraphElement element)
        {
            Children.Add(element);
            element.Parent = this;
        }
        public void InsertElement(int index, IGraphElement element)
        {
            Children.Insert(index, element);
            element.Parent = this;
        }
        public void RemoveElement(IGraphElement element)
        {
            element.Parent = null;
            Children.Remove(element);
        }

        public void Clear()
        {
            foreach (var element in Children)
            {
                element.Parent = null;
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

        private Dictionary<ILayoutable, SizeF> ChildrenMeasuringSize = new Dictionary<ILayoutable, SizeF>();
        public EHorizontalAlignment HorizontalAlignment { get; set; } = EHorizontalAlignment.Left;
        public EVerticalAlignment VerticalAlignment { get; set; } = EVerticalAlignment.Top;
        public SizeF Measuring(SizeF availableSize)
        {
            ChildrenMeasuringSize.Clear();
            var desiredSize = new SizeF();
            foreach (var element in Children)
            {
                if (element is ILayoutable layoutable)
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
                    if (Orientation == EOrientation.Vertical)
                    {
                        desiredSize.Height += childDesireSize.Height;
                        if (desiredSize.Width < childDesireSize.Width)
                        {
                            desiredSize.Width = childDesireSize.Width;
                        }
                    }
                    else
                    {
                        desiredSize.Width += childDesireSize.Width;
                        if (desiredSize.Height < childDesireSize.Height)
                        {
                            desiredSize.Height = childDesireSize.Height;
                        }
                    }
                }
            }

            return new SizeF(desiredSize.Width + Margin.Left + Margin.Right, desiredSize.Height + Margin.Top + Margin.Bottom);
        }
        Vector2 GetStartLocation(Rect finalRect, int childIndex)
        {
            if (Orientation == EOrientation.Vertical)
            {
                Vector2 startLocation = Vector2.Zero;
                for (int i = 0; i < childIndex; ++i)
                {
                    var layoutablechild = Children[i] as ILayoutable;
                    startLocation.Y += ChildrenMeasuringSize[layoutablechild].Height;
                }
                return startLocation;
            }
            else if (Orientation == EOrientation.Horizontal)
            {
                Vector2 startLocation = Vector2.Zero;
                for (int i = 0; i < childIndex; ++i)
                {
                    var layoutablechild = Children[i] as ILayoutable;
                    startLocation.X += ChildrenMeasuringSize[layoutablechild].Width;
                }
                return startLocation;
            }
            return Vector2.Zero;
        }

        public SizeF Arranging(Rect finalRect)
        {
            Size = new SizeF(finalRect.Width, finalRect.Height);
            Location = finalRect.Location + new Vector2(Margin.Left, Margin.Top);

            if (Orientation == EOrientation.Vertical)
            {
                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    if (child is ILayoutable layoutableChild)
                    {
                        var childMeasuringSize = ChildrenMeasuringSize[layoutableChild];

                        var startLocation = GetStartLocation(finalRect, i);
                        switch (HorizontalAlignment)
                        {
                            case EHorizontalAlignment.Right:
                                startLocation.X = finalRect.Width - childMeasuringSize.Width;
                                break;
                            case EHorizontalAlignment.Center:
                                startLocation.X = (finalRect.Width - childMeasuringSize.Width) * 0.5f;
                                break;
                            case EHorizontalAlignment.Left:
                                startLocation.X = 0;
                                break;
                            case EHorizontalAlignment.Stretch:
                                childMeasuringSize.Width = finalRect.Width;
                                break;
                        }
                        Rect rect = new Rect(startLocation, childMeasuringSize);
                        layoutableChild.Arranging(rect);

                    }
                }
            }
            else if (Orientation == EOrientation.Horizontal)
            {
                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    if (child is ILayoutable layoutableChild)
                    {
                        var childMeasuringSize = ChildrenMeasuringSize[layoutableChild];
                        var startLocation = GetStartLocation(finalRect, i);
                        switch (VerticalAlignment)
                        {
                            case EVerticalAlignment.Bottom:
                                startLocation.Y = finalRect.Height - childMeasuringSize.Height;
                                break;
                            case EVerticalAlignment.Center:
                                startLocation.Y = (finalRect.Height - childMeasuringSize.Height) * 0.5f;
                                break;
                            case EVerticalAlignment.Top:
                                startLocation.Y = 0;
                                break;
                            case EVerticalAlignment.Stretch:
                                childMeasuringSize.Height = finalRect.Height;
                                break;
                        }
                        Rect rect = new Rect(startLocation, childMeasuringSize);
                        layoutableChild.Arranging(rect);
                    }
                }
            }
            return finalRect.Size;
        }
    }

    public class TtGraphElementRender_StackPanel : IGraphElementRender
    {
        public TtGraphElement_StackPanel stackPanel { get; set; } = null;
        public void Draw(IRenderableElement renderableElement, ref FGraphElementRenderingContext context)
        {
            TtGraphElement_StackPanel stackPanel = renderableElement as TtGraphElement_StackPanel;
            var cmd = ImGuiAPI.GetWindowDrawList();
            foreach (var child in stackPanel.Children)
            {
                var render = TtElementRenderDevice.CreateGraphElementRender(child);
                if (render != null)
                {
                    render.Draw(child, ref context);
                }
            }
        }
    }
}
