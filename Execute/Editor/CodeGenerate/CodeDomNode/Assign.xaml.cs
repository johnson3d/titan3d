using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Assign.xaml
    /// </summary>
    //[CodeGenerateSystem.ShowInNodeList("逻辑/赋值", "赋值操作节点")]
    public partial class Assign : CodeGenerateSystem.Base.IDebugableNode
    {
        bool mBreaked = false;
        public bool Breaked
        {
            get { return mBreaked; }
            set
            {
                if (mBreaked == value)
                    return;
                mBreaked = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    BreakedPinShow = mBreaked;
                    ChangeParentLogicLinkLine(mBreaked);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }
        public void ChangeParentLogicLinkLine(bool change)
        {
            ChangeParentLogicLinkLine(change, MethodLink_Pre);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodLink_Pre);
        }
        public bool CanBreak()
        {
            return true;
        }

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlSetValueElement = SetValueElement;
            mCtrlValueElement = ValueElement;
            mCtrlMethodLink_Next = MethodLink_Next;

            SetUpLinkElement(MethodLink_Pre);
        }

        protected override void CollectionErrorMsg()
        {
            if(MethodLink_Pre.HasLink)
            {
                if(!SetValueElement.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "未设置输入值";
                }
                if (!ValueElement.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "未设置输出值";
                }
            }
        }
    }
}
