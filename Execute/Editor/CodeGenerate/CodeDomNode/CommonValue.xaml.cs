using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for CommonValue.xaml
    /// </summary>
    /// // 简单类型的参数（只有一个值，非集合，如整形、字符串等）
    public partial class CommonValue
    {
        public string ValueText
        {
            get
            {
                var param = CSParam as CommonValueConstructionParams;
                if(param != null)
                    return param.Value;
                return "";
            }
            set
            {
                var param = CSParam as CommonValueConstructionParams;
                if(param != null)
                    param.Value = value;
                OnPropertyChanged("ValueText");
            }
        }

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlValueLinkHandle = ValueLinkHandle;
            ValueGrid.DataContext = this;
        }

        protected bool IsNumberic(string message)
        {
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
            if (rex.IsMatch(message))
                return true;
            else
                return false;
        }

        private void baseNodeControl_Loaded(object sender, RoutedEventArgs e)
        {
            var bindExp = ValueTextBox.GetBindingExpression(TextBox.TextProperty);
            if (bindExp != null)
            {
                if (bindExp.ParentBinding.ValidationRules.Count > 0)
                {
                    var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                    if (rule != null)
                        rule.OnValidateCheck = (object value, CultureInfo cultureInfo) =>
                        {
                            var param = CSParam as CommonValueConstructionParams;
                            if (param == null)
                                return new ValidationResult(false, "参数类型不合法");

                            try
                            {
                                System.Convert.ChangeType(value, param.ValueType);
                                return ValidationResult.ValidResult;
                            }
                            catch (System.Exception)
                            {
                                return new ValidationResult(false, "数值不合法");
                            }
                        };
                }
            }
        }
    }
}
