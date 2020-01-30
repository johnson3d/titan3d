using CodeGenerateSystem.Base;
using EngineNS.Bricks.AI.BehaviorTree;
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
    public class BehaviorTree_ConditionFuncDecoratorControlConstructionParams : BehaviorTree_BTNodeInnerNodeConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "CustomCondition";
        [EngineNS.Rtti.MetaData]
        public bool Inverse { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public FlowControlType FlowControl { get; set; } = FlowControlType.None;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_ConditionFuncDecoratorControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Inverse = Inverse;
            retVal.FlowControl = FlowControl;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_ConditionFuncDecoratorControlConstructionParams))]
    public partial class BehaviorTree_ConditionFuncDecoratorControl : BehaviorTree_BTNodeInnerNode
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_ConditionFuncDecoratorControl), new FrameworkPropertyMetadata("-1"));
        partial void InitConstruction();
        #region DP

        public bool Inverse
        {
            get { return (bool)GetValue(InverseProperty); }
            set
            {
                SetValue(InverseProperty, value);
                var para = CSParam as BehaviorTree_ConditionFuncDecoratorControlConstructionParams;
                para.Inverse = value;
            }
        }
        public static readonly DependencyProperty InverseProperty = DependencyProperty.Register("Inverse", typeof(bool), typeof(BehaviorTree_ConditionFuncDecoratorControl), new UIPropertyMetadata(false, OnInversePropertyChanged));
        private static void OnInversePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_ConditionFuncDecoratorControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.Inverse = (bool)e.NewValue;
        }
        public FlowControlType FlowControl
        {
            get { return (FlowControlType)GetValue(FlowControlProperty); }
            set
            {
                SetValue(FlowControlProperty, value);
                var para = CSParam as BehaviorTree_ConditionFuncDecoratorControlConstructionParams;
                para.FlowControl = value;
            }
        }
        public static readonly DependencyProperty FlowControlProperty = DependencyProperty.Register("FlowControl", typeof(FlowControlType), typeof(BehaviorTree_ConditionFuncDecoratorControl), new UIPropertyMetadata(FlowControlType.None, OnFlowControlPropertyChanged));
        private static void OnFlowControlPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BehaviorTree_ConditionFuncDecoratorControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.FlowControl = (FlowControlType)e.NewValue;
        }
        #endregion
        #region ShowProperty
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "Inverse", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(FlowControlType), "FlowControl", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            CreateBinding(mTemplateClassInstance, "Inverse", BehaviorTree_ConditionFuncDecoratorControl.InverseProperty, Inverse);
            CreateBinding(mTemplateClassInstance, "FlowControl", BehaviorTree_ConditionFuncDecoratorControl.FlowControlProperty, FlowControl);
        }
        protected override void NodeNameChangedOverride(BaseNodeControl d, string oldVal, string newVal)
        {
            base.NodeNameChangedOverride(d, oldVal, newVal);
            if (mLinkedNodesContainer != null)
            {
                for (int i = 0; i < mLinkedNodesContainer.CtrlNodeList.Count; ++i)
                {
                    if (mLinkedNodesContainer.CtrlNodeList[i] is MethodCustom)
                    {
                        var mc = mLinkedNodesContainer.CtrlNodeList[i] as MethodCustom;
                        var methodInfo = mc.GetMethodInfo();
                        methodInfo.MethodName = "CustomCondition_" + ValidName;
                        methodInfo.DisplayName = NodeName;
                    }
                }
            }
        }
        #endregion
        public BehaviorTree_ConditionFuncDecoratorControl(BehaviorTree_ConditionFuncDecoratorControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Inverse = csParam.Inverse;
            FlowControl = csParam.FlowControl;
            BindingTemplateClassInstanceProperties();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }
        #region Duplicate
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var node = base.Duplicate(param);
            Action action = async () =>
            {
                var container = await GetNodesContainer();
                var containerCopy = container.Duplicate() as CodeGenerateSystem.Controls.NodesContainerControl;
                containerCopy.GUID = node.Id;
                containerCopy.TitleString = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + node.Id.ToString();
                param.TargetNodesContainer.HostControl.SubNodesContainers.Add(node.Id, containerCopy);
                containerCopy.HostControl = param.TargetNodesContainer.HostControl;
                node.LinkedNodesContainer = containerCopy;
            };
            action.Invoke();
            return node;
        }
        async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetNodesContainer()
        {
            if (mLinkedNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + this.Id.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateSubContainerDefaultNodes();
                }
            }
            return mLinkedNodesContainer;
        }
        #endregion Duplicate
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            var item = new MenuItem()
            {
                Name = "OpenCostomGraph",
                Header = "OpenCostomGraph",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var noUse = OpenCustomActionGraph();
            };
            contextMenu.Items.Add(item);
        }
        protected override void MenuItem_Click_Del(object sender, RoutedEventArgs e)
        {
            if (!CheckCanDelete())
                return;
            var modifiderContainer = ParentNode as BehaviorTree_BTNodeModifiers;
            modifiderContainer?.DeleteClick(this);
            IsDirty = true;
        }

        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var noUse = OpenCustomActionGraph();
            base.MouseLeftButtonDoubleClick(sender, e);
        }
        async System.Threading.Tasks.Task OpenCustomActionGraph()
        {
            var assist = this.HostNodesContainer.HostControl;
            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_CustomCondition" + ":" + this.Id.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                CreateSubContainerDefaultNodes();
            }
            mLinkedNodesContainer.HostNode = this;
            mLinkedNodesContainer.OnFilterContextMenu = CustomCondition_FilterContextMenu;
            //mLinkedNodesContainer.OnCheckDropAvailable = CustomLAPoseGraphCheckDropAvailable;
        }
        private void CustomCondition_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var assist = mLinkedNodesContainer.HostControl;
            assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            var cdvCP = new CodeDomNode.CenterDataValueControl.CenterDataValueControlConstructParam()
            {
                CSType = CSParam.CSType,
                ConstructParam = "",
            };
            cdvCP.CenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CenterDataValueControl), "CenterDataValue", cdvCP, "");
        }
        void CreateSubContainerDefaultNodes()
        {
            var param = CSParam as BehaviorTree_ConditionFuncDecoratorControlConstructionParams;
            var miAssist = new CustomMethodInfo();
            miAssist.MethodName = "CustomCondition_" + ValidName;
            miAssist.DisplayName = NodeName;
            var eTime = new CustomMethodInfo.FunctionParam();
            eTime.ParamType = new VariableType(typeof(long), param.CSType);
            eTime.ParamName = "elapseTime";
            miAssist.InParams.Add(eTime);
            var context = new CustomMethodInfo.FunctionParam();
            context.ParamType = new VariableType(typeof(EngineNS.GamePlay.Actor.GCenterData), param.CSType);
            context.ParamName = "context";
            context.Attributes.Add(new EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute(BTCenterDataWarpper.CenterDataType));
            miAssist.InParams.Add(context);
            var returnState = new CustomMethodInfo.FunctionParam();
            returnState.ParamType = new VariableType(typeof(bool), param.CSType);
            returnState.ParamName = "Return";
            miAssist.OutParams.Add(returnState);
            var nodeType = typeof(CodeDomNode.MethodCustom);
            var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
            {
                CSType = param.CSType,
                HostNodesContainer = mLinkedNodesContainer,
                ConstructParam = "",
                IsShowProperty = false,
                MethodInfo = miAssist,
            };
            var node = mLinkedNodesContainer.AddOrigionNode(nodeType, csParam, 0, 100);
            node.IsDeleteable = false;
            node.NodeNameAddShowNodeName = false;

            var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
            {
                CSType = param.CSType,
                HostNodesContainer = mLinkedNodesContainer,
                ConstructParam = "",
                ShowPropertyType = ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                MethodInfo = miAssist,
            };
            var retNode = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.ReturnCustom), retCSParam, 500, 100) as CodeDomNode.ReturnCustom;
            retNode.IsDeleteable = false;


            mLinkedNodesContainer.HostNode = this;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_ConditionFuncDecoratorControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_ConditionFuncDecoratorControl);
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
            if (mLinkedNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_CustomCondition" + ":" + this.Id.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateSubContainerDefaultNodes();
                }
            }
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                if (ctrl is CodeDomNode.MethodCustom)
                {
                    await ctrl.GCode_CodeDom_GenerateCode(codeClass, null, context.ClassContext);
                }
            }
            Type nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Decorator.ConditionFuncDecorator);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            var flowAssign = new CodeAssignStatement();
            flowAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "FlowControl");
            flowAssign.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(FlowControlType)), FlowControl.ToString());
            codeStatementCollection.Add(flowAssign);


            var inverseAssign = new CodeAssignStatement();
            inverseAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Inverse");
            inverseAssign.Right = new CodePrimitiveExpression(Inverse);
            codeStatementCollection.Add(inverseAssign);

            var actionAssign = new CodeAssignStatement();
            actionAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Func");
            actionAssign.Right = new CodeVariableReferenceExpression("CustomCondition_" + ValidName);
            codeStatementCollection.Add(actionAssign);
        }
    }
}
