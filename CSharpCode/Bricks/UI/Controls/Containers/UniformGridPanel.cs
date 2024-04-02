using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;

namespace EngineNS.UI.Controls.Containers
{
    [Editor_UIControl("Container.UniformGridPanel", "UniformGridPanel", "")]
    public partial class TtUniformGridPanel : TtContainer
    {
        [AttachedProperty(Name = "Column", Category = "Layout(UniformGrid)", DefaultValue = 1)]
        static void OnColumnChanged(IBindableObject element, TtBindableProperty property, uint value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var uniformGridPanel = VisualTreeHelper.GetParent(ui) as TtUniformGridPanel;
                uniformGridPanel?.InvalidateMeasure();
            }
        }
        [AttachedProperty(Name = "Row", Category = "Layout(UniformGrid)", DefaultValue = 1)]
        static void OnRowChanged(IBindableObject element, TtBindableProperty property, uint value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var uniformGridPanel = VisualTreeHelper.GetParent(ui) as TtUniformGridPanel;
                uniformGridPanel?.InvalidateMeasure();
            }
        }
        uint mColumns = 1;
        [Meta]
        public uint Columns
        {
            get => mColumns;
            set
            {
                OnValueChange(value, mColumns);
                mColumns = value;
            }
        }
        uint mRows = 1;
        [Meta]
        public uint Rows
        {
            get => mRows;
            set
            {
                OnValueChange(value, mRows);
                mRows = value;
            }
        }
        public TtUniformGridPanel()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }
        public TtUniformGridPanel(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            var borderSize = new SizeF(mBorderThickness.Left + mBorderThickness.Right, mBorderThickness.Top + mBorderThickness.Bottom);
            var paddingSize = new SizeF(mPadding.Left + mPadding.Right, mPadding.Top + mPadding.Bottom);
            var retVal = new SizeF(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
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

            float strideX = arrangeSize.Width / mColumns;
            float strideY = arrangeSize.Height / mRows;
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var col = TtUniformGridPanel.GetColumn(childUI) - 1;
                var row = TtUniformGridPanel.GetRow(childUI) - 1;
                if (col < 0 || row < 0 || mColumns < col || mRows < row)
                    continue;
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = new RectangleF(col * strideX + innerRect.X, row * strideY + innerRect.Y, strideX, strideY);
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
