using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EditorCommon.Bind
{
    /// <summary>
    /// Interaction logic for ControlPropertyBindControl.xaml
    /// </summary>
    public partial class ControlPropertyBindControl : UserControl
    {
        //UISystem.Bind.ControlPropertyBindInfo mHostBindInfo;
        //public UISystem.Bind.ControlPropertyBindInfo HostBindInfo
        //{
        //    get { return mHostBindInfo; }
        //}

        //public object BindInstance
        //{
        //    get { return (object)GetValue(BindInstanceProperty); }
        //    set { SetValue(BindInstanceProperty, value); }
        //}
        //public static readonly DependencyProperty BindInstanceProperty =
        //                    DependencyProperty.Register("BindInstance", typeof(object), typeof(ControlPropertyBindControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        //public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}

        //public EditorCommon.CustomPropertyDescriptor BindProperty
        //{
        //    get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
        //    set { SetValue(BindPropertyProperty, value); }
        //}
        //public static readonly DependencyProperty BindPropertyProperty =
        //                    DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(ControlPropertyBindControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));

        //public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    //var edit = d as ControlPropertyBindControl;
        //    //edit.UpdateControlBindShow();
        //}

        //private Dictionary<UISystem.UIBindAutoUpdate, TextBlock> mControlTextBlockDictionarys = new Dictionary<UISystem.UIBindAutoUpdate, TextBlock>();

        //public ControlPropertyBindControl(object bindInstance, EditorCommon.CustomPropertyDescriptor bindProperty, UISystem.Bind.ControlPropertyBindInfo hostBindInfo)
        //{
        //    InitializeComponent();

        //    mHostBindInfo = hostBindInfo;
        //    BindInstance = bindInstance;
        //    BindProperty = bindProperty;

        //    foreach (var enName in System.Enum.GetNames(typeof(UISystem.Bind.enBindingMode)))
        //    {
        //        ComboBox_BindMode.Items.Add(enName);
        //    }
        //    //ComboBox_BindMode.SelectedIndex = 0;
        //    UpdateControlBindShow();
        //    UpdateBindInfoShow(mHostBindInfo);
        //}

        //private void UpdateControlBindShow()
        //{
        //    var winControl = BindInstance as UISystem.WinBase;
        //    if (winControl == null)
        //        return;

        //    var root = winControl.GetRoot(typeof(UISystem.Template.ControlTemplate));
        //    if (root == null)
        //        root = winControl.GetRoot(typeof(UISystem.WinForm));
        //    if (root == null)
        //        return;

        //    ComboBox_BindControl.Items.Clear();
        //    ComboBox_BindProperty.Items.Clear();

        //    var childControls = root.GetAllChildControls();
        //    foreach (UISystem.WinBase child in childControls)
        //    {
        //        TextBlock textBlock = new TextBlock();
        //        textBlock.SetBinding(TextBlock.TextProperty, new Binding("NameInEditor") { Source = child });
        //        textBlock.Tag = child;
        //        ComboBox_BindControl.Items.Add(textBlock);

        //        mControlTextBlockDictionarys[child] = textBlock;
        //    }

        //    //var proBindInfo = winControl.GetControlPropertyBinds(BindProperty.Name);
        //    //if (proBindInfo != null)
        //    //{
        //    //    ComboBox_BindMode.SelectedItem = proBindInfo.BindingMode.ToString();
        //    //    ComboBox_BindControl.SelectedItem = proBindInfo.SourceObject.NameInEditor;
        //    //    ComboBox_BindProperty.SelectedItem = proBindInfo.SourcePropertyInfo.Name;
        //    //    Binded = true;
        //    //}
        //}

        //private void UpdateBindInfoShow(UISystem.Bind.ControlPropertyBindInfo info)
        //{
        //    if (info != null)
        //    {
        //        if (info.SourceObject == null && !string.IsNullOrEmpty(info.mParseDataString))
        //        {
        //            var winControl = BindInstance as UISystem.WinBase;
        //            if (winControl == null)
        //                return;

        //            var root = winControl.GetRoot(typeof(UISystem.Template.ControlTemplate));
        //            if (root == null)
        //                root = winControl.GetRoot(typeof(UISystem.WinForm));
        //            if (root == null)
        //                return;

        //            //UISystem.Bind.ControlPropertyBindInfo.Parse(root, info);// info.Parse(root);
        //            info.Parse(root as UISystem.WinBase);
        //        }

        //        ComboBox_BindMode.SelectedItem = info.BindingMode.ToString();
        //        TextBlock bindControl = null;
        //        if (info.SourceObject != null && mControlTextBlockDictionarys.TryGetValue(info.SourceObject, out bindControl))
        //        {
        //            ComboBox_BindControl.SelectedItem = bindControl;//.NameInEditor;
        //        }
        //        if(info.SourcePropertyInfo != null)
        //            ComboBox_BindProperty.SelectedItem = info.SourcePropertyInfo.Name;
        //        //Binded = true;
        //    }
        //}
        
        //private void ComboBox_BindControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    ComboBox_BindProperty.Items.Clear();

        //    if (ComboBox_BindControl.SelectedIndex < 0)
        //        return;

        //    var targetControl = ((TextBlock)ComboBox_BindControl.SelectedItem).Tag as UISystem.WinBase;

        //    mHostBindInfo.SourceObject = targetControl;
            
        //    foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(targetControl))
        //    {
        //        var attribute = property.Attributes[typeof(CSUtility.Editor.UIEditor_BindingPropertyAttribute)];
        //        if (attribute == null)
        //            continue;

        //        if (property.PropertyType != BindProperty.PropertyType)
        //        {
        //            CSUtility.Editor.UIEditor_BindingPropertyAttribute chkBindAttribute = BindProperty.Attributes[typeof(CSUtility.Editor.UIEditor_BindingPropertyAttribute)] as CSUtility.Editor.UIEditor_BindingPropertyAttribute;
        //            if (chkBindAttribute == null || chkBindAttribute.AvailableTypes == null)
        //                continue;

        //            bool bFindType = false;
        //            foreach (var chkType in chkBindAttribute.AvailableTypes)
        //            {
        //                if (chkType == property.PropertyType)
        //                {
        //                    bFindType = true;
        //                    break;
        //                }
        //            }

        //            if (!bFindType)
        //                continue;
        //        }

        //        ComboBox_BindProperty.Items.Add(property.Name);
        //    }
        //}

        //private void ComboBox_BindProperty_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (ComboBox_BindControl.SelectedIndex < 0 || ComboBox_BindProperty.SelectedIndex < 0)
        //        return;

        //    UISystem.WinBase win = BindInstance as UISystem.WinBase;
        //    if (win == null)
        //        return;

        //    var targetControl = ((TextBlock)ComboBox_BindControl.SelectedItem).Tag as UISystem.WinBase;
        //    var propertyInfo = TypeDescriptor.GetProperties(targetControl)[ComboBox_BindProperty.SelectedItem.ToString()];
        //    var bindMode = (UISystem.Bind.enBindingMode)(System.Enum.Parse(typeof(UISystem.Bind.enBindingMode), ComboBox_BindMode.SelectedItem.ToString()));
        //    mHostBindInfo.SourceObject = targetControl;
        //    mHostBindInfo.SourcePropertyInfo = propertyInfo;
        //    mHostBindInfo.BindingMode = bindMode;
        //    //switch (bindMode)
        //    //{
        //    //    case UISystem.Bind.enBindingMode.OneWay:
        //    //        {
        //    //            //var bindInfo = targetControl.GetControlPropertyBinds(propertyInfo.Name);
        //    //            //if (bindInfo == null)
        //    //            //{
        //    //            //    bindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            //    targetControl.AddControlPropertyBind(propertyInfo.Name, bindInfo);
        //    //            //}

        //    //            //bindInfo.SourceObject = win;
        //    //            //bindInfo.SourcePropertyInfo = win.GetType().GetProperty(BindProperty.Name);
        //    //            //bindInfo.BindingMode = bindMode;
        //    //            UISystem.Bind.ControlPropertyBindInfo bindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            bindInfo.SourceObject = win;
        //    //            bindInfo.SourcePropertyInfo = win.GetType().GetProperty(BindProperty.Name);
        //    //            bindInfo.BindingMode = UISystem.Bind.enBindingMode.OneWay;
        //    //            targetControl.AddControlPropertyBind(propertyInfo.Name, bindInfo);
        //    //        }
        //    //        break;

        //    //    case UISystem.Bind.enBindingMode.OneWayToSource:
        //    //        {
        //    //            //var bindInfo = win.GetControlPropertyBinds(BindProperty.Name);
        //    //            //if (bindInfo == null)
        //    //            //{
        //    //            //    bindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            //    win.AddControlPropertyBind(BindProperty.Name, bindInfo);
        //    //            //}

        //    //            //bindInfo.SourceObject = targetControl;
        //    //            //bindInfo.SourcePropertyInfo = propertyInfo;
        //    //            //bindInfo.BindingMode = bindMode;
        //    //            mHostBindInfo.SourceObject = targetControl;
        //    //            mHostBindInfo.SourcePropertyInfo = propertyInfo;
        //    //            mHostBindInfo.BindingMode = bindMode;
        //    //        }
        //    //        break;

        //    //    case UISystem.Bind.enBindingMode.TwoWay:
        //    //        {
        //    //            //var tgBindInfo = targetControl.GetControlPropertyBinds(propertyInfo.Name);
        //    //            //if (tgBindInfo == null)
        //    //            //{
        //    //            //    tgBindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            //    targetControl.AddControlPropertyBind(propertyInfo.Name, tgBindInfo);
        //    //            //}
        //    //            //tgBindInfo.SourceObject = win;
        //    //            //tgBindInfo.SourcePropertyInfo = win.GetType().GetProperty(BindProperty.Name);
        //    //            //tgBindInfo.BindingMode = bindMode;

        //    //            //var bindInfo = win.GetControlPropertyBinds(BindProperty.Name);
        //    //            //if (bindInfo == null)
        //    //            //{
        //    //            //    bindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            //    win.AddControlPropertyBind(BindProperty.Name, bindInfo);
        //    //            //}
        //    //            //bindInfo.SourceObject = targetControl;
        //    //            //bindInfo.SourcePropertyInfo = propertyInfo;
        //    //            //bindInfo.BindingMode = bindMode;
        //    //            UISystem.Bind.ControlPropertyBindInfo bindInfo = new UISystem.Bind.ControlPropertyBindInfo();
        //    //            bindInfo.SourceObject = win;
        //    //            bindInfo.SourcePropertyInfo = win.GetType().GetProperty(BindProperty.Name);
        //    //            bindInfo.BindingMode = UISystem.Bind.enBindingMode.OneWayToSource;
        //    //            targetControl.AddControlPropertyBind(propertyInfo.Name, bindInfo);

        //    //            mHostBindInfo.SourceObject = targetControl;
        //    //            mHostBindInfo.SourcePropertyInfo = propertyInfo;
        //    //            mHostBindInfo.BindingMode = bindMode;
        //    //        }
        //    //        break;
        //    //}

        //    //Binded = true;
        //}

        //private void ComboBox_BindMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    UISystem.WinBase win = BindInstance as UISystem.WinBase;
        //    if (win == null)
        //        return;

        //    //var bindInfo = win.GetControlPropertyBinds(BindProperty.Name);
        //    //if (bindInfo == null)
        //    //    return;

        //    //bindInfo.BindingMode = (UISystem.Bind.enBindingMode)(System.Enum.Parse(typeof(UISystem.Bind.enBindingMode), ComboBox_BindMode.SelectedItem.ToString()));
        //    mHostBindInfo.BindingMode = (UISystem.Bind.enBindingMode)(System.Enum.Parse(typeof(UISystem.Bind.enBindingMode), ComboBox_BindMode.SelectedItem.ToString()));
        //}
    }
}
