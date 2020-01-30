using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Value_Null.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("数值/null", "空数值节点，设置参数为空")]
    public partial class Value_Null
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
    }
}
