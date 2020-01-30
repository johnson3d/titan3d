using CodeDomNode.Animation;
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

namespace McLogicAnimationEditor
{
    /// <summary>
    /// Interaction logic for MacrossPanel.xaml
    /// </summary>
    public partial class MCLAMacrossPanel : Macross.MacrossPanelBase
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
        public readonly static string LogicAnimationPostProcessCategoryName = "PostProcess";
        public MCLAMacrossPanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            var names = new string[] { GraphCategoryName, LogicAnimationGraphNodeCategoryName, LogicAnimationPostProcessCategoryName, VariableCategoryName, PropertyCategoryName, FunctionCategoryName };
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
            Macross.CategoryItem.RegistInitAction("LA_AnimLayer", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                {
                    item.PropertyShowItem = new LAAnimLayerCategoryItemPropertys();

                    var varItemPro = item.PropertyShowItem as LAAnimLayerCategoryItemPropertys;
                    var varData = data as LAAnimLayerCategoryItemInitData;
                    varItemPro.Name = varData.Name;
                    varItemPro.ToolTips = varData.ToolTips;
                    varItemPro.LayerType = varData.LayerType;
                    varItemPro.CategoryItem = item;
                    BindingOperations.SetBinding(item, CategoryItem.NameProperty, new Binding("Name") { Source = varItemPro, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(item, CategoryItem.ToolTipsProperty, new Binding("ToolTips") { Source = varItemPro, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(varItemPro, LAAnimLayerCategoryItemPropertys.LayerTypeProperty, new Binding("LayerType") { Source = varData, Mode = BindingMode.TwoWay });

                }
                //BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = initData.UIElement.Initializer });
                var menuItem = new MenuItem()
                {
                    Name = "LA_AnimLayerItem",
                    Header = "添加LinkNode",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    var animGrapItem = new CategoryItem(item, item.ParentCategory);
                    animGrapItem.CategoryItemType = CategoryItem.enCategoryItemType.LogicAnimGraph;
                    animGrapItem.Name = GetValidName("LinkNode", item.ParentCategory);
                    animGrapItem.InitTypeStr = "LA_AnimGraph";
                    var tempData = new LAAnimGraphCategoryItemInitData();
                    tempData.Name = animGrapItem.Name;
                    tempData.ToolTips = animGrapItem.ToolTips;
                    tempData.Reset();
                    animGrapItem.Initialize(ctrl, tempData);
                    item.Children.Add(animGrapItem); Action create = async () =>
                    {
                        var nodeContainer = await ctrl.GetNodesContainer(animGrapItem, true);
                        var csParam = new LAGraphNodeControlConstructionParams();
                        csParam.CSType = ctrl.CSType;
                        csParam.LAGNodeName = animGrapItem.Name;
                        csParam.LinkedCategoryItemID = animGrapItem.Id;
                        csParam.IsSelfGraphNode = true;
                        var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LAGraphNodeControl), csParam, 0, 200);
                        ins.HostNodesContainer = nodeContainer.NodesControl;
                    };
                    create.Invoke();
                };
                item.CategoryItemContextMenu.Items.Add(menuItem);
                {
                    menuItem = new MenuItem()
                    {
                        Name = "LA_AnimLayerItem_Delete",
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

            Macross.CategoryItem.RegistInitAction("LA_AnimGraph", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                {
                    item.PropertyShowItem = new LAAnimGraphCategoryItemPropertys();
                    var varItemPro = item.PropertyShowItem as LAAnimGraphCategoryItemPropertys;
                    var varData = data as LAAnimGraphCategoryItemInitData;
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
                    varItemPro.LinkControl = ctrl as MCLAMacrossLinkControl;
                    varItemPro.CategoryItem = item;
                    BindingOperations.SetBinding(item, CategoryItem.NameProperty, new Binding("Name") { Source = varItemPro, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(item, CategoryItem.ToolTipsProperty, new Binding("ToolTips") { Source = varItemPro, Mode = BindingMode.TwoWay });
                }
                var initData = data as LAAnimGraphCategoryItemInitData;

                //BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = initData.UIElement.Initializer });
                item.Icon = TryFindResource("Icon_Graph") as ImageSource;
                item.OnDoubleClick += (categoryItem) =>
                {
                    var noUse = ctrl.ShowNodesContainer(categoryItem);
                };

                var menuItem = new MenuItem()
                {
                    Name = "LA_AnimGraphItem",
                    Header = "添加LinkNode",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };

                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    var animGrapItem = new CategoryItem(item, item.ParentCategory);
                    animGrapItem.CategoryItemType = CategoryItem.enCategoryItemType.LogicAnimGraph;
                    animGrapItem.Name = GetValidName("LinkNode", item.ParentCategory);
                    animGrapItem.InitTypeStr = "LA_AnimGraph";
                    var tempData = new LAAnimGraphCategoryItemInitData();
                    tempData.Name = animGrapItem.Name;
                    tempData.ToolTips = animGrapItem.ToolTips;
                    tempData.Reset();
                    animGrapItem.Initialize(ctrl, tempData);
                    item.Children.Add(animGrapItem);
                    Action create = async () =>
                     {
                         var nodeContainer = await ctrl.GetNodesContainer(animGrapItem, true);
                         var csParam = new LAGraphNodeControlConstructionParams();
                         csParam.CSType = ctrl.CSType;
                         csParam.LAGNodeName = animGrapItem.Name;
                         csParam.LinkedCategoryItemID = animGrapItem.Id;
                         csParam.IsSelfGraphNode = true;
                         var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LAGraphNodeControl), csParam, 0, 200);
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

            Macross.CategoryItem.RegistInitAction("LA_PostProcess", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                {
                    item.PropertyShowItem = new LAAnimPostProcessCategoryItemPropertys();
                    var varItemPro = item.PropertyShowItem as LAAnimPostProcessCategoryItemPropertys;
                    var varData = new LAAnimPostProcessCategoryItemInitData();
                    varItemPro.Name = varData.Name;
                    varItemPro.ToolTips = varData.ToolTips;
                    varItemPro.CategoryItem = item;
                    BindingOperations.SetBinding(item, CategoryItem.NameProperty, new Binding("Name") { Source = varItemPro, Mode = BindingMode.TwoWay });
                    BindingOperations.SetBinding(item, CategoryItem.ToolTipsProperty, new Binding("ToolTips") { Source = varItemPro, Mode = BindingMode.TwoWay });
                var initData = data as LAAnimPostProcessCategoryItemInitData;
                }

                //BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = initData.UIElement.Initializer });
                item.Icon = TryFindResource("Icon_Graph") as ImageSource;
                Macross.CategoryItem.Delegate_OnDoubleClick openAction = async (categoryItem) =>
                {
                    var noUse = await ctrl.ShowNodesContainer(categoryItem);
                };
                item.OnDoubleClick += openAction;
                var menuItem = new MenuItem()
                {
                    Name = "AnimPostProcessOpenGraph",
                    Header = "打开",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    openAction.Invoke(item);
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
            AddFunction(FunctionCategoryName);
        }
        private void MenuItem_Graph_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_AnimGraphLayer_Click(object sender, RoutedEventArgs e)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(LogicAnimationGraphNodeCategoryName, out category))
                return;

            var validName = GetValidName("BaseLayer", category);
            var item = new CategoryItem(null, category);
            item.CategoryItemType = CategoryItem.enCategoryItemType.LogicAnimGraphLayer;
            item.Name = validName;
            item.InitTypeStr = "LA_AnimLayer";
            var data = new LAAnimLayerCategoryItemInitData();
            data.Name = validName;
            data.ToolTips = item.ToolTips;
            data.LayerType = EngineNS.Bricks.Animation.AnimStateMachine.AnimLayerType.Default;
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
            //Macross.CategoryItem.RegistInitAction("UI_UIElementVariable", new
        }

        private void MenuItem_PostProcess_Click(object sender, RoutedEventArgs e)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(LogicAnimationPostProcessCategoryName, out category))
                return;

            var validName = GetValidName("LogicAnimPostProcess", category);
            var item = new CategoryItem(null, category);
            item.CategoryItemType = CategoryItem.enCategoryItemType.LogicAnimPostProcess;
            item.Name = validName;
            item.InitTypeStr = "LA_PostProcess";
            var data = new LAAnimPostProcessCategoryItemInitData();
            data.Name = validName;
            data.ToolTips = item.ToolTips;
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
            Action create = async () =>
            {
                var nodeContainer = await HostControl.GetNodesContainer(item, true);
                {
                    var csParam = new LAStartPoseControlConstructionParams()
                    {
                        CSType = HostControl.CSType,
                        NodeName = "InPose",
                        HostNodesContainer = nodeContainer.NodesControl,
                        ConstructParam = "",
                    };
                    var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LAStartPoseControl), csParam, 50, 250);
                    ins.HostNodesContainer = nodeContainer.NodesControl;
                    ins.IsDeleteable = false;
                }
                {
                    var csParam = new LAFinalPoseControlConstructionParams()
                    {
                        CSType = HostControl.CSType,
                        NodeName = "OutPose",
                        HostNodesContainer = nodeContainer.NodesControl,
                        ConstructParam = "",
                    };
                    var ins = nodeContainer.NodesControl.AddNodeControl(typeof(LAFinalPoseControl), csParam, 550, 250);
                    ins.HostNodesContainer = nodeContainer.NodesControl;
                    ins.IsDeleteable = false;
                }
            };
            create.Invoke();
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
            AddProperty(PropertyCategoryName);
        }
    }
}
