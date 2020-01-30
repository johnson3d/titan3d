using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CodeGenerateSystem.Base;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("逻辑/遍历(for)", "循环遍历节点")]
    public sealed partial class ForLoopNode
    {
        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlMethodLink_Completed = MethodLink_Complete;
            mCtrlMethodLink_LoopBody = MethodLink_LoopBody;
            mCtrlIndexFirst = IndexFirst;
            mCtrlIndexLast = IndexLast;
            mCtrlIndex = IndexPin;

            SetUpLinkElement(MethodLink_Pre);
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ForLoopNode;
            copyedNode.FirstIndex = mFirstIndex;
            copyedNode.LastIndex = mLastIndex;
            return copyedNode;
        }
    }
}
