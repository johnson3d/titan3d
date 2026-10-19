using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public enum ESplineSide
    {
        Left,
        Right,
        Both,
    }

    /// <summary>
    /// 沿样条按间距在道路两侧 (路面外 EdgeOffset 处) 生成点, 用于路边种树、路灯、护栏等。
    /// 所有距离单位为米。WidthCurve 的 X 为样条 alpha [0,1], Y 为 HalfWidth 的缩放系数。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline Lateral Offset", "Sampling\\Spline Lateral Offset", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineLateralOffsetNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn WidthCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut PointsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut TangentsPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut AlphasPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut SeedsPin { get; set; } = new PinOut();

        public TtBufferCreator FloatInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);
        public TtBufferCreator FloatDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, 1, 1);
        public TtBufferCreator IntDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, 1, 1);

        const int MaxSamplesPerSide = 1000000;

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        [Rtti.Meta("")]
        public ESplineSide Side { get; set; } = ESplineSide.Both;
        /// <summary>沿样条间距 (米)</summary>
        [Rtti.Meta("")]
        public float Spacing { get; set; } = 4.0f;
        /// <summary>起始弧长 (米)</summary>
        [Rtti.Meta("")]
        public float StartDistance { get; set; } = 0.0f;
        /// <summary>结束弧长 (米), 负数表示样条终点</summary>
        [Rtti.Meta("")]
        public float EndDistance { get; set; } = -1.0f;
        /// <summary>道路半宽 (米), 受 WidthCurve 缩放</summary>
        [Rtti.Meta("")]
        public float HalfWidth { get; set; } = 3.0f;
        /// <summary>点到路面边缘的距离 (米)</summary>
        [Rtti.Meta("")]
        public float EdgeOffset { get; set; } = 1.5f;
        /// <summary>横向随机抖动幅度 (米)</summary>
        [Rtti.Meta("")]
        public float LateralJitter { get; set; } = 0.0f;
        /// <summary>纵向随机抖动幅度 (米)</summary>
        [Rtti.Meta("")]
        public float LongitudinalJitter { get; set; } = 0.0f;
        /// <summary>两侧交错半个间距</summary>
        [Rtti.Meta("")]
        public bool Stagger { get; set; } = false;
        [Rtti.Meta("")]
        public int Seed { get; set; } = 12345;
        /// <summary>点高度偏移 (米)</summary>
        [Rtti.Meta("")]
        public float HeightOffset { get; set; } = 0.0f;

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineLateralOffsetNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", FloatInputDesc);
            AddInput(WidthCurvePin, "WidthCurve", null, "Bezier");
            AddOutput(PointsPin, "Points", Vector3Desc);
            AddOutput(TangentsPin, "Tangents", Vector3Desc);
            AddOutput(AlphasPin, "Alphas", FloatDesc);
            AddOutput(SeedsPin, "Seeds", IntDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == PointsPin || pin == TangentsPin)
                return Vector3Desc;
            if (pin == AlphasPin)
                return FloatDesc;
            if (pin == SeedsPin)
                return IntDesc;
            return null;
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, $"Spline Lateral Offset '{Name}'",
                out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            var points = graph.BufferCache.FindBuffer(PointsPin);
            var tangents = graph.BufferCache.FindBuffer(TangentsPin);
            var alphas = graph.BufferCache.FindBuffer(AlphasPin);
            var seeds = graph.BufferCache.FindBuffer(SeedsPin);
            points?.ResizePixels();
            tangents?.ResizePixels();
            alphas?.ResizePixels();
            seeds?.ResizePixels();
            var height = graph.ResolveInput(HeightPin) as TtSuperBuffer<float, FFloatOperator>;
            string label = $"Spline Lateral Offset '{Name}'";
            try
            {
                if (points == null || tangents == null || alphas == null || seeds == null)
                    return false;
                if (graph.ExecutionContext == null || mPreparedPath?.IsValid != true)
                {
                    TtPgcSplineUtility.Warn(label, string.IsNullOrEmpty(LastDiagnostic) ? "No valid spline path" : LastDiagnostic);
                    return false;
                }
                float spacing = Spacing;
                if (!float.IsFinite(spacing) || spacing <= 1e-3f)
                {
                    TtPgcSplineUtility.Warn(label, "Spacing must be a finite value greater than 0.001");
                    return false;
                }

                var context = graph.ExecutionContext;
                var widthCurve = GetInputNode(graph, WidthCurvePin) as TtBezier;
                float length = mPreparedPath.Length;
                bool isClosed = mPreparedPath.IsClosed;
                float start = float.IsFinite(StartDistance) ? MathHelper.Clamp(StartDistance, 0.0f, length) : 0.0f;
                float end = !float.IsFinite(EndDistance) || EndDistance < 0.0f ? length : MathHelper.Clamp(EndDistance, start, length);
                // 闭合样条终点与起点重合, 不重复取样
                bool includeEnd = !(isClosed && end >= length - 1e-4f && start <= 1e-4f);

                for (int sideIndex = 0; sideIndex < 2; sideIndex++)
                {
                    bool isRight = sideIndex == 1;
                    if ((Side == ESplineSide.Left && isRight) || (Side == ESplineSide.Right && !isRight))
                        continue;
                    float sign = isRight ? 1.0f : -1.0f;
                    float phase = Stagger && !isRight ? spacing * 0.5f : 0.0f;
                    for (int k = 0; k < MaxSamplesPerSide; k++)
                    {
                        float baseDistance = start + phase + k * spacing;
                        if (baseDistance > end + 1e-4f || (!includeEnd && baseDistance >= end - 1e-4f))
                            break;
                        uint hashLongitudinal = TtPgcHash.Hash(k, sideIndex, Seed, 0x1b873593u);
                        uint hashLateral = TtPgcHash.Hash(k, sideIndex, Seed, 0xcc9e2d51u);
                        uint hashSeed = TtPgcHash.Hash(k, sideIndex, Seed, 0x85ebca6bu);
                        float distance = baseDistance + LongitudinalJitter * TtPgcHash.ToSignedUnit(hashLongitudinal);
                        distance = isClosed ? TtPgcSplineUtility.ResolveDistance(distance, length, true) :
                            MathHelper.Clamp(distance, start, end);
                        if (!mPreparedPath.TrySample(distance, out var center, out var tangent))
                            continue;
                        TtPgcSplineUtility.TryGetHorizontalFrame(in tangent, out var forward, out var right);

                        float alpha = length > 0.0f ? distance / length : 0.0f;
                        float halfWidth = Math.Max(0.0f, HalfWidth) *
                            Math.Max(0.0f, TtPgcCurveUtility.Evaluate(widthCurve, alpha, 1.0f));
                        float lateral = halfWidth + EdgeOffset + LateralJitter * TtPgcHash.ToSignedUnit(hashLateral);
                        var point = center + right * (sign * lateral);
                        var worldXZ = new Vector2(point.X, point.Z);
                        if (TtPgcGridUtility.TrySampleHeight(context, height, in worldXZ, out float groundY))
                            point.Y = groundY;
                        point.Y += HeightOffset;

                        int pointSeed = unchecked((int)hashSeed);
                        points.AddPixel(in point);
                        tangents.AddPixel(in forward);
                        alphas.AddPixel(in alpha);
                        seeds.AddPixel(in pointSeed);
                    }
                }
                return true;
            }
            finally
            {
                graph.ReleaseInput(HeightPin);
            }
        }
    }
}
