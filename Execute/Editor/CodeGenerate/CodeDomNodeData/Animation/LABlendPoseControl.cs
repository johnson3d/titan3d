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
    public class LABlendPoseControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Weight { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "BlendPose";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LABlendPoseControlConstructionParams;

            retVal.Weight = Weight;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LABlendPoseControlConstructionParams))]
    public partial class LABlendPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {

        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        #region DP

        public float Weight
        {
            get { return (float)GetValue(WeightProperty); }
            set
            {
                SetValue(WeightProperty, value);
                var para = CSParam as LABlendPoseControlConstructionParams;
                para.Weight = value;
            }
        }
        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register("Weight", typeof(float), typeof(LABlendPoseControl), new UIPropertyMetadata(0.0f, OnWeightPropertyyChanged));
        private static void OnWeightPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LABlendPoseControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Weight = (float)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Weight", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Weight", LABlendPoseControl.WeightProperty, Weight);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LABlendPoseControl(LABlendPoseControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Weight = csParam.Weight;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }
        #region AddDeleteValueLink
        private void WeightValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            WeightValueTextBlock.Visibility = Visibility.Visible;
        }

        private void WeightValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            WeightValueTextBlock.Visibility = Visibility.Collapsed;
        }
        #endregion AddDeleteValueLink
        #region InitializeLinkControl
        CodeGenerateSystem.Base.LinkPinControl mSrcPoseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mDescPoseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mWeightValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LABlendPoseControlConstructionParams csParam)
        {
            mSrcPoseLinkHandle = SrcPoseHandle;
            mDescPoseLinkHandle = DescPoseHandle;
            mWeightValueLinkHandle = WeightValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mSrcPoseLinkHandle.MultiLink = false;
            mDescPoseLinkHandle.MultiLink = false;
            mWeightValueLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mSrcPoseLinkHandle.NameStringVisible = Visibility.Visible;
            mSrcPoseLinkHandle.NameString = "SourceePose";
            mDescPoseLinkHandle.NameStringVisible = Visibility.Visible;
            mDescPoseLinkHandle.NameString = "DestinationPose";
            //mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            //mAdditiveLinkHandle.NameString = "AdditivePose";
            mWeightValueLinkHandle.NameStringVisible = Visibility.Visible;
            mWeightValueLinkHandle.NameString = "Weight";
            WeightValueTextBlock.Visibility = Visibility.Visible;
            mWeightValueLinkHandle.OnAddLinkInfo += WeightValueLinkHandle_OnAddLinkInfo;
            mWeightValueLinkHandle.OnDelLinkInfo += WeightValueLinkHandle_OnDelLinkInfo;
            AddLinkPinInfo("SrcPoseLinkHandle", mSrcPoseLinkHandle, null);
            AddLinkPinInfo("DescPoseLinkHandle", mDescPoseLinkHandle, null);
            AddLinkPinInfo("WeightValueLinkHandle", mWeightValueLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "SrcPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "DescPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "WeightValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LABlendPoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAClipNodeControl);
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
            var srcLinkObj = mSrcPoseLinkHandle.GetLinkedObject(0, true);
            var srcLinkElm = mSrcPoseLinkHandle.GetLinkedPinControl(0, true);
            if (srcLinkObj != null)
                await srcLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, srcLinkElm, context);
            var desLinkObj = mDescPoseLinkHandle.GetLinkedObject(0, true);
            var desLinkElm = mDescPoseLinkHandle.GetLinkedPinControl(0, true);
            if (desLinkObj != null)
                await desLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, desLinkElm, context);

            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_BlendPose);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);
            if (srcLinkObj != null)
            {
                var srcAssign = new CodeAssignStatement();
                srcAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "SourceNode");
                srcAssign.Right = srcLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(srcAssign);
            }
            if (desLinkObj != null)
            {
                var desAssign = new CodeAssignStatement();
                desAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "DestinationNode");
                desAssign.Right = desLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(desAssign);
            }
            await GenerateValueEvaluateCode(codeClass, mWeightValueLinkHandle, ValidName + "_WeightEvaluate", "EvaluateWeight", codeStatementCollection, context);
            return;
        }
        async System.Threading.Tasks.Task GenerateValueEvaluateCode(CodeTypeDeclaration codeClass, LinkPinControl linkHandle, string methodName, string assignFuncName, CodeStatementCollection codeStatementCollection, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(typeof(float));
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);

            CodeExpression valueExpression = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, Weight);

            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(valueExpression));
            codeClass.Members.Add(valueEvaluateMethod);
            var weightAssign = new CodeAssignStatement();
            weightAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), assignFuncName);
            weightAssign.Right = new CodeVariableReferenceExpression(valueEvaluateMethod.Name);
            codeStatementCollection.Add(weightAssign);
        }
    }
}
