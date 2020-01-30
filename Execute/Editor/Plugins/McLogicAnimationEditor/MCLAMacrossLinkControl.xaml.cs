using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EditorCommon;
using EditorCommon.Resources;
using EngineNS.Bricks.Animation.Skeleton;
using Macross;
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

namespace McLogicAnimationEditor
{
    /// <summary>
    /// Interaction logic for AnimationMacrossLinkControl.xaml
    /// </summary>
    public partial class MCLAMacrossLinkControl : MacrossLinkControlBase, EditorCommon.Controls.ResourceBrowser.IContentControlHost
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
        public ResourceInfo[] GetSelectedResourceInfos()
        {
            return ContentBrowser?.GetSelectedResourceInfos();
        }
        public void SelectResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            ContentBrowser?.SelectResourceInfos(resInfo);
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
        public ResourceInfos.McLogicAnimationResourceInfo McLogicAnimationResourceInfo
        {
            get { return mCurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo; }
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
                var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo;
                mSkeletonAssetName = EngineNS.RName.GetRName(animRinfo.SkeletonAsset);
                Skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, EngineNS.RName.GetRName(animRinfo.SkeletonAsset));
                //mAnimationInstance.Get().Init();
                CenterDataWarpper.CenterDataName = McLogicAnimationResourceInfo.CenterDataTypeName;
                MacrossOpPanel.SetResourceInfo(value);
                var noUse = InitContentBrowserItems();
                if (CSType != EngineNS.ECSType.Client)
                    return;
                Action action = async () =>
                 {
                     await mPreviewSceneControl.Initialize(mSceneName);
                     ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                     await EditorCommon.Utility.PreviewHelper.GetPreviewMeshBySkeleton(animRinfo);
                     if (!string.IsNullOrEmpty(animRinfo.PreViewMesh))
                     {
                         await RefreshPreviewActor(animRinfo.PreViewMesh);
                     }
                 };
                action();
            }
        }
        public async Task RefreshPreviewActor(string preViewMesh)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
            {
                var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo;
                mPreviewActor = await mPreviewSceneControl.CreateUniqueActor(EngineNS.RName.GetRName(preViewMesh));
                mPreviewActor.CreateCenterData(animRinfo.CenterDataTypeName);
                mPreviewActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
                var laCom = new EngineNS.GamePlay.Component.GLogicAnimationComponent();
                var init = new EngineNS.GamePlay.Component.GLogicAnimationComponentInitializer();
                init.OnlyForGame = false;
                init.ComponentMacross = CurrentResourceInfo.ResourceName;
                mPreviewActor.AddComponent(laCom);
                await laCom.SetInitializer(EngineNS.CEngine.Instance.RenderContext, mPreviewActor, mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>(), init);
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    AnimPG.Instance = laCom.McComponent;
                    return null;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Logic);


        }
        public void ResetPreview()
        {
            var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo;
            var noUse = RefreshPreviewActor(animRinfo.PreViewMesh);
            //var animInfo = mCurrentResourceInfo as ResourceInfos.AnimationMacrossResourceInfo;
            //mPreviewActor.RemoveComponent(mAnimationInstance.Get().SpecialName);
            //mAnimationInstance = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<EngineNS.GamePlay.Component.GMacrossAnimationComponent>
            //        (
            //        animInfo.ResourceName
            //        );
            //mAnimationInstance.Get().SkeletonAssetName = EngineNS.RName.GetRName(animInfo.SkeletonAsset);
            //mPreviewActor.AddComponent(mAnimationInstance.Get());
            //var meshComp = mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            //if (meshComp != null)
            //{
            //    var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            //    skinModifier.AnimationPoseProxy = mAnimationInstance.Get().AnimationPoseProxy;
            //}
            //mAnimationInstance.Get().Init();
        }

        EditorCommon.ViewPort.PreviewSceneControl mPreviewSceneControl = null;
        EngineNS.GamePlay.Actor.GActor mPreviewActor = null;
        public MCLAMacrossLinkControl()
        {
            InitializeComponent();

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            mPG = PreviewScenePG;

            mPreviewSceneControl = new EditorCommon.ViewPort.PreviewSceneControl();
            ViewportDock.Content = mPreviewSceneControl.ViewPort;

            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;

            TransitionCtrlAssistCtrl.HostControl = this;
            var noUse1 = TransitionCtrlAssistCtrl.Initialize();
            TransitionEventCtrlAssistCtrl.HostControl = this;
            var noUse2 = TransitionEventCtrlAssistCtrl.Initialize();
            //NodesCtrlAssist.NodesControl.OnFilterContextMenu = EventGraphNodesControl_FilterContextMenu;


        }

        public CodeDomNode.AI.CenterDataWarpper CenterDataWarpper = new CodeDomNode.AI.CenterDataWarpper();
        public class LogicAnimationProperty
        {
            MCLAMacrossLinkControl mLACtrl = null;
            EngineNS.RName mCenterData = EngineNS.RName.EmptyName;
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.GamePlay.Actor.GCenterData))]
            public EngineNS.RName CenterData
            {
                get => mCenterData;
                set
                {
                    mCenterData = value;
                    mLACtrl.McLogicAnimationResourceInfo.CenterDataTypeName = value;
                    mLACtrl.CenterDataWarpper.CenterDataName = value;
                }
            }
            public LogicAnimationProperty()
            {

            }
            public LogicAnimationProperty(MCLAMacrossLinkControl ctrl)
            {
                mLACtrl = ctrl;
                if (ctrl != null && ctrl.McLogicAnimationResourceInfo != null && ctrl.McLogicAnimationResourceInfo.CenterDataTypeName != EngineNS.RName.EmptyName)
                    mCenterData = ctrl.McLogicAnimationResourceInfo.CenterDataTypeName;
            }
        }
        public override void OnSelectNull(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            mPG.Instance = new LogicAnimationProperty(this);
        }
        LATransitionNodeControl mCurrentSelectLATransitionNodeControl = null;
        public override void OnSelectNodeControl(BaseNodeControl node)
        {
            if (node == null)
                return;
            if (node is LATransitionNodeControl)
            {
                TransitionDetailsPG.Instance = node.GetShowPropertyObject();
                var latrasiton = node as LATransitionNodeControl;

                if (latrasiton.CtrlValueLinkHandle.GetLinkInfosCount() > 0)
                {
                    var latLinkInfo = latrasiton.CtrlValueLinkHandle.GetLinkInfo(0) as ClickableLinkInfo;
                    latLinkInfo.SelectLinkInfo();
                    var noUse = OpenLATransitionGraph(latrasiton.CtrlValueLinkHandle.GetLinkInfo(0));
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
            if (linkInfo.m_linkFromObjectInfo.HostNodeControl is LATransitionNodeControl)
            {
                var transition = linkInfo.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
                TransitionDetailsPG.Instance = transition.GetTransitionCrossfadeShowPropertyObject(linkInfo.m_linkToObjectInfo.HostNodeControl.Id); ;
                var laTCtrl = linkInfo.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
                if (mCurrentSelectLATransitionNodeControl != null)
                {
                    mCurrentSelectLATransitionNodeControl.IsSelected = false;
                }
                mCurrentSelectLATransitionNodeControl = laTCtrl;
                laTCtrl.IsSelected = true;
            }
            else
            {
                if (linkInfo.m_linkFromObjectInfo.HostNodeControl is LAGraphNodeControl)
                {
                    var linkNode = linkInfo.m_linkFromObjectInfo.HostNodeControl as LAGraphNodeControl;
                    if (linkNode.IsSelfGraphNode)
                    {
                        TransitionDetailsPG.Instance = null;
                        //TransitionDetailsPG.Instance = linkNode.GetTransitionCrossfadeShowPropertyObject(linkInfo.m_linkToObjectInfo.HostNodeControl.Id);
                    }
                }
            }
            var noUse = OpenLATransitionGraph(linkInfo);
            var noUse2 = OpenTransitionExecuteGraph(linkInfo);
        }
        NodesControlAssist mLAGControlAssist = null;
        public override async Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey)
        {
            var ctrl = await base.ShowNodesContainer(graphKey);
            //ctrl.OnSelectedLinkInfo -= Ctrl_OnNodesContainerSelectedLinkInfo;
            //ctrl.OnSelectedLinkInfo += Ctrl_OnNodesContainerSelectedLinkInfo;
            if (graphKey.CategoryItemType == CategoryItem.enCategoryItemType.LogicAnimGraph)
            {
                mLAGControlAssist = ctrl;
                ctrl.NodesControl.TypeString = "Logic";
                ctrl.NodesControl.OnFilterContextMenu = LogicAnimGraphNodesControl_FilterContextMenu;
                for (int i = 0; i < ctrl.NodesControl.CtrlNodeList.Count; ++i)
                {
                    if (ctrl.NodesControl.CtrlNodeList[i] is LAClipNodeControl)
                    {
                        var clip = ctrl.NodesControl.CtrlNodeList[i] as LAClipNodeControl;
                        if (clip.SkeletonAsset == "")
                            clip.SkeletonAsset = (CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo).SkeletonAsset;
                        clip.CenterDataWarpper.CenterDataName = CenterDataWarpper.CenterDataName;
                    }
                }
            }
            if (graphKey.CategoryItemType == CategoryItem.enCategoryItemType.LogicAnimPostProcess)
            {
                mLAGControlAssist = ctrl;
                ctrl.NodesControl.TypeString = "Logic";
                ctrl.NodesControl.OnFilterContextMenu = LogicAnimPostProcessNodesControl_FilterContextMenu;
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
                await NodesCtrlAssistCtrl.InitializeNodesContainer(linkContainer);
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
            if (info.m_linkFromObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAGraphNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LATransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString() + "_" + ((UInt64)info.m_linkFromObjectInfo.HostNodeControl.Id.GetHashCode()).ToString();
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAClipNodeControl;
                from = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString() + "_" + ((UInt64)info.m_linkFromObjectInfo.HostNodeControl.Id.GetHashCode()).ToString();
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAGraphNodeControl;
                to = ((UInt64)gNode.LinkedCategoryItemID.GetHashCode()).ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAClipNodeControl;
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
            mTransitonNodesContainer =await GetLATransitionGraph(linkInfo); ;
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
                var csParam = new LAFinalTransitionResultConstructionParams()
                {
                    CSType = CSType,
                    NodeName = "Result",
                    HostNodesContainer = nodesContainerControl,
                    ConstructParam = "",
                };
                //var node = mTransitonNodesContainer.AddOrigionNode(typeof(FinalTransitionResultControl), csParam, 50, 0) as FinalTransitionResultControl;
                var node = nodesContainerControl.AddNodeControl(typeof(LAFinalTransitionResultControl), csParam, 380, 100) as LAFinalTransitionResultControl;
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
        async Task RefreshReference(CategoryItem item, Macross.ResourceInfos.MacrossResourceInfo info)
        {

            var ctrlAssist = await GetNodesContainer(item, true);
            var linkContainer = ctrlAssist.NodesControl;
            for (int i = 0; i < linkContainer.CtrlNodeList.Count; ++i)
            {
                if (linkContainer.CtrlNodeList[i] is LAClipNodeControl)
                {
                    var ctrl = linkContainer.CtrlNodeList[i] as LAClipNodeControl;
                    if (ctrl.Animation != EngineNS.RName.EmptyName && !info.ReferenceRNameList.Contains(ctrl.Animation))
                    {
                        info.ReferenceRNameList.Add(ctrl.Animation);
                    }
                    if (ctrl.RefAnimation != EngineNS.RName.EmptyName && !info.ReferenceRNameList.Contains(ctrl.RefAnimation))
                    {
                        info.ReferenceRNameList.Add(ctrl.RefAnimation);
                    }
                    await RefreshReference(ctrl.LinkedNodesContainer, info);
                }
            }
            await RefreshReference(linkContainer, info);
            foreach (var child in item.Children)
            {
                await RefreshReference(child, info);
            }

        }
        public async Task RefreshReference(Macross.ResourceInfos.MacrossResourceInfo info)
        {

            foreach (var cat in MacrossOpPanel.CategoryDic)
            {
                foreach (var item in cat.Value.Items)
                {
                    await RefreshReference(item, info);
                }
            }
        }
        async Task RefreshReference(NodesContainerControl linkContainer, Macross.ResourceInfos.MacrossResourceInfo info)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            for (int i = 0; i < linkContainer.CtrlNodeList.Count; ++i)
            {
                if (linkContainer.CtrlNodeList[i] is LAAnimPoseStreamControl)
                {
                    var ctrl = linkContainer.CtrlNodeList[i] as LAAnimPoseStreamControl;
                    if (ctrl.FileName != EngineNS.RName.EmptyName && !info.ReferenceRNameList.Contains(ctrl.FileName))
                    {
                        info.ReferenceRNameList.Add(ctrl.FileName);
                    }
                }
                if (linkContainer.CtrlNodeList[i] is LABlendSpaceControl)
                {
                    var ctrl = linkContainer.CtrlNodeList[i] as LABlendSpaceControl;
                    if (ctrl.FileName != EngineNS.RName.EmptyName && !info.ReferenceRNameList.Contains(ctrl.FileName))
                    {
                        info.ReferenceRNameList.Add(ctrl.FileName);
                    }
                }
                if (linkContainer.CtrlNodeList[i] is LAAdditiveBlendSpaceControl)
                {
                    var ctrl = linkContainer.CtrlNodeList[i] as LAAdditiveBlendSpaceControl;
                    if (ctrl.FileName != EngineNS.RName.EmptyName && !info.ReferenceRNameList.Contains(ctrl.FileName))
                    {
                        info.ReferenceRNameList.Add(ctrl.FileName);
                    }
                }
            }
        }
        bool CreateNodeListAttribute(EditorCommon.Resources.ResourceInfo info)
        {
            var previewInfo = info as IResourceInfoPreviewForEditor;
            if (previewInfo == null)
                return false;
            if (previewInfo.SkeletonAsset != McLogicAnimationResourceInfo.SkeletonAsset)
                return false;
            var ty = info.ResourceType;
            var nodeListAttribute = info as EditorCommon.CodeGenerateSystem.INodeListAttribute;
            if (info.GetType() == typeof(EditorCommon.ResourceInfos.AnimationClipResourceInfo))
            {
                var csParam = new LAAnimPoseStreamControlConstructionParams();
                csParam.CSType = NodesCtrlAssist.CSType;
                csParam.NodeName = info.ResourceName.PureName();
                csParam.FileName = info.ResourceName;
                csParam.OnAdded = OnAnimAdded;
                csParam.Notifies = new List<string>();
                var animSeqInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                foreach (var pair in animSeqInfo.NotifyTrackMap)
                {
                    //if(!csParam.Notifies.Contains(pair.NotifyName))
                    {
                        csParam.Notifies.Add(pair.NotifyName);
                    }
                }
                nodeListAttribute.CSParam = csParam;
                nodeListAttribute.NodeType = typeof(LAAnimPoseStreamControl);
            }
            if (info.GetType() == typeof(EditorCommon.ResourceInfos.AnimationBlendSpace1DResourceInfo))
            {
                var csParam = new LABlendSpaceControlConstructionParams();
                csParam.CSType = NodesCtrlAssist.CSType;
                csParam.NodeName = info.ResourceName.PureName();
                csParam.FileName = info.ResourceName;
                csParam.Is1D = true;
                csParam.OnAdded = OnAnimAdded;
                //var animSeqInfo = info as EditorCommon.ResourceInfos.AnimationBlendSpace1DResourceInfo;
                nodeListAttribute.CSParam = csParam;
                nodeListAttribute.NodeType = typeof(LABlendSpaceControl);
            }
            if (info.GetType() == typeof(EditorCommon.ResourceInfos.AnimationBlendSpaceResourceInfo))
            {
                var csParam = new LABlendSpaceControlConstructionParams();
                csParam.CSType = NodesCtrlAssist.CSType;
                csParam.NodeName = info.ResourceName.PureName();
                csParam.FileName = info.ResourceName;
                csParam.Is1D = false;
                csParam.OnAdded = OnAnimAdded;
                //var animSeqInfo = info as EditorCommon.ResourceInfos.AnimationBlendSpaceResourceInfo;
                nodeListAttribute.CSParam = csParam;
                nodeListAttribute.NodeType = typeof(LABlendSpaceControl);
            }
            if (info.GetType() == typeof(EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace1DResourceInfo))
            {
                var csParam = new LAAdditiveBlendSpaceControlConstructionParams();
                csParam.CSType = NodesCtrlAssist.CSType;
                csParam.NodeName = info.ResourceName.PureName();
                csParam.FileName = info.ResourceName;
                csParam.Is1D = true;
                csParam.OnAdded = OnAnimAdded;
                //var animSeqInfo = info as EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace1DResourceInfo;
                nodeListAttribute.CSParam = csParam;
                nodeListAttribute.NodeType = typeof(LAAdditiveBlendSpaceControl);
            }
            if (info.GetType() == typeof(EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace2DResourceInfo))
            {
                var csParam = new LAAdditiveBlendSpaceControlConstructionParams();
                csParam.CSType = NodesCtrlAssist.CSType;
                csParam.NodeName = info.ResourceName.PureName();
                csParam.FileName = info.ResourceName;
                csParam.Is1D = false;
                csParam.OnAdded = OnAnimAdded;
                //var animSeqInfo = info as EditorCommon.ResourceInfos.AnimationAdditiveBlendSpace2DResourceInfo;
                nodeListAttribute.CSParam = csParam;
                nodeListAttribute.NodeType = typeof(LAAdditiveBlendSpaceControl);
            }
            return true;
        }
        async System.Threading.Tasks.Task InitContentBrowserItems()
        {
            ContentBrowser.HostControl = this;
            ShowSourceInDirSerialId++;
            string[] animTyes =
                { EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip ,
                  EngineNS.Editor.Editor_RNameTypeAttribute.AnimationBlendSpace1D,
                  EngineNS.Editor.Editor_RNameTypeAttribute.AnimationBlendSpace,
                  EngineNS.Editor.Editor_RNameTypeAttribute.AnimationAdditiveBlendSpace,
                  EngineNS.Editor.Editor_RNameTypeAttribute.AnimationAdditiveBlendSpace1D,};
            await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
            {
                await EditorCommon.Utility.PreviewHelper.InitPreviewResources(ContentBrowser, animTyes, ShowSourceInDirSerialId, (info) =>
                {
                    return CreateNodeListAttribute(info);
                });
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
        }

        #region FilterContextMenu
        public void OnAnimAdded(EngineNS.RName skeletonAsset)
        {
            // Skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, skeletonAsset);

        }
        private void LogicAnimGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var assist = filterData.HostContainerControl.HostControl as NodesControlAssist;
            //NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            nodesList.ClearNodes();
            var lcCP = new CodeDomNode.Animation.LAClipNodeControlConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                NodeName = "LogicAnimClip",
                LinkedCategoryItemID = GetLAGraphCategoryItem(assist.LinkedCategoryItemName).Id,
                SkeletonAsset = (CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo).SkeletonAsset,
                OnAdded = OnAnimAdded,
            };
            lcCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(LAClipNodeControl), "LogicAnimClip", lcCP, "");
            var additiveLCCP = new CodeDomNode.Animation.LAClipNodeControlConstructionParams()
            {
                CSType = CSType,
                ConstructParam = "",
                NodeName = "AdditiveLogicAnimClip",
                LinkedCategoryItemID = GetLAGraphCategoryItem(assist.LinkedCategoryItemName).Id,
                SkeletonAsset = (CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo).SkeletonAsset,
                IsAdditive = true,
                OnAdded = OnAnimAdded,
            };
            lcCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(LAClipNodeControl), "AdditiveLogicAnimClip", lcCP, "");
            //search LAGraphList to add LAGNode expect self
            if (MacrossOpPanel.CategoryDic.ContainsKey(MacrossPanelBase.LogicAnimationGraphNodeCategoryName))
            {
                var item = GetAnimLayerCategoryItem(assist.LinkedCategoryItemName);

                for (int i = 0; i < item.Children.Count; ++i)
                {
                    var lagCP = new CodeDomNode.Animation.LAGraphNodeControlConstructionParams()
                    {
                        CSType = CSType,
                        ConstructParam = item.Children[i].Name,
                        LAGNodeName = item.Children[i].Name,
                        DisplayName = item.Children[i].Name,
                        LinkedCategoryItemID = item.Children[i].Id,
                        IsSelfGraphNode = false,
                    };
                    if (assist.LinkedCategoryItemName != item.Children[i].Name)
                        nodesList.AddNodesFromType(filterData, typeof(LAGraphNodeControl), "LALinkNode", lagCP, "");
                    AddNodes(0, filterData, nodesList, item.Children[i].Children);
                }
            }

        }
        private void LogicAnimPostProcessNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            //NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            nodesList.ClearNodes();
            {
                var cp = new CodeDomNode.Animation.LAAdditivePoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAAdditivePoseControl), "BlendPose/ApplyAdditivePose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAMakeAdditivePoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAMakeAdditivePoseControl), "BlendPose/MakeAdditivePose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByBoolControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByBoolControl), "BlendPose/SelectPoseByBool", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByIntControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByIntControl), "BlendPose/SelectPoseByInt", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByEnumControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByEnumControl), "BlendPose/SelectPoseByEnum", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABlendPoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABlendPoseControl), "BlendPose/BlendPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABlendPosePerBoneControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABlendPosePerBoneControl), "BlendPose/LayeredBlendPerBone", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAMaskPoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAMaskPoseControl), "BlendPose/MaskPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAZeroPoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAZeroPoseControl), "Pose/ZeroPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABindedPoseControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABindedPoseControl), "Pose/TPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LACCDIKControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LACCDIKControl), "SkeletonBoneControl/CCDIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAFABRIKControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAFABRIKControl), "SkeletonBoneControl/FABRIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LALookAtControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LALookAtControl), "SkeletonBoneControl/LookAt", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LALegIKControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LALegIKControl), "SkeletonBoneControl/LegIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASplineIKControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASplineIKControl), "SkeletonBoneControl/SplineIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LATwoBoneIKControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LATwoBoneIKControl), "SkeletonBoneControl/TwoBoneIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABoneDrivenControllerControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABoneDrivenControllerControl), "SkeletonBoneControl/BoneDrivenController", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAHandIKRetargetingControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAHandIKRetargetingControl), "SkeletonBoneControl/HandIKRetargeting", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAResetRootTransformControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAResetRootTransformControl), "SkeletonBoneControl/ResetRootTransform", cp, "");
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
        private CategoryItem GetLAGraphCategoryItem(string linkNodeName)
        {
            var cat = MacrossOpPanel.CategoryDic[MacrossPanelBase.LogicAnimationGraphNodeCategoryName];
            for (int i = 0; i < cat.Items.Count; ++i)
            {
                var result = GetGraphCategoryItem(cat.Items[i], linkNodeName);
                if (result != null)
                    return result;
            }
            return null;
        }
        private CategoryItem GetAnimLayerCategoryItem(string linkNodeName)
        {
            var cat = MacrossOpPanel.CategoryDic[MacrossPanelBase.LogicAnimationGraphNodeCategoryName];
            for (int i = 0; i < cat.Items.Count; ++i)
            {
                var result = GetAnimLayerCategoryItem(cat.Items[i], linkNodeName);
                if (result != null)
                    return result;
            }
            return null;
        }
        private CategoryItem GetAnimLinkNode_LayerParent(CategoryItem item)
        {
            if (item.Parent != null)
            {
                if (item.Parent.InitTypeStr == "LA_AnimLayer")
                {
                    return item.Parent;
                }
                else
                {
                    return GetAnimLinkNode_LayerParent(item.Parent);
                }
            }
            return null;
        }
        private CategoryItem GetAnimLayerCategoryItem(CategoryItem item, string linkNodeName)
        {
            if (item.Name == linkNodeName)
            {
                return GetAnimLinkNode_LayerParent(item);
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                var result = GetAnimLayerCategoryItem(item.Children[i], linkNodeName);
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
                var lagCP = new CodeDomNode.Animation.LAGraphNodeControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = items[i].Name,
                    LAGNodeName = items[i].Name,
                    DisplayName = items[i].Name,
                    LinkedCategoryItemID = GetLAGraphCategoryItem(assist.LinkedCategoryItemName).Id,
                    IsSelfGraphNode = false,
                };
                if (mLAGControlAssist.LinkedCategoryItemName != items[i].Name)
                    nodesList.AddNodesFromType(filterData, typeof(LAGraphNodeControl), pre + "LALinkNode", lagCP, "");
                AddNodes(depth, filterData, nodesList, items[i].Children);
            }
        }
        private void TransitionGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var animRinfo = CurrentResourceInfo as ResourceInfos.McLogicAnimationResourceInfo;
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
                fucParam.ParamType = new CodeDomNode.VariableType(typeof(EngineNS.Bricks.Animation.Notify.CGfxNotify), CSType);
                fucParam.Attributes.Add(new EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute(notify.Type));
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
