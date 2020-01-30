using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DockControl.Controls
{
    public class DockAbleWindowBase : ResourceLibrary.WindowBase
    {
        public delegate void Delegate_OnClosed(DockAbleWindowBase win);
        public Delegate_OnClosed OnWindowClosed;

        public DropSurface TopSurface
        {
            get;
            protected set;
        } = null;
        protected List<DropSurface> mSurfaces = new List<DropSurface>();

        // 圆角窗口参数
        // private int mWinRound = 5;

        // 优先级，根据优先级来处理窗口的拖放操作
        protected UInt64 mPriority;
        public UInt64 Priority
        {
            get { return mPriority; }
        }
        
        //是否添加到布局管理列表里
        public bool LayoutManaged { get; set; } = true;
        public DockAbleWindowBase()
        {
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            // 获取窗口句柄
            //var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            // 设置窗体阴影
            //var ret = Win32.SetClassLong(hwnd, Win32.GCL_STYLE, Win32.GetClassLong(hwnd, Win32.GCL_STYLE) | Win32.CS_DROPSHADOW);

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Loaded += DockAbleWindowBase_Loaded;
            this.Closed += DockAbleWindowBase_Closed;
            this.IsVisibleChanged += DockAbleWindowBase_IsVisibleChanged;
            
            this.SizeChanged += DockAbleWindowBase_SizeChanged;
            this.LocationChanged += DockAbleWindowBase_LocationChanged;
            this.StateChanged += DockAbleWindowBase_StateChanged1;

            this.Activated += DockAbleWindowBase_Activated;
            this.Deactivated += DockAbleWindowBase_Deactivated;

            mPriority = DockManager.Instance.GetPriority();
        }

        private void DockAbleWindowBase_Activated(object sender, EventArgs e)
        {
            mPriority = DockManager.Instance.GetPriority();
            DockManager.Instance.CurrentActiveWindow = this;
        }
        private void DockAbleWindowBase_Deactivated(object sender, EventArgs e)
        {
            if (DockManager.Instance.CurrentActiveWindow == this)
                DockManager.Instance.CurrentActiveWindow = null;
        }

        private void DockAbleWindowBase_StateChanged1(object sender, EventArgs e)
        {
            
        }

        System.DateTime mLastTime = System.DateTime.Now;
        private void DockAbleWindowBase_LocationChanged(object sender, EventArgs e)
        {
            if (mStartDrag && this.Top >= System.Windows.Forms.SystemInformation.WorkingArea.Bottom)
            {
                this.Top = System.Windows.Forms.SystemInformation.WorkingArea.Bottom - this.Height;
            }

            var now = System.DateTime.Now;
            if((now - mLastTime).TotalMilliseconds > 300)
            {
                mLastTime = now;
                DockManager.Instance.OnLayoutChanged();            
            }
        }

        public void AddSurface(DropSurface surface)
        {
            if (!mSurfaces.Contains(surface))
            {
                if (TopSurface == null)
                    TopSurface = surface;
                surface.Active = this.IsVisible;
                mSurfaces.Add(surface);
            }
        }

        public void RemoveSurface(DropSurface surface)
        {
            mSurfaces.Remove(surface);
        }
        private void DockAbleWindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ////// 获取窗口句柄
            ////var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            ////// 设置圆角窗体
            ////Win32.SetWindowRgn(hwnd, Win32.CreateRoundRectRgn(0, 0, System.Convert.ToInt32(this.ActualWidth), System.Convert.ToInt32(this.ActualHeight), mWinRound, mWinRound), true);            
        }

        void DockAbleWindowBase_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            foreach (var surface in mSurfaces)
            {
                surface.Active = this.IsVisible;
            }
        }

        void DockAbleWindowBase_Closed(object sender, EventArgs e)
        {
            DockManager.Instance?.RemoveDockableWin(this);
        }

        void DockAbleWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            DockManager.Instance.AddDockableWin(this);

            ////// 获取窗口句柄
            ////var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            ////// 获取窗口样式
            ////var oldStyle = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE);
            ////// 更改窗体样式为无边框窗体
            ////Win32.SetWindowLong(hwnd, Win32.GWL_STYLE, oldStyle & ~Win32.WS_CAPTION);
            ////// 设置窗体为透明窗体
            //////Win32.SetLayeredWindowAttributes(hwnd, 0x00000000, 0, Win32.LWA_ALPHA | Win32.LWA_COLORKEY);
            ////// 设置圆角窗体
            ////Win32.SetWindowRgn(hwnd, Win32.CreateRoundRectRgn(0, 0, System.Convert.ToInt32(this.ActualWidth), System.Convert.ToInt32(this.ActualHeight), mWinRound, mWinRound), true);
        }


        #region 默认模板设置

        protected override void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            if (CanClose)
            {
                //EditorCommon.Program.ShowedWindows.Remove(this);
                this.Close();
            }
            else
                this.Hide();

            if (OnWindowClosed != null)
                OnWindowClosed(this);
        }




