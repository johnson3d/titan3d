using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using System.Windows.Data;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public partial class ExpandNodeChild
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();

            var param = CSParam as ExpandNodeChildConstructionParams;
            TextBlock_Name.Text = param.ParamName + "(" + EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ParamType) + ")";

            mCtrlValue_In = ValueIn;
            mCtrlValue_Out = ValueOut;

            if(param.IsReadOnly)
            {
                ValueIn.Visibility = System.Windows.Visibility.Hidden;
            }

            BindingOperations.SetBinding(CheckBox_EnableSet, CheckBox.IsCheckedProperty, new Binding("EnableSet") { Source = param, Mode=BindingMode.TwoWay });
        }
    }
}
