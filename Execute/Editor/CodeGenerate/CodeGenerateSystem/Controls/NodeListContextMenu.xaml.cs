using System;
using System.Collections.Generic;
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

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// Interaction logic for NodeListContextMenu.xaml
    /// </summary>
    public partial class NodeListContextMenu : UserControl
    {
        public NodeListContextMenu()
        {
            InitializeComponent();
            NodesList.EnableDrag = false;
        }

        public NodeListControl GetNodesList()
        {
            return NodesList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            NodesList.FocusSearchBox();
        }

        public Action OnCopy;
        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            OnCopy?.Invoke();
        }
        public Action OnPaste;
        private void Button_Paste_Click(object sender, RoutedEventArgs e)
        {
            OnPaste?.Invoke();
        }
        public Action OnDelete;
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            OnDelete?.Invoke();
        }
    }
}
