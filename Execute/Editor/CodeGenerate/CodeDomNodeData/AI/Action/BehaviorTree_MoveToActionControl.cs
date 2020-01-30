using CodeGenerateSystem.Base;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeDomNode.AI
{
    public class BehaviorTree_MoveToActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "MoveTo";
        [EngineNS.Rtti.MetaData]
        public string TargetPosition { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string ArriveDistance { get; set; } = "";
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_MoveToActionControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.TargetPosition = TargetPosition;
            retVal.ArriveDistance = ArriveDistance;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_MoveToActionControlConstructionParams))]
    public partial class BehaviorTree_MoveToActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_MoveToActionControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP
        public string ArriveDistance
        {
            get { return (string)GetValue(ArriveDistanceProperty); }
            set
            {
                SetValue(ArriveDistanceProperty, value);
                var para = CSParam as BehaviorTree_MoveToActionControlConstructionParams;
                para.ArriveDistance = value;
            }
        }
        public static readonly DependencyProperty ArriveDistanceProperty = DependencyProperty.Register("ArriveDistance", typeof(string), typeof(BehaviorTree_MoveToActionControl), new UIPropertyMetadata("", OnArriveDistancePropertyChanged));
        private static void OnArriveDistancePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_MoveToActionControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.ArriveDistance = (string)e.NewValue;
        }
        public string TargetPosition
        {
            get { return (string)GetValue(TargetPositionProperty); }
            set
            {
                SetValue(TargetPositionProperty, value);
                var para = CSParam as BehaviorTree_MoveToActionControlConstructionParams;
                para.TargetPosition = value;
            }
        }
        public static readonly DependencyProperty TargetPositionProperty = DependencyProperty.Register("TargetPosition", typeof(string), typeof(BehaviorTree_MoveToActionControl), new UIPropertyMetadata("", OnTargetPositionPropertyChanged));
        private static void OnTargetPositionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_MoveToActionControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.TargetPosition = (string)e.NewValue;
        }

        #endregion

        #region ShowProperty
        public override void OnCenterDataChange()
        {
            BindingTemplateClassInstanceProperties();
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "TargetPosition", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute(BTCenterDataWarpper.CenterDataType, new Type[] { typeof(EngineNS.Vector3) }) }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "ArriveDistance", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute(BTCenterDataWarpper.CenterDataType, new Type[] { typeof(long), typeof(Int32), typeof(float), typeof(double) }) }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, "PG_" + BTCenterDataWarpper.CenterDataType.Name + this.Id.ToString(), false);


            CreateBinding(mTemplateClassInstance, "TargetPosition", BehaviorTree_MoveToActionControl.TargetPositionProperty, TargetPosition);
            CreateBinding(mTemplateClassInstance, "ArriveDistance", BehaviorTree_MoveToActionControl.ArriveDistanceProperty, ArriveDistance);
        }
        #endregion
        public BehaviorTree_MoveToActionControl(BehaviorTree_MoveToActionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;

            TargetPosition = csParam.TargetPosition;
            ArriveDistance = csParam.ArriveDistance;
            mChildNodeContainer = DecoratorPanel;
            mServiceContainer = ServicePanel;
            BindingTemplateClassInstanceProperties();
            Background = TryFindResource("BehaviorTreeNode_CustomActionColor") as SolidColorBrush;
            InitializeLinkControl(csParam);
        }
        protected override void InitializeLinkControl(ConstructionParams csParam)
        {
            mInLinkHandle = LinkInHandle;
            AddLinkPinInfo("InLinkHandle", mInLinkHandle, null);
            base.InitializeLinkControl(csParam);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_MoveToActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_MoveToActionControl);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Type nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Leaf.Action.MoveToBehavior);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            if (TargetPosition != "")
            {
                var evaluateMethod = await Helper.CreateEvaluateMethod(codeClass, ValidName + "TargetPositionEvaluateMethod", typeof(Vector3), BTCenterDataWarpper.CenterDataType, TargetPosition, Vector3.Zero, context);
                var methodAssign = new CodeAssignStatement();
                methodAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "TargetPositionEvaluateFunc");
                methodAssign.Right = new CodeVariableReferenceExpression(evaluateMethod.Name);
                codeStatementCollection.Add(methodAssign);
            }
            if (ArriveDistance != "")
            {
                var evaluateMethod = await Helper.CreateEvaluateMethod(codeClass, ValidName + "ArriveDistanceEvaluateMethod", typeof(float), BTCenterDataWarpper.CenterDataType, ArriveDistance, 0.0f, context);
                var methodAssign = new CodeAssignStatement();
                methodAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "ArriveDistanceEvaluateFunc");
                methodAssign.Right = new CodeVariableReferenceExpression(evaluateMethod.Name);
                codeStatementCollection.Add(methodAssign);
            }
            var hostAssign = new CodeAssignStatement();
            hostAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Host");
            hostAssign.Right = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "HostActor");
            codeStatementCollection.Add(hostAssign);
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
