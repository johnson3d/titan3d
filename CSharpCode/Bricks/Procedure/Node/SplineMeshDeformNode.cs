using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public enum EMeshForwardAxis
    {
        PositiveX,
        NegativeX,
        PositiveZ,
        NegativeZ,
    }

    /// <summary>
    /// 把一个 .ums 模型沿样条首尾相接重复, 并在 CPU 端按样条弯曲每个顶点 (类似 UE SplineMeshComponent),
    /// 用于护栏、路缘石、围墙、管线。所有距离单位为米。
    /// 模型局部空间: ForwardAxis 为沿样条方向, +Y 为上, 右方向 = Cross(Up, Forward)。
    /// CPU 变形而非 GPU Modifier: 结果是普通静态网格, 碰撞/烘焙/拾取都直接可用, 也不需要每帧求值。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spline Mesh Deform", "Mesh\\Spline Mesh Deform", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSplineMeshDeformNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinOut PointsPin { get; set; } = new PinOut();

        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);

        /// <summary>输出顶点总数上限, 超过时减少重复段数</summary>
        const int MaxOutputVertices = 4000000;
        /// <summary>模型沿前进轴的最小长度 (米), 更短视为退化</summary>
        const float MinMeshLengthMeters = 1e-3f;

        [Rtti.Meta("")]
        public ESplineQueryScope QueryScope { get; set; } = ESplineQueryScope.VolumeDescendants;
        [Rtti.Meta("")]
        public string SplineName { get; set; }
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName MeshName { get; set; }
        [Rtti.Meta("")]
        public EMeshForwardAxis ForwardAxis { get; set; } = EMeshForwardAxis.PositiveZ;
        /// <summary>起始弧长 (米)</summary>
        [Rtti.Meta("")]
        public float StartDistance { get; set; } = 0.0f;
        /// <summary>结束弧长 (米), 负数表示样条终点</summary>
        [Rtti.Meta("")]
        public float EndDistance { get; set; } = -1.0f;
        /// <summary>拉伸每段使其恰好铺满区间; 关闭时按原长铺, 末尾不足一段的部分留空</summary>
        [Rtti.Meta("")]
        public bool StretchToFit { get; set; } = true;
        /// <summary>横向偏移 (米), 沿样条右侧为正</summary>
        [Rtti.Meta("")]
        public float LateralOffset { get; set; } = 0.0f;
        /// <summary>横向缩放</summary>
        [Rtti.Meta("")]
        public float LateralScale { get; set; } = 1.0f;
        /// <summary>高度偏移 (米)</summary>
        [Rtti.Meta("")]
        public float HeightOffset { get; set; } = 0.0f;
        [Rtti.Meta("")]
        public int MaxTiles { get; set; } = 1000;
        [Rtti.Meta("")]
        public bool EnableCollision { get; set; } = true;
        [Rtti.Meta("")]
        public string NamePrefix { get; set; } = "SplineMesh";

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        TtSplinePathSnapshot mPreparedPath;

        public TtSplineMeshDeformNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF402020;
            BackColor = 0x80808080;

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
            if (MeshName != null)
                ameta.AddReferenceAsset(MeshName);
        }

        public override bool InitProcedure(TtPgcGraph graph)
        {
            TtPgcSplineUtility.TryPreparePath(graph, QueryScope, SplineName, $"Spline Mesh Deform '{Name}'",
                out mPreparedPath, out var diagnostic);
            LastDiagnostic = diagnostic;
            return true;
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            var points = graph.BufferCache.FindBuffer(PointsPin);
            points?.ResizePixels();
            string label = $"Spline Mesh Deform '{Name}'";
            if (graph.ExecutionContext == null || mPreparedPath?.IsValid != true)
            {
                TtPgcSplineUtility.Warn(label, string.IsNullOrEmpty(LastDiagnostic) ? "No valid spline path" : LastDiagnostic);
                return false;
            }
            if (MeshName == null)
            {
                TtPgcSplineUtility.Warn(label, "MeshName is required");
                return false;
            }
            var materialMesh = TtPgcAssetUtility.LoadMaterialMeshWithData(MeshName);
            if (materialMesh == null)
            {
                TtPgcSplineUtility.Warn(label, $"Cannot load mesh {MeshName}");
                return false;
            }
            var source = TtDeformSourceMesh.Read(materialMesh);
            if (source.Positions.Count == 0 || source.AtomIndices.Count == 0)
            {
                TtPgcSplineUtility.Warn(label, $"Mesh {MeshName} has no readable CPU mesh data");
                return false;
            }

            var buffer = Deform(source, mPreparedPath, out var tileStarts, out var warning);
            if (!string.IsNullOrEmpty(warning))
                TtPgcSplineUtility.Warn(label, warning);
            if (buffer == null)
                return false;
            if (points != null)
            {
                for (int i = 0; i < tileStarts.Count; i++)
                {
                    var start = tileStarts[i];
                    points.AddPixel(in start);
                }
            }

            var pivot = Vector3.Zero;
            for (int i = 0; i < tileStarts.Count; i++)
                pivot += tileStarts[i];
            pivot /= tileStarts.Count;
            buffer.Translate(-pivot);
            var provider = buffer.Build("@PgcSplineMesh");
            if (provider == null)
            {
                TtPgcSplineUtility.Warn(label, "Failed to build deformed mesh data");
                return false;
            }

            var materials = new Graphics.Pipeline.Shader.TtMaterial[source.AtomMaterials.Count];
            Graphics.Pipeline.Shader.TtMaterial fallback = null;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = source.AtomMaterials[i];
                if (materials[i] == null)
                    materials[i] = fallback ??= TtPgcAssetUtility.LoadMaterial(null);
                if (materials[i] == null)
                {
                    TtPgcSplineUtility.Warn(label, "Cannot load the default material");
                    return false;
                }
            }
            var worldTransform = FTransform.CreateTransform(pivot.AsDVector(), Vector3.One, Quaternion.Identity);
            graph.ExecutionContext.AddMeshRequest(new TtPgcMeshRequest(NodeId, Name, NamePrefix, provider,
                in worldTransform, materials, EnableCollision));
            return true;
        }

        internal void GetAxes(out Vector3 forward, out Vector3 right)
        {
            switch (ForwardAxis)
            {
                case EMeshForwardAxis.PositiveX:
                    forward = Vector3.UnitX;
                    break;
                case EMeshForwardAxis.NegativeX:
                    forward = -Vector3.UnitX;
                    break;
                case EMeshForwardAxis.NegativeZ:
                    forward = -Vector3.UnitZ;
                    break;
                default:
                    forward = Vector3.UnitZ;
                    break;
            }
            var up = Vector3.Up;
            right = Vector3.Cross(in up, in forward);
        }

        /// <summary>
        /// 模型局部 (x=右, y=上, f=前) 映射到样条: pos = P(d) + R*x + U*y, 其中 d 随 f 线性变化。
        /// 源基 (R0, Up, A) 与目标基 (R, U, F) 构造方式相同, 是纯旋转, 所以切线 W (手性) 不变。
        /// </summary>
        internal TtPgcMeshBuffer Deform(TtDeformSourceMesh source, TtSplinePathSnapshot path,
            out List<Vector3> tileStarts, out string warning)
        {
            tileStarts = new List<Vector3>();
            warning = null;
            GetAxes(out var axis, out var axisRight);
            var up = Vector3.Up;

            float minF = float.MaxValue;
            float maxF = float.MinValue;
            for (int i = 0; i < source.Positions.Count; i++)
            {
                float f = Vector3.Dot(source.Positions[i], axis);
                minF = Math.Min(minF, f);
                maxF = Math.Max(maxF, f);
            }
            float meshLength = maxF - minF;
            if (!(meshLength > MinMeshLengthMeters))
            {
                warning = "Mesh is degenerate along the forward axis";
                return null;
            }

            float length = path.Length;
            bool isClosed = path.IsClosed;
            float start = float.IsFinite(StartDistance) ? MathHelper.Clamp(StartDistance, 0.0f, length) : 0.0f;
            float end = !float.IsFinite(EndDistance) || EndDistance < 0.0f ? length : MathHelper.Clamp(EndDistance, start, length);
            float range = end - start;
            if (!(range > MinMeshLengthMeters))
            {
                warning = "Spline range is empty";
                return null;
            }

            int tiles = StretchToFit ? Math.Max(1, (int)Math.Round(range / meshLength)) : (int)Math.Floor(range / meshLength);
            int maxTiles = Math.Max(1, MaxTiles);
            int vertexLimitTiles = Math.Max(1, MaxOutputVertices / Math.Max(1, source.Positions.Count));
            if (tiles > maxTiles || tiles > vertexLimitTiles)
            {
                warning = $"Tile count {tiles} is clamped to {Math.Min(maxTiles, vertexLimitTiles)}";
                tiles = Math.Min(maxTiles, vertexLimitTiles);
            }
            if (tiles <= 0)
            {
                warning = "Spline range is shorter than the mesh";
                return null;
            }
            float tileLength = StretchToFit ? range / tiles : meshLength;
            float stretch = tileLength / meshLength;
            float lateralScale = Math.Max(1e-3f, LateralScale);

            var buffer = new TtPgcMeshBuffer();
            int sourceVertexCount = source.Positions.Count;
            for (int tile = 0; tile < tiles; tile++)
            {
                uint baseVertex = (uint)buffer.VertexCount;
                float tileStart = start + tile * tileLength;
                if (SampleFrame(path, tileStart, isClosed, out var startPos, out _, out var startRight, out _))
                    tileStarts.Add(startPos + startRight * LateralOffset);

                for (int i = 0; i < sourceVertexCount; i++)
                {
                    var v = source.Positions[i];
                    float f = (Vector3.Dot(in v, in axis) - minF) * stretch;
                    float x = Vector3.Dot(in v, in axisRight) * lateralScale + LateralOffset;
                    float y = v.Y + HeightOffset;
                    SampleFrame(path, tileStart + f, isClosed, out var center, out var forward, out var right, out var frameUp);

                    var position = center + right * x + frameUp * y;
                    var normal = RotateToFrame(source.Normals[i], in axisRight, in up, in axis, in right, in frameUp, in forward);
                    normal.Normalize();
                    var srcTangent = source.Tangents[i];
                    var tangent3 = RotateToFrame(new Vector3(srcTangent.X, srcTangent.Y, srcTangent.Z),
                        in axisRight, in up, in axis, in right, in frameUp, in forward);
                    tangent3.Normalize();
                    var tangent = new Vector4(tangent3.X, tangent3.Y, tangent3.Z, srcTangent.W);
                    buffer.AddVertex(in position, in normal, in tangent, source.UVs[i], source.Colors[i]);
                }

                for (int atom = 0; atom < source.AtomIndices.Count; atom++)
                {
                    var sourceIndices = source.AtomIndices[atom];
                    var target = buffer.GetAtom(atom);
                    for (int k = 0; k < sourceIndices.Count; k++)
                        target.Add(sourceIndices[k] + baseVertex);
                }
            }
            if (tileStarts.Count == 0)
            {
                warning = "Failed to sample the spline";
                return null;
            }
            return buffer;
        }

        static Vector3 RotateToFrame(in Vector3 v, in Vector3 srcRight, in Vector3 srcUp, in Vector3 srcForward,
            in Vector3 right, in Vector3 up, in Vector3 forward)
        {
            return right * Vector3.Dot(in v, in srcRight) + up * Vector3.Dot(in v, in srcUp) +
                forward * Vector3.Dot(in v, in srcForward);
        }

        /// <summary>
        /// 样条上的正交标架: F 为 3D 切线 (保留坡度), R = Cross(Up, F) 保持水平 (不随坡度侧倾), U = Cross(F, R)。
        /// </summary>
        static bool SampleFrame(TtSplinePathSnapshot path, float distance, bool isClosed,
            out Vector3 position, out Vector3 forward, out Vector3 right, out Vector3 up)
        {
            distance = TtPgcSplineUtility.ResolveDistance(distance, path.Length, isClosed);
            if (!path.TrySample(distance, out position, out var tangent))
            {
                forward = Vector3.Forward;
                right = Vector3.Right;
                up = Vector3.Up;
                return false;
            }
            forward = tangent;
            var worldUp = Vector3.Up;
            if (forward.Normalize() <= 1e-6f)
                forward = Vector3.Forward;
            right = Vector3.Cross(in worldUp, in forward);
            if (right.Normalize() <= 1e-5f)
            {
                // 切线竖直: 退回水平标架, 前方取水平前方
                TtPgcSplineUtility.TryGetHorizontalFrame(in tangent, out _, out right);
            }
            up = Vector3.Cross(in forward, in right);
            up.Normalize();
            return true;
        }

        /// <summary>从 .ums 读出的 CPU 网格, 多个子网格的 atom 按顺序拼接。</summary>
        internal sealed class TtDeformSourceMesh
        {
            public readonly List<Vector3> Positions = new List<Vector3>();
            public readonly List<Vector3> Normals = new List<Vector3>();
            public readonly List<Vector4> Tangents = new List<Vector4>();
            public readonly List<Vector2> UVs = new List<Vector2>();
            public readonly List<uint> Colors = new List<uint>();
            public readonly List<List<uint>> AtomIndices = new List<List<uint>>();
            public readonly List<Graphics.Pipeline.Shader.TtMaterial> AtomMaterials = new List<Graphics.Pipeline.Shader.TtMaterial>();

            public static unsafe TtDeformSourceMesh Read(Graphics.Mesh.TtMaterialMesh materialMesh)
            {
                var result = new TtDeformSourceMesh();
                foreach (var subMesh in materialMesh.SubMeshes)
                {
                    var provider = subMesh.Mesh?.MeshDataProvider;
                    if (provider == null)
                        continue;
                    var core = provider.mCoreObject;
                    uint vertexCount = core.GetVertexNumber();
                    var posBlob = core.GetStream(NxRHI.EVertexStreamType.VST_Position);
                    if (vertexCount == 0 || !posBlob.IsValidPointer)
                        continue;
                    var norBlob = core.GetStream(NxRHI.EVertexStreamType.VST_Normal);
                    var tanBlob = core.GetStream(NxRHI.EVertexStreamType.VST_Tangent);
                    var uvBlob = core.GetStream(NxRHI.EVertexStreamType.VST_UV);
                    var colorBlob = core.GetStream(NxRHI.EVertexStreamType.VST_Color);
                    var pPos = (Vector3*)posBlob.GetData();
                    var pNor = norBlob.IsValidPointer ? (Vector3*)norBlob.GetData() : null;
                    var pTan = tanBlob.IsValidPointer ? (Vector4*)tanBlob.GetData() : null;
                    var pUV = uvBlob.IsValidPointer ? (Vector2*)uvBlob.GetData() : null;
                    var pColor = colorBlob.IsValidPointer ? (uint*)colorBlob.GetData() : null;

                    uint baseVertex = (uint)result.Positions.Count;
                    for (uint i = 0; i < vertexCount; i++)
                    {
                        result.Positions.Add(pPos[i]);
                        result.Normals.Add(pNor != null ? pNor[i] : Vector3.Up);
                        result.Tangents.Add(pTan != null ? pTan[i] : new Vector4(1, 0, 0, 0));
                        result.UVs.Add(pUV != null ? pUV[i] : Vector2.Zero);
                        result.Colors.Add(pColor != null ? pColor[i] : 0xffffffff);
                    }

                    uint atomCount = core.GetAtomNumber();
                    for (uint atom = 0; atom < atomCount; atom++)
                    {
                        if (core.GetAtomLOD(atom) == 0)
                            continue;
                        var desc = core.GetAtom(atom, 0);
                        if (desc == null)
                            continue;
                        var indices = new List<uint>();
                        uint triangleCount = desc->m_NumPrimitives;
                        for (uint t = 0; t < triangleCount; t++)
                        {
                            uint a = 0, b = 0, c = 0;
                            if (!core.GetAtomTriangle(atom, t, ref a, ref b, ref c))
                                break;
                            if (a >= vertexCount || b >= vertexCount || c >= vertexCount)
                                continue;
                            indices.Add(a + baseVertex);
                            indices.Add(b + baseVertex);
                            indices.Add(c + baseVertex);
                        }
                        if (indices.Count == 0)
                            continue;
                        result.AtomIndices.Add(indices);
                        var materials = subMesh.Materials;
                        result.AtomMaterials.Add(materials != null && atom < materials.Count ? materials[(int)atom] : null);
                    }
                }
                return result;
            }
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcSplineMeshDeform
    {
        public void UnitTestEntrance()
        {
            // 直线样条沿 +Z 长 10 米, 模型是沿 +Z 长 2 米、在 +X 侧 1 米处的一个三角形
            var spline = new TtBezier3DSpline { Segments = 64 };
            spline.AppendPoint(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitZ * (10.0f / 3.0f));
            spline.AppendPoint(Vector3.UnitZ * 10.0f, Vector3.UnitZ * (20.0f / 3.0f), Vector3.UnitZ * 11.0f);
            var path = TtSplinePathSnapshot.Create(spline.CreateSnapshot(), FTransform.Identity);

            var source = new TtSplineMeshDeformNode.TtDeformSourceMesh();
            source.Positions.Add(new Vector3(1, 0, 0));
            source.Positions.Add(new Vector3(1, 0, 2));
            source.Positions.Add(new Vector3(1, 1, 0));
            for (int i = 0; i < 3; i++)
            {
                source.Normals.Add(-Vector3.UnitX);
                source.Tangents.Add(new Vector4(0, 0, 1, 1));
                source.UVs.Add(Vector2.Zero);
                source.Colors.Add(0xffffffff);
            }
            source.AtomIndices.Add(new List<uint> { 0, 1, 2 });
            source.AtomMaterials.Add(null);

            var node = new TtSplineMeshDeformNode();
            var buffer = node.Deform(source, path, out var tileStarts, out _);
            UnitTest.TtUnitTestManager.TAssert(buffer != null && tileStarts.Count == 5 && buffer.VertexCount == 15,
                "Spline mesh deform tile count is incorrect");
            UnitTest.TtUnitTestManager.TAssert(Math.Abs(buffer.Positions[3].X - 1.0f) <= 1e-2f &&
                Math.Abs(buffer.Positions[3].Z - 2.0f) <= 1e-2f && Math.Abs(buffer.Positions[5].Y - 1.0f) <= 1e-2f,
                "Spline mesh deform position mapping is incorrect");
            UnitTest.TtUnitTestManager.TAssert(buffer.Normals[0].X < -0.99f && buffer.Tangents[0].W == 1.0f,
                "Spline mesh deform must rotate normals and keep tangent handedness");
            UnitTest.TtUnitTestManager.TAssert(buffer.AtomIndices[0].Count == 15 && buffer.AtomIndices[0][3] == 3,
                "Spline mesh deform index offset is incorrect");
        }
    }
}
