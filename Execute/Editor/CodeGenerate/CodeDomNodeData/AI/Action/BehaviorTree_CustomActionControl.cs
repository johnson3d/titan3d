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
    public class BehaviorTree_CustomActionControlConstructionParams : BehaviorTree_BTNodeModifiersConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "CustomTask";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_CustomActionControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(BehaviorTree_CustomActionControlConstructionParams))]
    public partial class BehaviorTree_CustomActionControl : BehaviorTree_BTNodeModifiers
    {
        public string Priotiry
        {
            get { return (string)GetValue(PriotiryProperty); }
            set { SetValue(PriotiryProperty, value); }
        }
        public static readonly DependencyProperty PriotiryProperty = DependencyProperty.Register("Priotiry", typeof(string), typeof(BehaviorTree_CustomActionControl), new FrameworkPropertyMetadata("-1"));
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

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);


            //CreateBinding(mTemplateClassInstance, "CompositeNodeType", BehaviorTree_CustomActionControl.CompositeNodeTypeProperty, CompositeNodeType);

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
                        methodInfo.MethodName = "CustomTask_" + ValidName;
                        methodInfo.DisplayName = "CustomTask_" + NodeName;
                    }
                }
            }
        }
        #endregion
        public BehaviorTree_CustomActionControl(BehaviorTree_CustomActionControlConstructionParams csParam)
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
        #region CustomAction
        public override void OnCenterDataChange()
        {
            if (mLinkedNodesContainer != null)
            {
                for (int i = 0; i < mLinkedNodesContainer.CtrlNodeList.Count; ++i)
                {
                    if (mLinkedNodesContainer.CtrlNodeList[i] is MethodCustom)
                    {
                        var method = mLinkedNodesContainer.CtrlNodeList[i] as MethodCustom;
                        //CustomMethodInfo methodInfo = new CustomMethodInfo();

                        var methodInfo = method.GetMethodInfo();
                        var param = methodInfo.InParams[1];
                        param.Attributes.Clear();
                        param.Attributes.Add(new EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute(BTCenterDataWarpper.CenterDataType));
                        Action resetMethodInfo = async () =>
                         {
                             await method.ResetMethodInfo(methodInfo);
                             param.OnVariableTypeChanged(param.ParamType);
                         };
                        resetMethodInfo();
                    }
                }
            }
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
                Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + this.Id.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                CreateSubContainerDefaultNodes();
            }
            mLinkedNodesContainer.HostNode = this;
            //mLinkedNodesContainer.OnFilterContextMenu = CustomAction_FilterContextMenu;
            //mLinkedNodesContainer.OnCheckDropAvailable = CustomLAPoseGraphCheckDropAvailable;
        }
      
        void CreateSubContainerDefaultNodes()
        {
            var param = CSParam as BehaviorTree_CustomActionControlConstructionParams;
            var miAssist = new CustomMethodInfo();
            miAssist.MethodName = "CustomTask_" + ValidName;
            miAssist.DisplayName = "CustomTask_" + NodeName;
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
            returnState.ParamType = new VariableType(typeof(EngineNS.Bricks.AI.BehaviorTree.BehaviorStatus), param.CSType);
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
        bool CustomLAPoseGraphCheckDropAvailable(DragEventArgs e)
        {
            if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
                var data = dragObject[0] as EditorCommon.CodeGenerateSystem.INodeListAttribute;
                var ty = data.NodeType;
                if (data != null)
                    return true;
            }
            return true;

        }
        private void CustomAction_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
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
        #endregion CustomLAPose
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BehaviorTree_CustomActionControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(BehaviorTree_CustomActionControl);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
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
            Type nodeType = typeof(EngineNS.Bricks.AI.BehaviorTree.Leaf.Action.ActionBehavior);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);

            var actionAssign = new CodeAssignStatement();
            actionAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Func");
            actionAssign.Right = new CodeVariableReferenceExpression("CustomTask_" + ValidName);
            codeStatementCollection.Add(actionAssign);
            return new CodeVariableReferenceExpression(ValidName);
        }
    }
}
