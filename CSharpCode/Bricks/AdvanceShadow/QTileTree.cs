using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;
using EngineNS.UI.Controls;
using Org.BouncyCastle.Asn1.Mozilla;
using EngineNS.Profiler;

namespace EngineNS.Bricks.AdvanceShadow
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtQTree
    {
        public Dictionary<TtNode, TtShadowObject> ShadowObjectDictionary = new Dictionary<TtNode, TtShadowObject>();
        public Matrix mOrtho2UVMtx = Matrix.Identity;

        public Vector3 LightDirection;
        public DMatrix ProjectShadowMatrix;
        public TtFullQTreeBuilder QTreeBuilder;
        public TtQNode[] QNodes = null;
        public TtCpu2GpuBuffer<FAdvShadowNodeData> AdvShadowNodeDatas = null;
        //public FShadowPage[] ShadowPages = null;
        public Stack<int> PageAllocator = new Stack<int>();
        public TtQNode Root = null;
        public float MaxShadowDistance = 1000;
        public int MaxDeepLevel = 5;
        public int MaxPageCount = 0;
        public float EsmConstant = 1.0f;
        public float MaxExp = 11.0f;//80.0f;//
        public float GaussSigma = 1.0f;

        public void UpdateLightDirection(Vector3 dir)
        {
            if (LightDirection == dir)
                return;

            DPlane plane = new DPlane(in DVector3.UnitY, 0);
            ProjectShadowMatrix = DMatrix.MakeShadow(dir.AsDVector(), plane);
            LightDirection = dir;

            foreach (var i in QTreeBuilder.Nodes)
            {
                i.BuildCullPlanes();
            }

            Root.ClearShadowNodes();
            foreach(var i in ShadowObjectDictionary)
            {
                this.PushShadowNode(i.Value.SceneNode, i.Value.IsDynamic);
            }

            MarkAllLeafDirty();
        }
        public void MarkAllLeafDirty()
        {
            Root.Iterate(static (node) =>
            {
                if (node.NodeType == TtQNode.ENodeType.Leaf)
                {
                    node.Leaf.IsDirty = true;
                }
                return true;
            });
        }
        [Category("Debug")]
        public int AlivePage
        {
            get => MaxPageCount - PageAllocator.Count;
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
        public unsafe void Initialize(int maxDeepLevel, DBoundingBox2D aabb, int maxTile)
        {
            MaxDeepLevel = maxDeepLevel;

            int side = 0;
            int total = 0;
            CountFullTreeNode(MaxDeepLevel, out side, out total);

            PageAllocator.Clear();
            MaxPageCount = Math.Min(side * side, maxTile);
            for (int i = 0; i < MaxPageCount; i++)
            {
                PageAllocator.Push(MaxPageCount - 1 - i);
            }

            QTreeBuilder = new TtFullQTreeBuilder();
            QTreeBuilder.InitBuilder(MaxDeepLevel, in aabb);
            QNodes = QTreeBuilder.Nodes;
            AdvShadowNodeDatas = new TtCpu2GpuBuffer<FAdvShadowNodeData>();
            AdvShadowNodeDatas.Initialize(NxRHI.EBufferType.BFT_SRV);
            AdvShadowNodeDatas.SetSize(QTreeBuilder.AdvShadowNodeDatas.Length);
            
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
        public void FreeChildTree(TtQNode node)
        {
            if (node.NodeType == TtQNode.ENodeType.DeathNode)
            {
                return;
            }
            if (node.NodeType == TtQNode.ENodeType.Node && node.Child00 != null)
            {
                FreeChildTree(node.Child00);
                FreeChildTree(node.Child01);
                FreeChildTree(node.Child10);
                FreeChildTree(node.Child11);

                //node.PushShadowObjects(this, node.Child00.ShadowObjects);
                //node.PushShadowObjects(this, node.Child01.ShadowObjects);
                //node.PushShadowObjects(this, node.Child10.ShadowObjects);
                //node.PushShadowObjects(this, node.Child11.ShadowObjects);

                //node.Child00.ShadowObjects.Clear();
                //node.Child01.ShadowObjects.Clear();
                //node.Child10.ShadowObjects.Clear();
                //node.Child11.ShadowObjects.Clear();

                node.Child00.AsDeathNode();
                node.Child01.AsDeathNode();
                node.Child10.AsDeathNode();
                node.Child11.AsDeathNode();
            }
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
        public uint UpdateShadowMapTime
        {
            get => mUpdateShadowMapTime;
        }
        public DVector2 CameralPosition;
        public unsafe void UpdateQTree(TtWorld world, TtCamera cameral, int limitLeaf)
        {
            using (new Profiler.TimeScopeHelper(ScopeUpdateQTree))
            {
                CameralPosition = new DVector2(cameral.GetPosition().X, cameral.GetPosition().Z);
                CheckShadowObjectChanged();

                UpdateQTree(in CameralPosition, Root);

                QTreeBuilder.BuildNodeArray();

                fixed (FAdvShadowNodeData* p = &QTreeBuilder.AdvShadowNodeDatas[0])
                {
                    AdvShadowNodeDatas.UpdateData(0, p, QTreeBuilder.AdvShadowNodeDatas.Length * sizeof(FAdvShadowNodeData));
                    //AdvShadowNodeDatas.Flush2GPU();
                }

                mUpdateShadowMapNodes.Clear();
                GetDirtyLeafs(world, Root, mUpdateShadowMapNodes);
                mUpdateShadowMapNodes.Sort((x, y) =>
                {
                    //todo: compare AABB size
                    //x.AABB.GetSize()
                    if (x.UpdateShadowMapTime > y.UpdateShadowMapTime)
                        return 1;
                    else if (x.UpdateShadowMapTime < y.UpdateShadowMapTime)
                        return -1;
                    else
                    {
                        if (x.DeepLevel < y.DeepLevel)
                            return 1;
                        else if (x.DeepLevel > y.DeepLevel)
                            return -1;
                        else
                            return y.ShadowObjects.Count.CompareTo(x.ShadowObjects.Count);
                    }
                });
                if (mUpdateShadowMapNodes.Count > limitLeaf)
                {
                    mUpdateShadowMapNodes.RemoveRange(limitLeaf, mUpdateShadowMapNodes.Count - limitLeaf);
                }

                var count = mUpdateShadowMapNodes.Count;
                for (int i = 0; i < count; i++)
                {
                    var cur = mUpdateShadowMapNodes[i].Parent;
                    int parentCount = 0;
                    const int MaxParentCount = 2;
                    while (cur != null && parentCount < MaxParentCount)
                    {
                        parentCount++;
                        if (cur.ShadowObjects.Count > 0)
                        {
                            bool bFind = false;
                            foreach (var j in mUpdateShadowMapNodes)
                            {
                                if (j == cur)
                                {
                                    bFind = true;
                                    break;
                                }
                            }
                            if (bFind == false)
                            {
                                cur.Leaf.IsDirty = true;
                                mUpdateShadowMapNodes.Add(cur);
                            }
                        }
                        cur = cur.Parent;
                    }
                }
                mUpdateShadowMapTime++;
            }   
        }
        public void CheckShadowObjectChanged()
        {
            foreach (var i in ShadowObjectDictionary)
            {
                if (i.Value.SceneNode.BoundVolume.AbsAABB == i.Value.AABB)
                {
                    continue;
                }
                i.Value.AABB = i.Value.SceneNode.BoundVolume.AbsAABB;
                Root.RemoveShadowNode(i.Value.SceneNode);
                PushShadowObject(Root, i.Value);
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
            if (node.NodeType == TtQNode.ENodeType.Leaf)
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
                    FreeChildTree(node);
                }

                node.AsLeaf();
                return;
            }

            node.AsNode();

            //if (node.ShadowObjects.Count > 0)
            if (node.Child00.NodeType == TtQNode.ENodeType.DeathNode)
            {
                node.Child00.PushShadowObjects(this, node.ShadowObjects);
                node.Child01.PushShadowObjects(this, node.ShadowObjects);
                node.Child10.PushShadowObjects(this, node.ShadowObjects);
                node.Child11.PushShadowObjects(this, node.ShadowObjects);
                //node.ShadowObjects.Clear();
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
        
        public void PushShadowNode(TtNode node, bool isDynamic)
        {
            TtShadowObject shadowObj;
            if (ShadowObjectDictionary.TryGetValue(node, out shadowObj) == false)
            {
                shadowObj = new TtShadowObject();
                ShadowObjectDictionary.Add(node, shadowObj);
            }

            ref var aabb = ref node.RefAbsAABB;
            
            shadowObj.SceneNode = node;
            shadowObj.AABB = node.BoundVolume.AbsAABB;
            shadowObj.IsDynamic = isDynamic;
            PushShadowObject(Root, shadowObj);
        }
        private void PushShadowObject(TtQNode node, TtShadowObject shadowObject)
        {
            if (node.PushObject(shadowObject) == false)
            {
                return;
            }

            if (node.Child00 != null && node.Child00.NodeType != TtQNode.ENodeType.DeathNode)
            {
                PushShadowObject(node.Child00, shadowObject);
                PushShadowObject(node.Child01, shadowObject);
                PushShadowObject(node.Child10, shadowObject);
                PushShadowObject(node.Child11, shadowObject);
            }
        }
        public void RemoveShadowNode(TtNode node)
        {
            TtShadowObject shadowObj;
            if (ShadowObjectDictionary.TryGetValue(node, out shadowObj) == false)
                return;
            ShadowObjectDictionary.Remove(node);
            this.Root.RemoveShadowNode(node);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AdvanceShadow", "AdvanceShadow", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtAdvanceShadowNode.TtAdvanceShadowData), DefaultNamePrefix = "AdvanceShadow")]
    public class TtAdvanceShadowNode : TtSceneActorNode
    {
        public class TtAdvanceShadowData : TtNodeData
        {
            [Rtti.Meta("")]
            [Category("Option")]
            public DVector2 BoxCenter { get; set; } = DVector2.Zero;
            [Rtti.Meta("")]
            [Category("Option")]
            public double BoxExtent { get; set; } = 1024;
            [Rtti.Meta("")]
            [Category("Option")]
            public int MaxDeepLevel { get; set; } = 8;
            [Rtti.Meta("")]
            [Category("Option")]
            public float MaxShadowDistance { get; set; } = 500.0f;
            [Rtti.Meta("")]
            [Category("Option")]
            public int ShadowMapPage { get; set; } = 512;
            [Rtti.Meta("")]
            [Category("Option")]
            public int MaxDirtyPagePerFrame { get; set; } = 3;
        }

        public TtAdvanceShadowMapNode mRenderGraphNode;
        public TtQTree mShadowMapTree = null;
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            mShadowMapTree = new TtQTree();

            var data1 = GetNodeData<TtAdvanceShadowData>();
            DBoundingBox2D aabb = new DBoundingBox2D(data1.BoxCenter, data1.BoxExtent);
            mShadowMapTree.Initialize(data1.MaxDeepLevel, aabb, data1.ShadowMapPage);
            mShadowMapTree.MaxShadowDistance = data1.MaxShadowDistance;

            return ret;
        }
        protected override TtTask OnPostInitNode(TtNode parent)
        {
            return base.OnPostInitNode(parent);
        }
        public override void Dispose()
        {
            ShowDebugger = false;
            base.Dispose();
        }
        public override TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtAdvanceShadowNode>.Scope;
        }
        bool mPushShadowNode = false;

        [ThreadStatic]
        static Profiler.TimeScope mScopeTree;
        static Profiler.TimeScope ScopeTree
        {
            get
            {
                if (mScopeTree == null)
                    mScopeTree = new Profiler.TimeScope(typeof(TtAdvanceShadowNode), nameof(QTree));
                return mScopeTree;
            }
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            base.OnTickLogic(args);

            if (mShadowMapTree != null)
            {
                using (new Profiler.TimeScopeHelper(ScopeTree))
                {
                    UpdateLightDirection(this.GetWorld());
                    if (mPushShadowNode == false)
                    {
                        this.GetWorld().Root.IterateNodes(static (node, arg) =>
                        {
                            var mShadowMapTree = arg as TtQTree;
                            if (node.IsCastShadow)
                            {
                                mShadowMapTree.PushShadowNode(node, false);
                            }
                            return true;
                        }, mShadowMapTree);
                        mPushShadowNode = true;
                    }
                    //test code,for renderdoc capture
                    //mShadowMapTree.MarkAllLeafDirty();
                }

                var cullingNode = args.Policy.FindFirstNode<TtCpuCullingNode>();
                if (cullingNode != null && cullingNode.VisParameter.CullCamera != null)
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
        [Category("Option")]
        public float EsmConstant
        {
            get => mShadowMapTree.EsmConstant;
            set
            {
                mShadowMapTree.EsmConstant = value;
            }
        }
        [Category("Option")]
        public float MaxExp
        {
            get => mShadowMapTree.MaxExp;
            set
            {
                mShadowMapTree.MaxExp = value;
            }
        }
        [Category("Option")]
        public float GaussSigma
        {
            get => mShadowMapTree.GaussSigma;
            set
            {
                mShadowMapTree.GaussSigma = value;
            }
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
                    mShadowMapTree.PushShadowNode(node, isDynamic);
                }
                return true;
            }, null);
        }
        public void RemoveShadowNode(TtNode parent)
        {
            this.mShadowMapTree.Root.RemoveShadowNode(parent);
        }
    }
}
