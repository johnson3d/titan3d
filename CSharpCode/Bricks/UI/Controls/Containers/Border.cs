using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls.Containers
{
    [Editor_UIControl("Container.Border", "Border", "")]
    public partial class TtBorder : TtContainer
    {
        [AttachedProperty(Name = "HorizontalAlignment", Category = "Layout(Border)")]
        static void OnChildHorizontalAlignmentChanged(IBindableObject element, TtBindableProperty property, HorizontalAlignment value)
        {
            var ui = element as TtUIElement;
            if(ui != null)
            {
                var border = VisualTreeHelper.GetParent(ui) as TtBorder;
                border?.InvalidateMeasure();
            }
        }

        [AttachedProperty(Name = "VerticalAlignment", Category = "Layout(Border)")]
        static void OnChildVerticalAlignmentChanged(IBindableObject element, TtBindableProperty property, VerticalAlignment value)
        {
            var ui = element as TtUIElement;
            if(ui != null)
            {
                var boder = VisualTreeHelper.GetParent(ui) as TtBorder;
                boder?.InvalidateMeasure();
            }
        }

        Vector4 mCornerRadius = Vector4.Zero;
        [Meta]
        [BindProperty, CornerRadiusEditor]
        public Vector4 CornerRadius
        {
            get => mCornerRadius;
            set
            {
                OnValueChange(value, mCornerRadius);
                mCornerRadius = value;
            }
        }

        public TtBorder()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
            SizeToContent = ESizeToContent.WidthAndHeight;
        }
        public TtBorder(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
            SizeToContent = ESizeToContent.WidthAndHeight;
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            var borderSize = new SizeF(mBorderThickness.Left + mBorderThickness.Right, mBorderThickness.Top + mBorderThickness.Bottom);
            var paddingSize = new SizeF(mPadding.Left + mPadding.Right, mPadding.Top + mPadding.Bottom);
            var retVal = new SizeF(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            if(visualChildrenCount > 0)
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
                }
                retVal.Width += tempSize.Width;
                retVal.Height += tempSize.Height;
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
            for (int i=0; i<visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var hAlig = TtBorder.GetHorizontalAlignment(childUI);
                var vAlig = TtBorder.GetVerticalAlignment(childUI);
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = RectangleF.Empty;
                switch(hAlig)
                {
                    case HorizontalAlignment.Left:
                        childFinalRect.X = arrangeSize.X + childMargin.Left;
                        childFinalRect.Width = childDesiredSize.Width + childMargin.Left + childMargin.Right;
                        break;
                    case HorizontalAlignment.Center:
                        childFinalRect.X = arrangeSize.X + childDesiredSize.Width * 0.5f + childMargin.Left - childMargin.Right;
                        childFinalRect.Width = childDesiredSize.Width + childMargin.Left + childMargin.Right;
                        break;
                    case HorizontalAlignment.Right:
                        childFinalRect.X = arrangeSize.X + arrangeSize.Width - childDesiredSize.Width + childMargin.Right;
                        childFinalRect.Width = childDesiredSize.Width + childMargin.Left + childMargin.Right;
                        break;
                    case HorizontalAlignment.Stretch:
                        childFinalRect.X = arrangeSize.X;
                        childFinalRect.Width = arrangeSize.Width;
                        break;
                }
                switch(vAlig)
                {
                    case VerticalAlignment.Top:
                        childFinalRect.Y = arrangeSize.Y + childMargin.Top;
                        childFinalRect.Height = childDesiredSize.Height + childMargin.Top + childMargin.Bottom;
                        break;
                    case VerticalAlignment.Center:
                        childFinalRect.Y = arrangeSize.Y + childDesiredSize.Height * 0.5f + childMargin.Top - childMargin.Bottom;
                        childFinalRect.Height = childDesiredSize.Height + childMargin.Top + childMargin.Bottom;
                        break;
                    case VerticalAlignment.Bottom:
                        childFinalRect.Y = arrangeSize.Y + arrangeSize.Height - childDesiredSize.Height + childMargin.Bottom;
                        childFinalRect.Height = childDesiredSize.Height + childMargin.Top + childMargin.Bottom;
                        break;
                    case VerticalAlignment.Stretch:
                        childFinalRect.Y = arrangeSize.Y;
                        childFinalRect.Height = arrangeSize.Height;
                        break;
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
            if(Background != null)
            {
                var rect = new RectangleF(mCurFinalRect.X + mBorderThickness.Left,
                                          mCurFinalRect.Y + mBorderThickness.Top,
                                          mCurFinalRect.Width - mBorderThickness.Left - mBorderThickness.Right,
                                          mCurFinalRect.Height - mBorderThickness.Top - mBorderThickness.Bottom);
                Background.Draw(this, in mDesignClipRect, in rect, batch, in mCornerRadius);
            }
            if (BorderBrush != null && !BorderThickness.IsEmpty())
                BorderBrush.Draw(this, in mDesignClipRect, in mCurFinalRect, batch, in mCornerRadius, in mBorderThickness);

            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.Draw(canvas, batch);
            }
        }
    }
}
