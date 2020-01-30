using System;
using System.Collections.Generic;
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

using Macross;

namespace ParticleEditor
{

    public partial class ParticlePanel : Macross.MacrossPanelBase
    {
        public readonly static string ParticleCategoryName = "Particle";//ParticleShape

        public enum ParticleTemplateType : int
        {
            ParticleSystem_Template = 1,
            ParticleShape_Template,
        }

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                //ShowItemWithFilter(ChildList, mFilterString);
                OnPropertyChanged("FilterString");
            }
        }

        public ParticleMacrossLinkControl HostParticleControl;
        public ParticlePanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            InitializeCategorys(CategoryPanels);
            AddParticleCategory(CategoryPanels);
            Button_AddNew.SubmenuOpened += Button_AddNew_SubmenuOpened;

            Macross.CategoryItem.RegistInitAction("Particle_ParticleEmitter", new Action<CategoryItem, IMacrossOperationContainer, CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                Initialize(item, ctrl);
            }));
        }

        protected void AddParticleCategory(StackPanel categoryPanel)
        {
            var category = new Category(this);
            category.CategoryName = ParticleCategoryName;
            mCategoryDic.Add(ParticleCategoryName, category);
            categoryPanel.Children.Add(category);

            category.OnSelectedItemChanged = (categoryName) =>
            {
                if (ParticleCategoryName == categoryName)
                    return;

                Category ctg;
                if (mCategoryDic.TryGetValue(ParticleCategoryName, out ctg))
                {
                    ctg.UnSelectAllItems();
                }
            };
        }

        private void Button_AddNew_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var noUse = InitializeOverrideAddNewMenu(MenuItem_OverrideFunction);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_Variable_Click(object sender, RoutedEventArgs e)
        {
            AddVariable(VariableCategoryName);
        }

        private void MenuItem_Function_Click(object sender, RoutedEventArgs e)
        {
            var noUse = AddFunction(FunctionCategoryName);
        }

        private void MenuItem_Graph_Click(object sender, RoutedEventArgs e)
        {
            AddGraph(GraphCategoryName);
        }

        private void MenuItem_Attribute_Click(object sender, RoutedEventArgs e)
        {
            AddAttribute(AttributeCategoryName);
        }
        //private void MenuItem_ParticleEmitter_Click(object sender, RoutedEventArgs e)
        //{
        //    Category category;
        //    if (!mCategoryDic.TryGetValue(ParticleCategoryName, out category))
        //        return;

        //    var window = new CreateParticleShape();
        //    window.ShowDialog();

        //    if (window.CurrentParticleShape == null)
        //        return;

        //    if (string.IsNullOrEmpty(window.UIName.Text))
        //        return;

        //    string result;
        //    if (CheckNameFormat(window.UIName.Text, out result) == false)
        //    {
        //        EditorCommon.MessageBox.Show(result);
        //        return;
        //    }

        //    int i = 0;
        //    string newName = "ParticleSystem_";
        //    bool repetition = true;
        //    do
        //    {
        //        repetition = false;
        //        foreach (var cItem in category.Items)
        //        {
        //            if (newName + i == cItem.Name)
        //            {
        //                repetition = true;
        //                i++;
        //                break;
        //            }
        //        }
        //    } while (repetition);
        //    var item = new CategoryItem(null, category);
        //    item.CategoryItemType = CategoryItem.enCategoryItemType.Unknow;// CategoryItem.enCategoryItemType.ParticleEmitter;
        //    item.Name = newName + i;
        //    item.InitTypeStr = "Particle_ParticleEmitter";
        //    var data = new Macross.CategoryItem.InitializeData();
        //    item.Initialize(HostControl, data);
        //    category.Items.Add(item);
        //    HostControl.CreateNodesContainer(item);
        //    var noUse = CreateLinkedCategoryTemplate(item, newName + i, window.UIName.Text, window.CurrentParticleShape.ParticleShapeType);
        //    //CreateParticleMethodCategory(item, "CreateParticleSystem", 0, 0);
        //    //CreateParticleMethodCategory(item, "DoParticleCompose", 0, 600);
        //    //CreateParticleShapeObjectNode(item, typeof(EngineNS.Bricks.Particle.CGfxParticleSystem), item.Name, 0, 300);

        //    //var subitem = new CategoryItem(item, item.ParentCategory);
        //    //subitem.Initialize(HostControl);
        //    //subitem.CategoryItemType = CategoryItem.enCategoryItemType.ParticleEmitter;
        //    //HostControl.CreateNodesContainer(subitem);
        //    //item.Children.Add(subitem);
        //    //subitem.Name = window.UIName.Text;
        //    //CreateParticleMethodCategory(subitem, "DoParticleSubStateBorn", 0, 0);
        //    //CreateParticleMethodCategory(subitem, "DoParticleSubStateTick", 0, 300);
        //    //CreateParticleMethodCategory(subitem, "DoParticleStateBorn", 0, 600);
        //    //CreateParticleMethodCategory(subitem, "DoParticleStateTick", 0, 900);
        //    //CreateParticleMethodCategory(subitem, "CreateParticleShape", 0, 1500);

        //    //CreateParticleShapeObjectNode(subitem, window.CurrentParticleShape.ParticleShapeType, subitem.Name, 0, 1700);
        //    //CreateLinkedCategoryTemplate(item, window.CurrentParticleShape.ParticleShapeType, item.Name);
        //}

        private void MenuItem_ParticleEmitter_Click(object sender, RoutedEventArgs e)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(ParticleCategoryName, out category))
                return;

            //var window = new CreateParticleShape();
            //window.ShowDialog();

            //if (window.CurrentParticleShape == null)
            //    return;

            //if (string.IsNullOrEmpty(window.UIName.Text))
            //    return;

            //string result;
            //if (CheckNameFormat(window.UIName.Text, out result) == false)
            //{
            //    EditorCommon.MessageBox.Show(result);
            //    return;
            //}

            int i = 0;
            string newName = "ParticleSystem_";
            bool repetition = true;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new CategoryItem(null, category);

            item.CheckVisibility = Visibility.Visible;
            item.OnIsShowChanged -= HostParticleControl.OnIsShowChanged;
            item.OnIsShowChanged += HostParticleControl.OnIsShowChanged;

            item.CategoryItemType = CategoryItem.enCategoryItemType.ParticleEmitter;// CategoryItem.enCategoryItemType.ParticleEmitter;
            item.Name = newName + i;
            item.InitTypeStr = "Particle_ParticleEmitter";
            Initialize(item, HostControl);
            category.Items.Add(item);
            HostControl.CreateNodesContainer(item);

            var test = CreateLinkedNode(item);

        }

        public async Task CreateLinkedNode(CategoryItem MainGridItem)
        {
            var nodesContainer = await HostControl.GetNodesContainer(MainGridItem, false);
            if (nodesContainer == null)
                throw new InvalidOperationException("nodesContainer is null");

            var nodeType = typeof(CodeDomNode.Particle.ParticleSystemControl);

            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, new CodeDomNode.Particle.ParticleSystemControlConstructionParams(), 50, 50);
            node.IsDeleteable = false;
            node.HostNodesContainer = nodesContainer.NodesControl;
            MainGridItem.AddInstanceNode(nodesContainer.NodesControl.GUID, node);

        }
        public async Task OnChangeParticleCategoryName(CategoryItem item, string newname, string oldname)
        {
            if (item.HostLinkControl != null)
            {
                var nodesContainer = await item.HostLinkControl.GetNodesContainer(item, true);
                var nodes = nodesContainer.NodesControl.CtrlNodeList;
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodesContainer.OnChangeParticleCategoryName(nodes[i], oldname, newname, false);
                }
            }

        }

        bool CheckNameFormat(string name, out string result)
        {
            if (name.IndexOf(" ") != -1)
            {
                result = "名字中不能包含空格！";
                return false;
            }


            string pattern = "[\u4e00-\u9fbb]";
            if (System.Text.RegularExpressions.Regex.IsMatch(name, pattern))
            {
                result = "名字中不能包含中文！";
                return false;
            }

            int num;
            if (int.TryParse(name.Substring(0, 1), out num))
            {
                result = "名字中首字母不能是数字！";
                return false;
            }
            result = "";
            return true;
        }


        public void Initialize(CategoryItem item, IMacrossOperationContainer ctrl)
        {
            item.Icon = this.TryFindResource("Icon_Graph") as ImageSource;

            if (item.CategoryItemContextMenu == null)
            {
                item.CategoryItemContextMenu = new ContextMenu();
                item.CategoryItemContextMenu.Style = ctrl.NodesCtrlAssist.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style;
            }
            item.OnDoubleClick += (categoryItem) =>
            {
                HostParticleControl.ClearCategorys();
                var noUse = ctrl.ShowNodesContainer(categoryItem);
            };

            if (item.PropertyShowItem == null)
                item.PropertyShowItem = new GraphCategoryItemPropertys();
            var varItemPro = item.PropertyShowItem as GraphCategoryItemPropertys;
            varItemPro.HostCategoryItem = item;
            BindingOperations.SetBinding(varItemPro, GraphCategoryItemPropertys.GraphNameProperty, new Binding("Name") { Source = item, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(varItemPro, GraphCategoryItemPropertys.TooltipProperty, new Binding("ToolTips") { Source = item, Mode = BindingMode.TwoWay });
            var menuItem = new MenuItem()
            {
                Name = "GraphOpenGraph",
                Header = "打开",
                Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                HostParticleControl.ClearCategorys();
                var noUse = ctrl.ShowNodesContainer(item);
            };
            item.CategoryItemContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem()
            {
                Name = "GraphDelete",
                Header = "删除",
                Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                if (EditorCommon.MessageBox.Show($"即将删除{item.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                {
                    item.ParentCategory.Items.Remove(item);
                    ctrl.RemoveNodesContainer(item);
                    var fileName = ctrl.GetGraphFileName(item.Name);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                    if (item.Parent != null)
                    {
                        item.Parent.Children.Remove(item);
                    }
                }
            };
            item.CategoryItemContextMenu.Items.Add(menuItem);


            item.OnNameChangedEvent += (categoryItem, newValue, oldVaue) =>
            {
                var noUse = OnChangeParticleCategoryName(categoryItem, newValue, oldVaue);
            };
        }

    }
}
