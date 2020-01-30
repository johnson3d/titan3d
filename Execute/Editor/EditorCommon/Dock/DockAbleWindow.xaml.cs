using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DockControl
{
    /// <summary>
    /// DockAbleWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DockAbleWindow : Controls.DockAbleWindowBase
	{
        public static System.Random mRand = new System.Random();
        //List<IDropSurface> mContainDropSurfaces = new List<IDropSurface>();
        //public void RemoveContainDropSurfaces()
        //{
        //    foreach (var dockCtrl in mContainDropSurfaces)
        //    {
        //        DockManager.Instance.RemoveDropSurface(dockCtrl);
        //    }
        //    mContainDropSurfaces.Clear();
        //}
        
		public DockAbleWindow()
		{
			this.InitializeComponent();
            //EditorCommon.Program.ShowedWindows.Add(this);
		}

        // test only /////////////////
        public void Test()
        {
            this.MainGrid.Children.Clear();

            var tabCtrl = new DockControl.Controls.DockAbleTabControl()
            {
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
            };
            var dockCtrl = new DockControl.Controls.DockAbleContainerControl(this)
            {
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
                //HorizontalContentAlignment = HorizontalAlignment.Stretch,
                //VerticalContentAlignment = VerticalAlignment.Stretch,
                //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
            };
            DockManager.Instance.AddDropSurface(dockCtrl);
            dockCtrl.Content = tabCtrl;

            this.MainGrid.Children.Add(dockCtrl);

            for (int i = 0; i < 6; i++)
            {
                var tabItem = new DockControl.Controls.DockAbleTabItem()
                {
                    //HorizontalAlignment = HorizontalAlignment.Stretch,
                    //VerticalAlignment = VerticalAlignment.Stretch,
                    //Style = FindResource("TabItemStyle_DoclAble") as System.Windows.Style,
                    Header = "TabItem " + i
                };
                tabItem.Content = new Grid()
                {
                    //HorizontalAlignment = HorizontalAlignment.Stretch,
                    //VerticalAlignment = VerticalAlignment.Stretch,
                    Background = new SolidColorBrush(Color.FromArgb(255, (byte)mRand.Next(0, 255), (byte)mRand.Next(0, 255), (byte)mRand.Next(0, 255)))
                };
                tabCtrl.Items.Add(tabItem);
            }
        }
        //////////////////////////////


        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            //EditorCommon.GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //RemoveContainDropSurfaces();
        }

        public new void Show()
        {
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            base.Show();
            ResourceLibrary.Win32.BringWindowToTop(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        }

        public void SetContent(IDockAbleControl control)
        {
            var tabItem = new Controls.DockAbleTabItem();
            tabItem.Header = control.KeyValue;
            tabItem.DockGroup = control.DockGroup;
            tabItem.Content = control;
            tabItem.IsTopLevel = true;
            SetContent(tabItem);
        }
        public void SetContent(Controls.DockAbleTabItem element)
        {
            MainGrid.Children.Clear();

            if(element == null)
                return;

            EditorCommon.Program.RemoveElementFromParent(element);

            var dockCtrl = new DockControl.Controls.DockAbleContainerControl(this)
            {
                //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
                Group = element.DockGroup,
            };
//            mContainDropSurfaces.Add(dockCtrl);
            DockManager.Instance.AddDropSurface(dockCtrl);

            if (element is Controls.DockAbleTabItem)
            {
                var tabItem = element as Controls.DockAbleTabItem;
                tabItem.IsTopLevel = true;
                var tabCtrl = new DockControl.Controls.DockAbleTabControl();
                dockCtrl.Content = tabCtrl;
                tabCtrl.Items.Add(tabItem);
                tabCtrl.SelectedIndex = 0;
                tabCtrl.IsTopLevel = true;
                MainGrid.Children.Add(dockCtrl);
            }
            else
            {
                dockCtrl.Content = element;
                MainGrid.Children.Add(dockCtrl);
            }
        }
        
        private void Window_Closed(object sender, System.EventArgs e)
        {
            //EditorCommon.Program.ShowedWindows.Remove(this);
            var children = EditorCommon.Program.GetChildren(this, typeof(IDockAbleControl));
            foreach(var child in children)
            {
                var ctrl = child as IDockAbleControl;
                if (ctrl == null)
                    continue;

                if (DockManager.Instance.MouseWin.DockElement != null &&
                   DockManager.Instance.MouseWin.DockElement.Content == ctrl)
                    continue;
                ctrl.IsShowing = false;
                ctrl.IsActive = false;
            }
        }
    }
}