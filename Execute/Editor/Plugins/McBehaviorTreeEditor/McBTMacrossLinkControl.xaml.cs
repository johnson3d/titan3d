using CodeDomNode.AI;
using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EditorCommon;
using EditorCommon.Resources;
using EngineNS.Bricks.Animation.Skeleton;
using Macross;
using McBehaviorTreeEditor.ResourceInfos;
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

namespace McBehaviorTreeEditor
{
    /// <summary>
    /// Interaction logic for AnimationMacrossLinkControl.xaml
    /// </summary>
    public partial class McBTMacrossLinkControl : MacrossLinkControlBase, EditorCommon.Controls.ResourceBrowser.IContentControlHost
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
        EngineNS.RName mSceneName = EngineNS.RName.GetRName("tempBTMacrossEditor");
        public EngineNS.Bricks.Animation.Skeleton.CGfxSkeleton Skeleton
        {
            get;
            set;
        }
        public ResourceInfos.McBehaviorTreeResourceInfo McBehaviorTreeResourceInfo
        {
            get { return mCurrentResourceInfo as ResourceInfos.McBehaviorTreeResourceInfo; }
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
                MacrossOpPanel.SetResourceInfo(value);
                BTCenterDataWarpper.CenterDataName = McBehaviorTreeResourceInfo.CenterDataTypeName;
                //var noUse = InitContentBrowserItems();
                if (CSType != EngineNS.ECSType.Client)
                    return;
                //Action action = async () =>
                // {
                //     //await mPreviewSceneControl.Initialize(mSceneName);
                //     //ProGrid_PreviewScene.Instance = mPreviewSceneControl;
                //     //await EditorCommon.Utility.PreviewHelper.GetPreviewMeshBySkeleton(animRinfo);
                //     //if (!string.IsNullOrEmpty(animRinfo.PreViewMesh))
                //     //{
                //     //    await RefreshPreviewActor(animRinfo.PreViewMesh);
                //     //}
                // };
                //action();
            }
        }
        async Task RefreshPreviewActor(string preViewMesh)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
            //{
            //    mPreviewActor = await mPreviewSceneControl.CreateUniqueActor(EngineNS.RName.GetRName(preViewMesh));
            //    mPreviewActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            //    var laCom = new EngineNS.GamePlay.Component.GLogicAnimationComponent();
            //    var init = new EngineNS.GamePlay.Component.GLogicAnimationComponentInitializer();
            //    init.OnlyForGame = false;
            //    init.ComponentMacross = CurrentResourceInfo.ResourceName;
            //    mPreviewActor.AddComponent(laCom);
            //    await laCom.SetInitializer(EngineNS.CEngine.Instance.RenderContext, mPreviewActor, mPreviewActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>(), init);
            //    EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //    {
            //        AnimPG.Instance = laCom.McComponent;
            //        return null;
            //    }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            //    return true;
            //}, EngineNS.Thread.Async.EAsyncTarget.Logic);


        }
        public void ResetPreview()
        {
            //var animRinfo = CurrentResourceInfo as ResourceInfos.McBehaviorTreeResourceInfo;
            //var noUse = RefreshPreviewActor(animRinfo.PreViewMesh);

        }
        public McBTMacrossLinkControl()
        {
            InitializeComponent();

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            mPG = PropertiesPG;
            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;
        }
        public CenterDataWarpper BTCenterDataWarpper = new CenterDataWarpper();
        public void ChangeCenterData(EngineNS.RName name)
        {
            McBehaviorTreeResourceInfo.CenterDataTypeName = name;
            BTCenterDataWarpper.CenterDataName = name;
            Action refreshData = async () =>
            {
                using (var it = MacrossOpPanel.CategoryDic.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var cat = it.Current.Value;
                        for (int i = 0; i < cat.Items.Count; ++i)
                        {
                            var assist = await GetNodesContainer(cat.Items[i], true, true);
                            for (int j = 0; j < assist.NodesControl.CtrlNodeList.Count; ++j)
                            {
                                if (assist.NodesControl.CtrlNodeList[j] is BehaviorTree_BTCenterDataControl)
                                {
                                    var cdCtrl = assist.NodesControl.CtrlNodeList[j] as BehaviorTree_BTCenterDataControl;
                                    cdCtrl.BTCenterDataWarpper.CenterDataName = name;
                                }
                            }

                        }
                    }
                }
            };
            refreshData();
        }
        public class BehaviorTreeProperty
        {
            McBTMacrossLinkControl mBTCtrl = null;
            EngineNS.RName mCenterData = EngineNS.RName.EmptyName;
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.GamePlay.Actor.GCenterData))]
            public EngineNS.RName CenterData
            {
                get => mCenterData;
                set
                {
                    mCenterData = value;
                    mBTCtrl.ChangeCenterData(value);

                }
            }
            public BehaviorTreeProperty()
            {

            }
            public BehaviorTreeProperty(McBTMacrossLinkControl ctrl)
            {
                mBTCtrl = ctrl;
                if (ctrl != null && ctrl.McBehaviorTreeResourceInfo != null && ctrl.McBehaviorTreeResourceInfo.CenterDataTypeName != EngineNS.RName.EmptyName)
                    mCenterData = ctrl.McBehaviorTreeResourceInfo.CenterDataTypeName;
            }
        }
        public override void OnSelectNull(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            mPG.Instance = new BehaviorTreeProperty(this);
        }
        public override void OnSelectNodeControl(BaseNodeControl node)
        {
            if (node == null)
                return;
            if (node is LATransitionNodeControl)
            {
                //TransitionDetailsPG.Instance = node.GetShowPropertyObject();
                //var latrasiton = node as LATransitionNodeControl;

                //if (latrasiton.CtrlValueLinkHandle.GetLinkInfosCount() > 0)
                //{
                //    var latLinkInfo = latrasiton.CtrlValueLinkHandle.GetLinkInfo(0) as ClickableLinkInfo;
                //    latLinkInfo.SelectLinkInfo();
                //    var noUse = OpenLATransitionGraph(latrasiton.CtrlValueLinkHandle.GetLinkInfo(0));
                //}

                //if (mCurrentSelectLATransitionNodeControl != null)
                //{
                //    mCurrentSelectLATransitionNodeControl.IsSelected = false;
                //}
                //mCurrentSelectLATransitionNodeControl = latrasiton;
                //latrasiton.IsSelected = true;
            }
            else
            {
                ShowItemPropertys(node.GetShowPropertyObject());
            }
        }
        public override void OnUnSelectNodes(List<BaseNodeControl> nodes)
        {

        }
        public override void OnSelectedLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            //if (linkInfo.m_linkFromObjectInfo.HostNodeControl is LATransitionNodeControl)
            //{
            //    TransitionDetailsPG.Instance = linkInfo.m_linkFromObjectInfo.HostNodeControl.GetShowPropertyObject();
            //    var laTCtrl = linkInfo.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
            //    if (mCurrentSelectLATransitionNodeControl != null)
            //    {
            //        mCurrentSelectLATransitionNodeControl.IsSelected = false;
            //    }
            //    mCurrentSelectLATransitionNodeControl = laTCtrl;
            //    laTCtrl.IsSelected = true;
            //}
            //else
            //    TransitionDetailsPG.Instance = null;
            //var noUse = OpenLATransitionGraph(linkInfo);
        }
        NodesControlAssist mLAGControlAssist = null;
        public override async Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey)
        {
            DockControl.Controls.DockAbleTabControl tabCtrl = null;
            DockControl.Controls.DockAbleContainerControl dockContainer = null;
            if (mNodesContainerDic.Count > 0)
            {
                foreach (var data in mNodesContainerDic)
                {
                    var parent = EditorCommon.Program.GetParent(data.Value, typeof(DockControl.Controls.DockAbleTabControl)) as DockControl.Controls.DockAbleTabControl;
                    if (parent == null)
                        continue;

                    tabCtrl = parent;
                    dockContainer = EditorCommon.Program.GetParent(parent, typeof(DockControl.Controls.DockAbleContainerControl)) as DockControl.Controls.DockAbleContainerControl;
                    break;
                }
            }

            var ctrl = await GetNodesContainer(graphKey, true, true);

            if (tabCtrl != null)
            {
                var parentTabItem = EditorCommon.Program.GetParent(ctrl, typeof(DockControl.Controls.DockAbleTabItem)) as DockControl.Controls.DockAbleTabItem;
                if (parentTabItem == null)
                {
                    var tabItem = new DockControl.Controls.DockAbleTabItem()
                    {
                        Content = ctrl,
                    };
                    tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.HeaderProperty, new Binding("ShowName") { Source = graphKey, Mode = BindingMode.TwoWay });
                    tabItem.CanClose += () =>
                    {
                        if (ctrl.IsDirty)
                        {
                            var result = EditorCommon.MessageBox.Show($"{graphKey.Name}还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                            switch (result)
                            {
                                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                                    Save();
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.No:
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                                    return false;
                            }
                        }
                        return true;
                    };
                    tabItem.OnClose += () =>
                    {
                        mNodesContainerDic.Remove(graphKey);
                    };
                    tabItem.DockGroup = dockContainer.Group;
                    dockContainer.AddChild(tabItem);
                }
                else
                    parentTabItem.IsSelected = true;
            }
            //ctrl.OnSelectedLinkInfo -= Ctrl_OnNodesContainerSelectedLinkInfo;
            //ctrl.OnSelectedLinkInfo += Ctrl_OnNodesContainerSelectedLinkInfo;
            if (graphKey.CategoryItemType == CategoryItem.enCategoryItemType.BehaviorTree)
            {
                mLAGControlAssist = ctrl;
                ctrl.NodesControl.TypeString = "BehaviorTree";
                ctrl.NodesControl.OnFilterContextMenu = BehaviorTreeGraphNodesControl_FilterContextMenu;
                for (int i = 0; i < ctrl.NodesControl.CtrlNodeList.Count; ++i)
                {
                    if (ctrl.NodesControl.CtrlNodeList[i] is BehaviorTree_BTCenterDataControl)
                    {
                        var btCD = ctrl.NodesControl.CtrlNodeList[i] as BehaviorTree_BTCenterDataControl;
                        btCD.BTCenterDataWarpper.CenterDataName = BTCenterDataWarpper.CenterDataName;
                    }
                }
            }
            return ctrl;
        }

        bool CreateNodeListAttribute(EditorCommon.Resources.ResourceInfo info)
        {
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
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
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
        private void BehaviorTreeGraphNodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            //NodesCtrlAssist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            nodesList.ClearNodes();
            var compositeEnum = typeof(CompositeNodeType);
            foreach (var typeName in compositeEnum.GetEnumNames())
            {
                {
                    var csParam = new BehaviorTree_CompositeControlConstructionParams()
                    {
                        CSType = CSType,
                        ConstructParam = "",
                        NodeName = "CompositeNode",
                        CompositeNodeType = (CompositeNodeType)Enum.Parse(compositeEnum, typeName),
                    };
                    csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                    nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_CompositeControl), "Composite/" + typeName, csParam, "");
                }
            }
            {
                var csParam = new BehaviorTree_CustomActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "CustomTask",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_CustomActionControl), "Task/CustomTask", csParam, "");
            }
            {
                var csParam = new BehaviorTree_SubTreeControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "SubTree",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_SubTreeControl), "Task/SubTree", csParam, "");
            }
            {
                var csParam = new BehaviorTree_FinishWithResultActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "FinishWithResult",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_FinishWithResultActionControl), "Task/FinishWithResult", csParam, "");
            }
            {
                var csParam = new BehaviorTree_MakeNoiseActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "MakeNoise",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_MakeNoiseActionControl), "Task/MakeNoise", csParam, "");
            }
            {
                var csParam = new BehaviorTree_MoveToActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "MoveTo",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_MoveToActionControl), "Task/MoveTo", csParam, "");
            }
            {
                var csParam = new BehaviorTree_MoveDirectlyTowardActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "MoveDirectlyToward",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_MoveDirectlyTowardActionControl), "Task/MoveDirectlyToward", csParam, "");
            }
            {
                var csParam = new BehaviorTree_PlayAnimationActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "PlayAnimation",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_PlayAnimationActionControl), "Task/PlayAnimation", csParam, "");
            }
            {
                var csParam = new BehaviorTree_PlaySoundActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "PlaySound",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_PlaySoundActionControl), "Task/PlaySound", csParam, "");
            }
            {
                var csParam = new BehaviorTree_RotateToFaceActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "RotateToFace",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_RotateToFaceActionControl), "Task/RotateToFace", csParam, "");
            }
            {
                var csParam = new BehaviorTree_WaitActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "Wait",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_WaitActionControl), "Task/Wait", csParam, "");
            }
            {
                var csParam = new BehaviorTree_WaitDataTimeActionControlConstructionParams()
                {
                    CSType = CSType,
                    ConstructParam = "",
                    NodeName = "WaitTimeByData",
                };
                csParam.BTCenterDataWarpper.CenterDataName = this.BTCenterDataWarpper.CenterDataName;
                nodesList.AddNodesFromType(filterData, typeof(BehaviorTree_WaitDataTimeActionControl), "Task/WaitTimeByData", csParam, "");
            }
        }
        #endregion
    }
}
