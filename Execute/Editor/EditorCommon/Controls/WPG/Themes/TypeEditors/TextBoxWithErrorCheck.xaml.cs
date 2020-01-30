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

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for TextBoxWithErrorCheck.xaml
    /// </summary>
    public partial class TextBoxWithErrorCheck : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(TextBoxWithErrorCheck), new UIPropertyMetadata(null));

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(TextBoxWithErrorCheck), new UIPropertyMetadata(null));

        public object BindValue
        {
            get { return GetValue(BindValueProperty); }
            set { SetValue(BindValueProperty, value); }
        }
        public static readonly DependencyProperty BindValueProperty = DependencyProperty.Register("BindValue", typeof(object), typeof(TextBoxWithErrorCheck));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextBoxWithErrorCheck), new FrameworkPropertyMetadata(false));


        public TextBoxWithErrorCheck()
        {
            InitializeComponent();
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            var icc = BindInstance as InputWindow.IInputErrorCheckClass;
            if (TextBox_Value != null && icc != null)
            {
                var bindExp = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                        if (rule != null)
                            rule.OnValidateCheck = icc.IsInputValidate;
                    }
                }
            }
        }
    }
}
