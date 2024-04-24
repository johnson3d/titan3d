using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;

namespace EngineNS.UI.Controls.Containers
{
    public enum ELayout_Orientation
    {
        Horizontal,
        Vertical,
    }

    public enum EAlignment_Horizontal
    {
        Left,
        Center,
        Stretch,
        Right,
    }
    public enum EAlignment_Vertical
    {
        Top,
        Center,
        Stretch,
        Bottom,
    }
    [Editor_UIControl("Container.StackPanel", "StackPanel", "")]
    public partial class TtStackPanel : TtContainer
    {
        [Bind.AttachedProperty(Name = "HorizontalAlignment", Category = "Layout(StackPanel)")]
        static void OnChildHorizontalAlignmentChanged(IBindableObject element, TtBindableProperty property, EAlignment_Horizontal value)
        {
            var ui = element as TtUIElement;
            if(ui != null)
            {
                var canvas = VisualTreeHelper.GetParent(ui) as TtStackPanel;
                canvas?.InvalidateArrange();
            }
        }
        [Bind.AttachedProperty(Name = "VerticalAlignment", Category = "Layout(StackPanel)")]
        static void OnChildVerticalAlignmentChanged(IBindableObject element, TtBindableProperty property, EAlignment_Vertical value)
        {
            var ui = element as TtUIElement;
            if(ui != null)
            {
                var canvas = VisualTreeHelper.GetParent(ui) as TtStackPanel;
                canvas?.InvalidateArrange();
            }
        }
        ELayout_Orientation mOrientation = ELayout_Orientation.Horizontal;
        [Bind.BindProperty]
        [Meta]
        public ELayout_Orientation Orientation
        {
            get => mOrientation;
            set
            {
                if(mOrientation == value)
                    return;
                OnValueChange(value, mOrientation);
                mOrientation = value;
                InvalidateArrange();
            }
        }

        public TtStackPanel()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }
        public TtStackPanel(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            var borderSize = new SizeF(mBorderThickness.Left + mBorderThickness.Right, mBorderThickness.Top + mBorderThickness.Bottom);
            var paddingSize = new SizeF(mPadding.Left + mPadding.Right, mPadding.Top + mPadding.Bottom);
            var retVal = new SizeF(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            if (visualChildrenCount > 0)
            {
                var childAvailableSize = new SizeF(
                    MathF.Max(0.0f, availableSize.Width - retVal.Width),
                    MathF.Max(0.0f, availableSize.Height - retVal.Height));
                var tempSize = SizeF.Empty;
                for (int i = 0; i < visualChildrenCount; i++)
                {
                    var childUI = VisualTreeHelper.GetChild(this, i);
                    childUI.Measure(in childAvailableSize);
                    var childSize = childUI.DesiredSize;
                    tempSize.Width = Math.Max(tempSize.Width, childSize.Width);
                    tempSize.Height = Math.Max(tempSize.Height, childSize.Height);
                    if (Orientation == ELayout_Orientation.Vertical)
                    {
                        retVal.Height += tempSize.Height;
                        retVal.Width = MathF.Max(retVal.Width, tempSize.Width);
                    }
                    else
                    {
                        retVal.Width += tempSize.Width;
                        retVal.Height = MathF.Max(retVal.Height, tempSize.Height);
                    }
                }
                return retVal;
            }

            return retVal;
        }
        protected override void ArrangeOverride(in RectangleF arrangeSize)
        {
            var borderThick = BorderThickness;
            var innerRect = new RectangleF(
                arrangeSize.Left + borderThick.Left,
                arrangeSize.Top + borderThick.Top,
                Math.Max(0.0f, arrangeSize.Width - borderThick.Left - borderThick.Right),
                Math.Max(0.0f, arrangeSize.Height - borderThick.Top - borderThick.Bottom));

            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            float accumulateX = innerRect.X, accumulateY = innerRect.Y;
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = RectangleF.Empty;
                if (Orientation == ELayout_Orientation.Vertical)
                {
                    EAlignment_Horizontal alignment = TtStackPanel.GetHorizontalAlignment(childUI);
                    switch (alignment)
                    {
                        case EAlignment_Horizontal.Left:
                        {
                            childFinalRect.X = innerRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        } 
                            break;
                        case EAlignment_Horizontal.Center:
                        {
                            childFinalRect.X = (innerRect.Width - childDesiredSize.Width) * 0.5f + innerRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        } 
                            break;

                        case EAlignment_Horizontal.Stretch:
                        {
                            childFinalRect.X = innerRect.X;
                            childFinalRect.Width = innerRect.Width;
                        }
                            break;
                        case EAlignment_Horizontal.Right:
                        {
                            childFinalRect.X = innerRect.Width - childDesiredSize.Width + innerRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    childFinalRect.Y = accumulateY;
                    childFinalRect.Height = childDesiredSize.Height;
                    accumulateY += childFinalRect.Height;
                }
                else
                {
                    EAlignment_Vertical alignment = TtStackPanel.GetVerticalAlignment(childUI);
                    switch (alignment)
                    {
                        case EAlignment_Vertical.Top:
                        {
                            childFinalRect.Y = innerRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;        
                        }
                            break;
                        case EAlignment_Vertical.Center:
                        {
                            childFinalRect.Y = (innerRect.Height - childDesiredSize.Height) * 0.5f + innerRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;        
                        }
                            break;
                        case EAlignment_Vertical.Stretch:
                        {
                            childFinalRect.Y = innerRect.Y;
                            childFinalRect.Height = innerRect.Height;
                        }
                            break;
                        case EAlignment_Vertical.Bottom:
                        {
                            childFinalRect.Y = innerRect.Height - childDesiredSize.Height + innerRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;    
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    childFinalRect.X = accumulateX;
                    childFinalRect.Width = childDesiredSize.Width;
                    
                    accumulateX += childFinalRect.Width;
                }
                childUI.Arrange(in childFinalRect);
            }
        }
        public override bool IsReadyToDraw()
        {
            if (!base.IsReadyToDraw())
                return false;

            return ((Background != null) ? Background.IsReadyToDraw() : true) &&
                   ((BorderBrush != null) ? BorderBrush.IsReadyToDraw() : true);
        }
        public override void Draw(TtCanvas canvas, TtCanvasDrawBatch batch)
        {
            if (Background != null)
            {
                var rect = new RectangleF(mCurFinalRect.X + mBorderThickness.Left,
                                          mCurFinalRect.Y + mBorderThickness.Top,
                                          mCurFinalRect.Width - mBorderThickness.Left - mBorderThickness.Right,
                                          mCurFinalRect.Height - mBorderThickness.Top - mBorderThickness.Bottom);
                Background.Draw(this, in mDesignClipRect, in rect, batch, in Vector4.Zero);
            }
            if (BorderBrush != null && !BorderThickness.IsEmpty())
                BorderBrush.Draw(this, in mDesignClipRect, in mCurFinalRect, batch, in Vector4.Zero, in mBorderThickness);

            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.Draw(canvas, batch);
            }
        }
    }
}
