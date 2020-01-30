using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EditorCommon.Bind
{
    /// <summary>
    /// Interaction logic for BindButton.xaml
    /// </summary>
    public partial class BindButton : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(BindButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(BindButton), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));

        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindButton edit = d as BindButton;

            edit.Binded = false;

            bool bFindAttribute = false;
            var newPro = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            foreach (var att in newPro.Attributes)
            {
                if (att is EngineNS.Editor.UIEditor_BindingPropertyAttribute)
                {
                    bFindAttribute = true;
                    break;
                }
            }

            if (bFindAttribute)
            {
                edit.BindingRectVisibility = Visibility.Visible;
            }
            else
            {
                edit.BindingRectVisibility = Visibility.Collapsed;
                return;
            }

            //edit.ComboBox_BindClass.Items.Clear();
            //edit.ComboBox_BindProperty.Items.Clear();
            //var bindClassList = UISystem.UIReflectionManager.Instance.GetBindClassInfosWithPropertyType(newPro);
            //foreach (var bindInfo in bindClassList)
            //{
            //    edit.ComboBox_BindClass.Items.Add(bindInfo.ClassType.FullName);
            //}

            //UISystem.WinBase win = edit.BindInstance as UISystem.WinBase;
            //if (win != null)
            //{
            //    var proBindInfo = win.GetClassPropertyBinds(newPro.Name);
            //    if (proBindInfo != null)
            //    {
            //        edit.ComboBox_BindMode.SelectedItem = proBindInfo.BindingMode.ToString();
            //        edit.ComboBox_BindClass.SelectedItem = proBindInfo.ClassType.FullName;
            //        edit.ComboBox_BindProperty.SelectedItem = proBindInfo.PropertyInfo.Name;
            //        edit.Binded = true;
            //    }
            //}

            edit.TextBlockPropertyName.Text = newPro.Name;
            //if (edit.mClassPropertyBindInfo != null)
            //{
            //    CodeLinker.ChildObjectInfo ccInfo;
            //    if (edit.m_ctrlDataBase.m_bindChildObjectDic.TryGetValue(newPro.Name, out ccInfo))
            //    {
            //        edit.BindingChildObject = ccInfo;
            //    }
            //    else
            //        edit.BindingChildObject = null;
            //}
            //edit.UpdateBindShow();
        }

        public Visibility BindingRectVisibility
        {
            get { return (Visibility)GetValue(BindingRectVisibilityProperty); }
            set { SetValue(BindingRectVisibilityProperty, value); }
        }
        public static readonly DependencyProperty BindingRectVisibilityProperty =
                    DependencyProperty.Register("BindingRectVisibility", typeof(Visibility), typeof(BindButton), new UIPropertyMetadata(Visibility.Collapsed));

        protected bool Binded
        {
            set
            {
                if (value)
                {
                    Rect_Btn.Fill = FindResource("BindedBrush") as Brush;
                }
                else
                {
                    Rect_Btn.Fill = FindResource("UnBindedBrush") as Brush;
                }
            }
        }

        public BindButton()
        {
            InitializeComponent();
        }

        //bool mBindInitialized = false;
        private void UpdateBindShow()
        {
            //if (mBindInitialized)
            //    return;

            //////ListBox_ClassBind.Items.Clear();
            //////ListBox_ControlBind.Items.Clear();

            //////UISystem.WinBase win = BindInstance as UISystem.WinBase;
            //////if (win != null)
            //////{
            //////    var bindInfos = win.GetPropertyBinds(BindProperty.Name);
            //////    if (bindInfos != null)
            //////    {
            //////        // 新的UI逻辑图，绑定不一样了，这里先注掉
            //////        //////foreach (UISystem.Bind.ClassPropertyBindInfo bindInfo in (from bind in bindInfos where bind is UISystem.Bind.ClassPropertyBindInfo select bind))
            //////        //////{
            //////        //////    ClassPropertyBindControl ctrl = new ClassPropertyBindControl(BindInstance, BindProperty, bindInfo);

            //////        //////    ListBox_ClassBind.Items.Add(ctrl);
            //////        //////}
            //////        //////foreach (UISystem.Bind.ControlPropertyBindInfo bindInfo in (from bind in bindInfos where bind is UISystem.Bind.ControlPropertyBindInfo select bind))
            //////        //////{
            //////        //////    ControlPropertyBindControl ctrl = new ControlPropertyBindControl(BindInstance, BindProperty, bindInfo);

            //////        //////    ListBox_ControlBind.Items.Add(ctrl);
            //////        //////}
            //////        //foreach (var bindInfo in bindInfos)
            //////        //{
            //////        //    if (bindInfo is UISystem.Bind.ClassPropertyBindInfo)
            //////        //    {
            //////        //        ClassPropertyBindControl ctrl = new ClassPropertyBindControl(BindInstance, BindProperty, (UISystem.Bind.ClassPropertyBindInfo)bindInfo);
            //////        //        //ctrl.BindInstance = BindInstance;
            //////        //        //ctrl.BindProperty = BindProperty;
            //////        //        ListBox_ClassBind.Items.Add(ctrl);
            //////        //    }
            //////        //    else if (bindInfo is UISystem.Bind.ControlPropertyBindInfo)
            //////        //    {
            //////        //        ControlPropertyBindControl ctrl = new ControlPropertyBindControl(BindInstance, BindProperty, (UISystem.Bind.ControlPropertyBindInfo)bindInfo);
            //////        //        ListBox_ControlBind.Items.Add(ctrl);
            //////        //    }
            //////        //}
            //////    }
            //////}

            //////if (ListBox_ClassBind.Items.Count > 0 || ListBox_ControlBind.Items.Count > 0)
            //////    Binded = true;
            //////else
            //////    Binded = false;
            IsEnabled = true;
            var bindInsType = BindInstance.GetType();
            if(bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                // 多选
                int count = 0;
                foreach (var objIns in (IEnumerable)BindInstance)
                {
                    count++;
                }
                if(count > 1)
                {
                    BindFunctionName = "无法同时操作多个控件";
                    IsEnabled = false;
                }
                foreach (var objIns in (IEnumerable)BindInstance)
                {
                    if (objIns == null)
                        continue;

                    var uiElement = objIns as EngineNS.UISystem.UIElement;
                    if (uiElement == null)
                    {
                        continue;
                    }
                    string bindFunctionName;
                    if(uiElement.PropertyBindFunctions.TryGetValue(BindProperty.Name, out bindFunctionName))
                    {
                        BindFunctionName = $"UIBindFunc_{BindProperty.Name}({uiElement.Initializer.Name})";// bindFunctionName;
                        BindFunctionOpIsAdd = false;
                    }
                    else
                    {
                        BindFunctionOpIsAdd = true;
                    }

                    List<EngineNS.UISystem.VariableBindInfo> varBindInfos;
                    if(uiElement.VariableBindInfosDic.TryGetValue(BindProperty.Name, out varBindInfos))
                    {
                        ListBox_VariableBind.Items.Clear();
                        foreach(var info in varBindInfos)
                        {
                            var ctrl = new EditorCommon.Bind.VariableBind(this, uiElement, info, BindProperty.GetPropertyType(uiElement));
                            ListBox_VariableBind.Items.Add(ctrl);
                        }
                    }

                    UpdateBindShow(uiElement);
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                if(uiElement != null)
                {
                    string bindFunctionName;
                    if (uiElement.PropertyBindFunctions.TryGetValue(BindProperty.Name, out bindFunctionName))
                    {
                        BindFunctionName = $"UIBindFunc_{BindProperty.Name}({uiElement.Initializer.Name})";// bindFunctionName;
                        BindFunctionOpIsAdd = false;
                    }
                    else
                    {
                        BindFunctionOpIsAdd = true;
                    }

                    List<EngineNS.UISystem.VariableBindInfo> varBindInfos;
                    if (uiElement.VariableBindInfosDic.TryGetValue(BindProperty.Name, out varBindInfos))
                    {
                        foreach (var info in varBindInfos)
                        {
                            var ctrl = new EditorCommon.Bind.VariableBind(this, uiElement, info, BindProperty.GetPropertyType(uiElement));
                            ListBox_VariableBind.Items.Add(ctrl);
                        }
                    }

                    UpdateBindShow(uiElement);
                }
            }

            //mBindInitialized = true;
        }

        public void UpdateBindShow(EngineNS.UISystem.UIElement uiElement)
        {
            if (uiElement.VariableBindInfosDic.Count > 0)
                TB_AddVariableBind.Visibility = Visibility.Collapsed;
            else
                TB_AddVariableBind.Visibility = Visibility.Visible;

            if(uiElement.PropertyBindFunctions.ContainsKey(BindProperty.Name))
            {
                Binded = true;
                return;
            }
            if(uiElement.VariableBindInfosDic.ContainsKey(BindProperty.Name))
            {
                Binded = true;
                return;
            }
            Binded = false;
        }

        private void Button_ResetToDefault(object sender, System.Windows.RoutedEventArgs e)
        {
            //////var control = BindInstance as UISystem.WinBase;
            //////if (control == null)
            //////    return;

            //////var value = control.DefaultValueTemplate.GetDefaultValue(BindProperty.Name);
            //////BindProperty.SetValue(BindInstance, value);
        }

        private void userControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //UpdateBindShow();
        }

        public string BindFunctionName
        {
            get { return (string)GetValue(BindFunctionNameProperty); }
            set { SetValue(BindFunctionNameProperty, value); }
        }
        public static readonly DependencyProperty BindFunctionNameProperty = DependencyProperty.Register("BindFunctionName", typeof(string), typeof(BindButton), new UIPropertyMetadata(""));
        public bool BindFunctionOpIsAdd
        {
            get { return (bool)GetValue(BindFunctionOpIsAddProperty); }
            set { SetValue(BindFunctionOpIsAddProperty, value); }
        }
        public static readonly DependencyProperty BindFunctionOpIsAddProperty = DependencyProperty.Register("BindFunctionOpIsAdd", typeof(bool), typeof(BindButton), new UIPropertyMetadata(true));
        public bool BindFunctionOpEnable
        {
            get { return (bool)GetValue(BindFunctionOpEnableProperty); }
            set { SetValue(BindFunctionOpEnableProperty, value); }
        }
        public static readonly DependencyProperty BindFunctionOpEnableProperty = DependencyProperty.Register("BindFunctionOpEnable", typeof(bool), typeof(BindButton), new UIPropertyMetadata(true));
        private void Button_AddCustomBind_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            BindFunctionOpIsAdd = false;
            Binded = true;
            var bindInsType = BindInstance.GetType();
            if (bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                foreach(var insObj in (IEnumerable)BindInstance)
                {
                    var uiElement = insObj as EngineNS.UISystem.UIElement;
                    //BindFunctionName = $"UIBindFunc_{uiElement.Initializer.Name}_{BindProperty.Name}";
                    BindFunctionName = $"UIBindFunc_{BindProperty.Name}({uiElement.Initializer.Name})";
                    uiElement.PropertyCustomBindAddAction?.Invoke(uiElement, BindProperty.Name, BindProperty.GetPropertyType(uiElement));
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                //BindFunctionName = $"UIBindFunc_{uiElement.Initializer.Name}_{BindProperty.Name}";
                BindFunctionName = $"UIBindFunc_{BindProperty.Name}({uiElement.Initializer.Name})";
                uiElement.PropertyCustomBindAddAction?.Invoke(uiElement, BindProperty.Name, BindProperty.GetPropertyType(uiElement));
            }
        }
        private void Button_FindCustomBind_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var bindInsType = BindInstance.GetType();
            if (bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                foreach (var insObj in (IEnumerable)BindInstance)
                {
                    var uiElement = insObj as EngineNS.UISystem.UIElement;
                    uiElement.PropertyCustomBindFindAction?.Invoke(uiElement, BindProperty.Name);
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                uiElement.PropertyCustomBindFindAction?.Invoke(uiElement, BindProperty.Name);
            }
            popup.IsOpen = false;
        }
        private void Button_DelCustomBind_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (EditorCommon.MessageBox.Show($"即将删除绑定函数{BindFunctionName}，删除后不可恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                return;
            var bindInsType = BindInstance.GetType();
            if (bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                foreach (var insObj in (IEnumerable)BindInstance)
                {
                    var uiElement = insObj as EngineNS.UISystem.UIElement;
                    uiElement.PropertyCustomBindRemoveAction?.Invoke(uiElement, BindProperty.Name);
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                uiElement.PropertyCustomBindRemoveAction?.Invoke(uiElement, BindProperty.Name);
            }
            BindFunctionOpIsAdd = true;
            Binded = false;
            BindFunctionName = "";
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBindShow();
        }

        #region VariableBind

        class UIElementViewMode : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            public List<UIElementViewMode> Children
            {
                get;
                set;
            } = new List<UIElementViewMode>();

            string mName;
            public string Name
            {
                get => mName;
                set
                {
                    mName = value;
                    OnPropertyChanged("Name");
                }
            }

            ImageSource mImageIcon = null;
            public ImageSource ImageIcon
            {
                get { return mImageIcon; }
                set
                {
                    mImageIcon = value;
                    OnPropertyChanged("ImageIcon");
                }
            }

            string mHightLightString;
            public string HightLightString
            {
                get => mHightLightString;
                set
                {
                    mHightLightString = value;
                    OnPropertyChanged("HightLightString");
                }
            }

            Visibility mVisibility = Visibility.Visible;
            public Visibility Visibility
            {
                get => mVisibility;
                set
                {
                    mVisibility = value;
                    OnPropertyChanged("Visibility");
                }
            }
            bool mIsExpanded = true;
            public bool IsExpanded
            {
                get => mIsExpanded;
                set
                {
                    mIsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
            EngineNS.UISystem.UIElement mHostElement;
            public EngineNS.UISystem.UIElement HostElement
            {
                get => mHostElement;
            }
            public UIElementViewMode(EngineNS.UISystem.UIElement uiElement)
            {
                mHostElement = uiElement;
                Name = uiElement.Initializer.Name;
                if (string.IsNullOrEmpty(Name))
                    Name = "[" + uiElement.GetType().Name + "]";
                var atts = uiElement.GetType().GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as EngineNS.UISystem.Editor_UIControlAttribute;
                    ImageIcon = new BitmapImage(new Uri($"/UIEditor;component/Icons/{att.Icon}", UriKind.Relative));
                }

                var panel = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
                if(panel != null)
                {
                    foreach(var child in panel.ChildrenUIElements)
                    {
                        var vm = new UIElementViewMode(child);
                        Children.Add(vm);
                    }
                }
            }

            public void Reset()
            {
                Visibility = Visibility.Visible;
                HightLightString = "";

                foreach(var child in Children)
                {
                    child.Reset();
                }
            }
            public bool ShowWithFilter(string filterStr)
            {
                bool retValue = false;
                foreach(var child in Children)
                {
                    retValue |= child.ShowWithFilter(filterStr);
                }

                if (Name.ToLower().Contains(filterStr))
                {
                    HightLightString = filterStr;
                    retValue |= true;
                }
                else
                {
                    HightLightString = "";
                    retValue |= false;
                }
                if (retValue)
                    Visibility = Visibility.Visible;
                else
                    Visibility = Visibility.Collapsed;
                return retValue;
            }
        }
        string mUIElementFilterString;
        public string UIElementFilterString
        {
            get => mUIElementFilterString;
            set
            {
                mUIElementFilterString = value;
                ShowUIElementWithFilter();
            }
        }
        List<UIElementViewMode> mUIVMs = new List<UIElementViewMode>();
        void ShowUIElementWithFilter()
        {
            if(string.IsNullOrEmpty(UIElementFilterString))
            {
                foreach(var vm in mUIVMs)
                {
                    vm.Reset();
                }
            }
            else
            {
                var lowerFilter = mUIElementFilterString.ToLower();
                foreach(var vm in mUIVMs)
                {
                    vm.ShowWithFilter(lowerFilter);
                }
            }

            TreeView_UIs.ItemsSource = mUIVMs;
        }

        class VariableViewMode : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            string mVarialbeName;
            public string VarialbeName
            {
                get => mVarialbeName;
                set
                {
                    mVarialbeName = value;
                    OnPropertyChanged("VarialbeName");
                }
            }

            public Type VariableType;
            public EngineNS.UISystem.UIElement UI;

            string mHightLightString;
            public string HightLightString
            {
                get => mHightLightString;
                set
                {
                    mHightLightString = value;
                    OnPropertyChanged("HightLightString");
                }
            }
        }

        System.Collections.ObjectModel.ObservableCollection<VariableViewMode> mAllVariables = new System.Collections.ObjectModel.ObservableCollection<VariableViewMode>();
        System.Collections.ObjectModel.ObservableCollection<VariableViewMode> mShowVariables = new System.Collections.ObjectModel.ObservableCollection<VariableViewMode>();
        string mFilterString;
        public string FilterString
        {
            get => mFilterString;
            set
            {
                mFilterString = value;
                ShowVariablesWithFilter();
            }
        }
        void ShowVariablesWithFilter()
        {
            mShowVariables.Clear();
            if(string.IsNullOrEmpty(mFilterString))
            {
                foreach(var val in mAllVariables)
                {
                    val.HightLightString = "";
                }
                mShowVariables = new System.Collections.ObjectModel.ObservableCollection<VariableViewMode>(mAllVariables);
            }
            else
            {
                var lowerFiler = mFilterString.ToLower();
                foreach (var item in mAllVariables)
                {
                    if (item.VarialbeName.ToLower().Contains(lowerFiler))
                    {
                        item.HightLightString = lowerFiler;
                        mShowVariables.Add(item);
                    }
                }
            }
            ListBox_Variables.ItemsSource = mShowVariables;
        }

        private void ListBoxVariables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Variables.SelectedIndex < 0)
                OKButton.IsEnabled = false;
            else
                OKButton.IsEnabled = true;
        }
        void AddVariableBind(EngineNS.UISystem.UIElement uiElement)
        {
            var bindInfo = new EngineNS.UISystem.VariableBindInfo();
            bindInfo.BindFromUIElementId = uiElement.Id;
            bindInfo.BindFromPropertyName = BindProperty.Name;
            bindInfo.BindFromPropertyType = BindProperty.GetPropertyType(uiElement);

            var selVal = (VariableViewMode)ListBox_Variables.SelectedValue;
            bindInfo.BindToUIElementId = selVal.UI.Id;
            bindInfo.VariableName = selVal.VarialbeName;
            bindInfo.BindToVariableType = selVal.VariableType;
            //bindInfo.FunctionName = "VariableFunc_" + System.Guid.NewGuid().ToString().Replace("-", "_");
            uiElement.VariableBindAddAction(bindInfo);

            var varBindCtrl = new EditorCommon.Bind.VariableBind(this, uiElement, bindInfo, BindProperty.GetPropertyType(uiElement));
            ListBox_VariableBind.Items.Add(varBindCtrl);
        }
        private void Grid_VariableBindOP_Loaded(object sender, RoutedEventArgs e)
        {
            var bindInsType = BindInstance.GetType();
            if (bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                foreach (var insObj in (IEnumerable)BindInstance)
                {
                    var uiElement = insObj as EngineNS.UISystem.UIElement;
                    mCurrentProcessUIElement = uiElement;
                    InitVariableShow(uiElement);
                    break;
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                mCurrentProcessUIElement = uiElement;
                InitVariableShow(uiElement);
            }
        }

        void InitVariableShow(EngineNS.UISystem.UIElement uiElement)
        {
            if (uiElement == null)
                return;
            var parent = uiElement;
            while (parent != null)
            {
                if (parent.Parent is EngineNS.UISystem.UIHost)
                    break;
                parent = parent.Parent;
            }

            var viewMode = new UIElementViewMode(parent);
            mUIVMs.Clear();
            mUIVMs.Add(viewMode);
            ShowUIElementWithFilter();
        }
        EngineNS.UISystem.UIElement mCurrentProcessUIElement;
        private void TreeView_UIs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = TreeView_UIs.SelectedItem as UIElementViewMode;

            var type = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(item.HostElement.GetType().FullName);
            if (type == null)
                type = item.HostElement.GetType();

            List<string> proExist = new List<string>();
            foreach (VariableBind ctrl in ListBox_VariableBind.Items)
            {
                if (ctrl.BindInfo.BindToUIElementId == item.HostElement.Id)
                {
                    proExist.Add(ctrl.BindInfo.VariableName);
                }
            }

            var pros = type.GetProperties();
            mAllVariables.Clear();
            foreach (var pro in pros)
            {
                var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), true);
                if (atts.Length == 0)
                    continue;
                if (pro.Name == BindProperty.Name &&
                   item.HostElement == mCurrentProcessUIElement)
                    continue;

                if (proExist.Contains(pro.Name))
                    continue;

                var val = new VariableViewMode();
                val.UI = item.HostElement;
                val.VarialbeName = pro.Name;
                val.VariableType = pro.PropertyType;
                mAllVariables.Add(val);
            }
            ShowVariablesWithFilter();
        }
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            var bindInsType = BindInstance.GetType();
            if (bindInsType.GetInterface(typeof(IEnumerable).FullName, false) != null)
            {
                foreach (var insObj in (IEnumerable)BindInstance)
                {
                    var uiElement = insObj as EngineNS.UISystem.UIElement;
                    AddVariableBind(uiElement);
                }
            }
            else
            {
                var uiElement = BindInstance as EngineNS.UISystem.UIElement;
                AddVariableBind(uiElement);
            }

            TB_AddVariableBind.IsChecked = false;
            TB_AddVariableBind.Visibility = Visibility.Collapsed;
        }
        #endregion

    }
}
