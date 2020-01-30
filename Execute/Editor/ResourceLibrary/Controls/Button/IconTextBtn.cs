using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ResourceLibrary.Controls.Button
{
    public interface IIconTextBtn_CustomIsSubmenuOpen
    {
        bool IsCustom { get; }
    }

    public class IconTextBtn : System.Windows.Controls.MenuItem
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(IconTextBtn), new FrameworkPropertyMetadata(""));

        public new ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static new readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(IconTextBtn), new FrameworkPropertyMetadata(null));

        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(IconTextBtn), new FrameworkPropertyMetadata(40.0));
        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(IconTextBtn), new FrameworkPropertyMetadata(40.0));

        public bool IsComboBox
        {
            get { return (bool)GetValue(IsComboBoxProperty); }
            set { SetValue(IsComboBoxProperty, value); }
        }
        public static readonly DependencyProperty IsComboBoxProperty =
            DependencyProperty.Register("IsComboBox", typeof(bool), typeof(IconTextBtn), new FrameworkPropertyMetadata(false));
        public Visibility ComboArrawShow
        {
            get { return (Visibility)GetValue(ComboArrawShowProperty); }
            set { SetValue(ComboArrawShowProperty, value); }
        }
        public static readonly DependencyProperty ComboArrawShowProperty =
            DependencyProperty.Register("ComboArrawShow", typeof(Visibility), typeof(IconTextBtn), new FrameworkPropertyMetadata(Visibility.Visible));

        public object ExtContent
        {
            get { return GetValue(ExtContentProperty); }
            set { SetValue(ExtContentProperty, value); }
        }
        public static readonly DependencyProperty ExtContentProperty =
            DependencyProperty.Register("ExtContent", typeof(object), typeof(IconTextBtn), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnExtContentChanged)));
        private static void OnExtContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IconTextBtn)d;
            ctrl.SetValue(HasExtContentProperty, (e.NewValue != null) ? true : false);
        }
        public bool HasExtContent
        {
            get { return (bool)GetValue(HasExtContentProperty); }
        }
        public static readonly DependencyProperty HasExtContentProperty =
            DependencyProperty.Register("HasExtContent", typeof(bool), typeof(IconTextBtn), new FrameworkPropertyMetadata(false));

        static IconTextBtn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconTextBtn), new FrameworkPropertyMetadata(typeof(IconTextBtn)));
            HeaderProperty.OverrideMetadata(typeof(IconTextBtn), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceHeader)));

            EventManager.RegisterClassHandler(typeof(IconTextBtn), Mouse.PreviewMouseUpEvent, new MouseButtonEventHandler(OnMenuItemPreviewClick));
        }

        private static void OnMenuItemPreviewClick(object sender, MouseButtonEventArgs e)
        {
            var items = e.OriginalSource as ItemsControl;
            if(items != null)
            {
                if (items.Items.Count > 0)
                    return;
            }

            var btn = sender as IconTextBtn;
            if (btn.IsSubmenuOpen)
            {
                var parent = e.Source as FrameworkElement;
                if (parent == null)
                    return;
                var itbciso = e.Source as IIconTextBtn_CustomIsSubmenuOpen;
                if(itbciso != null)
                {
                    if (itbciso.IsCustom)
                        return;
                }
                while (parent != null)
                {
                    if (parent == btn)
                        return;
                    if (parent is MenuItem)
                        break;
                    parent = parent.Parent as FrameworkElement;
                }
                if (parent == null)
                    return;
                var menuItem = parent as MenuItem;
                if (menuItem.HasItems)
                    return;
                if (ResourceLibrary.Controls.Menu.MenuAssist.GetNotCloseMenuOnClick(menuItem))
                    return;
                btn.Dispatcher.BeginInvoke(new Action(() =>
                {
                    btn.IsSubmenuOpen = false;
                }), DispatcherPriority.Input, null);
            }
        }

        private static object CoerceHeader(DependencyObject d, object value)
        {
            return value;
        }
        
        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //}

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (this.Items.Count == 0)
                IsComboBox = false;
            else
                IsComboBox = true;
        }

        bool mIsDown = false;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            mIsDown = true;
            if(!IsKeyboardFocusWithin)
            {
                Focus();
            }
            IsPressed = true;
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (mIsDown)
            {
                if (IsComboBox)
                    this.IsSubmenuOpen = !this.IsSubmenuOpen;
                //Command?.Execute(null);
                OnClick();
            }
            IsPressed = false;
            if (IsCheckable)
                IsChecked = !IsChecked;
            mIsDown = false;
            e.Handled = true;
        }

        internal static readonly RoutedEvent PreviewClickEvent = EventManager.RegisterRoutedEvent("PreviewClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconTextBtn));

        protected override void OnClick()
        {
            RaiseEvent(new RoutedEventArgs(PreviewClickEvent, this));
            if(AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            {
                AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
                if (peer != null)
                    peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(InvokeClickAfterRender), false);
        }
        private object InvokeClickAfterRender(object arg)
        {
            var userInitiated = (bool)arg;
            RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, this));
            if(this.Command != null)
            {
                var parameter = this.CommandParameter;
                var target = this.CommandTarget;

                var routed = this.Command as RoutedCommand;
                if (routed != null)
                {
                    if (target == null)
                        target = this as IInputElement;
                    if (routed.CanExecute(parameter, target))
                        routed.Execute(parameter, target);
                }
                else if (this.Command.CanExecute(parameter))
                    this.Command.Execute(parameter);
            }
            return null;
        }
        
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            //base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            //base.OnMouseLeave(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //base.OnMouseMove(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            IsPressed = false;
        }
    }
}
