using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// ErrorReportControl.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorReportControl : UserControl
    {
        public enum ReportType
        {
            Error,
            Warning,
        }

        public class ArrowData
        {
            FrameworkElement m_elementStart;
            FrameworkElement m_elementEnd;

            Shapes.Arrow m_arrow;
            Canvas m_parentCanvas;

            public ArrowData(Canvas parentCanvas, FrameworkElement e1, FrameworkElement e2, Brush color)
            {
                m_elementStart = e1;
                m_elementEnd = e2;

                m_parentCanvas = parentCanvas;

                m_arrow = new Shapes.Arrow();
                m_arrow.HeadWidth = 15;
                m_arrow.HeadHeight = 5;
                m_arrow.Stroke = color;
                m_arrow.StrokeThickness = 1.5;
                m_arrow.Opacity = 0.7;
                m_arrow.IsHitTestVisible = false;
                Canvas.SetZIndex(m_arrow, 3);
                m_parentCanvas.Children.Add(m_arrow);

                UpdateArrow();
            }

            public void Clear()
            {
                m_parentCanvas.Children.Remove(m_arrow);
            }

            public void UpdateArrow()
            {
                Point pt1 = new Point(0, m_elementStart.ActualHeight * 0.5);
                pt1 = m_elementStart.TranslatePoint(pt1, m_parentCanvas);
                Point pt2 = new Point(m_elementEnd.Width * 0.5, m_elementEnd.Height * 0.5);
                pt2 = m_elementEnd.TranslatePoint(pt2, m_parentCanvas);

                m_arrow.X1 = pt1.X;
                m_arrow.Y1 = pt1.Y;
                m_arrow.X2 = pt2.X;
                m_arrow.Y2 = pt2.Y;
            }
        }

        List<ArrowData> m_arrowList = new List<ArrowData>();
        Canvas m_parentCanvas;
        
        Point m_deltaPt;

        public ErrorReportControl(Canvas parentCanvas)
        {
            InitializeComponent();

            Canvas.SetZIndex(this, 3);
            m_parentCanvas = parentCanvas;

            PlayShakeScaleAnimation();
        }

        public void Clear()
        {
            foreach (var arrow in m_arrowList)
            {
                arrow.Clear();
            }
            m_arrowList.Clear();

            ErrorStackPanel.Children.Clear();
            ErrorGroupBox.Visibility = Visibility.Collapsed;
            WarningStackPanel.Children.Clear();
            WarningGroupBox.Visibility = Visibility.Collapsed;
        }

        public void AddReport(List<FrameworkElement> elementList, ReportType rType, string strMsg)
        {
            StackPanel usedStackPanel = null;
            GroupBox usedGroupBox = null;
            Brush brush = null;
            string postString = "";

            switch (rType)
            {
                case ReportType.Error:
                    usedStackPanel = ErrorStackPanel;
                    brush = Brushes.Red;
                    postString = "错误";
                    usedGroupBox = ErrorGroupBox;
                    break;

                case ReportType.Warning:
                    usedStackPanel = WarningStackPanel;
                    brush = Brushes.Yellow;
                    postString = "警告";
                    usedGroupBox = WarningGroupBox;
                    break;
            }

            if (usedGroupBox.Visibility != Visibility.Visible)
                usedGroupBox.Visibility = Visibility.Visible;

            Label lInfo = new Label()
            {
                Content = strMsg,
                Margin = new Thickness(2, 2, 2, 2),
                Foreground = brush,
                BorderThickness = new Thickness(1),
                BorderBrush = brush,
                Background = new System.Windows.Media.SolidColorBrush(Color.FromArgb(100, 0, 0, 0))
            };
            usedStackPanel.Children.Add(lInfo);
            usedGroupBox.Header = usedStackPanel.Children.Count.ToString() + "个" + postString;

            foreach (var element in elementList)
            {
                if (element == null)
                    continue;
                ArrowData arData = new ArrowData(m_parentCanvas, lInfo, element, brush);
                m_arrowList.Add(arData);
            }

            UpdateArrow();
        }

        public void UpdateArrow()
        {
            foreach (var arrow in m_arrowList)
            {
                arrow.UpdateArrow();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            UpdateArrow();
        }
        
        private void GroupBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((FrameworkElement)sender, CaptureMode.Element);
            m_deltaPt = e.GetPosition(this);

            e.Handled = true;
        }

        private void GroupBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void GroupBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition(m_parentCanvas);
                Point newPos = new Point(pt.X - m_deltaPt.X, pt.Y - m_deltaPt.Y);
                Canvas.SetLeft(this, newPos.X);
                Canvas.SetTop(this, newPos.Y);

                UpdateArrow();
            }

            e.Handled = true;
        }

        public void PlayShakeScaleAnimation()
        {
            this.RenderTransform = new ScaleTransform();
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation TransAnim = new DoubleAnimation();
            TransAnim.From = 1.0;
            TransAnim.To = 1.2;
            TransAnim.AutoReverse = true;
            TransAnim.Duration = TimeSpan.FromSeconds(0.15);
            TransAnim.Completed += new EventHandler(TransAnim_ScaleX_Completed);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, TransAnim);

            TransAnim = new DoubleAnimation();
            TransAnim.From = 1.0;
            TransAnim.To = 1.2;
            TransAnim.AutoReverse = true;
            TransAnim.Duration = TimeSpan.FromSeconds(0.1);
            TransAnim.Completed += new EventHandler(TransAnim_ScaleY_Completed);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, TransAnim);
        }

        void TransAnim_ScaleX_Completed(object sender, EventArgs e)
        {
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        void TransAnim_ScaleY_Completed(object sender, EventArgs e)
        {
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }
    }
}
