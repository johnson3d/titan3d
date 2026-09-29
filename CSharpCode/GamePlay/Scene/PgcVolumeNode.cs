using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    [TtNode(NodeDataType = typeof(TtPgcSpawnNodeHub.TtPgcSpawnNodeHubData), DefaultNamePrefix = "PgcSpawn")]
    [Rtti.Meta("")]
    public partial class TtPgcSpawnNodeHub : TtHubNode
    {
        [Rtti.Meta("")]
        public class TtPgcSpawnNodeHubData : TtNodeData
        {
            [Rtti.Meta("")]
            [Browsable(false)]
            public Guid SourceNodeId { get; set; }
        }

        [Browsable(false)]
        public TtPgcSpawnNodeHubData PgcSpawnHubData => GetNodeData<TtPgcSpawnNodeHubData>();

        [Browsable(false)]
        public Guid SourceNodeId => PgcSpawnHubData?.SourceNodeId ?? Guid.Empty;
    }

    [Bricks.CodeBuilder.ContextMenu("PGC Volume", "Procedure\\PGC Volume", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPgcVolumeNode.TtPgcVolumeNodeData), DefaultNamePrefix = "PgcVolume")]
    [Rtti.Meta("")]
    public partial class TtPgcVolumeNode : TtVolumeBaseNode
    {
        [Rtti.Meta("")]
        public class TtPgcVolumeNodeData : TtVolumeBaseData
        {
            [Rtti.Meta("")]
            [Category("PGC")]
            [RName.PGRName(FilterExts = Bricks.Procedure.TtPgcAsset.AssetExt)]
            public RName PgcAssetName { get; set; }

            [Rtti.Meta("")]
            [Category("PGC")]
            public Vector2 GridCellSize { get; set; } = Vector2.One;

            [Rtti.Meta("")]
            [Category("PGC")]
            public bool AutoGenerate { get; set; } = true;

            [Rtti.Meta("")]
            [Category("PGC")]
            public int Seed { get; set; } = 12345;
        }

        readonly Dictionary<(Guid NodeId, string PinName), Bricks.Procedure.TtBufferComponent> mExternalInputs =
            new Dictionary<(Guid, string), Bricks.Procedure.TtBufferComponent>();
        readonly List<TtNode> mGeneratedNodes = new List<TtNode>();
        readonly List<TtNode> mGeneratedRoots = new List<TtNode>();
        readonly List<TtBezierSplineNode> mSplineDependencies = new List<TtBezierSplineNode>();
        readonly List<Bricks.Procedure.FSplineQuery> mSplineQueries =
            new List<Bricks.Procedure.FSplineQuery>();
        Bricks.Procedure.TtPgcAsset mRuntimeAsset;
        RName mLoadedAssetName;
        bool mGenerationDirty = true;
        bool mGenerating;
        ulong mExternalInputVersion;
        ulong mLastRunSignature;
        string mLastDiagnostic;

        [Browsable(false)]
        public TtPgcVolumeNodeData PgcVolumeData => GetNodeData<TtPgcVolumeNodeData>();

        [Category("PGC")]
        [RName.PGRName(FilterExts = Bricks.Procedure.TtPgcAsset.AssetExt)]
        public RName PgcAssetName
        {
            get => PgcVolumeData?.PgcAssetName;
            set
            {
                if (PgcVolumeData == null || PgcVolumeData.PgcAssetName == value)
                    return;
                PgcVolumeData.PgcAssetName = value;
                ReleaseRuntimeGraph();
                Invalidate();
            }
        }

        [Category("PGC")]
        public Vector2 GridCellSize
        {
            get => PgcVolumeData?.GridCellSize ?? Vector2.One;
            set
            {
                if (PgcVolumeData == null)
                    return;
                var clamped = new Vector2(Math.Max(1e-4f, value.X), Math.Max(1e-4f, value.Y));
                if (PgcVolumeData.GridCellSize == clamped)
                    return;
                PgcVolumeData.GridCellSize = clamped;
                Invalidate();
            }
        }

        [Category("PGC")]
        public bool AutoGenerate
        {
            get => PgcVolumeData?.AutoGenerate ?? false;
            set
            {
                if (PgcVolumeData == null || PgcVolumeData.AutoGenerate == value)
                    return;
                PgcVolumeData.AutoGenerate = value;
                if (value)
                    Invalidate();
            }
        }

        [Category("PGC")]
        public int Seed
        {
            get => PgcVolumeData?.Seed ?? 0;
            set
            {
                if (PgcVolumeData == null || PgcVolumeData.Seed == value)
                    return;
                PgcVolumeData.Seed = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public string LastDiagnostic => mLastDiagnostic;

        [Browsable(false)]
        public Bricks.Procedure.TtPgcGraph RuntimeGraph => mRuntimeAsset?.AssetGraph;

        [Browsable(false)]
        public ulong LastRunSignature => mLastRunSignature;

        [Browsable(false)]
        public int GeneratedNodeCount => mGeneratedNodes.Count;

        public bool IsGeneratedNode(TtNode node)
        {
            for (var current = node; current != null && !ReferenceEquals(current, this);
                current = current.Parent)
            {
                if (current.HasStyle(ENodeStyles.PgcGenerated))
                    return true;
            }
            return false;
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(
            TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            data ??= new TtPgcVolumeNodeData();
            var result = await base.InitializeNode(world, data, bvType, placementType);
            Invalidate();
            return result;
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            RebuildGeneratedNodes();
        }

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            base.AddAssetReferences(ameta);
            if (PgcAssetName != null)
                ameta.AddReferenceAsset(PgcAssetName);
        }

        public void BindExternalInput(Guid nodeId, string pinName,
            Bricks.Procedure.TtBufferComponent buffer)
        {
            if (nodeId == Guid.Empty)
                throw new ArgumentException("A PGC node id is required", nameof(nodeId));
            if (string.IsNullOrEmpty(pinName))
                throw new ArgumentException("A PGC pin name is required", nameof(pinName));
            var key = (nodeId, pinName);
            if (buffer == null)
                mExternalInputs.Remove(key);
            else
                mExternalInputs[key] = buffer;
            mExternalInputVersion++;
            Invalidate();
        }

        public void ClearExternalInputs()
        {
            if (mExternalInputs.Count == 0)
                return;
            mExternalInputs.Clear();
            mExternalInputVersion++;
            Invalidate();
        }

        public void Invalidate()
        {
            mGenerationDirty = true;
        }

        public bool Generate()
        {
            if (mGenerating)
                return false;
            mGenerating = true;
            try
            {
                mLastDiagnostic = null;
                RebuildGeneratedNodes();
                if (!EnsureRuntimeGraph())
                    return false;

                ref var bounds = ref BoundVolume.AbsAABB;
                double sizeX = Math.Max(0.0, bounds.Maximum.X - bounds.Minimum.X);
                double sizeZ = Math.Max(0.0, bounds.Maximum.Z - bounds.Minimum.Z);
                var cellSize = GridCellSize;
                int width = Math.Max(1, (int)Math.Ceiling(sizeX / cellSize.X) + 1);
                int height = Math.Max(1, (int)Math.Ceiling(sizeZ / cellSize.Y) + 1);
                var graph = mRuntimeAsset.AssetGraph;
                graph.DefaultCreator = Bricks.Procedure.TtBufferCreator.CreateInstance<
                    Bricks.Procedure.TtSuperBuffer<float, Bricks.Procedure.FFloatOperator>>(width, height, 1);

                var origin = new DVector3(bounds.Minimum.X, bounds.Minimum.Y, bounds.Minimum.Z);
                var context = new Bricks.Procedure.TtPgcExecutionContext(GetWorld(), in origin,
                    in cellSize, in bounds, this)
                {
                    Seed = Seed,
                };
                foreach (var input in mExternalInputs)
                    context.BindExternalInput(input.Key.NodeId, input.Key.PinName, input.Value, mExternalInputVersion);

                mLastRunSignature = ComputeRunSignature(in bounds, graph.Version);
                context.RunSignature = mLastRunSignature;
                graph.Compile(graph.Root, context);
                mSplineDependencies.Clear();
                for (int i = 0; i < context.SplineDependencies.Count; i++)
                    mSplineDependencies.Add(context.SplineDependencies[i]);
                mSplineQueries.Clear();
                for (int i = 0; i < context.SplineQueries.Count; i++)
                    mSplineQueries.Add(context.SplineQueries[i]);
                mLastRunSignature = ComputeRunSignature(in bounds, graph.Version);
                context.RunSignature = mLastRunSignature;
                if (context.Diagnostics.Count > 0)
                    mLastDiagnostic = string.Join("; ", context.Diagnostics);
                if (!ApplySpawnRequests(context))
                    return false;
                mGenerationDirty = false;
                return true;
            }
            catch (Exception exception)
            {
                mLastDiagnostic = exception.Message;
                Profiler.Log.WriteException(exception);
                return false;
            }
            finally
            {
                mGenerationDirty = false;
                mGenerating = false;
            }
        }

        bool ApplySpawnRequests(Bricks.Procedure.TtPgcExecutionContext context)
        {
            if (!context.CanApplySpawnRequests)
                return false;

            var nextGeneratedHubs = new List<TtPgcSpawnNodeHub>();
            var nextGeneratedNodes = new List<TtNode>();
            var hubsBySource = new Dictionary<Guid, TtPgcSpawnNodeHub>();
            try
            {
                for (int requestIndex = 0; requestIndex < context.SpawnRequests.Count; requestIndex++)
                {
                    var request = context.SpawnRequests[requestIndex];
                    if (request.SourceNodeId == Guid.Empty)
                        throw new InvalidOperationException("PGC spawn request contains an empty source node id");
                    if (request.NodeType == null || request.NodeType.IsAbstract ||
                        !request.NodeType.IsSubclassOf(typeof(TtNode)))
                        throw new InvalidOperationException("PGC spawn request contains an invalid node type");

                    if (!hubsBySource.TryGetValue(request.SourceNodeId, out var hub))
                    {
                        var hubData = new TtPgcSpawnNodeHub.TtPgcSpawnNodeHubData
                        {
                            Name = request.SourceNodeName,
                            SourceNodeId = request.SourceNodeId,
                        };
                        var hubTask = TtNode.SpawnNode<TtPgcSpawnNodeHub>(this, null, hubData,
                            EBoundVolumeType.Box, typeof(TtPlacement), context.World);
                        hub = hubTask.GetResultUntilCompleted();
                        if (hub == null)
                            throw new InvalidOperationException($"Failed to create generated hub for '{request.SourceNodeName}'");
                        hub.SetStyle(ENodeStyles.PgcGenerated);
                        hubsBySource.Add(request.SourceNodeId, hub);
                        nextGeneratedHubs.Add(hub);
                    }

                    for (int itemIndex = 0; itemIndex < request.WorldTransforms.Count; itemIndex++)
                    {
                        var nodeData = TtNode.CreateNodeData(request.NodeType);
                        if (nodeData == null)
                            throw new InvalidOperationException($"Cannot create NodeData for {request.NodeType.FullName}");
                        nodeData.Name = $"{request.NamePrefix}_{request.Seeds[itemIndex]}_{itemIndex}";
                        if (nodeData is TtMeshNode.TtMeshNodeData meshData)
                            meshData.MeshName = request.MeshName;

                        var localTransform = WorldToGeneratedLocal(request.WorldTransforms[itemIndex]);
                        var spawnTask = TtNode.SpawnNode(hub, request.NodeType, async (node) =>
                        {
                            node.Placement.TransformData = localTransform;
                        }, nodeData, EBoundVolumeType.Box, typeof(TtPlacement), context.World);
                        var spawnedNode = spawnTask.GetResultUntilCompleted();
                        if (spawnedNode == null)
                            throw new InvalidOperationException($"Failed to spawn {request.NodeType.FullName}");
                        spawnedNode.SetStyle(ENodeStyles.PgcGenerated);
                        nextGeneratedNodes.Add(spawnedNode);
                        if (spawnedNode is TtMeshNode meshNode && meshNode.RenderMesh == null)
                            throw new InvalidOperationException($"Failed to load mesh for {request.NodeType.FullName}");
                    }
                }
            }
            catch (Exception exception)
            {
                DisposeGeneratedNodes(nextGeneratedHubs);
                mLastDiagnostic = string.IsNullOrEmpty(mLastDiagnostic) ? exception.Message :
                    $"{mLastDiagnostic}; {exception.Message}";
                Profiler.Log.WriteException(exception);
                return false;
            }

            DisposeGeneratedNodes(mGeneratedRoots);
            mGeneratedRoots.Clear();
            mGeneratedRoots.AddRange(nextGeneratedHubs);
            mGeneratedNodes.Clear();
            mGeneratedNodes.AddRange(nextGeneratedNodes);
            UpdateAABB();
            return true;
        }

        FTransform WorldToGeneratedLocal(in FTransform worldTransform)
        {
            ref var parentTransform = ref Placement.AbsTransform;
            if (Placement.InheritScale)
            {
                var inverseParent = parentTransform.Inverse();
                FTransform.Multiply(out var localTransform, in worldTransform, in inverseParent);
                return localTransform;
            }

            var inverseRotation = parentTransform.Quat.Inverse();
            var localPosition = inverseRotation * (worldTransform.Position - parentTransform.Position);
            var localRotation = worldTransform.Quat * inverseRotation;
            localRotation.Normalize();
            var localScale = worldTransform.Scale;
            return FTransform.CreateTransform(in localPosition, in localScale, in localRotation);
        }

        void RebuildGeneratedNodes()
        {
            mGeneratedNodes.Clear();
            mGeneratedRoots.Clear();
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                if (child is TtPgcSpawnNodeHub hub)
                {
                    hub.SetStyle(ENodeStyles.PgcGenerated);
                    mGeneratedRoots.Add(hub);
                    for (int childIndex = 0; childIndex < hub.Children.Count; childIndex++)
                    {
                        var generatedNode = hub.Children[childIndex];
                        if (generatedNode != null)
                        {
                            generatedNode.SetStyle(ENodeStyles.PgcGenerated);
                            mGeneratedNodes.Add(generatedNode);
                        }
                    }
                }
                else if (child?.HasStyle(ENodeStyles.PgcGenerated) == true)
                {
                    mGeneratedRoots.Add(child);
                    mGeneratedNodes.Add(child);
                }
            }
        }

        static void DisposeGeneratedNodes<T>(IReadOnlyList<T> nodes) where T : TtNode
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                if (node == null)
                    continue;
                node.Parent = null;
                node.DisposeWithChildren();
            }
        }

        bool EnsureRuntimeGraph()
        {
            var assetName = PgcAssetName;
            if (assetName == null)
            {
                mLastDiagnostic = "PGC Volume has no PGC asset";
                return false;
            }
            if (mRuntimeAsset != null && mLoadedAssetName == assetName)
            {
                mRuntimeAsset.AssetGraph.HostWorld = GetWorld();
                mRuntimeAsset.AssetGraph.HostVolume = this;
                return true;
            }

            ReleaseRuntimeGraph();
            mRuntimeAsset = Bricks.Procedure.TtPgcAsset.LoadAsset(assetName);
            if (mRuntimeAsset != null)
            {
                mRuntimeAsset.AssetGraph.HostWorld = GetWorld();
                mRuntimeAsset.AssetGraph.HostVolume = this;
            }
            mLoadedAssetName = assetName;
            if (mRuntimeAsset?.AssetGraph?.Root == null)
            {
                mLastDiagnostic = $"Cannot load PGC asset: {assetName}";
                ReleaseRuntimeGraph();
                return false;
            }
            return true;
        }

        ulong ComputeRunSignature(in DBoundingBox bounds, uint graphVersion)
        {
            unchecked
            {
                ulong hash = 1469598103934665603ul;
                AddHash(ref hash, PgcAssetName?.GetHashCode() ?? 0);
                AddHash(ref hash, unchecked((int)graphVersion));
                AddHash(ref hash, Seed);
                AddHash(ref hash, GridCellSize.GetHashCode());
                AddHash(ref hash, bounds.Minimum.GetHashCode());
                AddHash(ref hash, bounds.Maximum.GetHashCode());
                AddHash(ref hash, Placement.AbsTransform.GetHashCode());
                AddHash(ref hash, mExternalInputVersion.GetHashCode());

                for (int i = 0; i < mSplineDependencies.Count; i++)
                    AddSplineHash(ref hash, mSplineDependencies[i]);
                return hash;
            }
        }

        static void AddSplineHash(ref ulong hash, TtBezierSplineNode spline)
        {
            if (spline?.Spline == null)
                return;
            AddHash(ref hash, spline.NodeId.GetHashCode());
            AddHash(ref hash, spline.Parent?.NodeId.GetHashCode() ?? 0);
            AddHash(ref hash, spline.Spline.Revision.GetHashCode());
            AddHash(ref hash, spline.Placement.AbsTransform.GetHashCode());
        }

        internal static void NotifySplineQuerySourceChanged(TtBezierSplineNode spline,
            TtWorld previousWorld = null)
        {
            InvalidateSplineQueryVolumes(previousWorld?.Root);
            var currentWorld = spline?.GetWorld();
            if (currentWorld != null && !ReferenceEquals(currentWorld, previousWorld))
                InvalidateSplineQueryVolumes(currentWorld.Root);
        }

        static void InvalidateSplineQueryVolumes(TtNode node)
        {
            if (node == null)
                return;
            if (node is TtPgcVolumeNode volume && volume.mSplineQueries.Count > 0)
                volume.Invalidate();
            for (int i = 0; i < node.Children.Count; i++)
                InvalidateSplineQueryVolumes(node.Children[i]);
        }

        static void AddHash(ref ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211ul;
            }
        }

        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (AutoGenerate && !mGenerationDirty && mRuntimeAsset?.AssetGraph != null)
            {
                ref var bounds = ref BoundVolume.AbsAABB;
                if (ComputeRunSignature(in bounds, mRuntimeAsset.AssetGraph.Version) != mLastRunSignature)
                    mGenerationDirty = true;
            }
            if (AutoGenerate && mGenerationDirty)
                Generate();
            return base.OnTickLogic(args);
        }

        protected override void OnAbsAABBChanged()
        {
            base.OnAbsAABBChanged();
            Invalidate();
        }

        protected override void OnRemoveFromWorld()
        {
            RebuildGeneratedNodes();
            DisposeGeneratedNodes(mGeneratedRoots);
            mGeneratedRoots.Clear();
            mGeneratedNodes.Clear();
            ReleaseRuntimeGraph();
            base.OnRemoveFromWorld();
        }

        void ReleaseRuntimeGraph()
        {
            if (mRuntimeAsset?.AssetGraph != null)
            {
                mRuntimeAsset.AssetGraph.BufferCache?.ResetCache();
                if (ReferenceEquals(mRuntimeAsset.AssetGraph.HostVolume, this))
                {
                    mRuntimeAsset.AssetGraph.HostVolume = null;
                    mRuntimeAsset.AssetGraph.HostWorld = null;
                }
            }
            mRuntimeAsset = null;
            mLoadedAssetName = null;
            mSplineDependencies.Clear();
            mSplineQueries.Clear();
            mLastRunSignature = 0;
        }

        public override void Dispose()
        {
            RebuildGeneratedNodes();
            DisposeGeneratedNodes(mGeneratedRoots);
            mGeneratedRoots.Clear();
            mGeneratedNodes.Clear();
            ReleaseRuntimeGraph();
            mExternalInputs.Clear();
            base.Dispose();
        }
    }
}
