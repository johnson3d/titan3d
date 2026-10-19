using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    /// <summary>
    /// 沿样条在 CPU 端挤出道路网格, 作为程序化 MeshNode 交给 PGC Volume 生成。所有距离单位为米。
    /// WidthCurve 的 X 为样条 alpha [0,1], Y 为 HalfWidth 的缩放系数。
    /// UV: U 横向 [0, UTiles], V 沿路方向每 UVTileLength 米重复一次。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline Road Mesh", "Mesh\\Spline Road Mesh", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineRoadMeshNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn WidthCurvePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut PointsPin { get; set; } = new PinOut();

        public TtBufferCreator FloatInputDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);

        /// <summary>沿样条的环数上限, 防止 SampleSpacing 过小导致顶点爆炸</summary>
        const int MaxRings = 100000;
        const int MaxCrossSegments = 64;
        /// <summary>内弯侧半宽不超过转弯半径的比例, 避免内侧顶点折叠</summary>
        const float InnerRadiusRatio = 0.95f;

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        /// <summary>道路半宽 (米), 受 WidthCurve 缩放</summary>
        [Rtti.Meta("")]
        public float HalfWidth { get; set; } = 3.0f;
        /// <summary>横截面分段数, 贴地时越多越贴合</summary>
        [Rtti.Meta("")]
        public int CrossSegments { get; set; } = 4;
        /// <summary>沿样条的采样间距 (米)</summary>
        [Rtti.Meta("")]
        public float SampleSpacing { get; set; } = 1.0f;
        /// <summary>纹理沿路方向重复一次的长度 (米)</summary>
        [Rtti.Meta("")]
        public float UVTileLength { get; set; } = 4.0f;
        /// <summary>纹理横向重复次数</summary>
        [Rtti.Meta("")]
        public float UTiles { get; set; } = 1.0f;
        /// <summary>路面抬高 (米), 防止与地形 z-fighting</summary>
        [Rtti.Meta("")]
        public float HeightOffset { get; set; } = 0.02f;
        /// <summary>顶点高度取 Height 输入 (需连接); 否则取样条高度</summary>
        [Rtti.Meta("")]
        public bool ConformToHeight { get; set; } = false;
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt + "," + Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
        public RName Material { get; set; }
        [Rtti.Meta("")]
        public bool EnableCollision { get; set; } = true;
        [Rtti.Meta("")]
        public string NamePrefix { get; set; } = "Road";

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineRoadMeshNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF402020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", FloatInputDesc);
            AddInput(WidthCurvePin, "WidthCurve", null, "Bezier");
            AddOutput(PointsPin, "Points", Vector3Desc);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == PointsPin)
                return Vector3Desc;
            return null;
        }

        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            base.UpdateAMetaReferences(ameta);
            if (Material != null)
                ameta.AddReferenceAsset(Material);
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, $"Spline Road Mesh '{Name}'",
                out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            var points = graph.BufferCache.FindBuffer(PointsPin);
            points?.ResizePixels();
            var height = graph.ResolveInput(HeightPin) as TtSuperBuffer<float, FFloatOperator>;
            string label = $"Spline Road Mesh '{Name}'";
            try
            {
                if (graph.ExecutionContext == null || mPreparedPath?.IsValid != true)
                {
                    TtPgcSplineUtility.Warn(label, string.IsNullOrEmpty(LastDiagnostic) ? "No valid spline path" : LastDiagnostic);
                    return false;
                }
                float spacing = SampleSpacing;
                if (!float.IsFinite(spacing) || spacing <= 1e-3f)
                {
                    TtPgcSplineUtility.Warn(label, "SampleSpacing must be a finite value greater than 0.001");
                    return false;
                }

                var context = graph.ExecutionContext;
                var widthCurve = GetInputNode(graph, WidthCurvePin) as TtBezier;
                var buffer = BuildRoad(mPreparedPath, widthCurve, context, ConformToHeight ? height : null,
                    out var ringCenters);
                if (buffer == null)
                {
                    TtPgcSplineUtility.Warn(label, "Spline is too short to build a road");
                    return false;
                }
                if (points != null)
                {
                    for (int i = 0; i < ringCenters.Count; i++)
                    {
                        var center = ringCenters[i];
                        points.AddPixel(in center);
                    }
                }

                var pivot = Vector3.Zero;
                for (int i = 0; i < ringCenters.Count; i++)
                    pivot += ringCenters[i];
                pivot /= ringCenters.Count;
                buffer.Translate(-pivot);

                var provider = buffer.Build("@PgcRoad");
                if (provider == null)
                {
                    TtPgcSplineUtility.Warn(label, "Failed to build road mesh data");
                    return false;
                }
                var material = TtPgcAssetUtility.LoadMaterial(Material);
                if (material == null)
                {
                    TtPgcSplineUtility.Warn(label, "Cannot load road material or the default material");
                    return false;
                }
                var worldTransform = FTransform.CreateTransform(pivot.AsDVector(), Vector3.One, Quaternion.Identity);
                context.AddMeshRequest(new TtPgcMeshRequest(NodeId, Name, NamePrefix, provider, in worldTransform,
                    new Graphics.Pipeline.Shader.TtMaterial[] { material }, EnableCollision));
                return true;
            }
            finally
            {
                graph.ReleaseInput(HeightPin);
            }
        }

        /// <summary>
        /// 生成世界空间道路网格。环 i 沿路方向, 列 j 从左 (j=0) 到右 (j=C)。
        /// 三角形 (i,j),(i+1,j),(i,j+1) 与 (i+1,j),(i+1,j+1),(i,j+1) 在左手 Y-up 下法线朝 +Y。
        /// </summary>
        internal TtPgcMeshBuffer BuildRoad(TtSplinePathSnapshot path, TtBezier widthCurve,
            TtPgcExecutionContext context, TtSuperBuffer<float, FFloatOperator> height, out List<Vector3> ringCenters)
        {
            ringCenters = new List<Vector3>();
            float length = path.Length;
            if (!(length > 1e-3f))
                return null;
            int crossSegments = Math.Clamp(CrossSegments, 1, MaxCrossSegments);
            int segmentCount = (int)Math.Clamp(Math.Ceiling(length / SampleSpacing), 1, MaxRings);
            // 闭合样条保留首尾重合的一环, 让 V 连续不回绕
            int ringCount = segmentCount + 1;

            var centers = new Vector3[ringCount];
            var forwards = new Vector3[ringCount];
            var rights = new Vector3[ringCount];
            var distances = new float[ringCount];
            int validCount = 0;
            for (int i = 0; i < ringCount; i++)
            {
                float distance = length * i / segmentCount;
                if (!path.TrySample(Math.Min(distance, length), out var center, out var tangent))
                    continue;
                TtPgcSplineUtility.TryGetHorizontalFrame(in tangent, out var forward, out var right);
                centers[validCount] = center;
                forwards[validCount] = forward;
                rights[validCount] = right;
                distances[validCount] = distance;
                validCount++;
            }
            if (validCount < 2)
                return null;

            float baseHalfWidth = Math.Max(0.0f, HalfWidth);
            float tileLength = UVTileLength > 1e-3f ? UVTileLength : 1.0f;
            var buffer = new TtPgcMeshBuffer();
            for (int i = 0; i < validCount; i++)
            {
                float alpha = distances[i] / length;
                float halfWidth = baseHalfWidth * Math.Max(0.0f, TtPgcCurveUtility.Evaluate(widthCurve, alpha, 1.0f));
                float halfLeft = halfWidth;
                float halfRight = halfWidth;

                int prev = i > 0 ? i - 1 : (path.IsClosed ? validCount - 2 : i);
                int next = i + 1 < validCount ? i + 1 : (path.IsClosed ? 1 : i);
                if (prev != next && prev >= 0 && next < validCount)
                {
                    float ds = distances[next] - distances[prev];
                    if (ds <= 0.0f)
                        ds += length;
                    float cosAngle = MathHelper.Clamp(Vector3.Dot(in forwards[prev], in forwards[next]), -1.0f, 1.0f);
                    float curvature = ds > 1e-4f ? (float)Math.Acos(cosAngle) / ds : 0.0f;
                    if (curvature > 1e-4f)
                    {
                        float innerLimit = InnerRadiusRatio / curvature;
                        var turn = forwards[next] - forwards[prev];
                        // 向右转时右侧是内弯
                        if (Vector3.Dot(in turn, in rights[i]) > 0.0f)
                            halfRight = Math.Min(halfRight, innerLimit);
                        else
                            halfLeft = Math.Min(halfLeft, innerLimit);
                    }
                }

                var center = centers[i];
                var right = rights[i];
                var forward = forwards[i];
                float v = distances[i] / tileLength;
                float w = TtPgcMeshBuffer.TangentHandedness(Vector3.Up, in right, in forward);
                var tangent = new Vector4(right.X, right.Y, right.Z, w);
                for (int j = 0; j <= crossSegments; j++)
                {
                    float s = -1.0f + 2.0f * j / crossSegments;
                    float lateral = s < 0.0f ? s * halfLeft : s * halfRight;
                    var position = center + right * lateral;
                    if (TtPgcGridUtility.TrySampleHeight(context, height, new Vector2(position.X, position.Z), out float groundY))
                        position.Y = groundY;
                    position.Y += HeightOffset;
                    var uv = new Vector2((float)j / crossSegments * UTiles, v);
                    buffer.AddVertex(in position, Vector3.Up, in tangent, in uv);
                }
                ringCenters.Add(center);
            }

            var indices = buffer.GetAtom(0);
            uint stride = (uint)(crossSegments + 1);
            for (uint i = 0; i + 1 < (uint)validCount; i++)
            {
                for (uint j = 0; j < (uint)crossSegments; j++)
                {
                    uint a = i * stride + j;
                    uint b = (i + 1) * stride + j;
                    uint c = i * stride + j + 1;
                    uint d = (i + 1) * stride + j + 1;
                    indices.Add(a); indices.Add(b); indices.Add(c);
                    indices.Add(b); indices.Add(d); indices.Add(c);
                }
            }
            buffer.ComputeSmoothNormals(Vector3.Up);
            return buffer;
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcRoadMesh
    {
        public void UnitTestEntrance()
        {
            var spline = new TtBezier3DSpline { Segments = 64 };
            spline.AppendPoint(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitZ * (10.0f / 3.0f));
            spline.AppendPoint(Vector3.UnitZ * 10.0f, Vector3.UnitZ * (20.0f / 3.0f), Vector3.UnitZ * 11.0f);
            var path = TtSplinePathSnapshot.Create(spline.CreateSnapshot(), FTransform.Identity);
            var node = new TtSplineRoadMeshNode { HalfWidth = 2.0f, CrossSegments = 2, SampleSpacing = 1.0f, HeightOffset = 0.0f };
            var buffer = node.BuildRoad(path, null, null, null, out var centers);
            UnitTest.TtUnitTestManager.TAssert(buffer != null && centers.Count >= 11 && buffer.VertexCount == centers.Count * 3,
                "Road mesh ring/vertex count is incorrect");
            // 沿 +Z 前进, 第 0 列在左 (-X), 最后一列在右 (+X)
            UnitTest.TtUnitTestManager.TAssert(Math.Abs(buffer.Positions[0].X + 2.0f) <= 1e-3f &&
                Math.Abs(buffer.Positions[2].X - 2.0f) <= 1e-3f, "Road mesh lateral layout is incorrect");
            bool upward = true;
            for (int i = 0; i < buffer.Normals.Count; i++)
                upward &= buffer.Normals[i].Y > 0.99f;
            UnitTest.TtUnitTestManager.TAssert(upward, "Road mesh winding does not face +Y");
            UnitTest.TtUnitTestManager.TAssert(buffer.Tangents[0].W == 0.0f,
                "Road mesh tangent handedness is incorrect");
        }
    }
}
