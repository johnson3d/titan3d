using System;
using System.Collections.Generic;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;

namespace EngineNS.Bricks.Procedure
{
    public enum ESplineQueryScope
    {
        VolumeChildren,
        VolumeDescendants,
        VolumeOverlapping,
    }

    public readonly struct FSplineQuery : IEquatable<FSplineQuery>
    {
        public readonly ESplineQueryScope Scope;
        public readonly string NodeName;

        public FSplineQuery(ESplineQueryScope scope, string nodeName)
        {
            Scope = scope;
            NodeName = string.IsNullOrWhiteSpace(nodeName) ? null : nodeName.Trim();
        }

        public bool Equals(FSplineQuery other)
        {
            return Scope == other.Scope && string.Equals(NodeName, other.NodeName,
                StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is FSplineQuery other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Scope, NodeName);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(NodeName) ? Scope.ToString() : $"{Scope}: {NodeName}";
        }
    }

    public sealed class TtPgcSpawnRequest
    {
        readonly FTransform[] mWorldTransforms;
        readonly int[] mSeeds;

        public Guid SourceNodeId { get; }
        public string SourceNodeName { get; }
        public Type NodeType { get; }
        public RName MeshName { get; }
        public string NamePrefix { get; }
        public IReadOnlyList<FTransform> WorldTransforms => mWorldTransforms;
        public IReadOnlyList<int> Seeds => mSeeds;

        public TtPgcSpawnRequest(Guid sourceNodeId, string sourceNodeName, Type nodeType,
            RName meshName, string namePrefix, IReadOnlyList<FTransform> worldTransforms,
            IReadOnlyList<int> seeds)
        {
            SourceNodeId = sourceNodeId;
            SourceNodeName = string.IsNullOrWhiteSpace(sourceNodeName) ? "Spawn Node" : sourceNodeName;
            NodeType = nodeType;
            MeshName = meshName;
            NamePrefix = string.IsNullOrEmpty(namePrefix) ? "PgcNode" : namePrefix;
            mWorldTransforms = Copy(worldTransforms);
            mSeeds = Copy(seeds);
            if (mSeeds.Length != mWorldTransforms.Length)
                throw new ArgumentException("Spawn request seeds must match transform count", nameof(seeds));
        }

