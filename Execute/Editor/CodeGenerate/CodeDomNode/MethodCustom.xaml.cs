using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Data;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public sealed partial class MethodCustom : CodeGenerateSystem.Base.IDebugableNode
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
        }
        public override void Tick(long elapsedMillisecond)
        {
        }
        public bool CanBreak()
        {
            return true;
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();

            var param = CSParam as MethodCustomConstructParam;
            ResetMethodInfo_WPF(param.MethodInfo);

            mCtrlMethodPin_Next = MethodPin_Next;
            mParamsPanel = StackPanel_Params;
            mShowProperty = param.IsShowProperty;
        }

        partial void ResetMethodInfo_WPF(CodeDomNode.CustomMethodInfo methodInfo)
        {
            NodeName = methodInfo.MethodName;
            ShowNodeName = methodInfo.DisplayName;
            methodInfo.PropertyChanged += (sender, e) =>
            {
                switch(e.PropertyName)
                {
                    case "DisplayName":
                        ShowNodeName = methodInfo.DisplayName;
                        break;
                }
            };
            SetBinding(NodeNameBinderProperty, new Binding("MethodName") { Source = methodInfo, Mode = BindingMode.TwoWay });
        }

        bool mShowProperty = true;
        public override object GetShowPropertyObject()
        {
            if (mShowProperty)
                return ((MethodCustomConstructParam)CSParam).MethodInfo;
            else
                return null;
        }

        public override bool CanDuplicate()
        {
            return false;
        }
    }
}
