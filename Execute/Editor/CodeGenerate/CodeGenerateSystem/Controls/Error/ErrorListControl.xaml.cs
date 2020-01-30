using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// ErrorListControl.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorListControl : UserControl
    {
        public delegate void Delegate_ListSelectionChanged(Base.BaseNodeControl node);
        public event Delegate_ListSelectionChanged OnListSelectionChanged;

        public Base.NodesContainer NodesContainer
        {
            get;
            set;
        }

        public ObservableCollection<ErrorListItem> ErrorListItems
        {
            get;
            set;
        }

        public ErrorListControl()
        {
            InitializeComponent();

            MainGrid.DataContext = this;
            ErrorListItems = new ObservableCollection<ErrorListItem>();
        }

        public void Clear()
        {
            ErrorListItems.Clear();
        }

        public void AddErrorMsg(Base.BaseNodeControl node, string errorMsg)
        {
            foreach (var item in ErrorListItems)
            {
                if (item.Node == node)
                {
                    item.Update();
                    return;
                }
            }

            ErrorListItem listItem = new ErrorListItem(node, errorMsg);
            listItem.MouseLeftButtonDown += new MouseButtonEventHandler(listItem_MouseLeftButtonDown);
            listItem.Update();
            ErrorListItems.Add(listItem);
        }

        void listItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ErrorListItem item = sender as ErrorListItem;
            if (item != null)
            {
                OnListSelectionChanged?.Invoke(item.Node);
                NodesContainer.FocusNode(item.Node);
            }
        }
    }
}
