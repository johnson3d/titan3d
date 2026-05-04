using System;
using System.Collections.Generic;
using System.ComponentModel;

// =============================================================================
// TtBVHDebugNode
//
// A scene node that demonstrates and visualizes TtDynamicBVH<T>:
//   - Randomly generates N AABBs inside a configurable box.
//   - Lets the user choose between Dynamic (incremental Insert) and Bulk
//     (Morton/LBVH BuildFromLeaves) construction.
//   - Optional Optimize() pass after build (SAH-driven local rotations).
//   - Runs a query demo (random AABB overlap + ray cast) and highlights the
//     hit leaves in red until the next query / clear.
//
// Visualization strategy:
//   Every frame we walk the BVH in OnGatherVisibleMeshes and submit each
//   node's AABB through TtVisParameter.AddAABB (yellow = internal node,
//   green = leaf, red = highlighted by last query). This is the same path
//   TtBoundsOctreeNode.DrawAllBounds uses, so we don't need any per-node
//   mesh / placement / child node — just one debug node and a tree.
//
// Hierarchy inspection:
//   Use the [Cmd] DumpHierarchyToLog button — it prints an indented dump of
//   the BVH (depth, node id, kind, AABB) to Profiler.Log.
//
// Usage in the editor:
//   1) Right-click the world outliner -> Add Node -> Graphics -> BVHDebugNode.
//   2) Tweak NumLeaves / Seed / SpaceExtent in the property grid.
//   3) Click [Cmd] BuildDynamic or [Cmd] BuildBulk to (re)build the tree.
//   4) Optionally click [Cmd] OptimizeNow then [Cmd] RunQueryDemo.
//   5) Click [Cmd] DumpHierarchyToLog to inspect the tree shape.
// =============================================================================

namespace EngineNS.Bricks.Collision.BVH
{
    [Bricks.CodeBuilder.ContextMenu("BVHDebugNode", "Graphics\\BVHDebugNode", GamePlay.Scene.TtNode.EditorKeyword)]
    [GamePlay.Scene.TtNode(NodeDataType = typeof(TtBVHDebugNode.TtBVHDebugNodeData), DefaultNamePrefix = "BVHDebug")]
    [Rtti.Meta]
    public partial class TtBVHDebugNode : GamePlay.Scene.TtNode
    {
        public enum EBuildMode
        {
            Dynamic,    // incremental InsertProxy
            Bulk,       // Morton/LBVH BuildFromLeaves
        }

        [Rtti.Meta]
        public class TtBVHDebugNodeData : GamePlay.Scene.TtNodeData
        {
            public TtBVHDebugNodeData()
            {
                HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            }

            [Rtti.Meta]
            [Category("BVH")]
            public int NumLeaves { get; set; } = 20;

            [Rtti.Meta]
            [Category("BVH")]
            public int Seed { get; set; } = 1234;

            [Rtti.Meta]
            [Category("BVH")]
            public Vector3 SpaceExtent { get; set; } = new Vector3(50, 20, 50);

            [Rtti.Meta]
            [Category("BVH")]
            public Vector3 LeafMinSize { get; set; } = new Vector3(1, 1, 1);

            [Rtti.Meta]
            [Category("BVH")]
            public Vector3 LeafMaxSize { get; set; } = new Vector3(5, 5, 5);

            [Rtti.Meta]
            [Category("BVH")]
            public float Margin { get; set; } = 0.5f;

            [Rtti.Meta]
            [Category("BVH")]
            public EBuildMode BuildMode { get; set; } = EBuildMode.Bulk;

            [Rtti.Meta]
            [Category("BVH")]
            public int OptimizeIterations { get; set; } = 0;
        }

        // ------------------------------------------------------------------
        // Live state
        // ------------------------------------------------------------------
        TtDynamicBVH<int> mBvh;
        Aabb[] mLeafBoxes;                                  // The randomly generated source AABBs.

