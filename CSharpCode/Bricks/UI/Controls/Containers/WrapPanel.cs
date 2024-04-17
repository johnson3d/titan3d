using EngineNS.BehaviorTree;
using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls.Containers
{
    [Editor_UIControl("Container.WrapPanel", "WrapPanel", "")]
    public partial class TtWrapPanel : TtContainer
    {
        ELayout_Orientation mOrientation = ELayout_Orientation.Horizontal;
        [Meta]
        [BindProperty]
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
        public TtWrapPanel()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }
        public TtWrapPanel(TtContainer parent)
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
                if (Orientation == ELayout_Orientation.Vertical)
                {
                    float accumulateInCol = 0;
                    List<float> maxHeightsEachCol = new List<float>();
                    maxHeightsEachCol.Add(0);
                    int col = 0;
                    for (int i = 0; i < visualChildrenCount; i++)
                    {
                        var childUI = VisualTreeHelper.GetChild(this, i);
                        childUI.Measure(in childAvailableSize);
                        var childSize = childUI.DesiredSize;
                        if (accumulateInCol + childSize.Width > availableSize.Width)
                        {
                            accumulateInCol = 0;
                            maxHeightsEachCol.Add(0);
                            col++;
                        }
                        accumulateInCol += childSize.Width;
                        var childMaxHeightInCol = Math.Max(tempSize.Height, childSize.Height);
                        maxHeightsEachCol[col] = childMaxHeightInCol;
                    }
                    foreach(var height in maxHeightsEachCol)
                    {
                        retVal.Height += height;
                    }
                    retVal.Width += availableSize.Width; ;
                    return retVal;
                }
                else
                {
                    float accumulateInRow = 0;
                    List<float> maxWidthsEachRow = new List<float>();
                    maxWidthsEachRow.Add(0);
                    int row = 0;
                    for (int i = 0; i < visualChildrenCount; i++)
                    {
                        var childUI = VisualTreeHelper.GetChild(this, i);
                        childUI.Measure(in childAvailableSize);
                        var childSize = childUI.DesiredSize;
                        if (accumulateInRow + childSize.Height > availableSize.Height)
                        {
                            accumulateInRow = 0;
                            maxWidthsEachRow.Add(0);
                            row++;
                        }
                        var childMaxWidthInRow = Math.Max(tempSize.Width, childSize.Width);
                        maxWidthsEachRow[row] = childMaxWidthInRow;
                    }
                    foreach (var width in maxWidthsEachRow)
                    {
                        retVal.Width += width;
                    }
                    retVal.Height += availableSize.Height;
                    return retVal;
                }
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
            if (Orientation == ELayout_Orientation.Vertical)
            {
                float accumulateInCol = innerRect.X;
                float accumulateY = innerRect.Y;
                float lastMaxHeightInCol = 0;
                for (int i = 0; i < visualChildrenCount; i++)
                {
                    var childUI = VisualTreeHelper.GetChild(this, i);
                    var childMargin = childUI.Margin;
                    var childDesiredSize = childUI.DesiredSize;
                    RectangleF childFinalRect = RectangleF.Empty;
                    childFinalRect.Size = childDesiredSize;
                    if (accumulateInCol + childDesiredSize.Width > innerRect.Right)
                    {
                        accumulateInCol = 0;
                        accumulateY += lastMaxHeightInCol;
                    }
                    childFinalRect.X = accumulateInCol;
                    childFinalRect.Y = accumulateY;
                    childUI.Arrange(in childFinalRect);
                    lastMaxHeightInCol = MathF.Max(lastMaxHeightInCol, childDesiredSize.Height);
                    accumulateInCol += childDesiredSize.Width;
                }
            }
            else
            {
                float accumulateInRow = innerRect.Y;
                float accumulateX = innerRect.X;
                float lastMaxWidthInRow = 0;
                for (int i = 0; i < visualChildrenCount; i++)
                {
                    var childUI = VisualTreeHelper.GetChild(this, i);
                    var childMargin = childUI.Margin;
                    var childDesiredSize = childUI.DesiredSize;
                    RectangleF childFinalRect = RectangleF.Empty;
                    childFinalRect.Size = childDesiredSize;
                    if (accumulateInRow + childDesiredSize.Height > innerRect.Bottom)
                    {
                        accumulateInRow = 0;
                        accumulateX += lastMaxWidthInRow;
                    }
                    childFinalRect.Y = accumulateInRow;
                    childFinalRect.X = accumulateX;
                    childUI.Arrange(in childFinalRect);
                    lastMaxWidthInRow = MathF.Max(lastMaxWidthInRow, childDesiredSize.Width);
                    accumulateInRow += childDesiredSize.Height;
                }
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
