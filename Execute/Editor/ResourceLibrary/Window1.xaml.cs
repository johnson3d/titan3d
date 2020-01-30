using System.Collections;
using System.ComponentModel;
using System.Windows;

namespace ResourceLibrary
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            //TestCode();
        }

        //class TreeListTestItem : TreeItemViewModel, INotifyPropertyChanged
        //{
        //    #region INotifyPropertyChangedMembers
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    protected void OnPropertyChanged(string propertyName)
        //    {
        //        EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        //    }
        //    #endregion
        //}

        //ObservableCollectionAdv<ITreeModel> mSource = new ObservableCollectionAdv<ITreeModel>();
        //void TestCode()
        //{
        //    mSource.Clear();
        //    for (int i=0; i<5; i++)
        //    {
        //        var treeList = new TreeListTestItem()
        //        {
        //            Name = "Test" + i,
        //        };
        //        for(int j=0; j<3; j++)
        //        {
        //            var cdItem = new TreeListTestItem()
        //            {
        //                Name = "Child" + j,
        //                Parent = treeList,
        //            };
        //            treeList.Children.Add(cdItem);
        //        }
        //        mSource.Add(treeList);
        //    }
        //    TreeList_Test.TreeListItemsSource = mSource;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TestCode();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            //var item = mSource[0] as TreeListTestItem;
            //item.Children.Add(new TreeListTestItem()
            //{
            //    Name = "TTT" + item.Children.Count,
            //});
        }
    }
}
