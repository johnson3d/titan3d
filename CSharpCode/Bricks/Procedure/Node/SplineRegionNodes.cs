using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Spline Region", "Sampling\\Spline Region", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineRegionSamplerNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn ClipMaskPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn DensityCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut PointsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut DensityPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut SeedsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut RegionMaskPin { get; set; } = new PinOut();

        public TtBufferCreator FloatInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtBufferCreator MaskInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public TtBufferCreator PointsDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);
        public TtBufferCreator DensityDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, 1, 1);
        public TtBufferCreator SeedsDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, 1, 1);
        public TtBufferCreator RegionMaskDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, 1);

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        [Browsable(false)]
        public Guid SourceNodeId { get; set; }
        [Rtti.Meta("")]
        public float InteriorSampleSpacing { get; set; } = 1.0f;
        [Rtti.Meta("")]
        public float BorderSampleSpacing { get; set; } = 0.25f;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;
        [Rtti.Meta("")]
        public float Jitter { get; set; } = 0.0f;
        [Rtti.Meta("")]
        public bool ProjectOntoSplineSurface { get; set; } = false;
        [Rtti.Meta("")]
        public float BoundaryEpsilon { get; set; } = 1e-4f;
        [Rtti.Meta("")]
        [Browsable(false)]
        public List<BezierPointBase> DensityFalloff { get; set; } = new List<BezierPointBase>();

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplineRegionSnapshot mPreparedRegion;

        public TtSplineRegionSamplerNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", FloatInputDesc);
            AddInput(ClipMaskPin, "ClipMask", MaskInputDesc);
            AddInput(DensityCurvePin, "DensityCurve", null, "Bezier");
            AddOutput(PointsPin, "Points", PointsDesc);
            AddOutput(DensityPin, "Density", DensityDesc);
            AddOutput(SeedsPin, "Seeds", SeedsDesc);
            AddOutput(RegionMaskPin, "RegionMask", RegionMaskDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == PointsPin)
                return PointsDesc;
            if (pin == DensityPin)
                return DensityDesc;
            if (pin == SeedsPin)
                return SeedsDesc;
            if (pin == RegionMaskPin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var source = graph?.ResolveInput(ClipMaskPin) ??
                    graph?.ResolveInput(HeightPin);
                if (source != null)
                    RegionMaskDesc.SetSize(source.BufferCreator);
                else if (graph?.DefaultCreator != null)
                    RegionMaskDesc.SetSize(graph.DefaultCreator);
                RegionMaskDesc.ZSize = 1;
                return RegionMaskDesc;
            }
            return null;
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            mPreparedRegion = null;
            LastDiagnostic = null;
            if (graph.ExecutionContext == null)
            {
                LastDiagnostic = "Spline Region sampler requires a runtime PGC execution context";
                return true;
            }

            var query = new FSplineQuery(QueryScope, SplineName);
            if (!graph.ExecutionContext.TryGetSplineRegion(in query,
                Math.Max(1e-5f, BorderSampleSpacing), out mPreparedRegion, out var error))
            {
                LastDiagnostic = error;
                graph.ExecutionContext.AddDiagnostic($"Spline Region sampler '{Name}': {error}");
            }
            else if (graph.FindInLinkerSingle(HeightPin) == null &&
                !graph.ExecutionContext.TryGetExternalInput(NodeId, HeightPin.Name, out _) &&
                !ProjectOntoSplineSurface)
            {
                LastDiagnostic = "Height is not bound; samples use the spline region average Y";
                graph.ExecutionContext.AddDiagnostic($"Spline Region sampler '{Name}': {LastDiagnostic}");
            }
            return true;
        }

        public override bool OnProcedure(TtPgcGraph graph)
        {
            var points = graph.BufferCache.FindBuffer(PointsPin);
            var density = graph.BufferCache.FindBuffer(DensityPin);
            var seeds = graph.BufferCache.FindBuffer(SeedsPin);
            var regionMask = graph.BufferCache.FindBuffer(RegionMaskPin);
            points?.ResizePixels();
            density?.ResizePixels();
            seeds?.ResizePixels();
            ClearMask(regionMask);

            if (graph.ExecutionContext == null || mPreparedRegion?.IsValid != true)
            {
                ReportDiagnostic();
                ReleaseInputs(graph);
                return false;
            }

            BuildRegionMask(graph, regionMask);
            BuildPointSamples(graph, points, density, seeds, regionMask);
            ReleaseInputs(graph);
            return true;
        }

        void BuildRegionMask(TtPgcGraph graph, TtBufferComponent regionMask)
        {
            if (regionMask == null)
                return;
            var context = graph.ExecutionContext;
            var clipMask = graph.ResolveInput(ClipMaskPin) as TtSuperBuffer<sbyte, FSByteOperator>;
            float epsilon = Math.Max(0.0f, BoundaryEpsilon);
            for (int z = 0; z < regionMask.Height; z++)
            {
                for (int x = 0; x < regionMask.Width; x++)
                {
                    var worldXZ = context.GridToWorldXZ(x, z);
                    sbyte value = mPreparedRegion.Contains(in worldXZ, epsilon) &&
                        IsClipMaskSet(clipMask, x, z, regionMask.Width, regionMask.Height) ? (sbyte)1 : (sbyte)0;
                    regionMask.SetPixel<sbyte>(x, z, 0, in value);
                }
            }
        }

        unsafe void BuildPointSamples(TtPgcGraph graph, TtBufferComponent points,
            TtBufferComponent density, TtBufferComponent seeds, TtBufferComponent regionMask)
        {
            if (points == null || density == null || seeds == null || regionMask == null ||
                regionMask.Width <= 0 || regionMask.Height <= 0)
                return;

            var context = graph.ExecutionContext;
            var height = graph.ResolveInput(HeightPin) as TtSuperBuffer<float, FFloatOperator>;
            var clipMask = graph.ResolveInput(ClipMaskPin) as TtSuperBuffer<sbyte, FSByteOperator>;
            var densityCurve = GetInputNode(graph, DensityCurvePin) as TtBezier;
            var densityFalloff = densityCurve?.BzPoints;
            float spacing = Math.Max(1e-5f, InteriorSampleSpacing);
            float jitter = MathHelper.Clamp(Jitter, 0.0f, 1.0f);
            float epsilon = Math.Max(0.0f, BoundaryEpsilon);

            var tileMin = context.GridToWorldXZ(0, 0);
            var tileMax = context.GridToWorldXZ(regionMask.Width - 1, regionMask.Height - 1);
            var overlapMin = new Vector2(Math.Max(tileMin.X, mPreparedRegion.MinimumXZ.X),
                Math.Max(tileMin.Y, mPreparedRegion.MinimumXZ.Y));
            var overlapMax = new Vector2(Math.Min(tileMax.X, mPreparedRegion.MaximumXZ.X),
                Math.Min(tileMax.Y, mPreparedRegion.MaximumXZ.Y));
            if (overlapMin.X > overlapMax.X || overlapMin.Y > overlapMax.Y)
                return;

            long minCellX = (long)Math.Floor(overlapMin.X / spacing) - 1;
            long maxCellX = (long)Math.Ceiling(overlapMax.X / spacing) + 1;
            long minCellZ = (long)Math.Floor(overlapMin.Y / spacing) - 1;
            long maxCellZ = (long)Math.Ceiling(overlapMax.Y / spacing) + 1;
            float densityDistance = Math.Max(spacing,
                Math.Min(mPreparedRegion.MaximumXZ.X - mPreparedRegion.MinimumXZ.X,
                    mPreparedRegion.MaximumXZ.Y - mPreparedRegion.MinimumXZ.Y) * 0.5f);

            for (long cellZ = minCellZ; cellZ <= maxCellZ; cellZ++)
            {
                for (long cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                    float worldX = (float)(cellX * spacing);
                    float worldZ = (float)(cellZ * spacing);
                    if (jitter > 0.0f)
                    {
                        worldX += HashToSignedUnit(HashCoordinates(cellX, cellZ, Seed, 0x68bc21ebu)) * spacing * 0.5f * jitter;
                        worldZ += HashToSignedUnit(HashCoordinates(cellX, cellZ, Seed, 0x02e5be93u)) * spacing * 0.5f * jitter;
                    }
                    var worldXZ = new Vector2(worldX, worldZ);
                    if (worldX < tileMin.X - epsilon || worldX > tileMax.X + epsilon ||
                        worldZ < tileMin.Y - epsilon || worldZ > tileMax.Y + epsilon ||
                        !mPreparedRegion.Contains(in worldXZ, epsilon))
                        continue;

                    var grid = context.WorldToGrid(in worldXZ);
                    int gridX = (int)Math.Round(grid.X);
                    int gridZ = (int)Math.Round(grid.Y);
                    if (!IsClipMaskSet(clipMask, gridX, gridZ, regionMask.Width, regionMask.Height))
                        continue;

                    float worldY;
                    if (height != null && gridX >= 0 && gridZ >= 0 &&
                        gridX < height.Width && gridZ < height.Height)
                        worldY = height.GetPixel<float>(gridX, gridZ, 0);
                    else if (ProjectOntoSplineSurface)
                        worldY = mPreparedRegion.ProjectHeight(in worldXZ);
                    else
                        worldY = mPreparedRegion.AverageY;

                    float sampleDensity = 1.0f;
                    if (densityFalloff != null && densityFalloff.Count >= 2)
                    {
                        float normalizedDistance = MathHelper.Clamp(
                            mPreparedRegion.DistanceToBoundary(in worldXZ) / densityDistance, 0.0f, 1.0f);
                        sampleDensity = BezierCalculate.ValueOnBezier(densityFalloff,
                            normalizedDistance, true).Y;
                        if (!float.IsFinite(sampleDensity))
                            sampleDensity = 1.0f;
                    }

                    var point = new Vector3(worldX, worldY, worldZ);
                    int pointSeed = unchecked((int)HashQuantizedWorldPoint(in worldXZ, Seed));
                    points.AddPixel(in point);
                    density.AddPixel(in sampleDensity);
                    seeds.AddPixel(in pointSeed);
                }
            }
        }

        static void ClearMask(TtBufferComponent mask)
        {
            if (mask == null)
                return;
            sbyte zero = 0;
            for (int z = 0; z < mask.Height; z++)
            {
                for (int x = 0; x < mask.Width; x++)
                    mask.SetPixel<sbyte>(x, z, 0, in zero);
            }
        }

        static bool IsClipMaskSet(TtSuperBuffer<sbyte, FSByteOperator> clipMask,
            int x, int z, int referenceWidth, int referenceHeight)
        {
            if (clipMask == null)
                return true;
            if (x < 0 || z < 0 || referenceWidth <= 0 || referenceHeight <= 0)
                return false;
            int clipX = referenceWidth == clipMask.Width ? x :
                (int)Math.Round((double)x * Math.Max(0, clipMask.Width - 1) / Math.Max(1, referenceWidth - 1));
            int clipZ = referenceHeight == clipMask.Height ? z :
                (int)Math.Round((double)z * Math.Max(0, clipMask.Height - 1) / Math.Max(1, referenceHeight - 1));
            if (clipX < 0 || clipZ < 0 || clipX >= clipMask.Width || clipZ >= clipMask.Height)
                return false;
            return clipMask.GetPixel<sbyte>(clipX, clipZ, 0) != 0;
        }

        void ReleaseInputs(TtPgcGraph graph)
        {
            graph.ReleaseInput(HeightPin);
            graph.ReleaseInput(ClipMaskPin);
        }

        void ReportDiagnostic()
        {
            string message = string.IsNullOrEmpty(LastDiagnostic) ?
                "Spline Region sampler has no valid closed spline region" : LastDiagnostic;
            Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Warning,
                $"Spline Region sampler '{Name}': {message}");
        }

        static uint HashCoordinates(long x, long z, int seed, uint salt)
        {
            unchecked
            {
                uint hash = 2166136261u ^ (uint)seed ^ salt;
                hash = (hash ^ (uint)x) * 16777619u;
                hash = (hash ^ (uint)(x >> 32)) * 16777619u;
                hash = (hash ^ (uint)z) * 16777619u;
                hash = (hash ^ (uint)(z >> 32)) * 16777619u;
                hash ^= hash >> 16;
                hash *= 0x7feb352du;
                hash ^= hash >> 15;
                hash *= 0x846ca68bu;
                hash ^= hash >> 16;
                return hash;
            }
        }

        static uint HashQuantizedWorldPoint(in Vector2 point, int seed)
        {
            long x = (long)Math.Round(point.X * 1000.0f);
            long z = (long)Math.Round(point.Y * 1000.0f);
            return HashCoordinates(x, z, seed, 0x9e3779b9u);
        }

        static float HashToSignedUnit(uint hash)
        {
            return (hash & 0x00ffffffu) / 8388607.5f - 1.0f;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Spline Path", "Sampling\\Spline Path", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplinePathSamplerNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinOut PointsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut TangentsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut DistancesPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut AlphasPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut SeedsPin { get; set; } = new PinOut();

        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);
        public TtBufferCreator FloatDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, 1, 1);
        public TtBufferCreator IntDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, 1, 1);

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        [Browsable(false)]
        public Guid SourceNodeId { get; set; }
        [Rtti.Meta("")]
        public float Spacing { get; set; } = 1.0f;
        [Rtti.Meta("")]
        public float StartDistance { get; set; } = 0.0f;
        [Rtti.Meta("")]
        public float EndDistance { get; set; } = -1.0f;
        [Rtti.Meta("")]
        public bool IncludeEndPoint { get; set; } = true;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplinePathSamplerNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(PointsPin, "Points", Vector3Desc);
            AddOutput(TangentsPin, "Tangents", Vector3Desc);
            AddOutput(DistancesPin, "Distances", FloatDesc);
            AddOutput(AlphasPin, "Alphas", FloatDesc);
            AddOutput(SeedsPin, "Seeds", IntDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == PointsPin || pin == TangentsPin)
                return Vector3Desc;
            if (pin == DistancesPin || pin == AlphasPin)
                return FloatDesc;
            if (pin == SeedsPin)
                return IntDesc;
            return null;
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            mPreparedPath = null;
            LastDiagnostic = null;
            if (graph.ExecutionContext == null)
            {
                LastDiagnostic = "Spline Path sampler requires a runtime PGC execution context";
                return true;
            }
            var query = new FSplineQuery(QueryScope, SplineName);
            if (!graph.ExecutionContext.TryGetSplinePath(in query, out mPreparedPath, out var error))
            {
                LastDiagnostic = error;
                graph.ExecutionContext.AddDiagnostic($"Spline Path sampler '{Name}': {error}");
            }
            return true;
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            var points = graph.BufferCache.FindBuffer(PointsPin);
            var tangents = graph.BufferCache.FindBuffer(TangentsPin);
            var distances = graph.BufferCache.FindBuffer(DistancesPin);
            var alphas = graph.BufferCache.FindBuffer(AlphasPin);
            var seeds = graph.BufferCache.FindBuffer(SeedsPin);
            points?.ResizePixels();
            tangents?.ResizePixels();
            distances?.ResizePixels();
            alphas?.ResizePixels();
            seeds?.ResizePixels();

            if (mPreparedPath?.IsValid != true || points == null || tangents == null ||
                distances == null || alphas == null || seeds == null)
            {
                ReportDiagnostic();
                return false;
            }

            float spacing = Spacing;
            if (!float.IsFinite(spacing) || spacing <= 1e-5f)
            {
                LastDiagnostic = "Spacing must be a finite value greater than zero";
                ReportDiagnostic();
                return false;
            }

            float length = mPreparedPath.Length;
            float start = ResolveStartDistance(StartDistance, length, mPreparedPath.IsClosed);
            float end = ResolveEndDistance(EndDistance, start, length, mPreparedPath.IsClosed);
            if (end < start)
            {
                LastDiagnostic = "EndDistance precedes StartDistance on an open spline";
                ReportDiagnostic();
                return false;
            }

            const int maxSamples = 1000000;
            int sampleCount = 0;
            float endpointEpsilon = Math.Max(1e-5f, spacing * 1e-4f);
            for (float distance = start; distance < end - endpointEpsilon; distance += spacing)
            {
                if (!AddSample(distance, length, points, tangents, distances, alphas, seeds))
                    break;
                if (++sampleCount >= maxSamples)
                {
                    LastDiagnostic = $"Spline Path sampler exceeded the {maxSamples} sample safety limit";
                    ReportDiagnostic();
                    return false;
                }
            }
            if (IncludeEndPoint && (sampleCount == 0 || end - (start + (sampleCount - 1) * spacing) > endpointEpsilon))
                AddSample(end, length, points, tangents, distances, alphas, seeds);
            return true;
        }

        unsafe bool AddSample(float distance, float length, TtBufferComponent points,
            TtBufferComponent tangents, TtBufferComponent distances, TtBufferComponent alphas,
            TtBufferComponent seeds)
        {
            if (!mPreparedPath.TrySample(distance, out var point, out var tangent))
                return false;
            float alpha = length > 0.0f ? distance / length : 0.0f;
            int pointSeed = unchecked((int)HashDistance(distance, Seed));
            points.AddPixel(in point);
            tangents.AddPixel(in tangent);
            distances.AddPixel(in distance);
            alphas.AddPixel(in alpha);
            seeds.AddPixel(in pointSeed);
            return true;
        }

        static float ResolveStartDistance(float value, float length, bool isClosed)
        {
            if (!float.IsFinite(value))
                return 0.0f;
            if (!isClosed)
                return MathHelper.Clamp(value, 0.0f, length);
            value %= length;
            if (value < 0.0f)
                value += length;
            return value;
        }

        static float ResolveEndDistance(float value, float start, float length, bool isClosed)
        {
            if (!float.IsFinite(value) || value < 0.0f)
                return length;
            if (!isClosed)
                return MathHelper.Clamp(value, 0.0f, length);
            while (value < start)
                value += length;
            return Math.Min(value, start + length);
        }

        static uint HashDistance(float distance, int seed)
        {
            long value = (long)Math.Round(distance * 1000.0f);
            unchecked
            {
                uint hash = 2166136261u ^ (uint)seed ^ 0x9e3779b9u;
                hash = (hash ^ (uint)value) * 16777619u;
                hash = (hash ^ (uint)(value >> 32)) * 16777619u;
                hash ^= hash >> 16;
                hash *= 0x7feb352du;
                hash ^= hash >> 15;
                hash *= 0x846ca68bu;
                return hash ^ (hash >> 16);
            }
        }

        void ReportDiagnostic()
        {
            string message = string.IsNullOrEmpty(LastDiagnostic) ?
                "Spline Path sampler has no valid spline path" : LastDiagnostic;
            Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Warning,
                $"Spline Path sampler '{Name}': {message}");
        }
    }
}
