using EngineNS.Rtti;
using EngineNS.UI;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using NPOI.POIFS.Properties;

namespace EngineNS.Bricks.UI.Controls.Containers
{
    public enum EGridCellSplitType
    {
        Auto, // auto
        Fix, // 25
        Ratio, // 25*
    }

    public class TtGridColumnDefinition : TtGridCellDefinition
    {
        public String SplitType = "Auto";
    }

    public class TtGridRowDefinition : TtGridCellDefinition
    {
        public String SplitType = "Auto";
    }
    public class TtGridCellDefinition
    {
        public TtGridColumnDefinition ColDefinition;
        public TtGridRowDefinition RowDefinition;

        public EGridCellSplitType ColSplitType =>  GetSplitType(ColDefinition.SplitType);
        public EGridCellSplitType RowSplitType =>  GetSplitType(RowDefinition.SplitType);

        private EGridCellSplitType GetSplitType(string split)
        {
            if (split.ToLower() == "auto")
            {
                return EGridCellSplitType.Auto;
            }
            else if (split.Contains('*'))
            {
                return EGridCellSplitType.Ratio;
            }
            else if (float.TryParse(split, out float num))
            {
                return EGridCellSplitType.Fix;
            }
            else
            {
                return EGridCellSplitType.Auto;
            }
        }
    }

    [Editor_UIControl("Container.Grid", "Grid", "")]
    public partial class TtGrid : TtContainer
    {
        [EngineNS.UI.Bind.AttachedProperty(Name = "RowIndex", Category = "Layout(Grid)")]
        static void OnChildRowIndexChanged(IBindableObject element, TtBindableProperty property, int value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var grid = VisualTreeHelper.GetParent(ui) as TtGrid;
                grid?.InvalidateArrange();
            }
        }
        [EngineNS.UI.Bind.AttachedProperty(Name = "ColumnIndex", Category = "Layout(Grid)")]
        static void OnChildColumnIndexChanged(IBindableObject element, TtBindableProperty property, int value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var grid = VisualTreeHelper.GetParent(ui) as TtGrid;
                grid?.InvalidateArrange();
            }
        }

        [AttachedProperty(Name = "HorizontalAlignment", Category = "Layout(Grid)")]
        static void OnChildHorizontalAlignmentChanged(IBindableObject element, TtBindableProperty property, EAlignment_Horizontal value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = VisualTreeHelper.GetParent(ui) as TtGrid;
                canvas?.InvalidateArrange();
            }
        }
        [AttachedProperty(Name = "VerticalAlignment", Category = "Layout(Grid)")]
        static void OnChildVerticalAlignmentChanged(IBindableObject element, TtBindableProperty property, EAlignment_Vertical value)
        {
            var ui = element as TtUIElement;
            if (ui != null)
            {
                var canvas = VisualTreeHelper.GetParent(ui) as TtGrid;
                canvas?.InvalidateArrange();
            }
        }

        private List<TtGridColumnDefinition> ColumnDefinitions { get; set; } = new List<TtGridColumnDefinition>();
        private List<TtGridColumnDefinition> RowDefinitions { get; set; } = new List<TtGridColumnDefinition>();

        public TtGrid()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }

        public TtGrid(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }

        private List<List<RectangleF>> CellsRectangle = new List<List<RectangleF>>();

        public RectangleF GetCellSize(int x, int y)
        {
            return CellsRectangle[x][y];
        }
        protected void MeasureGridCell(in SizeF availableSize)
        {

        }
        protected List<TtUIElement> GetElementsAtRow(int rowIndex)
        {
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            if (visualChildrenCount == 0)
                return null;
            List<TtUIElement> elements = new List<TtUIElement>();
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var index = TtGrid.GetRowIndex(childUI);
                if(index == rowIndex)
                {
                    elements.Add(childUI);
                }
            }
            return elements;
        }
        protected List<TtUIElement> GetElementsAtColumn(int colIndex)
        {
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            if (visualChildrenCount == 0)
                return null;
            List<TtUIElement> elements = new List<TtUIElement>();
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var index = TtGrid.GetColumnIndex(childUI);
                if (index == colIndex)
                {
                    elements.Add(childUI);
                }
            }
            return elements;
        }

        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            var borderSize = new SizeF(mBorderThickness.Left + mBorderThickness.Right,
                mBorderThickness.Top + mBorderThickness.Bottom);
            var paddingSize = new SizeF(mPadding.Left + mPadding.Right, mPadding.Top + mPadding.Bottom);
            var retVal = new SizeF(borderSize.Width + paddingSize.Width, borderSize.Height + paddingSize.Height);
            var visualChildrenCount = VisualTreeHelper.GetChildrenCount(this);
            if (visualChildrenCount == 0)
            {
                return retVal;
            }
            var childAvailableSize = new SizeF(
                   MathF.Max(0.0f, availableSize.Width - retVal.Width),
                   MathF.Max(0.0f, availableSize.Height - retVal.Height));
            foreach (var row in RowDefinitions)
            {
                switch (row.ColSplitType)
                {
                    case EGridCellSplitType.Auto:
                        {
                            //统计该行最大值
                        }
                        break;
                    case EGridCellSplitType.Fix:
                        {

                        }
                        break;
                    case EGridCellSplitType.Ratio:
                        {

                        }
                        break;
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
            float accumulateX = innerRect.X, accumulateY = innerRect.Y;
            for (int i = 0; i < visualChildrenCount; i++)
            {
                var childUI = VisualTreeHelper.GetChild(this, i);
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = RectangleF.Empty;

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
                BorderBrush.Draw(this, in mDesignClipRect, in mCurFinalRect, batch, in Vector4.Zero,
                    in mBorderThickness);

            var count = VisualTreeHelper.GetChildrenCount(this);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(this, i);
                child.Draw(canvas, batch);
            }
        }
    }
}