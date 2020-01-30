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
    public class HierarchyItemView : DependencyObject, INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject
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

        EngineNS.UISystem.UIElement mLinkedUIELement;
        public EngineNS.UISystem.UIElement LinkedUIElement => mLinkedUIELement;

        public HierarchyItemView(EngineNS.UISystem.UIElement element)
        {
            mLinkedUIELement = element;

            BindingOperations.SetBinding(this, NameProperty, new Binding("Name") { Source = mLinkedUIELement.Initializer });
            var atts = mLinkedUIELement.GetType().GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlAttribute), false);
            if(atts.Length > 0)
            {
                var att = atts[0] as EngineNS.UISystem.Editor_UIControlAttribute;
                ImageIcon = new BitmapImage(new Uri($"/UIEditor;component/Icons/{att.Icon}", UriKind.Relative));
            }
        }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(HierarchyItemView), new PropertyMetadata("", OnNamePropertyChanged, OnNamePropertyCVCallback));
        private static void OnNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //var ctrl = sender as HierarchyItemView;
            //var newName = (string)e.NewValue;
            //ctrl.mLinkedUIELement.Initializer.Name = newName;
        }
        private static object OnNamePropertyCVCallback(DependencyObject d, object value)
        {
            var ctrl = d as HierarchyItemView;

            var newName = (string)value;
            if(string.IsNullOrEmpty(newName))
                return "[" + ctrl.mLinkedUIELement.GetType().Name + "]";

            return newName;
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

        bool mIsExpanded = true;
        public bool IsExpanded
        {
            get { return mIsExpanded; }
            set
            {
                mIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        bool mIsSelected = false;
        public bool IsSelected
        {
            get { return mIsSelected; }
            set
            {
                mIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        bool mTreeViewItemIsSelected = false;
        public bool TreeViewItemIsSelected
        {
            get { return mTreeViewItemIsSelected; }
            set
            {
                mTreeViewItemIsSelected = value;
                OnPropertyChanged("TreeViewItemIsSelected");
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

        System.Windows.Media.Brush mTreeViewItemForeGround = System.Windows.Media.Brushes.White;
        [Browsable(false)]
        public System.Windows.Media.Brush TreeViewItemForeground
        {
            get => mTreeViewItemForeGround;
            set
            {
                mTreeViewItemForeGround = value;
                OnPropertyChanged("TreeViewItemForeground");
            }
        }
        System.Windows.Media.Brush mTreeViewItemBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0));
        [Browsable(false)]
        public System.Windows.Media.Brush TreeViewItemBackground
        {
            get => mTreeViewItemBackground;
            set
            {
                mTreeViewItemBackground = value;
                OnPropertyChanged("TreeViewItemBackground");
            }
        }
        System.Windows.Visibility mUpInsertLineVisible = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility UpInsertLineVisible
        {
            get { return mUpInsertLineVisible; }
            set
            {
                mUpInsertLineVisible = value;
                OnPropertyChanged("UpInsertLineVisible");
            }
        }

        System.Windows.Visibility mDownInsertLineVisible = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility DownInsertLineVisible
        {
            get { return mDownInsertLineVisible; }
            set
            {
                mDownInsertLineVisible = value;
                OnPropertyChanged("DownInsertLineVisible");
            }
        }

        System.Windows.Visibility mChildInsertLineVisible = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility ChildInsertLineVisible
        {
            get { return mChildInsertLineVisible; }
            set
            {
                mChildInsertLineVisible = value;
                OnPropertyChanged("ChildInsertLineVisible");
            }
        }

        double mTreeViewItemHeight;
        [Browsable(false)]
        public double TreeViewItemHeight
        {
            get => mTreeViewItemHeight;
            set
            {
                mTreeViewItemHeight = value;
                OnPropertyChanged("TreeViewItemHeight");
            }
        }

        HierarchyItemView mParent = null;
        public HierarchyItemView Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
            }
        }
        ObservableCollection<HierarchyItemView> mChildList = new ObservableCollection<HierarchyItemView>();
        public ObservableCollection<HierarchyItemView> ChildList
        {
            get { return mChildList; }
            internal set { mChildList = value; }
        }
    }

    /// <summary>
    /// HierarchyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class HierarchyPanel : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        Dictionary<EngineNS.UISystem.UIElement, HierarchyItemView> mChildDic = new Dictionary<EngineNS.UISystem.UIElement, HierarchyItemView>();
        ObservableCollection<HierarchyItemView> mChildList = new ObservableCollection<HierarchyItemView>();
        public ObservableCollection<HierarchyItemView> ChildList
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
                TreeView_Controls.ItemsSource = null;
                ShowItemsWithFilter(ChildList, mFilterString);
                TreeView_Controls.ItemsSource = ChildList;
                OnPropertyChanged("FilterString");
            }
        }

        internal DesignPanel HostDesignPanel;
        public HierarchyPanel()
        {
            InitializeComponent();
            TreeView_Controls.ItemsSource = ChildList;

            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }

        void ClearItems()
        {
            ChildList.Clear();
        }
        private bool ShowItemsWithFilter(ObservableCollection<HierarchyItemView> items, string filter)
        {
            bool retValue = false;
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(filter))
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
                            if (pyStr.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
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

        internal void OnReceiveSelectUIElements(EngineNS.UISystem.UIElement[] selectUIs)
        {
            //mBroadcastSelectedOperation = false;
            mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
            foreach (var view in mSelectedItemViews)
            {
                view.IsSelected = false;
                view.TreeViewItemIsSelected = false;
            }
            mSelectedItemViews.Clear();
            if (selectUIs == null)
                return;
            foreach(var ui in selectUIs)
            {
                HierarchyItemView item;
                if (!mChildDic.TryGetValue(ui, out item))
                    continue;

                item.IsSelected = true;
                mSelectedItemViews.Add(item);
            }
            //mBroadcastSelectedOperation = true;
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }
        internal void OnReceiveDeleteUIElements(EngineNS.UISystem.UIElement[] deletedUIs)
        {
            //mBroadcastSelectedOperation = false;
            mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
            foreach(var ui in deletedUIs)
            {
                HierarchyItemView item;
                if (!mChildDic.TryGetValue(ui, out item))
                    continue;

                if (item.Parent == null)
                    ChildList.Remove(item);
                else
                    item.Parent.ChildList.Remove(item);
            }
            for(int i=mSelectedItemViews.Count - 1; i>=0; i--)
            {
                foreach(var ui in deletedUIs)
                {
                    if(mSelectedItemViews[i].LinkedUIElement == ui)
                    {
                        mSelectedItemViews.RemoveAt(i);
                        break;
                    }
                }
            }
            //mBroadcastSelectedOperation = true;
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }

        Point mMouseDownPos = new Point();
        FrameworkElement mMouseDownElement;
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as HierarchyItemView;
            if (listItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownElement = grid;
                mMouseDownPos = e.GetPosition(grid);
            }
        }
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as FrameworkElement;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && grid == mMouseDownElement)
            {
                var pos = e.GetPosition(grid);
                if ((System.Math.Abs(pos.X - mMouseDownPos.X) > 3) ||
                   (System.Math.Abs(pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.IDragAbleObject[] array = mSelectedItemViews.ToArray();
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EngineNS.UISystem.Program.HierarchyControlDragType, array);
                }
            }
        }
        private void TreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mMouseDownElement = null;
        }
        private void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;
            var item = element.DataContext as HierarchyItemView;
            if (item == null)
                return;
            item.UpInsertLineVisible = Visibility.Hidden;
            item.DownInsertLineVisible = Visibility.Hidden;
            item.ChildInsertLineVisible = Visibility.Hidden;
        }

        EngineNS.UISystem.Controls.UserControl mCurrentUserControl;
        public async Task SetObjectToEditor(EngineNS.CRenderContext rc, EditorCommon.Resources.ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            mCurrentUserControl = HostDesignPanel.CurrentUI;
            mChildDic.Clear();
            ChildList.Clear();
            UpdateShowUIControls(mCurrentUserControl, ChildList, null);
        }

        public void ReplaceUIElement(EngineNS.UISystem.UIElement oldUI, EngineNS.UISystem.UIElement newUI)
        {
            HierarchyItemView oldView;
            if(mChildDic.TryGetValue(oldUI, out oldView))
            {
                var parent = oldView.Parent;
                var newView = new HierarchyItemView(newUI);
                newView.Parent = parent;
                var index = parent.ChildList.IndexOf(oldView);
                parent.ChildList.Remove(oldView);
                parent.ChildList.Insert(index, newView);
                mChildDic[newUI] = newView;
                mChildDic.Remove(oldUI);
            }
        }

        void UpdateShowUIControls(EngineNS.UISystem.UIElement uiElement, ObservableCollection<HierarchyItemView> items, HierarchyItemView parent)
        {
            if (uiElement == null)
                return;

            var itemView = new HierarchyItemView(uiElement);
            itemView.Parent = parent;
            items.Add(itemView);
            mChildDic[uiElement] = itemView;

            if(uiElement is EngineNS.UISystem.Controls.Containers.Panel)
            {
                if(!(uiElement is EngineNS.UISystem.Controls.UserControl && uiElement != mCurrentUserControl))
                {
                    var panel = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
                    foreach (var child in panel.ChildrenUIElements)
                    {
                        UpdateShowUIControls(child, itemView.ChildList, itemView);
                    }
                }
            }
        }

        public void OnReceiveAddChildren(EngineNS.UISystem.UIElement parent, EngineNS.UISystem.UIElement[] children, int insertIndex)
        {
            HierarchyItemView hiv;
            if (mChildDic.TryGetValue(parent, out hiv))
            {
                //mBroadcastSelectedOperation = false;
                mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
                foreach (var view in mSelectedItemViews)
                {
                    view.IsSelected = false;
                }
                mSelectedItemViews.Clear();

                foreach (var child in children)
                {
                    var itemView = new HierarchyItemView(child);
                    if (insertIndex < 0 || insertIndex >= hiv.ChildList.Count)
                        hiv.ChildList.Add(itemView);
                    else
                        hiv.ChildList.Insert(insertIndex, itemView);
                    mChildDic[child] = itemView;
                    HierarchyItemView parentView;
                    if (mChildDic.TryGetValue(parent, out parentView))
                    {
                        itemView.Parent = parentView;
                    }
                    itemView.IsSelected = true;
                    mSelectedItemViews.Add(itemView);
                }
                //mBroadcastSelectedOperation = true;
                mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
            }
            else
                throw new InvalidOperationException("未找到UIElement对应的View");
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Delete:
                    //mBroadcastSelectedOperation = false;
                    mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
                    var uis = new EngineNS.UISystem.UIElement[mSelectedItemViews.Count];
                    for(int i=0; i<mSelectedItemViews.Count; i++)
                    {
                        var view = mSelectedItemViews[i];
                        //if (view.Parent == null)
                        //{
                        //    ChildList.Remove(view);
                        //}
                        //else
                        //{
                        //    view.Parent.ChildList.Remove(view);
                        //}
                        uis[i] = view.LinkedUIElement;
                    }
                    //mSelectedItemViews.Clear();
                    //mBroadcastSelectedOperation = true;
                    HostDesignPanel.BroadcastDeleteUIs(this, uis);
                    mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
                    break;
            }
        }

        void ExpandAllTreeViewParent(HierarchyItemView view)
        {
            if (view == null)
                return;
            var viewStack = new Stack<HierarchyItemView>();
            viewStack.Push(view);
            var viewParent = view.Parent;
            while(viewParent != null)
            {
                viewStack.Push(viewParent);
                viewParent = viewParent.Parent;
            }

            ItemsControl item = TreeView_Controls;
            while(viewStack.Count > 0)
            {
                var ctrl = viewStack.Pop();
                item = FindTreeViewItem(item, ctrl);
            }
            if (item != null)
                item.BringIntoView();
        }
        private TreeViewItem FindTreeViewItem(ItemsControl item, object data)
        {
            if (item == null)
                return null;
            TreeViewItem findItem = null;
            bool itemIsExpand = false;
            if (item is TreeViewItem)
            {
                TreeViewItem tviCurrent = item as TreeViewItem;
                itemIsExpand = tviCurrent.IsExpanded;
                if (!tviCurrent.IsExpanded)
                {
                    //如果这个TreeViewItem未展开过，则不能通过ItemContainerGenerator来获得TreeViewItem
                    tviCurrent.SetValue(TreeViewItem.IsExpandedProperty, true);
                    //必须使用UpdaeLayour才能获取到TreeViewItem
                    tviCurrent.UpdateLayout();
                }
            }
            // 优先查找同级对象
            for (int i = 0; i < item.Items.Count; i++)
            {
                TreeViewItem tvItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
                if (tvItem == null)
                    continue;
                object itemData = item.Items[i];
                if (itemData == data)
                {
                    findItem = tvItem;
                    break;
                }
            }
            if (findItem == null)
            {
                for (int i = 0; i < item.Items.Count; i++)
                {
                    TreeViewItem tvItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
                    if (tvItem == null)
                        continue;
                    if (tvItem.Items.Count > 0)
                    {
                        findItem = FindTreeViewItem(tvItem, data);
                        if (findItem != null)
                            break;
                    }
                }
            }
            if (findItem == null)
            {
                TreeViewItem tviCurrent = item as TreeViewItem;
                if (tviCurrent != null)
                {
                    tviCurrent.SetValue(TreeViewItem.IsExpandedProperty, itemIsExpand);
                    tviCurrent.UpdateLayout();
                }
            }
            return findItem;
        }


        #region Select
        ObservableCollection<HierarchyItemView> mSelectedItemViews = new ObservableCollection<HierarchyItemView>();
        public ObservableCollection<HierarchyItemView> SelectedItemViews
        {
            get => mSelectedItemViews;
            set
            {
                mSelectedItemViews = value;
                OnPropertyChanged("SelectedItemViews");
            }
        }
        //bool mBroadcastSelectedOperation = true;
        private void SelectedItemViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if(mBroadcastSelectedOperation)
            {
                var uis = new EngineNS.UISystem.UIElement[mSelectedItemViews.Count];
                for (int i = 0; i < mSelectedItemViews.Count; i++)
                {
                    uis[i] = mSelectedItemViews[i].LinkedUIElement;
                }
                HostDesignPanel.BroadcastSelectedUI(this, uis);
            }
        }

        private void SelectControl(HierarchyItemView view)
        {
            if (view == null)
                return;

            view.IsSelected = true;
        }
        private void UnSelectControl(HierarchyItemView view, bool withChild = false)
        {
            if (view == null)
                return;

            view.IsSelected = false;

            if(withChild)
            {
                foreach (var ctrl in view.ChildList)
                {
                    UnSelectControl(ctrl, withChild);
                }
            }
        }
        class SelectItemIndexData
        {
            public HierarchyItemView Control;
            public int[] TotalIndex;  // 保证在拖动时选中对象的顺序不变
        }
        List<SelectItemIndexData> mSelectItemIndexDatas = new List<SelectItemIndexData>();
        public void UpdateSelectItems(ObservableCollection<HierarchyItemView> selectedItems, ObservableCollection<HierarchyItemView> unSelectedItems)
        {
            if(unSelectedItems != null)
            {
                foreach(var ctrl in unSelectedItems)
                {
                    UnSelectControl(ctrl);
                    foreach(var data in mSelectItemIndexDatas)
                    {
                        if(data.Control == ctrl)
                        {
                            mSelectItemIndexDatas.Remove(data);
                            break;
                        }
                    }
                }
            }
            if(selectedItems != null)
            {
                foreach(var ctrl in selectedItems)
                {
                    ExpandAllTreeViewParent(ctrl);

                    SelectControl(ctrl);
                    var data = new SelectItemIndexData()
                    {
                        Control = ctrl,
                        TotalIndex = GetItemViewIndex(ctrl)
                    };
                    mSelectItemIndexDatas.Add(data);
                }

                mSelectItemIndexDatas.Sort(CompareSelectItemByIndex);
            }
        }
        private static int CompareSelectItemByIndex(SelectItemIndexData data1, SelectItemIndexData data2)
        {
            var count = System.Math.Min(data1.TotalIndex.Length, data2.TotalIndex.Length);
            for(int i=0; i<count; i++)
            {
                if (data1.TotalIndex[i] < data2.TotalIndex[i])
                    return -1;
                else if (data1.TotalIndex[i] > data2.TotalIndex[i])
                    return 1;
                else
                    continue;
            }
            if (data1.TotalIndex.Length < data2.TotalIndex.Length)
                return -1;
            else if (data1.TotalIndex.Length > data2.TotalIndex.Length)
                return 1;
            return 0;
        }
        int[] GetItemViewIndex(HierarchyItemView view)
        {
            if (view == null)
                return null;
            var idx = mChildList.IndexOf(view);
            if (idx >= 0)
                return new int[] { idx };
            var rootParent = view;
            int deepCount = 1;
            while(rootParent != null)
            {
                if (rootParent.Parent == null)
                    break;
                rootParent = rootParent.Parent;
                deepCount++;
            }

            var idxArray = new int[deepCount];
            idxArray[0] = mChildList.IndexOf(rootParent);
            GetTotalChildIndex(rootParent, view, ref idxArray, 1);
            return idxArray;
        }
        bool GetTotalChildIndex(HierarchyItemView parent, HierarchyItemView ctrl, ref int[] indexArray, int deep)
        {
            int idx = 0;
            foreach(var child in parent.ChildList)
            {
                indexArray[deep] = idx;
                idx++;
                if (child == ctrl)
                    return true;
                else if(child != null)
                {
                    if (GetTotalChildIndex(child, ctrl, ref indexArray, deep + 1))
                        return true;
                }
            }

            return false;
        }

        #endregion

        bool CheckIsDragContainsView(HierarchyItemView view)
        {
            foreach(var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
            {
                if (item == view)
                    return true;
            }
            return false;
        }
        bool CheckParentInDraggedItems(HierarchyItemView view)
        {
            if (view == null)
                return false;
            if (CheckIsDragContainsView(view.Parent))
                return true;
            return CheckParentInDraggedItems(view.Parent);
        }
        string GetDraggedItemName(EditorCommon.DragDrop.IDragAbleObject obj)
        {
            var view = obj as HierarchyItemView;
            if(view != null)
            {
                return view.Name;
            }
            var ctrl = obj as ControlView;
            if(ctrl != null)
            {
                return ctrl.Name;
            }
            return "";
        }

        Brush mErrorInfoBrush = new SolidColorBrush(Color.FromRgb(255, 113, 113));
        private void Rectangle_InsertChild_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            view.ChildInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"拖动的控件中包含{view.Name}";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if(CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                if(view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}无法放入多个对象";
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                    mDropType = enDropType.Invalid;
                }
                else
                {
                    e.Effects = DragDropEffects.Move;
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                    mDropType = enDropType.AddChild;
                }
            }
            else
            {
                e.Effects = DragDropEffects.Move;
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
                if (view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                {
                    var cc = view.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Border;
                    if(cc.Content != null)
                    {
                        HierarchyItemView contentView;
                        if(mChildDic.TryGetValue(cc.Content, out contentView))
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象{contentView.Name}";
                        else
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象";
                        mDropType = enDropType.ReplceContent;
                    }
                    else
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                        mDropType = enDropType.SetContent;
                    }
                }
                else
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                    mDropType = enDropType.AddChild;
                }
            }
        }

        private void Rectangle_InsertChild_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            view.ChildInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
        }

        private void Path_InsertUp_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            if (view.Parent == null)
                return;

            view.UpInsertLineVisible = Visibility.Visible;
            if(CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法插入自己之前";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            {
                e.Effects = DragDropEffects.None;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }

            e.Effects = DragDropEffects.Move;
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
            }
            else
            {
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
            }
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}插入{view.Name}之前";
            mDropType = enDropType.InsertBefore;
        }

        private void Path_InsertUp_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            view.UpInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
        }

        private void Path_InsertDown_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            if (view.Parent == null)
                return;
            view.DownInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法插入自己之后";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            e.Effects = DragDropEffects.Move;
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
            else
            {
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
            }
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}插入{view.Name}之后";
            mDropType = enDropType.InsertAfter;
        }

        private void Path_InsertDown_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            view.DownInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
        }

        async Task<EngineNS.UISystem.UIElement> CreateUIElement(EngineNS.CRenderContext rc, ControlView ctrlView)
        {
            var atts = ctrlView.UIControlType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), true);
            if (atts.Length <= 0)
                return null;
            var att = atts[0] as EngineNS.UISystem.Editor_UIControlInitAttribute;
            var init = System.Activator.CreateInstance(att.InitializerType) as EngineNS.UISystem.UIElementInitializer;
            var uiCtrl = System.Activator.CreateInstance(ctrlView.UIControlType) as EngineNS.UISystem.UIElement;
            await uiCtrl.Initialize(rc, init);
            return uiCtrl;
        }

        enum enDropType
        {
            None,
            AddChild,
            ReplceContent,
            SetContent,
            ReplceParentContent,
            InsertBefore,
            InsertAfter,
            Invalid,
        }
        enDropType mDropType;
        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            var noUse = DropProcess(sender, e);
        }
        private async Task DropProcess(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as HierarchyItemView;
            if (view == null)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            switch (mDropType)
            {
                case enDropType.AddChild:
                    {
                        var panel = view.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Panel;
                        var panelDR = panel.DesignRect;
                        List<EngineNS.UISystem.UIElement> addedControls = new List<EngineNS.UISystem.UIElement>();
                        List<EngineNS.UISystem.UIElement> selectedControls = new List<EngineNS.UISystem.UIElement>();
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            var itemView = item as HierarchyItemView;
                            if (itemView != null)
                            {
                                var currentUI = itemView.LinkedUIElement;

                                var oldParentElement = itemView.Parent.LinkedUIElement;
                                var oldParentPanel = oldParentElement as EngineNS.UISystem.Controls.Containers.Panel;
                                oldParentPanel.RemoveChild(currentUI);
                                var opItemDR = currentUI.DesignRect;
                                panel.AddChild(currentUI);
                                currentUI.Slot.ProcessSetContentDesignRect(ref opItemDR);

                                itemView.Parent.ChildList.Remove(itemView);
                                view.ChildList.Add(itemView);
                                itemView.Parent = view;

                                selectedControls.Add(currentUI);
                            }
                            else
                            {
                                var ctrlView = item as ControlView;
                                if (ctrlView != null)
                                {
                                    var ctrl = await CreateUIElement(rc, ctrlView);
                                    //panel.AddChild(ctrl);
                                    //var dr = ctrl.DesignRect;
                                    //var rect = new EngineNS.RectangleF(panelDR.X, panelDR.Y, dr.Width, dr.Height);
                                    //ctrl.Slot.ProcessSetContentDesignRect(ref rect);

                                    //var ctrlItemView = new HierarchyItemView(ctrl);
                                    //view.ChildList.Add(ctrlItemView);
                                    //mChildDic[ctrl] = ctrlItemView;
                                    //ctrlItemView.Parent = view;
                                    addedControls.Add(ctrl);
                                    //ctrlItemView.IsSelected = true;

                                    //selectedControls.Add(ctrl);
                                }
                            }
                        }
                        if (addedControls.Count > 0)
                            HostDesignPanel.BroadcastAddChildren(this, view.LinkedUIElement, panelDR.Location, addedControls.ToArray());
                        if(selectedControls.Count > 0)
                            HostDesignPanel.BroadcastSelectedUI(this, selectedControls.ToArray());
                    }
                    break;
                case enDropType.ReplceContent:
                case enDropType.SetContent:
                    {
                        var newContentControl = view.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Border;
                        var ccDR = newContentControl.DesignRect;
                        List<EngineNS.UISystem.UIElement> addedControls = new List<EngineNS.UISystem.UIElement>();
                        List<EngineNS.UISystem.UIElement> selectedControls = new List<EngineNS.UISystem.UIElement>();
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            var itemView = item as HierarchyItemView;
                            if(itemView != null)
                            {
                                var currentUI = itemView.LinkedUIElement;
                                var oldParentElement = itemView.Parent.LinkedUIElement;
                                var oldParentPanel = oldParentElement as EngineNS.UISystem.Controls.Containers.Panel;
                                oldParentPanel.RemoveChild(currentUI);
                                var opItemDR = currentUI.DesignRect;
                                newContentControl.Content = currentUI;
                                currentUI.Slot.ProcessSetContentDesignRect(ref opItemDR);

                                itemView.Parent.ChildList.Remove(itemView);
                                view.ChildList.Clear();
                                view.ChildList.Add(itemView);
                                itemView.Parent = view;

                                selectedControls.Add(currentUI);
                            }
                            else
                            {
                                var ctrlView = item as ControlView;
                                if(ctrlView != null)
                                {
                                    var ctrl = await CreateUIElement(rc, ctrlView);
                                    //newContentControl.Content = ctrl;
                                    //var dr = ctrl.DesignRect;
                                    //var rect = new EngineNS.RectangleF(ccDR.X, ccDR.Y, dr.Width, dr.Height);
                                    //ctrl.Slot.ProcessSetContentDesignRect(ref rect);

                                    //var ctrlItemView = new HierarchyItemView(ctrl);
                                    //view.ChildList.Clear();
                                    //view.ChildList.Add(ctrlItemView);
                                    //mChildDic[ctrl] = ctrlItemView;
                                    //ctrlItemView.Parent = view;
                                    addedControls.Add(ctrl);
                                    //ctrlItemView.IsSelected = true;

                                    //selectedControls.Add(ctrl);
                                }
                            }
                        }
                        if (addedControls.Count > 0)
                            HostDesignPanel.BroadcastAddChildren(this, view.LinkedUIElement, ccDR.Location, addedControls.ToArray());
                        if(selectedControls.Count > 0)
                            HostDesignPanel.BroadcastSelectedUI(this, selectedControls.ToArray());
                    }
                    break;
                case enDropType.ReplceParentContent:
                    throw new InvalidOperationException("未实现!");
                case enDropType.InsertBefore:
                case enDropType.InsertAfter:
                    {

                        var parentItem = view.Parent;
                        var parentPanel = parentItem.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Panel;
                        int insertIndex = 0;
                        if (mDropType == enDropType.InsertBefore)
                            insertIndex = parentPanel.ChildrenUIElements.IndexOf(view.LinkedUIElement);
                        else
                            insertIndex = parentPanel.ChildrenUIElements.IndexOf(view.LinkedUIElement) + 1;
                        var parentPanelDR = parentPanel.DesignRect;
                        List<EngineNS.UISystem.UIElement> addedControls = new List<EngineNS.UISystem.UIElement>();
                        List<EngineNS.UISystem.UIElement> selectedControls = new List<EngineNS.UISystem.UIElement>();
                        foreach(var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            var itemView = item as HierarchyItemView;
                            if(itemView != null)
                            {
                                var currentUI = itemView.LinkedUIElement;

                                var oldParentElement = itemView.Parent.LinkedUIElement;
                                var oldParentPanel = oldParentElement as EngineNS.UISystem.Controls.Containers.Panel;
                                oldParentPanel.RemoveChild(currentUI);
                                var opItemDR = currentUI.DesignRect;
                                parentPanel.InsertChild(insertIndex, currentUI);
                                currentUI.Slot.ProcessSetContentDesignRect(ref opItemDR);

                                itemView.Parent.ChildList.Remove(itemView);
                                if (insertIndex >= parentItem.ChildList.Count)
                                    parentItem.ChildList.Add(itemView);
                                else
                                    parentItem.ChildList.Insert(insertIndex, itemView);
                                itemView.Parent = parentItem;

                                selectedControls.Add(currentUI);
                            }
                            else
                            {
                                var ctrlView = item as ControlView;
                                if(ctrlView != null)
                                {
                                    var ctrl = await CreateUIElement(rc, ctrlView);
                                    //parentPanel.InsertChild(insertIndex, ctrl);
                                    //var dr = ctrl.DesignRect;
                                    //var rect = new EngineNS.RectangleF(parentPanelDR.X, parentPanelDR.Y, dr.Width, dr.Height);
                                    //ctrl.Slot.ProcessSetContentDesignRect(ref rect);

                                    //var ctrlItemView = new HierarchyItemView(ctrl);
                                    //if (insertIndex >= parentItem.ChildList.Count)
                                    //    parentItem.ChildList.Add(ctrlItemView);
                                    //else
                                    //    parentItem.ChildList.Insert(insertIndex, ctrlItemView);
                                    //mChildDic[ctrl] = ctrlItemView;
                                    //ctrlItemView.Parent = parentItem;
                                    addedControls.Add(ctrl);
                                    //ctrlItemView.IsSelected = true;

                                    //selectedControls.Add(ctrl);
                                }
                            }
                        }
                        if (addedControls.Count > 0)
                            HostDesignPanel.BroadcastAddChildren(this, parentItem.LinkedUIElement, parentPanelDR.Location, addedControls.ToArray(), insertIndex);
                        if(selectedControls.Count > 0)
                            HostDesignPanel.BroadcastSelectedUI(this, selectedControls.ToArray());
                    }
                    break;
                case enDropType.Invalid:
                    break;
            }
        }
    }
}
