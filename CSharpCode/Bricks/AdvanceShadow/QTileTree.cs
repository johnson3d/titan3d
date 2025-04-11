using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;
using static EngineNS.Bricks.AdvanceShadow.TtQNode;

namespace EngineNS.Bricks.AdvanceShadow
{
    public struct FAdvShadowNodeData
    {
        public void SetDefault()
        {
            ChildIndex00 = -1;
            ChildIndex01 = -1;
            ChildIndex10 = -1;
            ChildIndex11 = -1;
            PageIndex = -2;
        }
        public int ChildIndex00;
        public int ChildIndex01;
        public int ChildIndex10;
        public int ChildIndex11;

        public int PageIndex;
    }
    public partial class TtQNode
    {
        public TtQNode(int deepLevel)
        {
            DeepLevel = deepLevel;
        }

        public TtQTree QTree;
        public int NodeIndex;
        public int DeepLevel;
        public DBoundingBox2D AABB;
        public TtQNode Child00;
        public TtQNode Child01;
        public TtQNode Child10;
        public TtQNode Child11;
        public TtQLeaf Leaf = null;
        public int PageIndex = -2;
        
        public bool IsLeaf
        {
            get => PageIndex >= 0;
        }
        public bool IsNode
        {
            get => PageIndex == -1;
        }
        public bool IsDeathNode
        {
            get => PageIndex == -2;
        }
        public struct FShadowObject
        {
            public TtNode SceneNode;
            public DBoundingBox2D AABB;
            public bool IsDynamic;
        }
        public Dictionary<TtNode, FShadowObject> ShadowObjects = new Dictionary<TtNode, FShadowObject>();
        public void SetToGpuData(ref FAdvShadowNodeData data)
        {
            data.PageIndex = PageIndex;
            if (Child00 != null)
            {
                data.ChildIndex00 = Child00.NodeIndex;
                data.ChildIndex01 = Child01.NodeIndex;
                data.ChildIndex10 = Child10.NodeIndex;
                data.ChildIndex11 = Child11.NodeIndex;
            }
        }
        internal void AsLeaf()
        {
            if (PageIndex < 0)
            {
                PageIndex = QTree.AllocPage();
            }
            if (Leaf == null)
            {   
                Leaf = new TtQLeaf(this);
            }
        }
        internal void AsNode()
        {
            if (PageIndex >= 0)
            {
                QTree.FreePage(PageIndex);
            }
            PageIndex = -1;
            if (Leaf != null)
            {
                Leaf = null;
            }
        }
        internal void AsDeathNode()
        {
            if (PageIndex >= 0)
            {
                QTree.FreePage(PageIndex);
            }
            PageIndex = -2;
            if (Leaf != null)
            {
                Leaf = null;
            }
        }
        internal bool PushObject(in FShadowObject shadowObj)
        {
            if (DBoundingBox2D.Contains(in shadowObj.AABB, in AABB) == ContainmentType.Disjoint)
            {
                return false;
            }
            ShadowObjects.Add(shadowObj.SceneNode, shadowObj);

            if (Leaf != null)
            {
                Leaf.IsDirty = true;

                if (shadowObj.IsDynamic)
                    Leaf.IsContainDynamicObject = true;
            }

            return true;
        }
        public void RemoveShadowNode(TtNode node)
        {
            ShadowObjects.Remove(node);
            if (Child00 != null)
            {
                Child00.RemoveShadowNode(node);
                Child01.RemoveShadowNode(node);
                Child10.RemoveShadowNode(node);
                Child11.RemoveShadowNode(node);
            }
            else
            {
                if (Leaf != null)
                {
                    Leaf.IsDirty = true;
                }
            }
        }
        
        public void Initialize(TtQTree tree, in DVector2 min, in DVector2 max)
        {
            QTree = tree;
            AABB.Minimum = min;
            AABB.Maximum = max;
        }
        public void PushShadowObjects(TtQTree tree, Dictionary<TtNode,FShadowObject> nodes)
        {
            foreach (var i in nodes)
            {
                if (ShadowObjects.ContainsKey(i.Key))
                    continue;
                this.PushObject(i.Value);
            }
        }

