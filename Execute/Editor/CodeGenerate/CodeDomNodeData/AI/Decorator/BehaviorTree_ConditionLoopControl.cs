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
using System.Windows.Media;

namespace CodeDomNode.AI
{
    public class BehaviorTree_ConditionLoopControlConstructionParams : BehaviorTree_BTNodeInnerNodeConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "ConditionLoop";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_ConditionLoopControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_ConditionLoopControlConstructionParams))]
    public partial class BehaviorTree_ConditionLoopControl : BehaviorTree_BTNodeInnerNode
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_ConditionLoopControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP

       

        #endregion

        #region ShowProperty
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(CompositeNodeType), "CompositeNodeType", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            //CreateBinding(mTemplateClassInstance, "CompositeNodeType", BehaviorTree_ConditionLoopControl.CompositeNodeTypeProperty, CompositeNodeType);

        }
        #endregion
        public BehaviorTree_ConditionLoopControl(BehaviorTree_ConditionLoopControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            BindingTemplateClassInstanceProperties();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        protected override void MenuItem_Click_Del(object sender, RoutedEventArgs e)
        {
            if (!CheckCanDelete())
                return;
            var modifiderContainer = ParentNode as BehaviorTree_BTNodeModifiers;
            modifiderContainer?.DeleteClick(this);
            IsDirty = true;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_ConditionLoopControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_ConditionLoopControl);
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
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
    }
}
