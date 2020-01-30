using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    public enum AnimAssetLocation
    {
        Graph,
        State,
    }
    [EngineNS.Rtti.MetaClass]
    public class AnimAsset : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public AnimAssetLocation AnimAssetLocation { get; set; }
        [EngineNS.Rtti.MetaData]
        public string AnimAssetLocationName { get; set; }
        [EngineNS.Rtti.MetaData]
        public string AnimAssetName { get; set; }
    }
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
            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
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
            List<AnimAsset> animationNames = new List<AnimAsset>();
            foreach (var ctrl in HostNodesContainer.CtrlNodeList)
            {
                if (ctrl is AnimationAsset)
                {
                    var animParam = ctrl.CSParam as AnimationAssetConstructionParams;
                    animationNames.Add(animParam.AnimAsset);
                }
            }
            foreach (var sub in HostNodesContainer.HostControl.SubNodesContainers)
            {
                foreach (var ctrl in sub.Value.CtrlNodeList)
                {
                    if (ctrl is AnimationAsset)
                    {
                        var animParam = ctrl.CSParam as AnimationAssetConstructionParams;
                        animationNames.Add(animParam.AnimAsset);
                    }
                }
            }
            foreach (var animAsset in animationNames)
            {
                var tmCP = new AnimationTimeRemainingConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    NodeName = animAsset.AnimAssetName,
                    AnimAsset = animAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(AnimationTimeRemainingControl), "AnimationTimeRemaining/" + animAsset.AnimAssetLocationName +"_" + animAsset.AnimAssetName + "_TimeRemaining", tmCP, "");
            }
        }
        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as AnimStateTransitionControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl;

            if (mLinkedNodesContainer == null)
            {
                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (!data.IsCreated)
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
                        await mLinkedNodesContainer.Load(node);
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
                var csParam = new FinalTransitionResultConstructionParams()
                {
                    CSType = param.CSType,
                    NodeName = "Result",
                    HostNodesContainer = mLinkedNodesContainer,
                    ConstructParam = "",
                };
                var node = mLinkedNodesContainer.AddOrigionNode(typeof(FinalTransitionResultControl), csParam, 300, 0) as FinalTransitionResultControl;
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
                    if (ctrl is FinalTransitionResultControl)
                    {
                        await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
                    }
                }
            }

        }
    }
}
