using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace UIEditor
{
    public class ControlView : INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IDragAbleObject
        public FrameworkElement GetDragVisual()
        {
            return null;
        }
        #endregion

        internal Type UIControlType;

        string mName = "";
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        ImageSource mImageIcon = null;
        public ImageSource ImageIcon
        {
            get { return mImageIcon; }
            set
            {
                mImageIcon = value;
                OnPropertyChanged("ImageIcon");
            }
        }

        Visibility mVisiblity = Visibility.Visible;
        public Visibility Visibility
        {
            get { return mVisiblity; }
            set
            {
                mVisiblity = value;
                OnPropertyChanged("Visibility");
            }
        }

        string mHighLightString = "";
        [Browsable(false)]
        public string HighLightString
        {
            get { return mHighLightString; }
            set
            {
                mHighLightString = value;
                OnPropertyChanged("HighLightString");
            }
        }

        bool mIsExpanded = false;
        public bool IsExpanded
        {
            get { return mIsExpanded; }
            set
            {
                mIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        string mDescription = "";
        public string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                OnPropertyChanged("Description");
            }
        }

        ControlView mParent = null;
        public ControlView Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
            }
        }
        ObservableCollection<ControlView> mChildList = new ObservableCollection<ControlView>();
        public ObservableCollection<ControlView> ChildList
        {
            get { return mChildList; }
            internal set { mChildList = value; }
        }
    }

    /// <summary>
    /// ControlsPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ControlsPanel : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        ObservableCollection<ControlView> mChildList = new ObservableCollection<ControlView>();
        public ObservableCollection<ControlView> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }

        string mFilterString = "";
        public string FilterString
        {
            get => mFilterString;
            set
            {
                mFilterString = value;
                ShowItemsWithFilter(ChildList, mFilterString);
                OnPropertyChanged("FilterString");
            }
        }

        internal DesignPanel HostDesignPanel;

        public ControlsPanel()
        {
            InitializeComponent();
            TreeView_Controls.ItemsSource = ChildList;
        }

        void ClearItems()
        {
            ChildList.Clear();
        }
        public void CollectionUIControls()
        {
            ClearItems();
            // Assemblys
            var curUITypeName = HostDesignPanel?.CurrentUI?.GetType().FullName;
            var types = EngineNS.Rtti.RttiHelper.GetTypes(EngineNS.ECSType.Client);
            foreach(var type in types)
            {
                if (curUITypeName != null && type.FullName == curUITypeName)
                    continue;

                var atts = type.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlAttribute), false);
                if (atts.Length == 0)
                    continue;

                var att = atts[0] as EngineNS.UISystem.Editor_UIControlAttribute;
                var path = new List<string>(att.Path.Split('.'));
                InitControlViews(null, ChildList, path, att, type);
            }
        }
        void InitControlViews(ControlView parent, ObservableCollection<ControlView> childList, List<string> path, EngineNS.UISystem.Editor_UIControlAttribute att, Type type)
        {
            if (path.Count == 0)
                return;

            var curName = path[0];
            path.RemoveAt(0);
            bool find = false;
            for(int idx = 0; idx < childList.Count; idx++)
            {
                if(childList[idx].Name == curName)
                {
                    InitControlViews(childList[idx], childList[idx].ChildList, path, att, type);
                    find = true;
                    break;
                }
            }
            if(!find)
            {
                var view = new ControlView()
                {
                    Name = curName,
                    Parent = parent,
                };
                childList.Add(view);
                if(path.Count > 0)
                {
                    InitControlViews(view, view.ChildList, path, att, type);
                }
                else
                {
                    view.Description = att.Description;
                    view.UIControlType = type;
                    //if (type == typeof(EngineNS.UISystem.Controls.UserControl) || type.IsSubclassOf(typeof(EngineNS.UISystem.Controls.UserControl)))
                    //    view.ImageIcon = new BitmapImage(new Uri($"/UIEditor;component/Icons/UserWidget.png", UriKind.Relative));
                    //else
                        view.ImageIcon = new BitmapImage(new Uri($"/UIEditor;component/Icons/{att.Icon}", UriKind.Relative));
                }
            }
        }
        private bool ShowItemsWithFilter(ObservableCollection<ControlView> items, string filter)
        {
            bool retValue = false;
            foreach(var item in items)
            {
                if (item == null)
                    continue;

                if(string.IsNullOrEmpty(filter))
                {
                    item.Visibility = Visibility.Visible;
                    item.HighLightString = filter;
                    ShowItemsWithFilter(item.ChildList, filter);
                }
                else
                {
                    if (item.ChildList.Count == 0)
                    {
                        if (item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            item.Visibility = System.Windows.Visibility.Visible;
                            item.HighLightString = filter;
                            retValue = true;
                        }
                        else
                        {
                            // 根据名称拼音筛选
                            var pyStr = EngineNS.Localization.PinYin.GetAllPYString(item.Name);
                            if(pyStr.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                item.Visibility = Visibility.Visible;
                                retValue = true;
                            }
                            else
                                item.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        bool bFind = ShowItemsWithFilter(item.ChildList, filter);
                        if (bFind == false)
                            item.Visibility = System.Windows.Visibility.Collapsed;
                        else
                        {
                            item.Visibility = Visibility.Visible;
                            item.IsExpanded = true;
                            retValue = true;
                        }
                    }
                }
            }
            return retValue;
        }

        private void TreeView_Controls_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        Point mMouseDownPos = new Point();
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as ControlView;
            if (listItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownPos = e.GetPosition(grid);
            }
         }
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as ControlView;
            if (listItem == null)
                return;

            if (listItem.ChildList.Count > 0)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(grid);
                if ((System.Math.Abs(pos.X - mMouseDownPos.X) > 3) ||
                   (System.Math.Abs(pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EngineNS.UISystem.Program.ControlDragType, new EditorCommon.DragDrop.IDragAbleObject[] { listItem });
                }
            }
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            CollectionUIControls();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(HostDesignPanel.CurrentUI != null)
                CollectionUIControls();
        }
    }
}
