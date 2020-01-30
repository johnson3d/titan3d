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

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// PropertyClassOperator.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyClassOperator : UserControl
    {
        public Type PropertyType
        {
            get { return (Type)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(Type), typeof(PropertyClassOperator), new UIPropertyMetadata(null, new PropertyChangedCallback(PropertyTypeChangedCallback)));
        static void PropertyTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PropertyClassOperator;
            var newValue = (Type)e.NewValue;
            if (newValue.GetCustomAttributes(typeof(EngineNS.Editor.Editor_NoDefaultObjectAttribute), true).Length > 0)
                ctrl.ShowCreateOrReplaceBtn = Visibility.Collapsed;
            else
                ctrl.ShowCreateOrReplaceBtn = Visibility.Visible;
        }
        public object ClassValue
        {
            get { return GetValue(ClassValueProperty); }
            set { SetValue(ClassValueProperty, value); }
        }

        public static readonly DependencyProperty ClassValueProperty =
            DependencyProperty.Register("ClassValue", typeof(object), typeof(PropertyClassOperator), new UIPropertyMetadata(null, new PropertyChangedCallback(ClassValuePropertyChangedCallback)));
        static void ClassValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PropertyClassOperator;
            ctrl.PG.Instance = e.NewValue;
            ctrl.PG.Filter = ctrl.FilterString;
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(PropertyClassOperator), new PropertyMetadata(false));

        public string FilterString
        {
            get { return (string)GetValue(FilterStringProperty); }
            set { SetValue(FilterStringProperty, value); }
        }
        public static readonly DependencyProperty FilterStringProperty =
            DependencyProperty.Register("FilterString", typeof(string), typeof(PropertyClassOperator), new UIPropertyMetadata("", new PropertyChangedCallback(FilterStringPropertyChangedCallback)));
        static void FilterStringPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PropertyClassOperator;
            ctrl.PG.Filter = (string)e.NewValue;
        }
        public Visibility ShowCreateOrReplaceBtn
        {
            get { return (Visibility)GetValue(ShowCreateOrReplaceBtnProperty); }
            set { SetValue(ShowCreateOrReplaceBtnProperty, value); }
        }
        public static readonly DependencyProperty ShowCreateOrReplaceBtnProperty = DependencyProperty.Register("ShowCreateOrReplaceBtn", typeof(Visibility), typeof(PropertyClassOperator), new PropertyMetadata(Visibility.Visible));

        public PropertyClassOperator()
        {
            InitializeComponent();
        }
        ObservableCollection<TypeItem> mTypeItems = new ObservableCollection<TypeItem>();
        void InitializeTypes()
        {
            mTypeItems.Clear();

            if (PropertyType == null)
                return;

            ShowCreateOrReplaceBtn = Visibility.Visible;
            // null
            var item = TypeItem.CreateTypeItem(null, TryFindResource("Icon_Pill") as ImageSource, "null");
            TypeItem.AddTypeItem(new List<string>() { "null" }, mTypeItems, item);

            var baseType = PropertyType;
            var assemblies = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblys(EngineNS.ECSType.All);
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var typeObj in types)
                {
                    var type = typeObj as Type;
                    if (type.Name.Contains("<"))
                        continue;
                    if (type.Name.Contains("="))
                        continue;

                    if (type != baseType)
                    {
                        if (baseType.IsInterface)
                        {
                            if (type.GetInterface(baseType.FullName) == null)
                                continue;
                        }
                        else if (!type.IsSubclassOf(baseType))
                            continue;
                    }

                    var cons = type.GetConstructor(Type.EmptyTypes);
                    // 只能显示有默认构造函数的类型
                    if (cons == null)
                        continue;

                    List<string> path = new List<string>() { type.Name };
                    var nameAtts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_ClassDisplayNameAttribute), false);
                    if (nameAtts.Length > 0)
                    {
                        var nameAtt = nameAtts[0] as EngineNS.Editor.Editor_ClassDisplayNameAttribute;
                        if (nameAtt.DisplayName != null)
                            path = new List<string>(nameAtt.DisplayName);
                    }
                    else
                    {
                        path = new List<string>(type.FullName.Split('.'));
                    }

                    item = TypeItem.CreateTypeItem(type, TryFindResource("Icon_Pill") as ImageSource);
                    TypeItem.AddTypeItem(path, mTypeItems, item);
                }
            }

            TreeView_Types.ItemsSource = mTypeItems;
        }

        private void TreeView_Types_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue == null)
                return;
            IsDropDownOpen = false;
            var cls = e.NewValue as TypeItem;
            if (cls == null)
                return;

            if (cls.Type == null)
                ClassValue = null;
            else
                ClassValue = EngineNS.Rtti.RttiHelper.CreateInstance(cls.Type);
            //if (cls == null)
            //{
            //    IsDropDownOpen = false;
            //    return;
            //}
            //if (cls.Type == null)
            //    return;

            //if (CanChangeType?.Invoke() == false)
            //{
            //    IsDropDownOpen = false;
            //    return;
            //}

            //if (cls.ChildList.Count == 0)
            //    IsDropDownOpen = false;

            ////CurrentTypeName = cls.Type.Name;
            //VarType = cls.Type;
            //OnTypeChangedAction?.Invoke(cls);
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            InitializeTypes();
        }
    }
}
