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

namespace EditorCommon.Bind
{
    /// <summary>
    /// VariableBind.xaml 的交互逻辑
    /// </summary>
    public partial class VariableBind : UserControl
    {
        public string UIElementName
        {
            get { return (string)GetValue(UIElementNameProperty); }
            set { SetValue(UIElementNameProperty, value); }
        }
        public static readonly DependencyProperty UIElementNameProperty = DependencyProperty.Register("UIElementName", typeof(string), typeof(VariableBind), new FrameworkPropertyMetadata(""));
        public string VariableName
        {
            get { return (string)GetValue(VariableNameProperty); }
            set { SetValue(VariableNameProperty, value); }
        }
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(VariableBind), new FrameworkPropertyMetadata(""));

        public EngineNS.UISystem.enBindMode BindMode
        {
            get { return (EngineNS.UISystem.enBindMode)GetValue(BindModeProperty); }
            set { SetValue(BindModeProperty, value); }
        }
        public static readonly DependencyProperty BindModeProperty = DependencyProperty.Register("BindMode", typeof(EngineNS.UISystem.enBindMode), typeof(VariableBind), new FrameworkPropertyMetadata(EngineNS.UISystem.enBindMode.OnWay));

        EngineNS.UISystem.VariableBindInfo mBindInfo;
        public EngineNS.UISystem.VariableBindInfo BindInfo => mBindInfo;
        EngineNS.UISystem.UIElement mBindUIElement;
        Type mPropertyType;
        BindButton mHostControl;
        public VariableBind(BindButton hostCtrl, EngineNS.UISystem.UIElement uiElement, EngineNS.UISystem.VariableBindInfo bindInfo, Type proType)
        {
            InitializeComponent();

            mHostControl = hostCtrl;
            mBindUIElement = uiElement;
            mBindInfo = bindInfo;
            mPropertyType = proType;
            VariableName = mBindInfo.VariableName;
            BindMode = mBindInfo.BindMode;
            BindingOperations.SetBinding(this, BindModeProperty, new Binding("BindMode") { Source = mBindInfo, Mode = BindingMode.TwoWay });

            UIElementName = uiElement.Initializer.Name;
            BindingOperations.SetBinding(this, UIElementNameProperty, new Binding("Name") { Source = uiElement.Initializer });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_FindCustomBind_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mBindUIElement.VariableBindFindAction(mBindInfo);
        }
        private void Button_DelCustomBind_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (EditorCommon.MessageBox.Show($"即将删除参数绑定，删除后不可恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                return;

            mBindUIElement.VariableBindRemoveAction?.Invoke(mBindInfo);
            mHostControl.ListBox_VariableBind.Items.Remove(this);
            mHostControl.UpdateBindShow(mBindUIElement);
        }
    }
}
