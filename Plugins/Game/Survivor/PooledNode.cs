using Assimp;
using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Survivor
{
    [EngineNS.Bricks.CodeBuilder.ContextMenu("MeshCreator", "Game\\Survivor\\MeshCreator", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSceneMeshCreator.TtSceneMeshCreatorData), DefaultNamePrefix = "MeshCreator")]
    public class TtSceneMeshCreator : TtSceneActorNode
    {
        public class TtSceneMeshCreatorData : TtNodeData
        {
            [EngineNS.Rtti.Meta]
            [RName.PGRName(FilterExts = EngineNS.Graphics.Mesh.TtMaterialMesh.AssetExt)]
            public RName MeshName { get; set; }
            [EngineNS.Rtti.Meta]
            public int Count { get; set; } = 1024;
        }

        public List<TtMeshNode> mMeshes = new List<TtMeshNode>();
        EngineNS.Bricks.AdvanceShadow.TtAdvanceShadowNode AdvShadowNode = null;
        public override void Dispose()
        {
            foreach(var i in mMeshes)
            {
                AdvShadowNode?.RemoveShadowNode(i);
            }
            
            base.Dispose();
        }
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            AdvShadowNode = world.Root.FindFirstChild<EngineNS.Bricks.AdvanceShadow.TtAdvanceShadowNode>();
            var creatorData = GetNodeData<TtSceneMeshCreatorData>();
            if (creatorData.MeshName == null)
            {
                creatorData.MeshName = RName.GetRName("mesh/base/box_1.ums", RName.ERNameType.Engine);
            }
            
            return true;
        }
        protected override async TtTask OnPostInitNode(TtNode parent)
        {
            await base.OnPostInitNode(parent);
            var creatorData = GetNodeData<TtSceneMeshCreatorData>();
            await ResetMeshCount(creatorData.Count);
        }
        [Category("Option")]
        public int MeshCount
        {
            get
            {  
                return mMeshes.Count;
            }
            set
            {
                ResetMeshCount(value).WaitCompletedAndDispose();
            }
        }
        [Category("Option")]
        public bool DoRandomPosition
        {
            get => true;
            set
            {
                RandomPosition();
            }
        }
        public void RandomPosition()
        {
            foreach (var i in mMeshes)
            {
                var rd = MathHelper.RandomDirection(false).AsDVector();
                i.Placement.Position = rd * 1024;
            }
        }
        public async TtTask ResetMeshCount(int count)
        {
            if (count == mMeshes.Count)
            {
                return;
            }
            var creatorData = GetNodeData<TtSceneMeshCreatorData>();
            creatorData.Count = count;
            if (count - mMeshes.Count > 0)
            {
                count = count - mMeshes.Count;
                DVector3 size = new DVector3(1024, 0, 1024);
                for (int i = 0; i < count; i++)
                {
                    TtMeshNode.TtMeshNodeData meshData = new TtMeshNode.TtMeshNodeData();
                    meshData.MeshName = creatorData.MeshName;
                    var mesh = await TtNode.SpawnNode<TtMeshNode>(this, null, meshData);
                    var rd = MathHelper.RandomDirection(false).AsDVector();
                    mesh.Placement.Position = rd * size;
                    mesh.IsCastShadow = true;
                    AdvShadowNode?.PushShadowNodes(mesh, false);
                    mMeshes.Add(mesh);
                }
            }
            else
            {
                count = mMeshes.Count - count;
                for (int i = 0; i < count; i++)
                {
                    var mesh = mMeshes[i];
                    AdvShadowNode?.RemoveShadowNode(mesh);
                    mesh.Parent = null;
                    mMeshes.RemoveAt(i);
                    mesh.Dispose();
                }
            }
        }
    }
}
