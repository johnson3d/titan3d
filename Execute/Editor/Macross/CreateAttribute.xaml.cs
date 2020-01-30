using ResourceLibrary;
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
    /// CreateAttribute.xaml 的交互逻辑
    /// </summary>
    
    public partial class CreateAttribute : WindowBase, INotifyPropertyChanged
    {
        public enum Attribute : Byte
        {
            Class = 0,
            Property = 1,
        }

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region binddata
        public class BindData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string HighLightString
            {
                get { return (string)GetValue(HighLightStringProperty); }
                set { SetValue(HighLightStringProperty, value); }
            }

            //public string ClassName
            //{
            //    get { return (string)GetValue(ClassNameProperty); }
            //    set { SetValue(ClassNameProperty, value); }
            //}
            public string AttributeName
            {
                get;
                set;
            }

            public AttributeType Host
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(null));
            //public static readonly DependencyProperty ClassNameProperty = DependencyProperty.Register("ClassName", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(true));


            public BindData()
            {
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }
        #endregion

        public CreateAttribute(Attribute type)
        {
            InitializeComponent();

            if (type == Attribute.Class)
            {
                AllAttribute = GAttributeManager.ClassAttribute;
            }
            else
            {
                AllAttribute = GAttributeManager.PropertyAttribute;
            }
        }

        public static AttributeManager GAttributeManager = new AttributeManager();
        public Dictionary<string, AttributeType> AllAttribute;
        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        string mSearchFilter = "";
        public string SearchFilter
        {
            get
            {
                return mSearchFilter;
            }

            set
            {
                mSearchFilter = value;
                RefreshAttributes();

                OnPropertyChanged("SearchFilter");
            }
        }

        private void RefreshAttributes()
        {
            TreeViewItemsNodes.Clear();
            foreach (var i in AllAttribute)
            {
                if (mSearchFilter.Equals("") || i.Key.ToLower().IndexOf(mSearchFilter.ToLower()) != -1)
                {
                    BindData data = new BindData();
                    data.AttributeName = i.Key;
                    data.Host = i.Value;
                    data.Description = i.Value.Description;
                    TreeViewItemsNodes.Add(data);
                }
            }
        }

        #region event
        public List<AttributeType> CurrentAttributeTypes
        {
            get;
            private set;
        }
        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrentAttributeTypes = new List<AttributeType>();
            UIAttribute.TreeListItemsSource = TreeViewItemsNodes;
            RefreshAttributes();
        }

        private void Button_Select(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UIAttribute.SelectedItem == null)
            {
                this.Close();
                return;
            }

            foreach (var item in UIAttribute.SelectedItems)
            {
                EditorCommon.TreeListView.TreeNode treenode = item as EditorCommon.TreeListView.TreeNode;
                BindData data = treenode.Tag as BindData;
                if (data == null)
                {
                    continue;
                }
                CurrentAttributeTypes.Add(data.Host);
            }

            this.Close();
        }

        private void Button_Cancel(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void Windowbase_Closed(object sender, EventArgs e)
        {

        }
    }
}
