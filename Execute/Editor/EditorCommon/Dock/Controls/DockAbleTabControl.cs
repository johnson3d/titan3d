using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace DockControl.Controls
{
    public class DockAbleTabControl : TabControl
    {
        public bool IsTopLevel
        {
            get { return (bool)GetValue(IsTopLevelProperty); }
            set { SetValue(IsTopLevelProperty, value); }
        }
        public static readonly DependencyProperty IsTopLevelProperty =
            DependencyProperty.Register("IsTopLevel", typeof(bool), typeof(DockAbleTabControl), new FrameworkPropertyMetadata(false));

        public Visibility ShowHeaderVisible
        {
            get { return (Visibility)GetValue(ShowHeaderVisibleProperty); }
            set { SetValue(ShowHeaderVisibleProperty, value); }
        }
        public static readonly DependencyProperty ShowHeaderVisibleProperty =
            DependencyProperty.Register("ShowHeaderVisible", typeof(Visibility), typeof(DockAbleTabControl), new FrameworkPropertyMetadata(Visibility.Collapsed, new PropertyChangedCallback(OnShowHeaderVisibleChanged)));
        public static void OnShowHeaderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DockAbleTabControl;
        }

        static DockAbleTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockAbleTabControl),
                                                     new FrameworkPropertyMetadata(typeof(DockAbleTabControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var item = GetTemplateChild("PART_ShowHeaderPath") as UIElement;
            if(item != null)
            {
                item.MouseLeftButtonDown += ShowHeaderPathItem_MouseLeftButtonDown;
                item.MouseLeftButtonUp += ShowHeaderPathItem_MouseLeftButtonUp;
            }

        }

        private void ShowHeaderPathItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowHeaderVisible = Visibility.Collapsed;
        }
        private void ShowHeaderPathItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Resize Dockable Control's Header size;
            var controlWidth = ActualWidth;
            double itemsWidth = 0;
            var header = GetTemplateChild("HeaderPanel") as StackPanel;
            if (header.Children.Count <= 1)
                return;

            if (header != null)
            {
                // 1，Calculate full expand TabItem Width
                for( int i = 0; i < header.Children.Count; ++i )
                {
                    var item = header.Children[i] as DockControl.Controls.DockAbleTabItem;
                    if (item == null)
                        break;
                    itemsWidth += item.GetItemFullWidth();              
                }
                double itemMargin = 8;

                // 2，Calculate&&Set short TabItem Width
                if (itemsWidth + itemMargin > controlWidth)
                {
                    var newItemWidth = System.Math.Max( (controlWidth-itemMargin) / header.Children.Count, 18 );
                    for (int i = 0; i < header.Children.Count; ++i)
                    {
                        var item = header.Children[i] as DockControl.Controls.DockAbleTabItem;
                        if (item == null)
                            break;

                        item.Width = newItemWidth;
                    }
                }
                // 3，Calculate&&Set full expand TabItem Width
                else
                {
                    for (int i = 0; i < header.Children.Count; ++i)
                    {
                        var item = header.Children[i] as DockControl.Controls.DockAbleTabItem;
                        if (item == null)
                            break;

                        item.Width = item.GetItemFullWidth();
                        //var headerText = item.GetHeaderText();
                        //if (headerText != null)
                        //{
                        //    var formattedText = new System.Windows.Media.FormattedText(
                        //                            headerText.Text,
                        //                            System.Globalization.CultureInfo.CurrentCulture,
                        //                            headerText.FlowDirection,
                        //                            new System.Windows.Media.Typeface(headerText.FontFamily, headerText.FontStyle, headerText.FontWeight, headerText.FontStretch),
                        //                            headerText.FontSize,
                        //                            headerText.Foreground);
                        //    //headerText.Width = formattedText.Width;
                        //    item.Width = formattedText.Width + 40;
                        //}
                    }
                }
            }

        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if(this.Items.Count == 1)
            {
                var item = this.Items[0] as DockAbleTabItem;
                if(item != null)
                {
                    item.HideTabEnable = true;
                    item.CloseOtherEnable = false;
                    SetBinding(ShowHeaderVisibleProperty, new Binding("ShowHeaderVisible") { Source = item, Mode = BindingMode.TwoWay });
                }
            }
            else
            {
                foreach(var item in this.Items)
                {
                    var tbItem = item as DockAbleTabItem;
                    if (tbItem != null)
                    {
                        tbItem.HideTabEnable = false;
                        tbItem.CloseOtherEnable = true;
                    }
                }
            }
        }
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {

        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
        }
    }
}