        // Proxy ids that are currently highlighted by the latest query demo.
        // Touched on the gameplay thread (set in RunQueryDemo) and read on the
        // gather thread (OnGatherVisibleMeshes). Concurrent-modification risk
        // is acceptable for a debug visualization: a flicker for one frame is
        // harmless, and we never resize the BVH during gather.
        readonly HashSet<int> mHighlightedProxies = new HashSet<int>();

        // Optional toggles for what to draw — useful when the tree gets dense.
        bool mDrawInternalNodes = true;
        bool mDrawLeafNodes = true;

        // Last build / query stats — exposed read-only on the property grid so
        // users can see what the tree looks like without opening a debugger.
        int mStatProxyCount;
        int mStatHeight;
        double mStatSAHCost;
        int mStatLastQueryHits;
        string mStatBuildSummary = "(not built)";

        // Wireframe colors (mirrored from TtBoundsOctreeNode.DrawAllBounds style).
        static readonly Color4f ColorInternal  = new Color4f(1.0f, 0.95f, 0.85f, 0.10f);
        static readonly Color4f ColorLeaf      = new Color4f(1.0f, 0.10f, 0.90f, 0.20f);
        static readonly Color4f ColorHighlight = new Color4f(1.0f, 1.00f, 0.10f, 0.10f);

        // ------------------------------------------------------------------
        // Wireframe-mesh cache
        //
        // Building one MakeBox per BVH node per frame is wasteful when the tree
        // doesn't change. We bake one TtRenderMesh per leaf/internal node into
        // mNodeMeshCache and just push them via AddVisibleMesh in subsequent
        // frames. Highlighted nodes still go through the transient AddAABB
        // path (the highlight color is baked into the wire vertices, so cached
        // green/yellow boxes can't be re-tinted on the fly).
        //
        // The cache is invalidated wholesale (DisposeMeshCache) whenever the
        // BVH topology or highlight set changes — both events are explicit
        // user actions (build / optimize / query), so we never need to do
        // partial cache eviction.
        //
        // IMPORTANT: cached meshes hold references into mSharedVB / mSharedIB
        // (our OWN TtTransientBuffer instances, NOT the per-frame ones exposed
        // by TtVisParameter — those get Reset() at the end of every frame and
        // would invalidate any mesh that referenced them). We feed our own
        // buffers into vp.CreateAABB(.., sharedVB, sharedIB) so the cached
        // meshes stay valid across frames. The buffers are Reset() only when
        // we tear down the cache (build / optimize / clear), and Disposed
        // alongside the node.
        // ------------------------------------------------------------------
        readonly Dictionary<int, Graphics.Mesh.TtRenderMesh> mNodeMeshCache = new Dictionary<int, Graphics.Mesh.TtRenderMesh>();
        NxRHI.TtTransientBuffer mSharedVB = new NxRHI.TtTransientBuffer();
        NxRHI.TtTransientBuffer mSharedIB = new NxRHI.TtTransientBuffer();

        // ==================================================================
        // PG properties
        // ==================================================================

        [Category("BVH")]
        public int NumLeaves
        {
            get => (NodeData as TtBVHDebugNodeData)?.NumLeaves ?? 0;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.NumLeaves = Math.Max(1, value); }
        }

