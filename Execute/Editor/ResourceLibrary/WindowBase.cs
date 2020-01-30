using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;

namespace ResourceLibrary
{
    public class WindowBase : Window
    {
        // 显示标题栏
        public bool ShowTitle
        {
            get { return (bool)GetValue(ShowTitleProperty); }
            set { SetValue(ShowTitleProperty, value); }
        }
        public static readonly DependencyProperty ShowTitleProperty = DependencyProperty.RegisterAttached("ShowTitle", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        // 置顶按钮
        public bool ShowTopMostButton
        {
            get { return (bool)GetValue(ShowTopMostButtonProperty); }
            set { SetValue(ShowTopMostButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowTopMostButtonProperty = DependencyProperty.RegisterAttached("ShowTopMostButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        // 横向填充按钮
        public bool ShowFillHorizontalButton
        {
            get { return (bool)GetValue(ShowFillHorizontalButtonProperty); }
            set { SetValue(ShowFillHorizontalButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowFillHorizontalButtonProperty = DependencyProperty.RegisterAttached("ShowFillHorizontalButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        // 纵向填充按钮
        public bool ShowFillVerticalButton
        {
            get { return (bool)GetValue(ShowFillVerticalButtonProperty); }
            set { SetValue(ShowFillVerticalButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowFillVerticalButtonProperty = DependencyProperty.RegisterAttached("ShowFillVerticalButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        // 最小化按钮
        public bool ShowMinimizedButton
        {
            get { return (bool)GetValue(ShowMinimizedButtonProperty); }
            set { SetValue(ShowMinimizedButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowMinimizedButtonProperty = DependencyProperty.RegisterAttached("ShowMinimizedButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));
        
        // 最大化按钮
        public bool ShowMaximizedButton
        {
            get { return (bool)GetValue(ShowMaximizedButtonProperty); }
            set { SetValue(ShowMaximizedButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowMaximizedButtonProperty = DependencyProperty.RegisterAttached("ShowMaximizedButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));
        
        // 最大化按钮
        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.RegisterAttached("ShowCloseButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        public bool ShowTutorialBtn
        {
            get { return (bool)GetValue(ShowTutorialBtnProperty); }
            set { SetValue(ShowTutorialBtnProperty, value); }
        }
        public static readonly DependencyProperty ShowTutorialBtnProperty = DependencyProperty.RegisterAttached("ShowTutorialBtn", typeof(bool), typeof(WindowBase), new PropertyMetadata(false));


        public WindowBase()
        {
            //this.StateChanged += DockAbleWindowBase_StateChanged;
            this.Activated += WindowBase_Activated;
            this.SizeChanged += WindowBase_SizeChanged;
        }

        private void WindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            if (this.ActualHeight > screen.WorkingArea.Height || this.ActualWidth > screen.WorkingArea.Width)
            {
                this.WindowState = WindowState.Normal;
                this.WinState = WindowState.Normal;
                Button_Maximized_Click(null, null);
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            switch(WindowState)
            {
                case WindowState.Maximized:
                case WindowState.Minimized:
                    mNormalWinRect = new Rect(this.Left, this.Top, this.Width, this.Height);
                    break;
            }
        }

        private void WindowBase_Activated(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
        }

        protected System.Windows.Thickness mCustomBorderThickness = new System.Windows.Thickness(0);
        public bool CanClose = true;

        Rectangle mPART_Rect_Top, mPART_Rect_Bottom, mPART_Rect_Left, mPART_Rect_Right, mPART_Rect_TopLeft, mPART_Rect_BottomLeft, mPART_Rect_TopRight, mPART_Rect_BottomRight;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var item = GetTemplateChild("PART_Title") as FrameworkElement;
            if (item != null)
            {
                item.MouseMove += Rectangle_Title_MouseMove;
                item.MouseDown += Rectangle_MouseDown;
                item.MouseUp += Rectangle_MouseUp;
            }

            var rhiType = GetTemplateChild("PART_RhiType") as TextBlock;
            if(rhiType != null)
            {
                BindingOperations.SetBinding(rhiType, TextBlock.TextProperty, new Binding("RHIType") { Source = EngineNS.CEngine.Instance.Desc });
            }

            var tgBtn = GetTemplateChild("PART_Button_TopMost") as System.Windows.Controls.Primitives.ToggleButton;
            if (tgBtn != null)
            {
                tgBtn.IsChecked = this.Topmost;
                tgBtn.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding("Topmost") { Source = this, Mode = BindingMode.TwoWay });
            }

            var btn = GetTemplateChild("PART_Button_FillHorizontal") as Button;
            if (btn != null)
                btn.Click += Button_FillHorizontal_Click;

            btn = GetTemplateChild("PART_Button_FillVertical") as Button;
            if (btn != null)
                btn.Click += Button_FillVertical_Click;

            btn = GetTemplateChild("PART_Button_Minimized") as Button;
            if (btn != null)
                btn.Click += Button_Minimized_Click;

            btn = GetTemplateChild("PART_Button_Maximized") as Button;
            if (btn != null)
                btn.Click += Button_Maximized_Click;
            btn = GetTemplateChild("PART_Button_Restore") as Button;
            if (btn != null)
                btn.Click += Button_Maximized_Click;

            btn = GetTemplateChild("PART_Button_Close") as Button;
            if (btn != null)
                btn.Click += Button_Close_Click;

            mPART_Rect_Top = GetTemplateChild("PART_Rect_Top") as Rectangle;
            mPART_Rect_Bottom = GetTemplateChild("PART_Rect_Bottom") as Rectangle;
            mPART_Rect_Left = GetTemplateChild("PART_Rect_Left") as Rectangle;
            mPART_Rect_Right = GetTemplateChild("PART_Rect_Right") as Rectangle;
            mPART_Rect_TopLeft = GetTemplateChild("PART_Rect_TopLeft") as Rectangle;
            mPART_Rect_BottomLeft = GetTemplateChild("PART_Rect_BottomLeft") as Rectangle;
            mPART_Rect_TopRight = GetTemplateChild("PART_Rect_TopRight") as Rectangle;
            mPART_Rect_BottomRight = GetTemplateChild("PART_Rect_BottomRight") as Rectangle;

            mCustomBorderThickness = this.BorderThickness;
        }

        #region 窗口标题拦操作

        private void Button_FillHorizontal_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 横向填充屏幕
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            var dpi = Win32.GetPhysicalUnitScale();
            this.Left = screen.WorkingArea.Left * dpi;
            this.Width = screen.WorkingArea.Width * dpi;
        }

        private void Button_FillVertical_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 纵向填充屏幕
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            var dpi = Win32.GetPhysicalUnitScale();
            this.Top = screen.WorkingArea.Top * dpi;
            this.Height = screen.WorkingArea.Height * dpi;
        }

        private void Button_Minimized_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            WinState = WindowState.Minimized;
        }

        Rect mNormalWinRect;
        public WindowState WinState
        {
            get { return (WindowState)GetValue(WinStateProperty); }
            set { SetValue(WinStateProperty, value); }
        }
        public static readonly DependencyProperty WinStateProperty = DependencyProperty.RegisterAttached("WinState", typeof(WindowState), typeof(WindowBase), new PropertyMetadata(WindowState.Normal));

        private void Button_Maximized_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (WinState == WindowState.Maximized)
            {
                WinState = WindowState.Normal;
                this.Left = mNormalWinRect.Left;
                this.Top = mNormalWinRect.Top;
                this.Width = mNormalWinRect.Width;
                this.Height = mNormalWinRect.Height;
            }
            else
            {
                WinState = WindowState.Maximized;
                mNormalWinRect = new Rect(this.Left, this.Top, this.Width, this.Height);
                var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                var dpi = Win32.GetPhysicalUnitScale();
                this.Left = screen.WorkingArea.Left * dpi;
                this.Top = screen.WorkingArea.Top * dpi;
                this.Height = screen.WorkingArea.Height * dpi;
                this.Width = screen.WorkingArea.Width * dpi;
            }
        }

        protected virtual void Button_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CanClose)
                this.Close();
            else
                this.Hide();
        }

        private void Rectangle_Title_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && mStartDrag)
                {
                    if (WinState == WindowState.Maximized || WindowState == WindowState.Maximized)
                    {
                        if (WindowState == WindowState.Maximized)
                            WindowState = WindowState.Normal;
                        var mousePt = this.PointToScreen(e.GetPosition(this));
                        WinState = WindowState.Normal;

                        var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                        var dpi = Win32.GetPhysicalUnitScale();

                        this.Left = mousePt.X * dpi - ((mNormalWinRect.Width / screen.WorkingArea.Width) * mOffsetPoint.X) / dpi;
                        this.Top = mousePt.Y * dpi - ((mNormalWinRect.Height / screen.WorkingArea.Height) * mOffsetPoint.Y) / dpi;
                        this.Width = mNormalWinRect.Width;
                        this.Height = mNormalWinRect.Height;
                    }

                    this.DragMove();
                    e.Handled = true;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        protected bool mStartDrag = false;
        Point mOffsetPoint;
        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ClickCount)
            {
                case 1:
                    {
                        mOffsetPoint = e.GetPosition(this);
                        mStartDrag = true;
                    }
                    break;

                case 2:
                    {
                        Button_Maximized_Click(null, null);
                    }
                    break;
            }
        }

        void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mStartDrag = false;
        }


        #endregion
    }
}
