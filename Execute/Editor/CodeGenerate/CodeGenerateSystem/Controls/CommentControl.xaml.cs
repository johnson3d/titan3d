using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// CommentControl.xaml 的交互逻辑
    /// </summary>
    public partial class CommentControl : UserControl
    {
        string mOldComment = "";
        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(string), typeof(CommentControl), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnCommentChanged)));
        public static void OnCommentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CommentControl;

            string newValue = (string)e.NewValue;
            if (!string.IsNullOrEmpty(newValue))
                ctrl.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(newValue) && ctrl.CanModifiyInControl)
                ctrl.TextBlock_Tips.Visibility = Visibility.Visible;
            else
                ctrl.TextBlock_Tips.Visibility = Visibility.Hidden;
        }

        public enum enDockState
        {
            Up,
            Bottom,
            Left,
            Right,
        }
        public enDockState DockState
        {
            get { return (enDockState)GetValue(DockStateProperty); }
            set { SetValue(DockStateProperty, value); }
        }
        public static readonly DependencyProperty DockStateProperty =
            DependencyProperty.Register("DockState", typeof(enDockState), typeof(CommentControl), new FrameworkPropertyMetadata(enDockState.Up, new PropertyChangedCallback(OnDockStateChanged)));
        public static void OnDockStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CommentControl;
            ctrl.UpdateDock();
        }
        public Thickness DockOffset
        {
            get { return (Thickness)GetValue(DockOffsetProperty); }
            set { SetValue(DockOffsetProperty, value); }
        }
        public static readonly DependencyProperty DockOffsetProperty =
            DependencyProperty.Register("DockOffset", typeof(Thickness), typeof(CommentControl), new FrameworkPropertyMetadata(new Thickness(0)));

        public bool CanModifiyInControl
        {
            get { return (bool)GetValue(CanModifiyInControlProperty); }
            set { SetValue(CanModifiyInControlProperty, value); }
        }
        public static readonly DependencyProperty CanModifiyInControlProperty =
            DependencyProperty.Register("CanModifiyInControl", typeof(bool), typeof(CommentControl), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnCanModifiyInControlChanged)));
        public static void OnCanModifiyInControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CommentControl;

            bool newValue = (bool)e.NewValue;
            if(newValue)
            {
                if (string.IsNullOrEmpty(ctrl.Comment))
                    ctrl.TextBlock_Tips.Visibility = Visibility.Visible;
                else
                    ctrl.TextBlock_Tips.Visibility = Visibility.Hidden;
            }
            else
            {
                ctrl.TextBlock_Tips.Visibility = Visibility.Hidden;
            }
        }

        DispatcherTimer mouseTimerEnter = new DispatcherTimer();
        DispatcherTimer mouseTimerLeave = new DispatcherTimer();

        public CommentControl()
        {
            InitializeComponent();

            mouseTimerEnter.Interval = TimeSpan.FromMilliseconds(1000);
            mouseTimerEnter.Tick += MouseTimerEnter_Tick;
            mouseTimerLeave.Interval = TimeSpan.FromMilliseconds(1000);
            mouseTimerLeave.Tick += MouseTimerLeave_Tick;

            this.RenderTransform = new ScaleTransform();
            this.RenderTransformOrigin = new Point(0, 0.5f);
        }

        private void UpdateDock()
        {
            var width = double.IsNaN(this.ActualWidth) ? 0 : this.ActualWidth;
            var height = double.IsNaN(this.ActualHeight) ? 0 : this.ActualHeight;

            switch(DockState)
            {
                case enDockState.Up:
                    {
                        this.HorizontalAlignment = HorizontalAlignment.Left;
                        this.VerticalAlignment = VerticalAlignment.Top;
                        this.Margin = new Thickness(0 + DockOffset.Left, -height + DockOffset.Top, -width + DockOffset.Right, DockOffset.Bottom);
                    }
                    break;
                case enDockState.Bottom:
                    break;
                case enDockState.Left:
                    break;
                case enDockState.Right:
                    break;
            }
        }

        private void TextBox_Comments_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    {
                        TextBox_Comments.Visibility = Visibility.Collapsed;
                        TextBlock_Comments.Visibility = Visibility.Visible;
                        if (string.IsNullOrEmpty(Comment))
                            TextBlock_Tips.Visibility = Visibility.Visible;
                        else
                            TextBlock_Tips.Visibility = Visibility.Hidden;
                    }
                    break;
                case Key.Escape:
                    {
                        Comment = mOldComment;
                        TextBox_Comments.Visibility = Visibility.Collapsed;
                        TextBlock_Comments.Visibility = Visibility.Visible;
                        if (string.IsNullOrEmpty(Comment))
                            TextBlock_Tips.Visibility = Visibility.Visible;
                        else
                            TextBlock_Tips.Visibility = Visibility.Hidden;
                    }
                    break;
            }
        }

        private void TextBox_Comments_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox_Comments.Visibility = Visibility.Collapsed;
            TextBlock_Comments.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(Comment))
                TextBlock_Tips.Visibility = Visibility.Visible;
            else
                TextBlock_Tips.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2 && CanModifiyInControl)
            {
                // 左键双击
                mOldComment = Comment;
                TextBox_Comments.Visibility = Visibility.Visible;
                TextBlock_Comments.Visibility = Visibility.Collapsed;
                TextBlock_Tips.Visibility = Visibility.Hidden;
                Keyboard.Focus(TextBox_Comments);
                TextBox_Comments.SelectAll();
            }

            e.Handled = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDock();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            var pt = oldParent as FrameworkElement;
            if(pt != null)
            {
                pt.MouseEnter -= Parent_MouseEnter;
                pt.MouseLeave -= Parent_MouseLeave;
            }

            pt = this.Parent as FrameworkElement;
            pt.MouseEnter += Parent_MouseEnter;
            pt.MouseLeave += Parent_MouseLeave;
        }

        private void Parent_MouseEnter(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(Comment))
            {
                mouseTimerLeave.Stop();
                mouseTimerEnter.Start();
            }
        }
        private void Parent_MouseLeave(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(Comment))
            {
                mouseTimerEnter.Stop();
                mouseTimerLeave.Start();
            }
        }

        private void MouseTimerEnter_Tick(object sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Visible)
            {
                this.Visibility = Visibility.Visible;
                mouseTimerEnter.Stop();
            }
        }
        private void MouseTimerLeave_Tick(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Collapsed;
                mouseTimerLeave.Stop();
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            mouseTimerLeave.Stop();
            e.Handled = true;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(Comment))
            {
                this.Visibility = Visibility.Collapsed;
            }

            e.Handled = true;
        }

        public void ScaleTips(int scale)
        {
            if (string.IsNullOrEmpty(TextBlock_Comments.Text))
                return;
            if (scale <= 100)
            {
                var scaletransform = this.RenderTransform as ScaleTransform;
                scaletransform.ScaleX = 100 / scale;
                scaletransform.ScaleY = 100 / scale;
            }
            else
            {
                //TextBlock_Comments.RenderTransform.Transform
            }
        }
    }
}
