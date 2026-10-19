using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public struct FPgcPoint
    {
        public Vector3 Position;
        public Vector3 Tangent;
        public Vector3 Normal;
        public Vector3 Scale;
        public int Seed;
    }

    /// <summary>
    /// 点流处理节点基类: 输入/输出都是 Points/Tangents/Normals/Scales/Seeds 五路平行数组。
    /// 未连接的可选输入在输出端补默认值 (Forward / Up / One / 位置哈希), 保证下游拿到的五路元素数一致。
    /// 子类在 Execute 里修改点或标记剔除, 输出保持原有顺序。
    /// </summary>
    [Rtti.Meta("")]
    public partial class TtPgcPointStreamNodeBase : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn PointsInPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn TangentsInPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn NormalsInPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn ScalesInPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn SeedsInPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut PointsOutPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut TangentsOutPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut NormalsOutPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ScalesOutPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut SeedsOutPin { get; set; } = new PinOut();

        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);
        public TtBufferCreator IntDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, 1, 1);
        public TtBufferCreator FloatDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, 1, 1);

        const uint DefaultSeedSalt = 0x27d4eb2fu;

        [Browsable(false)]
        public string LastDiagnostic { get; protected set; }

        public TtPgcPointStreamNodeBase()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40A0FF;
            TitleColor = 0xFF203850;
            BackColor = 0x80808080;

            AddInput(PointsInPin, "Points", Vector3Desc);
            AddInput(TangentsInPin, "Tangents", Vector3Desc);
            AddInput(NormalsInPin, "Normals", Vector3Desc);
            AddInput(ScalesInPin, "Scales", Vector3Desc);
            AddInput(SeedsInPin, "Seeds", IntDesc);
            AddOutput(PointsOutPin, "Points", Vector3Desc);
            AddOutput(TangentsOutPin, "Tangents", Vector3Desc);
            AddOutput(NormalsOutPin, "Normals", Vector3Desc);
            AddOutput(ScalesOutPin, "Scales", Vector3Desc);
            AddOutput(SeedsOutPin, "Seeds", IntDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == PointsOutPin || pin == TangentsOutPin || pin == NormalsOutPin || pin == ScalesOutPin)
                return Vector3Desc;
            if (pin == SeedsOutPin)
                return IntDesc;
            return null;
        }

        protected string Label => $"{GetType().Name} '{Name}'";

        /// <summary>返回 false 表示执行失败 (输出为空)。</summary>
        protected virtual bool Execute(TtPgcGraph graph, FPgcPoint[] points, bool[] keep)
        {
            return true;
        }

        protected virtual void ReleaseExtraInputs(TtPgcGraph graph)
        {
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            LastDiagnostic = null;
            var pointsOut = graph.BufferCache.FindBuffer(PointsOutPin);
            var tangentsOut = graph.BufferCache.FindBuffer(TangentsOutPin);
            var normalsOut = graph.BufferCache.FindBuffer(NormalsOutPin);
            var scalesOut = graph.BufferCache.FindBuffer(ScalesOutPin);
            var seedsOut = graph.BufferCache.FindBuffer(SeedsOutPin);
            pointsOut?.ResizePixels();
            tangentsOut?.ResizePixels();
            normalsOut?.ResizePixels();
            scalesOut?.ResizePixels();
            seedsOut?.ResizePixels();

            var pointsIn = graph.ResolveInput(PointsInPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var tangentsIn = graph.ResolveInput(TangentsInPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var normalsIn = graph.ResolveInput(NormalsInPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var scalesIn = graph.ResolveInput(ScalesInPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var seedsIn = graph.ResolveInput(SeedsInPin) as TtSuperBuffer<int, FIntOperator>;
            try
            {
                if (pointsIn == null)
                {
                    LastDiagnostic = "Points input is required";
                    TtPgcSplineUtility.Warn(Label, LastDiagnostic);
                    return false;
                }
                int count = GetElementCount(pointsIn);
                if (!ValidateOptionalCount(tangentsIn, count, "Tangents") ||
                    !ValidateOptionalCount(normalsIn, count, "Normals") ||
                    !ValidateOptionalCount(scalesIn, count, "Scales") ||
                    !ValidateOptionalCount(seedsIn, count, "Seeds"))
                    return false;

                var points = new FPgcPoint[count];
                var keep = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    ref var point = ref points[i];
                    point.Position = GetLinearPixel<Vector3>(pointsIn, i);
                    point.Tangent = tangentsIn == null ? Vector3.Forward : GetLinearPixel<Vector3>(tangentsIn, GetOptionalIndex(tangentsIn, i));
                    point.Normal = normalsIn == null ? Vector3.Up : GetLinearPixel<Vector3>(normalsIn, GetOptionalIndex(normalsIn, i));
                    point.Scale = scalesIn == null ? Vector3.One : GetLinearPixel<Vector3>(scalesIn, GetOptionalIndex(scalesIn, i));
                    point.Seed = seedsIn == null ? unchecked((int)TtPgcHash.HashPoint(in point.Position, 0, DefaultSeedSalt)) :
                        GetLinearPixel<int>(seedsIn, GetOptionalIndex(seedsIn, i));
                    keep[i] = true;
                }

                if (!Execute(graph, points, keep))
                    return false;

                for (int i = 0; i < count; i++)
                {
                    if (!keep[i])
                        continue;
                    ref var point = ref points[i];
                    pointsOut?.AddPixel(in point.Position);
                    tangentsOut?.AddPixel(in point.Tangent);
                    normalsOut?.AddPixel(in point.Normal);
                    scalesOut?.AddPixel(in point.Scale);
                    seedsOut?.AddPixel(in point.Seed);
                }
                return true;
            }
            finally
            {
                graph.ReleaseInput(PointsInPin);
                graph.ReleaseInput(TangentsInPin);
                graph.ReleaseInput(NormalsInPin);
                graph.ReleaseInput(ScalesInPin);
                graph.ReleaseInput(SeedsInPin);
                ReleaseExtraInputs(graph);
            }
        }

        protected bool ValidateOptionalCount(TtBufferComponent buffer, int pointCount, string inputName)
        {
            if (buffer == null)
                return true;
            int count = GetElementCount(buffer);
            if (count == 1 || count == pointCount)
                return true;
            LastDiagnostic = $"{inputName} has {count} elements; expected 1 or {pointCount}";
            TtPgcSplineUtility.Warn(Label, LastDiagnostic);
            return false;
        }

        protected static int GetElementCount(TtBufferComponent buffer)
        {
            if (buffer == null)
                return 0;
            long count = (long)buffer.Width * buffer.Height * buffer.Depth;
            return count > int.MaxValue ? int.MaxValue : (int)count;
        }

        protected static int GetOptionalIndex(TtBufferComponent buffer, int pointIndex)
        {
            return GetElementCount(buffer) == 1 ? 0 : pointIndex;
        }

        protected static T GetLinearPixel<T>(TtBufferComponent buffer, int index) where T : unmanaged
        {
            int width = Math.Max(1, buffer.Width);
            int height = Math.Max(1, buffer.Height);
            int x = index % width;
            int y = (index / width) % height;
            int z = index / (width * height);
            return buffer.GetPixel<T>(x, y, z);
        }

        internal static float PointRandom(in FPgcPoint point, int seed, uint salt)
        {
            return TtPgcHash.ToUnit(TtPgcHash.Hash(point.Seed, 0, seed, salt));
        }

        internal static float PointSignedRandom(in FPgcPoint point, int seed, uint salt)
        {
            return TtPgcHash.ToSignedUnit(TtPgcHash.Hash(point.Seed, 0, seed, salt));
        }
    }

    /// <summary>
    /// 按密度随机剔除: 保留概率 = Density * DensityScale。Density 未连接时视为 1。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Density Filter", "Points\\Density Filter", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtPointDensityFilterNode : TtPgcPointStreamNodeBase
    {
        [Browsable(false)]
        public PinIn DensityPin { get; set; } = new PinIn();

        [Rtti.Meta("")]
        public float DensityScale { get; set; } = 1.0f;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;

        public TtPointDensityFilterNode()
        {
            AddInput(DensityPin, "Density", FloatDesc);
        }

        protected override bool Execute(TtPgcGraph graph, FPgcPoint[] points, bool[] keep)
        {
            var density = graph.ResolveInput(DensityPin) as TtSuperBuffer<float, FFloatOperator>;
            if (!ValidateOptionalCount(density, points.Length, "Density"))
                return false;
            for (int i = 0; i < points.Length; i++)
            {
                float value = density == null ? 1.0f : GetLinearPixel<float>(density, GetOptionalIndex(density, i));
                if (!float.IsFinite(value))
                    value = 0.0f;
                keep[i] = PointRandom(in points[i], Seed, 0x3c6ef372u) < value * DensityScale;
            }
            return true;
        }

        protected override void ReleaseExtraInputs(TtPgcGraph graph)
        {
            graph.ReleaseInput(DensityPin);
        }
    }

    /// <summary>
    /// 最小间距 (泊松盘式) 剔除: 以点种子哈希为优先级贪心保留, XZ 平面内任意两点距离不小于 MinDistance (米)。
    /// 结果与输入顺序无关, 重生成稳定。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Min Distance Filter", "Points\\Min Distance Filter", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtPointMinDistanceNode : TtPgcPointStreamNodeBase
    {
        /// <summary>最小间距 (米)</summary>
        [Rtti.Meta("")]
        public float MinDistance { get; set; } = 2.0f;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;

        protected override bool Execute(TtPgcGraph graph, FPgcPoint[] points, bool[] keep)
        {
            float minDistance = MinDistance;
            if (!float.IsFinite(minDistance) || minDistance <= 0.0f)
                return true;
            FilterMinDistance(points, keep, minDistance, Seed);
            return true;
        }

        internal static void FilterMinDistance(FPgcPoint[] points, bool[] keep, float minDistance, int seed)
        {
            int count = points.Length;
            var order = new int[count];
            var priority = new uint[count];
            for (int i = 0; i < count; i++)
            {
                order[i] = i;
                priority[i] = TtPgcHash.Hash(points[i].Seed, 0, seed, 0xa54ff53au);
                keep[i] = false;
            }
            Array.Sort(order, (a, b) =>
            {
                int result = priority[a].CompareTo(priority[b]);
                return result != 0 ? result : a.CompareTo(b);
            });

            float minDistanceSq = minDistance * minDistance;
            var cells = new Dictionary<long, List<int>>();
            foreach (int index in order)
            {
                var position = points[index].Position;
                long cellX = (long)Math.Floor(position.X / minDistance);
                long cellZ = (long)Math.Floor(position.Z / minDistance);
                bool blocked = false;
                for (long dz = -1; dz <= 1 && !blocked; dz++)
                {
                    for (long dx = -1; dx <= 1 && !blocked; dx++)
                    {
                        if (!cells.TryGetValue(CellKey(cellX + dx, cellZ + dz), out var list))
                            continue;
                        foreach (int other in list)
                        {
                            var otherPosition = points[other].Position;
                            float x = position.X - otherPosition.X;
                            float z = position.Z - otherPosition.Z;
                            if (x * x + z * z < minDistanceSq)
                            {
                                blocked = true;
                                break;
                            }
                        }
                    }
                }
                if (blocked)
                    continue;
                keep[index] = true;
                long key = CellKey(cellX, cellZ);
                if (!cells.TryGetValue(key, out var cell))
                {
                    cell = new List<int>();
                    cells.Add(key, cell);
                }
                cell.Add(index);
            }
        }

        static long CellKey(long x, long z)
        {
            return (x << 32) ^ (z & 0xffffffffL);
        }
    }

    /// <summary>
    /// 按到样条的水平距离剔除: 默认剔除距离中心线小于 ExcludeDistance (米, 受 WidthCurve 缩放) 的点,
    /// 用于让路边散布避开路面。Invert 时只保留该范围内的点。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline Distance Filter", "Points\\Spline Distance Filter", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineDistanceExcludeNode : TtPgcPointStreamNodeBase
    {
        [Browsable(false)]
        public PinIn WidthCurvePin { get; set; } = new PinIn();

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        /// <summary>到样条中心线的排除距离 (米)</summary>
        [Rtti.Meta("")]
        public float ExcludeDistance { get; set; } = 4.0f;
        [Rtti.Meta("")]
        public bool Invert { get; set; } = false;

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineDistanceExcludeNode()
        {
            AddInput(WidthCurvePin, "WidthCurve", null, "Bezier");
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, Label, out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        protected override bool Execute(TtPgcGraph graph, FPgcPoint[] points, bool[] keep)
        {
            if (graph.ExecutionContext == null || mPreparedPath?.IsValid != true)
            {
                TtPgcSplineUtility.Warn(Label, "No valid spline path");
                return false;
            }
            var widthCurve = GetInputNode(graph, WidthCurvePin) as TtBezier;
            float exclude = Math.Max(0.0f, ExcludeDistance);
            float maxExclude = exclude * Math.Max(0.0f, TtPgcCurveUtility.EvaluateMax(widthCurve, 1.0f));
            var polyline = TtSplinePolyline.Create(mPreparedPath, maxExclude);
            for (int i = 0; i < points.Length; i++)
            {
                var position = points[i].Position;
                var worldXZ = new Vector2(position.X, position.Z);
                bool inside = false;
                if (polyline.TryFindClosestXZ(in worldXZ, maxExclude, out var closest))
                {
                    float localExclude = exclude * Math.Max(0.0f, TtPgcCurveUtility.Evaluate(widthCurve, closest.Alpha, 1.0f));
                    inside = closest.HorizontalDistance < localExclude;
                }
                keep[i] = Invert ? inside : !inside;
            }
            return true;
        }
    }

    /// <summary>
    /// 逐点随机变换: 绕 (混合后的) 法线随机偏航、随机均匀缩放、XZ 随机位移。
    /// 输出 Tangents/Normals 直接供 Spawn Node 构造朝向。角度单位为度, 位移单位为米。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Transform Randomize", "Points\\Transform Randomize", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtPointTransformRandomizeNode : TtPgcPointStreamNodeBase
    {
        /// <summary>随机偏航范围 ±YawRangeDegrees (度)</summary>
        [Rtti.Meta("")]
        public float YawRangeDegrees { get; set; } = 180.0f;
        /// <summary>true 时以输入切线为基准朝向, 否则以 +Z 为基准</summary>
        [Rtti.Meta("")]
        public bool AlignToTangent { get; set; } = false;
        /// <summary>0 = 竖直向上, 1 = 完全贴合输入法线</summary>
        [Rtti.Meta("")]
        public float NormalAlign { get; set; } = 0.0f;
        [Rtti.Meta("")]
        public float ScaleMin { get; set; } = 0.8f;
        [Rtti.Meta("")]
        public float ScaleMax { get; set; } = 1.2f;
        /// <summary>XZ 随机位移幅度 (米)</summary>
        [Rtti.Meta("")]
        public float PositionJitter { get; set; } = 0.0f;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;

        protected override bool Execute(TtPgcGraph graph, FPgcPoint[] points, bool[] keep)
        {
            float normalAlign = MathHelper.Clamp(NormalAlign, 0.0f, 1.0f);
            float yawRange = float.IsFinite(YawRangeDegrees) ? YawRangeDegrees * (MathF.PI / 180.0f) : 0.0f;
            for (int i = 0; i < points.Length; i++)
            {
                ref var point = ref points[i];
                var up = Vector3.Lerp(Vector3.Up, point.Normal, normalAlign);
                if (up.Normalize() <= 1e-6f)
                    up = Vector3.Up;
                var forward = AlignToTangent ? point.Tangent : Vector3.Forward;
                forward -= up * Vector3.Dot(in forward, in up);
                if (forward.Normalize() <= 1e-6f)
                {
                    forward = Math.Abs(up.Z) < 0.999f ? Vector3.Forward : Vector3.Right;
                    forward -= up * Vector3.Dot(in forward, in up);
                    forward.Normalize();
                }

                float yaw = PointSignedRandom(in point, Seed, 0x510e527fu) * yawRange;
                if (yaw != 0.0f)
                {
                    var rotation = Quaternion.RotationAxis(in up, yaw);
                    forward = Quaternion.RotateVector3(in rotation, in forward);
                }

                float scale = ScaleMin + (ScaleMax - ScaleMin) * PointRandom(in point, Seed, 0x9b05688cu);
                if (!float.IsFinite(scale))
                    scale = 1.0f;

                if (PositionJitter > 0.0f)
                {
                    point.Position.X += PointSignedRandom(in point, Seed, 0x1f83d9abu) * PositionJitter;
                    point.Position.Z += PointSignedRandom(in point, Seed, 0x5be0cd19u) * PositionJitter;
                }
                point.Tangent = forward;
                point.Normal = up;
                point.Scale *= scale;
            }
            return true;
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcPointFilters
    {
        public void UnitTestEntrance()
        {
            const int sideCount = 20;
            const float spacing = 0.5f;
            const float minDistance = 2.0f;
            var points = new FPgcPoint[sideCount * sideCount];
            for (int z = 0; z < sideCount; z++)
            {
                for (int x = 0; x < sideCount; x++)
                {
                    ref var point = ref points[z * sideCount + x];
                    point.Position = new Vector3(x * spacing, 0, z * spacing);
                    point.Seed = z * sideCount + x;
                }
            }
            var keep = new bool[points.Length];
            TtPointMinDistanceNode.FilterMinDistance(points, keep, minDistance, 1);

            int kept = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (!keep[i])
                    continue;
                kept++;
                for (int j = i + 1; j < points.Length; j++)
                {
                    if (!keep[j])
                        continue;
                    var a = points[i].Position;
                    var b = points[j].Position;
                    float distanceSq = (a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z);
                    UnitTest.TtUnitTestManager.TAssert(distanceSq >= minDistance * minDistance - 1e-4f,
                        "Min distance filter kept two points that are too close");
                }
            }
            UnitTest.TtUnitTestManager.TAssert(kept > 1, "Min distance filter removed too many points");

            var reversed = new FPgcPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                reversed[i] = points[points.Length - 1 - i];
            var reversedKeep = new bool[points.Length];
            TtPointMinDistanceNode.FilterMinDistance(reversed, reversedKeep, minDistance, 1);
            bool orderIndependent = true;
            for (int i = 0; i < points.Length; i++)
                orderIndependent &= keep[i] == reversedKeep[points.Length - 1 - i];
            UnitTest.TtUnitTestManager.TAssert(orderIndependent, "Min distance filter depends on input order");
        }
    }
}
