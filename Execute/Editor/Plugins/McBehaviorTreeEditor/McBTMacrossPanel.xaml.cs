using CodeDomNode.AI;
using Macross;
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

namespace McBehaviorTreeEditor
{
    /// <summary>
    /// Interaction logic for MacrossPanel.xaml
    /// </summary>
    public partial class McBTMacrossPanel : Macross.MacrossPanelBase
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
        public static string BehaviorTreeCategoryName = "BehaviorTree";
        public McBTMacrossPanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            var names = new string[] { GraphCategoryName, VariableCategoryName, PropertyCategoryName, FunctionCategoryName , BehaviorTreeCategoryName };
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
            Macross.CategoryItem.RegistInitAction("BehaviorTreeGraph", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new BTGraphCategoryItemPropertys();
                var varItemPro = item.PropertyShowItem as BTGraphCategoryItemPropertys;
                BindingOperations.SetBinding(varItemPro, BTGraphCategoryItemPropertys.NameProperty, new Binding("Name") { Source = item, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(varItemPro, BTGraphCategoryItemPropertys.TooltipProperty, new Binding("ToolTips") { Source = item, Mode = BindingMode.TwoWay });
                varItemPro.CategoryItem = item;
                var initData = data as BTGraphCategoryItemInitData;
                item.Icon = TryFindResource("Icon_Graph") as ImageSource;
                item.OnDoubleClick += (categoryItem) =>
                {
                    var noUse = ctrl.ShowNodesContainer(categoryItem);
                };
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

        private void MenuItem_BehaviorTree_Click(object sender, RoutedEventArgs e)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(BehaviorTreeCategoryName, out category))
                return;

            var validName = GetValidName("BehaviorTree", category);
            var item = new CategoryItem(null, category);
            item.CategoryItemType = CategoryItem.enCategoryItemType.BehaviorTree;
            item.Name = validName;
            item.InitTypeStr = "BehaviorTreeGraph";
            var data = new BTGraphCategoryItemInitData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);

            Action create = async () =>
            {
                var nodeContainer = await HostControl.GetNodesContainer(item, true);
                {
                    {
                        var csParam = new BehaviorTree_RootControlConstructionParams()
                        {
                            CSType = HostControl.CSType,
                            NodeName = "Root",
                            HostNodesContainer = nodeContainer.NodesControl,
                            ConstructParam = "",
                        };
                        var ins = nodeContainer.NodesControl.AddNodeControl(typeof(BehaviorTree_RootControl), csParam, 500, 100);
                        ins.HostNodesContainer = nodeContainer.NodesControl;
                        ins.IsDeleteable = false;
                    }
                }
            };
            create.Invoke();
        }
    }
}
