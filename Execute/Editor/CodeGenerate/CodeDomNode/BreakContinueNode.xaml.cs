using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

// “用户控件”项模板在 http://go.microsoft.com/fwlink/?LinkId=234236 上有介绍

namespace CodeDomNode
{
    public partial class BreakContinueNode
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlMethod_Pre = Method_Pre;

            NodeName = CSParam.ConstructParam;
            mValueStr = CSParam.ConstructParam;
        }

    }
}
