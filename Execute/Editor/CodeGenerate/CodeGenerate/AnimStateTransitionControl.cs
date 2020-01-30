using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CodeGenerateSystem.Controls
{
    public class AnimStateTransitionControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; }
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as AnimStateTransitionControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(AnimStateTransitionControlConstructionParams))]
    public partial class AnimStateTransitionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();

        public AnimStateTransitionControl(AnimStateTransitionControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
        }

        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var noUse = OpenGraph();


        }

        protected override void EndLink(LinkPinControl linkObj)
        {
            return;

        }
        protected override void DragMove(MouseEventArgs e)
        {
            
        }
        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as AnimStateTransitionControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl;
            var title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString();
            bool isCreated;
            mLinkedNodesContainer = assist.ShowSubNodesContainer(this.Id, title, out isCreated);

            if (isCreated)
            {
                await InitializeLinkedNodesContainer();
            }
            mLinkedNodesContainer.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
        }
        private void TransitionGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var assist = mLinkedNodesContainer.HostControl;
            assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            List<string> animationNames = new List<string>();
            foreach (var ctrl in HostNodesContainer.CtrlNodeList)
            {
                if (ctrl is AnimationAsset)
                {
                    animationNames.Add(ctrl.NodeName);
                }
            }
            foreach (var sub in HostNodesContainer.HostControl.SubNodesContainers)
            {
                foreach (var ctrl in sub.Value.CtrlNodeList)
                {
                    if (ctrl is AnimationAsset)
                    {
                        animationNames.Add(ctrl.NodeName);
                    }
                }
            }
            foreach (var name in animationNames)
            {
                var tmCP = new CodeGenerateSystem.Controls.AnimationTimeRemainingConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    NodeName = name,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeGenerateSystem.Controls.AnimationTimeRemainingControl), "AnimationTimeRemaining/" + name + "TimeRemaining", tmCP, "");
            }
        }
        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as AnimStateTransitionControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl;

            if (mLinkedNodesContainer == null)
            {
                var title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString();
                bool isCreated;
                mLinkedNodesContainer = assist.GetSubNodesContainer(Id, title, out isCreated);
                if (!isCreated)
                    return;
            }
            // 读取graph
            var tempFile = assist.GetGraphFileName(assist.LinkedCategoryItemName);
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(tempFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            bool bLoaded = false;
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("SubLinks");
                var idStr = Id.ToString();
                foreach (var node in linkNode.GetNodes())
                {
                    if (node.GetName() == idStr)
                    {
                        mLinkedNodesContainer.Load(node);
                        bLoaded = true;
                        break;
                    }
                }
            }
            if (bLoaded)
            {

            }
            else
            {
                var csParam = new CodeGenerateSystem.Controls.FinalTransitionResultConstructionParams()
                {
                    CSType = param.CSType,
                    NodeName = "Result",
                    HostNodesContainer = mLinkedNodesContainer,
                    ConstructParam = "",
                };
                var node = mLinkedNodesContainer.AddOrigionNode(typeof(CodeGenerateSystem.Controls.FinalTransitionResultControl), csParam, 300, 0) as CodeGenerateSystem.Controls.FinalTransitionResultControl;
                node.IsDeleteable = false;


            }
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimStateLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Both, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "AnimationPose";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(AnimStateTransitionControl);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {

            //subMacross
            if (mLinkedNodesContainer != null)
            {
                foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
                {
                    if (ctrl is CodeGenerateSystem.Controls.FinalTransitionResultControl)
                    {
                        await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
                    }
                }
            }

        }
    }
}
