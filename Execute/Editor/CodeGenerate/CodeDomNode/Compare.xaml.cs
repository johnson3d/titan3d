using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Compare.xaml
    /// </summary>
    public partial class Compare
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlParamLink_1 = ParamLink_1;
            mCtrlParamLink_2 = ParamLink_2;
            mCtrlresultHandle = resultHandle;
        }
        
    }
}
