using CodeGenerateSystem.Base;
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
    public class BehaviorTree_WaitDataTimeActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "WaitTimeByData";
        [EngineNS.Rtti.MetaData]
        public string Time { get; set; } = "";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_WaitDataTimeActionControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Time = Time;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_WaitDataTimeActionControlConstructionParams))]
    public partial class BehaviorTree_WaitDataTimeActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_WaitDataTimeActionControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP

        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set
            {
                SetValue(TimeProperty, value);
                var para = CSParam as BehaviorTree_WaitDataTimeActionControlConstructionParams;
                para.Time = value;
            }
        }
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(string), typeof(BehaviorTree_WaitDataTimeActionControl), new UIPropertyMetadata("", OnTimePropertyyChanged));
        private static void OnTimePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_WaitDataTimeActionControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.Time = (string)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Time", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_ClassPropertySelectAttributeAttribute(BTCenterDataWarpper.CenterDataType, new Type[] { typeof(long), typeof(Int32), typeof(float), typeof(double) }) }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Time", BehaviorTree_WaitDataTimeActionControl.TimeProperty, Time);

        }
        #endregion
        public BehaviorTree_WaitDataTimeActionControl(BehaviorTree_WaitDataTimeActionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Time = csParam.Time;
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
            return "BehaviorTree_WaitDataTimeActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_WaitDataTimeActionControl);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Type nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Leaf.Action.DelayBehavior);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            var evaluateMethod = await Helper.CreateEvaluateMethod(codeClass, ValidName + "WaitTimeEvaluateMethod", typeof(long), BTCenterDataWarpper.CenterDataType, Time, 1000, context);
            var methodAssign = new CodeAssignStatement();
            methodAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "WaitTimeEvaluateFunc");
            methodAssign.Right = new CodeVariableReferenceExpression(evaluateMethod.Name);
            codeStatementCollection.Add(methodAssign);
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
