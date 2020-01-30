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
    public sealed partial class ReturnCustom : CodeGenerateSystem.Base.IDebugableNode
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
            this.InitializeComponent();

            mCtrlMethodLink = MethodLink;
            mParamsPanel = StackPanel_Values;
            SetUpLinkElement(MethodLink);
        }

        protected override void CollectionErrorMsg()
        {
            if (!MethodLink.HasLink)
                return;

            Type tempClassType = null;
            if(mTemplateClassInstance != null)
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

        partial void ResetMethodInfo_WPF(CodeDomNode.CustomMethodInfo methodInfo)
        {
        }

        //public bool ShowProperty = true;
        public override object GetShowPropertyObject()
        {
            var csParam = CSParam as ReturnCustomConstructParam;
            switch(csParam.ShowPropertyType)
            {
                case ReturnCustomConstructParam.enShowPropertyType.MethodInfo:
                    return ((ReturnCustomConstructParam)CSParam).MethodInfo;
                case ReturnCustomConstructParam.enShowPropertyType.ReturnValue:
                    return mTemplateClassInstance;
                case ReturnCustomConstructParam.enShowPropertyType.None:
                    return null;
            }
            return null;
        }

        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys)
        {
            if(mTemplateClassInstance != null)
            {
                var classType = mTemplateClassInstance.GetType();
                foreach(var childNode in mChildNodes)
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

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ReturnCustom;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            foreach(var child in copyedNode.mChildNodes)
            {
                var childParameterCtrl = child as MethodInvokeParameterControl;
                if(childParameterCtrl != null)
                {
                    childParameterCtrl.OnUpdateParamTypeAction = copyedNode.OnUpdateChildParamType;
                }
            }
            copyedNode.InitTemplateClass_WPF(null);
            return copyedNode;
        }
    }
}
