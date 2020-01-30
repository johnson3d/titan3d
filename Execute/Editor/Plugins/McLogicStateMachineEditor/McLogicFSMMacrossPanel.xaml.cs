using CodeDomNode.Animation;
using Macross;
using McLogicStateMachineEditor.Controls;
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

namespace McLogicStateMachineEditor
{
    /// <summary>
    /// Interaction logic for MacrossPanel.xaml
    /// </summary>
    public partial class McLogicFSMMacrossPanel : Macross.MacrossPanelBase
    {
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
        public readonly static string LogicStateMachineCategoryName = "LogicStateMachineNode";
        public McLogicFSMMacrossPanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            var names = new string[] { GraphCategoryName, VariableCategoryName, PropertyCategoryName, FunctionCategoryName , LogicStateMachineCategoryName };
            InitializeCategorys(CategoryPanels, names);
            InitializeMenuItems();
            Button_AddNew.SubmenuOpened += Button_AddNew_SubmenuOpened;
        }

        private void Button_AddNew_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var noUse = InitializeOverrideAddNewMenu(MenuItem_OverrideFunction);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void InitializeMenuItems()
        {
            Macross.CategoryItem.RegistInitAction("LFSM_Graph", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                {
                    item.PropertyShowItem = new LFSMGraphCategoryItemPropertys();
                    var varItemPro = item.PropertyShowItem as LFSMGraphCategoryItemPropertys;
                    var varData = data as LFSMGraphCategoryItemInitData;
                    if (varData == null)
                    {
                        varItemPro.Name = item.Name;
                        varItemPro.ToolTips = item.ToolTips;
                    }
                    else
                    {
                        varItemPro.Name = varData.Name;
                        varItemPro.ToolTips = varData.ToolTips;
                    }
                    varItemPro.LinkControl = ctrl as McLogicFSMLinkControl;
                    varItemPro.CategoryItem = item;
                    BindingOperations.SetBinding(item, CategoryItem.NameProperty, new Binding("Name") { Source = varItemPro, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(item, CategoryItem.ToolTipsProperty, new Binding("ToolTips") { Source = varItemPro, Mode = BindingMode.TwoWay });
                }
                var initData = data as LFSMGraphCategoryItemInitData;

                //BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = initData.UIElement.Initializer });
                item.Icon = TryFindResource("Icon_Graph") as ImageSource;
                item.OnDoubleClick += (categoryItem) =>
                {
                    var fsmCtrl = ctrl as McLogicFSMLinkControl;
                    var noUse = fsmCtrl.ShowFSMNodesContainer(categoryItem);
                };

                var menuItem = new MenuItem()
                {
                    Name = "LFSM_GraphItem",
                    Header = "添加LinkNode",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };

                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    var animGrapItem = new CategoryItem(item, item.ParentCategory);
                    animGrapItem.Name = GetValidName("LinkNode", item.ParentCategory);
                    animGrapItem.InitTypeStr = "LFSM_Graph";
                    var tempData = new LFSMGraphCategoryItemInitData();
                    tempData.Name = animGrapItem.Name;
                    tempData.ToolTips = animGrapItem.ToolTips;
                    tempData.Reset();
                    animGrapItem.Initialize(ctrl, tempData);
                    item.Children.Add(animGrapItem);
                    Action create = async () =>
                     {
                         var nodeContainer = await ctrl.GetNodesContainer(animGrapItem, true);
                         var csParam = new LogicFSMGraphNodeControlConstructionParams();
                         csParam.CSType = ctrl.CSType;
                         csParam.LAGNodeName = animGrapItem.Name;
                         csParam.LinkedCategoryItemID = animGrapItem.Id;
                         csParam.IsSelfGraphNode = true;
                         var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LogicFSMGraphNodeControl), csParam, 0, 200);
                         ins.HostNodesContainer = nodeContainer.NodesControl;
                     };
                    create.Invoke();
                };
                item.CategoryItemContextMenu.Items.Add(menuItem);
                {
                    menuItem = new MenuItem()
                    {
                        Name = "LA_AnimGraphItem_Delete",
                        Header = "删除",
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                    };
                    ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                    menuItem.Click += (object sender, RoutedEventArgs e) =>
                    {
                        if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
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
                }
            }));
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

        }
        bool NameRepetition(string name, CategoryItem categoryItem)
        {
            if (categoryItem.Name == name)
                return true;
            foreach (var cItem in categoryItem.Children)
            {
                if (NameRepetition(name, cItem))
                    return true;
            }
            return false;
        }
        string GetValidName(string name, Category category)
        {
            string validName = name;
            bool repetition = false;
            foreach (var cItem in category.Items)
            {
                if (NameRepetition(validName, cItem))
                {
                    repetition = true;
                    break;
                }
            }
            if (repetition)
            {
                for (int i = 1; i < int.MaxValue; ++i)
                {
                    validName = name + i;
                    repetition = false;
                    foreach (var cItem in category.Items)
                    {
                        if (NameRepetition(validName, cItem))
                        {
                            repetition = true;
                            break;
                        }
                    }
                    if (!repetition)
                        break;
                }
            }
            return validName;
        }

        private void MenuItem_Property_Click(object sender, RoutedEventArgs e)
        {
            var noUse = AddProperty(PropertyCategoryName);
        }

        private void MenuItem_LogicGraph_Click(object sender, RoutedEventArgs e)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(LogicStateMachineCategoryName, out category))
                return;

            var validName = GetValidName("LinkNode", category);
            var item = new CategoryItem(null, category);
            item.Name = validName;
            item.InitTypeStr = "LFSM_Graph";
            var data = new LFSMGraphCategoryItemInitData();
            data.Name = validName;
            data.ToolTips = item.ToolTips;
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
            Action create = async () =>
            {
                var nodeContainer = await HostControl.GetNodesContainer(item, true);
                var csParam = new LogicFSMGraphNodeControlConstructionParams();
                csParam.CSType = HostControl.CSType;
                csParam.LAGNodeName = item.Name;
                csParam.LinkedCategoryItemID = item.Id;
                csParam.IsSelfGraphNode = true;
                var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LogicFSMGraphNodeControl), csParam, 0, 200);
                ins.HostNodesContainer = nodeContainer.NodesControl;
            };
            create.Invoke();
        }
    }
}
