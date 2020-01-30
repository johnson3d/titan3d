using CodeGenerateSystem.Base;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    public class LASPChildBlendTimeNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string IndexValue { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public float BlendTimeValue { get; set; } = 0.1f;

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LASPChildBlendTimeNodeControlConstructionParams;

            retVal.IndexValue = IndexValue;
            retVal.BlendTimeValue = BlendTimeValue;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LASPChildBlendTimeNodeControlConstructionParams))]
    public partial class LASPChildBlendTimeNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();

        #region DP
        public float BlendTimeValue
        {
            get { return (float)GetValue(BlendTimeValueProperty); }
            set
            {
                SetValue(BlendTimeValueProperty, value);
                var para = CSParam as LASPChildBlendTimeNodeControlConstructionParams;
                para.BlendTimeValue = value;
            }
        }
        public static readonly DependencyProperty BlendTimeValueProperty = DependencyProperty.Register("BlendTimeValue", typeof(float), typeof(LASPChildBlendTimeNodeControl), new UIPropertyMetadata(0.1f, OnBlendTimeValuePropertyChanged));
        private static void OnBlendTimeValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASPChildBlendTimeNodeControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.BlendTimeValue = (float)e.NewValue;
        }
        public string IndexValue
        {
            get { return (string)GetValue(IndexValueProperty); }
            set
            {
                SetValue(IndexValueProperty, value);
                var para = CSParam as LASPChildBlendTimeNodeControlConstructionParams;
                para.IndexValue = value;
            }
        }
        public static readonly DependencyProperty IndexValueProperty = DependencyProperty.Register("IndexValue", typeof(string), typeof(LASPChildBlendTimeNodeControl), new UIPropertyMetadata("", OnIndexValuePropertyChanged));
        private static void OnIndexValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASPChildBlendTimeNodeControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.IndexValue = (string)e.NewValue;
        }

        #endregion

        #region ShowProperty
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Alpha", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            //CreateBinding(mTemplateClassInstance, "Alpha", LAAdditivePoseControl.AlphaProperty, Alpha);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LASPChildBlendTimeNodeControl(LASPChildBlendTimeNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IndexValue = csParam.IndexValue;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }

        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        #region InitializeLinkControl
        LinkPinControl mLinkInHandle = new CodeGenerateSystem.Base.LinkPinControl();

        void InitializeLinkControl(LASPChildBlendTimeNodeControlConstructionParams csParam)
        {
            mLinkInHandle = ValueIn;
            mLinkInHandle.OnAddLinkInfo += LinkInHandle_OnAddLinkInfo;
            mLinkInHandle.OnDelLinkInfo += LinkInHandle_OnDelLinkInfo;
            AddLinkPinInfo("LASPBlendTimeLinkInHandle", mLinkInHandle, null);

        }
        private void LinkInHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            BlendTimeValueTextBlock.Visibility = Visibility.Collapsed;
        }
        private void LinkInHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            BlendTimeValueTextBlock.Visibility = Visibility.Visible;
        }
        #endregion InitializeLinkControl
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "LASPBlendTimeLinkInHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LASPChildBlendTimeNodeControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LASPChildBlendTimeNodeControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var validName = StringRegex.GetValidName(NodeName);
            return new CodeVariableReferenceExpression(validName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return;
        }
    }
}
