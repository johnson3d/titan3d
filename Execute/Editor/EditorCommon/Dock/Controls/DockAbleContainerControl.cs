using System.Windows;
using System.Windows.Controls;

namespace DockControl.Controls
{
    public class DockAbleContainerControl : DropSurface
    {
        public override Rect SurfaceRect
        {
            get
            {
                try
                {
                    var inputSource = PresentationSource.FromVisual(this);
                    if (inputSource == null)
                        return Rect.Empty;
                    return new Rect(this.PointToScreen(new Point(0, 0)), new Size(this.ActualWidth, this.ActualHeight));
                }
                catch (System.Exception)
                {
                    return Rect.Empty;
                }
            }
        }

        public override FrameworkElement HostParent
        {
            get
            {
                return this.Parent as FrameworkElement;
            }
        }

        public override void OnDragEnter(Point point)
        {
            //DockAssistVisibility = Visibility.Visible;
        }
        public override void OnDragOver(Point point)
        {
        }
        public override void OnDragLeave(Point point)
        {
            //DockAssistVisibility = Visibility.Collapsed;
        }
        public override bool OnDrop(Point point)
        {
            return false;
        }

        public override void AddChild(Controls.DockAbleTabItem element)
        {
            if (element == null)
                return;

            if (this.Content == null)
            {
                // 默认使用TabControl
                this.Content = new DockAbleTabControl();
            }

            EditorCommon.Program.RemoveElementFromParent(element);
            if (this.Content is ItemsControl)
            {
                var itemsCtrl = this.Content as ItemsControl;
                itemsCtrl.Items.Add(element);

                var selCtrl = itemsCtrl as DockAbleTabControl;
                if (selCtrl != null)
                {
                    selCtrl.SelectedItem = element;
                    element.IsTopLevel = selCtrl.IsTopLevel;
                }
            }
            else if (this.Content is Panel)
            {
                var panel = this.Content as Panel;
                panel.Children.Add(element);
            }
            else if (this.Content is ContentControl)
            {
                var cCtrl = this.Content as ContentControl;
                cCtrl.Content = element;
            }
        }

        //public System.Windows.Visibility DockAssistVisibility
        //{
        //    get { return (System.Windows.Visibility)GetValue(DockAssistVisibilityProperty); }
        //    set { SetValue(DockAssistVisibilityProperty, value); }
        //}
        //public static readonly DependencyProperty DockAssistVisibilityProperty =
        //    DependencyProperty.Register("DockAssistVisibility", typeof(System.Windows.Visibility), typeof(DockAbleContainerControl), new FrameworkPropertyMetadata(System.Windows.Visibility.Collapsed));


        static DockAbleContainerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockAbleContainerControl),
                                                     new FrameworkPropertyMetadata(typeof(DockAbleContainerControl)));
        }

        public DockAbleContainerControl()
            : base(null)
        {
        }

        public DockAbleContainerControl(DockAbleWindowBase win)
            : base(win)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var item = GetTemplateChild("PART_Border");
        }
    }
}
