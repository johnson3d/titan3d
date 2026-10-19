using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    /// <summary>
    /// 网格逐像素求到样条的水平距离, 并给出道路/路肩遮罩与衰减。所有距离单位为米。
    /// WidthCurve 的 X 为样条 alpha [0,1], Y 为 HalfWidth 的缩放系数。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline Distance", "Sampling\\Spline Distance", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineDistanceNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn WidthCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut DistancePin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut AlphaPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut SplineHeightPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut FalloffPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut RoadMaskPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ShoulderMaskPin { get; set; } = new PinOut();

        public TtBufferCreator FloatInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtBufferCreator FloatGridDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, 1);
        public TtBufferCreator MaskGridDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, 1);

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        /// <summary>道路半宽 (米)</summary>
        [Rtti.Meta("")]
        public float HalfWidth { get; set; } = 3.0f;
        /// <summary>路肩宽度 (米), 在路面外侧</summary>
        [Rtti.Meta("")]
        public float ShoulderWidth { get; set; } = 2.0f;
        /// <summary>距离查询上限 (米), 超出后 Distance 输出为该值</summary>
        [Rtti.Meta("")]
        public float SearchDistance { get; set; } = 20.0f;

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineDistanceNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", FloatInputDesc);
            AddInput(WidthCurvePin, "WidthCurve", null, "Bezier");
            AddOutput(DistancePin, "Distance", FloatGridDesc);
            AddOutput(AlphaPin, "Alpha", FloatGridDesc);
            AddOutput(SplineHeightPin, "SplineHeight", FloatGridDesc);
            AddOutput(FalloffPin, "Falloff", FloatGridDesc);
            AddOutput(RoadMaskPin, "RoadMask", MaskGridDesc);
            AddOutput(ShoulderMaskPin, "ShoulderMask", MaskGridDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as TtPgcGraph;
            var source = graph?.ResolveInput(HeightPin);
            if (pin == DistancePin || pin == AlphaPin || pin == SplineHeightPin || pin == FalloffPin)
                return TtPgcGridUtility.ResolveGridCreator(graph, FloatGridDesc, source);
            if (pin == RoadMaskPin || pin == ShoulderMaskPin)
                return TtPgcGridUtility.ResolveGridCreator(graph, MaskGridDesc, source);
            return null;
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            LastDiagnostic = null;
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, $"Spline Distance '{Name}'",
                out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        public override bool OnProcedure(TtPgcGraph graph)
        {
            var distanceBuffer = graph.BufferCache.FindBuffer(DistancePin);
            var alphaBuffer = graph.BufferCache.FindBuffer(AlphaPin);
            var splineHeightBuffer = graph.BufferCache.FindBuffer(SplineHeightPin);
            var falloffBuffer = graph.BufferCache.FindBuffer(FalloffPin);
            var roadMaskBuffer = graph.BufferCache.FindBuffer(RoadMaskPin);
            var shoulderMaskBuffer = graph.BufferCache.FindBuffer(ShoulderMaskPin);
            var height = graph.ResolveInput(HeightPin) as TtSuperBuffer<float, FFloatOperator>;
            try
            {
                var reference = distanceBuffer ?? alphaBuffer ?? splineHeightBuffer ?? falloffBuffer ??
                    roadMaskBuffer ?? shoulderMaskBuffer;
                if (reference == null)
                    return true;
                if (graph.ExecutionContext == null || mPreparedPath?.IsValid != true)
                {
                    TtPgcSplineUtility.Warn($"Spline Distance '{Name}'",
                        string.IsNullOrEmpty(LastDiagnostic) ? "No valid spline path" : LastDiagnostic);
                    return false;
                }

                var context = graph.ExecutionContext;
                var widthCurve = GetInputNode(graph, WidthCurvePin) as TtBezier;
                float halfWidth = Math.Max(0.0f, HalfWidth);
                float shoulderWidth = Math.Max(0.0f, ShoulderWidth);
                float maxHalfWidth = halfWidth * Math.Max(0.0f, TtPgcCurveUtility.EvaluateMax(widthCurve, 1.0f));
                float searchDistance = Math.Max(Math.Max(0.0f, SearchDistance), maxHalfWidth + shoulderWidth);
                var polyline = TtSplinePolyline.Create(mPreparedPath, searchDistance);

                int width = reference.Width;
                int depth = reference.Height;
                for (int z = 0; z < depth; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var worldXZ = context.GridToWorldXZ(x, z);
                        float distance = searchDistance;
                        float alpha = 0.0f;
                        float splineHeight = height != null && x < height.Width && z < height.Height ?
                            height.GetPixel<float>(x, z, 0) : 0.0f;
                        float falloff = 0.0f;
                        sbyte road = 0;
                        sbyte shoulder = 0;
                        if (polyline.TryFindClosestXZ(in worldXZ, searchDistance, out var closest))
                        {
                            distance = closest.HorizontalDistance;
                            alpha = closest.Alpha;
                            splineHeight = closest.Position.Y;
                            float localHalfWidth = halfWidth * Math.Max(0.0f,
                                TtPgcCurveUtility.Evaluate(widthCurve, closest.Alpha, 1.0f));
                            if (distance <= localHalfWidth)
                            {
                                road = 1;
                                falloff = 1.0f;
                            }
                            else if (distance <= localHalfWidth + shoulderWidth)
                            {
                                shoulder = 1;
                                falloff = shoulderWidth > 0.0f ?
                                    TtPgcCurveUtility.SmoothStep01(1.0f - (distance - localHalfWidth) / shoulderWidth) : 0.0f;
                            }
                        }
                        distanceBuffer?.SetPixel<float>(x, z, 0, in distance);
                        alphaBuffer?.SetPixel<float>(x, z, 0, in alpha);
                        splineHeightBuffer?.SetPixel<float>(x, z, 0, in splineHeight);
                        falloffBuffer?.SetPixel<float>(x, z, 0, in falloff);
                        roadMaskBuffer?.SetPixel<sbyte>(x, z, 0, in road);
                        shoulderMaskBuffer?.SetPixel<sbyte>(x, z, 0, in shoulder);
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
