using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public enum ESplineHeightMode
    {
        /// <summary>向样条高度压平 (可升可降)</summary>
        Flatten,
        /// <summary>只抬高, 用于路基填方</summary>
        RaiseOnly,
        /// <summary>只降低, 用于路堑挖方</summary>
        LowerOnly,
    }

    /// <summary>
    /// 按样条把高度图压到路面高度, 路面外 FalloffWidth 内平滑过渡回原地形。所有距离单位为米。
    /// WidthCurve 的 X 为样条 alpha [0,1], Y 为 HalfWidth 的缩放系数。
    /// FalloffCurve 的 X 为过渡带归一化距离 (0=路面边缘, 1=过渡带外缘), Y 为混合权重; 未连接时用 smoothstep(1-t)。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline To Height", "Terrain\\Spline To Height", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineToHeightNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn WidthCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn FalloffCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutHeightPin { get; set; } = new PinOut();

        public TtBufferCreator FloatInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtBufferCreator FloatGridDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, 1);

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        [Rtti.Meta("")]
        public ESplineHeightMode Mode { get; set; } = ESplineHeightMode.Flatten;
        /// <summary>完全压平的半宽 (米), 受 WidthCurve 缩放</summary>
        [Rtti.Meta("")]
        public float HalfWidth { get; set; } = 3.0f;
        /// <summary>路面外的过渡带宽度 (米)</summary>
        [Rtti.Meta("")]
        public float FalloffWidth { get; set; } = 4.0f;
        /// <summary>目标高度相对样条的偏移 (米), 负值让路面略低于地形</summary>
        [Rtti.Meta("")]
        public float HeightOffset { get; set; } = 0.0f;
        /// <summary>整体混合强度 [0,1]</summary>
        [Rtti.Meta("")]
        public float Strength { get; set; } = 1.0f;

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineToHeightNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF404020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", FloatInputDesc);
            AddInput(WidthCurvePin, "WidthCurve", null, "Bezier");
            AddInput(FalloffCurvePin, "FalloffCurve", null, "Bezier");
            AddOutput(OutHeightPin, "Height", FloatGridDesc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == OutHeightPin)
            {
                var graph = ParentGraph as TtPgcGraph;
                return TtPgcGridUtility.ResolveGridCreator(graph, FloatGridDesc, graph?.ResolveInput(HeightPin));
            }
            return null;
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, $"Spline To Height '{Name}'",
                out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        public override bool OnProcedure(TtPgcGraph graph)
        {
            var output = graph.BufferCache.FindBuffer(OutHeightPin);
            var height = graph.ResolveInput(HeightPin) as TtSuperBuffer<float, FFloatOperator>;
            string label = $"Spline To Height '{Name}'";
            try
            {
                if (output == null)
                    return true;
                if (height == null)
                {
                    TtPgcSplineUtility.Warn(label, "Height input is required");
                    return false;
                }
                int width = Math.Min(output.Width, height.Width);
                int depth = Math.Min(output.Height, height.Height);
                bool hasPath = graph.ExecutionContext != null && mPreparedPath?.IsValid == true;
                if (!hasPath)
                    TtPgcSplineUtility.Warn(label, string.IsNullOrEmpty(LastDiagnostic) ? "No valid spline path" : LastDiagnostic);

                var context = graph.ExecutionContext;
                var widthCurve = GetInputNode(graph, WidthCurvePin) as TtBezier;
                var falloffCurve = GetInputNode(graph, FalloffCurvePin) as TtBezier;
                float halfWidth = Math.Max(0.0f, HalfWidth);
                float falloffWidth = Math.Max(0.0f, FalloffWidth);
                float strength = MathHelper.Clamp(Strength, 0.0f, 1.0f);
                float maxHalfWidth = halfWidth * Math.Max(0.0f, TtPgcCurveUtility.EvaluateMax(widthCurve, 1.0f));
                float searchDistance = maxHalfWidth + falloffWidth;
                var polyline = hasPath ? TtSplinePolyline.Create(mPreparedPath, searchDistance) : null;

                for (int z = 0; z < depth; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float source = height.GetPixel<float>(x, z, 0);
                        float result = source;
                        var worldXZ = hasPath ? context.GridToWorldXZ(x, z) : default;
                        if (hasPath && polyline.TryFindClosestXZ(in worldXZ, searchDistance, out var closest))
                        {
                            float localHalfWidth = halfWidth * Math.Max(0.0f,
                                TtPgcCurveUtility.Evaluate(widthCurve, closest.Alpha, 1.0f));
                            float weight = EvaluateWeight(falloffCurve, closest.HorizontalDistance, localHalfWidth, falloffWidth);
                            weight *= strength;
                            if (weight > 0.0f)
                            {
                                float target = closest.Position.Y + HeightOffset;
                                result = ApplyMode(Mode, source, target, weight);
                            }
                        }
                        output.SetPixel<float>(x, z, 0, in result);
                    }
                }
                return true;
            }
            finally
            {
                graph.ReleaseInput(HeightPin);
            }
        }

        internal static float EvaluateWeight(TtBezier falloffCurve, float distance, float halfWidth, float falloffWidth)
        {
            if (distance <= halfWidth)
                return 1.0f;
            if (falloffWidth <= 0.0f || distance >= halfWidth + falloffWidth)
                return 0.0f;
            float t = (distance - halfWidth) / falloffWidth;
            float weight = falloffCurve != null ?
                TtPgcCurveUtility.Evaluate(falloffCurve, t, TtPgcCurveUtility.SmoothStep01(1.0f - t)) :
                TtPgcCurveUtility.SmoothStep01(1.0f - t);
            return MathHelper.Clamp(weight, 0.0f, 1.0f);
        }

        internal static float ApplyMode(ESplineHeightMode mode, float source, float target, float weight)
        {
            float blended = source + (target - source) * weight;
            switch (mode)
            {
                case ESplineHeightMode.RaiseOnly:
                    return Math.Max(source, blended);
                case ESplineHeightMode.LowerOnly:
                    return Math.Min(source, blended);
                default:
                    return blended;
            }
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcSplineToHeight
    {
        public void UnitTestEntrance()
        {
            UnitTest.TtUnitTestManager.TAssert(TtSplineToHeightNode.EvaluateWeight(null, 1.0f, 3.0f, 4.0f) == 1.0f,
                "Spline height weight inside the road must be 1");
            UnitTest.TtUnitTestManager.TAssert(TtSplineToHeightNode.EvaluateWeight(null, 8.0f, 3.0f, 4.0f) == 0.0f,
                "Spline height weight outside the falloff must be 0");
            float middle = TtSplineToHeightNode.EvaluateWeight(null, 5.0f, 3.0f, 4.0f);
            UnitTest.TtUnitTestManager.TAssert(Math.Abs(middle - 0.5f) <= 1e-4f,
                "Spline height default falloff must be smoothstep");
            UnitTest.TtUnitTestManager.TAssert(
                TtSplineToHeightNode.ApplyMode(ESplineHeightMode.RaiseOnly, 5.0f, 2.0f, 1.0f) == 5.0f &&
                TtSplineToHeightNode.ApplyMode(ESplineHeightMode.LowerOnly, 5.0f, 2.0f, 1.0f) == 2.0f &&
                TtSplineToHeightNode.ApplyMode(ESplineHeightMode.Flatten, 5.0f, 2.0f, 0.5f) == 3.5f,
                "Spline height blend mode is incorrect");
        }
    }
}
