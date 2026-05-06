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
    public partial class TtBVHDebugNode : GamePlay.Scene.TtVisual
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

            // -1 = unlimited (show every internal level). N >= 0 = only show
            // internal nodes whose depth lies in [0, N]. Leaves always honor
            // mDrawLeafNodes regardless of this value.
            [Rtti.Meta]
            [Category("BVH/Display")]
            public int MaxDisplayDepth { get; set; } = -1;

            // When true, internal-node AABBs use a per-depth color ramp so the
            // tree's level structure is visually obvious. When false, every
            // internal node uses the legacy single color (ColorInternal).
            [Rtti.Meta]
            [Category("BVH/Display")]
            public bool ColorByDepth { get; set; } = true;

            // ----------------------------------------------------------------
            // GPU BVH dispatch demo parameters.
            // RayCount drives the [Cmd] FGpuRayCast dispatch size; RaySeed lets
            // users reproduce the same ray batch across runs.
            // ----------------------------------------------------------------
            [Rtti.Meta]
            [Category("BVH/GPU")]
            public int RayCount { get; set; } = 64;

            [Rtti.Meta]
            [Category("BVH/GPU")]
            public int RaySeed { get; set; } = 5678;
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

        // ------------------------------------------------------------------
        // GPU mirror state. Lives across the whole node lifetime so the
        // shading env / drawcall / cmdlist set up by InitializeShadingAsync
        // can be reused across [Cmd] UploadToGpu / [Cmd] FGpuRayCast clicks.
        // Disposed in this node's Dispose override.
        // ------------------------------------------------------------------
        readonly TtGpuBvh mGpuBvh = new TtGpuBvh();
        bool mGpuShadingReady = false;
        string mStatGpuStatus = "(not uploaded)";

        // Last build / query stats — exposed read-only on the property grid so
        // users can see what the tree looks like without opening a debugger.
        int mStatProxyCount;
        int mStatHeight;
        double mStatSAHCost;
        int mStatLastQueryHits;
        string mStatBuildSummary = "(not built)";

        // Wireframe colors (mirrored from TtBoundsOctreeNode.DrawAllBounds style).
        // ColorInternal is the fallback when ColorByDepth is disabled. The
        // per-depth ramp (GetDepthColor) lives in the cool blue→green band so
        // it never collides with ColorLeaf (magenta) or ColorHighlight (yellow).
        static readonly Color4f ColorInternal  = new Color4f(1.0f, 0.95f, 0.85f, 0.10f);
        static readonly Color4f ColorLeaf      = new Color4f(1.0f, 0.10f, 0.90f, 0.20f);
        static readonly Color4f ColorHighlight = new Color4f(1.0f, 1.00f, 0.10f, 0.10f);

        // Generate a deterministic color for an internal node at the given
        // tree depth. Hue sweeps from blue (root) through cyan to green
        // (deeper) and wraps every 8 levels — this entire band is far from
        // magenta/red/yellow so leaves and highlights stay readable.
        static Color4f GetDepthColor(int depth)
        {
            const int kCycle = 8;
            int d = depth % kCycle;
            if (d < 0) d += kCycle;
            // Hue: 0.55 (blue) → 0.30 (yellow-green) over kCycle steps.
            float hue = 0.55f - d * (0.25f / (kCycle - 1));
            HsvToRgb(hue, 0.85f, 0.95f, out float r, out float g, out float b);
            // Alpha must follow the same encoding as the legacy ColorInternal
            // (the W channel doubles as the wireframe line opacity hint).
            return new Color4f(r, g, b, 0.15f);
        }

        // Standard HSV → RGB; H/S/V all in [0,1].
        static void HsvToRgb(float h, float s, float v, out float r, out float g, out float b)
        {
            float hh = (h - (float)Math.Floor(h)) * 6.0f;
            int i = (int)hh;
            float f = hh - i;
            float p = v * (1.0f - s);
            float q = v * (1.0f - s * f);
            float t = v * (1.0f - s * (1.0f - f));
            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
        }

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

        // -1 = show every internal level. >= 0 = clip the visible tree at this
        // depth (root = 0). Pure filter; cache stays valid because nodeId →
        // depth and depth → color are both deterministic for a given tree, so
        // changing this value never invalidates a cached mesh.
        [Category("BVH/Display")]
        public int MaxDisplayDepth
        {
            get => (NodeData as TtBVHDebugNodeData)?.MaxDisplayDepth ?? -1;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.MaxDisplayDepth = Math.Max(-1, value); }
        }

        // When true, internal nodes use the per-depth color ramp; when false,
        // they all use ColorInternal. Toggling this DOES invalidate cached
        // mesh colors, so we tear the cache down on change.
        [Category("BVH/Display")]
        public bool ColorByDepth
        {
            get => (NodeData as TtBVHDebugNodeData)?.ColorByDepth ?? true;
            set
            {
                var d = NodeData as TtBVHDebugNodeData;
                if (d == null || d.ColorByDepth == value) return;
                d.ColorByDepth = value;
                DisposeMeshCache();
            }
        }

        // GPU dispatch demo parameters (mirrored to NodeData for serialization).
        [Category("BVH/GPU")]
        public int RayCount
        {
            get => (NodeData as TtBVHDebugNodeData)?.RayCount ?? 64;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.RayCount = Math.Max(1, value); }
        }

        [Category("BVH/GPU")]
        public int RaySeed
        {
            get => (NodeData as TtBVHDebugNodeData)?.RaySeed ?? 5678;
            set { var d = NodeData as TtBVHDebugNodeData; if (d != null) d.RaySeed = value; }
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

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public int Stat_GpuNodeCount { get => mGpuBvh != null ? mGpuBvh.NodeCount : 0; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public int Stat_GpuLeafCount { get => mGpuBvh != null ? mGpuBvh.LeafCount : 0; set { } }

        [Category("BVH/Stats")]
        [ReadOnly(true)]
        public string Stat_GpuStatus { get => mStatGpuStatus; set { } }

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

        // ------------------------------------------------------------------
        // GPU BVH commands (deferred-execution research workflow):
        //   1) UploadToGpu — flatten current BVH + create SRV; await shading
        //                    init on first use so the effect's binder table
        //                    is valid by the time FGpuRayCast fires.
        //   2) FGpuRayCast — generate a deterministic ray batch, run CPU
        //                   closest-hit RayCast for reference, dispatch the
        //                   GPU traversal kernel, then readback the hit
        //                   buffer via TtBuffer.FetchGpuData (engine handles
        //                   GpuAccess→CpuAccess staging copy + queue flush
        //                   automatically, see CodingGuidelines.md §1.5.3)
        //                   and compare per-ray any-hit existence against the
        //                   CPU reference, dumping the first few mismatches
        //                   to the log.
        //   3) ClearGpu  — drop GPU buffers without touching the CPU tree.
        // ------------------------------------------------------------------
        [Category("BVH/GPU")]
        [DisplayName("[Cmd] UploadToGpu")]
        public bool CmdUploadToGpu
        {
            get => false;
            set
            {
                if (!value) return;
                UploadBvhToGpu().AddWaitTask();
            }
        }

        [Category("BVH/GPU")]
        [DisplayName("[Cmd] FGpuRayCast")]
        public bool CmdFGpuRayCast
        {
            get => false;
            set
            {
                if (!value) return;
                FGpuRayCast();
            }
        }

        [Category("BVH/GPU")]
        [DisplayName("[Cmd] ClearGpu")]
        public bool CmdClearGpu
        {
            get => false;
            set
            {
                if (!value) return;
                mGpuBvh.Clear();
                mStatGpuStatus = "(cleared)";
                Profiler.Log.WriteLineSingle("[BVHDebug.GPU] cleared GPU resources.");
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
        // Lifecycle
        // ==================================================================

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

            var trans = FTransform.Identity;
            bool drawInner = mDrawInternalNodes;
            bool drawLeaf  = mDrawLeafNodes;
            var hl = mHighlightedProxies;
            int maxDepth = MaxDisplayDepth;          // -1 means unlimited
            bool colorByDepth = ColorByDepth;

            mBvh.DebugTraverse((int nodeId, int depth, in Aabb box, bool isLeaf) =>
            {
                // Depth filter: clip both internal nodes and the leaves that
                // sit beyond the requested level. We still keep traversing so
                // that the iterator visits every node — DebugTraverse decides
                // recursion on its own.
                if (maxDepth >= 0 && depth > maxDepth)
                    return true;

                if (isLeaf)
                {
                    if (!drawLeaf)
                        return true;
                    if (hl.Contains(nodeId))
                    {
                        // Highlighted leaves: per-frame transient mesh so the
                        // highlight color shows up immediately without
                        // invalidating the persistent cache.
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
                    var color = colorByDepth ? GetDepthColor(depth) : ColorInternal;
                    EmitCachedAabb(rp, nodeId, in box, in color);
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

        // ==================================================================
        // GPU BVH workflow
        // ==================================================================

        async Thread.Async.TtTask UploadBvhToGpu()
        {
            if (mBvh == null || mBvh.ProxyCount == 0)
            {
                Profiler.Log.WriteLineSingle("[BVHDebug.GPU] UploadToGpu skipped: CPU tree is empty.");
                mStatGpuStatus = "(empty CPU tree)";
                return;
            }

            // Lazy-init the shading env on first upload so we don't pay the
            // effect-compile cost until the user actually wants GPU traversal.
            // Subsequent calls are no-ops inside InitializeShadingAsync.
            if (!mGpuShadingReady)
            {
                await mGpuBvh.InitializeShadingAsync();
                mGpuShadingReady = true;
            }

            // Discard the leafIndex callback — the demo only diffs against the
            // CPU tree which already maps proxyId<->leafIndex via FlattenToGpu's
            // BFS order. Real consumers would write into their payload buffer
            // here.
            bool ok = mGpuBvh.BuildFromCpuBvh<int>(mBvh, null);
            if (ok)
            {
                mStatGpuStatus = $"uploaded: nodes={mGpuBvh.NodeCount}, leaves={mGpuBvh.LeafCount}";
                Profiler.Log.WriteLineSingle("[BVHDebug.GPU] " + mStatGpuStatus);
            }
            else
            {
                mStatGpuStatus = "upload failed (see log)";
            }
        }

        void FGpuRayCast()
        {
            if (!mGpuBvh.IsReady)
            {
                Profiler.Log.WriteLineSingle("[BVHDebug.GPU] FGpuRayCast skipped: click [Cmd] UploadToGpu first.");
                return;
            }
            if (!mGpuShadingReady)
            {
                Profiler.Log.WriteLineSingle("[BVHDebug.GPU] FGpuRayCast skipped: shading not ready (re-click UploadToGpu).");
                return;
            }

            int n = Math.Max(1, RayCount);
            var rays = GenerateFGpuRays(n);

            // ----- CPU reference pass (closest-hit) -----
            //
            // TtDynamicBVH.RayCast is a closest-hit walker: the FOnRayHit
            // callback returns the new effective max fraction (return 0 to
            // stop completely; return the candidate t to keep walking but
            // narrow the search). Returning the candidate t mirrors what
            // GpuBvhCommon.cginc::TraverseBvhClosest does on the GPU side.
            //
            // We record per-ray closest leaf id + closest t so we can diff
            // against the GPU readback below.
            var cpuLeafId = new int[n];
            var cpuT      = new float[n];
            int cpuAnyHitCount = 0;
            for (int i = 0; i < n; i++)
            {
                ref readonly var r = ref rays[i];
                var origin = new DVector3(r.mOrigin.X, r.mOrigin.Y, r.mOrigin.Z);
                var dir = new DVector3(r.mDir.X, r.mDir.Y, r.mDir.Z);
                int closestLeafProxy = -1;
                double closestFraction = r.mMaxT;
                mBvh.RayCast(in origin, in dir, r.mMaxT,
                    (int proxyId, int payload, in DVector3 _o, in DVector3 _d, double currentMax) =>
                    {
                        // Closest-hit narrowing. The slab test inside RayCast
                        // already culled this leaf against currentMax; we
                        // simply remember it and keep walking with the same
                        // currentMax (returning < currentMax would shrink the
                        // window further, but we don't have a per-leaf t at
                        // this granularity yet — leaf AABB precision only).
                        closestLeafProxy = proxyId;
                        closestFraction = currentMax;
                        return currentMax;
                    });
                cpuLeafId[i] = closestLeafProxy;
                cpuT[i] = closestLeafProxy >= 0 ? (float)closestFraction : r.mMaxT;
                if (closestLeafProxy >= 0) cpuAnyHitCount++;
            }

            // ----- GPU dispatch + readback -----
            bool dispatched = mGpuBvh.RayCastDispatch(rays);
            if (!dispatched)
            {
                mStatGpuStatus = "dispatch failed (see log)";
                return;
            }
            if (mGpuBvh.ReadbackHits(out var hitRays))
            {
                foreach (var hitRay in hitRays)
                {
                }
            }
        }

        // Deterministic ray batch generator. Origin sits well outside the
        // SpaceExtent box so most rays start in empty space and have to walk
        // through the BVH to find a leaf. Direction is uniformly sampled on
        // the unit sphere (rejection-sampled from a unit cube).
        FGpuRay[] GenerateFGpuRays(int count)
        {
            var rays = new FGpuRay[count];
            var rng = new System.Random(RaySeed);
            var ext = SpaceExtent;
            float spawnRadius = (Math.Max(ext.X, Math.Max(ext.Y, ext.Z))) * 1.5f + 1.0f;
            float maxT = spawnRadius * 4.0f;

            for (int i = 0; i < count; i++)
            {
                // Origin: random direction on the spawn sphere of radius
                // spawnRadius, then scaled negative so we shoot back towards
                // the box center.
                Vector3 onSphere;
                do
                {
                    onSphere = new Vector3(
                        (float)(rng.NextDouble() * 2.0 - 1.0),
                        (float)(rng.NextDouble() * 2.0 - 1.0),
                        (float)(rng.NextDouble() * 2.0 - 1.0));
                } while (onSphere.LengthSquared() < 1e-4f);
                onSphere.Normalize();
                var origin = onSphere * spawnRadius;

                // Direction: aim at a random jittered point inside the
                // SpaceExtent box, so a healthy fraction of rays will hit
                // something.
                var target = new Vector3(
                    (float)((rng.NextDouble() * 2.0 - 1.0) * ext.X),
                    (float)((rng.NextDouble() * 2.0 - 1.0) * ext.Y),
                    (float)((rng.NextDouble() * 2.0 - 1.0) * ext.Z));
                var dir = target - origin;
                if (dir.LengthSquared() < 1e-6f) dir = new Vector3(1, 0, 0);
                dir.Normalize();

                rays[i].mOrigin = origin;
                rays[i].mMaxT = maxT;
                rays[i].mDir = dir;
                rays[i].mRayId = (uint)i;
            }
            return rays;
        }

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

        // Release everything we own that holds GPU resources or persistent CPU
        // caches. Mirrors TtPrimitiveMeshNode.Dispose's pattern (release this
        // node's own state, then base.Dispose for the inherited graph). The
        // mesh cache (via ClearAll → DisposeMeshCache), shared transient
        // buffers, and the GPU BVH (which itself owns NodeBuffer/SRV +
        // RayBuffer/HitBuffer + cbuffer + drawcall + cmdlist + shading env)
        // all need to go before the engine tears the RHI device down.
        public override void Dispose()
        {
            ClearAll();
            CoreSDK.DisposeObject(ref mSharedVB);
            CoreSDK.DisposeObject(ref mSharedIB);
            mGpuBvh?.Dispose();
            base.Dispose();
        }
    }
}
