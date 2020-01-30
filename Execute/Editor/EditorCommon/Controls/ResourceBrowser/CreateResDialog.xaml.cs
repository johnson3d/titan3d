using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EditorCommon.Resources;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// EditorLoading.xaml 的交互逻辑
    /// </summary>
    public partial class CreateResDialog : ResourceLibrary.WindowBase, EditorCommon.Resources.ICustomCreateDialog
    {
        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }

        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateResDialog), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(CreateResDialog), new FrameworkPropertyMetadata(""));

        public bool OKButtonEnable
        {
            get { return (bool)GetValue(OKButtonEnableProperty); }
            set { SetValue(OKButtonEnableProperty, value); }
        }

        public static readonly DependencyProperty OKButtonEnableProperty =
            DependencyProperty.Register("OKButtonEnable", typeof(bool), typeof(CreateResDialog), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnOKButtonEnableChanged)));
        public static void OnOKButtonEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public EditorCommon.Resources.IResourceCreateData ResCreateData
        {
            get { return (EditorCommon.Resources.IResourceCreateData)GetValue(ResCreateDataProperty); }
            set { SetValue(ResCreateDataProperty, value); }
        }
        public static readonly DependencyProperty ResCreateDataProperty =
            DependencyProperty.Register("ResCreateData", typeof(EditorCommon.Resources.IResourceCreateData), typeof(CreateResDialog), new FrameworkPropertyMetadata(OnResCreateDataChanged));
        public static void OnResCreateDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CreateResDialog;
            ctrl.SetBinding(ResourceNameProperty, new Binding("ResourceName") { Source = e.NewValue, Mode=BindingMode.TwoWay });
        }


        public CreateResDialog()
        {
            InitializeComponent();
        }

        InputWindow.Delegate_ValidateCheck mOnValidateCheck = null;
        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (TextBox_Value != null)
            {
                var bindExp = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = mOnValidateCheck;
                        }
                    }
                }
            }
        }

        public IResourceCreateData GetCreateData()
        {
            ResCreateData.Description = Description;
            return ResCreateData;
        }

        public bool? ShowDialog(InputWindow.Delegate_ValidateCheck onCheck)
        {
            try
            {
                mOnValidateCheck = onCheck;
                return this.ShowDialog();
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            }

            return false;
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
