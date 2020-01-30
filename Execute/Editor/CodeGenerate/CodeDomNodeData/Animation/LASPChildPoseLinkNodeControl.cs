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
    public class LASPChildPoseLinkNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string IndexValue { get; set; } = "";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LASPChildPoseLinkNodeControlConstructionParams;

            retVal.IndexValue = IndexValue;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LASPChildPoseLinkNodeControlConstructionParams))]
    public partial class LASPChildPoseLinkNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();

        #region DP

        public string IndexValue
        {
            get { return (string)GetValue(IndexValueProperty); }
            set
            {
                SetValue(IndexValueProperty, value);
                var para = CSParam as LASPChildPoseLinkNodeControlConstructionParams;
                para.IndexValue = value;
            }
        }
        public int Index
        {
            get { return int.Parse(IndexValue); }
        }
        public static readonly DependencyProperty IndexValueProperty = DependencyProperty.Register("IndexValue", typeof(string), typeof(LASPChildPoseLinkNodeControl), new UIPropertyMetadata("", OnChildValuePropertyChanged));
        private static void OnChildValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASPChildPoseLinkNodeControl;
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

        public LASPChildPoseLinkNodeControl(LASPChildPoseLinkNodeControlConstructionParams csParam)
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

        void InitializeLinkControl(LASPChildPoseLinkNodeControlConstructionParams csParam)
        {
            mLinkInHandle = ValueIn;

            AddLinkPinInfo("LASPChildPoseLinkInHandle", mLinkInHandle, null);

        }



        #endregion InitializeLinkControl
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "LASPChildPoseLinkInHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LASPChildPoseLinkNodeControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LASPChildPoseLinkNodeControl);
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
