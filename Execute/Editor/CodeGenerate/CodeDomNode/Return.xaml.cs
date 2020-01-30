using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Return.xaml
    /// </summary>
    //[CodeGenerateSystem.ShowInNodeList("逻辑.return", "函数返回值节点，设置脚本的返回值")]
    public partial class Return : CodeGenerateSystem.Base.IDebugableNode
    {
        #region IDebugableNode

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
            ChangeParentLogicLinkLine(change, MethodLink);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodLink);
        }
        public bool CanBreak()
        {
            return true;
        }

        #endregion
        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodLink = MethodLink;
            mParamsPanel = StackPanel_Values;
            SetUpLinkElement(MethodLink);
        }

        protected override void CollectionErrorMsg()
        {
            if (!MethodLink.HasLink)
                return;

            Type tempClassType = null;
            if (mTemplateClassInstance != null)
            {
                tempClassType = mTemplateClassInstance.GetType();
            }
            foreach (var child in mChildNodes)
            {
                if (child is MethodInvokeParameterControl)
                {
                    var pm = child as MethodInvokeParameterControl;
                    var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    if (!pm.HasLink() && pm.ParamFlag == System.CodeDom.FieldDirection.Out)
                    {
                        if (tempClassType != null)
                        {
                            var pro = tempClassType.GetProperty(pmParam.ParamInfo.ParamName);
                            if (pro != null)
                                continue;
                        }
                        HasError = true;
                        var param = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        ErrorDescription = $"{param.ParamInfo.ParamName}没有链接";
                        return;
                    }
                }
                else
                    throw new InvalidOperationException();
            }
        }

        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            if (mTemplateClassInstance != null)
            {
                var classType = mTemplateClassInstance.GetType();
                foreach (var childNode in mChildNodes)
                {
                    var metCtrl = childNode as MethodInvokeParameterControl;
                    if (metCtrl == null || metCtrl.IsConstructOut)
                        continue;

                    var ctrlParam = metCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    BindingOperations.ClearBinding(metCtrl.DefaultValueTextBlock, TextBlock.TextProperty);
                    if (classType.GetProperty(ctrlParam.ParamInfo.ParamName) != null)
                        metCtrl.DefaultValueTextBlock.SetBinding(TextBlock.TextProperty, new Binding(ctrlParam.ParamInfo.ParamName) { Source = mTemplateClassInstance });
                }
            }
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as Return;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            copyedNode.InitTemplateClass_WPF(null);
            return copyedNode;
        }
    }
}
