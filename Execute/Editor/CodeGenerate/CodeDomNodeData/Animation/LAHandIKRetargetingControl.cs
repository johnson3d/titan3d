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
    public class LAHandIKRetargetingControlConstructionParams : SkeletonConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Alpha { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "HandIKRetargeting";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAHandIKRetargetingControlConstructionParams;

            retVal.Alpha = Alpha;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAHandIKRetargetingControlConstructionParams))]
    public partial class LAHandIKRetargetingControl : CodeGenerateSystem.Base.BaseNodeControl
    {
     
        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        #region DP

        public float Alpha
        {
            get { return (float)GetValue(AlphaProperty); }
            set
            {
                SetValue(AlphaProperty, value);
                var para = CSParam as LAHandIKRetargetingControlConstructionParams;
                para.Alpha = value;
            }
        }
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(float), typeof(LAHandIKRetargetingControl), new UIPropertyMetadata(1.0f, OnAlphaPropertyyChanged));
        private static void OnAlphaPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAHandIKRetargetingControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Alpha = (float)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Alpha", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Alpha", LAHandIKRetargetingControl.AlphaProperty, Alpha);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LAHandIKRetargetingControl(LAHandIKRetargetingControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Alpha = csParam.Alpha;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }
        #region AddDeleteValueLink
        private void ActiveValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Collapsed;
        }
        private void ActiveValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Visible;
        }


        private void AlphaValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            AlphaValueTextBlock.Visibility = Visibility.Visible;
        }

        private void AlphaValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            AlphaValueTextBlock.Visibility = Visibility.Collapsed;
        }
        #endregion AddDeleteValueLink
        #region InitializeLinkControl
        CodeGenerateSystem.Base.LinkPinControl mActiveValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mInPoseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mAlphaValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LAHandIKRetargetingControlConstructionParams csParam)
        {
            mActiveValueLinkHandle = ActiveValueHandle;
            mInPoseLinkHandle = InPoseHandle;
            mAlphaValueLinkHandle = AlphaValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mActiveValueLinkHandle.MultiLink = false;
            mInPoseLinkHandle.MultiLink = false;
            mAlphaValueLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mActiveValueLinkHandle.NameStringVisible = Visibility.Visible;
            mActiveValueLinkHandle.NameString = "EffectorLocation";
            mActiveValueLinkHandle.OnAddLinkInfo += ActiveValueLinkHandle_OnAddLinkInfo;
            mActiveValueLinkHandle.OnDelLinkInfo += ActiveValueLinkHandle_OnDelLinkInfo;

            mInPoseLinkHandle.NameStringVisible = Visibility.Visible;
            mInPoseLinkHandle.NameString = "Pose";
            //mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            //mAdditiveLinkHandle.NameString = "AdditivePose";
            mAlphaValueLinkHandle.NameStringVisible = Visibility.Visible;
            mAlphaValueLinkHandle.NameString = "Alpha";
            AlphaValueTextBlock.Visibility = Visibility.Visible;
            mAlphaValueLinkHandle.OnAddLinkInfo += AlphaValueLinkHandle_OnAddLinkInfo;
            mAlphaValueLinkHandle.OnDelLinkInfo += AlphaValueLinkHandle_OnDelLinkInfo;
            AddLinkPinInfo("ActiveValueLinkHandle", mActiveValueLinkHandle, null);
            AddLinkPinInfo("InPoseLinkHandle", mInPoseLinkHandle, null);
            AddLinkPinInfo("AlphaValueLinkHandle", mAlphaValueLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ActiveValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Float3, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "InPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AlphaValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAHandIKRetargetingControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAHandIKRetargetingControl);
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var inPoseLinkObj = mInPoseLinkHandle.GetLinkedObject(0, true);
            var inPoseLinkElm = mInPoseLinkHandle.GetLinkedPinControl(0, true);
            await inPoseLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, inPoseLinkElm, context);
        }
    }
}
