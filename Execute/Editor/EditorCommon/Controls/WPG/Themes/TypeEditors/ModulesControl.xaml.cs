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

namespace WPG.Themes.TypeEditors
{
    internal class ModuleTreeViewItem : DependencyObject
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ModuleTreeViewItem));
        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(ModuleTreeViewItem));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(ModuleTreeViewItem));
        public ImageSource ImageIcon
        {
            get { return (ImageSource)GetValue(ImageIconProperty); }
            set { SetValue(ImageIconProperty, value); }
        }
        public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register("ImageIcon", typeof(ImageSource), typeof(ModuleTreeViewItem));
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ModuleTreeViewItem));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ModuleTreeViewItem), new PropertyMetadata(true));

        public ModuleTreeViewItem Parent
        {
            get;
            internal set;
        } = null;

        public ObservableCollection<ModuleTreeViewItem> ChildList
        {
            get;
            set;
        } = new ObservableCollection<ModuleTreeViewItem>();

        public EngineNS.GamePlay.SceneGraph.GSceneGraph mHostScene;
        EngineNS.GamePlay.SceneGraph.GSceneModule mHostModule;
        public EngineNS.GamePlay.SceneGraph.GSceneModule Module => mHostModule;

        public ModuleTreeViewItem(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            mHostScene = scene;
            if(scene != null)
            {
                BindingOperations.SetBinding(this, NameProperty, new Binding("Name") { Source = scene });
                TypeName = $"({scene.GetType().FullName})";
            }
        }
        public ModuleTreeViewItem(EngineNS.GamePlay.SceneGraph.GSceneModule module)
        {
            mHostModule = module;
            if(module != null)
            {
                BindingOperations.SetBinding(this, NameProperty, new Binding("Name") { Source = module });
                TypeName = $"({module.GetType().FullName})";
            }
        }

        public void ShowInPropertyGrid(PropertyGrid proGrid)
        {
            if (proGrid == null)
                return;
            proGrid.Instance = mHostModule;
        }

        //public async Task AddModule(EngineNS.GamePlay.SceneGraph.GSceneModule module)
        //{
        //
        //}
    }

    /// <summary>
    /// ModulesControl.xaml 的交互逻辑
    /// </summary>
    public partial class ModulesControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                var noUse = InitializeCreateMenu();
                OnPropertyChanged("FilterString");
            }
        }

        public ModulesControl()
        {
            InitializeComponent();
            TreeView_Modules.ItemsSource = mModules;
        }

        private void AddModule_Click(object sender, RoutedEventArgs e)
        {
            var noUse = InitializeCreateMenu();
        }
        private void TreeView_Modules_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            RefreshComponent();
        }

        public PropertyGrid LinkedPropertyGrid;
        private void RefreshComponent()
        {
            if (LinkedPropertyGrid == null)
                return;

            var item = TreeView_Modules.SelectedItem as ModuleTreeViewItem;
            if (item == null)
                return;

            item.ShowInPropertyGrid(LinkedPropertyGrid);
        }
        private void TreeViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        public void SetSceneGraph(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            InitModulesTreeView(scene);
        }

        EngineNS.GamePlay.SceneGraph.GSceneGraph mCurrentScene;
        ObservableCollection<ModuleTreeViewItem> mModules = new ObservableCollection<ModuleTreeViewItem>();
        void InitModulesTreeView(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            mModules.Clear();

            mCurrentScene = scene;
            if (mCurrentScene == null)
                return;
            var item = new ModuleTreeViewItem(scene);
            mModules.Add(item);

            foreach(var module in scene.Modules.Values)
            {
                var moduleItem = new ModuleTreeViewItem(module);
                item.ChildList.Add(moduleItem);
            }
        }
        public async Task InitializeCreateMenu()
        {
            if (mCurrentScene == null)
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
                var baseType = typeof(EngineNS.GamePlay.SceneGraph.GSceneModule);
                foreach (var type in types)
                {
                    if (mCurrentScene.Modules.ContainsKey(type))
                        continue;
                    if (type == baseType || type.IsSubclassOf(baseType))
                    {
                        var atts = type.GetCustomAttributes(typeof(EngineNS.GamePlay.SceneGraph.ModuleInitInfoAttribute), false);
                        if (atts.Length == 0)
                            continue;

                        var att = atts[0] as EngineNS.GamePlay.SceneGraph.ModuleInitInfoAttribute;
                        var menuNames = new List<string>(5);
                        if(att.ModuleGroup != null)
                        {
                            menuNames.AddRange(att.ModuleGroup);
                        }
                        else
                        {
                            menuNames.Add("Common");
                            menuNames.Add(type.FullName);
                        }
                        await CreateMenu(itemsControl.Items, menuNames, type, att.Description);
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
        async Task CreateMenu(ItemCollection items, List<string> menuNames, Type type, string description)
        {
            if (menuNames.Count == 0)
                return;

            var curMenuName = menuNames[0];
            menuNames.RemoveAt(0);
            if(menuNames.Count == 0)
            {
                var item = CreateMenuItem(items, curMenuName, type, description);
                item.Click += async (object sender, RoutedEventArgs e) =>
                {
                    var atts = type.GetCustomAttributes(typeof(EngineNS.GamePlay.SceneGraph.ModuleInitInfoAttribute), false);
                    if(atts.Length > 0)
                    {
                        var module = EngineNS.Rtti.RttiHelper.CreateInstance(type) as EngineNS.GamePlay.SceneGraph.GSceneModule;
                        var att = atts[0] as EngineNS.GamePlay.SceneGraph.ModuleInitInfoAttribute;
                        var initer = EngineNS.Rtti.RttiHelper.CreateInstance(att.IniterType) as EngineNS.GamePlay.SceneGraph.GSceneModule.GSceneModuleInitializer;
                        initer.Name = type.Name;
                        await module.SetInitializer(EngineNS.CEngine.Instance.RenderContext, mCurrentScene, initer);
                        mCurrentScene.AddModule(module);
                        InitModulesTreeView(mCurrentScene);
                    }
                };
                return;
            }
            bool find = false;
            foreach(var item in items)
            {
                var menuItem = item as MenuItem;
                if (menuItem == null)
                    continue;

                if(string.Equals(menuItem.Header, curMenuName))
                {
                    await CreateMenu(menuItem.Items, menuNames, type, description);
                    find = true;
                }
            }
            if(!find)
            {
                var item = CreateMenuItem(items, curMenuName, type, description);
                await CreateMenu(item.Items, menuNames, type, description);
            }
        }
        MenuItem CreateMenuItem(ItemCollection items, string menuName, Type type, string description)
        {
            var newItem = new MenuItem()
            {
                Name = "MenuItem_CreateMenuItem_" + menuName,
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                Header = menuName,
                ToolTip = description,
            };
            items.Add(newItem);

            return newItem;
        }
    }
}
