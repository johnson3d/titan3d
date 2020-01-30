using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CodeDomNode.Particle
{
    public class StringRegex
    {
        public static string GetValidName(string name)
        {
          return  Regex.Replace(name, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
        }
    }
    //public class AnimStateLinkInfoForUndoRedo
    //{
    //    public AnimStateLinkInfo linkInfo = null;
    //}
    public class StructNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; }
        [EngineNS.Rtti.MetaData]

        public CodeGenerateSystem.Base.enLinkType LinkType { get; set; } = CodeGenerateSystem.Base.enLinkType.Module;

        [EngineNS.Rtti.MetaData]
        public virtual string BaseClassName { get; set; }

        public delegate void SetHostNodesContainerCallBack();
        public event SetHostNodesContainerCallBack SetHostNodesContainerEvent;
        public override CodeGenerateSystem.Base.NodesContainer HostNodesContainer
        {
            get => base.HostNodesContainer;
            set
            {
                base.HostNodesContainer = value;
                if (value != null)
                {
                    SetHostNodesContainerEvent?.Invoke();
                }
            }
        }
        //public StructNodeControlConstructionParams()
        //{

        //}

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as StructNodeControlConstructionParams;
            retVal.NodeName = NodeName;
            retVal.LinkType = CodeGenerateSystem.Base.enLinkType.Module;
            retVal.BaseClassName = BaseClassName;
            return retVal;
        }

        //用于存放每个图的category items数据
        public Dictionary<string, Macross.Category> CategoryDic;
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(StructNodeControlConstructionParams))]
    public partial class StructNodeControl : CodeGenerateSystem.Base.BaseNodeControl, IParticleNode
    {
        StructLinkControl mCtrlValueLinkHandleUp = new StructLinkControl();
        StructLinkControl mCtrlValueLinkHandleDown = new StructLinkControl();
        public StructLinkControl CtrlValueLinkHandleDown
        {
            get => mCtrlValueLinkHandleDown;
        }

        partial void InitConstruction();

        public StructNodeControl(StructNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("Name");
            xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;

            if (string.IsNullOrEmpty(NodeName))
            {
                NodeName = "NewNode";
            }

            IsOnlyReturnValue = true;
            AddLinkPinInfo("StructLinkHandleUp", mCtrlValueLinkHandleUp, null);
            AddLinkPinInfo("StructLinkHandleDown", mCtrlValueLinkHandleDown, null);

        }
        public string GetClassName()
        {
            return "Node" + Id.ToString().Replace("-", "_");
        }
        public StructLinkControl GetLinkControlUp()
        {
            return mCtrlValueLinkHandleUp;
        }

        public CreateObject GetCreateObject()
        {
            return null;
        }
        public async System.Threading.Tasks.Task InitGraph()
        {
        }

        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            //{
            //    await InitGraphEvent();
            //    await OpenGraph();
            //    return true;
            //}, EngineNS.Thread.Async.EAsyncTarget.Editor);

        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        protected override void EndLink(LinkPinControl linkObj)
        {
            if (linkObj == null)
                return;

            bool alreadyLink = false;
            var pinInfo = GetLinkPinInfo("StructLinkHandleDown");
            if (HostNodesContainer.StartLinkObj == linkObj)
            {
                base.EndLink(null);
                if (HostNodesContainer.PreviewLinkCurve != null)
                    HostNodesContainer.PreviewLinkCurve.Visibility = System.Windows.Visibility.Hidden;
                return;
            }
            if (pinInfo == null)
                return;
           
            if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(HostNodesContainer.StartLinkObj, linkObj) && 
                ModuleLinkInfo.CanLinkWith2(HostNodesContainer.StartLinkObj, linkObj))
            {
                if (linkObj.LinkOpType == enLinkOpType.End)
                {
                    if (linkObj.LinkInfos.Count > 0)
                    {
                        linkObj.LinkInfos[0].m_linkFromObjectInfo.RemoveLink(linkObj.LinkInfos[0]);
                        linkObj.LinkInfos[0].m_linkToObjectInfo.RemoveLink(linkObj.LinkInfos[0]);
                        linkObj.LinkInfos[0].Clear();
                        linkObj.LinkInfos.Clear();
                    }
                }

                var container = new NodesContainer.LinkInfoContainer();
                //if (mStartLinkObj.LinkOpType == enLinkOpType.Start)
                //{
                    container.Start = HostNodesContainer.StartLinkObj;
                    container.End = linkObj;
                //}
                //else
                //{
                //    container.Start = objInfo;
                //    container.End = mStartLinkObj;
                //}
                HostNodesContainer.IsOpenContextMenu = false;
                var redoAction = new Action<Object>((obj) =>
                {
                    var linkInfo = new ModuleLinkInfo(HostNodesContainer.ContainerDrawCanvas, container.Start, container.End);
                });
                redoAction.Invoke(null);
                //EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                //                            (obj) =>
                //                            {
                //                                for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                //                                {
                //                                    var info = container.End.GetLinkInfo(i);
                //                                    if (info.m_linkFromObjectInfo == container.Start)
                //                                    {
                //                                        info.Clear();
                //                                        break;
                //                                    }
                //                                }
                //                            }, "Create Link");
                IsDirty = true;
            }

            var count = pinInfo.GetLinkInfosCount();
            for (int index = 0; index < count; ++index)
            {
                //AnimStateLinkInfoForUndoRedo undoRedoLinkInfo = new AnimStateLinkInfoForUndoRedo();
                //var linkInfo = pinInfo.GetLinkInfo(index);
                //if (linkInfo.m_linkFromObjectInfo == HostNodesContainer.StartLinkObj && linkInfo.m_linkToObjectInfo == linkObj)
                //{
                //    alreadyLink = true;
                //    undoRedoLinkInfo.linkInfo = linkInfo as AnimStateLinkInfo;
                //    NodesContainer.TransitionStaeBaseNodeForUndoRedo transCtrl = new NodesContainer.TransitionStaeBaseNodeForUndoRedo();
                //    var redoAction = new Action<Object>((obj) =>
                //    {
                //        transCtrl.TransitionStateNode = undoRedoLinkInfo.linkInfo.AddTransition();
                //    });
                //    redoAction.Invoke(null);
                //    EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                //                                (obj) =>
                //                                {
                //                                    undoRedoLinkInfo.linkInfo.RemoveTransition(transCtrl.TransitionStateNode);

                //                                }, "Create StateTransition");
                //}

                //if (HostNodesContainer.PreviewLinkCurve != null)
                //    HostNodesContainer.PreviewLinkCurve.Visibility = System.Windows.Visibility.Hidden;
                //base.EndLink(null,false);

            }
            //if (!alreadyLink)
            //    base.EndLink(linkObj);
        }

        protected override void MenuItem_Click_Del(object sender, RoutedEventArgs e)
        {
            if (!CheckCanDelete())
                return;
            //List<AnimStateLinkInfoForUndoRedo> undoRedoLinkInfos = new List<AnimStateLinkInfoForUndoRedo>();

            //for (int i = 0; i < mCtrlValueLinkHandle.GetLinkInfosCount(); i++)
            //{
            //    var undoRedoLinkInfo = new AnimStateLinkInfoForUndoRedo();
            //    var linkInfo = mCtrlValueLinkHandle.GetLinkInfo(i);
            //    var animStateLinkInfo = linkInfo as AnimStateLinkInfo;
            //    undoRedoLinkInfo.linkInfo = animStateLinkInfo;
            //    undoRedoLinkInfos.Add(undoRedoLinkInfo);
            //}

            //var redoAction = new Action<object>((obj) =>
            //{
            //    foreach (var linkInfo in undoRedoLinkInfos)
            //    {
            //        linkInfo.linkInfo.RemoveAllFromContainer();
            //    }
            //    HostNodesContainer.DeleteNode(this);
            //});
            //redoAction.Invoke(null);
            //EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
            //                                (obj) =>
            //                                {
            //                                    if (m_bMoveable)
            //                                    {
            //                                        if (ParentDrawCanvas != null)
            //                                        {
            //                                            ParentDrawCanvas.Children.Add(this);
            //                                            ParentDrawCanvas.Children.Add(mParentLinkPath);
            //                                        }
            //                                        HostNodesContainer.CtrlNodeList.Add(this);
            //                                    }
            //                                    foreach (var data in undoRedoLinkInfos)
            //                                    {
            //                                        data.linkInfo.ResetLink();
            //                                        data.linkInfo.AddAllToContainer();
            //                                        data.linkInfo.UpdateLink();
            //                                    }
            //                                }, "Delete Node");
            IsDirty = true;
        }
        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as StructNodeControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl;
            var TitleString = HostNodesContainer.TitleString;
            if (string.IsNullOrEmpty(TitleString))
            {
                TitleString = "MainGraph";
            }
            var title = TitleString + "/" + NodeName + ":" + this.Id.ToString();
            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = title,
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                await InitializeLinkedNodesContainer();
            }
            mLinkedNodesContainer.HostNode = this;
            //mLinkedNodesContainer.OnFilterContextMenu = StateControl_FilterContextMenu;
        }
        private void StateControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            //var ctrlAssist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            //List<string> cachedPosesName = new List<string>();
            //foreach (var ctrl in ctrlAssist.NodesControl.CtrlNodeList)
            //{
            //    if (ctrl is CachedAnimPoseControl)
            //    {
            //        if (!cachedPosesName.Contains(ctrl.NodeName))
            //            cachedPosesName.Add(ctrl.NodeName);
            //    }
            //}
            //foreach (var sub in ctrlAssist.SubNodesContainers)
            //{
            //    foreach (var ctrl in sub.Value.CtrlNodeList)
            //    {
            //        if (ctrl is CachedAnimPoseControl)
            //        {
            //            if (!cachedPosesName.Contains(ctrl.NodeName))
            //                cachedPosesName.Add(ctrl.NodeName);
            //        }
            //    }
            //}
            //var assist = mLinkedNodesContainer.HostControl;
            //assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            //var nodesList = contextMenu.GetNodesList();
            //nodesList.ClearNodes();
            //var stateCP = new AnimStateControlConstructionParams()
            //{
            //    CSType = HostNodesContainer.CSType,
            //    ConstructParam = "",
            //    NodeName = "State",
            //};
            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Particle.StructNodeControl), "State", stateCP, "");
            //foreach (var cachedPoseName in cachedPosesName)
            //{
            //    var getCachedPose = new GetCachedAnimPoseConstructionParams()
            //    {
            //        CSType = HostNodesContainer.CSType,
            //        ConstructParam = "",
            //        NodeName = "CachedPose_" + cachedPoseName,
            //    };
            //    nodesList.AddNodesFromType(filterData, typeof(GetCachedAnimPoseControl), "CachedAnimPose/" + getCachedPose.NodeName, getCachedPose, "");
            //}
        }
        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as StructNodeControlConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;

            if (mLinkedNodesContainer == null)
            {
                var TitleString = HostNodesContainer.TitleString;
                if (string.IsNullOrEmpty(HostNodesContainer.TitleString))
                {
                    TitleString = "MainGraph";
                }
                var title = TitleString + "/" + param.NodeName + ":" + this.Id.ToString();
                
                bool isCreated;
                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = title,
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (!data.IsCreated)
                    return;
            }
            // 读取graph
            var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
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


                //var csParam = new FinalAnimPoseConstructionParams()
                //{
                //    CSType = param.CSType,
                //    NodeName = "FinalPose",
                //    HostNodesContainer = mLinkedNodesContainer,
                //    ConstructParam = "",
                //};
                //var node = mLinkedNodesContainer.AddOrigionNode(typeof(FinalAnimPoseControl), csParam, 300, 0) as FinalAnimPoseControl;
                //node.IsDeleteable = false;

                //var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                //{
                //    CSType = param.CSType,
                //    HostNodesContainer = mLinkedNodesContainer,
                //    ConstructParam = "",
                //};
                //var retNode = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.ReturnCustom), retCSParam, 300, 0) as CodeDomNode.ReturnCustom;
                //retNode.IsDeleteable = false;
                //retNode.ShowProperty = false;
            }
            mLinkedNodesContainer.HostNode = this;
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "StructLinkHandleUp", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "StructLinkHandleDown", CodeGenerateSystem.Base.enLinkType.Module, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "Module";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(StructNodeControl);
        }
        public void ResetRecursionReachedFlag()
        {
            //if (!mCtrlValueLinkHandle.RecursionReached)
            //    return;
            //mCtrlValueLinkHandle.RecursionReached = false;
            ////指向的节点
            //for (int i = 0; i < mCtrlValueLinkHandle.GetLinkInfosCount(); ++i)
            //{
            //    var linkInfo = mCtrlValueLinkHandle.GetLinkInfo(i);
            //    if (linkInfo.m_linkFromObjectInfo == mCtrlValueLinkHandle)
            //    {
            //        var anim = linkInfo.m_linkToObjectInfo.HostNodeControl as AnimStateControl;
            //        if (anim != null)
            //            anim.ResetRecursionReachedFlag();
            //    }
            //}
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        CodeVariableReferenceExpression stateRef = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            //if (!mCtrlValueLinkHandle.RecursionReached)
            //{
            //    mCtrlValueLinkHandle.RecursionReached = true;
            //    /*
            //     EngineNS.GamePlay.StateMachine.AnimationState State = new EngineNS.GamePlay.StateMachine.AnimationState("State", StateMachine);
            //     State.AnimationPose = StateMachine.AnimationPose;
            //     */
            //    var stateMachineRef = context.AnimStateMachineReferenceExpression;
            //    var validName = StringRegex.GetValidName(NodeName);
            //    System.CodeDom.CodeVariableDeclarationStatement stateStateMent = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)),
            //                                                                        validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)), new CodeExpression[] { new CodePrimitiveExpression(validName), stateMachineRef }));
            //    codeStatementCollection.Add(stateStateMent);

            //    stateRef = new CodeVariableReferenceExpression(validName);
            //    CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression(stateRef, "AnimationPoseProxy");
            //    CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            //    animPoseAssign.Left = animPoseField;
            //    animPoseAssign.Right = context.StateMachineAnimPoseReferenceExpression;
            //    codeStatementCollection.Add(animPoseAssign);

            //    context.StateAnimPoseReferenceExpression = animPoseField;
            //    //子图
            //    context.AminStateReferenceExpression = stateRef;
            //    if (mLinkedNodesContainer != null)
            //    {
            //        foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            //        {
            //            if ((ctrl is CodeDomNode.MethodOverride) ||
            //                            (ctrl is CodeDomNode.MethodCustom) || ctrl is FinalAnimPoseControl || ctrl is CodeDomNode.Animation.StateEntryControl)
            //            {
            //                await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            //            }
            //        }
            //    }

            //    //指向的节点
            //    for (int i = 0; i < mCtrlValueLinkHandle.GetLinkInfosCount(); ++i)
            //    {
            //        var linkInfo = mCtrlValueLinkHandle.GetLinkInfo(i);
            //        if (linkInfo.m_linkFromObjectInfo == mCtrlValueLinkHandle)
            //        {

            //            //需要返回state，来添加两者之间 的转换关系
            //            await linkInfo.m_linkToObjectInfo.HostNodeControl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            //            if (mCtrlValueLinkHandle.LinkCurveType == enLinkCurveType.Line)
            //            {
            //                var linkCurve = linkInfo.LinkPath as CodeGenerateSystem.Base.AnimStateTransitionCurve;
            //                foreach (var transitionCtrl in linkCurve.TransitionControlList)
            //                {
            //                    //构建状态转换宏图
            //                    await transitionCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            //                }
            //                var desState = context.ReturnedAminStateReferenceExpression;
            //                var stateTransitionMethod = context.StateTransitionMethodReferenceExpression;
            //                if (stateTransitionMethod != null)
            //                {
            //                    //生成转换代码
            //                    CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(stateRef, "AddStateTransition");
            //                    CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(methodRef, new CodeExpression[] { desState, new CodeMethodReferenceExpression(null, stateTransitionMethod.Name) });
            //                    codeStatementCollection.Add(methodInvoke);
            //                }
            //            }
            //        }
            //    }
            //}
            ////将自己的state返回给上层递归
            //context.ReturnedAminStateReferenceExpression = stateRef;

            ////返回currentState
            //foreach (var node in mCtrlValueLinkHandle.GetLinkedObjects())
            //{
            //    if (node is CodeDomNode.Animation.StateEntryControl)
            //    {
            //        context.FirstStateReferenceExpression = stateRef;
            //    }
            //}
        }
    }
}
