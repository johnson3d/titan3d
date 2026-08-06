using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.DesignMacross;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.IO;
using EngineNS.Rtti;
using EngineNS.Thread.Async;
using System;

namespace EngineNS.DesignMacross.Editor.Preview
{
    /// <summary>
    /// 动画预览面板 - 使用TtPreviewViewport渲染3D模型, TtRNameSelector选择模型资源
    /// </summary>
    public class TtAnimationPreviewPanel : ITickable, IDisposable
    {
        public EngineNS.Editor.TtPreviewViewport PreviewViewport = new EngineNS.Editor.TtPreviewViewport();
        public TtRNameSelector ModelSelector = new TtRNameSelector()
        {
            Name = "##AnimationPreviewModel",
            FilterExts = TtMaterialMesh.AssetExt,
        };
        public float PlaneScale = 5.0f;

        TtMeshNode mCurrentMeshNode = null;
        TtMeshNode PlaneMeshNode;
        TtDesignMacrossNode mDesignMacrossNode = null;
        RName mPreivewMeshName;
        RName mDesignMacross;
        EAssetState mAssetState = EAssetState.Initialized;
        bool mInitialized = false;

        public bool IsShow { get; set; }
        /// <summary>
        /// 绑定的DesignMacross RName, 用于在预览场景中创建TtDesignMacrossNode驱动动画
        /// </summary>
        public RName DesignMacross
        {
            get => mDesignMacross;
            set
            {
                mDesignMacross = value;
                // 参考TtAnimationClipEditor: 打开预览时自动加载TtDesignMacrossAMeta中保存的预览模型
                if (mPreivewMeshName == null && value != null)
                {
                    var ameta = value.AMeta as TtDesignMacrossAMeta;
                    if (ameta != null && ameta.PreviewMeshName != null)
                        LoadModel(ameta.PreviewMeshName);
                }
            }
        }

        public TtAnimationPreviewPanel()
        {
        }
        ~TtAnimationPreviewPanel()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (!mInitialized)
                return;
            mInitialized = false;
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            CoreSDK.DisposeObject(ref PreviewViewport);
            ModelSelector.Cleanup();
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            if (mInitialized)
                return true;

            await ModelSelector.Initialize();

            PreviewViewport.Title = "AnimationPreview";
            PreviewViewport.OnInitialize = Initialize_PreviewScene;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.MainRPolicyName, 0, 1);

            TtEngine.Instance.TickableManager.AddTickable(this);
            mInitialized = true;
            return true;
        }

        protected async Thread.Async.TtTask<bool> Initialize_PreviewScene(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as EngineNS.Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var aabb = new BoundingBox(3, 3, 3);
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector() + new DVector3(0, 1, 0);
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);

            var planeMaterialName = TtEngine.Instance.ConfigManager.GetConfig<EngineNS.Editor.Forms.TtMeshPrimitiveEditorConfig>().PlaneMaterialName;
            var studioContext = await PreviewViewport.CreateStudioEnvironment(aabb, PlaneScale, planeMaterialName);
            PlaneMeshNode = studioContext?.FloorNode;

            return true;
        }

        public async Task Reset()
        {
            if (!IsShow)
                return;

            Dispose();
            await Initialize();

        }

        /// <summary>
        /// 获取当前加载的模型RuntimePose, 供外部驱动动画
        /// </summary>
        public TtLocalSpaceRuntimePose GetCurrentRuntimePose()
        {
            if (mCurrentMeshNode == null)
                return null;

            return mCurrentMeshNode.RuntimePose;
        }

        /// <summary>
        /// 获取当前加载的网格节点
        /// </summary>
        public TtMeshNode GetCurrentMeshNode()
        {
            return mCurrentMeshNode;
        }

        /// <summary>
        /// 获取当前加载的模型RName
        /// </summary>
        public RName GetCurrentModelRName()
        {
            return mPreivewMeshName;
        }

        /// <summary>
        /// 加载指定RName的模型
        /// </summary>
        public void LoadModel(RName modelRName)
        {
            if (mAssetState == EAssetState.Loading)
                return;
            mPreivewMeshName = modelRName;
            if (modelRName == null)
                return;
            mAssetState = EAssetState.Loading;
            System.Action exec = async () =>
            {
                var mesh = await modelRName.GetAsset<TtMaterialMesh>();
                if (mesh == null)
                {
                    mAssetState = EAssetState.LoadFailed;
                    return;
                }
                mAssetState = EAssetState.LoadFinished;
                await OnPreviewModelChange(mesh);
                // 参考TtAnimationClipEditor: 预览模型保存到TtDesignMacrossAMeta
                if (mDesignMacross != null)
                {
                    var ameta = mDesignMacross.AMeta as TtDesignMacrossAMeta;
                    if (ameta != null && ameta.PreviewMeshName != modelRName)
                    {
                        ameta.PreviewMeshName = modelRName;
                        ameta.SaveAMeta((IO.IAsset)null);
                    }
                }
            };
            exec();
        }

        /// <summary>
        /// 模型资源加载完成后, 在预览场景中重建网格节点和DesignMacrossNode
        /// </summary>
        private async TtTask OnPreviewModelChange(TtMaterialMesh materialMesh)
        {
            // 清理旧的DM节点
            if (mDesignMacrossNode != null)
            {
                mDesignMacrossNode.Parent = null;
                mDesignMacrossNode = null;
            }
            // 清理旧的网格节点
            if (mCurrentMeshNode != null)
            {
                mCurrentMeshNode.Parent = null;
            }

            var meshData = new TtMeshNode.TtMeshNodeData();
            meshData.MeshName = materialMesh.AssetName;
            meshData.MdfQueueType = TtTypeDesc.TypeStr(typeof(TtMdfSkinMesh));
            meshData.AtomType = TtTypeDesc.TypeStr(typeof(TtRenderMesh.TtAtom));
            var mesh = new TtRenderMesh();
            mesh.Initialize(materialMesh, TtTypeDescGetter<TtMdfSkinMesh>.TypeDesc);

            var meshNode = await TtMeshNode.AddMeshNode(PreviewViewport.World, PreviewViewport.World.Root, meshData, typeof(GamePlay.TtPlacement), mesh,
                        DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            meshNode.NodeData.Name = "PreviewObject";
            meshNode.IsAcceptShadow = true;
            meshNode.IsCastShadow = true;

            mCurrentMeshNode = meshNode;

            // 以meshNode为父节点创建DesignMacrossNode
            if (mDesignMacross != null)
            {
                var dmData = new TtDesignMacrossNode.TtDesignMacrossNodeData();
                dmData.DesignMacrossName = mDesignMacross;
                dmData.Name = "PreviewDMNode";
                var dmNode = await TtNode.SpawnNode<TtDesignMacrossNode>(
                    meshNode, null, dmData,
                    EBoundVolumeType.Box, typeof(GamePlay.TtPlacement), PreviewViewport.World);
                if (dmNode != null)
                {
                    dmNode.NodeData.Name = "PreviewDMNode";
                    dmNode.DesignMacross = mDesignMacross;
                    mDesignMacrossNode = dmNode;
                }
            }
        }

        #region ITickable
        public int GetTickOrder()
        {
            return 0;
        }
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion ITickable
    }
}
