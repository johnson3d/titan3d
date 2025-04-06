using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;
using static EngineNS.Bricks.AdvanceShadow.TtQTree;
using System.Security.Cryptography.Xml;
using static EngineNS.Bricks.AdvanceShadow.TtQNode;

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
        public DBoundingBox2D AABB;
        public TtQNode Child00;
        public TtQNode Child01;
        public TtQNode Child10;
        public TtQNode Child11;
        public struct FShadowObject
        {
            public TtNode SceneNode;
            public DBoundingBox2D AABB;
        }
        public List<FShadowObject> ShadowObjects = new List<FShadowObject>();
        internal void PushObject(in FShadowObject shadowObj)
        {
            ShadowObjects.Add(shadowObj);
        }
        public ref FQNode GetNodeData(TtQTree tree)
        {
            return ref tree.QNodes[NodeIndex];
        }
        public void Initialize(TtQTree tree, in DVector2 min, in DVector2 max)
        {
            AABB.Minimum = min;
            AABB.Maximum = max;
            //todo 增加检测是否空node
            GetNodeData(tree).Tile = tree.AllocTile();
        }
        public void PushShadowObjects(TtQTree tree, List<FShadowObject> nodes)
        {
            foreach (var i in nodes)
            {
                this.PushObject(in i);
            }
        }
    }

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtQTree
    {
        public TtQNode.FQNode[] QNodes = null;
        public Stack<int> NodeAllocator = new Stack<int>();
        public Stack<int> TileAllocator = new Stack<int>();
        public TtQNode Root = null;
        public float MaxShadowDistance = 1000;
        public int MaxDeepLeve = 5;
        public int MaxTileCount = 0;
        [Category("Debug")]
        public int AliveNode
        {
            get => QNodes.Length - NodeAllocator.Count;
        }
        [Category("Debug")]
        public int AliveTile
        {
            get => MaxTileCount - TileAllocator.Count;
        }
        public static void CountFullTreeNode(int level, out int side, out int total)
        {
            side = 1;
            total = 1;
            for (int i = 0; i < level; i++)
            {
                side *= 2;
                total += side * side;
            }
        }
        public void Initialize(int maxDeepLevel, DBoundingBox2D aabb, int maxTile)
        {
            MaxDeepLeve = maxDeepLevel;
            int side = 0;
            int total = 0;
            CountFullTreeNode(MaxDeepLeve, out side, out total);

            QNodes = new TtQNode.FQNode[total];
            NodeAllocator.Clear();
            for (int i = 0; i < total; i++)
            {
                NodeAllocator.Push(total - 1 - i);
            }
            TileAllocator.Clear();
            MaxTileCount = Math.Min(side * side, maxTile);
            for (int i = 0; i < MaxTileCount; i++)
            {
                TileAllocator.Push(MaxTileCount - 1 - i);
            }

            Root = new TtQNode(AllocNode(), 0);
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
        public void FreeNodeTree(TtQNode node)
        {
            if (node.Child00 != null)
            {
                FreeNodeTree(node.Child00);
                node.Child00 = null;
                FreeNodeTree(node.Child01);
                node.Child01 = null;
                FreeNodeTree(node.Child10);
                node.Child10 = null;
                FreeNodeTree(node.Child11);
                node.Child11 = null;
            }
            if (QNodes[node.NodeIndex].Tile >= 0)
            {
                FreeTile(QNodes[node.NodeIndex].Tile);
                QNodes[node.NodeIndex].Tile = -1;
            }
            FreeNode(node.NodeIndex);
            node.NodeIndex = -1;
            node.ShadowObjects.Clear();
        }
        protected void UpdateQTree(TtCamera cameral, TtQNode node)
        {
            ref var data = ref node.GetNodeData(this);
            if (NeedSplit(cameral, node) == false)
            {
                if (node.Child00 != null)
                {
                    FreeNodeTree(node.Child00);
                    node.Child00 = null;
                    FreeNodeTree(node.Child01);
                    node.Child01 = null;
                    FreeNodeTree(node.Child10);
                    node.Child10 = null;
                    FreeNodeTree(node.Child11);
                    node.Child11 = null;
                }
                return;
            }

            if (QNodes[node.NodeIndex].Tile >= 0)
            {
                FreeTile(QNodes[node.NodeIndex].Tile);
                QNodes[node.NodeIndex].Tile = -1;
            }
            if (node.Child00 == null)
            {
                var center = node.AABB.GetCenter();

                data.Child00 = AllocNode();
                node.Child00 = new TtQNode(data.Child00, node.DeepLevel + 1);
                var min = new DVector2(node.AABB.Minimum.X, node.AABB.Minimum.Y);
                var max = new DVector2(center.X, center.Y);
                node.Child00.Initialize(this, in min, in max);
                node.Child00.PushShadowObjects(this, node.ShadowObjects);

                data.Child01 = AllocNode();
                node.Child01 = new TtQNode(data.Child01, node.DeepLevel + 1);
                min = new DVector2(center.X, node.AABB.Minimum.Y);
                max = new DVector2(node.AABB.Maximum.X, center.Y);
                node.Child01.Initialize(this, in min, in max);
                node.Child01.PushShadowObjects(this, node.ShadowObjects);

                data.Child10 = AllocNode();
                node.Child10 = new TtQNode(data.Child10, node.DeepLevel + 1);
                min = new DVector2(node.AABB.Minimum.X, center.Y);
                max = new DVector2(center.X, node.AABB.Maximum.Y);
                node.Child10.Initialize(this, in min, in max);
                node.Child10.PushShadowObjects(this, node.ShadowObjects);

                data.Child11 = AllocNode();
                node.Child11 = new TtQNode(data.Child11, node.DeepLevel + 1);
                min = new DVector2(center.X, center.Y);
                max = new DVector2(node.AABB.Maximum.X, node.AABB.Maximum.Y); ;
                node.Child11.Initialize(this, in min, in max);
                node.Child11.PushShadowObjects(this, node.ShadowObjects);
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
            if (node.DeepLevel >= limitLevel)//超过深度限制，不要再切分了
            {
                return false;
            }

            return true;
        }
        public float MinDistance(TtCamera cameral, in DBoundingBox2D aabb)
        {
            var pos = new DVector2(cameral.GetPosition().X, cameral.GetPosition().Z);
            if (DBoundingBox2D.Contains(in aabb, in pos) == ContainmentType.Contains)
                return 0;
            
            float dist = float.MaxValue;
            for(int i=0; i < 4; i++)
            {
                var c = aabb.GetCorner(i);
                var d = DVector2.Distance(in pos, in c);
                if (d <= dist)
                    dist = (float)d;
            }
            return dist;
        }
        public struct FStats
        {
            public int Node;
            public int Tile;
        }
        public void DrawQTree(ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            DrawQTree(Root, cmdlist, in drawSize, in DrawOffset, ref stats);
        }
        private void DrawQTree(TtQNode node, ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            var size = Root.AABB.GetSize();
            var min = new Vector2((float)(node.AABB.Minimum.X/ size.X), (float)(node.AABB.Minimum.Y / size.Y)) * drawSize + DrawOffset;
            var max = new Vector2((float)(node.AABB.Maximum.X / size.X), (float)(node.AABB.Maximum.Y / size.Y)) * drawSize + DrawOffset;
            var level = (byte)(node.DeepLevel * 255 / MaxDeepLeve);
            var color = new Color4b(level, level, level, 255);
            cmdlist.AddRect(in min, in max, color.ToAbgr(), 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
            stats.Node++;
            if (node.Child00 != null)
            {
                DrawQTree(node.Child00, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(node.Child01, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(node.Child10, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(node.Child11, cmdlist, in drawSize, in DrawOffset, ref stats);
            }
            else
            {
                stats.Tile += 1;
            }
        }

        public void PushShadowNode(TtNode node, in Vector3 lightDir)
        {
            ref var aabb = ref node.AbsAABB;
            DPlane plane = new DPlane(in DVector3.UnitY, 0);

            var matrix = DMatrix.MakeShadow(lightDir.AsDVector(), plane);
            DBoundingBox2D projAABB = new DBoundingBox2D();
            projAABB.InitEmptyBox();
            for (int i = 0; i < 8; i++)
            {
                var c = aabb.GetCorner(i);
                var cp = DVector3.TransformCoordinate(in c, in matrix);

                var cp2d = new DVector2(cp.X, cp.Z);
                projAABB.Merge(in cp2d);
            }
            FShadowObject shadowObj;
            shadowObj.SceneNode = node;
            shadowObj.AABB = projAABB;
            PushShadowObject(Root, in shadowObj);
        }
        private void PushShadowObject(TtQNode node, in FShadowObject shadowObject)
        {
            if (node.AABB.Contains(shadowObject.AABB) == ContainmentType.Disjoint)
            {
                return;
            }
            node.PushObject(shadowObject);
            if (node.Child00 != null)
            {
                PushShadowObject(node.Child00, in shadowObject);
                PushShadowObject(node.Child01, in shadowObject);
                PushShadowObject(node.Child10, in shadowObject);
                PushShadowObject(node.Child11, in shadowObject);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AdvanceShadow", "AdvanceShadow", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtAdvanceShadowNode.TtAdvanceShadowData), DefaultNamePrefix = "AdvanceShadow")]
    public class TtAdvanceShadowNode : TtSceneActorNode
    {
        public class TtAdvanceShadowData : TtNodeData
        {
            [Rtti.Meta]
            public int MaxDeepLeve { get; set; } = 6;
            [Rtti.Meta]
            public float MaxShadowDistance { get; set; } = 500.0f;
        }

        public TtQTree mShadowMapTree = null;
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            DBoundingBox2D aabb = new DBoundingBox2D(DVector2.Zero, new DVector2(1024, 1024));
            mShadowMapTree = new TtQTree();
            mShadowMapTree.Initialize(GetNodeData<TtAdvanceShadowData>().MaxDeepLeve, aabb, 1024);
            mShadowMapTree.MaxShadowDistance = GetNodeData<TtAdvanceShadowData>().MaxShadowDistance;
            return ret;
        }
        public override void Dispose()
        {
            ShowDebugger = false;
            base.Dispose();
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
                    if(mDebugger!=null)
                    {
                        mDebugger.mCullingNode = cullingNode;
                    }
                }
            }
            return true;
        }
        TtQTreeVisualDebugger mDebugger;
        [Category("Debug")]
        public TtQTreeVisualDebugger VisualDebugger
        {
            get => mDebugger;
        }
        [Category("Option")]
        public bool ShowDebugger
        {
            get
            {
                return mDebugger != null;
            }
            set
            {
                if (value == true)
                {
                    if (mDebugger == null)
                    {
                        mDebugger = new TtQTreeVisualDebugger();
                        mDebugger.mAdanceShadowNode = this;
                    }
                    TtEngine.RootFormManager.RegRootForm(mDebugger);
                }
                else
                {
                    if (mDebugger != null)
                    {
                        TtEngine.RootFormManager.UnregRootForm(mDebugger);
                        mDebugger = null;
                    }
                }
            }
        }
    }

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtQTreeVisualDebugger : IRootForm
    {
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public TtQTreeVisualDebugger()
        {
            
        }
        public unsafe void Dispose()
        {
            
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }
        public TtAdvanceShadowNode mAdanceShadowNode;
        public TtCpuCullingNode mCullingNode;
        [Category("Debug")]
        public TtQTree QTree
        {
            get => mAdanceShadowNode.mShadowMapTree;
        }
        public void OnDraw()
        {
            var result = EGui.UIProxy.DockProxy.BeginMainForm("Advance Shadow Debugger", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                var DrawOffset = new Vector2();
                DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);

                var cmdlist = ImGuiAPI.GetWindowDrawList();
                var size = ImGuiAPI.GetWindowSize();
                float side = MathF.Min(size.X, size.Y);
                var stats = new TtQTree.FStats();
                stats.Node = 0;
                stats.Tile = 0;
                mAdanceShadowNode.mShadowMapTree.DrawQTree(cmdlist, new Vector2(side, side), in DrawOffset, ref stats);

                if (mCullingNode != null)
                {
                    var cameral = mCullingNode.VisParameter.CullCamera;
                    var pos = new Vector2((float)cameral.GetPosition().X, (float)cameral.GetPosition().Z);
                    var t = mAdanceShadowNode.mShadowMapTree.Root.AABB.GetSize();
                    pos.X = (float)(pos.X * side / t.X);
                    pos.Y = (float)(pos.Y * side / t.Y);
                    pos += DrawOffset;
                    cmdlist.AddCircle(in pos, 5, Color4b.Red.ToAbgr(), 10, 1);
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
