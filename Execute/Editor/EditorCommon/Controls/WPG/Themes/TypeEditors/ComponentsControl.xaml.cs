using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ResourceLibrary;
using EngineNS.GamePlay.Component;
using System.ComponentModel;
using EngineNS.GamePlay.Actor;
using EditorCommon.Resources;
using ResourceLibrary.Controls.Menu;

namespace WPG.Themes.TypeEditors
{
    #region 

    #endregion
    #region ComCtrlTreeViewItem
    public enum ViewItemType
    {
        Actor,
        ComponentContainer,
        Component,
    }
    public class ComponentOutlinerTreeViewOperation
    {
        public ComponentsControl ComponentsControl { get; set; }
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
        public ComCtrlTreeViewItem SelectedOutlinerItem { get; set; } = null;
        public List<ComCtrlTreeViewItem> SelectedOutlinerItems { get; set; } = null;
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
    public class Helper
    {
        public static ImageSource GetIcon(GComponent component)
        {
            var atts = component.GetType().GetCustomAttributes(typeof(EngineNS.Editor.Editor_ComponentClassIconAttribute), true);
            if (atts.Length > 0)
            {
                var att = atts[0] as EngineNS.Editor.Editor_ComponentClassIconAttribute;
                if (!string.IsNullOrEmpty(att.IconRNameStr))
                {
                    var rName = EngineNS.RName.GetRName(att.IconRNameStr, att.IconRNameType);
                    var imgs = EditorCommon.ImageInit.SyncGetImage(rName.Address);
                    return imgs[0];
                }
            }
            return null;
        }
    }
    public class ComCtrlTreeViewItem : DependencyObject, INotifyPropertyChanged, EditorCommon.DragDrop.IDragAbleObject
    {
        public Brush PrefabeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF78C5EF"));
        public Brush DefaultBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa"));
        public ComponentsControl HostComponentsControl { get; set; } = null;
        public bool NeedScrollToWhenLoaded { get; set; } = false;
        public static ComCtrlTreeViewItem CreateComCtrlTreeViewItem(ComCtrlTreeViewItem parent, ComponentsControl componentsControl, Type type)
        {
            GComponent component = Activator.CreateInstance(type) as GComponent;
            var atts = type.GetCustomAttributes(typeof(CustomConstructionParamsAttribute), false);
            EngineNS.GamePlay.Component.GComponent.GComponentInitializer initializer = null;
            if (atts.Length > 0)
            {
                for (int i = 0; i < atts.Length; i++)
                {
                    var ccAtt = atts[i] as CustomConstructionParamsAttribute;
                    if (ccAtt != null)
                    {
                        initializer = Activator.CreateInstance(ccAtt.ConstructionParamsType) as EngineNS.GamePlay.Component.GComponent.GComponentInitializer;
                        break;
                    }
                }
            }
            if (initializer != null)
            {
                GActor hostActor = null;
                IComponentContainer componentContainer = null;
                if (parent is ActorComCtrlTreeViewItem)
                {
                    var actorItem = parent as ActorComCtrlTreeViewItem;
                    hostActor = actorItem.Actor;
                    componentContainer = actorItem.Actor;
                }
                if (parent is ComponentContainerComCtrlTreeViewItem)
                {
                    var item = parent as ComponentContainerComCtrlTreeViewItem;
                    componentContainer = item.Container;
                    hostActor = (item.Container as GComponent).Host;
                }
                var noUse = component.SetInitializer(EngineNS.CEngine.Instance.RenderContext, hostActor, componentContainer, initializer);
            }
            if (component is GComponentsContainer)
            {
                return new ComponentContainerComCtrlTreeViewItem(component as GComponentsContainer, componentsControl);
            }
            else if (component is GComponent)
            {
                return new ComponentComCtrlTreeViewItem(component, componentsControl);
            }
            return null;
        }
        public bool CanHaveChild { get; set; } = true;
        public virtual bool CheckDrag()
        {
            var dragedObjectList = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            for (int i = 0; i < dragedObjectList.Count; ++i)
            {
                if (!(dragedObjectList[i] is ComCtrlTreeViewItem) && !(dragedObjectList[i] is IResourceInfoCreateComponent))
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
        #region DP
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ComCtrlTreeViewItem));
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(ComCtrlTreeViewItem));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(ComCtrlTreeViewItem));
        public ImageSource ImageIcon
        {
            get { return (ImageSource)GetValue(ImageIconProperty); }
            set { SetValue(ImageIconProperty, value); }
        }
        public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register("ImageIcon", typeof(ImageSource), typeof(ComCtrlTreeViewItem));
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ComCtrlTreeViewItem));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ComCtrlTreeViewItem));
        public bool TreeviewItemChecked
        {
            get { return (bool)GetValue(TreeviewItemCheckedProperty); }
            set { SetValue(TreeviewItemCheckedProperty, value); }
        }
        public static readonly DependencyProperty TreeviewItemCheckedProperty = DependencyProperty.Register("TreeviewItemChecked", typeof(bool), typeof(ComCtrlTreeViewItem), new UIPropertyMetadata(true, OnTreeviewItemCheckedProperty));
        private static void OnTreeviewItemCheckedProperty(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as ComCtrlTreeViewItem;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.TreeviewItemChecked = (bool)e.NewValue;
        }
        public Visibility TreeviewItemCheckBoxVisibility
        {
            get { return (Visibility)GetValue(TreeviewItemCheckBoxVisibilityProperty); }
            set { SetValue(TreeviewItemCheckBoxVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TreeviewItemCheckBoxVisibilityProperty = DependencyProperty.Register("TreeviewItemCheckBoxVisibility", typeof(Visibility), typeof(ComCtrlTreeViewItem));
        public System.Windows.Media.Brush TreeViewItemForeground
        {
            get { return (Brush)GetValue(TreeViewItemForegroundProperty); }
            set { SetValue(TreeViewItemForegroundProperty, value); }
        }
        public static readonly DependencyProperty TreeViewItemForegroundProperty = DependencyProperty.Register("TreeViewItemForeground", typeof(Brush), typeof(ComCtrlTreeViewItem), new UIPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFbababa"))));
        #endregion DP
        #region Properties
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
        ComCtrlTreeViewItem mParent = null;
        public ComCtrlTreeViewItem Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
            }
        }

        ObservableCollection<ComCtrlTreeViewItem> mChildList = new ObservableCollection<ComCtrlTreeViewItem>();
        public ObservableCollection<ComCtrlTreeViewItem> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }
        #endregion
        #region Edit
        public virtual async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc, EngineNS.GamePlay.Actor.GActor host, IComponentContainer hostContainer)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public virtual async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public virtual void GenerateContexMenu(ContextMenu menu)
        {
            CreateEditMenu(menu);
        }
        public virtual void CreateEditMenu(ContextMenu menu)
        {
            ComponentOutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
        }
        public void Cut_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.Cut(this);
        }
        public void Copy_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.Copy(this);
        }
        public void Paste_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.Paste(this);
        }
        public void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.Duplicate(this);
        }
        public void Delete_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.Delete(this);
        }
        #endregion Edit
        public ComCtrlTreeViewItem(ComponentsControl componentsControl)
        {
            HostComponentsControl = componentsControl;
            TreeViewItemForeground = DefaultBrush;
        }
        public bool IsChildOf(ComCtrlTreeViewItem item)
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
        public virtual void AddComponent(ComCtrlTreeViewItem item)
        {


        }
        public virtual void InsertComponent(int index, ComCtrlTreeViewItem item)
        {

        }
        public virtual void RemoveComponent(ComCtrlTreeViewItem item)
        {

        }
        public virtual GComponent GetComponent()
        {
            return null;
        }
        public virtual object GetShowPropertyObject()
        {
            return null;
        }
        protected void BindingDependencyProperty(DependencyProperty property, string path, object target)
        {
            BindingOperations.SetBinding(this, property, new Binding(path) { Source = target, Mode = BindingMode.TwoWay });
        }
    }
    public class ActorComCtrlTreeViewItem : ComCtrlTreeViewItem
    {
        public GActor Actor { get; set; } = null;
        public ActorComCtrlTreeViewItem(GActor actor, ComponentsControl componentsControl) : base(componentsControl)
        {
            Actor = actor;
            BindingDependencyProperty(NameProperty, "SpecialName", actor);
            TypeName = $"(1{actor.GetType().FullName})";
            if (actor is GPrefab)
                ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/Prefab_64x.png", UriKind.Relative));
            else
                ImageIcon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/AssetIcons/Actor_64x.png", UriKind.Relative));
            TreeviewItemCheckBoxVisibility = Visibility.Visible;
            BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", actor);
        }
        public override void AddComponent(ComCtrlTreeViewItem item)
        {
            ChildList.Add(item);
            item.Parent = this;
            Actor.AddComponent(item.GetComponent());
        }
        public override void InsertComponent(int index, ComCtrlTreeViewItem item)
        {
            ChildList.Insert(index, item);
            item.Parent = this;
            Actor.InsertComponent(index, item.GetComponent());
        }
        public override void RemoveComponent(ComCtrlTreeViewItem item)
        {
            ChildList.Remove(item);
            item.Parent = null;
            Actor.RemoveComponent(item.GetComponent());
        }
        public override object GetShowPropertyObject()
        {
            return Actor;
        }
        public override void GenerateContexMenu(ContextMenu menu)
        {
            ComponentOutlinerTreeViewOperation.CreateTextSeparator(menu, "Actor");
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Focus", menu, Foucs_Click);
            base.GenerateContexMenu(menu);
        }
        public void Foucs_Click(object sender, RoutedEventArgs e)
        {
            HostComponentsControl.FocusCommand(this);
        }
        public override void CreateEditMenu(ContextMenu menu)
        {
            if (HostComponentsControl.CheckCanPaste(this))
                ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Paste", menu, Paste_Click);
        }
    }
    public class ComponentContainerComCtrlTreeViewItem : ComCtrlTreeViewItem
    {
        public GComponentsContainer Container { get; set; } = null;
        public ComponentContainerComCtrlTreeViewItem(GComponentsContainer container, ComponentsControl componentsControl) : base(componentsControl)
        {
            Container = container;
            ImageIcon = Helper.GetIcon(container as GComponent);
            BindingDependencyProperty(NameProperty, "SpecialName", container);
            TypeName = $"({container.GetType().FullName})";
            for (int i = 0; i < container.Components.Count; ++i)
            {
                if (container.Components[i] is EngineNS.GamePlay.Component.GPlacementComponent)
                    continue;
                if (container.Components[i] is GComponentsContainer)
                {
                    var item = new ComponentContainerComCtrlTreeViewItem(Container.Components[i] as GComponentsContainer, componentsControl);
                    item.Parent = this;
                    ChildList.Add(item);
                }
                else
                {
                    var item = new ComponentComCtrlTreeViewItem(Container.Components[i], componentsControl);
                    item.Parent = this;
                    ChildList.Add(item);
                }
            }
            if (Container is GVisualComponent)
            {
                TreeviewItemCheckBoxVisibility = Visibility.Visible;
                BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", Container);
            }
            else
            {
                TreeviewItemCheckBoxVisibility = Visibility.Collapsed;
            }
        }
        public override async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc, EngineNS.GamePlay.Actor.GActor host, IComponentContainer hostContainer)
        {
            var temp = new ComponentContainerComCtrlTreeViewItem(await Container.CloneComponent(rc, host, hostContainer) as GComponentsContainer, HostComponentsControl);
            return temp;
        }
        public override async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc)
        {
            var temp = new ComponentContainerComCtrlTreeViewItem(await Container.CloneComponent(rc, Container.Host, Container.HostContainer) as GComponentsContainer, HostComponentsControl);
            return temp;
        }
        public override void AddComponent(ComCtrlTreeViewItem item)
        {
            ChildList.Add(item);
            item.Parent = this;
            Container.AddComponent(item.GetComponent());
        }
        public override void InsertComponent(int index, ComCtrlTreeViewItem item)
        {
            ChildList.Insert(index, item);
            item.Parent = this;
            Container.InsertComponent(index, item.GetComponent());
        }
        public override void RemoveComponent(ComCtrlTreeViewItem item)
        {
            ChildList.Remove(item);
            item.Parent = null;
            Container.RemoveComponent(item.GetComponent());
        }
        public override GComponent GetComponent()
        {
            return Container as GComponent;
        }
        public override object GetShowPropertyObject()
        {
            return (Container as GComponent).GetShowPropertyObject();
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
                    var view = dragedObjectList[i] as ComCtrlTreeViewItem;
                    if (view != null && view is ActorComCtrlTreeViewItem)
                    {
                        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将 {preStr}放入{Name}之内,包含非法节点";
                        return false;
                    }
                }
            }
            else
            {
                var view = dragedObjectList[0] as ComCtrlTreeViewItem;
                if (view != null)
                {
                    preStr = view.Name;
                }
                if (dragedObjectList[0] is ActorComCtrlTreeViewItem)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将节点{preStr}放入{Name}之内";
                    return false;
                }
            }
            return base.CheckDrag();
        }
        public override void CreateEditMenu(ContextMenu menu)
        {
            ComponentOutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Cut", menu, Cut_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Copy", menu, Copy_Click);
            if (HostComponentsControl.CheckCanPaste(this))
                ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Paste", menu, Paste_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
        }
    }
    public class ComponentComCtrlTreeViewItem : ComCtrlTreeViewItem
    {
        public GComponent Component { get; set; } = null;
        public ComponentComCtrlTreeViewItem(GComponent component, ComponentsControl componentsControl) : base(componentsControl)
        {
            Component = component;
            ImageIcon = Helper.GetIcon(component);
            BindingDependencyProperty(NameProperty, "SpecialName", component);
            TypeName = $"({component.GetType().FullName})";
            CanHaveChild = false;
            if (component is GVisualComponent)
            {
                TreeviewItemCheckBoxVisibility = Visibility.Visible;
                BindingDependencyProperty(TreeviewItemCheckedProperty, "Visible", component);
            }
            else
            {
                TreeviewItemCheckBoxVisibility = Visibility.Collapsed;
            }
        }

        public override GComponent GetComponent()
        {
            return Component;
        }
        public override object GetShowPropertyObject()
        {
            return Component.GetShowPropertyObject();
        }
        public override bool CheckDrag()
        {
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"节点{Name}无法包含子节点";
            return false;
        }
        public override async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc, EngineNS.GamePlay.Actor.GActor host, IComponentContainer hostContainer)
        {
            var temp = new ComponentComCtrlTreeViewItem(await Component.CloneComponent(rc, host, hostContainer), HostComponentsControl);
            return temp;
        }
        public override async Task<ComCtrlTreeViewItem> Clone(EngineNS.CRenderContext rc)
        {
            var temp = new ComponentComCtrlTreeViewItem(await Component.CloneComponent(rc, Component.Host, Component.HostContainer), HostComponentsControl);
            return temp;
        }
        public override void CreateEditMenu(ContextMenu menu)
        {
            ComponentOutlinerTreeViewOperation.CreateTextSeparator(menu, "Edit");
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Cut", menu, Cut_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Copy", menu, Copy_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Duplicate", menu, Duplicate_Click);
            ComponentOutlinerTreeViewOperation.CreateItemInMainMenu("Delete", menu, Delete_Click);
        }
    }
    #endregion ComCtrlTreeViewItem

    /// <summary>
    /// Interaction logic for ComponentsControl.xaml
    /// </summary>
    public partial class ComponentsControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public string UndoRedoKey
        {
            get;
            set;
        }
        public EditorCommon.ViewPort.ViewPortControl ViewPort { get; set; } = null;
        ObservableCollection<ComCtrlTreeViewItem> mChildList = new ObservableCollection<ComCtrlTreeViewItem>();
        public ObservableCollection<ComCtrlTreeViewItem> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }

        public PropertyGrid LinkedPropertyGrid { get; set; }

        #region TreeViewItem
        public void SetActors(List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            // 目前只支持显示一个
            if (actors != null && actors.Count == 1)
            {
                InitActorTreeView(actors[0], this);
                ClearSelectedItemViews();
            }
            else
                ChildList.Clear();
        }
        public void SetActor(GActor actor)
        {
            // 目前只支持显示一个
            if (actor != null)
            {
                InitActorTreeView(actor, this);
                ClearSelectedItemViews();
            }
            else
                ChildList.Clear();
        }
        void ClearSelectedItemViews()
        {
            mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
            mSelectedItemViews.Clear();
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }
        void InitActorTreeView(EngineNS.GamePlay.Actor.GActor actor, ComponentsControl componentsControl, bool showPlacementComp = false)
        {
            ChildList.Clear();

            var actorItem = new ActorComCtrlTreeViewItem(actor, componentsControl);
            actorItem.IsExpanded = true;
            ChildList.Add(actorItem);

            foreach (var comp in actor.Components)
            {
                // PlacementComponent默认不显示
                if (!showPlacementComp && (comp is EngineNS.GamePlay.Component.GPlacementComponent))
                    continue;


                if (comp is GComponentsContainer)
                {
                    InitComponentsTreeView(actorItem, comp as GComponentsContainer, componentsControl, showPlacementComp);
                }
                else
                {
                    ComponentComCtrlTreeViewItem compItem = new ComponentComCtrlTreeViewItem(comp, componentsControl);
                    actorItem.AddComponent(compItem);
                }
            }
        }

        void InitComponentsTreeView(ComCtrlTreeViewItem parent, GComponentsContainer container, ComponentsControl componentsControl, bool showPlacementComp = false)
        {
            var containerItem = new ComponentContainerComCtrlTreeViewItem(container, componentsControl);
            containerItem.IsExpanded = true;
            parent.AddComponent(containerItem);
            //foreach (var comp in container.Components)
            //{
            //    // PlacementComponent默认不显示
            //    if (!showPlacementComp && (comp is EngineNS.GamePlay.Component.GPlacementComponent))
            //        continue;
            //    if (comp is GComponentsContainer)
            //    {
            //        InitComponentsTreeView(containerItem, comp as GComponentsContainer, componentsControl, showPlacementComp);
            //    }
            //    else
            //    {
            //        ComponentComCtrlTreeViewItem compItem = new ComponentComCtrlTreeViewItem(comp, componentsControl);
            //        containerItem.AddComponent(compItem);
            //    }
            //}
        }
        #endregion TreeViewItem
        public ComponentsControl()
        {
            InitializeComponent();
            TreeView_Components.ItemsSource = ChildList;

            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;

            BindingOperations.SetBinding(searchBox, SearchBox.SearchBox.SearchTextProperty, new Binding("FilterString") { Source = this });
        }
        //Search actors..
        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value.ToLower();
                //InitializeCreateMenu();
                var itemsControl = BasicAssetMenuHead.Parent as ItemsControl;
                ShowItemsWithFilter(itemsControl.Items, mFilterString);
                OnPropertyChanged("FilterString");
            }
        }
        //ItemsControl


        private bool ShowItemsWithFilter(ItemCollection itemsitems, string filter)
        {
            bool retValue = false;
            foreach (var item in itemsitems)
            {
                var menuItem = item as TypeMenuItem;
                if (menuItem == null)
                    continue;

                if (string.IsNullOrEmpty(filter))
                {
                    menuItem.Visibility = Visibility.Visible;
                    //item.HighLightString = filter;
                    if (menuItem.Items.Count > 0)
                    {
                        ShowItemsWithFilter(menuItem.Items, filter);
                    }
                    
                }
                else
                {
                    if (menuItem.Items.Count == 0)
                    {
                        var header = menuItem.Header as EditorCommon.Controls.ResourceBrowser.BaseResControl;
                        if (header != null && header.Text.ToLower().IndexOf(filter) > -1)
                        {
                            menuItem.Visibility = System.Windows.Visibility.Visible;
                            retValue = true;
                        }
                        else
                        {
                            // 根据名称拼音筛选
                            if (header != null && header.Text.ToLower().IndexOf(filter) > -1)
                            {
                                menuItem.Visibility = Visibility.Visible;
                                retValue = true;
                            }
                            else
                                menuItem.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        bool bFind = ShowItemsWithFilter(menuItem.Items, filter);
                        if (bFind == false)
                            menuItem.Visibility = System.Windows.Visibility.Collapsed;
                        else
                        {
                            menuItem.Visibility = Visibility.Visible;
                            retValue = true;
                        }
                    }
                }
            }
            return retValue;
        }

        public DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F:
                    {
                        FocusCommand(null);
                    }
                    break;
                case Key.Delete:
                    {
                        DeleteCommand();
                    }
                    break;
                case Key.C:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            Copy(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.X:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            Cut(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.V:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            PasteCommand(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.D:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            DuplicateCommand();
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Z:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Undo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Y:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Redo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
            }
        }

        #region CreateComponentMenu
        private void AddComponent_Click(object sender, RoutedEventArgs e)
        {
            var item = TreeView_Components.SelectedItem as ComponentComCtrlTreeViewItem;
            if (item != null)
                return;
            InitializeCreateMenu();
        }
        public void createMenu(ItemsControl itemsControl, Type type)
        {
            if (!type.IsSubclassOf(typeof(EngineNS.GamePlay.Component.GComponent)))
                return;

            if (type.ToString().IndexOf(mFilterString, StringComparison.OrdinalIgnoreCase) < 0)
                return;

            //有的组件actor中只能有一份
            bool needload = true;
            var atts = type.GetCustomAttributes(typeof(CustomConstructionParamsAttribute), false);

            string describe = "";
            string group = null;
            string displayName = "";
            if (atts.Length > 0)
            {
                for (int i = 0; i < atts.Length; i++)
                {
                    CustomConstructionParamsAttribute ccAtt = atts[i] as CustomConstructionParamsAttribute;
                    if (ccAtt != null)
                    {
                        describe = ccAtt.Describe;
                        group = ccAtt.ComponentGroup;
                        displayName = ccAtt.ComponentDisplayName;
                        //if (ccAtt.LoadOnce)
                        //{
                        //    foreach (var component in ChildList[0].Actor.Components)
                        //    {
                        //        if (component.GetType().Equals(type))
                        //        {
                        //            needload = false;
                        //            break;
                        //        }
                        //    }
                        //}


                        break;
                    }
                }
            }

            if (needload == false)
                return;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = type.ToString();
            }
            MenuItem baseMenuItem = new MenuItem()
            {
                Name = "MenuItem_ComponentsControl_CreateMenu_",// + displayName,
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                Header = new EditorCommon.Controls.ResourceBrowser.BaseResControl()
                {
                    //Icon = info.ResInfo.ResourceIcon,
                    //ResourceBrush = info.ResInfo.ResourceTypeBrush,
                    Text = displayName,
                },
                Height = 24,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetHasOffset(baseMenuItem, false);
            if (group != null)
            {
                List<string> menuNames = new List<string>();
                menuNames.Add(group);
                menuNames.Add(displayName);
                GenerateCreateMenu(itemsControl.Items, menuNames, type, baseMenuItem, describe);
            }
            else
            {
                var menuNames = new List<string>() { "Common", type.ToString() };
                GenerateCreateMenu(itemsControl.Items, menuNames, type, baseMenuItem, describe);
            }
        }
        public void InitializeCreateMenu()
        {
            if (ChildList.Count == 0)
                return;

            var itemsControl = BasicAssetMenuHead.Parent as ItemsControl;
            var baseIdx = itemsControl.Items.IndexOf(BasicAssetMenuHead);

            for (int i = itemsControl.Items.Count - 1; i > baseIdx; i--)
            {
                itemsControl.Items.RemoveAt(i);
            }
            try
            {
                var types = EngineNS.Rtti.RttiHelper.GetTypes();

                foreach (var type in types)
                {
                    if (type.GetInterface(typeof(EngineNS.Macross.IMacrossType).FullName) != null)
                        continue;
                    createMenu(itemsControl, type);
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        private void GenerateCreateMenu(ItemCollection items, List<string> menuNames, Type type, MenuItem baseMenuItem, string describe)
        {
            if (items.Count == 0 || menuNames == null)
            {
                GenerateCreateMenuItem(items, menuNames, type, baseMenuItem, describe);
            }
            else
            {
                bool bFind = false;
                foreach (var item in items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem == null)
                        continue;

                    EditorCommon.Controls.ResourceBrowser.BaseResControl baserescontrol = menuItem.Header as EditorCommon.Controls.ResourceBrowser.BaseResControl;
                    if (baserescontrol.Text == menuNames[0])
                    {
                        if (menuNames.Count == 1)
                        {
                            GenerateCreateMenuItem(items, menuNames, type, baseMenuItem, describe);
                        }
                        else
                        {
                            menuNames.RemoveAt(0);
                            GenerateCreateMenu(menuItem.Items, menuNames, type, baseMenuItem, describe);
                        }

                        bFind = true;
                        break;
                    }
                }

                if (!bFind)
                {
                    GenerateCreateMenuItem(items, menuNames, type, baseMenuItem, describe);
                }
            }
        }
        public class TypeMenuItem : MenuItem
        {
            public Type Type { get; set; }
        }
        private void GenerateCreateMenuItem(ItemCollection items, List<string> menuNames, Type type, MenuItem baseMenuItem, string describe)
        {
            TypeMenuItem item = null;
            if (menuNames != null)
            {
                item = new TypeMenuItem()
                {
                    Name = "MenuItem_ComponentsControl_CreateMenuItem_",// + menuNames[0],
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                    Header = new EditorCommon.Controls.ResourceBrowser.BaseResControl()
                    {
                        //Icon = info.ResInfo.ResourceIcon,
                        //ResourceBrush = info.ResInfo.ResourceTypeBrush,
                        Text = menuNames[0],
                    },
                    Height = 32,
                    Type = type,
                };

                items.Add(item);
            }
            if (menuNames != null && menuNames.Count > 1)
            {
                menuNames.RemoveAt(0);
                GenerateCreateMenu(item.Items, menuNames, type, baseMenuItem, describe);
            }
            else
            {
                var action = new System.Windows.RoutedEventHandler((sender, e) =>
                {
                    if (ChildList.Count == 0)
                        return;
                    var menuItem = sender as TypeMenuItem;
                    var ComponentTreeViewItem = TreeView_Components.SelectedItem as ComCtrlTreeViewItem;
                    if (ComponentTreeViewItem == null)
                    {
                        EditorCommon.MessageBox.Show("需要先选中组件， 再进行操作！");
                        return;
                    }
                    var childItem = ComCtrlTreeViewItem.CreateComCtrlTreeViewItem(ComponentTreeViewItem, this, menuItem.Type);
                    childItem.NeedScrollToWhenLoaded = true;
                    ComponentTreeViewItem.IsExpanded = true;
                    ComponentTreeViewItem.AddComponent(childItem);
                    e.Handled = true;
                });
                if (menuNames != null)
                {
                    item.Header = new EditorCommon.Controls.ResourceBrowser.BaseResControl()
                    {
                        Text = menuNames[0],
                    };
                    ResourceLibrary.Controls.Menu.MenuAssist.SetHasOffset(item, false);
                    item.Click += action;
                    item.ToolTip = describe;
                }
            }
        }
        #endregion CreateComponentMenu
        #region 控件拖动
        Point mMouseDownPos = new Point();
        FrameworkElement mMouseDownElement;
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as ComCtrlTreeViewItem;
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
            var item = element.DataContext as ComCtrlTreeViewItem;
            if (item == null)
                return;
            item.UpInsertLineVisible = Visibility.Hidden;
            item.DownInsertLineVisible = Visibility.Hidden;
            item.ChildInsertLineVisible = Visibility.Hidden;
        }
        void ExpandAllTreeViewParent(ComCtrlTreeViewItem view)
        {
            if (view == null)
                return;
            var viewStack = new Stack<ComCtrlTreeViewItem>();
            viewStack.Push(view);
            var viewParent = view.Parent;
            while (viewParent != null)
            {
                viewStack.Push(viewParent);
                viewParent = viewParent.Parent;
            }

            ItemsControl item = TreeView_Components;
            while (viewStack.Count > 0)
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
        ObservableCollection<ComCtrlTreeViewItem> mSelectedItemViews = new ObservableCollection<ComCtrlTreeViewItem>();
        public ObservableCollection<ComCtrlTreeViewItem> SelectedItemViews
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

            List<object> properties = new List<object>();
            for (int i = 0; i < mSelectedItemViews.Count; i++)
            {
                properties.Add(mSelectedItemViews[i].GetShowPropertyObject());
            }
            LinkedPropertyGrid.Instance = properties;
        }

        private void SelectControl(ComCtrlTreeViewItem view)
        {
            if (view == null)
                return;

            view.IsSelected = true;
        }
        private void UnSelectControl(ComCtrlTreeViewItem view, bool withChild = false)
        {
            if (view == null)
                return;

            view.IsSelected = false;

            if (withChild)
            {
                foreach (var ctrl in view.ChildList)
                {
                    UnSelectControl(ctrl, withChild);
                }
            }
        }
        class SelectItemIndexData
        {
            public ComCtrlTreeViewItem Control;
            public int[] TotalIndex;  // 保证在拖动时选中对象的顺序不变
        }
        List<SelectItemIndexData> mSelectItemIndexDatas = new List<SelectItemIndexData>();
        public void UpdateSelectItems(ObservableCollection<ComCtrlTreeViewItem> selectedItems, ObservableCollection<ComCtrlTreeViewItem> unSelectedItems)
        {
            if (unSelectedItems != null)
            {
                foreach (var ctrl in unSelectedItems)
                {
                    UnSelectControl(ctrl);
                    foreach (var data in mSelectItemIndexDatas)
                    {
                        if (data.Control == ctrl)
                        {
                            mSelectItemIndexDatas.Remove(data);
                            break;
                        }
                    }
                }
            }
            if (selectedItems != null)
            {
                foreach (var ctrl in selectedItems)
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
            for (int i = 0; i < count; i++)
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
        int[] GetItemViewIndex(ComCtrlTreeViewItem view)
        {
            if (view == null)
                return null;
            var idx = mChildList.IndexOf(view);
            if (idx >= 0)
                return new int[] { idx };
            var rootParent = view;
            int deepCount = 1;
            while (rootParent != null)
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
        bool GetTotalChildIndex(ComCtrlTreeViewItem parent, ComCtrlTreeViewItem ctrl, ref int[] indexArray, int deep)
        {
            int idx = 0;
            foreach (var child in parent.ChildList)
            {
                indexArray[deep] = idx;
                idx++;
                if (child == ctrl)
                    return true;
                else if (child != null)
                {
                    if (GetTotalChildIndex(child, ctrl, ref indexArray, deep + 1))
                        return true;
                }
            }

            return false;
        }

        #endregion

        bool CheckIsDragContainsView(ComCtrlTreeViewItem view)
        {
            foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
            {
                if (item == view)
                    return true;
            }
            return false;
        }
        bool CheckParentInDraggedItems(ComCtrlTreeViewItem view)
        {
            if (view == null)
                return false;
            if (CheckIsDragContainsView(view.Parent))
                return true;
            return CheckParentInDraggedItems(view.Parent);
        }
        string GetDraggedItemName(EditorCommon.DragDrop.IDragAbleObject obj)
        {
            var view = obj as ComCtrlTreeViewItem;
            if (view != null)
            {
                return view.Name;
            }
            return "";
        }

        Brush mErrorInfoBrush = new SolidColorBrush(Color.FromRgb(255, 113, 113));
        private void Rectangle_InsertChild_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            view.ChildInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"拖动的控件中包含{view.Name}";
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
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                //if (view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                //{
                //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}无法放入多个对象";
                //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                //    mDropType = enDropType.Invalid;
                //}
                //else
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
                //if (view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                //{
                //    var cc = view.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Border;
                //    if (cc.Content != null)
                //    {
                //        ComponentTreeViewItem contentView;
                //        if (mChildDic.TryGetValue(cc.Content, out contentView))
                //            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象{contentView.Name}";
                //        else
                //            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象";
                //        mDropType = enDropType.ReplceContent;
                //    }
                //    else
                //    {
                //        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                //        mDropType = enDropType.SetContent;
                //    }
                //}
                //else
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                    mDropType = enDropType.AddChild;
                }
            }
            e.Handled = true;
        }

        private void Rectangle_InsertChild_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            view.ChildInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
            e.Handled = true;
        }

        private void Path_InsertUp_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            if (view.Parent == null)
                return;

            view.UpInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
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
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            //if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            //{
            //    e.Effects = DragDropEffects.None;
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
            //    mDropType = enDropType.Invalid;
            //    return;
            //}

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
            e.Handled = true;
        }

        private void Path_InsertUp_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            view.UpInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
            e.Handled = true;
        }

        private void Path_InsertDown_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
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
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (view.Parent == null)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"非法操作";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            else if (!view.Parent.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            //if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            //{
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
            //    mDropType = enDropType.Invalid;
            //    return;
            //}
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
            e.Handled = true;
        }

        private void Path_InsertDown_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            view.DownInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
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
            e.Handled = true;
        }
        private async Task DropProcess(object sender, DragEventArgs e)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var element = sender as FrameworkElement;
            var view = element.DataContext as ComCtrlTreeViewItem;
            if (view == null)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            switch (mDropType)
            {
                case enDropType.AddChild:
                    {
                        List<ComsUndoRedoItem> items = new List<ComsUndoRedoItem>();
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            if (item is ComCtrlTreeViewItem)
                            {
                                var itemView = item as ComCtrlTreeViewItem;
                                if (itemView != null && view != itemView.Parent)
                                {
                                    items.Add(new ComsUndoRedoItem(itemView.Parent, itemView));
                                }
                            }
                            if (item is IResourceInfoCreateComponent)
                            {
                                var rInfoIetm = item as IResourceInfoCreateComponent;
                                IComponentContainer componentContainer = null;
                                if (view is ActorComCtrlTreeViewItem)
                                {
                                    var actorItem = view as ActorComCtrlTreeViewItem;
                                    componentContainer = actorItem.Actor;
                                }
                                else if (view is ComponentContainerComCtrlTreeViewItem)
                                {
                                    var containerItem = view as ComponentContainerComCtrlTreeViewItem;
                                    componentContainer = containerItem.Container;
                                }
                                var comp = await rInfoIetm.CreateComponent(componentContainer);
                                ComCtrlTreeViewItem newItem = null;
                                if (comp is GComponentsContainer)
                                {
                                    newItem = new ComponentContainerComCtrlTreeViewItem(comp as GComponentsContainer, this);
                                }
                                else
                                {
                                    newItem = new ComponentComCtrlTreeViewItem(comp, this);
                                }
                                items.Add(new ComsUndoRedoItem(null, newItem));
                            }
                        }
                        var redo = new Action<object>((obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                if (items[i].LastParent != null)
                                {
                                    items[i].LastParent.RemoveComponent(items[i].Item);
                                }
                                view.IsExpanded = true;
                                view.AddComponent(items[i].Item);
                                view.IsExpanded = true;
                            }
                        });
                        redo.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                var pair = items[i];
                                pair.Item.Parent.RemoveComponent(pair.Item);
                                if (pair.LastParent != null)
                                {
                                    pair.LastParent.AddComponent(pair.Item);
                                    pair.LastParent.IsExpanded = true;
                                }
                            }

                        }, $"添加{items.Count}个对象");
                    }
                    break;
                case enDropType.ReplceContent:
                case enDropType.SetContent:
                    {
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            var itemView = item as ComCtrlTreeViewItem;
                            if (itemView != null)
                            {
                                itemView.Parent.ChildList.Remove(itemView);
                                view.ChildList.Clear();
                                view.ChildList.Add(itemView);
                                view.IsExpanded = true;
                                itemView.Parent = view;
                            }
                            else
                            {

                            }
                        }
                    }
                    break;
                case enDropType.ReplceParentContent:
                    throw new InvalidOperationException("未实现!");
                case enDropType.InsertBefore:
                case enDropType.InsertAfter:
                    {
                        List<ComsInsertChildUndoRedoItem> items = new List<ComsInsertChildUndoRedoItem>();
                        var parentItem = view.Parent;
                        int insertIndex = 0;
                        insertIndex = parentItem.ChildList.IndexOf(view);
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            if (item is ComCtrlTreeViewItem)
                            {
                                var itemView = item as ComCtrlTreeViewItem;
                                if (itemView != null)
                                {
                                    if (itemView.Parent != parentItem)
                                    {
                                        insertIndex++;
                                    }
                                    items.Add(new ComsInsertChildUndoRedoItem(insertIndex, itemView.Parent, itemView));
                                }
                                else
                                {

                                }
                            }
                            if (item is IResourceInfoCreateComponent)
                            {
                                var rInfoIetm = item as IResourceInfoCreateComponent;
                                IComponentContainer componentContainer = null;
                                if (view is ActorComCtrlTreeViewItem)
                                {
                                    var actorItem = view as ActorComCtrlTreeViewItem;
                                    componentContainer = actorItem.Actor;
                                }
                                else if (view is ComponentContainerComCtrlTreeViewItem)
                                {
                                    var containerItem = view as ComponentContainerComCtrlTreeViewItem;
                                    componentContainer = containerItem.Container;
                                }
                                var comp = await rInfoIetm.CreateComponent(componentContainer);
                                ComCtrlTreeViewItem newItem = null;
                                if (comp is GComponentsContainer)
                                {
                                    newItem = new ComponentContainerComCtrlTreeViewItem(comp as GComponentsContainer, this);
                                }
                                else
                                {
                                    newItem = new ComponentComCtrlTreeViewItem(comp, this);
                                }
                                insertIndex++;
                                items.Add(new ComsInsertChildUndoRedoItem(insertIndex, null, newItem));
                            }
                        }

                        var redo = new Action<object>((obj) =>
                        {
                            foreach (var item in items)
                            {
                                parentItem.IsExpanded = true;
                                if (item.LastParent != null)
                                    item.LastParent.RemoveComponent(item.Item);
                                if (item.Index >= parentItem.ChildList.Count)
                                    parentItem.AddComponent(item.Item);
                                else
                                    parentItem.InsertComponent(item.Index, item.Item);
                                parentItem.IsExpanded = true;
                            }
                        });
                        redo.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                var pair = items[i];
                                pair.Item.Parent.RemoveComponent(pair.Item);
                                if (pair.LastParent != null)
                                {
                                    pair.LastParent.AddComponent(pair.Item);
                                    pair.LastParent.IsExpanded = true;
                                }
                            }

                        }, $"插入{items.Count}个对象");
                    }
                    break;
                case enDropType.Invalid:
                    break;
            }
        }
        private void TreeView_Components_DragEnter(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is ComCtrlTreeViewItem)
                    return;
            }

            if (ChildList.Count == 0)
                return;
            var element = sender as FrameworkElement;
            var view = ChildList[0];
            if (view == null)
                return;
            view.ChildInsertLineVisible = Visibility.Visible;
            if (!view.CheckDrag())
            {
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
            mDropType = enDropType.AddChild;
        }

        private void TreeView_Components_DragLeave(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is ComCtrlTreeViewItem)
                    return;
            }

            if (ChildList.Count == 0)
                return;
            var element = sender as FrameworkElement;
            var view = ChildList[0];
            if (view == null)
                return;
            view.ChildInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
        }

        private void TreeView_Components_Drop(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is ComCtrlTreeViewItem)
                    return;
            }

            if (ChildList.Count == 0)
                return;
            var element = sender as FrameworkElement;
            var view = ChildList[0];
            if (view == null)
                return;
            Action action = async () =>
             {
                 switch (mDropType)
                 {
                     case enDropType.AddChild:
                         {
                             foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                             {
                                 if (item is ComCtrlTreeViewItem)
                                 {
                                     var itemView = item as ComCtrlTreeViewItem;
                                     if (itemView != null && view != itemView.Parent)
                                     {
                                         itemView.Parent.RemoveComponent(itemView);
                                         view.AddComponent(itemView);
                                         view.IsExpanded = true;
                                     }
                                     else
                                     {

                                     }
                                 }
                                 if (item is IResourceInfoCreateComponent)
                                 {
                                     var rInfoIetm = item as IResourceInfoCreateComponent;
                                     IComponentContainer componentContainer = null;
                                     if (view is ActorComCtrlTreeViewItem)
                                     {
                                         var actorItem = view as ActorComCtrlTreeViewItem;
                                         componentContainer = actorItem.Actor;
                                     }
                                     else if (view is ComponentContainerComCtrlTreeViewItem)
                                     {
                                         var containerItem = view as ComponentContainerComCtrlTreeViewItem;
                                         componentContainer = containerItem.Container;
                                     }
                                     var comp = await rInfoIetm.CreateComponent(componentContainer);
                                     ComCtrlTreeViewItem newItem = null;
                                     if (comp is GComponentsContainer)
                                     {
                                         newItem = new ComponentContainerComCtrlTreeViewItem(comp as GComponentsContainer, this);
                                     }
                                     else
                                     {
                                         newItem = new ComponentComCtrlTreeViewItem(comp, this);
                                     }
                                     view.AddComponent(newItem);
                                     view.IsExpanded = true;
                                 }
                             }
                         }
                         break;
                     case enDropType.Invalid:
                         break;
                 }
                 view.ChildInsertLineVisible = Visibility.Hidden;
                 mDropType = enDropType.None;
             };
            action.Invoke();
        }
        #endregion 控件拖动
        #region Edit
        public List<ComCtrlTreeViewItem> CutClipsBoard = new List<ComCtrlTreeViewItem>();
        public List<ComCtrlTreeViewItem> CopyClipsBoard = new List<ComCtrlTreeViewItem>();
        ComponentOutlinerTreeViewOperation mOutlinerTreeViewOperation = new ComponentOutlinerTreeViewOperation();
        public bool CheckCanPaste(ComCtrlTreeViewItem item)
        {
            if (mIsCut)
            {
                return (CheckCanPasteTo(item) && CheckPasteCircle(item));
            }
            else
            {
                return CheckCanPasteTo(item);
            }
        }
        bool CheckCanPasteTo(ComCtrlTreeViewItem item)
        {
            if (item is ActorComCtrlTreeViewItem || item is ComponentContainerComCtrlTreeViewItem)
            {
                List<ComCtrlTreeViewItem> tempList = null;
                if (mIsCut)
                {
                    tempList = CutClipsBoard;
                }
                else
                {
                    tempList = CopyClipsBoard;
                }
                if (tempList.Count == 0)
                    return false;
                for (int i = 0; i < tempList.Count; ++i)
                {
                    if (!(tempList[i] is ComponentComCtrlTreeViewItem) && !(tempList[i] is ComponentContainerComCtrlTreeViewItem))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        bool CheckPasteCircle(ComCtrlTreeViewItem pasteTarget)
        {
            List<ComCtrlTreeViewItem> items = null;
            if (mIsCut)
                items = CutClipsBoard;
            else
                items = CopyClipsBoard;
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i] == pasteTarget)
                    return true;
                if (pasteTarget.IsChildOf(items[i]))
                    return true;
            }
            return false;
        }
        bool mIsCut = false;
        public void Cut(ComCtrlTreeViewItem sender)
        {
            CutClipsBoard.Clear();
            CutClipsBoard.AddRange(SelectedItemViews);
            mIsCut = true;
        }
        public void Copy(ComCtrlTreeViewItem sender)
        {
            CopyClipsBoard.Clear();
            Action action = async () =>
            {
                for (int i = 0; i < SelectedItemViews.Count; ++i)
                {
                    CopyClipsBoard.Add(await SelectedItemViews[i].Clone(EngineNS.CEngine.Instance.RenderContext));
                }
            };
            action.Invoke();
            mIsCut = false;
        }
        public void Paste(ComCtrlTreeViewItem sender)
        {
            PasteCommand(sender);
        }
        public void Duplicate(ComCtrlTreeViewItem sender)
        {
            DuplicateCommand();

        }
        public void Delete(ComCtrlTreeViewItem sender)
        {
            DeleteCommand();
        }
        private void TreeViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mOutlinerTreeViewOperation.SelectedOutlinerItems = mSelectedItemViews.ToList();

            var treeViewItem = mOutlinerTreeViewOperation.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                ContextMenu menu = null;
                mOutlinerTreeViewOperation.SelectedTreeViewItem = treeViewItem;
                mOutlinerTreeViewOperation.SelectedOutlinerItem = treeViewItem.Header as ComCtrlTreeViewItem;
                //if (mSelectedItemViews.Count == 1)
                {
                    menu = mOutlinerTreeViewOperation.CreateContextMenu(TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as System.Windows.Style);
                    mOutlinerTreeViewOperation.SelectedOutlinerItem.GenerateContexMenu(menu);
                    ComponentOutlinerTreeViewOperation.CreateSeparator(menu);
                    mOutlinerTreeViewOperation.CreateSelectMenu(menu);
                }
                //else
                //{
                //    mOutlinerTreeViewOperation.CreateMutiSelectContexMenu(menu);
                //}
                mOutlinerTreeViewOperation.CreateExpandCollapseMenu(menu, treeViewItem.IsExpanded);
                treeViewItem.ContextMenu = menu;
                menu.Visibility = Visibility.Visible;
                menu.PlacementTarget = treeViewItem;
                menu.IsOpen = true;
                e.Handled = true;
            }
        }
        #endregion Edit
        #region Command
        public class ComsInsertChildUndoRedoItem : ComsUndoRedoItem
        {
            public int Index { get; set; }
            public ComsInsertChildUndoRedoItem(int index, ComCtrlTreeViewItem lastParent, ComCtrlTreeViewItem item) : base(lastParent, item)
            {
                Index = index;
                LastParent = lastParent;
                Item = item;
            }
        }
        public class ComsUndoRedoItem
        {
            public ComCtrlTreeViewItem LastParent { get; set; }
            public ComCtrlTreeViewItem Item { get; set; }
            public ComsUndoRedoItem(ComCtrlTreeViewItem lastParent, ComCtrlTreeViewItem item)
            {
                LastParent = lastParent;
                Item = item;
            }
        }
        public void FocusCommand(ComCtrlTreeViewItem sender)
        {
            if (sender is ActorComCtrlTreeViewItem)
            {
                ViewPort.FocusShow((sender as ActorComCtrlTreeViewItem).Actor);
            }
        }
        public async void PasteCommand(ComCtrlTreeViewItem sender)
        {
            if (SelectedItemViews.Count == 0)
                return;
            ComCtrlTreeViewItem pasteTarget;
            if (SelectedItemViews[SelectedItemViews.Count - 1].Parent == null)
                pasteTarget = SelectedItemViews[SelectedItemViews.Count - 1];
            else
                pasteTarget = SelectedItemViews[SelectedItemViews.Count - 1].Parent;
            if (!CheckCanPasteTo(pasteTarget))
                return;
            if (CheckPasteCircle(pasteTarget))
                return;
            if (mIsCut)
            {
                List<ComsUndoRedoItem> addRemoveItems = new List<ComsUndoRedoItem>();
                for (int i = 0; i < CutClipsBoard.Count; ++i)
                {
                    if (CutClipsBoard[i].Parent != null)
                    {
                        addRemoveItems.Add(new ComsUndoRedoItem(CutClipsBoard[i].Parent, CutClipsBoard[i]));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                var redo = new Action<object>((obj) =>
                {
                    for (int i = 0; i < addRemoveItems.Count; ++i)
                    {
                        if (addRemoveItems[i].LastParent != null)
                        {
                            addRemoveItems[i].LastParent.RemoveComponent(addRemoveItems[i].Item);
                            pasteTarget.AddComponent(addRemoveItems[i].Item);
                            pasteTarget.IsExpanded = true;
                        }
                    }
                });
                redo.Invoke(null);
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                {
                    for (int i = 0; i < addRemoveItems.Count; ++i)
                    {
                        var pair = addRemoveItems[i];
                        pair.Item.Parent.RemoveComponent(pair.Item);
                        pair.LastParent.AddComponent(pair.Item);
                        pair.LastParent.IsExpanded = true;
                    }

                }, $"剪切粘贴{addRemoveItems.Count}个对象");
            }
            else
            {
                var coms = new List<ComCtrlTreeViewItem>();
                GActor host = null;
                IComponentContainer container = null;
                if (pasteTarget is ComponentContainerComCtrlTreeViewItem)
                {
                    container = (pasteTarget as ComponentContainerComCtrlTreeViewItem).Container;
                    host = (pasteTarget as ComponentContainerComCtrlTreeViewItem).Container.Host;
                }
                if (pasteTarget is ActorComCtrlTreeViewItem)
                {
                    container = (pasteTarget as ActorComCtrlTreeViewItem).Actor;
                    host = (pasteTarget as ActorComCtrlTreeViewItem).Actor;
                }
                for (int i = 0; i < CopyClipsBoard.Count; ++i)
                {
                    var com = await CopyClipsBoard[i].Clone(EngineNS.CEngine.Instance.RenderContext, host, container);
                    coms.Add(com);
                }
                Action<Object> redo = (obj) =>
                {
                    for (int i = 0; i < coms.Count; ++i)
                    {
                        pasteTarget.AddComponent(coms[i]);
                        pasteTarget.IsExpanded = true;
                    }
                };
                redo.Invoke(null);
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                {
                    for (int i = 0; i < coms.Count; ++i)
                    {
                        var pair = coms[i];
                        pair.Parent.RemoveComponent(pair);
                    }

                }, $"复制粘贴{coms.Count}个对象");
            }
        }
        public async void DuplicateCommand()
        {
            var coms = new List<ComsUndoRedoItem>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                var item = SelectedItemViews[i];
                GActor host = null;
                IComponentContainer container = null;
                if (item is ComponentContainerComCtrlTreeViewItem)
                {
                    container = (item as ComponentContainerComCtrlTreeViewItem).Container;
                    host = (item as ComponentContainerComCtrlTreeViewItem).Container.Host;
                }
                if (item is ActorComCtrlTreeViewItem)
                {
                    container = (item as ActorComCtrlTreeViewItem).Actor;
                    host = (item as ActorComCtrlTreeViewItem).Actor;
                }
                if (SelectedItemViews[i].Parent != null)
                {
                    var actor = await SelectedItemViews[i].Clone(EngineNS.CEngine.Instance.RenderContext, host, container);
                    coms.Add(new ComsUndoRedoItem(SelectedItemViews[i].Parent, actor));
                }
            }
            Action<Object> redo = (obj) =>
            {
                for (int i = 0; i < coms.Count; ++i)
                {
                    coms[i].LastParent.AddComponent(coms[i].Item);
                }
            };
            redo.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
            {
                for (int i = 0; i < coms.Count; ++i)
                {
                    var pair = coms[i];
                    pair.LastParent.RemoveComponent(pair.Item);
                }

            }, $"克隆{coms.Count}个对象");
        }
        public void DeleteCommand()
        {
            List<ComsUndoRedoItem> addRemoveItems = new List<ComsUndoRedoItem>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i].Parent == null)
                    continue;
                addRemoveItems.Add(new ComsUndoRedoItem(SelectedItemViews[i].Parent, SelectedItemViews[i]));
            }
            ClearSelectedItemViews();
            var redo = new Action<object>((obj) =>
            {
                for (int i = 0; i < addRemoveItems.Count; ++i)
                {
                    var pair = addRemoveItems[i];
                    pair.LastParent.RemoveComponent(pair.Item);
                }
            });
            redo.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
            {
                for (int i = 0; i < addRemoveItems.Count; ++i)
                {
                    var pair = addRemoveItems[i];
                    pair.LastParent.AddComponent(pair.Item);
                }

            }, $"删除{addRemoveItems.Count}个对象");
        }
        #endregion command

        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            var comItem = item.Header as ComCtrlTreeViewItem;
            if (comItem == null)
                return;
            if(comItem.NeedScrollToWhenLoaded)
            {
                item.BringIntoView();
                comItem.NeedScrollToWhenLoaded = false;
            }

        }
    }
}
