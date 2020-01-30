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
    public class BehaviorTree_MoveDirectlyTowardActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "MoveDirectlyToward";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_MoveDirectlyTowardActionControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_MoveDirectlyTowardActionControlConstructionParams))]
    public partial class BehaviorTree_MoveDirectlyTowardActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_MoveDirectlyTowardActionControl), new FrameworkPropertyMetadata("-1"));
        public Type CenterDataType { get; set; } = null;

        partial void InitConstruction();

        #region DP

       

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
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(CompositeNodeType), "CompositeNodeType", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            //CreateBinding(mTemplateClassInstance, "CompositeNodeType", BehaviorTree_MoveDirectlyTowardActionControl.CompositeNodeTypeProperty, CompositeNodeType);

        }
        #endregion
        public BehaviorTree_MoveDirectlyTowardActionControl(BehaviorTree_MoveDirectlyTowardActionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
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
            return "BehaviorTree_MoveDirectlyTowardActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_MoveDirectlyTowardActionControl);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
