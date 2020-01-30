using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for TypeCastControl.xaml
    /// </summary>
    public partial class TypeCastControl
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlClassLinkHandle_In = ClassLinkHandle_In;
            mCtrlClassLinkHandle_Out = ClassLinkHandle_Out;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as TypeCastControl;
            copyedNode.TargetTypeName = TargetTypeName;
            return copyedNode;
        }
    }
}
