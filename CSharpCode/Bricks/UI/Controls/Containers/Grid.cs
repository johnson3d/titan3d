using EngineNS.BehaviorTree;
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

    public class TtGridCellDefinition
    {
        public String SplitString = "Auto";

        public EGridCellSplitType Type =>  GetSplitType(SplitString);

        public bool TryGetValue(out float value)
        {
            if(Type == EGridCellSplitType.Fix)
            {
                return float.TryParse(SplitString, out value);
            }
            else if(Type == EGridCellSplitType.Ratio)
            {
                String numberStr = SplitString.Replace("*", "");
                return float.TryParse(numberStr, out value);
            }
            value = 0;
            return false;
        }

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

        [EngineNS.UI.Bind.BindProperty]
        [Meta]
        public List<TtGridCellDefinition> ColumnDefinitions { get; set; } = new List<TtGridCellDefinition>();
        [EngineNS.UI.Bind.BindProperty]
        [Meta]
        public List<TtGridCellDefinition> RowDefinitions { get; set; } = new List<TtGridCellDefinition>();

        public TtGrid()
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
        }

        public TtGrid(TtContainer parent)
            : base(parent)
        {
            mBorderThickness = new Thickness(1, 1, 1, 1);
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
        List<float> RowsSize = new List<float>();
        List<float> ColumnsSize = new List<float>();
        public bool TryGetCellRect(int rowIndex, int columnIndex, float startPointX, float startPointY, out RectangleF outCellRect)
        {
            outCellRect = new RectangleF();
            if ((rowIndex >= RowsSize.Count) || (columnIndex >= ColumnsSize.Count))
            {
                return false;
            }
            if(rowIndex < RowsSize.Count)
            {
                
            }
            float y = startPointY;
            for (int i = 0; i < rowIndex; ++i)
            {
                y += RowsSize[i];
            }
            float x = startPointX;
            for (int i = 0; i < columnIndex; ++i)
            {
                x += ColumnsSize[i];
            }
            outCellRect = new RectangleF(x, y, ColumnsSize[columnIndex], RowsSize[rowIndex]);
            return true;
        }
        protected override SizeF MeasureOverride(in SizeF availableSize)
        {
            RowsSize.Clear();
            ColumnsSize.Clear();
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
            if(RowDefinitions.Count > 0)
            {
                for (int i = 0; i < RowDefinitions.Count; ++i)
                {
                    float rowMaxHeight = 0;
                    var childrenUI = GetElementsAtRow(i);
                    foreach (var child in childrenUI)
                    {
                        child.Measure(in childAvailableSize);
                        if (rowMaxHeight < child.DesiredSize.Height)
                        {
                            rowMaxHeight = child.DesiredSize.Height;
                        }
                    }
                    RowsSize.Add(rowMaxHeight);
                }
            }
            else
            {
                float rowMaxHeight = 0;
                var childrenUI = GetElementsAtRow(0);
                foreach (var child in childrenUI)
                {
                    child.Measure(in childAvailableSize);
                    if (rowMaxHeight < child.DesiredSize.Height)
                    {
                        rowMaxHeight = child.DesiredSize.Height;
                    }
                }
                RowsSize.Add(rowMaxHeight);
            }
            if(ColumnDefinitions.Count > 0)
            {
                for (int i = 0; i < ColumnDefinitions.Count; ++i)
                {
                    float colMaxWidth = 0;
                    var childrenUI = GetElementsAtColumn(i);
                    foreach (var child in childrenUI)
                    {
                        child.Measure(in childAvailableSize);
                        if (colMaxWidth < child.DesiredSize.Width)
                        {
                            colMaxWidth = child.DesiredSize.Width;
                        }
                    }
                    ColumnsSize.Add(colMaxWidth);
                }
            }
            else
            {
                float colMaxWidth = 0;
                var childrenUI = GetElementsAtColumn(0);
                foreach (var child in childrenUI)
                {
                    child.Measure(in childAvailableSize);
                    if (colMaxWidth < child.DesiredSize.Width)
                    {
                        colMaxWidth = child.DesiredSize.Width;
                    }
                }
                ColumnsSize.Add(colMaxWidth);
            }
 

            float ratioTotalHeight = 0;
            float rowTotalRatio = 0;
            Dictionary<int, float> ratioRows = new Dictionary<int, float>();
            for (int i = 0; i < RowDefinitions.Count; ++i)
            {
                var row = RowDefinitions[i];
                if(row.Type == EGridCellSplitType.Fix)
                {
                    row.TryGetValue(out var value);
                    RowsSize[i] = value;
                }
                else if(row.Type == EGridCellSplitType.Ratio)
                {
                    row.TryGetValue(out var value);
                    ratioRows.Add(i, value);
                    ratioTotalHeight += RowsSize[i];
                    rowTotalRatio += value;
                }
            }
            foreach (var ratioRow in ratioRows)
            {
                RowsSize[ratioRow.Key] = ratioTotalHeight * (ratioRow.Value / rowTotalRatio);
            }

            float ratioTotalWidth = 0;
            float columnTotalRatio = 0;
            Dictionary<int, float> columnRows = new Dictionary<int, float>();
            for (int i = 0; i < ColumnDefinitions.Count; ++i)
            {
                var column = ColumnDefinitions[i];
                if (column.Type == EGridCellSplitType.Fix)
                {
                    column.TryGetValue(out var value);
                    ColumnsSize[i] = value;
                }
                else if (column.Type == EGridCellSplitType.Ratio)
                {
                    column.TryGetValue(out var value);
                    columnRows.Add(i, value);
                    ratioTotalWidth += ColumnsSize[i];
                    columnTotalRatio += value;
                }
            }
            foreach (var ratioColumn in columnRows)
            {
                ColumnsSize[ratioColumn.Key] = ratioTotalWidth * (ratioColumn.Value / columnTotalRatio);
            }

            foreach(var rowSize in RowsSize)
            {
                retVal.Width += rowSize;
            }
            foreach (var colSize in ColumnsSize)
            {
                retVal.Height += colSize;
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
                var rowIndex = GetRowIndex(childUI);
                var columnIndex = GetColumnIndex(childUI);
                if(!TryGetCellRect(rowIndex, columnIndex, innerRect.X, innerRect.Y, out var cellRect))
                {
                    continue;
                }
                var childMargin = childUI.Margin;
                var childDesiredSize = childUI.DesiredSize;
                RectangleF childFinalRect = RectangleF.Empty;
                EAlignment_Horizontal hAlignment = TtGrid.GetHorizontalAlignment(childUI);
                switch (hAlignment)
                {
                    case EAlignment_Horizontal.Left:
                        {
                            childFinalRect.X = cellRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        }
                        break;
                    case EAlignment_Horizontal.Center:
                        {
                            childFinalRect.X = (cellRect.Width - childDesiredSize.Width) * 0.5f + cellRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        }
                        break;

                    case EAlignment_Horizontal.Stretch:
                        {
                            childFinalRect.X = cellRect.X;
                            childFinalRect.Width = cellRect.Width;
                        }
                        break;
                    case EAlignment_Horizontal.Right:
                        {
                            childFinalRect.X = cellRect.Width - childDesiredSize.Width + cellRect.X;
                            childFinalRect.Width = childDesiredSize.Width;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                EAlignment_Vertical vAlignment = TtGrid.GetVerticalAlignment(childUI);
                switch (vAlignment)
                {
                    case EAlignment_Vertical.Top:
                        {
                            childFinalRect.Y = cellRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;
                        }
                        break;
                    case EAlignment_Vertical.Center:
                        {
                            childFinalRect.Y = (cellRect.Height - childDesiredSize.Height) * 0.5f + cellRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;
                        }
                        break;
                    case EAlignment_Vertical.Stretch:
                        {
                            childFinalRect.Y = cellRect.Y;
                            childFinalRect.Height = cellRect.Height;
                        }
                        break;
                    case EAlignment_Vertical.Bottom:
                        {
                            childFinalRect.Y = cellRect.Height - childDesiredSize.Height + cellRect.Y;
                            childFinalRect.Height = childDesiredSize.Height;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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