        public void GatherLeafs(List<TtQNode> leafs)
        {
            if (this.IsLeaf)
            {
                leafs.Add(this);
                return;
            }
            if (Child00 != null)
            {
                Child00.GatherLeafs(leafs);
                Child01.GatherLeafs(leafs);
                Child10.GatherLeafs(leafs);
                Child11.GatherLeafs(leafs);
            }
        }
        public int ShadowObjectCount
        {
            get => CountShadowObjects(true);
        }
        public int CountShadowObjects(bool bExludeSame = true)
        {
            List<TtQNode> leafs = new List<TtQNode>();
            GatherLeafs(leafs);
            if (bExludeSame)
            {
                Dictionary<TtNode, FShadowObject> objs = new Dictionary<TtNode, FShadowObject>();
                foreach (var i in leafs)
                {
                    foreach(var j in i.ShadowObjects)
                    {
                        if (objs.ContainsKey(j.Key))
                            continue;
                        objs.Add(j.Key, j.Value);
                    }
                }
                return objs.Count;
            }
            else
            {
                int result = 0;
                foreach (var i in leafs)
                {
                    result += i.ShadowObjects.Count;
                }
                return result;
            }
        }
    }

    public struct FShadowPage
    {
        public int TextureArray;
        public Matrix ShadowMatrix;
        public FShadowPage(int arrayIndex, Matrix mat)
        {
            TextureArray = arrayIndex;
            ShadowMatrix = mat;
        }
    }

