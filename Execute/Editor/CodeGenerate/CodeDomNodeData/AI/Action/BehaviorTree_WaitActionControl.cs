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
    public class BehaviorTree_WaitActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "Wait";
        [EngineNS.Rtti.MetaData]
        public long Time { get; set; } = 1000;

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_WaitActionControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Time= Time;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_WaitActionControlConstructionParams))]
    public partial class BehaviorTree_WaitActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_WaitActionControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP
        public long Time
        {
            get { return (long)GetValue(TimeProperty); }
            set
            {
                SetValue(TimeProperty, value);
                var para = CSParam as BehaviorTree_WaitActionControlConstructionParams;
                para.Time = value;
            }
        }
        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(long), typeof(BehaviorTree_WaitActionControl), new UIPropertyMetadata((long)1000, OnTimePropertyyChanged));
        private static void OnTimePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_WaitActionControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.Time = (long)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(long), "Time", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Time", BehaviorTree_WaitActionControl.TimeProperty, Time);

        }
        #endregion
        public BehaviorTree_WaitActionControl(BehaviorTree_WaitActionControlConstructionParams csParam)
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
            return "BehaviorTree_WaitActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_WaitActionControl);
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

            var actionAssign = new CodeAssignStatement();
            actionAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "WaitTime");
            actionAssign.Right = new CodePrimitiveExpression(Time);
            codeStatementCollection.Add(actionAssign);
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
