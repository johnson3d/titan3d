using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EditorCommon.Resources;
using EngineNS.Bricks.Animation.Skeleton;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for SocketSelectControl.xaml
    /// </summary>
    public partial class ClassPropertySelectControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        ObservableCollection<string> mProperties = new ObservableCollection<string>();
        public ObservableCollection<string> Properties
        {
            get { return mProperties; }
            set
            {
                mProperties = value;
                OnPropertyChanged("MemberList");
            }
        }


        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(ClassPropertySelectControl), new PropertyMetadata(new PropertyChangedCallback(OnPropertyNamePropertyChangedCallback)));

        static void OnPropertyNamePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        Type ClassType { get; set; } = null;
        Type[] FilterTypes { get; set; } = null;
        #region 筛选

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                ListBox_Container.Items.Filter = new Predicate<object>((object obj) =>
                {
                    var item = obj as TreeItemBone;
                    var name = item.Name;
                    if (name.ToLower().Contains(mFilterString.ToLower()))
                        return true;
                    return false;
                });
                if (string.IsNullOrEmpty(mFilterString))
                {
                    ListBox_Container.Items.Filter = null;
                }
                OnPropertyChanged("FilterString");
            }
        }

        #endregion
        public ClassPropertySelectControl()
        {
            InitializeComponent();
        }

        private void IconTextBtn_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            mProperties.Clear();
            //如果不是macrossType return Null
            var rname = EngineNS.Rtti.RttiHelper.GetRNameFromType(ClassType, true);
            var newType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(ClassType.FullName);
            if (newType == null)
                newType = ClassType;
            var properties = newType.GetProperties();
            for (int i = 0; i < properties.Length; ++i)
            {
                for (int j = 0; j < FilterTypes.Length; ++j)
                {
                    if (properties[i].PropertyType == FilterTypes[j])
                        mProperties.Add(properties[i].Name);
                }
            }

            ListBox_Container.ItemsSource = mProperties;
            SearchBoxCtrl.FocusInput();
        }
        private void ListBox_Container_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListBox_Container.SelectedItem;
            if (item == null)
                return;
            else
            {
                PropertyName = item as string;
            }
        }
        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(ClassPropertySelectControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));
        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var edit = d as ClassPropertySelectControl;

            var newPro = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            foreach (var att in newPro.Attributes)
            {
                if (att is EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute)
                {
                    var cms = att as EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute;
                    edit.ClassType = cms.ClassType;
                    edit.FilterTypes = cms.FilterTypes;
                }
            }
        }
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(ClassPropertySelectControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
