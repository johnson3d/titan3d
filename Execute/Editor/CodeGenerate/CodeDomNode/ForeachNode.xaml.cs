using System;
using System.CodeDom;
using System.Collections;
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
    [CodeGenerateSystem.ShowInNodeList("逻辑/循环遍历(foreach)", "循环遍历节点")]
    public partial class ForeachNode
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlMethodLink_Completed = MethodLink_Complete;
            mCtrlMethodLink_LoopBody = MethodLink_LoopBody;
            mCtrlArrayElement = ArrayElement;
            mCtrlArrayIndex = ArrayIndex;
            mCtrlArrayIn = ArrayIn;
            mCtrlArrayCount = ArrayCount;

            mCtrlDicKey = DicKey;
            mCtrlDicValue = DicValue;

            SetUpLinkElement(MethodLink_Pre);
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ForeachNode;
            copyedNode.CountDefaultValue = mCountDefaultValue;
            return copyedNode;
        }
    }
}
