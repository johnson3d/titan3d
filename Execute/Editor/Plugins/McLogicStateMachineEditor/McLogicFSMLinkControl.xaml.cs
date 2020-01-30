using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EditorCommon;
using EditorCommon.Resources;
using EngineNS.Bricks.Animation.Skeleton;
using Macross;
using McLogicStateMachineEditor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace McLogicStateMachineEditor
{
    public partial class McLogicFSMLinkControl : MacrossLinkControlBase
    {
        #region IContentControlHost
        public UInt64 ShowSourceInDirSerialId
        {
            get;
            private set;
        }
        public FrameworkElement GetContainerFromItem(ResourceInfo info)
        {
            return null;
        }

        public void AddResourceInfo(ResourceInfo resInfo)
        {
        }

        public void RemoveResourceInfo(ResourceInfo resInfo)
        {
        }

        public Task ShowSourcesInDir(EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData data)
        {
            return null;
        }

        public void UpdateFilter()
        {

        }
        #endregion

        //GActor mPreviewActor = null;
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempAnimationMacrossEditor");
        EngineNS.RName mSkeletonAssetName = EngineNS.RName.EmptyName;
        public EngineNS.Bricks.Animation.Skeleton.CGfxSkeleton Skeleton
        {
            get;
            set;
        }
        public ResourceInfos.McLogicStateMachineResourceInfo McLogicStateMachineResourceInfo
        {
            get { return mCurrentResourceInfo as ResourceInfos.McLogicStateMachineResourceInfo; }
        }
        Macross.ResourceInfos.MacrossResourceInfo mCurrentResourceInfo;
        public override Macross.ResourceInfos.MacrossResourceInfo CurrentResourceInfo
        {
            get => mCurrentResourceInfo;
            set
            {
                if (mCurrentResourceInfo == value)
                    return;
                mCurrentResourceInfo = value;
                var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicStateMachineResourceInfo;
                CenterDataWarpper.CenterDataName = McLogicStateMachineResourceInfo.CenterDataTypeName;
                MacrossOpPanel.SetResourceInfo(value);
                if (CSType != EngineNS.ECSType.Client)
                    return;
            }
        }
        public McLogicFSMLinkControl()
        {
            InitializeComponent();

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            mPG = PreviewScenePG;

            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;

            TransitionCtrlAssistCtrl.HostControl = this;
            var noUse = TransitionCtrlAssistCtrl.Initialize();
            TransitionEventCtrlAssistCtrl.HostControl = this;
            var noUse2 = TransitionEventCtrlAssistCtrl.Initialize();
            //NodesCtrlAssist.NodesControl.OnFilterContextMenu = EventGraphNodesControl_FilterContextMenu;
        }

        public CodeDomNode.AI.CenterDataWarpper CenterDataWarpper = new CodeDomNode.AI.CenterDataWarpper();
        public class LogicFSMProperty
        {
            McLogicFSMLinkControl mLFSMCtrl = null;
            EngineNS.RName mCenterData = EngineNS.RName.EmptyName;
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.GamePlay.Actor.GCenterData))]
            public EngineNS.RName CenterData
            {
                get => mCenterData;
                set
                {
                    mCenterData = value;
                    mLFSMCtrl.McLogicStateMachineResourceInfo.CenterDataTypeName = value;
                    mLFSMCtrl.CenterDataWarpper.CenterDataName = value;
                }
            }
            public LogicFSMProperty()
            {

            }
            public LogicFSMProperty(McLogicFSMLinkControl ctrl)
            {
                mLFSMCtrl = ctrl;
                if (ctrl != null && ctrl.McLogicStateMachineResourceInfo != null && ctrl.McLogicStateMachineResourceInfo.CenterDataTypeName != EngineNS.RName.EmptyName)
                    mCenterData = ctrl.McLogicStateMachineResourceInfo.CenterDataTypeName;
            }
        }
        public override void OnSelectNull(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            mPG.Instance = new LogicFSMProperty(this);
        }
        LFSMTransitionNodeControl mCurrentSelectLATransitionNodeControl = null;
        public override void OnSelectNodeControl(BaseNodeControl node)
        {
            if (node == null)
                return;
            if (node is LFSMTransitionNodeControl)
            {
                TransitionDetailsPG.Instance = node.GetShowPropertyObject();
                var latrasiton = node as LFSMTransitionNodeControl;

                if (latrasiton.CtrlValueLinkHandle.GetLinkInfosCount() > 0)
                {
                    var latLinkInfo = latrasiton.CtrlValueLinkHandle.GetLinkInfo(0) as ClickableLinkInfo;
                    latLinkInfo.SelectLinkInfo();
                    var noUse1 = OpenLATransitionGraph(latrasiton.CtrlValueLinkHandle.GetLinkInfo(0));
                    var noUse2 = OpenTransitionExecuteGraph(latrasiton.CtrlValueLinkHandle.GetLinkInfo(0));
                }

                if (mCurrentSelectLATransitionNodeControl != null)
                {
                    mCurrentSelectLATransitionNodeControl.IsSelected = false;
                }
                mCurrentSelectLATransitionNodeControl = latrasiton;
                latrasiton.IsSelected = true;
            }
            else
            {
                ShowItemPropertys(node.GetShowPropertyObject());
            }
        }
        public override void OnUnSelectNodes(List<BaseNodeControl> nodes)
        {

        }
        public override void OnDoubleCliclLinkInfo(LinkInfo linkInfo)
        {
            
        }
        public override void OnSelectedLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (linkInfo.m_linkFromObjectInfo.HostNodeControl is LFSMTransitionNodeControl)
            {
                var transition = linkInfo.m_linkFromObjectInfo.HostNodeControl as LFSMTransitionNodeControl;
                TransitionDetailsPG.Instance = transition.GetShowPropertyObject();
                var laTCtrl = linkInfo.m_linkFromObjectInfo.HostNodeControl as LFSMTransitionNodeControl;
                if (mCurrentSelectLATransitionNodeControl != null)
                {
                    mCurrentSelectLATransitionNodeControl.IsSelected = false;
                }
                mCurrentSelectLATransitionNodeControl = laTCtrl;
                laTCtrl.IsSelected = true;
            }
            else
            {
                if(linkInfo.m_linkFromObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
                {
                    var linkNode = linkInfo.m_linkFromObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                    if(linkNode.IsSelfGraphNode)
                    {
                        TransitionDetailsPG.Instance = null;
                        //TransitionDetailsPG.Instance = linkNode.GetTransitionCrossfadeShowPropertyObject(linkInfo.m_linkToObjectInfo.HostNodeControl.Id);
                    }
                }
            }
            var noUse1 = OpenLATransitionGraph(linkInfo);
            var noUse2 = OpenTransitionExecuteGraph(linkInfo);
        }
        NodesControlAssist mLAGControlAssist = null;
        public async Task<NodesControlAssist> ShowFSMNodesContainer(INodesContainerDicKey graphKey)
        {
            var ctrl = await base.ShowNodesContainer(graphKey);
            {
                mLAGControlAssist = ctrl;
                ctrl.NodesControl.TypeString = "FSM";
                ctrl.NodesControl.OnFilterContextMenu = LogicFSMGraphNodesControl_FilterContextMenu;
                for (int i = 0; i < ctrl.NodesControl.CtrlNodeList.Count; ++i)
                {
                    if (ctrl.NodesControl.CtrlNodeList[i] is LogicFSMNodeControl)
                    {
                        var clip = ctrl.NodesControl.CtrlNodeList[i] as LogicFSMNodeControl;
                        clip.CenterDataWarpper.CenterDataName = CenterDataWarpper.CenterDataName;
                    }
                }
            }
            return ctrl;
        }
        protected CodeGenerateSystem.Controls.NodesContainerControl mTransitoExecutenNodesContainer;
        bool mOpenintTransitionExecuteGraph = false;
        async System.Threading.Tasks.Task OpenTransitionExecuteGraph(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (mOpenintTransitionExecuteGraph)
                return;
            mOpenintTransitionExecuteGraph = true;
            mTransitoExecutenNodesContainer = await GetTransitionExecuteGraph(linkInfo);
            TransitionEventCtrlAssistCtrl.ShowNodesContainer(mTransitoExecutenNodesContainer);
            mTransitoExecutenNodesContainer.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
            mOpenintTransitionExecuteGraph = false;
        }
        public async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetTransitionExecuteGraph(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var title = "TransitedExecuteMethod_" + linkInfo.m_linkFromObjectInfo.HostNodeControl.NodeName + "_To_" + linkInfo.m_linkToObjectInfo.HostNodeControl.NodeName;
            var tempFile = "TransitedExecuteMethod_" + GetTransitionFileName(linkInfo);

            var absFile = $"{CurrentResourceInfo.ResourceName.Address}/{tempFile}_{CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
            CodeGenerateSystem.Controls.NodesContainerControl linkContainer = null;
            if (!mLinkContainersDic.TryGetValue(tempFile, out linkContainer))
            {
                var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
                linkContainer = new CodeGenerateSystem.Controls.NodesContainerControl();
                if (linkXndHolder == null)
                {
                    mTransitoExecutenNodesContainer = linkContainer;
                    {
                        var miAssist = new CodeDomNode.CustomMethodInfo();
                        miAssist.MethodName = tempFile + "_OnTransition";
                        miAssist.DisplayName = "OnTransition";
                        var nodeType = typeof(CodeDomNode.MethodCustom);
                        var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                        {
                            CSType = CSType,
                            HostNodesContainer = mTransitoExecutenNodesContainer,
                            ConstructParam = "",
                            IsShowProperty = false,
                            MethodInfo = miAssist,
                        };
                        var node = mTransitoExecutenNodesContainer.AddOrigionNode(nodeType, csParam, 0, 100);
                        node.IsDeleteable = false;
                        node.NodeNameAddShowNodeName = false;
                    }
                }
                else
                {
                    await linkContainer.Load(linkXndHolder.Node);
                }
                await TransitionEventCtrlAssistCtrl.InitializeNodesContainer(linkContainer);
                linkContainer.TypeString = "TransitionEvent";
                linkContainer.TitleString = title;
                mLinkContainersDic.Add(tempFile, linkContainer);
            }
            linkContainer.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
            return linkContainer;
        }
        protected CodeGenerateSystem.Controls.NodesContainerControl mTransitonNodesContainer;

        public CodeGenerateSystem.Controls.NodesContainerControl TransitonNodesContainer
        {
            get => mTransitonNodesContainer;
            set => mTransitonNodesContainer = value;
        }
        Dictionary<string, CodeGenerateSystem.Controls.NodesContainerControl> mLinkContainersDic = new Dictionary<string, CodeGenerateSystem.Controls.NodesContainerControl>();
        public async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetLATransitionGraph(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var title = linkInfo.m_linkFromObjectInfo.HostNodeControl.NodeName + " -> " + linkInfo.m_linkToObjectInfo.HostNodeControl.NodeName;

            var tempFile = "Transition_" + GetTransitionFileName(linkInfo);

            var absFile = $"{CurrentResourceInfo.ResourceName.Address}/{tempFile}_{CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
            CodeGenerateSystem.Controls.NodesContainerControl linkContainer = null;
            if (!mLinkContainersDic.TryGetValue(tempFile, out linkContainer))
            {
                var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
                linkContainer = new CodeGenerateSystem.Controls.NodesContainerControl();
                if (linkXndHolder == null)
                {
                    await InitializeTransitonNodesContainer(linkContainer, linkInfo);
                }
                else
                {
                    await linkContainer.Load(linkXndHolder.Node);
                }
                await TransitionCtrlAssistCtrl.InitializeNodesContainer(linkContainer);
                linkContainer.TypeString = "Transition";
                linkContainer.TitleString = title;
                mLinkContainersDic.Add(tempFile, linkContainer);
            }
            linkContainer.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
            return linkContainer;
        }
        public override string GetGraphFileName(string graphName)
        {
            var item = GetGraphCategoryItem(graphName);
            if (item == null)
            {
                return base.GetGraphFileName(graphName);
            }
            else
            {
                return $"{CurrentResourceInfo.ResourceName.Address}/link_{item.Id.ToString()}_{CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
            }
        }
        string GetTransitionFileName(LinkInfo info)
        {
            string from, to;
            if (info.m_linkFromObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LFSMTransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LFSMTransitionNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString() + "_" + ((UInt64)info.m_linkFromObjectInfo.HostNodeControl.Id.GetHashCode()).ToString();
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString() + "_" + ((UInt64)info.m_linkFromObjectInfo.HostNodeControl.Id.GetHashCode()).ToString();
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                to = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMNodeControl;
                to = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString() + "_" + ((UInt64)info.m_linkToObjectInfo.HostNodeControl.Id.GetHashCode()).ToString();
            }
            return from + "__To__" + to;
        }
        bool mOpeningLATransitionGraph = false;
        async System.Threading.Tasks.Task OpenLATransitionGraph(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (mOpeningLATransitionGraph)
                return;
            mOpeningLATransitionGraph = true;
            mTransitonNodesContainer =await GetLATransitionGraph(linkInfo);
            TransitionCtrlAssistCtrl.ShowNodesContainer(mTransitonNodesContainer);
            mTransitonNodesContainer.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
            mOpeningLATransitionGraph = false;
        }
        async System.Threading.Tasks.Task InitializeTransitonNodesContainer(NodesContainerControl nodesContainerControl, CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            // 读取graph
            //var tempFile = TransitionCtrlAssistCtrl.GetGraphFileName(assist.LinkedCategoryItemName);
            //var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(tempFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            bool bLoaded = false;
            //if (linkXndHolder != null)
            //{
            //    var linkNode = linkXndHolder.Node.FindNode("SubLinks");
            //    var idStr = Id.ToString();
            //    foreach (var node in linkNode.GetNodes())
            //    {
            //        if (node.GetName() == idStr)
            //        {
            //            mTransitonNodesContainer.Load(node);
            //            bLoaded = true;
            //            break;
            //        }
            //    }
            //}
            if (bLoaded)
            {

            }
            else
            {
                var csParam = new LFSMFinalTransitionResultConstructionParams()
                {
                    CSType = CSType,
                    NodeName = "Result",
                    HostNodesContainer = nodesContainerControl,
                    ConstructParam = "",
                };
                //var node = mTransitonNodesContainer.AddOrigionNode(typeof(FinalTransitionResultControl), csParam, 50, 0) as FinalTransitionResultControl;
                var node = nodesContainerControl.AddNodeControl(typeof(LFSMFinalTransitionResultControl), csParam, 380, 100) as LFSMFinalTransitionResultControl;
                node.IsDeleteable = false;


            }
        }

        public override async Task Load()
        {
            if (CurrentResourceInfo == null)
                return;
            var file = $"{CurrentResourceInfo.ResourceName.Address}/data_{CSType.ToString()}{EngineNS.CEngineDesc.MacrossExtension}";
            await LoadData(file);

            // 读取MainGraph连线
            await NodesCtrlAssist.Load();
            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.NodesControl.OnFilterContextMenu = GraphNodesControl_FilterContextMenu;
            TransitionCtrlAssistCtrl.NodesControl.OnFilterContextMenu = TransitionGraphNodesControl_FilterContextMenu;
            TransitionCtrlAssistCtrl.NodesControl.TypeString = "Transition";
        }
        public override void Save()
        {
            base.Save();
            using (var it = mLinkContainersDic.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var linkContainer = it.Current.Value;
                    var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
                    linkContainer.Save(xnd.Node);
                    var absFile = $"{CurrentResourceInfo.ResourceName.Address}/{it.Current.Key}_{CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
                    EngineNS.IO.XndHolder.SaveXND(absFile, xnd);
                }
            }
        }
       
        #region FilterContextMenu
        public void OnAnimAdded(EngineNS.RName skeletonAsset)
        {
            // Skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, skeletonAsset);

        }
        private void LogicFSMGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var assist = filterData.HostContainerControl.HostControl as NodesControlAssist;
            //NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            nodesList.ClearNodes();
            var lcCP = new LogicFSMNodeControlConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                NodeName = "LogicAnimClip",
                LinkedCategoryItemID = GetLFSMGraphCategoryItem(assist.LinkedCategoryItemName).Id,
                OnAdded = OnAnimAdded,
            };
            lcCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(LogicFSMNodeControl), "LogicClip", lcCP, "");
            //search LAGraphList to add LAGNode expect self
            if (MacrossOpPanel.CategoryDic.ContainsKey(McLogicFSMMacrossPanel.LogicStateMachineCategoryName))
            {
                var cat = MacrossOpPanel.CategoryDic[McLogicFSMMacrossPanel.LogicStateMachineCategoryName];

                for (int i = 0; i < cat.Items.Count; ++i)
                {
                    var lagCP = new LogicFSMGraphNodeControlConstructionParams()
                    {
                        CSType = CSType,
                        ConstructParam = cat.Items[i].Name,
                        LAGNodeName = cat.Items[i].Name,
                        DisplayName = cat.Items[i].Name,
                        LinkedCategoryItemID = cat.Items[i].Id,
                        IsSelfGraphNode = false,
                    };
                    if (assist.LinkedCategoryItemName != cat.Items[i].Name)
                        nodesList.AddNodesFromType(filterData, typeof(LogicFSMGraphNodeControl), "FSMLinkNode", lagCP, "");
                    AddNodes(0, filterData, nodesList, cat.Items[i].Children);
                }
            }

        }
        private CategoryItem GetGraphCategoryItem(string linkNodeName)
        {
            using (var it = MacrossOpPanel.CategoryDic.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var cat = it.Current.Value;
                    for (int i = 0; i < cat.Items.Count; ++i)
                    {
                        var result = GetGraphCategoryItem(cat.Items[i], linkNodeName);
                        if (result != null)
                            return result;
                    }
                }
            }

            return null;
        }
        private CategoryItem GetGraphCategoryItem(CategoryItem item, string linkNodeName)
        {
            if (item.Name == linkNodeName)
            {
                return item;
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                var result = GetGraphCategoryItem(item.Children[i], linkNodeName);
                if (result != null)
                    return result;
            }
            return null;
        }
        private CategoryItem GetLFSMGraphCategoryItem(string linkNodeName)
        {
            var cat = MacrossOpPanel.CategoryDic[McLogicFSMMacrossPanel.LogicStateMachineCategoryName];
            for (int i = 0; i < cat.Items.Count; ++i)
            {
                var result = GetGraphCategoryItem(cat.Items[i], linkNodeName);
                if (result != null)
                    return result;
            }
            return null;
        }
        public void AddNodes(int depth, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData, CodeGenerateSystem.Controls.NodeListControl nodesList, System.Collections.ObjectModel.ObservableCollection<CategoryItem> items)
        {
            var assist = filterData.HostContainerControl.HostControl as NodesControlAssist;
            var pre = "";
            depth++;
            for (int i = 0; i < depth; ++i)
            {
                pre += "  ";
            }
            for (int i = 0; i < items.Count; ++i)
            {
                var lagCP = new LogicFSMGraphNodeControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = items[i].Name,
                    LAGNodeName = items[i].Name,
                    DisplayName = items[i].Name,
                    LinkedCategoryItemID = GetLFSMGraphCategoryItem(assist.LinkedCategoryItemName).Id,
                    IsSelfGraphNode = false,
                };
                if (mLAGControlAssist.LinkedCategoryItemName != items[i].Name)
                    nodesList.AddNodesFromType(filterData, typeof(LogicFSMGraphNodeControl), pre + "FSMLinkNode", lagCP, "");
                AddNodes(depth, filterData, nodesList, items[i].Children);
            }
        }
        private void TransitionGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicStateMachineResourceInfo;
            NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            var cdvCP = new CodeDomNode.CenterDataValueControl.CenterDataValueControlConstructParam()
            {
                CSType = CSType,
                ConstructParam = "",
            };
            cdvCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CenterDataValueControl), "CenterDataValue", cdvCP, "");
        }
        private void GraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            if (Skeleton == null)
                return;
            foreach (var notify in Skeleton.Notifies)
            {
                var validName = System.Text.RegularExpressions.Regex.Replace(notify.Name, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "_");
                var methodInfo = new CodeDomNode.CustomMethodInfo();
                methodInfo.MethodName = "Anim_Notify_" + validName;
                var fucParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                fucParam.ParamType = new CodeDomNode.VariableType(notify.Type, CSType);
                fucParam.ParamName = "sender";
                methodInfo.InParams.Add(fucParam);
                var notifyCP = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    MethodInfo = methodInfo,
                    IsShowProperty = false,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.MethodCustom), "AnimNotify/Anim_Notify_" + notify.Name, notifyCP, "");
            }
            var cdvCP = new CodeDomNode.CenterDataValueControl.CenterDataValueControlConstructParam()
            {
                CSType = CSType,
                ConstructParam = "",
            };
            cdvCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CenterDataValueControl), "CenterDataValue", cdvCP, "");
        }
        #endregion
    }
}
