using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;

namespace EngineNS.Bricks.AdvanceShadow
{
    public class TtQNode
    {
        public struct FQNode
        {
            public void SetDefault()
            {
                Child00 = -1;
                Child01 = -1;
                Child10 = -1;
                Child11 = -1;
                Tile = -1;
                ShadowMatrix = Matrix.Identity;
            }
            public int Child00;
            public int Child01;
            public int Child10;
            public int Child11;

            public int Tile;
            public Matrix ShadowMatrix;
        }
        public TtQNode(int index, int deepLevel)
        {
            NodeIndex = index;
            DeepLevel = deepLevel;
        }

        public int NodeIndex;
        public int DeepLevel;
        public DBoundingBox AABB;
        public TtQNode Child00;
        public TtQNode Child01;
        public TtQNode Child10;
        public TtQNode Child11;
        public ref FQNode GetNodeData(TtQTree tree)
        {
            return ref tree.QNodes[NodeIndex];
        }
        public void Initialize(TtQTree tree, in DVector3 min, in DVector3 max)
        {
            AABB.Minimum = min;
            AABB.Maximum = max;
            //todo 增加检测是否空node
            GetNodeData(tree).Tile = tree.AllocTile();
        }
    }

    public class TtQTree
    {
        public TtQNode.FQNode[] QNodes = null;
        public Stack<int> NodeAllocator = new Stack<int>();
        public Stack<int> TileAllocator = new Stack<int>();
        public TtQNode Root = null;
        public float MaxShadowDistance = 1000;
        public int MaxDeepLeve = 5;
        public static int CountFullTreeNode(int level)
        {
            int side = 1;
            int total = 1;
            for (int i = 1; i <= level; i++)
            {
                side *= 2;
                total += side;
            }
            return total;
        }
        public void Initialize(int maxDeepLevel, DBoundingBox aabb, int maxTile)
        {
            MaxDeepLeve = maxDeepLevel;
            int total = CountFullTreeNode(MaxDeepLeve);

            QNodes = new TtQNode.FQNode[total];
            NodeAllocator.Clear();
            for (int i = 0; i < total; i++)
            {
                NodeAllocator.Push(total - 1 - i);
            }
            TileAllocator.Clear();
            maxTile = Math.Min(total, maxTile);
            for (int i = 0; i < maxTile; i++)
            {
                TileAllocator.Push(total - 1 - i);
            }

            Root = new TtQNode(0, 0);
            Root.Initialize(this, in aabb.Minimum, in aabb.Maximum);
        }
        public int AllocNode()
        {
            return NodeAllocator.Pop();
        }
        public void FreeNode(int index)
        {
            if (QNodes[index].Tile >= 0)
            {
                FreeTile(QNodes[index].Tile);
                QNodes[index].Tile = -1;
            }
            NodeAllocator.Push(index);
        }
        public int AllocTile()
        {
            return TileAllocator.Pop();
        }
        public void FreeTile(int index)
        {
            TileAllocator.Push(index);
        }
        public void UpdateQTree(TtCamera cameral)
        {
            UpdateQTree(cameral, Root);
        }
        protected void UpdateQTree(TtCamera cameral, TtQNode node)
        {
            ref var data = ref node.GetNodeData(this);
            if (NeedSplit(cameral, node) == false)
            {
                if (node.Child00 != null)
                {
                    FreeNode(data.Child00);
                    node.Child00 = null;
                    FreeNode(data.Child01);
                    node.Child01 = null;
                    FreeNode(data.Child10);
                    node.Child10 = null;
                    FreeNode(data.Child11);
                    node.Child11 = null;
                }
                return;
            }
            
            if (node.Child00 == null)
            {
                var center = node.AABB.GetCenter();

                data.Child00 = AllocNode();
                node.Child00 = new TtQNode(data.Child00, node.DeepLevel + 1);
                var min = new DVector3(node.AABB.Minimum.X, node.AABB.Minimum.Y, node.AABB.Minimum.Z);
                var max = new DVector3(center.X, node.AABB.Maximum.Y, center.Z); ;
                node.Child00.Initialize(this, in min, in max);

                data.Child01 = AllocNode();
                node.Child01 = new TtQNode(data.Child01, node.DeepLevel + 1);
                min = new DVector3(center.X, node.AABB.Minimum.Y, node.AABB.Minimum.Z);
                max = new DVector3(node.AABB.Maximum.X, node.AABB.Maximum.Y, center.Z) ;
                node.Child01.Initialize(this, in min, in max);

                data.Child10 = AllocNode();
                node.Child10 = new TtQNode(data.Child10, node.DeepLevel + 1);
                min = new DVector3(node.AABB.Minimum.X, node.AABB.Minimum.Y, center.Z);
                max = new DVector3(center.X, node.AABB.Maximum.Y, node.AABB.Maximum.Z); ;
                node.Child10.Initialize(this, in min, in max);

                data.Child11 = AllocNode();
                node.Child11 = new TtQNode(data.Child11, node.DeepLevel + 1);
                min = new DVector3(center.X, node.AABB.Minimum.Y, center.Z);
                max = new DVector3(node.AABB.Maximum.X, node.AABB.Maximum.Y, node.AABB.Maximum.Z); ;
                node.Child11.Initialize(this, in min, in max);
            }

            UpdateQTree(cameral, node.Child00);
            UpdateQTree(cameral, node.Child01);
            UpdateQTree(cameral, node.Child10);
            UpdateQTree(cameral, node.Child11);
        }
        public bool NeedSplit(TtCamera cameral, in TtQNode node)
        {
            //检测本Node的BoundBox2D足够小，就不用再切分了

            //距离摄像机远也不需要切分
            var dist = MinDistance(cameral, in node.AABB);
            if (dist > MaxShadowDistance)
                return false;
            var limitLevel = (int)(((MaxShadowDistance - dist) / MaxShadowDistance) * MaxDeepLeve);
            if (node.DeepLevel > limitLevel)//超过深度限制，不要再切分了
            {
                if(limitLevel==3)
                {
                    int xxx = 0;
                }
                return false;
            }

            return true;
        }
        public float MinDistance(TtCamera cameral, in DBoundingBox aabb)
        {
            if (DBoundingBox.Contains(aabb, cameral.GetPosition()) == ContainmentType.Contains)
                return 0;
            var pos = new DVector2(cameral.GetPosition().X, cameral.GetPosition().Z);
            float dist = float.MaxValue;
            for(int i=0; i < 8; i++)
            {
                var c = aabb.GetCorner(i);
                var c2 = new DVector2(c.X, c.Z);
                var d = DVector2.Distance(in pos, in c2);
                if (d <= dist)
                    dist = (float)d;
            }
            return dist;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AdvanceShadow", "AdvanceShadow", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtAdvanceShadowNode.TtAdvanceShadowData), DefaultNamePrefix = "AdvanceShadow")]
    public class TtAdvanceShadowNode : TtSceneActorNode
    {
        public class TtAdvanceShadowData : TtNodeData
        {
            [Rtti.Meta]
            public int MaxDeepLeve { get; set; } = 5;
            [Rtti.Meta]
            public float MaxShadowDistance { get; set; } = 500.0f;
        }

        public TtQTree mShadowMapTree = null;
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            DBoundingBox aabb = new DBoundingBox(DVector3.Zero, new DVector3(1024, 1024, 1024));
            mShadowMapTree = new TtQTree();
            mShadowMapTree.Initialize(GetNodeData<TtAdvanceShadowData>().MaxDeepLeve, aabb, 1024);
            mShadowMapTree.MaxShadowDistance = GetNodeData<TtAdvanceShadowData>().MaxShadowDistance;
            return ret;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            base.OnTickLogic(args);

            if (mShadowMapTree != null)
            {
                var cullingNode = args.Policy.FindFirstNode<TtCpuCullingNode>();
                if (cullingNode != null)
                {
                    mShadowMapTree.UpdateQTree(cullingNode.VisParameter.CullCamera);
                }
            }
            return true;
        }
    }
}