#endregion

#region SaveLoad

        protected virtual void SaveElement(FrameworkElement element, EngineNS.IO.XmlNode node, EngineNS.IO.XmlHolder holder)
        {
            if (element == null)
                return;

            node.AddAttrib("Type", element.GetType().Assembly.FullName + "|" + element.GetType().FullName);
            node.AddAttrib("GridRow", Grid.GetRow(element).ToString());
            node.AddAttrib("GridColumn", Grid.GetColumn(element).ToString());
            node.AddAttrib("HorizontalAlignment", element.HorizontalAlignment.ToString());
            node.AddAttrib("VerticalAlignment", element.VerticalAlignment.ToString());

            if (element is Grid)
            {
                var grid = element as Grid;
                foreach (var column in grid.ColumnDefinitions)
                {
                    var columnDefNode = node.AddNode("ColumnDefinition", "", holder);
                    columnDefNode.AddAttrib("GridUnitType", column.Width.GridUnitType.ToString());
                    columnDefNode.AddAttrib("Value", column.Width.Value.ToString());
                }
                foreach (var row in grid.RowDefinitions)
                {
                    var rowDefNode = node.AddNode("RowDefinition", "", holder);
                    rowDefNode.AddAttrib("GridUnitType", row.Height.GridUnitType.ToString());
                    rowDefNode.AddAttrib("Value", row.Height.Value.ToString());
                }

                foreach (FrameworkElement child in grid.Children)
                {
                    var elementNode = node.AddNode("Element", "", holder);
                    SaveElement(child, elementNode, holder);
                }
            }
            else if (element is DockAbleContainerControl)
            {
                var dacCtrl = element as DockAbleContainerControl;
                var elementNode = node.AddNode("Element", "", holder);
                SaveElement(dacCtrl.Content as FrameworkElement, elementNode, holder);                
            }
            else if (element is DockAbleTabControl)
            {
                var dockAbleTabControl = element as DockAbleTabControl;
                // 
                node.AddAttrib("SelectedIndex", dockAbleTabControl.SelectedIndex.ToString());

                foreach (FrameworkElement item in dockAbleTabControl.Items)
                {
                    var elementNode = node.AddNode("Element", "", holder);
                    SaveElement(item, elementNode, holder);
                }
            }
            else if (element is DockAbleTabItem)
            {
                var datItem = element as DockAbleTabItem;

                node.AddAttrib("Header", datItem.Header.ToString());                

                var elementNode = node.AddNode("Element", "", holder);
                SaveElement(datItem.Content as FrameworkElement, elementNode, holder);
            }
            else if (element is IDockAbleControl)
            {
                var daCtrl = element as IDockAbleControl;
                node.AddAttrib("KeyValue", daCtrl.KeyValue);

                var elementNode = node.AddNode("Element", "", holder);
                daCtrl.SaveElement(elementNode, holder);
            }
            else if (element is GridSplitter)
            {
                //var gsp = element as GridSplitter;
                //var color = ((SolidColorBrush)(gsp.Background)).Color;
                //var str = color.A + "|" + color.R + "|" + color.G + "|" + color.B;
                //node.AddAttrib("Background", str);
            }
        }

        protected virtual FrameworkElement GetElementInstance(EngineNS.IO.XmlNode node)
        {
            try
            {
                var att = node.FindAttrib("Type");
                if (att == null)
                    return null;

                var splits = att.Value.Split('|');
                var assembly = System.Reflection.Assembly.Load(splits[0]);
                var type = assembly.GetType(splits[1]);

                att = node.FindAttrib("KeyValue");
                if (att == null)
                    return assembly.CreateInstance(splits[1]) as FrameworkElement;
                else
                {
                    return DockManager.Instance._OnGetElementInstance(type, att.Value);
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return null;
        }

        protected virtual void LoadElement(FrameworkElement parentElement, EngineNS.IO.XmlNode node)
        {
            var element = GetElementInstance(node);
            if (element == null)
                return;
            if (element is IDockAbleControl)
            {
                var ctr = element as IDockAbleControl;

                element = ctr.LoadElement(node) as FrameworkElement;
                if (element == null)
                    return;
            }
            if (element is DropSurface)
            {
                ((DropSurface)element).HostWin = this;
            }

            if (parentElement is ItemsControl)
            {
                var itemsCtrl = parentElement as ItemsControl;
                itemsCtrl.Items.Add(element);
            }
            else if (parentElement is System.Windows.Controls.Panel)
            {
                var panel = parentElement as System.Windows.Controls.Panel;
                panel.Children.Add(element);
            }            
            else if (parentElement is ContentControl)
            {
                var contentCtrl = parentElement as ContentControl;
                contentCtrl.Content = element;
            }
            
            var att = node.FindAttrib("GridRow");
            if(att != null)
                Grid.SetRow(element, System.Convert.ToInt32(att.Value));
            att = node.FindAttrib("GridColumn");
            if(att != null)
                Grid.SetColumn(element, System.Convert.ToInt32(att.Value));
            att = node.FindAttrib("HorizontalAlignment");
            if (att != null)
            {
                HorizontalAlignment alg;
                System.Enum.TryParse<HorizontalAlignment>(att.Value, out alg);
                element.HorizontalAlignment = alg;
            }
            att = node.FindAttrib("VerticalAlignment");
            if (att != null)
            {
                VerticalAlignment alg;
                System.Enum.TryParse<VerticalAlignment>(att.Value, out alg);
                element.VerticalAlignment = alg;
            }

            if (element is Grid)
            {
                var grid = element as Grid;
                foreach (EngineNS.IO.XmlNode colNode in node.FindNodes("ColumnDefinition"))
                {
                    GridUnitType unitType = GridUnitType.Star;
                    var gAtt = colNode.FindAttrib("GridUnitType");
                    if (gAtt != null)
                    {
                        System.Enum.TryParse<GridUnitType>(gAtt.Value, out unitType);
                    }

                    double value = 1;
                    gAtt = colNode.FindAttrib("Value");
                    if (gAtt != null)
                    {
                        value = System.Convert.ToDouble(gAtt.Value);
                    }

                    var def = new ColumnDefinition()
                    {
                        Width = new GridLength(value, unitType)
                    };
                    grid.ColumnDefinitions.Add(def);
                }
                foreach (EngineNS.IO.XmlNode rowNode in node.FindNodes("RowDefinition"))
                {
                    GridUnitType unitType = GridUnitType.Star;
                    var gAtt = rowNode.FindAttrib("GridUnitType");
                    if (gAtt != null)
                    {
                        System.Enum.TryParse<GridUnitType>(gAtt.Value, out unitType);
                    }

                    double value = 1;
                    gAtt = rowNode.FindAttrib("Value");
                    if (gAtt != null)
                    {
                        value = System.Convert.ToDouble(gAtt.Value);
                    }

                    var def = new RowDefinition()
                    {
                        Height = new GridLength(value, unitType)
                    };
                    grid.RowDefinitions.Add(def);
                }
            }
            else if (element is DockAbleTabControl)
            {
                var datItem = element as DockAbleTabControl;
                var eAtt = node.FindAttrib("SelectedIndex");
                if (eAtt != null)
                    datItem.SelectedIndex = Convert.ToInt32(eAtt.Value);
            }
            else if (element is DockAbleTabItem)
            {
                var datItem = element as DockAbleTabItem;
                var eAtt = node.FindAttrib("Header");
                if (eAtt != null)
                    datItem.Header = eAtt.Value;
            }            
            else if (element is GridSplitter)
            {
                var gsp = element as GridSplitter;
                //var eAtt = node.FindAttrib("Background");
                //if (eAtt != null)
                //{
                //    var clrStr = eAtt.Value.Split('|');
                //    gsp.Background = new SolidColorBrush(Color.FromArgb(System.Convert.ToByte(clrStr[0]),
                //                                                        System.Convert.ToByte(clrStr[1]),
                //                                                        System.Convert.ToByte(clrStr[2]),
                //                                                        System.Convert.ToByte(clrStr[3])));
                //}
                gsp.Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "GridSplitterStyle_Default")) as Style;
                gsp.ShowsPreview = true;
            }
            else if (element is IDockAbleControl)
            {
                return;
            }

            foreach (var childNode in node.FindNodes("Element"))
            {
                LoadElement(element, childNode);
            }
        }

        public virtual void SaveLayout(EngineNS.IO.XmlNode node, EngineNS.IO.XmlHolder holder)
        {
            if (this.WindowState == WindowState.Minimized)
                return;
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);

            node.AddAttrib("Type", this.GetType().Assembly.FullName + "|" + this.GetType().FullName);
            node.AddAttrib("WinState", this.WindowState.ToString());
            node.AddAttrib("LeftRate", ((float)(this.Left - screen.WorkingArea.Left) / (float)screen.WorkingArea.Width).ToString());
            node.AddAttrib("TopRate", ((float)(this.Top - screen.WorkingArea.Top) / (float)screen.WorkingArea.Height).ToString());
            node.AddAttrib("WidthRate", ((float)this.Width / (float)screen.WorkingArea.Width).ToString());
            node.AddAttrib("HeightRate", ((float)this.Height / (float)screen.WorkingArea.Height).ToString());

            for (int i = 0; i < System.Windows.Forms.Screen.AllScreens.Length; ++i)
            {
                if (screen.Equals(System.Windows.Forms.Screen.AllScreens[i]))
                {
                    node.AddAttrib("ScreenIndex", i.ToString());
                    break;
                }
            }

            var contentNode = node.AddNode("WinContent", "", holder);
            SaveElement(this.Content as FrameworkElement, contentNode, holder);
        }

        public virtual void LoadLayout(EngineNS.IO.XmlNode node)
        {
            int screenIndex = 0;
            System.Windows.Forms.Screen screen;

            var att = node.FindAttrib("ScreenIndex");
            if (att != null)
                screenIndex = System.Convert.ToInt32(att.Value);
            if (System.Windows.Forms.Screen.AllScreens.Length <= screenIndex)
            {
                screen = System.Windows.Forms.Screen.PrimaryScreen;
            }
            else
            {
                screen = System.Windows.Forms.Screen.AllScreens[screenIndex];
            }
            
            if (screen == null)
                return;

            att = node.FindAttrib("LeftRate");
            if (att != null)
                this.Left = System.Convert.ToSingle(att.Value) * (float)screen.WorkingArea.Width + screen.WorkingArea.Left;
            att = node.FindAttrib("TopRate");
            if (att != null)
                this.Top = System.Convert.ToSingle(att.Value) * (float)screen.WorkingArea.Height + screen.WorkingArea.Top;
            att = node.FindAttrib("WidthRate");
            if (att != null)
                this.Width = System.Convert.ToSingle(att.Value) * (float)screen.WorkingArea.Width;
            att = node.FindAttrib("HeightRate");
            if (att != null)
                this.Height = System.Convert.ToSingle(att.Value) * (float)screen.WorkingArea.Height;
            att = node.FindAttrib("WinState");
            if (att != null)
            {
                WindowState state = WindowState.Normal;
                if (Enum.TryParse<WindowState>(att.Value, out state))
                    this.WindowState = state;
            }

            ReSetLocation(screen);

            var contentNode = node.FindNode("WinContent");
            LoadElement(this, contentNode);
        }

        public void ReSetLocation(System.Windows.Forms.Screen screen)
        {
            if (screen == null)
                return;

            if (WindowState != WindowState.Normal)
                return;

            if (this.Width >= screen.WorkingArea.Width && this.Height >= screen.WorkingArea.Height)
            {
                WindowState = WindowState.Maximized;                
            }
            else 
            {
                if (this.Width >= screen.WorkingArea.Width)
                    this.Width = screen.WorkingArea.Width;
                if (this.Height >= screen.WorkingArea.Height)
                    this.Height = screen.WorkingArea.Height;

                if (this.Left <= screen.WorkingArea.Left)
                    this.Left = screen.WorkingArea.Left;
                if (this.Left + this.Width >= screen.WorkingArea.Right)
                    this.Left = screen.WorkingArea.Right - this.Width;

                if (this.Top + this.Height >= screen.WorkingArea.Bottom)
                    this.Top = screen.WorkingArea.Bottom - this.Height;

                if (this.Width < 500)
                    this.Width = 500;
                if (this.Height < 300)
                    this.Height = 300;
            }                            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //if(this.WindowState != WindowState.Minimized)
            //    DockManager.Instance.OnLayoutChanged();            

            return base.MeasureOverride(availableSize);
        }

#endregion
    }
}
