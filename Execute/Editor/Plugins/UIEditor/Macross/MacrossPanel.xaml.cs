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

namespace UIEditor.UIMacross
{
    public interface IMacrossPanel
    {
        Macross.IMacrossOperationContainer HostControl
        {
            get;
            set;
        }
        Macross.CategoryItem[] GetVariables();
        Dictionary<string, Macross.Category> CategoryDic { get;}
        List<string> CreatedOverrideMethods { get; }
    }
    /// <summary>
    /// Interaction logic for MacrossPanel.xaml
    /// </summary>
    public partial class MacrossPanel : Macross.MacrossPanelBase
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

        public readonly static string UIElementVariableCategoryName = "UIElementVariable";
        public readonly static string UIEventFuncCategoryName = "UIEventFunction";
        public readonly static string UIBindFuncCategoryName = "UIBindFunction";
        public readonly static string UICustomEventCategoryName = "UICustomEvent";

        public MacrossPanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            InitializeCategorys(CategoryPanels, Macross.MacrossPanelBase.GraphCategoryName,
                                                Macross.MacrossPanelBase.FunctionCategoryName,
                                                Macross.MacrossPanelBase.VariableCategoryName,
                                                Macross.MacrossPanelBase.PropertyCategoryName,
                                                Macross.MacrossPanelBase.AttributeCategoryName,
                                                UIElementVariableCategoryName,
                                                UIEventFuncCategoryName,
                                                UIBindFuncCategoryName,
                                                UICustomEventCategoryName);

            Button_AddNew.SubmenuOpened += Button_AddNew_SubmenuOpened;
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
        private void MenuItem_Property_Click(object sender, RoutedEventArgs e)
        {
            var noUse = AddProperty(PropertyCategoryName);
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
        private void MenuItem_CustomEvent_Click(object sender, RoutedEventArgs e)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(UICustomEventCategoryName, out category))
                return;

            int i = 0;
            string newName = "NewEvent_";
            bool repetition = false;
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
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;// Macross.CategoryItem.enCategoryItemType.CustomFunction;
            item.Name = newName + i;
            item.CanDrag = true;
            item.InitTypeStr = "UI_UIElement_CustomEvent";
            var data = new UIEditor.UIMacross.UIElementCustomEventCategoryItemInitData();
            data.Reset();
            data.EventName = "UICustomEvent_" + EngineNS.Editor.Assist.GetValuedGUIDString(item.Id);
            data.DisplayName = newName + i;
            item.Initialize(HostControl, data);
            category.Items.Add(item);
        }
    }
}
