﻿using System;
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
    [Editor_UIControl("Container.StackPanel", "StackPanel", "")]
    public partial class TtStackPanel : TtContainer
    {
        ELayout_Orientation mOrientation = ELayout_Orientation.Horizontal;
        [Meta]
        public ELayout_Orientation Orientation
        {
            get => mOrientation;
            set
            {
                OnValueChange(value, mOrientation);
                mOrientation = value;
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
                    if (Orientation == ELayout_Orientation.Horizontal)
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
            float accumulateX = 0, accumulateY = 0;
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = RectangleF.Empty;
                if (Orientation == ELayout_Orientation.Horizontal)
                {
                    childFinalRect.X = arrangeSize.X;
                    childFinalRect.Width = arrangeSize.Width;
                    childFinalRect.Y = accumulateY;
                    accumulateY += childFinalRect.Height;
                }
                else
                {
                    childFinalRect.Y = arrangeSize.Y;
                    childFinalRect.Height = arrangeSize.Height;
                    childFinalRect.X = accumulateX;
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
                Background.Draw(mDesignClipRect, rect, batch, Vector4.Zero);
            }
            if (BorderBrush != null && !BorderThickness.IsEmpty())
                BorderBrush.Draw(mDesignClipRect, mCurFinalRect, batch, Vector4.Zero, mBorderThickness);

            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.Draw(canvas, batch);
            }
        }
    }
}