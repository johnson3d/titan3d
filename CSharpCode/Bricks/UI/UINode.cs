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
            foreach(var i in mUIHost)
            {
                i.OnDispose();
            }
            mUIHost.Clear();
            base.Dispose();
        }

        List< TtUIHost> mUIHost = new List<TtUIHost>();
        public void InsertUIHost(int index, TtUIHost host)
        {
            if (host.SceneNode != this)
            {
                host.SceneNode?.mUIHost.Remove(host);
            }
            else
            {
                return;
            }
            host.mSceneNode = this;
            mUIHost.Insert(index, host);
        }
        public void AddUIHost(TtUIHost host)
        {
            InsertUIHost(mUIHost.Count, host);
        }
        public void RemoveUIHost(int index)
        {
            mUIHost[index].mSceneNode = null;
            mUIHost.RemoveAt(index);
        }
        public void RemoveUIHost(TtUIHost host)
        {
            for (int i = 0; i < mUIHost.Count; i++)
            {
                if(mUIHost[i] == host)
                {
                    mUIHost[i].mSceneNode = null;
                    mUIHost.RemoveAt(i);
                    return;
                }
            }
        }
        
        public TtUIHost GetUIHost(int index = 0)
        {
            if (index < 0 || index >= mUIHost.Count)
                return null;
            return mUIHost[index];
        }
        public class TtUINodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = UI.TtUIAsset.AssetExt)]
            public RName UIName { get; set; }
        }

        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
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
            foreach (var i in mUIHost)
            {
                i.GatherVisibleMeshes(rp);
            }
        }
        protected override void OnAbsTransformChanged()
        {
            foreach (var i in mUIHost)
            {
                i.OnHostNodeAbsTransformChanged(this, GetWorld());
            }
        }
        public unsafe override bool OnLineCheckTriangle(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            var startF = start.ToSingleVector3();
            var endF = end.ToSingleVector3();
            //foreach (var i in mUIHost)
            for (int i = mUIHost.Count - 1; i >= 0; i--)
            {
                if (mUIHost[i].OnLineCheckTriangle(startF, endF, ref result))
                    return true;
            }
            return false;
        }
        public override void AddAssetReferences(IAssetMeta ameta)
        {
            if(UIName != null)
                ameta.AddReferenceAsset(UIName);
        }
        public static async TtTask<TtUINode> AddUINode(GamePlay.UWorld world, UNode parent, UNodeData data, Type placementType, TtUIHost uiHost, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var uiNode = await scene.NewNode(world, typeof(TtUINode), data, EBoundVolumeType.Box, placementType) as TtUINode;
            if (uiHost.AssetName != null)
                uiNode.NodeData.Name = uiHost.AssetName.Name;
            else
                uiNode.NodeData.Name = uiNode.SceneId.ToString();
            uiNode.AddUIHost(uiHost);
            uiNode.Parent = parent;
            uiNode.Placement.SetTransform(in pos, in scale, in quat);
            return uiNode;
        }

        public override void UpdateAABB()
        {
            AABB = DBoundingBox.EmptyBox();
            for(int i=0; i<mUIHost.Count; i++)
            {
                if(mUIHost[i].DrawMesh != null)
                {
                    AABB.Merge(mUIHost[i].DrawMesh.WorldAABB);
                }
            }
        }
    }
}
