using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    /// <summary>
    /// PGC 节点共用的确定性哈希。同一输入在任意机器、任意执行顺序下结果一致, 保证重生成稳定。
    /// </summary>
    internal static class TtPgcHash
    {
        public static uint Hash(long x, long z, int seed, uint salt)
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

        public static uint HashPoint(in Vector3 point, int seed, uint salt)
        {
            // 按毫米量化, 避免浮点尾数抖动导致同一个点得到不同哈希
            long x = (long)Math.Round(point.X * 1000.0f);
            long y = (long)Math.Round(point.Y * 1000.0f);
            long z = (long)Math.Round(point.Z * 1000.0f);
            return Hash(x ^ (y << 21), z, seed, salt);
        }

        /// <summary>[0, 1]</summary>
        public static float ToUnit(uint hash)
        {
            return (hash & 0x00ffffffu) / 16777215.0f;
        }

        /// <summary>[-1, 1]</summary>
        public static float ToSignedUnit(uint hash)
        {
            return (hash & 0x00ffffffu) / 8388607.5f - 1.0f;
        }
    }

    internal static class TtPgcCurveUtility
    {
        /// <summary>
        /// 以 [0,1] 的 alpha 在曲线 X 区间上取值。曲线未连接、点数不足或结果非有限值时返回 fallback。
        /// </summary>
        public static float Evaluate(TtBezier curve, float alpha, float fallback)
        {
            var points = curve?.BzPoints;
            if (points == null || points.Count < 2)
                return fallback;
            float x = curve.MinX + MathHelper.Clamp(alpha, 0.0f, 1.0f) * (curve.MaxX - curve.MinX);
            float value = BezierCalculate.ValueOnBezier(points, x, true).Y;
            return float.IsFinite(value) ? value : fallback;
        }

        public static float EvaluateMax(TtBezier curve, float fallback)
        {
            const int sampleCount = 64;
            if (curve?.BzPoints == null || curve.BzPoints.Count < 2)
                return fallback;
            float result = float.MinValue;
            for (int i = 0; i <= sampleCount; i++)
                result = Math.Max(result, Evaluate(curve, i / (float)sampleCount, fallback));
            return result;
        }

        public static float SmoothStep01(float t)
        {
            t = MathHelper.Clamp(t, 0.0f, 1.0f);
            return t * t * (3.0f - 2.0f * t);
        }
    }

    internal static class TtPgcGridUtility
    {
        /// <summary>
        /// 在 PGC 网格高度图上按世界 XZ 双线性取高度。高度图的值即世界 Y。
        /// </summary>
        public static bool TrySampleHeight(TtPgcExecutionContext context,
            TtSuperBuffer<float, FFloatOperator> height, in Vector2 worldXZ, out float value)
        {
            value = 0.0f;
            if (context == null || height == null || height.Width <= 0 || height.Height <= 0)
                return false;
            var grid = context.WorldToGrid(in worldXZ);
            float gx = MathHelper.Clamp(grid.X, 0.0f, height.Width - 1);
            float gz = MathHelper.Clamp(grid.Y, 0.0f, height.Height - 1);
            int x0 = (int)Math.Floor(gx);
            int z0 = (int)Math.Floor(gz);
            int x1 = Math.Min(x0 + 1, height.Width - 1);
            int z1 = Math.Min(z0 + 1, height.Height - 1);
            float fx = gx - x0;
            float fz = gz - z0;
            float h00 = height.GetPixel<float>(x0, z0, 0);
            float h10 = height.GetPixel<float>(x1, z0, 0);
            float h01 = height.GetPixel<float>(x0, z1, 0);
            float h11 = height.GetPixel<float>(x1, z1, 0);
            value = (h00 * (1 - fx) + h10 * fx) * (1 - fz) + (h01 * (1 - fx) + h11 * fx) * fz;
            return float.IsFinite(value);
        }

        public static TtBufferCreator ResolveGridCreator(TtPgcGraph graph, TtBufferCreator target,
            TtBufferComponent source)
        {
            if (source != null)
                target.SetSize(source.BufferCreator);
            else if (graph?.DefaultCreator != null)
                target.SetSize(graph.DefaultCreator);
            target.ZSize = 1;
            return target;
        }
    }

    internal static class TtPgcSplineUtility
    {
        public static bool TryPreparePath(TtPgcGraph graph, ESplineQueryScope scope, string splineName,
            string label, out TtSplinePathSnapshot path, out string diagnostic)
        {
            path = null;
            diagnostic = null;
            if (graph.ExecutionContext == null)
            {
                diagnostic = $"{label} requires a runtime PGC execution context";
                return false;
            }
            var query = new FSplineQuery(scope, splineName);
            if (!graph.ExecutionContext.TryGetSplinePath(in query, out path, out var error))
            {
                diagnostic = error;
                graph.ExecutionContext.AddDiagnostic($"{label}: {error}");
                path = null;
                return false;
            }
            return true;
        }

        public static void Warn(string label, string message)
        {
            Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Warning, $"{label}: {message}");
        }

        /// <summary>
        /// 水平右方向: Cross(Up, Forward) 在左手 Y-up 系下等于 (t.z, 0, -t.x)。切线竖直时返回 false。
        /// </summary>
        public static bool TryGetHorizontalFrame(in Vector3 tangent, out Vector3 forward, out Vector3 right)
        {
            forward = new Vector3(tangent.X, 0.0f, tangent.Z);
            float length = forward.Normalize();
            if (length <= 1e-5f || !float.IsFinite(forward.X) || !float.IsFinite(forward.Z))
            {
                forward = Vector3.Forward;
                right = Vector3.Right;
                return false;
            }
            right = new Vector3(forward.Z, 0.0f, -forward.X);
            return true;
        }

        public static float ResolveDistance(float distance, float length, bool isClosed)
        {
            if (!isClosed)
                return MathHelper.Clamp(distance, 0.0f, length);
            if (length <= 0.0f)
                return 0.0f;
            distance %= length;
            if (distance < 0.0f)
                distance += length;
            return distance;
        }
    }

    public struct FSplineClosestPoint
    {
        /// <summary>XZ 平面上到样条的距离 (米)</summary>
        public float HorizontalDistance;
        /// <summary>最近点沿样条的弧长 (米)</summary>
        public float SplineDistance;
        public float Alpha;
        public Vector3 Position;
        public Vector3 Tangent;
        /// <summary>带符号横向距离, 沿样条前进方向右侧为正 (米)</summary>
        public float SignedLateral;
    }

    /// <summary>
    /// 样条的世界空间折线近似 + XZ 稀疏桶索引, 用于网格逐像素的最近点查询。
    /// </summary>
    public sealed class TtSplinePolyline
    {
        /// <summary>折线采样步长 (米)</summary>
        public const float DefaultStepMeters = 0.25f;
        public const int MaxSegments = 200000;
        /// <summary>桶的最小边长 (米)</summary>
        const float MinBucketSizeMeters = 1.0f;

        readonly List<Vector3> mPoints = new List<Vector3>();
        readonly List<Vector3> mTangents = new List<Vector3>();
        readonly List<float> mDistances = new List<float>();
        readonly Dictionary<long, List<int>> mBuckets = new Dictionary<long, List<int>>();
        float mBucketSize;

        public IReadOnlyList<Vector3> Points => mPoints;
        public IReadOnlyList<Vector3> Tangents => mTangents;
        public IReadOnlyList<float> Distances => mDistances;
        public float Length { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsValid => mPoints.Count >= 2 && Length > 1e-6f;

        public static TtSplinePolyline Create(TtSplinePathSnapshot path, float queryRadius,
            float stepMeters = DefaultStepMeters)
        {
            var result = new TtSplinePolyline();
            if (path?.IsValid != true)
                return result;
            result.Length = path.Length;
            result.IsClosed = path.IsClosed;
            float step = Math.Max(1e-3f, stepMeters);
            int segments = (int)Math.Min(MaxSegments, Math.Max(1, Math.Ceiling(path.Length / step)));
            for (int i = 0; i <= segments; i++)
            {
                float distance = path.Length * i / segments;
                if (!path.TrySample(Math.Min(distance, path.Length), out var position, out var tangent))
                    continue;
                result.mPoints.Add(position);
                result.mTangents.Add(tangent);
                result.mDistances.Add(distance);
            }
            result.BuildIndex(queryRadius);
            return result;
        }

        void BuildIndex(float queryRadius)
        {
            mBuckets.Clear();
            mBucketSize = Math.Max(MinBucketSizeMeters, float.IsFinite(queryRadius) ? queryRadius : MinBucketSizeMeters);
            for (int i = 0; i + 1 < mPoints.Count; i++)
            {
                var a = mPoints[i];
                var b = mPoints[i + 1];
                long minX = CellOf(Math.Min(a.X, b.X));
                long maxX = CellOf(Math.Max(a.X, b.X));
                long minZ = CellOf(Math.Min(a.Z, b.Z));
                long maxZ = CellOf(Math.Max(a.Z, b.Z));
                for (long cz = minZ; cz <= maxZ; cz++)
                {
                    for (long cx = minX; cx <= maxX; cx++)
                    {
                        long key = BucketKey(cx, cz);
                        if (!mBuckets.TryGetValue(key, out var list))
                        {
                            list = new List<int>();
                            mBuckets.Add(key, list);
                        }
                        list.Add(i);
                    }
                }
            }
        }

        long CellOf(float value)
        {
            return (long)Math.Floor(value / mBucketSize);
        }

        static long BucketKey(long x, long z)
        {
            return (x << 32) ^ (z & 0xffffffffL);
        }

        public bool TryFindClosestXZ(in Vector2 worldXZ, float maxDistance, out FSplineClosestPoint result)
        {
            result = default;
            if (!IsValid || !(maxDistance >= 0.0f))
                return false;
            long minX = CellOf(worldXZ.X - maxDistance);
            long maxX = CellOf(worldXZ.X + maxDistance);
            long minZ = CellOf(worldXZ.Y - maxDistance);
            long maxZ = CellOf(worldXZ.Y + maxDistance);
            float bestSq = maxDistance * maxDistance;
            int bestSegment = -1;
            float bestT = 0.0f;
            for (long cz = minZ; cz <= maxZ; cz++)
            {
                for (long cx = minX; cx <= maxX; cx++)
                {
                    if (!mBuckets.TryGetValue(BucketKey(cx, cz), out var list))
                        continue;
                    for (int k = 0; k < list.Count; k++)
                    {
                        int segment = list[k];
                        float distanceSq = DistanceToSegmentSq(in worldXZ, segment, out float t);
                        if (distanceSq <= bestSq)
                        {
                            bestSq = distanceSq;
                            bestSegment = segment;
                            bestT = t;
                        }
                    }
                }
            }
            if (bestSegment < 0)
                return false;

            var a = mPoints[bestSegment];
            var b = mPoints[bestSegment + 1];
            var position = a + (b - a) * bestT;
            var tangent = mTangents[bestSegment] * (1.0f - bestT) + mTangents[bestSegment + 1] * bestT;
            if (tangent.Normalize() <= 1e-6f)
            {
                tangent = b - a;
                tangent.Normalize();
            }
            float splineDistance = mDistances[bestSegment] +
                (mDistances[bestSegment + 1] - mDistances[bestSegment]) * bestT;
            TtPgcSplineUtility.TryGetHorizontalFrame(in tangent, out _, out var right);
            float lateral = (worldXZ.X - position.X) * right.X + (worldXZ.Y - position.Z) * right.Z;

            result.HorizontalDistance = (float)Math.Sqrt(bestSq);
            result.SplineDistance = splineDistance;
            result.Alpha = Length > 0.0f ? MathHelper.Clamp(splineDistance / Length, 0.0f, 1.0f) : 0.0f;
            result.Position = position;
            result.Tangent = tangent;
            result.SignedLateral = lateral;
            return true;
        }

        float DistanceToSegmentSq(in Vector2 p, int segment, out float t)
        {
            var a = mPoints[segment];
            var b = mPoints[segment + 1];
            float abX = b.X - a.X;
            float abZ = b.Z - a.Z;
            float apX = p.X - a.X;
            float apZ = p.Y - a.Z;
            float lengthSq = abX * abX + abZ * abZ;
            t = lengthSq > 1e-12f ? MathHelper.Clamp((apX * abX + apZ * abZ) / lengthSq, 0.0f, 1.0f) : 0.0f;
            float dx = apX - abX * t;
            float dz = apZ - abZ * t;
            return dx * dx + dz * dz;
        }
    }

    /// <summary>
    /// CPU 端网格构建缓冲。不使用 native BuildTangent (其实现把位置当作 UV 读取, 结果错误), 切线由调用方给出。
    /// 切线 W 与 native/Shader 约定一致: W=0 时 Shader 取 B=cross(T,N), W=1 时 B=cross(N,T), B 应为 dP/dv。
    /// </summary>
    public sealed class TtPgcMeshBuffer
    {
        public readonly List<Vector3> Positions = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<Vector4> Tangents = new List<Vector4>();
        public readonly List<Vector2> UVs = new List<Vector2>();
        public readonly List<uint> Colors = new List<uint>();
        /// <summary>每个 atom 一组三角形索引 (每 3 个一个三角形)</summary>
        public readonly List<List<uint>> AtomIndices = new List<List<uint>>();

        public int VertexCount => Positions.Count;

        public uint AddVertex(in Vector3 position, in Vector3 normal, in Vector4 tangent, in Vector2 uv, uint color = 0xffffffff)
        {
            Positions.Add(position);
            Normals.Add(normal);
            Tangents.Add(tangent);
            UVs.Add(uv);
            Colors.Add(color);
            return (uint)(Positions.Count - 1);
        }

        public List<uint> GetAtom(int atom)
        {
            while (AtomIndices.Count <= atom)
                AtomIndices.Add(new List<uint>());
            return AtomIndices[atom];
        }

        public static float TangentHandedness(in Vector3 normal, in Vector3 tangent, in Vector3 dPdv)
        {
            var shaderBitangent = Vector3.Cross(in tangent, in normal);
            return Vector3.Dot(in shaderBitangent, in dPdv) >= 0.0f ? 0.0f : 1.0f;
        }

        /// <summary>按面积加权累加面法线, 覆盖 Normals。退化顶点用 fallback。</summary>
        public void ComputeSmoothNormals(in Vector3 fallback)
        {
            var accumulated = new Vector3[Positions.Count];
            foreach (var indices in AtomIndices)
            {
                for (int i = 0; i + 2 < indices.Count; i += 3)
                {
                    int a = (int)indices[i];
                    int b = (int)indices[i + 1];
                    int c = (int)indices[i + 2];
                    var ab = Positions[b] - Positions[a];
                    var ac = Positions[c] - Positions[a];
                    var face = Vector3.Cross(in ab, in ac);
                    accumulated[a] += face;
                    accumulated[b] += face;
                    accumulated[c] += face;
                }
            }
            for (int i = 0; i < accumulated.Length; i++)
            {
                var normal = accumulated[i];
                Normals[i] = normal.Normalize() > 1e-8f ? normal : fallback;
            }
        }

        public void Translate(in Vector3 offset)
        {
            for (int i = 0; i < Positions.Count; i++)
                Positions[i] = Positions[i] + offset;
        }

        public unsafe Graphics.Mesh.TtMeshDataProvider Build(string transientName)
        {
            int atomCount = 0;
            foreach (var indices in AtomIndices)
            {
                if (indices.Count >= 3)
                    atomCount++;
            }
            if (Positions.Count == 0 || atomCount == 0)
                return null;

            var provider = new Graphics.Mesh.TtMeshDataProvider();
            provider.AssetName = RName.GetRName(transientName, RName.ERNameType.Transient);
            var builder = provider.mCoreObject;
            uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Tangent) |
                (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                (1 << (int)NxRHI.EVertexStreamType.VST_UV));
            bool isIndex32 = Positions.Count > ushort.MaxValue;
            builder.Init(streams, isIndex32, atomCount);

            for (int i = 0; i < Positions.Count; i++)
            {
                var tangent = new Vector3(Tangents[i].X, Tangents[i].Y, Tangents[i].Z);
                builder.AddVertex(Positions[i], Normals[i], in tangent, UVs[i], Colors[i]);
            }

            uint atomIndex = 0;
            uint triangleBefore = 0;
            foreach (var indices in AtomIndices)
            {
                int triangleCount = indices.Count / 3;
                if (triangleCount == 0)
                    continue;
                for (int t = 0; t < triangleCount; t++)
                    builder.AddTriangle(indices[t * 3], indices[t * 3 + 1], indices[t * 3 + 2]);
                var desc = new NxRHI.FMeshAtomDesc();
                desc.SetDefault();
                desc.NumPrimitives = (uint)triangleCount;
                desc.StartIndex = triangleBefore * 3;
                provider.PushAtomLOD(atomIndex, in desc);
                triangleBefore += (uint)triangleCount;
                atomIndex++;
            }
            builder.CalcAABB();

            var tangentBlob = builder.GetStream(NxRHI.EVertexStreamType.VST_Tangent);
            if (tangentBlob.IsValidPointer)
            {
                var pTangent = (Vector4*)tangentBlob.GetData();
                for (int i = 0; i < Tangents.Count; i++)
                    pTangent[i].W = Tangents[i].W;
            }
            return provider;
        }
    }

    /// <summary>
    /// PGC 节点在执行期同步加载资产。GetResultUntilCompleted 会在等待期间泵主/逻辑线程事件, 不会死锁。
    /// </summary>
    internal static class TtPgcAssetUtility
    {
        /// <summary>等待 .ums 子网格异步加载完成的上限 (毫秒)</summary>
        const long SubMeshLoadTimeoutMilliseconds = 10000;

        public static Graphics.Pipeline.Shader.TtMaterial LoadMaterial(RName name)
        {
            try
            {
                if (name != null)
                {
                    Graphics.Pipeline.Shader.TtMaterial material = null;
                    if (name.ExtName == Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)
                        material = name.GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>().GetResultUntilCompleted();
                    else if (name.ExtName == Graphics.Pipeline.Shader.TtMaterial.AssetExt)
                        material = name.GetAsset<Graphics.Pipeline.Shader.TtMaterial>().GetResultUntilCompleted();
                    if (material != null)
                        return material;
                }
                return TtEngine.Instance.Config.DefaultMaterial?.GetAsset<Graphics.Pipeline.Shader.TtMaterial>()
                    .GetResultUntilCompleted();
            }
            catch (Exception exception)
            {
                Profiler.Log.WriteException(exception);
                return null;
            }
        }

        public static Graphics.Mesh.TtMaterialMesh LoadMaterialMeshWithData(RName name)
        {
            if (name == null)
                return null;
            try
            {
                return LoadMaterialMeshWithDataAsync(name).GetResultUntilCompleted();
            }
            catch (Exception exception)
            {
                Profiler.Log.WriteException(exception);
                return null;
            }
        }

        static async Thread.Async.TtTask<Graphics.Mesh.TtMaterialMesh> LoadMaterialMeshWithDataAsync(RName name)
        {
            var materialMesh = await name.GetAsset<Graphics.Mesh.TtMaterialMesh>();
            if (materialMesh == null)
                return null;

            // .ums 的子网格在 SaveData setter 里是 fire-and-forget 异步加载, 刚拿到时可能还是 null
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < SubMeshLoadTimeoutMilliseconds)
            {
                bool loading = false;
                foreach (var subMesh in materialMesh.SubMeshes)
                {
                    if (subMesh.Mesh == null && subMesh.AssetState == IO.EAssetState.Loading)
                        loading = true;
                }
                if (!loading)
                    break;
                await TtEngine.Instance.EventPoster.Post((Thread.Async.FPostEvent<bool>)((state) => true),
                    Thread.Async.EAsyncTarget.AsyncIO);
            }

            foreach (var subMesh in materialMesh.SubMeshes)
            {
                if (subMesh.Mesh != null)
                    await subMesh.Mesh.LoadMeshDataProvider();
            }
            return materialMesh;
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcSplinePolyline
    {
        public void UnitTestEntrance()
        {
            var spline = new TtBezier3DSpline { Segments = 64 };
            spline.AppendPoint(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitZ * (10.0f / 3.0f));
            spline.AppendPoint(Vector3.UnitZ * 10.0f, Vector3.UnitZ * (20.0f / 3.0f), Vector3.UnitZ * 11.0f);
            var path = TtSplinePathSnapshot.Create(spline.CreateSnapshot(), FTransform.Identity);
            var polyline = TtSplinePolyline.Create(path, 5.0f);
            UnitTest.TtUnitTestManager.TAssert(polyline.IsValid, "Spline polyline is invalid");

            // 沿 +Z 前进, +X 为右侧
            UnitTest.TtUnitTestManager.TAssert(polyline.TryFindClosestXZ(new Vector2(2.0f, 5.0f), 5.0f, out var right) &&
                Math.Abs(right.HorizontalDistance - 2.0f) <= 1e-3f && right.SignedLateral > 0.0f &&
                Math.Abs(right.Alpha - 0.5f) <= 1e-2f,
                "Spline polyline closest query on the right side is incorrect");
            UnitTest.TtUnitTestManager.TAssert(polyline.TryFindClosestXZ(new Vector2(-3.0f, 5.0f), 5.0f, out var left) &&
                left.SignedLateral < 0.0f,
                "Spline polyline lateral sign is incorrect");
            UnitTest.TtUnitTestManager.TAssert(!polyline.TryFindClosestXZ(new Vector2(20.0f, 5.0f), 5.0f, out _),
                "Spline polyline returned a point beyond the search radius");
        }
    }
}
