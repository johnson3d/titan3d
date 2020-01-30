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

namespace Macross
{
    /// <summary>
    /// Interaction logic for MacrossPanel.xaml
    /// </summary>
    public partial class MacrossPanel : MacrossPanelBase
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

        public MacrossPanel()
        {
            InitializeComponent();
            mAddNewButtonControl = Button_AddNew;
            InitializeCategorys(CategoryPanels);

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
    }
}
