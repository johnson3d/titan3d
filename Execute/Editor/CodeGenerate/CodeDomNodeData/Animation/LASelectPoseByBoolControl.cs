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
    public class LASelectPoseByBoolControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public bool BoolValue { get; set; } = true;
        [EngineNS.Rtti.MetaData]
        public float TrueBlendValue { get; set; } = 0.1f;
        [EngineNS.Rtti.MetaData]
        public float FalseBlendValue { get; set; } = 0.1f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "SelectPoseByBool";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LASelectPoseByBoolControlConstructionParams;

            retVal.TrueBlendValue = TrueBlendValue;
            retVal.FalseBlendValue = FalseBlendValue;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LASelectPoseByBoolControlConstructionParams))]
    public partial class LASelectPoseByBoolControl : CodeGenerateSystem.Base.BaseNodeControl
    {


        partial void InitConstruction();

        #region DP
        public bool BoolValue
        {
            get { return (bool)GetValue(BoolValueProperty); }
            set
            {
                SetValue(BoolValueProperty, value);
                var para = CSParam as LASelectPoseByBoolControlConstructionParams;
                para.TrueBlendValue = TrueBlendValue;
            }
        }
        public static readonly DependencyProperty BoolValueProperty = DependencyProperty.Register("BoolValue", typeof(bool), typeof(LASelectPoseByBoolControl), new UIPropertyMetadata(true, BoolValuePropertyChanged));
        private static void BoolValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASelectPoseByBoolControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.BoolValue = (bool)e.NewValue;
        }
        public float TrueBlendValue
        {
            get { return (float)GetValue(TrueBlendValueProperty); }
            set
            {
                SetValue(TrueBlendValueProperty, value);
                var para = CSParam as LASelectPoseByBoolControlConstructionParams;
                para.TrueBlendValue = TrueBlendValue;
            }
        }
        public static readonly DependencyProperty TrueBlendValueProperty = DependencyProperty.Register("TrueBlendValue", typeof(float), typeof(LASelectPoseByBoolControl), new UIPropertyMetadata(0.1f, TrueBlendValuePropertyChanged));
        private static void TrueBlendValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASelectPoseByBoolControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.TrueBlendValue = (float)e.NewValue;
        }
        public float FalseBlendValue
        {
            get { return (float)GetValue(FalseBlendValueProperty); }
            set
            {
                SetValue(FalseBlendValueProperty, value);
                var para = CSParam as LASelectPoseByBoolControlConstructionParams;
                para.FalseBlendValue = FalseBlendValue;
            }
        }
        public static readonly DependencyProperty FalseBlendValueProperty = DependencyProperty.Register("FalseBlendValue", typeof(float), typeof(LASelectPoseByBoolControl), new UIPropertyMetadata(0.1f, FalseBlendValuePropertyChanged));
        private static void FalseBlendValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASelectPoseByBoolControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.FalseBlendValue = (float)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "BoolValue", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "TrueBlendTime", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "FalseBlendTime", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "BoolValue", LASelectPoseByBoolControl.BoolValueProperty, BoolValue);
            CreateBinding(mTemplateClassInstance, "TrueBlendTime", LASelectPoseByBoolControl.TrueBlendValueProperty, TrueBlendValue);
            CreateBinding(mTemplateClassInstance, "FalseBlendTime", LASelectPoseByBoolControl.FalseBlendValueProperty, FalseBlendValue);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LASelectPoseByBoolControl(LASelectPoseByBoolControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);

        }
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        #region InitializeLinkControl
        #region AddDeleteValueLink
        private void BoolValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Visible;
        }

        private void BoolValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Collapsed;
        }
        private void TrueBlendValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            TrueBlendTextBlock.Visibility = Visibility.Visible;
        }

        private void TrueBlendValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            TrueBlendTextBlock.Visibility = Visibility.Collapsed;
        }
        private void FalseBlendValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            FalseBlendTextBlock.Visibility = Visibility.Visible;
        }

        private void FalseBlendValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            FalseBlendTextBlock.Visibility = Visibility.Collapsed;
        }
        #endregion
        LinkPinControl mActiveLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mTrueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mFalseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mTrueBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mFalseBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LASelectPoseByBoolControlConstructionParams csParam)
        {
            mActiveLinkHandle = ActiveValueHandle;
            mTrueLinkHandle = TruePoseHandle;
            mFalseLinkHandle = FalsePoseHandle;
            mTrueBlendValueLinkHandle = TrueBlendValueHandle;
            mFalseBlendValueLinkHandle = FalseBlendValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mTrueLinkHandle.MultiLink = false;
            mFalseBlendValueLinkHandle.MultiLink = false;
            mTrueBlendValueLinkHandle.MultiLink = false;
            mFalseBlendValueLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mActiveLinkHandle.NameStringVisible = Visibility.Visible;
            mActiveLinkHandle.NameString = "ActiveValue";
            mActiveLinkHandle.OnAddLinkInfo += BoolValueLinkHandle_OnAddLinkInfo;
            mActiveLinkHandle.OnDelLinkInfo += BoolValueLinkHandle_OnDelLinkInfo;

            mTrueLinkHandle.NameStringVisible = Visibility.Visible;
            mTrueLinkHandle.NameString = "TruePose";
            mFalseLinkHandle.NameStringVisible = Visibility.Visible;
            mFalseLinkHandle.NameString = "FalsePose";

            mTrueBlendValueLinkHandle.NameStringVisible = Visibility.Visible;
            mTrueBlendValueLinkHandle.NameString = "TrueBlendTime";
            TrueBlendTextBlock.Visibility = Visibility.Visible;
            mTrueBlendValueLinkHandle.OnAddLinkInfo += TrueBlendValueLinkHandle_OnAddLinkInfo;
            mTrueBlendValueLinkHandle.OnDelLinkInfo += TrueBlendValueLinkHandle_OnDelLinkInfo;

            mFalseBlendValueLinkHandle.NameStringVisible = Visibility.Visible;
            mFalseBlendValueLinkHandle.NameString = "FalseBlendTime";
            FalseBlendTextBlock.Visibility = Visibility.Visible;
            mFalseBlendValueLinkHandle.OnAddLinkInfo += FalseBlendValueLinkHandle_OnAddLinkInfo;
            mFalseBlendValueLinkHandle.OnDelLinkInfo += FalseBlendValueLinkHandle_OnDelLinkInfo;

            AddLinkPinInfo("ActiveValueHandle", mActiveLinkHandle, null);
            AddLinkPinInfo("TrueLinkHandle", mTrueLinkHandle, null);
            AddLinkPinInfo("FalseLinkHandle", mFalseLinkHandle, null);
            AddLinkPinInfo("TrueBlendValueLinkHandle", mTrueBlendValueLinkHandle, null);
            AddLinkPinInfo("FalseBlendValueLinkHandle", mFalseBlendValueLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ActiveValueHandle", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "TrueLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "FalseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "TrueBlendValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "FalseBlendValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LASelectPoseByBoolControl";
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
            var trueLinkObj = mTrueLinkHandle.GetLinkedObject(0, true);
            var trueLinkElm = mTrueLinkHandle.GetLinkedPinControl(0, true);
            if (trueLinkObj != null)
                await trueLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, trueLinkElm, context);
            var falseLinkObj = mFalseLinkHandle.GetLinkedObject(0, true);
            var falseLinkElm = mFalseLinkHandle.GetLinkedPinControl(0, true);
            if (falseLinkObj != null)
                await falseLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, falseLinkElm, context);

            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_SelectPoseByInt);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            if (trueLinkObj != null)
            {
                var trueItem =await CreateBlendTree_PoseItemForBlend(codeClass, codeStatementCollection, mTrueLinkHandle, ValidName+ "_TruePoseItem", mTrueBlendValueLinkHandle, TrueBlendValue, context);
                var addTrueInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(1), trueItem });
                codeStatementCollection.Add(addTrueInvoke);
            }
            if (falseLinkObj != null)
            {
                var falseItme = await CreateBlendTree_PoseItemForBlend(codeClass, codeStatementCollection, mFalseLinkHandle, ValidName + "_FalsePoseItem", mFalseBlendValueLinkHandle, FalseBlendValue, context);
                var addFalseInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(0), falseItme });
                codeStatementCollection.Add(addFalseInvoke);
            }
            var selectMethod =await CreateSelectedMethod(codeClass, ValidName + "_ActiveSelectValue", typeof(int), BoolValue, mActiveLinkHandle, context);
            var selectMethodAssign = new CodeAssignStatement();
            selectMethodAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateSelectedFunc");
            selectMethodAssign.Right = new CodeVariableReferenceExpression(selectMethod.Name);
            codeStatementCollection.Add(selectMethodAssign);
            return;
        }
        public async System.Threading.Tasks.Task<CodeVariableReferenceExpression> CreateBlendTree_PoseItemForBlend(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl poseLinkHandle, string itemName, LinkPinControl blendTimeLinkHandle, object defaultValue, GenerateCodeContext_Method context)
        {
            var poseLinkObj = poseLinkHandle.GetLinkedObject(0, true);
            var truePoseItemForBlend = new CodeVariableDeclarationStatement(typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_PoseItemForBlend), itemName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_PoseItemForBlend))));
            codeStatementCollection.Add(truePoseItemForBlend);
            var truePoseNodeAssign = new CodeAssignStatement();
            truePoseNodeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(itemName), "PoseNode");
            truePoseNodeAssign.Right = poseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
            codeStatementCollection.Add(truePoseNodeAssign);

            var blendTimeEvaluateMethod =await Helper.CreateEvaluateMethod(codeClass, itemName + "_BlendTimeEvaluateMethod", typeof(float), defaultValue, blendTimeLinkHandle, context);
            var blendTimeAssign = new CodeAssignStatement();
            blendTimeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(itemName), "EvaluateBlendTimeFunc");
            blendTimeAssign.Right = new CodeVariableReferenceExpression(blendTimeEvaluateMethod.Name);
            codeStatementCollection.Add(blendTimeAssign);
            return new CodeVariableReferenceExpression(itemName);
        }
        public async System.Threading.Tasks.Task<CodeMemberMethod> CreateSelectedMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, object defaultValue, LinkPinControl linkHandle, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);

            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var value = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, defaultValue);

            var vBoolCast = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(EngineNS.vBOOL)), "FromBoolean"), new CodeExpression[] { value });
            var vbool = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.vBOOL)), "vBoolValue", vBoolCast);
            valueEvaluateMethod.Statements.Add(vbool);
            var valueField = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("vBoolValue"), "Value");
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(valueField));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
    }
}