        static T[] Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
                return Array.Empty<T>();
            var result = new T[source.Count];
            for (int i = 0; i < source.Count; i++)
                result[i] = source[i];
            return result;
        }
    }

    public sealed class TtSplinePathSnapshot
    {
        readonly TtBezier3DSpline mWorldSpline;

        public ulong SourceRevision { get; }
        public bool IsClosed => mWorldSpline.IsClosed;
        public float Length => mWorldSpline.Length;
        public bool IsValid => mWorldSpline.SegmentCount > 0 && Length > 1e-6f;

        TtSplinePathSnapshot(TtBezier3DSpline worldSpline, ulong sourceRevision)
        {
            mWorldSpline = worldSpline;
            SourceRevision = sourceRevision;
        }

        public static TtSplinePathSnapshot Create(TtBezier3DSplineSnapshot source, in FTransform localToWorld)
        {
            var worldSpline = new TtBezier3DSpline();
            if (source == null)
                return new TtSplinePathSnapshot(worldSpline, 0);

            worldSpline.Segments = source.Segments;
            for (int i = 0; i < source.Points.Count; i++)
            {
                var sourcePoint = source.Points[i];
                var localPosition = sourcePoint.Position.AsDVector();
                var localLeftControl = sourcePoint.LeftControl.AsDVector();
                var localRightControl = sourcePoint.RightControl.AsDVector();
                var position = localToWorld.TransformPosition(in localPosition).ToSingleVector3();
                var leftControl = localToWorld.TransformPosition(in localLeftControl).ToSingleVector3();
                var rightControl = localToWorld.TransformPosition(in localRightControl).ToSingleVector3();
                worldSpline.AppendPoint(in position, in leftControl, in rightControl);
            }
            worldSpline.IsClosed = source.IsClosed;
            return new TtSplinePathSnapshot(worldSpline, source.Revision);
        }

        public bool TrySample(float distance, out Vector3 position, out Vector3 tangent)
        {
            position = Vector3.Zero;
            tangent = Vector3.Zero;
            if (!IsValid || !mWorldSpline.GetValueAndForword(distance, out position, out tangent))
                return false;
            float tangentLength = tangent.Normalize();
            if (tangentLength > 1e-6f && float.IsFinite(tangent.X) &&
                float.IsFinite(tangent.Y) && float.IsFinite(tangent.Z))
                return true;

            float delta = Math.Max(1e-4f, Length * 1e-4f);
            var before = mWorldSpline.GetValue(distance - delta);
            var after = mWorldSpline.GetValue(distance + delta);
            tangent = after - before;
            tangentLength = tangent.Normalize();
            if (tangentLength <= 1e-6f || !float.IsFinite(tangent.X) ||
                !float.IsFinite(tangent.Y) || !float.IsFinite(tangent.Z))
                tangent = Vector3.Zero;
            return true;
        }
    }

    /// <summary>
    /// 一次 PGC 编译的瞬时空间上下文。场景对象只在准备阶段读取，节点执行期间只访问不可变快照。
    /// </summary>
    public sealed class TtPgcExecutionContext
    {
        readonly Dictionary<(Guid NodeId, int SpacingKey), TtSplineRegionSnapshot> mSplineRegions =
            new Dictionary<(Guid, int), TtSplineRegionSnapshot>();
        readonly Dictionary<Guid, TtSplinePathSnapshot> mSplinePaths =
            new Dictionary<Guid, TtSplinePathSnapshot>();
        readonly Dictionary<(Guid NodeId, string PinName), TtBufferComponent> mExternalInputs =
            new Dictionary<(Guid, string), TtBufferComponent>();
        readonly List<string> mDiagnostics = new List<string>();
        readonly List<TtPgcSpawnRequest> mSpawnRequests = new List<TtPgcSpawnRequest>();
        readonly List<TtBezierSplineNode> mSplineDependencies = new List<TtBezierSplineNode>();
        readonly HashSet<Guid> mSplineDependencyIds = new HashSet<Guid>();
        readonly List<FSplineQuery> mSplineQueries = new List<FSplineQuery>();
        readonly HashSet<FSplineQuery> mSplineQuerySet = new HashSet<FSplineQuery>();
        readonly Dictionary<FSplineQuery, TtBezierSplineNode> mResolvedSplines =
            new Dictionary<FSplineQuery, TtBezierSplineNode>();

        public TtWorld World { get; }
        public TtPgcVolumeNode HostVolume { get; }
        public DVector3 WorldOrigin { get; }
        public Vector2 GridCellSize { get; }
        public DBoundingBox? VolumeBounds { get; }
        public int Seed { get; set; }
        public ulong RunSignature { get; set; }
        public ulong ExternalInputVersion { get; private set; }
        public IReadOnlyList<string> Diagnostics => mDiagnostics;
        public IReadOnlyList<TtPgcSpawnRequest> SpawnRequests => mSpawnRequests;
        public IReadOnlyList<TtBezierSplineNode> SplineDependencies => mSplineDependencies;
        public IReadOnlyList<FSplineQuery> SplineQueries => mSplineQueries;
        public bool CanApplySpawnRequests { get; private set; } = true;

        public TtPgcExecutionContext(TtWorld world, in DVector3 worldOrigin, in Vector2 gridCellSize)
        {
            World = world;
            WorldOrigin = worldOrigin;
            GridCellSize = new Vector2(Math.Max(1e-6f, gridCellSize.X), Math.Max(1e-6f, gridCellSize.Y));
        }

        public TtPgcExecutionContext(TtWorld world, in DVector3 worldOrigin, in Vector2 gridCellSize,
            in DBoundingBox volumeBounds, TtPgcVolumeNode hostVolume = null)
            : this(world, in worldOrigin, in gridCellSize)
        {
            VolumeBounds = volumeBounds;
            HostVolume = hostVolume;
        }

        public TtPgcExecutionContext(TtWorld world, in DVector3 worldOrigin, float gridCellSize)
            : this(world, in worldOrigin, new Vector2(gridCellSize))
        {
        }

        public void BindExternalInput(Guid nodeId, string pinName, TtBufferComponent buffer, ulong version = 0)
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
            ExternalInputVersion = version != 0 ? version : ExternalInputVersion + 1;
        }

        public bool TryGetExternalInput(Guid nodeId, string pinName, out TtBufferComponent buffer)
        {
            return mExternalInputs.TryGetValue((nodeId, pinName), out buffer);
        }

        public void AddDiagnostic(string message)
        {
            if (!string.IsNullOrEmpty(message))
                mDiagnostics.Add(message);
        }

        public void AddSpawnRequest(TtPgcSpawnRequest request)
        {
            if (request != null)
                mSpawnRequests.Add(request);
        }

        public void InvalidateSpawnRequests(string message)
        {
            CanApplySpawnRequests = false;
            AddDiagnostic(message);
        }

        public Vector2 GridToWorldXZ(float x, float z)
        {
            return new Vector2((float)WorldOrigin.X + x * GridCellSize.X,
                (float)WorldOrigin.Z + z * GridCellSize.Y);
        }

        public Vector2 WorldToGrid(in Vector2 worldXZ)
        {
            return new Vector2((worldXZ.X - (float)WorldOrigin.X) / GridCellSize.X,
                (worldXZ.Y - (float)WorldOrigin.Z) / GridCellSize.Y);
        }

        public bool TryGetSplinePath(in FSplineQuery query, out TtSplinePathSnapshot path,
            out string error)
        {
            path = null;
            if (!TryResolveSpline(in query, out var node, out error))
                return false;

            var resolvedId = node.NodeId;
            if (mSplinePaths.TryGetValue(resolvedId, out path))
                return path.IsValid;

            path = TtSplinePathSnapshot.Create(node.Spline.CreateSnapshot(), in node.Placement.AbsTransform);
            mSplinePaths.Add(resolvedId, path);
            if (!path.IsValid)
            {
                error = "Spline has no non-degenerate path";
                return false;
            }
            return true;
        }

        public bool TryGetSplineRegion(in FSplineQuery query, float borderSampleSpacing,
            out TtSplineRegionSnapshot region, out string error)
        {
            region = null;
            if (!TryResolveSpline(in query, out var node, out error))
                return false;

            int spacingKey = (int)Math.Round(Math.Max(1e-5f, borderSampleSpacing) * 100000.0f);
            var key = (node.NodeId, spacingKey);
            if (mSplineRegions.TryGetValue(key, out region))
                return region.IsValid;

            var snapshot = node.Spline.CreateSnapshot();
            region = TtSplineRegionSnapshot.Create(snapshot, in node.Placement.AbsTransform, borderSampleSpacing);
            mSplineRegions.Add(key, region);
            if (!region.IsValid)
            {
                error = region.InvalidReason;
                return false;
            }
            return true;
        }

        bool TryResolveSpline(in FSplineQuery query, out TtBezierSplineNode spline, out string error)
        {
            spline = null;
            error = null;
            if (mSplineQuerySet.Add(query))
                mSplineQueries.Add(query);
            if (mResolvedSplines.TryGetValue(query, out spline))
                return true;

            if (HostVolume == null)
            {
                error = $"Spline query '{query}' requires a PGC Volume execution context";
                return false;
            }
            if (!TryFindSpline(World, HostVolume, VolumeBounds, in query, out spline, out int matchCount))
            {
                error = $"Spline query '{query}' did not match a Bezier spline";
                return false;
            }

            mResolvedSplines.Add(query, spline);
            if (matchCount > 1)
                AddDiagnostic($"Spline query '{query}' matched {matchCount} splines; using '{spline.NodeName}'");
            if (mSplineDependencyIds.Add(spline.NodeId))
                mSplineDependencies.Add(spline);
            return true;
        }

        public static bool TryFindSpline(TtWorld world, TtPgcVolumeNode hostVolume,
            DBoundingBox? volumeBounds, in FSplineQuery query, out TtBezierSplineNode spline,
            out int matchCount)
        {
            spline = null;
            matchCount = 0;
            if (hostVolume == null)
                return false;

            switch (query.Scope)
            {
                case ESplineQueryScope.VolumeChildren:
                    CollectDirectChildren(hostVolume, in query, ref spline, ref matchCount);
                    break;
                case ESplineQueryScope.VolumeDescendants:
                    CollectDescendants(hostVolume, in query, ref spline, ref matchCount);
                    break;
                case ESplineQueryScope.VolumeOverlapping:
                    var root = world?.Root;
                    if (root != null)
                    {
                        var bounds = volumeBounds ?? hostVolume.AbsAABB;
                        CollectOverlapping(root, in bounds, in query, ref spline, ref matchCount);
                    }
                    break;
            }
            return spline != null;
        }

        static void CollectDirectChildren(TtNode root, in FSplineQuery query,
            ref TtBezierSplineNode first, ref int matchCount)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                if (root.Children[i] is TtBezierSplineNode spline && Matches(spline, in query))
                {
                    first ??= spline;
                    matchCount++;
                }
            }
        }

        static void CollectDescendants(TtNode root, in FSplineQuery query,
            ref TtBezierSplineNode first, ref int matchCount)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                var child = root.Children[i];
                if (child is TtBezierSplineNode spline && Matches(spline, in query))
                {
                    first ??= spline;
                    matchCount++;
                }
                CollectDescendants(child, in query, ref first, ref matchCount);
            }
        }

        static void CollectOverlapping(TtNode node, in DBoundingBox bounds, in FSplineQuery query,
            ref TtBezierSplineNode first, ref int matchCount)
        {
            if (node is TtBezierSplineNode spline && Matches(spline, in query))
            {
                var splineBounds = spline.AbsAABB;
                if (DBoundingBox.Intersects(in bounds, in splineBounds))
                {
                    first ??= spline;
                    matchCount++;
                }
            }
            for (int i = 0; i < node.Children.Count; i++)
                CollectOverlapping(node.Children[i], in bounds, in query, ref first, ref matchCount);
        }

        static bool Matches(TtBezierSplineNode spline, in FSplineQuery query)
        {
            return spline?.Spline != null && (string.IsNullOrEmpty(query.NodeName) ||
                string.Equals(spline.NodeName, query.NodeName, StringComparison.Ordinal));
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcExecutionContext
    {
        public void UnitTestEntrance()
        {
            TestWorldSplineSnapshot();
            TestExternalInputOwnershipBoundary();
        }

        static void TestWorldSplineSnapshot()
        {
            var spline = new TtBezier3DSpline { Segments = 256 };
            spline.AppendPoint(Vector3.Zero, -Vector3.UnitX, Vector3.UnitX * (2.0f / 3.0f));
            spline.AppendPoint(Vector3.UnitX * 2.0f, Vector3.UnitX * (4.0f / 3.0f), Vector3.UnitX * 3.0f);
            var transform = FTransform.CreateTransform(new DVector3(10, 2, 3), new Vector3(2), Quaternion.Identity);
            var path = TtSplinePathSnapshot.Create(spline.CreateSnapshot(), in transform);

            UnitTest.TtUnitTestManager.TAssert(path.IsValid && Math.Abs(path.Length - 4.0f) <= 1e-3f,
                $"World spline snapshot length is incorrect: {path.Length}");
            UnitTest.TtUnitTestManager.TAssert(path.TrySample(2.0f, out var point, out var tangent) &&
                Vector3.Distance(point, new Vector3(12, 2, 3)) <= 1e-3f &&
                Vector3.Distance(tangent, Vector3.UnitX) <= 1e-3f,
                "World spline snapshot did not transform position and tangent correctly");
        }

        static void TestExternalInputOwnershipBoundary()
        {
            var context = new TtPgcExecutionContext(null, DVector3.Zero, Vector2.One);
            var nodeId = Guid.NewGuid();
            var creator = TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(2, 2, 1);
            var external = TtBufferComponent.CreateInstance(in creator);
            try
            {
                context.BindExternalInput(nodeId, "Height", external, 7);
                var cache = new TtPgcBufferCache();
                cache.ResetCache();
                UnitTest.TtUnitTestManager.TAssert(context.TryGetExternalInput(nodeId, "Height", out var resolved) &&
                    ReferenceEquals(external, resolved) && resolved.Width == 2 && context.ExternalInputVersion == 7,
                    "Resetting an internal PGC cache changed or released an external input");
            }
            finally
            {
                external.Dispose();
            }
        }
    }
}