        [Category("BVH")]
        public int Seed
        {
            get => (NodeData as TtBVHDebugNodeData)?.Seed ?? 0;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.Seed = value; }
        }

        [Category("BVH")]
        public Vector3 SpaceExtent
        {
            get => (NodeData as TtBVHDebugNodeData)?.SpaceExtent ?? Vector3.One;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.SpaceExtent = value; }
        }

        [Category("BVH")]
        public Vector3 LeafMinSize
        {
            get => (NodeData as TtBVHDebugNodeData)?.LeafMinSize ?? Vector3.One;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.LeafMinSize = value; }
        }

        [Category("BVH")]
        public Vector3 LeafMaxSize
        {
            get => (NodeData as TtBVHDebugNodeData)?.LeafMaxSize ?? Vector3.One;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.LeafMaxSize = value; }
        }

        [Category("BVH")]
        public float Margin
        {
            get => (NodeData as TtBVHDebugNodeData)?.Margin ?? 0;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.Margin = Math.Max(0, value); }
        }

        [Category("BVH")]
        public EBuildMode BuildMode
        {
            get => (NodeData as TtBVHDebugNodeData)?.BuildMode ?? EBuildMode.Bulk;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.BuildMode = value; }
        }

        [Category("BVH")]
        public int OptimizeIterations
        {
            get => (NodeData as TtBVHDebugNodeData)?.OptimizeIterations ?? 0;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.OptimizeIterations = Math.Max(0, value); }
        }

        // Read-only stats.
        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public int Stat_ProxyCount { get => mStatProxyCount; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public int Stat_TreeHeight { get => mStatHeight; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public double Stat_SAHCost { get => mStatSAHCost; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public int Stat_LastQueryHits { get => mStatLastQueryHits; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public string Stat_BuildSummary { get => mStatBuildSummary; set { } }

        // ------------------------------------------------------------------
        // PG action buttons. The PG renders bool properties as toggle buttons;
        // setting them to true triggers the action and immediately resets to
        // false so the button can be clicked again.
        // ------------------------------------------------------------------

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] BuildDynamic")]
        public bool CmdBuildDynamic
        {
            get => false;
            set
            {
                if (!value) return;
                BuildMode = EBuildMode.Dynamic;
                DoRebuild();
            }
        }

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] BuildBulk (Morton/LBVH)")]
        public bool CmdBuildBulk
        {
            get => false;
            set
            {
                if (!value) return;
                BuildMode = EBuildMode.Bulk;
                DoRebuild();
            }
        }

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] OptimizeNow")]
        public bool CmdOptimizeNow
        {
            get => false;
            set
            {
                if (!value) return;
                if (mBvh == null) return;
                int swaps = mBvh.Optimize(Math.Max(1, OptimizeIterations == 0 ? 4 : OptimizeIterations));
                // SAH rotations changed internal-node AABBs (and possibly leaf
                // groupings), so cached meshes for those nodes are stale.
                DisposeMeshCache();
                mStatHeight = mBvh.GetHeight();
                mStatSAHCost = mBvh.GetSAHCost();
                mStatBuildSummary = $"Optimize: {swaps} swap(s), height={mStatHeight}, SAH={mStatSAHCost:F2}";
                Profiler.Log.WriteLineSingle("[BVHDebug] " + mStatBuildSummary);
            }
        }

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] RunQueryDemo")]
        public bool CmdRunQueryDemo
        {
            get => false;
            set
            {
                if (!value) return;
                RunQueryDemo();
            }
        }

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] ClearTree")]
        public bool CmdClearTree
        {
            get => false;
            set
            {
                if (!value) return;
                ClearAll();
            }
        }

        [Category("BVH/Commands")]
        [DisplayName("[Cmd] DumpHierarchyToLog")]
        public bool CmdDumpHierarchyToLog
        {
            get => false;
            set
            {
                if (!value) return;
                DumpHierarchyToLog();
            }
        }

        // Toggles for what to draw in OnGatherVisibleMeshes.
        [Category("BVH/Display")]
        public bool DrawInternalNodes
        {
            get => mDrawInternalNodes;
            set => mDrawInternalNodes = value;
        }

        [Category("BVH/Display")]
        public bool DrawLeafNodes
        {
            get => mDrawLeafNodes;
            set => mDrawLeafNodes = value;
        }

        // ==================================================================
        // Initialization
        // ==================================================================

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, GamePlay.Scene.TtNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtBVHDebugNodeData == null)
                data = new TtBVHDebugNodeData();
            if (await base.InitializeNode(world, data, GamePlay.Scene.EBoundVolumeType.Box, placementType) == false)
                return false;

            // We draw all AABBs ourselves in OnGatherVisibleMeshes via vp.AddAABB,
            // so make sure we get gathered every frame regardless of camera culling
            // (the BVH is centered on the node origin, but individual leaf boxes can
            // sit far away from it — DiscardAABB avoids being culled by our own
            // bound volume).
            SetStyle(GamePlay.Scene.TtNode.ENodeStyles.DiscardAABB
                   | GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
            return true;
        }

        protected override async Thread.Async.TtTask OnPostInitNode(GamePlay.Scene.TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            // Auto-build once when the node enters the scene so users see something
            // immediately after dragging the node into the world.
            DoRebuild();
        }

        // ==================================================================
        // Build pipeline (synchronous — no scene-actor spawning anymore)
        // ==================================================================

        void DoRebuild()
        {
            // Topology is about to change wholesale — drop every cached mesh so
            // node ids that get reused in the new tree don't render with stale
            // vertex positions / colors from the old tree.
            DisposeMeshCache();
            GenerateRandomLeaves();

            mBvh = new TtDynamicBVH<int>(Math.Max(16, NumLeaves * 2), Margin, 2.0f);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            if (BuildMode == EBuildMode.Bulk)
            {
                var boxList = new List<Aabb>(mLeafBoxes);
                var dataList = new List<int>(mLeafBoxes.Length);
                for (int i = 0; i < mLeafBoxes.Length; i++) dataList.Add(i);
                mBvh.BuildFromLeaves(boxList, dataList);
            }
            else
            {
                for (int i = 0; i < mLeafBoxes.Length; i++)
                    mBvh.InsertProxy(in mLeafBoxes[i], i);
            }

            sw.Stop();
            long buildMicros = sw.ElapsedTicks * 1_000_000L / System.Diagnostics.Stopwatch.Frequency;

            int swaps = 0;
            if (OptimizeIterations > 0)
                swaps = mBvh.Optimize(OptimizeIterations);

            mHighlightedProxies.Clear();
            mStatProxyCount = mBvh.ProxyCount;
            mStatHeight = mBvh.GetHeight();
            mStatSAHCost = mBvh.GetSAHCost();
            mStatLastQueryHits = 0;
            mStatBuildSummary = $"{BuildMode} build: leaves={mStatProxyCount}, height={mStatHeight}, SAH={mStatSAHCost:F2}, build={buildMicros}us, opt-swaps={swaps}";
            Profiler.Log.WriteLineSingle("[BVHDebug] " + mStatBuildSummary);
        }

        // Deterministic PRNG (System.Random with Seed) so the same Seed always
        // yields the same scene — easier to compare Dynamic vs Bulk output.
        void GenerateRandomLeaves()
        {
            int n = Math.Max(1, NumLeaves);
            mLeafBoxes = new Aabb[n];

            var rng = new System.Random(Seed);
            var spaceExt = SpaceExtent;
            var minSz = LeafMinSize;
            var maxSz = LeafMaxSize;
            var origin = Placement != null ? Placement.AbsTransform.Position : DVector3.Zero;

            for (int i = 0; i < n; i++)
            {
                // Center uniformly in [-spaceExt, +spaceExt] around the node origin.
                var cx = origin.X + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.X;
                var cy = origin.Y + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.Y;
                var cz = origin.Z + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.Z;

                // Per-axis half-extent in [minSz, maxSz].
                var ex = (float)(minSz.X + rng.NextDouble() * Math.Max(0, maxSz.X - minSz.X));
                var ey = (float)(minSz.Y + rng.NextDouble() * Math.Max(0, maxSz.Y - minSz.Y));
                var ez = (float)(minSz.Z + rng.NextDouble() * Math.Max(0, maxSz.Z - minSz.Z));

                mLeafBoxes[i] = new Aabb(new DVector3(cx, cy, cz), new Vector3(ex, ey, ez));
            }
        }

        // ==================================================================
        // Visualization: every frame push every BVH node's AABB as wireframe.
        // Cache hits go through AddVisibleMesh (no per-frame mesh build);
        // cache misses lazily bake one persistent TtRenderMesh per node.
        // Highlighted leaves go through the transient AddAABB path because
        // the highlight color is vertex-baked and we can't re-tint a cached
        // mesh.
        // ==================================================================

        public override void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
            if (mBvh == null || mBvh.ProxyCount == 0) 
                return;

            var world = rp.World;
            var trans = FTransform.Identity;
            bool drawInner = mDrawInternalNodes;
            bool drawLeaf  = mDrawLeafNodes;
            var hl = mHighlightedProxies;

            mBvh.DebugTraverse((int nodeId, int depth, in Aabb box, bool isLeaf) =>
            {
                if (isLeaf)
                {
                    if (!drawLeaf) 
                        return true;
                    if (hl.Contains(nodeId))
                    {
                        // Highlighted leaves: per-frame transient mesh so the
                        // red color shows up immediately without invalidating
                        // the persistent cache.
                        rp.AddAABB(in box, in ColorHighlight, in trans);
                    }
                    else
                    {
                        EmitCachedAabb(rp, nodeId, in box, in ColorLeaf);
                    }
                }
                else
                {
                    if (!drawInner)
                        return true;
                    EmitCachedAabb(rp, nodeId, in box, in ColorInternal);
                }
                return true;
            });
        }

        // Look up (or lazily build) the cached wireframe mesh for one BVH node
        // and push it into the visible-mesh list. Cache misses go through
        // vp.CreateAABB (the explicit-shared-buffer overload) so the resulting
        // mesh references our own TtTransientBuffer pair, not vp's per-frame
        // pair, and therefore survives the frame boundary.
        void EmitCachedAabb(GamePlay.TtWorld.TtVisParameter rp,
                            int nodeId, in Aabb box, in Color4f color)
        {
            if (!mNodeMeshCache.TryGetValue(nodeId, out var mesh) || mesh == null)
            {
                var trans = FTransform.Identity;
                mesh = rp.CreateAABB(in box, in color, in trans, null, mSharedVB, mSharedIB);
                if (mesh == null)
                    return;
                mNodeMeshCache[nodeId] = mesh;
            }
            rp.AddVisibleMesh(mesh);
        }

        // Tear down the persistent mesh cache. Called on every BVH topology
        // change (build / optimize / clear) and from Dispose. Also resets the
        // shared transient buffers so cross-frame allocations don't leak —
        // since every cached mesh is dropped, no live reference into the
        // buffers can survive past this call.
        void DisposeMeshCache()
        {
            if (mNodeMeshCache.Count > 0)
            {
                foreach (var kv in mNodeMeshCache)
                {
                    var m = kv.Value;
                    CoreSDK.DisposeObject(ref m);
                }
                mNodeMeshCache.Clear();
            }
            mSharedVB?.Reset();
            mSharedIB?.Reset();
        }

        // ==================================================================
        // Hierarchy dump (replaces the old "spawn child nodes for outliner"
        // approach — now we just print an indented tree to Profiler.Log).
        // ==================================================================

        void DumpHierarchyToLog()
        {
            if (mBvh == null || mBvh.ProxyCount == 0)
            {
                Profiler.Log.WriteLineSingle("[BVHDebug] DumpHierarchy: tree is empty.");
                return;
            }
            var sb = new System.Text.StringBuilder(256);
            sb.Append("[BVHDebug] Hierarchy (proxyCount=").Append(mStatProxyCount)
              .Append(", height=").Append(mStatHeight)
              .Append(", SAH=").Append(mStatSAHCost.ToString("F2")).Append(")\n");

            mBvh.DebugTraverse((int nodeId, int depth, in Aabb box, bool isLeaf) =>
            {
                for (int i = 0; i < depth; i++) sb.Append("  ");
                sb.Append(isLeaf ? "Leaf#" : "Inner#").Append(nodeId);
                sb.Append("  center=(").Append(box.Center.X.ToString("F1")).Append(',')
                                       .Append(box.Center.Y.ToString("F1")).Append(',')
                                       .Append(box.Center.Z.ToString("F1")).Append(')');
                sb.Append("  ext=(").Append(box.Extent.X.ToString("F1")).Append(',')
                                    .Append(box.Extent.Y.ToString("F1")).Append(',')
                                    .Append(box.Extent.Z.ToString("F1")).Append(")\n");
                return true;
            });
            Profiler.Log.WriteLineSingle(sb.ToString());
        }

        void ClearAll()
        {
            DisposeMeshCache();
            mHighlightedProxies.Clear();
            mBvh = null;
            mLeafBoxes = null;
            mStatProxyCount = 0;
            mStatHeight = 0;
            mStatSAHCost = 0;
            mStatLastQueryHits = 0;
            mStatBuildSummary = "(cleared)";
        }

        // ==================================================================
        // Query demo
        // ==================================================================

        void RunQueryDemo()
        {
            if (mBvh == null || mBvh.ProxyCount == 0)
            {
                Profiler.Log.WriteLineSingle("[BVHDebug] RunQueryDemo: tree is empty, build first.");
                return;
            }
            mHighlightedProxies.Clear();

            var rng = new System.Random(Seed ^ 0x5A5A5A5A);
            var origin = Placement != null ? Placement.AbsTransform.Position : DVector3.Zero;
            var spaceExt = SpaceExtent;

            // 1) AABB overlap query — pick a random query box covering ~25% of the space.
            var qcx = origin.X + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.X * 0.5;
            var qcy = origin.Y + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.Y * 0.5;
            var qcz = origin.Z + (rng.NextDouble() * 2.0 - 1.0) * spaceExt.Z * 0.5;
            var qExt = new Vector3(spaceExt.X * 0.25f, spaceExt.Y * 0.25f, spaceExt.Z * 0.25f);
            var queryBox = new Aabb(new DVector3(qcx, qcy, qcz), qExt);

            int aabbHits = 0;
            mBvh.QueryAabb(in queryBox, (int proxyId, int payloadIndex) =>
            {
                aabbHits++;
                mHighlightedProxies.Add(proxyId);
                return true;
            });

            // 2) Ray cast — origin at camera-ish height, direction into the volume.
            var rayOrigin = new DVector3(origin.X - spaceExt.X * 1.5f, origin.Y, origin.Z - spaceExt.Z * 1.5f);
            var rayDir = new DVector3(1.0, 0.0, 1.0); // 45deg in XZ plane
            int rayHits = 0;
            mBvh.RayCast(in rayOrigin, in rayDir, double.PositiveInfinity,
                (int proxyId, int payloadIndex, in DVector3 o, in DVector3 d, double maxFraction) =>
                {
                    rayHits++;
                    mHighlightedProxies.Add(proxyId);
                    return maxFraction; // continue scanning, don't clip
                });

            // 3) Self-overlap pairs — informational only, log the count.
            int pairCount = 0;
            mBvh.QuerySelfPairs((int a, int payloadA, int b, int payloadB) =>
            {
                pairCount++;
                return true;
            });

            mStatLastQueryHits = aabbHits + rayHits;
            mStatBuildSummary = $"Query: AABB hits={aabbHits}, RayCast hits={rayHits}, SelfPairs={pairCount}";
            Profiler.Log.WriteLineSingle("[BVHDebug] " + mStatBuildSummary);
        }

        public override void Dispose()
        {
            ClearAll();
            CoreSDK.DisposeObject(ref mSharedVB);
            CoreSDK.DisposeObject(ref mSharedIB);
            base.Dispose();
        }
    }
}