    public class TtQLeaf
    {
        public TtQLeaf(TtQNode node) 
        {
            HostNode = node;
            ShadowCamera = new TtCamera();
        }
        public uint UpdateShadowMapTime = 0;
        public TtQNode HostNode;
        public TtCamera ShadowCamera;
        public bool IsDirty = true;
        public bool IsContainDynamicObject = false;
        public int PageIndex
        {
            get => HostNode.PageIndex;
        }
        public ref FShadowPage GetShadowPage()
        {
            return ref HostNode.QTree.ShadowPages[PageIndex];
        }
        public void UpdateShadowMatrix(TtWorld world, uint time)
        {
            if (IsDirty == false)
            {
                return;
            }
            IsDirty = false;
            if (HostNode.ShadowObjects.Count == 0)
                return;

            if (PageIndex > HostNode.QTree.ShadowPages.Length)
                return;

            UpdateShadowMapTime = time;

            ref FShadowPage page = ref GetShadowPage();
            DBoundingBox aabb = new DBoundingBox();
            DBoundingBox2D aabb2d = new DBoundingBox2D();
            aabb.InitEmptyBox();
            aabb2d.InitEmptyBox();
            foreach (var i in HostNode.ShadowObjects)
            {
                aabb.Merge(in i.Key.AbsAABB);
                aabb2d.Merge(i.Value.AABB);
            }
            float BoxExt = 1.2f;
            var FrustumSphereDiameter = (float)aabb.GetMaxSide() * BoxExt;

            DVector2 c2d;
            float width;
            if(HostNode.AABB.Contains(in aabb2d) == ContainmentType.Contains)
            {
                c2d = aabb2d.GetCenter();
                width = (float)aabb2d.GetMaxSide();
            }
            else
            {
                c2d = HostNode.AABB.GetCenter();
                //Clamp by HostNode.AABB
                width = MathF.Min(FrustumSphereDiameter, (float)HostNode.AABB.GetSize().X);
            }

            ShadowCamera.SetMatrixStartPosition(world.CameraOffset);
            var c3d = new DVector3(c2d.X, 0, c2d.Y);
            ShadowCamera.LookAtLH(c3d - HostNode.QTree.LightDirection.AsDVector() * FrustumSphereDiameter * 0.5f, c3d, in Vector3.UnitY);
            var shadowZNear = 0.3f;// (\float)shadowCameraBox.Minimum.Z;
            var shadowZFar = 1000.0f;

            ShadowCamera.DoOrthoProjectionForShadow(width, width, shadowZNear, shadowZFar, 0, 0);
            ShadowCamera.UpdateConstBufferData(TtEngine.Instance.GfxDevice.RenderContext);

            Matrix vp = ShadowCamera.GetViewProjection();
            page.ShadowMatrix = vp * HostNode.QTree.mOrtho2UVMtx;
        }
    }

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtQTree
    {
        public Matrix mOrtho2UVMtx = Matrix.Identity;

        public Vector3 LightDirection;
        public TtFullQTreeBuilder QTreeBuilder;
        public TtQNode[] QNodes = null;
        public FAdvShadowNodeData[] AdvShadowNodeDatas = null;
        public FShadowPage[] ShadowPages = null;
        public Stack<int> PageAllocator = new Stack<int>();
        public TtQNode Root = null;
        public float MaxShadowDistance = 1000;
        public int MaxDeepLevel = 5;
        public int MaxTileCount = 0;
        public void UpdateLightDirection(Vector3 dir)
        {
            if (LightDirection == dir)
                return;

            LightDirection = dir;
            MarkAllLeafDirty();
        }
        public void MarkAllLeafDirty()
        {
            List<TtQNode> leafs = new List<TtQNode>();
            Root.GatherLeafs(leafs);
            foreach (var i in leafs)
            {
                i.Leaf.IsDirty = true;
            }
        }
        [Category("Debug")]
        public int AlivePage
        {
            get => MaxTileCount - PageAllocator.Count;
        }
        public static void CountFullTreeNode(int level, out int side, out int total)
        {
            side = 1;
            total = 1;
            for (int i = 1; i < level; i++)
            {
                side *= 2;
                total += side * side;
            }
        }
        public void Initialize(int maxDeepLevel, DBoundingBox2D aabb, int maxTile)
        {
            MaxDeepLevel = maxDeepLevel;

            int side = 0;
            int total = 0;
            CountFullTreeNode(MaxDeepLevel, out side, out total);

            PageAllocator.Clear();
            MaxTileCount = Math.Min(side * side, maxTile);
            ShadowPages = new FShadowPage[MaxTileCount];
            for (int i = 0; i < MaxTileCount; i++)
            {
                ShadowPages[i].TextureArray = i;
                PageAllocator.Push(MaxTileCount - 1 - i);
            }

            QTreeBuilder = new TtFullQTreeBuilder();
            QTreeBuilder.InitBuilder(MaxDeepLevel, in aabb);
            QNodes = QTreeBuilder.Nodes;
            AdvShadowNodeDatas = QTreeBuilder.AdvShadowNodeDatas;
            Root = QTreeBuilder.Root;
            Root.Initialize(this, in aabb.Minimum, in aabb.Maximum);

            UpdateAABB(Root);

            Root.AsLeaf();

            mOrtho2UVMtx = Matrix.MakeOrtho2UV(TtEngine.Instance.GfxDevice.RenderContext.RhiType);
        }
        public void UpdateAABB(TtQNode node)
        {
            if (node.Child00 == null)
            {
                return;
            }
            var center = node.AABB.GetCenter();

            var min = new DVector2(node.AABB.Minimum.X, node.AABB.Minimum.Y);
            var max = new DVector2(center.X, center.Y);
            node.Child00.Initialize(this, in min, in max);

            min = new DVector2(center.X, node.AABB.Minimum.Y);
            max = new DVector2(node.AABB.Maximum.X, center.Y);
            node.Child01.Initialize(this, in min, in max);

            min = new DVector2(node.AABB.Minimum.X, center.Y);
            max = new DVector2(center.X, node.AABB.Maximum.Y);
            node.Child10.Initialize(this, in min, in max);

            min = new DVector2(center.X, center.Y);
            max = new DVector2(node.AABB.Maximum.X, node.AABB.Maximum.Y); ;
            node.Child11.Initialize(this, in min, in max);

            UpdateAABB(node.Child00);
            UpdateAABB(node.Child01);
            UpdateAABB(node.Child10);
            UpdateAABB(node.Child11);
        }
        public int AllocPage()
        {
            if (PageAllocator.Count == 0)
                return int.MaxValue;
            return PageAllocator.Pop();
        }
        public void FreePage(int index)
        {
            if (index == -1)
                return;
            PageAllocator.Push(index);
        }
        public void FreeNodeTree(TtQNode node)
        {
            if (node.IsDeathNode)
            {
                return;
            }
            if (node.IsNode && node.Child00 != null)
            {
                FreeNodeTree(node.Child00);
                FreeNodeTree(node.Child01);
                FreeNodeTree(node.Child10);
                FreeNodeTree(node.Child11);

                node.PushShadowObjects(this, node.Child00.ShadowObjects);
                node.PushShadowObjects(this, node.Child01.ShadowObjects);
                node.PushShadowObjects(this, node.Child10.ShadowObjects);
                node.PushShadowObjects(this, node.Child11.ShadowObjects);

                node.Child00.ShadowObjects.Clear();
                node.Child01.ShadowObjects.Clear();
                node.Child10.ShadowObjects.Clear();
                node.Child11.ShadowObjects.Clear();
            }
            node.AsDeathNode();
        }
        [ThreadStatic]
        static Profiler.TimeScope mScopeUpdateQTree;
        static Profiler.TimeScope ScopeUpdateQTree
        {
            get
            {
                if (mScopeUpdateQTree == null)
                    mScopeUpdateQTree = new Profiler.TimeScope(typeof(TtQTree), nameof(UpdateQTree));
                return mScopeUpdateQTree;
            }
        }

        List<TtQNode> mUpdateShadowMapNodes = new List<TtQNode>();
        public List<TtQNode> UpdateShadowMapNodes
        {
            get => mUpdateShadowMapNodes;
        }
        private uint mUpdateShadowMapTime = 1;
        public DVector2 CameralPosition;
        public void UpdateQTree(TtWorld world, TtCamera cameral, int limitLeaf)
        {
            using (new Profiler.TimeScopeHelper(ScopeUpdateQTree))
            {
                CameralPosition = new DVector2(cameral.GetPosition().X, cameral.GetPosition().Z);
                UpdateQTree(in CameralPosition, Root);
                mUpdateShadowMapNodes.Clear();
                GetDirtyLeafs(world, Root, mUpdateShadowMapNodes);
                mUpdateShadowMapNodes.Sort((x,y)=>
                {
                    //todo: compare AABB size
                    //x.AABB.GetSize()
                    return x.Leaf.UpdateShadowMapTime.CompareTo(y.Leaf.UpdateShadowMapTime);
                });
                if (mUpdateShadowMapNodes.Count > limitLeaf)
                {
                    mUpdateShadowMapNodes.RemoveRange(limitLeaf, mUpdateShadowMapNodes.Count - limitLeaf);
                }
                foreach (var i in mUpdateShadowMapNodes)
                {
                    i.Leaf.UpdateShadowMatrix(world, mUpdateShadowMapTime);
                }
                mUpdateShadowMapTime++;

                //var count = Root.ShadowObjectCount;
                //if (count > 0)
                //{
                //}
            }   
        }
        public TtQNode GetPageNode(DVector2 pos)
        {
            if (Root.AABB.Contains(pos) == ContainmentType.Disjoint)
                return null;
            var dist = DVector2.Distance(in CameralPosition, in pos);
            if (dist > MaxShadowDistance)
                return null;
            var level = (int)(dist * MaxDeepLevel / MaxShadowDistance);
            var layer = QTreeBuilder.Layers[level];

            var numOfSide = layer.Side;
            //var numOfLayer = layer.Nodes.Length;
            var startIndex = layer.LayerStartIndex;
            var gridSize = layer.GridSize;
            var sigment = (pos - Root.AABB.Minimum) / gridSize;
            int x = (int)sigment.X;
            int y = (int)sigment.Y;
            int index = startIndex + (y * numOfSide + x);
            return QNodes[index];
        }
        protected bool GetDirtyLeafs(TtWorld world, TtQNode node, List<TtQNode> leaf, int limitLeaf = int.MaxValue)
        {
            if (node.IsLeaf)
            {
                if (node.Leaf.IsDirty == false)
                    return true;

                if (node.ShadowObjects.Count == 0)
                    return true;

                leaf.Add(node);
                return leaf.Count < limitLeaf;
            }
            if (node.Child00 != null)
            {
                if (GetDirtyLeafs(world, node.Child00, leaf, limitLeaf) == false)
                    return false;
                if (GetDirtyLeafs(world, node.Child01, leaf, limitLeaf) == false)
                    return false;
                if (GetDirtyLeafs(world, node.Child10, leaf, limitLeaf) == false)
                    return false;
                if (GetDirtyLeafs(world, node.Child11, leaf, limitLeaf) == false)
                    return false;
            }
            
            return true;
        }
        protected void UpdateQTree(in DVector2 cameralPos, TtQNode node)
        {
            if (node == null)
                return;

            if (NeedSplit(in cameralPos, node) == false)
            {
                if (node.Child00 != null)
                {
                    FreeNodeTree(node.Child00);
                    FreeNodeTree(node.Child01);
                    FreeNodeTree(node.Child10);
                    FreeNodeTree(node.Child11);

                    node.PushShadowObjects(this, node.Child00.ShadowObjects);
                    node.PushShadowObjects(this, node.Child01.ShadowObjects);
                    node.PushShadowObjects(this, node.Child10.ShadowObjects);
                    node.PushShadowObjects(this, node.Child11.ShadowObjects);

                    node.Child00.ShadowObjects.Clear();
                    node.Child01.ShadowObjects.Clear();
                    node.Child10.ShadowObjects.Clear();
                    node.Child11.ShadowObjects.Clear();
                }

                node.AsLeaf();
                return;
            }

            node.AsNode();

            if (node.ShadowObjects.Count > 0)
            {
                node.Child00.PushShadowObjects(this, node.ShadowObjects);
                node.Child01.PushShadowObjects(this, node.ShadowObjects);
                node.Child10.PushShadowObjects(this, node.ShadowObjects);
                node.Child11.PushShadowObjects(this, node.ShadowObjects);
                node.ShadowObjects.Clear();
            }

            UpdateQTree(in cameralPos, node.Child00);
            UpdateQTree(in cameralPos, node.Child01);
            UpdateQTree(in cameralPos, node.Child10);
            UpdateQTree(in cameralPos, node.Child11);
        }
        public bool NeedSplit(in DVector2 cameralPos, TtQNode node)
        {
            if(node.Child00 == null)
                return false;
            //距离摄像机远也不需要切分
            var dist = MinDistance(in cameralPos, in node.AABB);
            if (dist > MaxShadowDistance)
                return false;
            var limitLevel = (int)(((MaxShadowDistance - dist) / MaxShadowDistance) * MaxDeepLevel);
            if (node.DeepLevel >= limitLevel)//超过深度限制，不要再切分了
            {
                return false;
            }

            return true;
        }
        public float MinDistance(in DVector2 cameralPos, in DBoundingBox2D aabb)
        {
            var pos = cameralPos;
            
            if (pos.X > aabb.Maximum.X)
            {
                if (pos.Y > aabb.Maximum.Y)
                {
                    //corner tr
                    var dy = pos.Y - aabb.Maximum.Y;
                    var dx = pos.X - aabb.Maximum.X;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                else if (pos.Y < aabb.Minimum.Y)
                {
                    //corner br
                    var dy = aabb.Minimum.Y - pos.Y;
                    var dx = pos.X - aabb.Maximum.X;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                else
                {
                    return (float)(pos.X - aabb.Maximum.X);
                }
            }
            else if(pos.X < aabb.Minimum.X)
            {
                if (pos.Y > aabb.Maximum.Y)
                {
                    //corner tl
                    var dy = pos.Y - aabb.Maximum.Y;
                    var dx = aabb.Minimum.X - pos.X;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                else if (pos.Y < aabb.Minimum.Y)
                {
                    //corner bl
                    var dy = aabb.Minimum.Y - pos.Y;
                    var dx = aabb.Minimum.X - pos.X;
                    return (float)Math.Sqrt(dx * dx + dy * dy);
                }
                else
                {
                    return (float)(aabb.Minimum.X - pos.X);
                }
            }
            else
            {
                if (pos.Y > aabb.Maximum.Y)
                {
                    return (float)(pos.Y - aabb.Maximum.Y );
                }
                else if (pos.Y < aabb.Minimum.Y)
                {
                    return (float)(aabb.Minimum.Y - pos.Y);
                }
                else
                {
                    return 0;
                }
            }

            //if (DBoundingBox2D.Contains(in aabb, in pos) == ContainmentType.Contains)
            //    return 0;
            //float dist = float.MaxValue;
            //for(int i=0; i < 4; i++)
            //{
            //    var c = aabb.GetCorner(i);
            //    var d = DVector2.Distance(in pos, in c);
            //    if (d <= dist)
            //        dist = (float)d;
            //}
            //return dist;
        }
        public struct FStats
        {
            public int Node;
            public int Tile;
        }
        public void DrawQTree(TtAdvanceShadowMapNode graphNode, ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            DrawQTree(graphNode, Root, cmdlist, in drawSize, in DrawOffset, ref stats);
        }
        private void DrawQTree(TtAdvanceShadowMapNode graphNode, TtQNode node, ImDrawList cmdlist, in Vector2 drawSize, in Vector2 DrawOffset, ref FStats stats)
        {
            var size = Root.AABB.GetSize();
            var min = new Vector2((float)(node.AABB.Minimum.X / size.X), (float)(node.AABB.Minimum.Y / size.Y)) * drawSize + DrawOffset;
            var max = new Vector2((float)(node.AABB.Maximum.X / size.X), (float)(node.AABB.Maximum.Y / size.Y)) * drawSize + DrawOffset;
            
            if (node.IsNode && node.Child00 != null)
            {
                var level = (byte)(node.DeepLevel * 255 / MaxDeepLevel);
                var color = new Color4b(level, level, level, 255);
                cmdlist.AddRect(in min, in max, color.ToAbgr(), 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                stats.Node++;

                DrawQTree(graphNode, node.Child00, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child01, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child10, cmdlist, in drawSize, in DrawOffset, ref stats);
                DrawQTree(graphNode, node.Child11, cmdlist, in drawSize, in DrawOffset, ref stats);
            }
            else if(node.IsDeathNode)
            {
                return;
            }
            else
            {
                if (node.ShadowObjects.Count > 0 && graphNode.mDebuggerSRViews != null)
                {
                    var srv = graphNode.mDebuggerSRViews[node.Leaf.PageIndex];
                    cmdlist.AddImage((ulong)srv.GetTextureHandle(), in min, in max, in Vector2.Zero, in Vector2.One, 0xffffffff);
                }
                var level = (byte)(node.DeepLevel * 255 / MaxDeepLevel);
                var color = new Color4b(level, level, level, 255);
                cmdlist.AddRect(in min, in max, color.ToAbgr(), 0.0f, ImDrawFlags_.ImDrawFlags_None, 1.0f);
                stats.Node++;
                stats.Tile += 1;
            }
        }

        public void PushShadowNode(TtNode node, in Vector3 lightDir, bool isDynamic)
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
            shadowObj.IsDynamic = isDynamic;
            PushShadowObject(Root, in shadowObj);
        }
        private void PushShadowObject(TtQNode node, in FShadowObject shadowObject)
        {
            if (DBoundingBox2D.Contains(in shadowObject.AABB, in node.AABB) == ContainmentType.Disjoint)
            {
                return;
            }
            if (node.Child00 != null && node.Child00.IsDeathNode == false)
            {
                PushShadowObject(node.Child00, in shadowObject);
                PushShadowObject(node.Child01, in shadowObject);
                PushShadowObject(node.Child10, in shadowObject);
                PushShadowObject(node.Child11, in shadowObject);
            }
            else
            {
                node.PushObject(shadowObject);
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
            public int MaxDeepLeve { get; set; } = 7;
            [Rtti.Meta]
            public float MaxShadowDistance { get; set; } = 500.0f;
            [Rtti.Meta]
            public int ShadowMapPage { get; set; } = 512;
            [Rtti.Meta]
            public int MaxDirtyPagePerFrame { get; set; } = 3;
        }

        public TtAdvanceShadowMapNode mRenderGraphNode;
        public TtQTree mShadowMapTree = null;
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            DBoundingBox2D aabb = new DBoundingBox2D(DVector2.Zero, new DVector2(1024, 1024));
            mShadowMapTree = new TtQTree();
            var data1 = GetNodeData<TtAdvanceShadowData>();
            mShadowMapTree.Initialize(data1.MaxDeepLeve, aabb, data1.ShadowMapPage);
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
                UpdateLightDirection(this.GetWorld());
                //test code
                mShadowMapTree.MarkAllLeafDirty();

                var cullingNode = args.Policy.FindFirstNode<TtCpuCullingNode>();
                if (cullingNode != null)
                {
                    mShadowMapTree.UpdateQTree(this.GetWorld(), cullingNode.VisParameter.CullCamera, GetNodeData<TtAdvanceShadowData>().MaxDirtyPagePerFrame);
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

        public void UpdateLightDirection(TtWorld world)
        {
            mShadowMapTree.UpdateLightDirection(world.GetSun(0).DirectionLight.Direction);
        }
        public void PushShadowNodes(TtNode parent, bool isDynamic)
        {
            parent.IterateNodes((node, arg) =>
            {
                if (node.IsCastShadow)
                {
                    mShadowMapTree.PushShadowNode(node, mShadowMapTree.LightDirection, isDynamic);
                }
                return true;
            }, null);
        }
        public void RemoveShadowNode(TtNode parent)
        {
            this.mShadowMapTree.Root.RemoveShadowNode(parent);
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
                mAdanceShadowNode.mShadowMapTree.DrawQTree(mAdanceShadowNode.mRenderGraphNode, cmdlist, new Vector2(side, side), in DrawOffset, ref stats);

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
