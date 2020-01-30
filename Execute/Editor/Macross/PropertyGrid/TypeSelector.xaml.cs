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

namespace Macross.PropertyGrid
{
    public class TypeItem : DependencyObject
    {
        public ImageSource TypeIcon
        {
            get { return (ImageSource)GetValue(TypeIconProperty); }
            set { SetValue(TypeIconProperty, value); }
        }
        public static readonly DependencyProperty TypeIconProperty = DependencyProperty.Register("TypeIcon", typeof(ImageSource), typeof(TypeItem), new UIPropertyMetadata(null));

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(TypeItem), new UIPropertyMetadata(null));
        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register("TypeName", typeof(string), typeof(TypeItem), new UIPropertyMetadata(null));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(TypeItem), new UIPropertyMetadata(""));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TypeItem), new UIPropertyMetadata(false));
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TypeItem), new UIPropertyMetadata(Visibility.Visible));

        public bool NotCreateInInitialize = false;
        public bool IsMacrossGetter = false;
        public Type MacrossClassType = null;

        public ObservableCollection<TypeItem> ChildList
        {
            get;
            set;
        } = new ObservableCollection<TypeItem>();
    }
    /// <summary>
    /// TypeSelector.xaml 的交互逻辑
    /// </summary>
    public partial class TypeSelector : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty = DependencyProperty.Register("BindInstance", typeof(object), typeof(TypeSelector), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChangedCallback)));
        static void OnBindInstanceChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSelector;
            var vcip = e.NewValue as VariableCategoryItemPropertys;
            if (vcip != null)
            {
                if (vcip.VariableTypeReadOnly)
                {
                    ctrl.EnableEdit = false;
                }
            }
            if(e.NewValue != null)
            {
                var csTypePro = e.NewValue.GetType().GetProperty("CSType");
                if(csTypePro != null && csTypePro.PropertyType == typeof(EngineNS.ECSType))
                {
                    ctrl.CSType = (EngineNS.ECSType)csTypePro.GetValue(e.NewValue);
                }
            }
        }
        public bool EnableEdit
        {
            get { return (bool)GetValue(EnableEditProperty); }
            set { SetValue(EnableEditProperty, value); }
        }
        public static readonly DependencyProperty EnableEditProperty = DependencyProperty.Register("EnableEdit", typeof(bool), typeof(TypeSelector), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string CurrentTypeName
        {
            get { return (string)GetValue(CurrentTypeNameProperty); }
            set { SetValue(CurrentTypeNameProperty, value); }
        }
        public static readonly DependencyProperty CurrentTypeNameProperty = DependencyProperty.Register("CurrentTypeName", typeof(string), typeof(TypeSelector), new PropertyMetadata(null));

        public Type VarType
        {
            get { return (Type)GetValue(VarTypeProperty); }
            set { SetValue(VarTypeProperty, value); }
        }

        public static readonly DependencyProperty VarTypeProperty =
            DependencyProperty.Register("VarType", typeof(Type), typeof(TypeSelector), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnVarTypeChangedCallback)));
        static void OnVarTypeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSelector;
            var tp = (Type)e.NewValue;
            ctrl.CurrentTypeName = tp.Name;
            ctrl.InitializeTypes();
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(TypeSelector), new PropertyMetadata(false));

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(TypeSelector), new PropertyMetadata("", new PropertyChangedCallback(OnSearchTextChangedCallback)));
        static void OnSearchTextChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSelector;
            ctrl.ShowItemsWithFilter(ctrl.mTypeItems, (string)e.NewValue);
        }
        bool ShowItemsWithFilter(ObservableCollection<TypeItem> items, string filter)
        {
            bool retValue = false;
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(filter))
                {
                    item.Visibility = Visibility.Visible;
                    item.HighLightString = filter;
                    ShowItemsWithFilter(item.ChildList, filter);
                }
                else
                {
                    if (item.TypeName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        item.Visibility = Visibility.Visible;
                        item.HighLightString = filter;
                        retValue = true;
                        //var bFind = ShowItemsWithFilter(item.ChildList, filter);
                        //if(bFind)
                        //{
                        //    item.IsExpanded = true;
                        //}
                    }
                    else
                    {
                        bool bFind = ShowItemsWithFilter(item.ChildList, filter);
                        if (bFind == false)
                            item.Visibility = Visibility.Collapsed;
                        else
                        {
                            item.Visibility = Visibility.Visible;
                            item.IsExpanded = true;
                            item.HighLightString = filter;
                            retValue = true;
                        }
                    }
                }
            }
            return retValue;
        }
        public TypeSelector()
        {
            InitializeComponent();
        }
        ObservableCollection<TypeItem> mTypeItems = new ObservableCollection<TypeItem>();
        Type[] mGeneralTypes = new Type[] { typeof(bool), typeof(byte), typeof(UInt16), typeof(UInt32), typeof(UInt64), typeof(sbyte), typeof(Int16), typeof(Int32), typeof(Int64),
                                            typeof(float), typeof(double), typeof(string)};
        Dictionary<string, TypeItem> mTypeItemCategorys = new Dictionary<string, TypeItem>();
        void AddTypeItem(List<string> path, ObservableCollection<TypeItem> typeItems, TypeItem item)
        {
            var currentPathName = path[0];
            path.RemoveAt(0);
            if (path.Count == 0)
                typeItems.Add(item);
            else
            {
                bool find = false;
                foreach (var itm in typeItems)
                {
                    if (itm.TypeName == currentPathName)
                    {
                        find = true;
                        AddTypeItem(path, itm.ChildList, item);
                        break;
                    }
                }

                if (!find)
                {
                    var parentItem = new TypeItem();
                    parentItem.TypeName = currentPathName;
                    typeItems.Add(parentItem);
                    AddTypeItem(path, parentItem.ChildList, item);
                }
            }
        }
        TypeItem CreateTypeItem(Type type, string typename = null)
        {
            var retVal = new TypeItem()
            {
                Type = type,
                TypeIcon = TryFindResource("Icon_Pill") as ImageSource,
            };

            if (typename == null)
            {
                retVal.TypeName = type.Name;
            }
            else
            {
                retVal.TypeName = typename;
            }

            return retVal;
        }
        public EngineNS.ECSType CSType = EngineNS.ECSType.Client;

        Type mCurChheckType = null;
        void InitializeTypes()
        {
            if (mCurChheckType == VarType && mTypeItems.Count != 0)
                return;
            mTypeItems.Clear();

            foreach (var type in mGeneralTypes)
            {
                var item = CreateTypeItem(type);
                mTypeItems.Add(item);
            }

            var assemblies = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblys(CSType);
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), false);
                foreach (var typeObj in types)
                {
                    var type = typeObj as Type;

                    // 枚举在后面分析
                    var enumAtts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossEnumAttribute), false);
                    if (enumAtts.Length > 0)
                        continue;

                    var atts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), false);
                    var att = atts[0] as EngineNS.Editor.Editor_MacrossClassAttribute;

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
                    if (att.HasType(EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Createable))
                    {
                        var item = CreateTypeItem(type);
                        AddTypeItem(path, mTypeItems, item);
                    }
                    else if (att.HasType(EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable))
                    {
                        var item = CreateTypeItem(type);
                        item.NotCreateInInitialize = true;
                        AddTypeItem(path, mTypeItems, item);
                    }
                    else if (att.HasType(EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter))
                    {
                        Type gettertype = typeof(EngineNS.Macross.MacrossGetter<>).MakeGenericType(type);//typeof(EngineNS.Macross.MacrossGetter<Object>);
                        var getteritem = CreateTypeItem(gettertype, "MacrossGetter<" + type.Name + ">");
                        getteritem.IsMacrossGetter = true;
                        getteritem.MacrossClassType = type;
                        AddTypeItem(path, mTypeItems, getteritem);
                    }

                }

                var enumtypes = assembly.GetTypes(typeof(EngineNS.Editor.Editor_MacrossEnumAttribute), false);
                foreach (var typeObj in enumtypes)
                {
                    var type = typeObj as Type;

                    List<string> path = null;
                    var nameAtts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_ClassDisplayNameAttribute), false);
                    if (nameAtts.Length > 0)
                    {
                        var nameAtt = nameAtts[0] as EngineNS.Editor.Editor_ClassDisplayNameAttribute;
                        if (nameAtt.DisplayName != null)
                        {
                            path = new List<string>(nameAtt.DisplayName);

                        }
                    }
                    else
                    {
                        path = new List<string>(type.FullName.Split('.'));
                    }

                    if (path == null)
                    {
                        path = new List<string>() { type.Name };
                    }
                    var item = CreateTypeItem(type);
                    AddTypeItem(path, mTypeItems, item);
                }
            }

            //创建macross getter
            //Type gettertype = typeof(EngineNS.Macross.MacrossGetter<Object>);
            //var getteritem = CreateTypeItem(gettertype);
            //getteritem.IsMacrossGetter = true;
            //mTypeItems.Add(getteritem);

            TreeView_Types.ItemsSource = mTypeItems;
        }
        public Func<bool> CanChangeType;
        public Action<TypeItem> OnTypeChangedAction;
        private void TreeView_Types_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var cls = e.NewValue as TypeItem;
            if (cls == null)
            {
                IsDropDownOpen = false;
                return;
            }
            if (cls.Type == null)
                return;
            
            if (CanChangeType?.Invoke() == false)
            {
                IsDropDownOpen = false;
                return;
            }

            if (cls.ChildList.Count == 0)
                IsDropDownOpen = false;

            //CurrentTypeName = cls.Type.Name;
            VarType = cls.Type;
            OnTypeChangedAction?.Invoke(cls);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTypes();
        }
    }
}
