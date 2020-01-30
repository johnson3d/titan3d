using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    public partial class MethodCustomInvoke : CodeGenerateSystem.Base.IDebugableNode
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
        public bool CanBreak()
        {
            return true;
        }

        public void ChangeParentLogicLinkLine(bool change)
        {
        }

        System.Windows.Visibility mAsyncIconVisibility = System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility AsyncIconVisibility
        {
            get => mAsyncIconVisibility;
            set
            {
                mAsyncIconVisibility = value;
                OnPropertyChanged("AsyncIconVisibility");
            }
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();

            var param = CSParam as MethodCustomInvokeConstructParam;
            ResetMethodInfo_WPF(param.MethodInfo);

            mCtrlMethodPin_Pre = MethodPin_Pre;
            mCtrlMethodPin_Next = MethodPin_Next;
            mParamsPanel = StackPanel_InParams;
            mValuesPanel = StackPanel_OutValues;
        }

        partial void ResetMethodInfo_WPF(CustomMethodInfo methodInfo)
        {
            NodeName = methodInfo.MethodName;
            if (string.IsNullOrEmpty(methodInfo.DisplayName) == false)
            {
                ShowNodeName = methodInfo.DisplayName;
            }
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

            if(methodInfo.IsAsync)
            {
                AsyncIconVisibility = System.Windows.Visibility.Visible;
            }
        }

        public bool ShowProperty = true;
        public override object GetShowPropertyObject()
        {
            if (ShowProperty)
                return mTemplateClassInstance;// ((MethodCustomInvoke.MethodCustomInvokeConstructParam)CSParam).MethodInfo;
            else
                return null;
        }

        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            if (mTemplateClassInstance != null)
            {
                var classType = mTemplateClassInstance.GetType();
                foreach (var childNode in mChildNodes)
                {
                    var metCtrl = childNode as MethodInvokeParameterControl;
                    // 输出参数不需要显示默认值
                    if (metCtrl == null || metCtrl.IsConstructOut)
                        continue;

                    var ctrlParam = metCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    BindingOperations.ClearBinding(metCtrl.DefaultValueTextBlock, TextBlock.TextProperty);
                    if (classType.GetProperty(ctrlParam.ParamInfo.ParamName) != null)
                        metCtrl.DefaultValueTextBlock.SetBinding(TextBlock.TextProperty, new Binding(ctrlParam.ParamInfo.ParamName) { Source = mTemplateClassInstance });
                }

                mTemplateClassInstance.OnPropertyChangedAction = (propertyName, newVal, oldVal) =>
                {
                    mParamDeclarationStatementsDic.Clear();
                    mParamDeclarationInitStatementsDic.Clear();
                };
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as MethodCustomInvoke;
            PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            return copyedNode;
        }
    }
}
