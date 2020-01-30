using System.Windows;
using System.Windows.Documents;

namespace EditorCommon.DragDrop
{
    public class DropAdorner : Adorner
    {
        System.Windows.Shapes.Rectangle mRect = new System.Windows.Shapes.Rectangle()
        {
            StrokeThickness = 2
        };

        bool mIsAllowDrop = false;
        public bool IsAllowDrop
        {
            get { return mIsAllowDrop; }
            set
            {
                mIsAllowDrop = value;
                if (mIsAllowDrop)
                {
                    mRect.Fill = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "DragDropAdornerAcceptBackgroundBrush")) as System.Windows.Media.Brush;
                    mRect.Stroke = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "DragDropAdornerAcceptBorderBrush")) as System.Windows.Media.Brush;
                }
                else
                {
                    mRect.Fill = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "DragDropAdornerDeniedBackgroundBrush")) as System.Windows.Media.Brush;
                    mRect.Stroke = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "DragDropAdornerDeniedBorderBrush")) as System.Windows.Media.Brush;
                }
            }
        }

        FrameworkElement mAdornedElement;
        public void SetAdornedElement(FrameworkElement adornedElement)
        {
            mAdornedElement = adornedElement;
            UpdateLayout();
        }

        public DropAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            mAdornedElement = adornedElement;
        }

        //protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);
        //}

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override System.Windows.Media.Visual GetVisualChild(int index)
        {
            return mRect;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            mRect.Width = mAdornedElement.ActualWidth;
            mRect.Height = mAdornedElement.ActualHeight;
            mRect.Arrange(new Rect(0, 0, mRect.Width, mRect.Height));

            return base.ArrangeOverride(finalSize);
        }
    }
}
