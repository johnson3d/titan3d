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
    public class LACCDIKControlConstructionParams : SkeletonConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Alpha { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public uint Iteration { get; set; } = 15;
        [EngineNS.Rtti.MetaData]
        public float LimitAngle { get; set; } = 180;
        [EngineNS.Rtti.MetaData]
        public string RootBoneName { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string EndEffecterBoneName { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string TargetBoneName { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "CCDIK";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LACCDIKControlConstructionParams;

            retVal.Alpha = Alpha;
            retVal.NodeName = NodeName;
            retVal.Iteration = Iteration;
            retVal.LimitAngle = LimitAngle;
            retVal.RootBoneName = RootBoneName;
            retVal.EndEffecterBoneName = EndEffecterBoneName;
            retVal.TargetBoneName = TargetBoneName;
            retVal.SkeletonAsset = SkeletonAsset;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LACCDIKControlConstructionParams))]
    public partial class LACCDIKControl : CodeGenerateSystem.Base.BaseNodeControl, WPG.Themes.TypeEditors.ISkeletonControl
    {
        partial void InitConstruction();
        string mSkeletonAsset = "";
        public string SkeletonAsset
        {
            get
            {
                return   mSkeletonAsset;
            }
            set
            {
                mSkeletonAsset = value;

            }
        }

        #region DP
        public float Alpha
        {
            get { return (float)GetValue(AlphaProperty); }
            set
            {
                SetValue(AlphaProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.Alpha = value;
            }
        }
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(float), typeof(LACCDIKControl), new UIPropertyMetadata(1.0f, OnAlphaPropertyyChanged));
        private static void OnAlphaPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Alpha = (float)e.NewValue;
        }
        public uint Iteration
        {
            get { return (uint)GetValue(IterationProperty); }
            set
            {
                SetValue(IterationProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.Iteration = value;
            }
        }
        public static readonly DependencyProperty IterationProperty = DependencyProperty.Register("Iteration", typeof(uint), typeof(LACCDIKControl), new UIPropertyMetadata((uint)15, OnIterationPropertyyChanged));
        private static void OnIterationPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.Alpha = (uint)e.NewValue;
        }
        public float LimitAngle
        {
            get { return (float)GetValue(LimitAngleProperty); }
            set
            {
                SetValue(LimitAngleProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.LimitAngle = value;
            }
        }
        public static readonly DependencyProperty LimitAngleProperty = DependencyProperty.Register("LimitAngle", typeof(float), typeof(LACCDIKControl), new UIPropertyMetadata(180.0f, OnLimitAnglePropertyyChanged));
        private static void OnLimitAnglePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.LimitAngle = (float)e.NewValue;
        }
        public string RootBoneName
        {
            get { return (string)GetValue(RootBoneNameProperty); }
            set
            {
                SetValue(RootBoneNameProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.RootBoneName = value;
            }
        }
        public static readonly DependencyProperty RootBoneNameProperty = DependencyProperty.Register("RootBoneName", typeof(string), typeof(LACCDIKControl), new UIPropertyMetadata("", OnRootBoneNamePropertyyChanged));
        private static void OnRootBoneNamePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.RootBoneName = (string)e.NewValue;
        }
        public string EndEffecterBoneName
        {
            get { return (string)GetValue(EndEffecterBoneNameProperty); }
            set
            {
                SetValue(EndEffecterBoneNameProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.EndEffecterBoneName = value;
            }
        }
        public static readonly DependencyProperty EndEffecterBoneNameProperty = DependencyProperty.Register("EndEffecterBoneName", typeof(string), typeof(LACCDIKControl), new UIPropertyMetadata("", OnEndEffecterBoneNamePropertyyChanged));
        private static void OnEndEffecterBoneNamePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.EndEffecterBoneName = (string)e.NewValue;
        }
        public string TargetBoneName
        {
            get { return (string)GetValue(TargetBoneNameProperty); }
            set
            {
                SetValue(TargetBoneNameProperty, value);
                var para = CSParam as LACCDIKControlConstructionParams;
                para.TargetBoneName = value;
            }
        }
        public static readonly DependencyProperty TargetBoneNameProperty = DependencyProperty.Register("TargetBoneName", typeof(string), typeof(LACCDIKControl), new UIPropertyMetadata("", OnTargetBoneNamePropertyyChanged));
        private static void OnTargetBoneNamePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LACCDIKControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.TargetBoneName = (string)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(uint), "Iteration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "LimitAngle", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "RootBone", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_LAGraphBonePoseSelectAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "EndEffecterBone", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_LAGraphBonePoseSelectAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "TargetBone", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_LAGraphBonePoseSelectAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Alpha", LACCDIKControl.AlphaProperty, Alpha);
            CreateBinding(mTemplateClassInstance, "Iteration", LACCDIKControl.IterationProperty, Iteration);
            CreateBinding(mTemplateClassInstance, "LimitAngle", LACCDIKControl.LimitAngleProperty, LimitAngle);
            CreateBinding(mTemplateClassInstance, "RootBone", LACCDIKControl.RootBoneNameProperty, RootBoneName);
            CreateBinding(mTemplateClassInstance, "EndEffecterBone", LACCDIKControl.EndEffecterBoneNameProperty, EndEffecterBoneName);
            CreateBinding(mTemplateClassInstance, "TargetBone", LACCDIKControl.TargetBoneNameProperty, TargetBoneName);
        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LACCDIKControl(LACCDIKControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Alpha = csParam.Alpha;

            Iteration = csParam.Iteration;
            LimitAngle = csParam.LimitAngle;
            RootBoneName = csParam.RootBoneName;
            EndEffecterBoneName = csParam.EndEffecterBoneName;
            TargetBoneName = csParam.TargetBoneName;
            SkeletonAsset = csParam.SkeletonAsset;
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
        void InitializeLinkControl(LACCDIKControlConstructionParams csParam)
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
            CollectLinkPinInfo(smParam, "ActiveValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Vector3, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "InPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AlphaValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LACCDIKControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LACCDIKControl);
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

            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_CCDIK);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);
            if (inPoseLinkObj != null)
            {
                var srcAssign = new CodeAssignStatement();
                srcAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "InPoseNode");
                srcAssign.Right = inPoseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(srcAssign);
            }
            var fieldAssign = new CodeAssignStatement();
            fieldAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Iteration");
            fieldAssign.Right = new CodePrimitiveExpression(Iteration);
            codeStatementCollection.Add(fieldAssign);
            fieldAssign = new CodeAssignStatement();
            fieldAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "LimitAngle");
            fieldAssign.Right = new CodePrimitiveExpression(LimitAngle);
            codeStatementCollection.Add(fieldAssign);
            fieldAssign = new CodeAssignStatement();
            fieldAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EndEffecterBoneName");
            fieldAssign.Right = new CodePrimitiveExpression(EndEffecterBoneName);
            codeStatementCollection.Add(fieldAssign);
            fieldAssign = new CodeAssignStatement();
            fieldAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "RootBoneName");
            fieldAssign.Right = new CodePrimitiveExpression(RootBoneName);
            codeStatementCollection.Add(fieldAssign);
            fieldAssign = new CodeAssignStatement();
            fieldAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "TargetBoneName");
            fieldAssign.Right = new CodePrimitiveExpression(TargetBoneName);
            codeStatementCollection.Add(fieldAssign);

            var targetPositionEvaluateMethod = new CodeMemberMethod();
            targetPositionEvaluateMethod.Name = ValidName + "_TargetPositionEvaluateMethod";
            targetPositionEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, targetPositionEvaluateMethod);
            CodeExpression valueExpression = null;
            var valueLinkObj = mActiveValueLinkHandle.GetLinkedObject(0, true);
            if (valueLinkObj == null)
            {
                valueExpression = new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Vector3)),new CodeExpression[] { new CodePrimitiveExpression(0), new CodePrimitiveExpression(0), new CodePrimitiveExpression(0) });
            }
            else
            {
                var valueLinkElm = mActiveValueLinkHandle.GetLinkedPinControl(0, true);
                if (!valueLinkObj.IsOnlyReturnValue)
                    await valueLinkObj.GCode_CodeDom_GenerateCode(codeClass, targetPositionEvaluateMethod.Statements, valueLinkElm, valueEvaluateMethodContex);
                valueExpression = valueLinkObj.GCode_CodeDom_GetValue(valueLinkElm, valueEvaluateMethodContex);
            }

            targetPositionEvaluateMethod.ReturnType = new CodeTypeReference(typeof(EngineNS.Vector3));
            targetPositionEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(valueExpression));
            codeClass.Members.Add(targetPositionEvaluateMethod);

            var targetPositionAssign = new CodeAssignStatement();
            targetPositionAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateTargetPositionFunc");
            targetPositionAssign.Right = new CodeVariableReferenceExpression(targetPositionEvaluateMethod.Name);
            codeStatementCollection.Add(targetPositionAssign);

            var alphaEvaluateMethod = await Helper.CreateEvaluateMethod(codeClass, ValidName + "AlphaEvaluateMethod", typeof(float), Alpha, mAlphaValueLinkHandle, context);
            var alphaAssign = new CodeAssignStatement();
            alphaAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateAlphaFunc");
            alphaAssign.Right = new CodeVariableReferenceExpression(alphaEvaluateMethod.Name);
            codeStatementCollection.Add(alphaAssign);
        }
    }
}
