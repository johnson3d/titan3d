using EditorCommon.Resources;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.SceneGraph;
using ResourceLibrary.Controls.Menu;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EditorCommon.Controls.Outliner
{
    public class OutlinerTreeViewOperation
    {
        public OutlinerControl OutlinerControl { get; set; }
        public void CreateMutiSelectContexMenu(ContextMenu menu)
        {

        }
        public static void CreateSeparator(ContextMenu menu)
        {
            menu.Items.Add(new Separator());
        }
        public static void CreateTextSeparator(ContextMenu menu, string text)
        {
            menu.Items.Add(new TextSeparator()
            {
                Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                Text = text,
            });
        }
        public static MenuItem CreateItemInMainMenu(string itemName, ContextMenu contextMenu, RoutedEventHandler handler)
        {
            MenuItem item = new MenuItem();
            item.Name = "MenuItem_ViewOperation_ContextMenu";
            item.Header = itemName;
            item.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa"));
            if (handler != null)
                item.Click += handler;
            contextMenu.Items.Add(item);
            return item;
        }
        public static MenuItem CreateItemInSecondaryMenu(string itemName, MenuItem menu, RoutedEventHandler handler)
        {
            MenuItem item = new MenuItem();
            item.Name = "MenuItem_ViewOperation_ContextMenu";
            item.Header = itemName;
            item.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa"));
            if (handler != null)
                item.Click += handler;
            menu.Items.Add(item);
            return item;
        }
        public TreeView OutlinerTreeView { get; set; } = null;
        public OutlinerItem SelectedOutlinerItem { get; set; } = null;
        public List<OutlinerItem> SelectedOutlinerItems { get; set; } = null;
        public List<TreeViewItem> SelectedTreeViewItems { get; set; } = null;
        public TreeViewItem SelectedTreeViewItem { get; set; } = null;

        public DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        public ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            return menu;
        }
        public ContextMenu CreateContextMenu(Style style)
        {
            var menu = new ContextMenu();
            menu.Style = style;

            return menu;
        }
        #region ExpandCollapseMenu
        public void CreateExpandCollapseMenu(ContextMenu menu, bool IsExpand)
        {
            CreateSeparator(menu);
            if (!IsExpand)
                CreateItemInMainMenu("ExpandSelected", menu, ExpandSelected_Click);
            else
                CreateItemInMainMenu("CollapseSelected", menu, CollapseSelected_Click);
            CreateItemInMainMenu("ExpandAll", menu, ExpandAll_Click);
            CreateItemInMainMenu("CollapseAll", menu, CollapseAll_Click); //CollapseAll
        }

        public void ExpandSelected_Click(object sender, RoutedEventArgs e)
        {
            ((TreeViewItem)SelectedTreeViewItem).ExpandSubtree();
        }
        public void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in OutlinerTreeView.Items)
            {
                DependencyObject dObject = OutlinerTreeView.ItemContainerGenerator.ContainerFromItem(item);
                ((TreeViewItem)dObject).ExpandSubtree();
            }

        }
        public void CollapseSelected_Click(object sender, RoutedEventArgs e)
        {
            CollapseTreeviewItems(((TreeViewItem)SelectedTreeViewItem));
        }
        public void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in OutlinerTreeView.Items)
            {
                DependencyObject dObject = OutlinerTreeView.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject));
            }
        }
        private void CollapseTreeviewItems(TreeViewItem Item)
        {
            Item.IsExpanded = false;
            foreach (var item in Item.Items)
            {
                DependencyObject dObject = Item.ItemContainerGenerator.ContainerFromItem(item);

                if (dObject != null)
                {
                    ((TreeViewItem)dObject).IsExpanded = false;

                    if (((TreeViewItem)dObject).HasItems)
                    {
                        CollapseTreeviewItems(((TreeViewItem)dObject));
                    }
                }
            }
        }
        #endregion ExpandCollapseMenu

        #region Select
        public void CreateSelectMenu(ContextMenu menu)
        {
            CreateTextSeparator(menu, "Select");
            CreateItemInMainMenu("SelectAll", menu, SelectAll_Click);
            CreateItemInMainMenu("UnSelectAll", menu, UnSelectAll_Click);
            CreateItemInMainMenu("InvertSelection", menu, InvertSelection_Click);
        }
        public void SelectAll_Click(object sender, RoutedEventArgs e)
        {

        }
        public void UnSelectAll_Click(object sender, RoutedEventArgs e)
        {

        }
        public void InvertSelection_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion Select


    }
    #region OutlinerItem 
    public class OutlinerItem : DependencyObject, INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject
    {
        public Brush PrefabeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF78C5EF"));
        public Brush DefaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa"));
        public Brush SceneBrush = new SolidColorBrush(Color.FromArgb(255, 106, 243, 85));
        public Brush ISceneNodeBrush = new SolidColorBrush(Color.FromArgb(255, 168, 243, 85));
        public bool NeedScrollToWhenLoaded { get; set; } = false;
        public virtual void RefreshForeground()
        {
            TreeViewItemForeground = DefaultBrush;
            for (int i = 0; i < ChildList.Count; ++i)
            {
                ChildList[i].RefreshForeground();
            }
        }
        public static OutlinerItem CreateOutlinerItem(Type type)
        {

            return null;
        }
        public bool CanHaveChild { get; set; } = true;
        public virtual bool CheckDrag()
        {
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            for (int i = 0; i < dragedObjectList.Count; ++i)
            {
                if (!(dragedObjectList[i] is OutlinerItem) && !(dragedObjectList[i] is IResourceInfoCreateActor))
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法放入{Name}之内,包含非法节点";
                    return false;
                }
            }
            return true;
        }
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
        #region DependencyProperty
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(OutlinerItem));
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(OutlinerItem));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(OutlinerItem));
        public ImageSource ImageIcon
        {
            get { return (ImageSource)GetValue(ImageIconProperty); }
            set { SetValue(ImageIconProperty, value); }
        }
        public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register("ImageIcon", typeof(ImageSource), typeof(OutlinerItem));
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(OutlinerItem));
        public bool TreeviewItemChecked
        {
            get { return (bool)GetValue(TreeviewItemCheckedProperty); }
            set { SetValue(TreeviewItemCheckedProperty, value); }
        }
        public static readonly DependencyProperty TreeviewItemCheckedProperty = DependencyProperty.Register("TreeviewItemChecked", typeof(bool), typeof(OutlinerItem), new UIPropertyMetadata(true, OnTreeviewItemCheckedProperty));
        private static void OnTreeviewItemCheckedProperty(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as OutlinerItem;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.TreeviewItemChecked = (bool)e.NewValue;
            ctrl.OnItemEnableChanged((bool)e.NewValue);
        }
        public Visibility TreeviewItemCheckBoxVisibility
        {
            get { return (Visibility)GetValue(TreeviewItemCheckBoxVisibilityProperty); }
            set { SetValue(TreeviewItemCheckBoxVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TreeviewItemCheckBoxVisibilityProperty = DependencyProperty.Register("TreeviewItemCheckBoxVisibility", typeof(Visibility), typeof(OutlinerItem));

        public virtual void OnItemEnableChanged(bool newValue)
        {

        }
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(OutlinerItem));

        protected void BindingDependencyProperty(DependencyProperty property, string path, object target)
        {
            BindingOperations.SetBinding(this, property, new Binding(path) { Source = target, Mode = BindingMode.TwoWay });
        }
        #endregion DependencyProperty
        #region Property
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
        protected bool mContoured = false;
        public virtual bool Contoured
        {
            get { return Contoured; }
            set
            {
                mContoured = value;
                OnPropertyChanged("Contoured");
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

        public System.Windows.Media.Brush TreeViewItemForeground
        {
            get { return (Brush)GetValue(TreeViewItemForegroundProperty); }
            set { SetValue(TreeViewItemForegroundProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemForegroundProperty = DependencyProperty.Register("TreeViewItemForeground", typeof(Brush), typeof(OutlinerItem), new UIPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa")), null));
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
        #endregion Property
        public OutlinerControl HostOutlinerControl { get; set; } = null;
        protected OutlinerItem mParent = null;
        public virtual OutlinerItem Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
            }
        }
        public bool IsBlongToPrefab()
        {
            if (Parent is PrefabOutlinerItem)
            {
                return true;
            }
            else
            {
                if (Parent is ActorOutlinerItem)
                {
                    var parent = Parent as ActorOutlinerItem;
                    if (parent.IsBlongToPrefab())
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }
        public bool IsBlongToPrefab(out PrefabOutlinerItem prefab)
        {
            if (Parent is PrefabOutlinerItem)
            {
                prefab = Parent as PrefabOutlinerItem;
                return true;
            }
            else
            {
                if (Parent is ActorOutlinerItem)
                {
                    var parent = Parent as ActorOutlinerItem;
                    if (parent.IsBlongToPrefab(out prefab))
                    {
                        return true;
                    }
                    else
                        return false;
                }
            }
            prefab = null;
            return false;
        }
        public bool IsChildOf(OutlinerItem item)
        {
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                if (item.ChildList[i] == this)
                {
                    return true;
                }
                if (IsChildOf(item.ChildList[i]))
                {
                    return true;
                }
            }
            return false;
        }
        ObservableCollection<OutlinerItem> mChildList = new ObservableCollection<OutlinerItem>();
        public ObservableCollection<OutlinerItem> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }
        public OutlinerItem(OutlinerControl outlinerControl)
        {
            HostOutlinerControl = outlinerControl;
            TreeViewItemForeground = DefaultBrush;
        }
        protected bool mItemOperationEnable = true;
        public void AddItem(OutlinerItem item)
        {
            if (ChildList.Contains(item))
                return;
            ChildList.Add(item);
            item.Parent = this;
        }
        public void RemoveItem(OutlinerItem item)
        {
            if (!ChildList.Contains(item))
                return;
            ChildList.Remove(item);
            item.Parent = null;
        }
        public virtual void Add(OutlinerItem item)
        {
            if (ChildList.Contains(item))
                return;
            item.Name = HostOutlinerControl.GeneratorValidNameInEditor(item.Name);
            ChildList.Add(item);
            item.Parent = this;
        }
        public virtual void Instert(int index, OutlinerItem item)
        {
            if (ChildList.Contains(item))
                return;
            item.Name = HostOutlinerControl.GeneratorValidNameInEditor(item.Name);
            ChildList.Insert(index, item);
            item.Parent = this;
        }
        public virtual void Remove(OutlinerItem item)
        {
            if (!ChildList.Contains(item))
                return;
            ChildList.Remove(item);
            item.Parent = null;
        }
        public virtual object GetShowPropertyObject()
        {
            return null;
        }
        public virtual async Task<OutlinerItem> Clone(EngineNS.CRenderContext rc)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        #region Edit
        public virtual void GenerateContexMenu(ContextMenu menu)
        {
            CreateEditMenu(menu);
        }
        public virtual void CreateEditMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            OutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
        }
        public void CreateEmptyActor_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.CreateEmptyActor(this);
        }
        public void Cut_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.Cut(this);
        }
        public void Copy_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.Copy(this);
        }
        public void Paste_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.Paste(this);
        }
        public void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.Duplicate(this);
        }
        public void Delete_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.Delete(this);
        }
        public void MakeGroup_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.MakeGroup(this);
        }
        #endregion Edit
    }
    public class PrefabOutlinerItem : OutlinerItem
    {
        public GActor Prefab { get; set; } = null;
        public PrefabOutlinerItem(GActor prefab, OutlinerControl outlinerControl) : base(outlinerControl)
        {
            Prefab = prefab;
            Prefab.OnAddChild += Actor_OnAddChild;
            Prefab.OnRemoveChild += Actor_OnRemoveChild;
            BindingDependencyProperty(NameProperty, "SpecialName", Prefab);
            BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", Prefab);
            TreeViewItemForeground = PrefabeBrush;
            ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/prefab_64x.png", UriKind.Relative));
            for (int i = 0; i < Prefab.GetChildrenUnsafe().Count; ++i)
            {
                var child = Prefab.GetChildrenUnsafe()[i];
                if (child is GPrefab)
                {
                    AddItem(new PrefabOutlinerItem(child, outlinerControl));
                }
                else if (child is GActor)
                {
                    AddItem(new ActorOutlinerItem(child, outlinerControl));
                }
            }
        }

        public override bool Contoured
        {
            get { return Contoured; }
            set
            {
                mContoured = value;
                Prefab.Selected = value;
                OnPropertyChanged("Contoured");
            }
        }
        public override async Task<OutlinerItem> Clone(EngineNS.CRenderContext rc)
        {
            var temp = new PrefabOutlinerItem(await Prefab.Clone(rc), HostOutlinerControl);
            return temp;
        }
        private void Actor_OnAddChild(GActor actor)
        {
            if (!mItemOperationEnable)
                return;
            var item = new ActorOutlinerItem(actor, HostOutlinerControl);
            AddItem(item);
        }
        private void Actor_OnRemoveChild(GActor actor)
        {
            if (!mItemOperationEnable)
                return;
            for (int i = 0; i < ChildList.Count; ++i)
            {
                var actorItem = ChildList[i] as ActorOutlinerItem;
                if (actorItem != null)
                {
                    if (actorItem.Actor == actor)
                    {
                        RemoveItem(actorItem);
                    }
                }
            }
        }

        public override void Add(OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            mItemOperationEnable = false;
            actorItem.Actor.SetParent(Prefab);
            base.Add(item);

            mItemOperationEnable = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (Prefab.Scene != null && !Prefab.Scene.World.AllActors.Contains(actorItem.Actor))
            {
                Prefab.Scene.World.AddActor(actorItem.Actor);
                Prefab.Scene.World.DefaultScene.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
        }
        public override void Instert(int index, OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            mItemOperationEnable = false;
            actorItem.Actor.SetParent(Prefab);
            base.Instert(index, item);

            mItemOperationEnable = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (Prefab.Scene != null && !Prefab.Scene.World.AllActors.Contains(actorItem.Actor))
            {
                Prefab.Scene.World.AddActor(actorItem.Actor);
                Prefab.Scene.World.DefaultScene.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
        }
        public override void Remove(OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            if (actorItem.Actor.Parent == Prefab)
            {
                mItemOperationEnable = false;
                actorItem.Actor.SetParent(null);
                mItemOperationEnable = true;
                if (Prefab.Scene != null)
                {
                    Prefab.Scene.World.RemoveActor(actorItem.Actor.ActorId);
                    Prefab.Scene.World.DefaultScene.RemoveActor(actorItem.Actor.ActorId);
                    EngineNS.CEngine.Instance.HitProxyManager.UnmapActor(actorItem.Actor);
                }
            }
            HostOutlinerControl.RemoveFromSelectedViews(item);
            base.Remove(item);
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Remove(item.ChildList[i]);
            }
        }
        public override object GetShowPropertyObject()
        {
            return Prefab;
        }
        public override bool CheckDrag()
        {
            var preStr = "";
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            if (dragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                for (int i = 0; i < dragedObjectList.Count; ++i)
                {
                    var view = dragedObjectList[i] as OutlinerItem;
                    if (view != null && !(view is ActorOutlinerItem))
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将 {preStr}放入{Name}之内,包含非法节点";
                        return false;
                    }
                }
            }
            else
            {
                var view = dragedObjectList[0] as OutlinerItem;
                if (view != null)
                {
                    preStr = view.Name;
                }
                if (dragedObjectList[0] is SceneOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景节点{preStr}放入{Name}之内";
                    return false;
                }
                if (dragedObjectList[0] is ISceneNodeOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景结构节点{preStr}放入{Name}之内";
                    return false;
                }
            }
            return base.CheckDrag();
        }
        #region GenerateContexMenu
        public override void GenerateContexMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Prefab");
            OutlinerTreeViewOperation.CreateItemInMainMenu("Focus", menu, Foucs_Click);
            base.GenerateContexMenu(menu);
        }
        public void Foucs_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.ViewPort.FocusShow(Prefab);
        }
        public override void CreateEditMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            OutlinerTreeViewOperation.CreateItemInMainMenu("CreateEmptyActor", menu, CreateEmptyActor_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Cut", menu, Cut_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Copy", menu, Copy_Click);
            if (HostOutlinerControl.CheckCanPaste(this))
                OutlinerTreeViewOperation.CreateItemInMainMenu("Paste", menu, Paste_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
            if (HostOutlinerControl.CheckCanMakeGroup())
            {
                OutlinerTreeViewOperation.CreateTextSeparator(menu, "Group");
                OutlinerTreeViewOperation.CreateItemInMainMenu("MakeGroup", menu, MakeGroup_Click);
            }
        }
        #endregion GenerateContexMenu
        public override void RefreshForeground()
        {
            TreeViewItemForeground = PrefabeBrush;
            base.RefreshForeground();
        }
    }
    public class ActorOutlinerItem : OutlinerItem
    {
        public GActor Actor { get; set; } = null;
        public override OutlinerItem Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
                if (value is PrefabOutlinerItem || IsBlongToPrefab())
                {
                    TreeViewItemForeground = PrefabeBrush;
                }
                else
                {
                    TreeViewItemForeground = DefaultBrush;
                }
            }
        }
        public override bool Contoured
        {
            get { return mContoured; }
            set
            {
                mContoured = value;
                Actor.Selected = value;
                OnPropertyChanged("Contoured");
            }
        }
        public override void RefreshForeground()
        {
            if (mParent is PrefabOutlinerItem || IsBlongToPrefab())
            {
                TreeViewItemForeground = PrefabeBrush;
            }
            else
            {
                TreeViewItemForeground = DefaultBrush;
            }
        }
        public ActorOutlinerItem(GActor actor, OutlinerControl outlinerControl) : base(outlinerControl)
        {
            Actor = actor;
            Actor.OnAddChild += Actor_OnAddChild;
            Actor.OnRemoveChild += Actor_OnRemoveChild;
            BindingDependencyProperty(NameProperty, "SpecialName", Actor);
            BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", Actor);
            ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/Actor_64x.png", UriKind.Relative));
            for (int i = 0; i < actor.GetChildrenUnsafe().Count; ++i)
            {
                var child = actor.GetChildrenUnsafe()[i];
                if (child is GPrefab)
                {
                    AddItem(new PrefabOutlinerItem(child, outlinerControl));
                }
                else if (child is GActor)
                {
                    AddItem(new ActorOutlinerItem(child, outlinerControl));
                }
            }
        }
        public override async Task<OutlinerItem> Clone(EngineNS.CRenderContext rc)
        {
            var temp = new ActorOutlinerItem(await Actor.Clone(rc), HostOutlinerControl);
            return temp;
        }


        private void Actor_OnAddChild(GActor actor)
        {
            if (actor.Parent != null)
                return;
            if (!mItemOperationEnable)
                return;
            var item = new ActorOutlinerItem(actor, HostOutlinerControl);
            AddItem(item);
        }
        private void Actor_OnRemoveChild(GActor actor)
        {
            if (!mItemOperationEnable)
                return;
            for (int i = 0; i < ChildList.Count; ++i)
            {
                var actorItem = ChildList[i] as ActorOutlinerItem;
                if (actorItem != null)
                {
                    if (actorItem.Actor == actor)
                    {
                        RemoveItem(actorItem);
                    }
                }
            }
        }

        public override void Add(OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            mItemOperationEnable = false;
            actorItem.Actor.SetParent(Actor);
            base.Add(item);

            mItemOperationEnable = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (Actor.Scene != null && !Actor.Scene.World.AllActors.Contains(actorItem.Actor))
            {
                Actor.Scene.World.AddActor(actorItem.Actor);
                Actor.Scene.World.DefaultScene.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
        }
        public override void Instert(int index, OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            mItemOperationEnable = false;
            actorItem.Actor.SetParent(Actor);
            base.Instert(index, item);

            mItemOperationEnable = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (Actor.Scene != null && !Actor.Scene.World.AllActors.Contains(actorItem.Actor))
            {
                Actor.Scene.World.AddActor(actorItem.Actor);
                Actor.Scene.World.DefaultScene.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
        }
        public override void Remove(OutlinerItem item)
        {
            var actorItem = item as ActorOutlinerItem;
            if (actorItem == null)
                return;
            if (actorItem.Actor.Parent == Actor)
            {
                mItemOperationEnable = false;
                actorItem.Actor.SetParent(null);
                mItemOperationEnable = true;
            }
            base.Remove(item);
            if (Actor.Scene != null && Actor.Scene.World.AllActors.Contains(actorItem.Actor))
            {
                Actor.Scene.World.RemoveActor(actorItem.Actor.ActorId);
                Actor.Scene.World.DefaultScene.RemoveActor(actorItem.Actor.ActorId);
                EngineNS.CEngine.Instance.HitProxyManager.UnmapActor(actorItem.Actor);
                HostOutlinerControl.RemoveFromSelectedViews(item);
            }
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Remove(item.ChildList[i]);
            }
        }
        public override object GetShowPropertyObject()
        {
            return Actor;
        }
        public override bool CheckDrag()
        {
            var preStr = "";
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            if (dragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                for (int i = 0; i < dragedObjectList.Count; ++i)
                {
                    var view = dragedObjectList[i] as OutlinerItem;
                    if (!(view is ActorOutlinerItem))
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将 {preStr}放入{Name}之内,包含非法节点";
                        return false;
                    }
                }
            }
            else
            {
                var view = dragedObjectList[0] as OutlinerItem;
                if (view != null)
                {
                    preStr = view.Name;
                }
                if (dragedObjectList[0] is SceneOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景节点{preStr}放入{Name}之内";
                    return false;
                }
                if (dragedObjectList[0] is ISceneNodeOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景结构节点{preStr}放入{Name}之内";
                    return false;
                }
            }
            return base.CheckDrag();
        }
        #region GenerateContexMenu
        public override void GenerateContexMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Actor");
            OutlinerTreeViewOperation.CreateItemInMainMenu("Focus", menu, Foucs_Click);
            base.GenerateContexMenu(menu);
        }
        public void Foucs_Click(object sender, RoutedEventArgs e)
        {
            HostOutlinerControl.FocusCommand(this);
        }

        public override void CreateEditMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            OutlinerTreeViewOperation.CreateItemInMainMenu("CreateEmptyActor", menu, CreateEmptyActor_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Cut", menu, Cut_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Copy", menu, Copy_Click);
            if (HostOutlinerControl.CheckCanPaste(this))
                OutlinerTreeViewOperation.CreateItemInMainMenu("Paste", menu, Paste_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            OutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
            if (HostOutlinerControl.CheckCanMakeGroup())
            {
                OutlinerTreeViewOperation.CreateTextSeparator(menu, "Group");
                OutlinerTreeViewOperation.CreateItemInMainMenu("MakeGroup", menu, MakeGroup_Click);
            }
        }
        #endregion GenerateContexMenu
    }
    public class WorldOutlinerItem : OutlinerItem
    {
        public EngineNS.GamePlay.GWorld World { get; set; } = null;
        public WorldOutlinerItem(EngineNS.GamePlay.GWorld world, OutlinerControl outlinerControl) : base(outlinerControl)
        {
            World = world;
            ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/World_64x.png", UriKind.Relative));
            TreeViewItemForeground = System.Windows.Media.Brushes.AliceBlue;
        }

    }
    public class SceneOutlinerItem : OutlinerItem
    {
        public GSceneGraph SceneGraph { get; set; } = null;
        public SceneOutlinerItem(GSceneGraph sceneGraph, OutlinerControl outlinerControl) : base(outlinerControl)
        {
            SceneGraph = sceneGraph;
            SceneGraph.OnAddActor += SceneGraph_OnAddActor;
            SceneGraph.OnReAddActor += SceneGraph_OnReAddActor;
            SceneGraph.OnAddDynamicActor += SceneGraph_OnAddDynamicActor;
            SceneGraph.OnRemoveActor += SceneGraph_OnRemoveActor;
            SceneGraph.OnRemoveDynamicActor += SceneGraph_OnRemoveDynamicActor;
            BindingDependencyProperty(NameProperty, "Name", SceneGraph);
            BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", SceneGraph);
            ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/World_64x.png", UriKind.Relative));
            TreeViewItemForeground = SceneBrush;
        }
        public override bool Contoured
        {
            get { return Contoured; }
            set
            {
                mContoured = value;
                foreach (var it in SceneGraph.Actors)
                {
                    GActor actor = it.Value;
                    actor.Selected = value;
                }
                OnPropertyChanged("Contoured");
            }
        }
        public bool IsContainActorItem(ObservableCollection<OutlinerItem> items, GActor actor)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i] is ActorOutlinerItem)
                {
                    var actorItem = items[i] as ActorOutlinerItem;
                    if (actorItem.Actor == actor)
                        return true;
                }
                if (items[i] is PrefabOutlinerItem)
                {
                    var actorItem = items[i] as PrefabOutlinerItem;
                    if (actorItem.Prefab == actor)
                        return true;
                }
                if (IsContainActorItem(items[i].ChildList, actor))
                    return true;
            }
            return false;
        }
        public bool IsBlongToPrefab(GActor actor)
        {
            if (actor.Parent != null)
            {
                if (actor.Parent is GPrefab)
                    return true;
                return IsBlongToPrefab(actor.Parent);
            }
            else
            {
                return false;
            }
        }
        private void SceneGraph_OnAddDynamicActor(GActor actor)
        {
            if (actor.Tag is InvisibleInOutliner)
                return;
            if (IsContainActorItem(ChildList, actor))
                return;
            if (IsBlongToPrefab(actor))
                return;
            if (!mItemOperationEnable)
                return;
            AddActorItemRecursion(this, actor);
            //IsExpanded = true;
        }

        private void SceneGraph_OnAddActor(GActor actor)
        {
            if (actor.Tag is InvisibleInOutliner)
                return;
            if (IsContainActorItem(ChildList, actor))
                return;
            if (IsBlongToPrefab(actor))
                return;
            if (!mItemOperationEnable)
                return;
            AddActorItemRecursion(this, actor);
        }
        private void SceneGraph_OnReAddActor(GActor actor)
        {
            if (actor.Tag is InvisibleInOutliner)
                return;
            if (IsContainActorItem(ChildList, actor))
                return;
            if (!mItemOperationEnable)
                return;
            if(actor.Parent == null)
                AddActorItemRecursion(this, actor);
            var parent = HostOutlinerControl.GetOutlinerItemByActor(this, actor.Parent);
            AddActorItemRecursion(parent, actor);
        }
        void AddActorItemRecursion(OutlinerItem parent, GActor actor)
        {
            OutlinerItem item = null;
            if (actor is GPrefab)
            {
                item = new PrefabOutlinerItem(actor, HostOutlinerControl);
            }
            else
            {
                item = new ActorOutlinerItem(actor, HostOutlinerControl);
            }
            item.IsExpanded = true;
            parent.AddItem(item);
            //for (int i = 0; i < actor.GetChildrenUnsafe().Count; ++i)
            //{
            //    AddActorItemRecursion(item, actor.GetChildrenUnsafe()[i]);
            //}
        }
        private void SceneGraph_OnRemoveDynamicActor(GActor actor)
        {
            if (!mItemOperationEnable)
                return;
            var item = HostOutlinerControl.GetOutlinerItemByActor(this, actor);
            if (item != null)
            {
                if (item.Parent == null)
                {

                }
                else
                {
                    item.Parent.RemoveItem(item);
                    //RemoveItem(item);
                    HostOutlinerControl.RemoveFromSelectedViews(item);
                }
            }
        }
        List<OutlinerUndoRedoItem> outlinerUndoRedoItems = new List<OutlinerUndoRedoItem>();
        private void SceneGraph_OnRemoveActor(GActor actor)
        {
            if (!mItemOperationEnable)
                return;

            var item = HostOutlinerControl.GetOutlinerItemByActor(this, actor);
            if (item != null)
            {
                if (item.Parent == null)
                {

                }
                else
                {
                    item.Parent.RemoveItem(item);
                    //RemoveItem(item);
                    HostOutlinerControl.RemoveFromSelectedViews(item);
                }
            }

        }



        public override void Add(OutlinerItem item)
        {
            mItemOperationEnable = false;
            base.Add(item);

            mItemOperationEnable = true;

            IsExpanded = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (item is ActorOutlinerItem)
            {
                var actorItem = item as ActorOutlinerItem;
                if (actorItem == null)
                    return;
                SceneGraph.World.AddActor(actorItem.Actor);
                SceneGraph.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
            if (item is PrefabOutlinerItem)
            {
                var actorItem = item as PrefabOutlinerItem;
                if (actorItem == null)
                    return;
                SceneGraph.World.AddActor(actorItem.Prefab);
                SceneGraph.AddActor(actorItem.Prefab);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Prefab);
            }
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneGraph.ChildrenNode.Add(isnItem.SceneNode);
            }
        }
        public override void Instert(int index, OutlinerItem item)
        {
            mItemOperationEnable = false;
            base.Instert(index, item);

            mItemOperationEnable = true;

            IsExpanded = true;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Add(item.ChildList[i]);
            }
            if (item is ActorOutlinerItem)
            {
                var actorItem = item as ActorOutlinerItem;
                if (actorItem == null)
                    return;
                SceneGraph.World.AddActor(actorItem.Actor);
                SceneGraph.AddActor(actorItem.Actor);
                EngineNS.CEngine.Instance.HitProxyManager.MapActor(actorItem.Actor);
            }
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneGraph.ChildrenNode.Insert(index, isnItem.SceneNode);
            }
        }
        public override void Remove(OutlinerItem item)
        {
            mItemOperationEnable = false;
            if (item is PrefabOutlinerItem)
            {
                var actorItem = item as PrefabOutlinerItem;
                if (actorItem == null)
                    return;
                SceneGraph.World.RemoveActor(actorItem.Prefab.ActorId);
                SceneGraph.RemoveActor(actorItem.Prefab);
            }
            if (item is ActorOutlinerItem)
            {
                var actorItem = item as ActorOutlinerItem;
                if (actorItem == null)
                    return;
                SceneGraph.World.RemoveActor(actorItem.Actor.ActorId);
                SceneGraph.RemoveActor(actorItem.Actor);
            }
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneGraph.ChildrenNode.Remove(isnItem.SceneNode);
            }
            mItemOperationEnable = true;
            base.Remove(item);
            HostOutlinerControl.RemoveFromSelectedViews(item);
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                item.Remove(item.ChildList[i]);
            }
        }
        public override object GetShowPropertyObject()
        {
            return SceneGraph;
        }
        public override bool CheckDrag()
        {
            var preStr = "";
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            if (dragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                for (int i = 0; i < dragedObjectList.Count; ++i)
                {
                    var view = dragedObjectList[i] as OutlinerItem;
                    if (view is SceneOutlinerItem)
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将 {preStr}放入{Name}之内,包含非法节点";
                        return false;
                    }
                }
            }
            else if (dragedObjectList.Count == 1)
            {
                var view = dragedObjectList[0] as OutlinerItem;
                if (view != null)
                {
                    preStr = view.Name;
                }
                if (dragedObjectList[0] is SceneOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景节点{preStr}放入{Name}之内";
                    return false;
                }
            }
            return base.CheckDrag();
        }
        public override void CreateEditMenu(ContextMenu menu)
        {
            OutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            OutlinerTreeViewOperation.CreateItemInMainMenu("CreateEmptyActor", menu, CreateEmptyActor_Click);
            if (HostOutlinerControl.CheckCanPaste(this))
                OutlinerTreeViewOperation.CreateItemInMainMenu("Paste", menu, Paste_Click);
            if (HostOutlinerControl.CheckCanMakeGroup())
            {
                OutlinerTreeViewOperation.CreateTextSeparator(menu, "Group");
                OutlinerTreeViewOperation.CreateItemInMainMenu("MakeGroup", menu, MakeGroup_Click);
            }
        }
        public override void RefreshForeground()
        {
            TreeViewItemForeground = SceneBrush;
            base.RefreshForeground();
        }
    }
    public class ISceneNodeOutlinerItem : OutlinerItem
    {
        public ISceneNode SceneNode { get; set; } = null;
        public ISceneNodeOutlinerItem(ISceneNode sceneNode, OutlinerControl outlinerControl) : base(outlinerControl)
        {
            SceneNode = sceneNode;
            BindingDependencyProperty(NameProperty, "Name", SceneNode);
            BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", SceneNode);
            ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/LevelBounds_64x.png", UriKind.Relative));
            TreeViewItemForeground = ISceneNodeBrush;
        }
        public override void Add(OutlinerItem item)
        {
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneNode.ChildrenNode.Add(isnItem.SceneNode);
            }
            base.Add(item);
        }
        public override void Instert(int index, OutlinerItem item)
        {
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneNode.ChildrenNode.Insert(index, isnItem.SceneNode);
            }
            base.Instert(index, item);
        }
        public override void Remove(OutlinerItem item)
        {
            if (item is ISceneNodeOutlinerItem)
            {
                var isnItem = item as ISceneNodeOutlinerItem;
                SceneNode.ChildrenNode.Remove(isnItem.SceneNode);
            }
            base.Remove(item);
        }
        public override object GetShowPropertyObject()
        {
            return SceneNode;
        }
        public override bool CheckDrag()
        {
            var preStr = "";
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            if (dragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                for (int i = 0; i < dragedObjectList.Count; ++i)
                {
                    var view = dragedObjectList[i] as OutlinerItem;
                    if (!(view is ISceneNodeOutlinerItem))
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将 {preStr}放入{Name}之内,包含非法节点";
                        return false;
                    }
                }
            }
            else
            {
                var view = dragedObjectList[0] as OutlinerItem;
                if (view != null)
                {
                    preStr = view.Name;
                }
                if (dragedObjectList[0] is SceneOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将场景节点{preStr}放入{Name}之内";
                    return false;
                }
                if (dragedObjectList[0] is ActorOutlinerItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将Actor节点{preStr}放入{Name}之内";
                    return false;
                }
            }
            return base.CheckDrag();
        }
        public override void CreateEditMenu(ContextMenu menu)
        {

        }
        public override void RefreshForeground()
        {
            TreeViewItemForeground = ISceneNodeBrush;
            base.RefreshForeground();
        }
    }
    #endregion OutlinerItem 
}
