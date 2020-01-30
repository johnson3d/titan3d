using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// AirViewer.xaml 的交互逻辑
    /// </summary>
    public partial class AirViewer : UserControl
    {
        NodesContainerControl mTargetNodesContainer;
        public NodesContainerControl TargetNodesContainer
        {
            get { return mTargetNodesContainer; }
            set
            {
                if(mTargetNodesContainer != null)
                {
                    mTargetNodesContainer.MainDrawCanvas.LayoutUpdated -= MainDrawCanvas_LayoutUpdated;
                }
                mTargetNodesContainer = value;

                if(mTargetNodesContainer != null)
                {
                    mTargetNodesContainer.MainDrawCanvas.LayoutUpdated += MainDrawCanvas_LayoutUpdated;
                    BackgroundBrush.Visual = mTargetNodesContainer.MainDrawCanvas;
                }
            }
        }

        private void MainDrawCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateShow();
        }

        public AirViewer()
        {
            InitializeComponent();
        }

        Rect mViewBoxRect = new Rect(0,0,1,1);
        System.DateTime mLastProcessTime = System.DateTime.Now;
        public void UpdateShow()
        {
            if (TargetNodesContainer == null)
                return;

            // 每次刷新时间间隔100毫秒，防止重复刷新
            var curTime = System.DateTime.Now;
            if ((curTime - mLastProcessTime).TotalMilliseconds < 100)
                return;

            mLastProcessTime = curTime;

            // 背景
            double viewLeft = double.MaxValue;
            double viewTop = double.MaxValue;
            double viewRight = double.MinValue;
            double viewBottom = double.MinValue;
            if(TargetNodesContainer.CtrlNodeList.Count > 0)
            {
                foreach(var node in TargetNodesContainer.CtrlNodeList)
                {
                    var left = node.GetLeftInCanvas(true);
                    if (viewLeft > left)
                        viewLeft = left;
                    var top = node.GetTopInCanvas(true);
                    if (viewTop > top)
                        viewTop = top;
                    var right = left + node.GetWidth(true);
                    if (viewRight < right)
                        viewRight = right;
                    var bottom = top + node.GetHeight(true);
                    if (viewBottom < bottom)
                        viewBottom = bottom;
                }
            }
            else
            {
                viewLeft = 0;
                viewTop = 0;
                viewRight = TargetNodesContainer.MainDrawCanvas.Width;
                viewBottom = TargetNodesContainer.MainDrawCanvas.Height;
            }

            var width = viewRight - viewLeft;
            var height = viewBottom - viewTop;
            if (width < TargetNodesContainer.MainDrawCanvas.Width)
                width = TargetNodesContainer.MainDrawCanvas.Width;
            if (height < TargetNodesContainer.MainDrawCanvas.Height)
                height = TargetNodesContainer.MainDrawCanvas.Height;
            double delta = 30;
            BackgroundBrush.Viewbox = new Rect(viewLeft - delta, viewTop - delta, width + delta * 2, height + delta * 2);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShow();
        }

        Rect mViewRectDelta = new Rect();
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (BackgroundBrush == null || TargetNodesContainer == null)
                return;

            // 外框比例
            if (this.ActualWidth == 0 || double.IsNaN(this.ActualWidth) ||
               this.ActualHeight == 0 || double.IsNaN(this.ActualHeight))
                return;

            var viewBoxDelta = BackgroundBrush.Viewbox.Width / BackgroundBrush.Viewbox.Height;
            var controlDelta = this.ActualWidth / this.ActualHeight;

            // 视口在画布上的位置
            var sPt = TargetNodesContainer.RectCanvas.TranslatePoint(new Point(0, 0), TargetNodesContainer.MainDrawCanvas);
            var ePt = TargetNodesContainer.RectCanvas.TranslatePoint(new Point(TargetNodesContainer.RectCanvas.ActualWidth, TargetNodesContainer.RectCanvas.ActualHeight), TargetNodesContainer.MainDrawCanvas);
            // 视口在Viewbox上的比例
            mViewRectDelta.X = (sPt.X - BackgroundBrush.Viewbox.Left) / BackgroundBrush.Viewbox.Width;
            mViewRectDelta.Y = (sPt.Y - BackgroundBrush.Viewbox.Top) / BackgroundBrush.Viewbox.Height;
            mViewRectDelta.Width = (ePt.X - sPt.X) / BackgroundBrush.Viewbox.Width;
            mViewRectDelta.Height = (ePt.Y - sPt.Y) / BackgroundBrush.Viewbox.Height;

            double viewWidth = 1;
            double viewHeight = 1;
            if (System.Math.Abs(viewBoxDelta - controlDelta) < 0.0000001)
            {
                // 比例一致
                viewWidth = this.ActualWidth;
                viewHeight = this.ActualHeight;
            }
            else
            {
                // 比例不一致
                if(viewBoxDelta > controlDelta)
                {
                    // 宽度占满，高度居中
                    // 实际高度
                    viewWidth = this.ActualWidth;
                    viewHeight = this.ActualWidth / viewBoxDelta;
                }
                else
                {
                    // 高度占满，宽度居中
                    // 实际宽度
                    viewWidth = this.ActualHeight * viewBoxDelta;
                    viewHeight = this.ActualHeight;
                }
            }

            var left = viewWidth * mViewRectDelta.X + (this.ActualWidth - viewWidth) * 0.5;
            var top = viewHeight * mViewRectDelta.Y + (this.ActualHeight - viewHeight) * 0.5;
            var width = viewWidth * mViewRectDelta.Width;
            var height = viewHeight * mViewRectDelta.Height;
            //if (left < 0)
            //{
            //    width += left;
            //    left = 0;
            //}
            //if (top < 0)
            //{
            //    height += top;
            //    top = 0;
            //}
            //if (left >= this.ActualWidth)
            //    left = this.ActualWidth - 1;
            //if (top >= this.ActualHeight)
            //    top = this.ActualHeight - 1;
            //if (left + width > this.ActualWidth)
            //    width = this.ActualWidth - left;
            if (width <= 0)
                width = 1;
            //if (top + height > this.ActualHeight)
            //    height = this.ActualHeight - top;
            if (height <= 0)
                height = 1;
            Rect_ViewBox.Margin = new Thickness(left, top, 0, 0);
            Rect_ViewBox.Width = width;
            Rect_ViewBox.Height = height;
        }

        Point mMouseDownOffset;
        bool mStartDrag = false;
        private void Rect_ViewBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(Rect_ViewBox);
                mStartDrag = true;

                mMouseDownOffset = e.GetPosition(Rect_ViewBox);
            }
        }

        private void Rect_ViewBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (TargetNodesContainer == null)
                return;

            if(e.LeftButton == MouseButtonState.Pressed && mStartDrag)
            {
                var pos = e.GetPosition(Rect_ViewBox) - mMouseDownOffset;

                var deltaX = pos.X / mViewRectDelta.Width;
                var deltaY = pos.Y / mViewRectDelta.Height;
                var left = Canvas.GetLeft(TargetNodesContainer.ViewBoxMain) - deltaX;
                var top = Canvas.GetTop(TargetNodesContainer.ViewBoxMain) - deltaY;
                if (double.IsNaN(left) || double.IsNaN(top))
                    return;
                Canvas.SetLeft(TargetNodesContainer.ViewBoxMain, left);
                Canvas.SetTop(TargetNodesContainer.ViewBoxMain, top);
            }
        }

        private void Rect_ViewBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mStartDrag = false;
        }
    }
}
