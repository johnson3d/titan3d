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
    public class LAAdditivePoseControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Alpha { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "ApplyAdditivePose";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAAdditivePoseControlConstructionParams;

            retVal.Alpha = Alpha;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAAdditivePoseControlConstructionParams))]
    public partial class LAAdditivePoseControl : CodeGenerateSystem.Base.BaseNodeControl
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
                var para = CSParam as LAAdditivePoseControlConstructionParams;
                para.Alpha = value;
            }
        }
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(float), typeof(LAAdditivePoseControl), new UIPropertyMetadata(1.0f, OnAlphaPropertyyChanged));
        private static void OnAlphaPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAAdditivePoseControl;
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


            CreateBinding(mTemplateClassInstance, "Alpha", LAAdditivePoseControl.AlphaProperty, Alpha);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LAAdditivePoseControl(LAAdditivePoseControlConstructionParams csParam)
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
        CodeGenerateSystem.Base.LinkPinControl mBaseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mAdditiveLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mAlphaValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LAAdditivePoseControlConstructionParams csParam)
        {
            mBaseLinkHandle = BasePoseHandle;
            mAdditiveLinkHandle = AdditivePoseHandle;
            mAlphaValueLinkHandle = AlphaValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mBaseLinkHandle.MultiLink = false;
            mAdditiveLinkHandle.MultiLink = false;
            mAlphaValueLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mBaseLinkHandle.NameStringVisible = Visibility.Visible;
            mBaseLinkHandle.NameString = "BasePose";
            mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            mAdditiveLinkHandle.NameString = "AdditivePose";
            //mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            //mAdditiveLinkHandle.NameString = "AdditivePose";
            mAlphaValueLinkHandle.NameStringVisible = Visibility.Visible;
            mAlphaValueLinkHandle.NameString = "Alpha";
            AlphaValueTextBlock.Visibility = Visibility.Visible;
            mAlphaValueLinkHandle.OnAddLinkInfo += AlphaValueLinkHandle_OnAddLinkInfo;
            mAlphaValueLinkHandle.OnDelLinkInfo += AlphaValueLinkHandle_OnDelLinkInfo;
            AddLinkPinInfo("BaseLinkHandle", mBaseLinkHandle, null);
            AddLinkPinInfo("AdditiveLinkHandle", mAdditiveLinkHandle, null);
            AddLinkPinInfo("AlphaValueLinkHandle", mAlphaValueLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "BaseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AdditiveLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AlphaValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAAdditivePoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAAdditivePoseControl);
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
            var baseLinkObj = mBaseLinkHandle.GetLinkedObject(0, true);
            var baseLinkElm = mBaseLinkHandle.GetLinkedPinControl(0, true);
            if (baseLinkObj != null)
                await baseLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, baseLinkElm, context);
            var additiveLinkObj = mAdditiveLinkHandle.GetLinkedObject(0, true);
            var additiveLinkElm = mAdditiveLinkHandle.GetLinkedPinControl(0, true);
            if (additiveLinkObj != null)
                await additiveLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, additiveLinkElm, context);

            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_AdditivePose);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);
            if (baseLinkObj != null)
            {
                var baseAssign = new CodeAssignStatement();
                baseAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "BaseNode");
                baseAssign.Right = baseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(baseAssign);
            }
            if (additiveLinkObj != null)
            {
                var additiveAssign = new CodeAssignStatement();
                additiveAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "AdditiveNode");
                additiveAssign.Right = additiveLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(additiveAssign);
            }

            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = ValidName + "_AlphaEvaluate";
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(typeof(float));
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            CodeExpression alphaValue = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, mAlphaValueLinkHandle, Alpha);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(alphaValue));
            codeClass.Members.Add(valueEvaluateMethod);

            var alphaAssign = new CodeAssignStatement();
            alphaAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateAlpha");
            alphaAssign.Right = new CodeVariableReferenceExpression(valueEvaluateMethod.Name);
            codeStatementCollection.Add(alphaAssign);
            return;
        }
    }
}
