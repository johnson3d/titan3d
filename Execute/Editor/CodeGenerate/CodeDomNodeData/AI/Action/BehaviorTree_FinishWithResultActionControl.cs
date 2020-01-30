using CodeGenerateSystem.Base;
using EngineNS.Bricks.AI.BehaviorTree;
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
    public class BehaviorTree_FinishWithResultActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "FinishWithResult";
        [EngineNS.Rtti.MetaData]
        public BehaviorStatus Result { get; set; } = BehaviorStatus.Success;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_FinishWithResultActionControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Result = Result;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_FinishWithResultActionControlConstructionParams))]
    public partial class BehaviorTree_FinishWithResultActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_FinishWithResultActionControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP

        public BehaviorStatus Result
        {
            get { return (BehaviorStatus)GetValue(ResultProperty); }
            set
            {
                SetValue(ResultProperty, value);
                var para = CSParam as BehaviorTree_FinishWithResultActionControlConstructionParams;
                para.Result = value;
            }
        }
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register("Result", typeof(BehaviorStatus), typeof(BehaviorTree_FinishWithResultActionControl), new UIPropertyMetadata(BehaviorStatus.Success, OnResultPropertyChanged));
        private static void OnResultPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_FinishWithResultActionControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.Result = (BehaviorStatus)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(BehaviorStatus), "Result", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Result", BehaviorTree_FinishWithResultActionControl.ResultProperty, Result);

        }
        #endregion
        public BehaviorTree_FinishWithResultActionControl(BehaviorTree_FinishWithResultActionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Result = csParam.Result;
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
            var shadowBorder = Template.FindName("PART_Shadow", this) as Border;
            if (shadowBorder != null)
                shadowBorder.Background = TryFindResource("BehaviorTreeNode_CustomActionColor") as SolidColorBrush;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_FinishWithResultActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_FinishWithResultActionControl);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Type nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Leaf.Action.FinishWithResultBehavior);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            var actionAssign = new CodeAssignStatement();
            actionAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Result");
            actionAssign.Right = new CodeFieldReferenceExpression(new System.CodeDom.CodeTypeReferenceExpression(typeof(BehaviorStatus)), Result.ToString());
            codeStatementCollection.Add(actionAssign);
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
