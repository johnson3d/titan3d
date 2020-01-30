using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    //[CodeGenerateSystem.ShowInNodeList("逻辑/base", "获取基类型实例")]
    public sealed partial class CenterDataValueControl
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();
            mCtrlValue = ValueElement;
        }
    }
}
