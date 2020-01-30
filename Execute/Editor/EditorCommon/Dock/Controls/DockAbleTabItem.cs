using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DockControl.Controls
{
    [TemplatePart(Name = "Part_Bd", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Content", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_Path0", Type = typeof(Path))]
    [TemplatePart(Name = "PART_Path1", Type = typeof(Path))]
    public class DockAbleTabItem : TabItem
    {
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(null));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(null));


        public string DockGroup
        {
            get { return (string)GetValue(DockGroupProperty); }
            set { SetValue(DockGroupProperty, value); }
        }
        public static readonly DependencyProperty DockGroupProperty =
            DependencyProperty.Register("Group", typeof(string), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(""));
        public bool IsTopLevel
        {
            get { return (bool)GetValue(IsTopLevelProperty); }
            set { SetValue(IsTopLevelProperty, value); }
        }
        public static readonly DependencyProperty IsTopLevelProperty =
            DependencyProperty.Register("IsTopLevel", typeof(bool), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(false));

        public double HeaderScaleX
        {
            get { return (double)GetValue(HeaderScaleXProperty); }
            set { SetValue(HeaderScaleXProperty, value); }
        }
        public static readonly DependencyProperty HeaderScaleXProperty =
            DependencyProperty.Register("HeaderScaleX", typeof(double), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(1.0));

        public double HeaderScaleY
        {
            get { return (double)GetValue(HeaderScaleYProperty); }
            set { SetValue(HeaderScaleYProperty, value); }
        }
        public static readonly DependencyProperty HeaderScaleYProperty =
            DependencyProperty.Register("HeaderScaleY", typeof(double), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(1.0));

        public CornerRadius CorRadius
        {
            get { return (CornerRadius)GetValue(CorRadiusProperty); }
            set { SetValue(CorRadiusProperty, value); }
        }
        public static readonly DependencyProperty CorRadiusProperty =
            DependencyProperty.Register("CorRadius", typeof(CornerRadius), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(new CornerRadius(0)));

        public Visibility HeaderVisible
        {
            get { return (Visibility)GetValue(HeaderVisibleProperty); }
            set { SetValue(HeaderVisibleProperty, value); }
        }
        public static readonly DependencyProperty HeaderVisibleProperty =
            DependencyProperty.Register("HeaderVisible", typeof(Visibility), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(Visibility.Visible, new PropertyChangedCallback(OnHeaderVisibleChanged)));
        public static void OnHeaderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DockAbleTabItem;
            var newValue = (Visibility)e.NewValue;
            if (newValue == ctrl.ShowHeaderVisible)
            {
                if (newValue == Visibility.Visible)
                    ctrl.ShowHeaderVisible = Visibility.Collapsed;
                else
                    ctrl.ShowHeaderVisible = Visibility.Visible;
            }
        }
        public Visibility ShowHeaderVisible
        {
            get { return (Visibility)GetValue(ShowHeaderVisibleProperty); }
            set { SetValue(ShowHeaderVisibleProperty, value); }
        }
        public static readonly DependencyProperty ShowHeaderVisibleProperty =
            DependencyProperty.Register("ShowHeaderVisible", typeof(Visibility), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnShowHeaderVisibleChanged)));
        public static void OnShowHeaderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DockAbleTabItem;
            var newValue = (Visibility)e.NewValue;
            if (newValue == ctrl.HeaderVisible)
            {
                if (newValue == Visibility.Visible)
                    ctrl.HeaderVisible = Visibility.Collapsed;
                else
                    ctrl.HeaderVisible = Visibility.Visible;
            }
        }
        public bool HideTabEnable
        {
            get { return (bool)GetValue(HideTabEnableProperty); }
            set { SetValue(HideTabEnableProperty, value); }
        }
        public static readonly DependencyProperty HideTabEnableProperty =
            DependencyProperty.Register("HideTabEnable", typeof(bool), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(true));
        public bool CloseEnable
        {
            get { return (bool)GetValue(CloseEnableProperty); }
            set { SetValue(CloseEnableProperty, value); }
        }
        public static readonly DependencyProperty CloseEnableProperty =
            DependencyProperty.Register("CloseEnable", typeof(bool), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(true));
        public bool CloseOtherEnable
        {
            get { return (bool)GetValue(CloseOtherEnableProperty); }
            set { SetValue(CloseOtherEnableProperty, value); }
        }
        public static readonly DependencyProperty CloseOtherEnableProperty =
            DependencyProperty.Register("CloseOtherEnable", typeof(bool), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(false));
        public double ControlWidth
        {
            get { return (double)GetValue(ControlWidthProperty); }
            set { SetValue(ControlWidthProperty, value); }
        }
        public static readonly DependencyProperty ControlWidthProperty =
            DependencyProperty.Register("ControlWidth", typeof(double), typeof(DockAbleTabItem), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnControlWidthChanged)));
        public static void OnControlWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DockAbleTabItem;
            var newValue = (double)e.NewValue;
            if (ctrl.mImageBG != null)
                ctrl.mImageBG.Width = newValue - ctrl.mImageBG.Margin.Left - ctrl.mImageBG.Margin.Right;
        }
        EditorCommon.Controls.ImageEx mImageBG;

        static DockAbleTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockAbleTabItem),
                                                     new FrameworkPropertyMetadata(typeof(DockAbleTabItem)));
        }

        public DockAbleTabItem()
        {
            this.Loaded += DockAbleTabItem_Loaded;
            this.Unloaded += DockAbleTabItem_Unloaded;
        }

        void DockAbleTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Content is IDockAbleControl)
            {
                ((IDockAbleControl)(this.Content)).IsActive = true;
            }
        }

        void DockAbleTabItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.Content is IDockAbleControl)
            {
                ((IDockAbleControl)(this.Content)).IsActive = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var border = GetTemplateChild("PART_Bd") as FrameworkElement;
            if (border != null)
            {
                border.MouseMove += Bd_MouseMove;
                border.MouseUp += Bd_MouseUp;
                border.MouseDown += Bd_MouseDown;
                border.MouseLeave += Bd_MouseLeave;
                var corVal = CorRadius;
                SetBinding(CorRadiusProperty, new Binding("CornerRadius") { Source = border, Mode = BindingMode.TwoWay });
                CorRadius = corVal;
                var visVal = HeaderVisible;
                SetBinding(HeaderVisibleProperty, new Binding("Visibility") { Source = border });
                HeaderVisible = visVal;
            }

            var grid = GetTemplateChild("PART_Grid") as Grid;
            if (grid != null)
            {
                SetBinding(ControlWidthProperty, new Binding("ActualWidth") { Source = grid });
            }
            mImageBG = GetTemplateChild("Image_Tab") as EditorCommon.Controls.ImageEx;

            var item = GetTemplateChild("PART_Close") as UIElement;
            if (item != null)
            {
                item.MouseLeftButtonDown += Close_MouseLeftButtonDown;
            }

            var scaleItem = GetTemplateChild("PART_HeaderScale") as ScaleTransform;
            if (scaleItem != null)
            {
                var valX = HeaderScaleX;
                var valY = HeaderScaleY;
                SetBinding(HeaderScaleXProperty, new Binding("ScaleX") { Source = scaleItem, Mode = BindingMode.TwoWay });
                SetBinding(HeaderScaleYProperty, new Binding("ScaleY") { Source = scaleItem, Mode = BindingMode.TwoWay });
                HeaderScaleX = valX;
                HeaderScaleY = valY;
            }

            var menuItem = GetTemplateChild("PART_MENU_HideTab") as MenuItem;
            if (menuItem != null)
                menuItem.Click += MenuItem_HideTab_Click;
            menuItem = GetTemplateChild("PART_MENU_Close") as MenuItem;
            if (menuItem != null)
                menuItem.Click += MenuItem_Close_Click;
            menuItem = GetTemplateChild("PART_MENU_CloseOther") as MenuItem;
            if (menuItem != null)
                menuItem.Click += MenuItem_CloseOther_Click;
        }

        private void MenuItem_HideTab_Click(object sender, RoutedEventArgs e)
        {
            HeaderVisible = Visibility.Collapsed;
        }
        public delegate void Delegate_OnClose();
        public event Delegate_OnClose OnClose;
        public delegate bool? Delegate_CanClose();
        public event Delegate_CanClose CanClose;
        public void Close()
        {
            if (CanClose != null && CanClose() != true)
                return;

            if (this.Content is IDockAbleControl)
            {
                var dac = this.Content as IDockAbleControl;
                if (dac.CanClose() == false)
                    return;
                dac.IsShowing = false;
                dac.IsActive = false;
                dac.Closed();
            }
            DockManager.Instance.RemoveFromParent(this);
            OnClose?.Invoke();
        }
        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }
        private void MenuItem_CloseOther_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void Close_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        bool mMouseDown = false;
        Point mMouseDownPoint;
        void Bd_MouseLeave(object sender, MouseEventArgs e)
        {
            if (mMouseDown)
            {
                if (mMouseDownPoint == e.GetPosition(this.Parent as FrameworkElement))
                    return;

                if (this.Content is IDockAbleControl)
                {
                    ((IDockAbleControl)(this.Content)).StartDrag();
                }
                DockManager.Instance.Drag(this, e.GetPosition(DockManager.Instance), mMouseDownPoint);
                if (this.Content is IDockAbleControl)
                {
                    ((IDockAbleControl)(this.Content)).EndDrag();
                }

                mMouseDown = false;
            }
        }

        private void Bd_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

            }
        }

        private void Bd_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseDownPoint = e.GetPosition(this.Parent as FrameworkElement);
            mMouseDown = true;
        }

        private void Bd_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseDown = false;
        }

        //public TextBlock GetHeaderText()
        //{
        //    var c = GetTemplateChild("PART_Content") as System.Windows.Controls.ContentPresenter;
        //    if (c != null)
        //    {
        //        var t = c.Content as TextBlock;
        //        return t;
        //    }
        //    return null;
        //}

        public double GetItemFullWidth()
        {
            double itemWidth = 0;
            var iconPart = GetTemplateChild("PART_Icon") as System.Windows.FrameworkElement;
            if (iconPart != null)
            {
                itemWidth += iconPart.Width;
            }
            var closePart = GetTemplateChild("PART_Close") as System.Windows.FrameworkElement;
            if (closePart != null)
            {
                itemWidth += closePart.Width;
            }
            var contentPart = GetTemplateChild("PART_Content") as System.Windows.Controls.ContentPresenter;
            if (contentPart != null)
            {
                var headerText = contentPart.Content as TextBlock;
                if (headerText != null)
                {
                    var formattedText = new System.Windows.Media.FormattedText(
                                            headerText.Text,
                                            System.Globalization.CultureInfo.CurrentCulture,
                                            headerText.FlowDirection,
                                            new System.Windows.Media.Typeface(headerText.FontFamily, headerText.FontStyle, headerText.FontWeight, headerText.FontStretch),
                                            headerText.FontSize,
                                            headerText.Foreground);
                    itemWidth += formattedText.Width;
                }
                else
                    itemWidth += contentPart.Width;
            }
            itemWidth += 15;

            return itemWidth;

            //if (headerText != null && iconPart != null && closePart != null)
            //{
            //    var formattedText = new System.Windows.Media.FormattedText(
            //                            headerText.Text,
            //                            System.Globalization.CultureInfo.CurrentCulture,
            //                            headerText.FlowDirection,
            //                            new System.Windows.Media.Typeface(headerText.FontFamily, headerText.FontStyle, headerText.FontWeight, headerText.FontStretch),
            //                            headerText.FontSize,
            //                            headerText.Foreground);
            //    return formattedText.Width + iconPart.Width + closePart.Width + 8;
            //}
            //return Width;
        }

    }
}
