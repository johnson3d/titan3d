using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace DockControl
{
    public interface IDockAbleControl
    {
        // 是否已经显示
        bool IsShowing { get; set; }
        // 是否在前台显示
        bool IsActive { get; set; }
        // 名称
        string KeyValue { get; }
        int Index { get; set; }

        string DockGroup { get; }

        void SaveElement(EngineNS.IO.XmlNode node, EngineNS.IO.XmlHolder holder);
        IDockAbleControl LoadElement(EngineNS.IO.XmlNode node);
        void StartDrag();
        void EndDrag();
        bool? CanClose();
        void Closed();
    }

    public class DropSurface : ContentControl
    {
        public delegate void Delegate_OnActiveChanged(bool active);
        public event Delegate_OnActiveChanged OnActiveChanged;

        public Guid Id
        {
            get;
        } = Guid.NewGuid();

        bool mActive = true;
        public virtual bool Active
        {
            get { return mActive; }
            set
            {
                mActive = value;
                if (OnActiveChanged != null)
                    OnActiveChanged(mActive);
            }
        }

        protected Rect mSurfaceRect;
        public virtual Rect SurfaceRect { get { return mSurfaceRect; } }
        public virtual FrameworkElement HostParent { get { return null; } }
        public virtual void OnDragEnter(Point point) { }
        public virtual void OnDragOver(Point point) { }
        public virtual void OnDragLeave(Point point) { }
        public virtual bool OnDrop(Point point) { return false; }

        public virtual void AddChild(Controls.DockAbleTabItem element) { }

        public Controls.DockAbleWindowBase HostWin { get; set; }
        public string Group
        {
            get { return (string)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.Register("Group", typeof(string), typeof(DropSurface), new FrameworkPropertyMetadata(""));

        public DropSurface(Controls.DockAbleWindowBase win)
        {
            HostWin = win;

            this.Loaded += IDropSurface_Loaded;
            this.Unloaded += IDropSurface_Unloaded;
        }

        UInt64 mDockAbleHashIdx = 0;
        void IDropSurface_Loaded(object sender, RoutedEventArgs e)
        {
            DockManager.Instance.AddDropSurface(this);

            if(string.IsNullOrEmpty(Group))
            {
                DependencyObject parent = this;
                do
                {
                    parent = LogicalTreeHelper.GetParent(parent);
                    if (parent is IDockAbleControl)
                    {
                        Group = parent.GetType().FullName + mDockAbleHashIdx++;
                        break;
                    }

                }
                while (parent != null);
            }
            if(HostWin == null)
            {
                DependencyObject parent = this;
                do
                {
                    parent = LogicalTreeHelper.GetParent(parent);
                    if(parent is Controls.DockAbleWindowBase)
                    {
                        HostWin = parent as Controls.DockAbleWindowBase;
                        break;
                    }
                }
                while (parent != null);
            }
            if (HostWin != null)
                HostWin.AddSurface(this);
        }

        void IDropSurface_Unloaded(object sender, RoutedEventArgs e)
        {
            DockManager.Instance.RemoveDropSurface(this);

            if (HostWin != null)
                HostWin.RemoveSurface(this);
        }
    }

	/// <summary>
	/// DockManager.xaml 的交互逻辑
	/// </summary>
	public partial class DockManager : Window, EngineNS.Editor.IEditorInstanceObject
    {
        public bool IsLoading = true;

        static UInt64 mPriority = 1;
        public UInt64 GetPriority()
        {
            return mPriority++;
        }

        public readonly static string DockableAllGroup = "CanDockAll";

        public static DockManager Instance
        {
            get
            {
                var name = typeof(DockManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new DockManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }

        public void FinalCleanup()
        {
            this.Close();
            mMouseWin.NeedClose = true;
            mMouseWin.Close();
        }
        
        DockMouseWin mMouseWin = new DockMouseWin();
        public DockMouseWin MouseWin
        {
            get { return mMouseWin; }
        }
        // 拖放辅助窗口（中间拖放位置的十字标）
        DockControl.Controls.DockAssistWindow mAssistWin = new DockControl.Controls.DockAssistWindow();

		DockManager()
		{
			this.InitializeComponent();

            int minLeft = 0, minTop = 0, maxWidth = 0, maxHeight = 0;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if(screen.WorkingArea.Left < minLeft)
                    minLeft = screen.WorkingArea.Left;
                if (screen.WorkingArea.Top < minTop)
                    minTop = screen.WorkingArea.Top;

                maxWidth += screen.WorkingArea.Width;
                maxHeight += screen.WorkingArea.Height;
            }

            //this.Left = minLeft;
            //this.Top = minTop;
            //this.Width = maxWidth;
            //this.Height = maxHeight;
            this.Show();
            //this.Hide();

            mAssistWin.Style = TryFindResource("WindowStyle_DockAssist") as System.Windows.Style;
		}
                        
        #region Surface对象管理

        List<DropSurface> mDropSurfaces = new List<DropSurface>();
        //List<DropSurface> mDropSurfacesWithDragOver = new List<DropSurface>();
        //Stack<DropSurface> mDropEnterSurfaceStack = new Stack<DropSurface>();
        public void AddDropSurface(DropSurface surface)
        {
            if (mDropSurfaces.Contains(surface))
                return;

            mDropSurfaces.Add(surface);

            //var ctrl = surface as DependencyObject;
            //while(ctrl != null && (!(ctrl is Window)))
            //{
            //    ctrl = VisualTreeHelper.GetParent(ctrl);
            //}
            //var win = ctrl as Window;
            //if(win != null)
            //{
            //    if (!EditorCommon.Program.ShowedWindows.Contains(win))
            //        EditorCommon.Program.ShowedWindows.Add(win);
            //}
        }
        public void RemoveDropSurface(DropSurface surface)
        {
            mDropSurfaces.Remove(surface);
            //mDropSurfacesWithDragOver.Remove(surface);

            mAssistWin.HostSurface = null;
        }

        #endregion

        #region Window管理

        public Controls.DockAbleWindowBase CurrentActiveWindow;

        List<Controls.DockAbleWindowBase> mDockableWindows = new List<Controls.DockAbleWindowBase>();
        public Controls.DockAbleWindowBase[] DockableWindows
        {
            get { return mDockableWindows.ToArray(); }            
        }
        public void AddDockableWin(Controls.DockAbleWindowBase win)
        {
            if (!win.LayoutManaged)
                return;

            if (mDockableWindows.Contains(win))
                return;
            
            mDockableWindows.Add(win);
        }
        public void RemoveDockableWin(Controls.DockAbleWindowBase win)
        {
            mDockableWindows.Remove(win);
            OnLayoutChanged();
        }

        public delegate FrameworkElement Delegate_OnGetElementInstance(Type elementType, string keyValue);
        public Delegate_OnGetElementInstance OnGetElementInstance;
        public FrameworkElement _OnGetElementInstance(Type elementType, string keyValue)
        {
            if (OnGetElementInstance != null)
                return OnGetElementInstance(elementType, keyValue);
            return null;
        }

        #endregion

        #region 鼠标操作处理

        void DockManager_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                mMouseWin.Hide();

                var pos = e.GetPosition(this);
                pos = this.PointToScreen(pos);
                EndDrag(pos);

                foreach (var win in mCloseWinList)
                {
                    win.Close();
                }
            }
        }

        double mDeltaMoveX = 0;
        double mDeltaMoveY = 0;
        void DockManager_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var pos = e.GetPosition(this);
                pos = this.PointToScreen(pos);
                mMouseWin.Left = pos.X + mDeltaMoveX;
                mMouseWin.Top = pos.Y + mDeltaMoveY;

                MoveDrag(pos);
            }
        }

        #endregion

        public void RemoveFromParent(FrameworkElement element)
        {
            if (EditorCommon.Program.GetParentChildrenCount(element) == 1)
            {
                var dockAbleContainer = EditorCommon.Program.GetParent(element, typeof(DockControl.Controls.DockAbleContainerControl)) as DockControl.Controls.DockAbleContainerControl;
                if (dockAbleContainer == null)
                {
                    var parent = element.Parent as FrameworkElement;
                    while (parent != null && !(parent is Window))
                    {
                        parent = parent.Parent as FrameworkElement;
                    }
                    if (parent != null)
                    {
                        var win = parent as Controls.DockAbleWindowBase;
                        if (win != null && win.CanClose)
                        {
                            win.Close();
                            //win.Hide();

                            //win.RemoveContainDropSurfaces();
                            mCloseWinList.Add(win);

                            mDropSurfaces.Remove(dockAbleContainer);
                        }
                    }
                }
                else
                {

                    if (EditorCommon.Program.GetParentChildrenCount(dockAbleContainer) == 1)
                    {
                        var parent = dockAbleContainer.Parent as FrameworkElement;
                        while (parent != null && !(parent is Window))
                        {
                            parent = parent.Parent as FrameworkElement;
                        }
                        if (parent != null)
                        {
                            var win = parent as Controls.DockAbleWindowBase;
                            if (win != null && win.CanClose)
                            {
                                win.Close();
                                //win.Hide();

                                //win.RemoveContainDropSurfaces();
                                mCloseWinList.Add(win);

                                mDropSurfaces.Remove(dockAbleContainer);
                            }
                        }
                    }
                    else
                    {
                        mDropSurfaces.Remove(dockAbleContainer);

                        var parent = dockAbleContainer.Parent as Grid;
                        // 去掉GridSplit
                        foreach (UIElement child in parent.Children)
                        {
                            if (child is GridSplitter)
                            {
                                parent.Children.Remove(child);
                                break;
                            }
                        }
                        parent.Children.Remove(dockAbleContainer);

                        var gridParent = parent.Parent as FrameworkElement;
                        var lastCtrl = parent.Children[0] as FrameworkElement;
                        EditorCommon.Program.SetElementParent(lastCtrl, gridParent);

                        if (gridParent is Grid)
                        {
                            var column = Grid.GetColumn(parent);
                            var row = Grid.GetRow(parent);

                            Grid.SetColumn(lastCtrl, column);
                            Grid.SetRow(lastCtrl, row);
                        }

                        EditorCommon.Program.RemoveElementFromParent(parent);
                    }
                }
            }
            EditorCommon.Program.RemoveElementFromParent(element);
            OnLayoutChanged();
        }

        List<Window> mCloseWinList = new List<Window>();
        // dragPoint相对于屏幕
        public bool Drag(Controls.DockAbleTabItem dragedElement, Point dragPoint, Point mouseDownPoint)
        {
            if (!IsMouseCaptured)
            {
                //this.Show();
                //CaptureMouse();
                if (CaptureMouse())
                {
                    var parentDockContainer = EditorCommon.Program.GetParent(dragedElement, typeof(Controls.DockAbleContainerControl)) as Controls.DockAbleContainerControl;
                    if (parentDockContainer != null)
                    {
                        if(string.IsNullOrEmpty(dragedElement.DockGroup))
                            dragedElement.DockGroup = parentDockContainer.Group;
                    }

                    RemoveFromParent(dragedElement);
                    mMouseWin.DockElement = dragedElement;
                    mDeltaMoveX = -mouseDownPoint.X;
                    mDeltaMoveY = -mouseDownPoint.Y;
                    StartDrag(dragPoint);
                    return true;
                }

            }

            return false;
        }

        private void StartDrag(Point point)
        {
            //for (int i = mDropSurfaces.Count - 1; i >= 0; i--)
            //{
            //    // 剔除无效的Surface
            //    if (mDropSurfaces[i].SurfaceRect.IsEmpty)
            //        RemoveDropSurface(mDropSurfaces[i]);
            //}

            //foreach (var surface in mDropSurfaces)
            //{
            //    if (surface.SurfaceRect.Contains(point))
            //    {
            //        if (!mDropSurfacesWithDragOver.Contains(surface))
            //        {
            //            mDropSurfacesWithDragOver.Add(surface);
            //            mDropEnterSurfaceStack.Push(surface);
            //            //surface.OnDragEnter(point);
            //            //mAssistWin.HostSurface = surface;
            //        }
            //    }
            //}
        }

        private void MoveDrag(Point point)
        {
            //var winPtr = Program.WindowFromPoint((int)point.X, (int)point.Y);
            if (mMouseWin.DockElement == null)
                return;

            //if (Mouse.Captured != this)
            //    CaptureMouse();

            DropSurface topSurface = null;
            var groupName = mMouseWin.DockElement.DockGroup;
            foreach (var surface in mDropSurfaces)
            {
                if (surface.SurfaceRect.Contains(point))
                {
                    if (groupName != DockableAllGroup && surface.Group != groupName)
                        continue;
                    if (topSurface == null)
                        topSurface = surface;
                    else
                    {
                        if (surface.HostWin.Topmost == topSurface.HostWin.Topmost)
                        {
                            var surfaceHwnd = new WindowInteropHelper(surface.HostWin).Handle;
                            var topSurfaceHwnd = new WindowInteropHelper(topSurface.HostWin).Handle;
                            if(surfaceHwnd == topSurfaceHwnd)
                            {
                                // 同一窗口
                                DependencyObject parent = surface;
                                do
                                {
                                    parent = VisualTreeHelper.GetParent(parent);
                                }
                                while (parent != null && parent != topSurface);
                                if (parent == topSurface)
                                    topSurface = surface;
                            }
                            else
                            {
                                var surfaceZOrder = ResourceLibrary.Win32.GetWindowZOrder(surfaceHwnd);
                                var topSurfaceZOrder = ResourceLibrary.Win32.GetWindowZOrder(topSurfaceHwnd);
                                if ((surfaceZOrder > topSurfaceZOrder) || (surface.HostWin.Priority > topSurface.HostWin.Priority))
                                    topSurface = surface;
                            }
                        }
                        else if (surface.HostWin.Topmost)
                        {
                            topSurface = surface;
                        }
                    }
                }
            }

            if (mAssistWin.HostSurface != null)
            {
                if (topSurface != null)
                {
                    if (mAssistWin.HostSurface != topSurface)
                    {
                        if (mAssistWin.HostSurface.HostWin.Topmost == topSurface.HostWin.Topmost)
                        {
                            var hostSurfaceHwnd = new WindowInteropHelper(mAssistWin.HostSurface.HostWin).Handle;
                            var hostSurfaceZOrder = ResourceLibrary.Win32.GetWindowZOrder(hostSurfaceHwnd);
                            var topSurfaceHwnd = new WindowInteropHelper(topSurface.HostWin).Handle;
                            var topSurfaceZOrder = ResourceLibrary.Win32.GetWindowZOrder(topSurfaceHwnd);
                            if((hostSurfaceZOrder < topSurfaceZOrder) || (mAssistWin.HostSurface.HostWin.Priority < topSurface.HostWin.Priority))
                            {
                                mAssistWin.HostSurface.OnDragLeave(point);
                                mAssistWin.HostSurface = null;

                                topSurface.OnDragEnter(point);
                                mAssistWin.HostSurface = topSurface;
                            }
                        }
                        else if (topSurface.HostWin.Topmost)
                        {
                            mAssistWin.HostSurface.OnDragLeave(point);
                            mAssistWin.HostSurface = null;

                            topSurface.OnDragEnter(point);
                            mAssistWin.HostSurface = topSurface;
                        }
                    }
                }

                if (!mAssistWin.HostSurface.SurfaceRect.Contains(point))
                {
                    mAssistWin.HostSurface.OnDragLeave(point);
                    mAssistWin.HostSurface = null;
                }
            }
            else
            {
                if (topSurface != null)
                {
                    topSurface.OnDragEnter(point);
                    mAssistWin.HostSurface = topSurface;
                }
            }

            /*foreach (var surface in mDropSurfaces)
            {
                if (!surface.Active)
                    continue;

                if (surface.SurfaceRect.Contains(point))
                {
                    if (!mDropSurfacesWithDragOver.Contains(surface))
                    {
                        mDropSurfacesWithDragOver.Add(surface);

                        //var win = Program.GetParent(surface as FrameworkElement, typeof(DockAbleWindow)) as Window;
                        //if (win == null)
                        //    continue;

                        //if(((new System.Windows.Interop.WindowInteropHelper(this)).Handle) == winPtr)
                            mDropEnterSurfaceStack.Push(surface);
                        //surface.OnDragEnter(point);
                        //mAssistWin.HostSurface = surface;
                    }
                    else
                    {
                        surface.OnDragOver(point);
                    }
                }
                else
                {
                    if (mDropSurfacesWithDragOver.Contains(surface))
                    {
                        mDropSurfacesWithDragOver.Remove(surface);
                        //surface.OnDragLeave(point);
                        //mAssistWin.HostSurface = null;
                    }
                }
            }

            if (mAssistWin.HostSurface != null)
            {
                if (!mAssistWin.HostSurface.SurfaceRect.Contains(point))
                {
                    mAssistWin.HostSurface.OnDragLeave(point);
                    mAssistWin.HostSurface = null;

                    while (mDropEnterSurfaceStack.Count > 0)
                    {
                        var surface = mDropEnterSurfaceStack.Pop();
                        if (surface.SurfaceRect.Contains(point))
                        {
                            surface.OnDragEnter(point);
                            mAssistWin.HostSurface = surface;
                            break;
                        }
                    }
                }
            }
            else
            {
                while (mDropEnterSurfaceStack.Count > 0)
                {
                    var surface = mDropEnterSurfaceStack.Pop();
                    if (!surface.Active)
                        continue;

                    if (surface.SurfaceRect.Contains(point))
                    {
                        surface.OnDragEnter(point);
                        mAssistWin.HostSurface = surface;
                        break;
                    }
                }
            }*/

            mAssistWin.OnDragOver(point);
        }
        private void EndDrag(Point point)
        {
            switch (mAssistWin.OnDrop(point))
            {
                case Controls.DockAssistWindow.DropType.None:
                    {
                        // 创建新窗口放置对象
                        var dockWin = new DockAbleWindow();
                        dockWin.SetContent(mMouseWin.DockElement);
                        dockWin.Left = point.X;
                        dockWin.Top = point.Y;
                        dockWin.Show();
                    }
                    break;

                case Controls.DockAssistWindow.DropType.Fill:
                    {
                        if (mAssistWin.HostSurface != null)
                            mAssistWin.HostSurface.AddChild(mMouseWin.DockElement);
                    }
                    break;

                case Controls.DockAssistWindow.DropType.Left:
                    {
                        if (mAssistWin.HostSurface != null && mAssistWin.HostSurface.HostParent != null)
                        {
                            Grid parentGrid = new Grid();
                            var columnLeft = new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            };
                            parentGrid.ColumnDefinitions.Add(columnLeft);
                            var columnMid = new ColumnDefinition()
                            {
                                Width = new GridLength(5, GridUnitType.Pixel)
                            };
                            parentGrid.ColumnDefinitions.Add(columnMid);
                            var columnRight = new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            };
                            parentGrid.ColumnDefinitions.Add(columnRight);
                            var split = new GridSplitter()
                            {
                                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "GridSplitterStyle_Default")) as Style,
                                ShowsPreview = true,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Cursor = Cursors.SizeWE,
                            };
                            Grid.SetColumn(split, 1);
                            parentGrid.Children.Add(split);

                            var tabCtrl = mAssistWin.HostSurface.Content as Controls.DockAbleTabControl;
                            if(tabCtrl != null)
                                mMouseWin.DockElement.IsTopLevel = tabCtrl.IsTopLevel;

                            EditorCommon.Program.SetElementParent(parentGrid, mAssistWin.HostSurface.HostParent);
                            Grid.SetColumn(parentGrid, Grid.GetColumn(mAssistWin.HostSurface as FrameworkElement));
                            Grid.SetRow(parentGrid, Grid.GetRow(mAssistWin.HostSurface as FrameworkElement));
                            EditorCommon.Program.RemoveElementFromParent(mAssistWin.HostSurface as FrameworkElement);

                            parentGrid.Children.Add(mAssistWin.HostSurface as UIElement);
                            Grid.SetColumn(mAssistWin.HostSurface as UIElement, 2);

                            EditorCommon.Program.RemoveElementFromParent(mMouseWin.DockElement);
                            var dockCtrl = new DockControl.Controls.DockAbleContainerControl(mAssistWin.HostSurface.HostWin)
                            {
                                //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
                                Group = mMouseWin.DockElement.DockGroup,
                            };
                            EditorCommon.Program.SetElementParent(mMouseWin.DockElement, dockCtrl);
                            parentGrid.Children.Add(dockCtrl);
                            Grid.SetColumn(dockCtrl, 0);
                        }
                    }
                    break;

                case Controls.DockAssistWindow.DropType.Right:
                    {
                        if (mAssistWin.HostSurface != null && mAssistWin.HostSurface.HostParent != null)
                        {
                            Grid parentGrid = new Grid();
                            var columnLeft = new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            };
                            parentGrid.ColumnDefinitions.Add(columnLeft);
                            var columnMid = new ColumnDefinition()
                            {
                                Width = new GridLength(5, GridUnitType.Pixel)
                            };
                            parentGrid.ColumnDefinitions.Add(columnMid);
                            var columnRight = new ColumnDefinition()
                            {
                                Width = new GridLength(1, GridUnitType.Star)
                            };
                            parentGrid.ColumnDefinitions.Add(columnRight);
                            var split = new GridSplitter()
                            {
                                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "GridSplitterStyle_Default")) as Style,
                                ShowsPreview = true,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Cursor = Cursors.SizeWE
                            };
                            Grid.SetColumn(split, 1);
                            parentGrid.Children.Add(split);

                            var tabCtrl = mAssistWin.HostSurface.Content as Controls.DockAbleTabControl;
                            if (tabCtrl != null)
                                mMouseWin.DockElement.IsTopLevel = tabCtrl.IsTopLevel;

                            EditorCommon.Program.SetElementParent(parentGrid, mAssistWin.HostSurface.HostParent);
                            Grid.SetColumn(parentGrid, Grid.GetColumn(mAssistWin.HostSurface as FrameworkElement));
                            Grid.SetRow(parentGrid, Grid.GetRow(mAssistWin.HostSurface as FrameworkElement));
                            EditorCommon.Program.RemoveElementFromParent(mAssistWin.HostSurface as FrameworkElement);

                            parentGrid.Children.Add(mAssistWin.HostSurface as UIElement);
                            Grid.SetColumn(mAssistWin.HostSurface as UIElement, 0);

                            EditorCommon.Program.RemoveElementFromParent(mMouseWin.DockElement);
                            var dockCtrl = new DockControl.Controls.DockAbleContainerControl(mAssistWin.HostSurface.HostWin)
                            {
                                //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
                                Group = mMouseWin.DockElement.DockGroup,
                            };
                            EditorCommon.Program.SetElementParent(mMouseWin.DockElement, dockCtrl);
                            parentGrid.Children.Add(dockCtrl);
                            Grid.SetColumn(dockCtrl, 2);
                        }
                    }
                    break;

                case Controls.DockAssistWindow.DropType.Top:
                    {
                        Grid parentGrid = new Grid();
                        var rowLeft = new RowDefinition()
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        };
                        parentGrid.RowDefinitions.Add(rowLeft);
                        var rowMid = new RowDefinition()
                        {
                            Height = new GridLength(5, GridUnitType.Pixel)
                        };
                        parentGrid.RowDefinitions.Add(rowMid);
                        var rowRight = new RowDefinition()
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        };
                        parentGrid.RowDefinitions.Add(rowRight);
                        var split = new GridSplitter()
                        {
                            Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "GridSplitterStyle_Default")) as Style,
                            ShowsPreview = true,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Cursor = Cursors.SizeNS
                        };
                        Grid.SetRow(split, 1);
                        parentGrid.Children.Add(split);

                        var tabCtrl = mAssistWin.HostSurface.Content as Controls.DockAbleTabControl;
                        if (tabCtrl != null)
                            mMouseWin.DockElement.IsTopLevel = tabCtrl.IsTopLevel;

                        EditorCommon.Program.SetElementParent(parentGrid, mAssistWin.HostSurface.HostParent);
                        Grid.SetColumn(parentGrid, Grid.GetColumn(mAssistWin.HostSurface as FrameworkElement));
                        Grid.SetRow(parentGrid, Grid.GetRow(mAssistWin.HostSurface as FrameworkElement));
                        EditorCommon.Program.RemoveElementFromParent(mAssistWin.HostSurface as FrameworkElement);

                        parentGrid.Children.Add(mAssistWin.HostSurface as UIElement);
                        Grid.SetRow(mAssistWin.HostSurface as UIElement, 2);

                        EditorCommon.Program.RemoveElementFromParent(mMouseWin.DockElement);
                        var dockCtrl = new DockControl.Controls.DockAbleContainerControl(mAssistWin.HostSurface.HostWin)
                        {
                            //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
                            Group = mMouseWin.DockElement.DockGroup,
                        };
                        EditorCommon.Program.SetElementParent(mMouseWin.DockElement, dockCtrl);
                        parentGrid.Children.Add(dockCtrl);
                        Grid.SetRow(dockCtrl, 0);
                    }
                    break;

                case Controls.DockAssistWindow.DropType.Bottom:
                    {
                        Grid parentGrid = new Grid();
                        var rowLeft = new RowDefinition()
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        };
                        parentGrid.RowDefinitions.Add(rowLeft);
                        var rowMid = new RowDefinition()
                        {
                            Height = new GridLength(5, GridUnitType.Pixel)
                        };
                        parentGrid.RowDefinitions.Add(rowMid);
                        var rowRight = new RowDefinition()
                        {
                            Height = new GridLength(1, GridUnitType.Star)
                        };
                        parentGrid.RowDefinitions.Add(rowRight);
                        var split = new GridSplitter()
                        {
                            Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "GridSplitterStyle_Default")) as Style,
                            ShowsPreview = true,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Cursor = Cursors.SizeNS
                        };
                        Grid.SetRow(split, 1);
                        parentGrid.Children.Add(split);

                        var tabCtrl = mAssistWin.HostSurface.Content as Controls.DockAbleTabControl;
                        if (tabCtrl != null)
                            mMouseWin.DockElement.IsTopLevel = tabCtrl.IsTopLevel;

                        EditorCommon.Program.SetElementParent(parentGrid, mAssistWin.HostSurface.HostParent);
                        Grid.SetColumn(parentGrid, Grid.GetColumn(mAssistWin.HostSurface as FrameworkElement));
                        Grid.SetRow(parentGrid, Grid.GetRow(mAssistWin.HostSurface as FrameworkElement));
                        EditorCommon.Program.RemoveElementFromParent(mAssistWin.HostSurface as FrameworkElement);

                        parentGrid.Children.Add(mAssistWin.HostSurface as UIElement);
                        Grid.SetRow(mAssistWin.HostSurface as UIElement, 0);

                        EditorCommon.Program.RemoveElementFromParent(mMouseWin.DockElement);
                        var dockCtrl = new DockControl.Controls.DockAbleContainerControl(mAssistWin.HostSurface.HostWin)
                        {
                            //Style = FindResource("ControlStyle_DockContainer") as System.Windows.Style,
                            Group = mMouseWin.DockElement.DockGroup,
                        };
                        EditorCommon.Program.SetElementParent(mMouseWin.DockElement, dockCtrl);
                        parentGrid.Children.Add(dockCtrl);
                        Grid.SetRow(dockCtrl, 2);
                    }
                    break;
            }

            //foreach (var surface in mDropSurfacesWithDragOver)
            //{
            //    if (surface != dropedSurface)
            //        surface.OnDragLeave(point);
            //}
            //mDropSurfacesWithDragOver.Clear();

            mAssistWin.HostSurface = null;
            mMouseWin.DockElement = null;
            //this.Hide();

            OnLayoutChanged();
        }

        public void OnLayoutChanged()
        {
            if (IsLoading)
                return;

            mLastLayoutTime = System.DateTime.Now;
            mIsDirty = true;
        }


        bool mIsDirty = false;
        System.DateTime mLastLayoutTime = System.DateTime.Now;
        public void Tick()
        {
            if(mIsDirty)
            {
                if ((System.DateTime.Now - mLastLayoutTime).TotalMilliseconds > 500)
                {
                    mIsDirty = false;
                    // 存储布局
                    SaveLayoutConfig(LayoutConfigFileName);
                }
            }
        }

        public string LayoutConfigFileName = "";

        public delegate DockControl.Controls.DockAbleWindowBase Delegate_OnGetWindowInstance(Type winType, string keyValue);
        public Delegate_OnGetWindowInstance OnGetWindowInstance;
        public DockControl.Controls.DockAbleWindowBase GetWindowInstance(EngineNS.IO.XmlNode node)
        {
            var att = node.FindAttrib("Type");
            if (att == null)
                return null;

            try
            {
                var splits = att.Value.Split('|');
                var assembly = System.Reflection.Assembly.Load(splits[0]);
                var type = assembly.GetType(splits[1]);

                att = node.FindAttrib("KeyValue");
                if (att == null)
                {
                    var winType = assembly.GetType(splits[1]);
                    if (winType == null)
                        return null;

                    var csInfo = winType.GetConstructor(new Type[] { });
                    if (csInfo != null)
                        return assembly.CreateInstance(splits[1]) as DockControl.Controls.DockAbleWindowBase;
                }
                else
                {
                    if (OnGetWindowInstance != null)
                        return OnGetWindowInstance(type, att.Value);
                }
            }
            catch(System.Exception)
            {
                return null;
            }

            return null;
        }

        public void SaveLayoutConfig(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var xmlholder = EngineNS.IO.XmlHolder.NewXMLHolder("EditorLayout", "");

            foreach (var win in DockControl.DockManager.Instance.DockableWindows)
            {
                var winNode = xmlholder.RootNode.AddNode("DockWindow", "", xmlholder);
                win.SaveLayout(winNode, xmlholder);
            }

            EngineNS.IO.XmlHolder.SaveXML(fileName, xmlholder, true);
        }
    }
}