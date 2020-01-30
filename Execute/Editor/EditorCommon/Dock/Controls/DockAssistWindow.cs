using System.Windows;

namespace DockControl.Controls
{
    class DockAssistWindow : Window
    {
        public enum DropType
        {
            None,
            Fill,
            Left,
            Right,
            Top,
            Bottom,
        }

        public System.Windows.Visibility RectFillVisibility
        {
            get { return (System.Windows.Visibility)GetValue(RectFillVisibilityProperty); }
            set { SetValue(RectFillVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RectFillVisibilityProperty =
            DependencyProperty.Register("RectFillVisibility", typeof(System.Windows.Visibility), typeof(DockAssistWindow), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        public System.Windows.Visibility RectLeftVisibility
        {
            get { return (System.Windows.Visibility)GetValue(RectLeftVisibilityProperty); }
            set { SetValue(RectLeftVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RectLeftVisibilityProperty =
            DependencyProperty.Register("RectLeftVisibility", typeof(System.Windows.Visibility), typeof(DockAssistWindow), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        public System.Windows.Visibility RectRightVisibility
        {
            get { return (System.Windows.Visibility)GetValue(RectRightVisibilityProperty); }
            set { SetValue(RectRightVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RectRightVisibilityProperty =
            DependencyProperty.Register("RectRightVisibility", typeof(System.Windows.Visibility), typeof(DockAssistWindow), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        public System.Windows.Visibility RectTopVisibility
        {
            get { return (System.Windows.Visibility)GetValue(RectTopVisibilityProperty); }
            set { SetValue(RectTopVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RectTopVisibilityProperty =
            DependencyProperty.Register("RectTopVisibility", typeof(System.Windows.Visibility), typeof(DockAssistWindow), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        public System.Windows.Visibility RectBottomVisibility
        {
            get { return (System.Windows.Visibility)GetValue(RectBottomVisibilityProperty); }
            set { SetValue(RectBottomVisibilityProperty, value); }
        }
        public static readonly DependencyProperty RectBottomVisibilityProperty =
            DependencyProperty.Register("RectBottomVisibility", typeof(System.Windows.Visibility), typeof(DockAssistWindow), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));

        public DockAssistWindow()
        {
        }
  
        static DockAssistWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockAssistWindow),
                                                     new FrameworkPropertyMetadata(typeof(DockAssistWindow)));
        }

        DropSurface mHostSurface = null;
        public DropSurface HostSurface
        {
            get { return mHostSurface; }
            set
            {
                if (mHostSurface == value)
                    return;

                if (mHostSurface != null)
                    mHostSurface.OnActiveChanged -= HostSurface_OnActiveChanged;
                mHostSurface = value;

                if (mHostSurface != null)
                {
                    mHostSurface.OnActiveChanged += HostSurface_OnActiveChanged;

                    var rect = mHostSurface.SurfaceRect;
                    this.Top = rect.Top;
                    this.Left = rect.Left;
                    this.Width = rect.Width;
                    this.Height = rect.Height;

                    if (this.Visibility != Visibility.Visible)
                    {
                        this.Show();
                        //System.Windows.Interop.HwndSource source = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(this);
                        //Program.BringWindowToTop(source.Handle);
                        ResourceLibrary.Win32.BringWindowToTop(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                    }
                }
                else
                {
                    this.Hide();
                }
            }
        }

        void HostSurface_OnActiveChanged(bool active)
        {
            if (active)
                this.Show();
            else
                this.Hide();
        }

        FrameworkElement mCtrlFill, mCtrlLeft, mCtrlRight, mCtrlTop, mCtrlBottom;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            mCtrlFill = GetTemplateChild("PART_Fill") as FrameworkElement;
            mCtrlLeft = GetTemplateChild("PART_Left") as FrameworkElement;
            mCtrlRight = GetTemplateChild("PART_Right") as FrameworkElement;
            mCtrlTop = GetTemplateChild("PART_Top") as FrameworkElement;
            mCtrlBottom = GetTemplateChild("PART_Bottom") as FrameworkElement;
        }

        private Rect GetAssistRect(FrameworkElement element)
        {
            if (element == null)
                return Rect.Empty;

            return new Rect(element.PointToScreen(new Point(0, 0)), new Size(element.ActualWidth, element.ActualHeight));
        }
        
        public void OnDragOver(Point point)
        {
            var rect = GetAssistRect(mCtrlFill);
            if (rect.Contains(point))
                RectFillVisibility = Visibility.Visible;
            else
                RectFillVisibility = Visibility.Collapsed;

            rect = GetAssistRect(mCtrlTop);
            if (rect.Contains(point))
                RectTopVisibility = Visibility.Visible;
            else
                RectTopVisibility = Visibility.Collapsed;

            rect = GetAssistRect(mCtrlRight);
            if (rect.Contains(point))
                RectRightVisibility = Visibility.Visible;
            else
                RectRightVisibility = Visibility.Collapsed;

            rect = GetAssistRect(mCtrlBottom);
            if (rect.Contains(point))
                RectBottomVisibility = Visibility.Visible;
            else
                RectBottomVisibility = Visibility.Collapsed;

            rect = GetAssistRect(mCtrlLeft);
            if (rect.Contains(point))
                RectLeftVisibility = Visibility.Visible;
            else
                RectLeftVisibility = Visibility.Collapsed;
        }

        public DropType OnDrop(Point point)
        {
            var rect = GetAssistRect(mCtrlFill);
            if (rect.Contains(point))
            {
                return DropType.Fill;
            }

            rect = GetAssistRect(mCtrlLeft);
            if (rect.Contains(point))
                return DropType.Left;

            rect = GetAssistRect(mCtrlRight);
            if (rect.Contains(point))
                return DropType.Right;

            rect = GetAssistRect(mCtrlTop);
            if (rect.Contains(point))
                return DropType.Top;

            rect = GetAssistRect(mCtrlBottom);
            if (rect.Contains(point))
                return DropType.Bottom;

            return DropType.None;
        }
    }
}
