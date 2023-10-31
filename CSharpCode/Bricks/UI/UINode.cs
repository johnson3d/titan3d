using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.IO;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI
{
    [Bricks.CodeBuilder.ContextMenu("UINode,UI", "UINode", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(TtUINode.TtUINodeData), DefaultNamePrefix = "UI")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtUINode : TtGpuSceneNode
    {
        public override void Dispose()
        {
            UIHost?.OnDispose();
            base.Dispose();
        }

        TtUIHost mUIHost;
        public TtUIHost UIHost
        {
            get => mUIHost;
            set
            {
                mUIHost = value;
                if (mUIHost != null && mUIHost.SceneNode != this)
                    mUIHost.SceneNode = this;
            }
        }

        public class TtUINodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = UI.TtUIAsset.AssetExt)]
            public RName UIName { get; set; }
        }

        public override async Task<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtUINodeData == null)
                data = new TtUINodeData();
            if(await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            // todo: load ui
            //var uiData = data as TtUINodeData;
            return true;
        }
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        [Category("Option")]
        public RName UIName
        {
            get
            {
                var uiData = NodeData as TtUINodeData;
                if (uiData == null)
                    return null;
                return uiData.UIName;
            }
            set
            {
                var uiData = NodeData as TtUINodeData;
                if (uiData == null)
                    return;

                uiData.UIName = value;
                // todo: load ui
            }
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);

            UpdateAbsTransform();
            var uiData = NodeData as TtUINodeData;
            if(uiData == null || uiData.UIName == null)
            {
                // error ui show
            }
            else
            {
                // todo: load ui
            }
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (UIHost == null)
                return;

            UIHost.GatherVisibleMeshes(rp);
        }
        protected override void OnAbsTransformChanged()
        {
            if (UIHost == null)
                return;
            UIHost.OnHostNodeAbsTransformChanged(this, GetWorld());
        }
        public unsafe override bool OnLineCheckTriangle(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            if (UIHost == null)
                return false;
            var startF = start.ToSingleVector3();
            var endF = end.ToSingleVector3();
            return UIHost.OnLineCheckTriangle(startF, endF, ref result);
        }
        public override void AddAssetReferences(IAssetMeta ameta)
        {
            if(UIName != null)
                ameta.AddReferenceAsset(UIName);
        }
        public UViewportSlate GetViewport()
        {
            return GetWorld()?.CurViewport;
        }

        public static async TtTask<TtUINode> AddUINode(GamePlay.UWorld world, UNode parent, UNodeData data, Type placementType, TtUIHost uiHost, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var uiNode = await scene.NewNode(world, typeof(TtUINode), data, EBoundVolumeType.Box, placementType) as TtUINode;
            if (uiHost.AssetName != null)
                uiNode.NodeData.Name = uiHost.AssetName.Name;
            else
                uiNode.NodeData.Name = uiNode.SceneId.ToString();
            uiNode.UIHost = uiHost;
            uiNode.Parent = parent;
            uiNode.Placement.SetTransform(in pos, in scale, in quat);
            return uiNode;
        }
    }
}
