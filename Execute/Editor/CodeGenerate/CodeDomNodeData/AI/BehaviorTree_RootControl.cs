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
    public class BehaviorTree_RootControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "Root";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_RootControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_RootControlConstructionParams))]
    public partial class BehaviorTree_RootControl : BehaviorTree_BTNodeModifiers
    {
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


            //CreateBinding(mTemplateClassInstance, "CompositeNodeType", BehaviorTree_RootControl.CompositeNodeTypeProperty, CompositeNodeType);

        }
        #endregion
        public override bool CanDuplicate()
        {
            return false;
        }
        public BehaviorTree_RootControl(BehaviorTree_RootControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Background = TryFindResource("BehaviorTreeNode_RootColor") as SolidColorBrush;
            BindingTemplateClassInstanceProperties();

            InitializeLinkControl(csParam);
        }
        protected override void InitializeLinkControl(ConstructionParams csParam)
        {
            mOutLinkHandle = LinkOutHandle;
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        int mPriority = -1;
        public void ReCalculatePriority()
        {
            mPriority = -1;
            var linkObj = mOutLinkHandle.GetLinkedObject(0);
            if (linkObj == null)
                return;
            var node = linkObj as BehaviorTree_BTCenterDataControl;
            if (node != null)
            {
                node.CalculateNodePriority(ref mPriority);
            }
        }
        public override void ContainerLoadComplete(NodesContainer container)
        {
            ReCalculatePriority();
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_RootControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_RootControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateBehaviorTree(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var outPoseLinkObj = mOutLinkHandle.GetLinkedObject(0, false);
            var outPoseLinkElm = mOutLinkHandle.GetLinkedPinControl(0, false);
            if (outPoseLinkObj != null)
            {
                await outPoseLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, outPoseLinkElm, context);
                var behaviorTreeRootAssign = new CodeAssignStatement();
                behaviorTreeRootAssign.Left = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "BehaviorTree"), "Root");
                behaviorTreeRootAssign.Right = outPoseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(behaviorTreeRootAssign);
            }

        }

    }
}
