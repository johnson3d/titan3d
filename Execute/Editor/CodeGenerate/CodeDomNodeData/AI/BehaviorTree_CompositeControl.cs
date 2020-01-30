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
    public enum CompositeNodeType
    {
        Sequence,
        Select,
        RandomSelect,
        Parallel,
    }
    public class BehaviorTree_CompositeControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public CompositeNodeType CompositeNodeType { get; set; } = CompositeNodeType.Sequence;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "Sequence";
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_CompositeControlConstructionParams;

            retVal.CompositeNodeType = CompositeNodeType;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_CompositeControlConstructionParams))]
    public partial class BehaviorTree_CompositeControl : BehaviorTree_BTNodeModifiers
    {
        public ImageSource NodeTypeImg
        {
            get { return (ImageSource)GetValue(NodeTypeImgProperty); }
            set { SetValue(NodeTypeImgProperty, value); }
        }
        public static readonly DependencyProperty NodeTypeImgProperty = DependencyProperty.Register("NodeTypeImg", typeof(ImageSource), typeof(BehaviorTree_CompositeControl), new FrameworkPropertyMetadata(null));
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_CompositeControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();

        #region DP
        public CompositeNodeType CompositeNodeType
        {
            get { return (CompositeNodeType)GetValue(CompositeNodeTypeProperty); }
            set
            {
                SetValue(CompositeNodeTypeProperty, value);
                var para = CSParam as BehaviorTree_CompositeControlConstructionParams;
                para.CompositeNodeType = CompositeNodeType;
            }
        }
        public static readonly DependencyProperty CompositeNodeTypeProperty = DependencyProperty.Register("CompositeNodeType", typeof(CompositeNodeType), typeof(BehaviorTree_CompositeControl), new UIPropertyMetadata(CompositeNodeType.Sequence, OnCompositeNodeTypePropertyyChanged));
        private static void OnCompositeNodeTypePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_CompositeControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.CompositeNodeType = (CompositeNodeType)e.NewValue;
        }

        #endregion DP
        #region ShowProperty
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(CompositeNodeType), "CompositeNodeType", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "CompositeNodeType", BehaviorTree_CompositeControl.CompositeNodeTypeProperty, CompositeNodeType);

        }
        #endregion ShowProperty
        public BehaviorTree_CompositeControl(BehaviorTree_CompositeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mChildNodeContainer = DecoratorPanel;
            mServiceContainer = ServicePanel;
            CompositeNodeType = csParam.CompositeNodeType;

            NodeName = CompositeNodeType.ToString();
            BindingTemplateClassInstanceProperties();

            InitializeLinkControl(csParam);
            Background = TryFindResource("BehaviorTreeNode_CompositeColor") as SolidColorBrush;
            switch (CompositeNodeType)
            {
                case CompositeNodeType.Select:
                    {
                        NodeTypeImg = TryFindResource("BehaviorTreeNode_Selecter_64x") as ImageSource;
                    }
                    break;
                case CompositeNodeType.RandomSelect:
                    {
                        NodeTypeImg = TryFindResource("BehaviorTreeNode_RandomSelecter_64x") as ImageSource;
                    }
                    break;
                case CompositeNodeType.Sequence:
                    {
                        NodeTypeImg = TryFindResource("BehaviorTreeNode_Sequence_64x") as ImageSource;
                    }
                    break;

                case CompositeNodeType.Parallel:
                    {
                        NodeTypeImg = TryFindResource("BehaviorTreeNode_Parallel_64x") as ImageSource;
                    }
                    break;
            }

        }
        protected override void InitializeLinkControl(ConstructionParams csParam)
        {
            mInLinkHandle = LinkInHandle;
            mOutLinkHandle = LinkOutHandle;
            AddLinkPinInfo("InLinkHandle", mInLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
            base.InitializeLinkControl(csParam);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_CompositeControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_CompositeControl);
        }

        //public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        //{
        //    return new CodeVariableReferenceExpression(ValidName);
        //}
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var linkObjs = mOutLinkHandle.GetLinkedObjects();
            for (int i = 0; i < linkObjs.Length; ++i)
            {
                for (int j = i; j < linkObjs.Length; ++j)
                {
                    if (linkObjs[i].GetLeftInCanvas(false) > linkObjs[j].GetLeftInCanvas(false))
                    {
                        var temp = linkObjs[i];
                        linkObjs[i] = linkObjs[j];
                        linkObjs[j] = temp;
                    }
                }
            }
            List<CodeExpression> childrenNode = new List<CodeExpression>();
            for (int i = 0; i < linkObjs.Length; ++i)
            {
                var linkElm = mOutLinkHandle.GetLinkedPinControl(i, true);
                await linkObjs[i].GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm, context);
                childrenNode.Add(linkObjs[i].GCode_CodeDom_GetSelfRefrence(null, null));
            }
            Type nodeType = null;
            switch (CompositeNodeType)
            {
                case CompositeNodeType.Select:
                    {
                        nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Composite.SelectBehavior);
                    }
                    break;
                case CompositeNodeType.RandomSelect:
                    {
                        nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Composite.SelectBehavior);
                    }
                    break;
                case CompositeNodeType.Sequence:
                    {
                        nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Composite.SequenceBehavior);
                    }
                    break;

                case CompositeNodeType.Parallel:
                    {
                        nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Composite.ParallelBehavior);
                    }
                    break;
            }
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            for (int i = 0; i < linkObjs.Length; ++i)
            {
                var addChild = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "AddChild"), new CodeExpression[] { childrenNode[i] });
                codeStatementCollection.Add(addChild);
            }
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
