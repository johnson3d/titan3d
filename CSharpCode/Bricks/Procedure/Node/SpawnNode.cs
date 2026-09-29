using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    /// <summary>
    /// 将点数据转换为场景节点生成请求。实际场景修改由 PGC 宿主在图执行完成后统一处理。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("Spawn Node", "Scene\\Spawn Node", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("")]
    public partial class TtSpawnNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn PointsPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn NormalsPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn TangentsPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn ScalesPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn SeedsPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut PointsOutPin { get; set; } = new PinOut();

        public TtBufferCreator Vector3Desc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3, FFloat3Operator>>(-1, 1, 1);
        public TtBufferCreator IntDesc { get; } =
            TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, 1, 1);

        [Rtti.Meta("")]
        [Browsable(false)]
        public string NodeTypeName { get; set; } = Rtti.TtTypeDesc.TypeStr(typeof(GamePlay.Scene.TtMeshNode));

        [Category("Spawn")]
        [EGui.Controls.PropertyGrid.TtPGTypeEditor(typeof(GamePlay.Scene.TtNode),
            FilterMode = EGui.Controls.UTypeSelector.EFilterMode.IncludeObjectType)]
        public Rtti.TtTypeDesc SpawnType
        {
            get => string.IsNullOrEmpty(NodeTypeName) ? null : Rtti.TtTypeDesc.TypeOf(NodeTypeName);
            set => NodeTypeName = value == null ? null : Rtti.TtTypeDesc.TypeStr(value);
        }

        [Rtti.Meta("")]
        [Category("Spawn")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName MeshName { get; set; }

        [Rtti.Meta("")]
        [Category("Spawn")]
        public string NamePrefix { get; set; } = "PgcNode";

        [Rtti.Meta("")]
        [Category("Transform")]
        public Vector3 DefaultScale { get; set; } = Vector3.One;

        [Browsable(false)]
        public string LastDiagnostic { get; private set; }

        public TtSpawnNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40A0FF;
            TitleColor = 0xFF204060;
            BackColor = 0x80808080;

            AddInput(PointsPin, "Points", Vector3Desc);
            AddInput(NormalsPin, "Normals", Vector3Desc);
            AddInput(TangentsPin, "Tangents", Vector3Desc);
            AddInput(ScalesPin, "Scales", Vector3Desc);
            AddInput(SeedsPin, "Seeds", IntDesc);
            AddOutput(PointsOutPin, "Points", Vector3Desc);
        }

        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            base.UpdateAMetaReferences(ameta);
            if (MeshName != null)
                ameta.AddReferenceAsset(MeshName);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin != PointsOutPin)
                return null;
            var graph = ParentGraph as TtPgcGraph;
            var points = graph?.ResolveInput(PointsPin);
            if (points != null)
                Vector3Desc.SetSize(points.BufferCreator);
            return Vector3Desc;
        }

        public override unsafe bool OnProcedure(TtPgcGraph graph)
        {
            LastDiagnostic = null;
            var points = graph.ResolveInput(PointsPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var normals = graph.ResolveInput(NormalsPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var tangents = graph.ResolveInput(TangentsPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var scales = graph.ResolveInput(ScalesPin) as TtSuperBuffer<Vector3, FFloat3Operator>;
            var seeds = graph.ResolveInput(SeedsPin) as TtSuperBuffer<int, FIntOperator>;
            try
            {
                var output = graph.BufferCache.FindBuffer(PointsOutPin);
                output?.ResizePixels();
                if (points == null)
                    return Fail(graph, "Points input is required");
                if (graph.ExecutionContext == null)
                    return Fail(graph, "Spawn Node requires a runtime PGC execution context");

                int count = GetElementCount(points);
                if (!ValidateOptionalCount(graph, normals, count, "Normals") ||
                    !ValidateOptionalCount(graph, tangents, count, "Tangents") ||
                    !ValidateOptionalCount(graph, scales, count, "Scales") ||
                    !ValidateOptionalCount(graph, seeds, count, "Seeds"))
                    return false;

                var nodeType = SpawnType?.SystemType;
                if (nodeType == null || nodeType.IsAbstract || !nodeType.IsSubclassOf(typeof(GamePlay.Scene.TtNode)) ||
                    GamePlay.Scene.TtNode.GetNodeAttribute(nodeType) == null)
                    return Fail(graph, "NodeType must be a non-abstract TtNode type with TtNodeAttribute");
                if (typeof(GamePlay.Scene.TtMeshNode).IsAssignableFrom(nodeType) && MeshName == null)
                    return Fail(graph, "MeshName is required when spawning TtMeshNode");

                var transforms = new FTransform[count];
                var requestSeeds = new int[count];
                for (int i = 0; i < count; i++)
                {
                    var point = GetLinearPixel<Vector3>(points, i);
                    output?.AddPixel(in point);
                    var normal = GetOptionalVector(normals, i, Vector3.Up);
                    var tangent = GetOptionalVector(tangents, i, Vector3.Forward);
                    var scale = GetOptionalVector(scales, i, DefaultScale);
                    var rotation = BuildRotation(in tangent, in normal, tangents != null, normals != null);
                    transforms[i] = FTransform.CreateTransform(point.AsDVector(), scale, rotation);
                    requestSeeds[i] = seeds == null ? unchecked(graph.ExecutionContext.Seed + i) :
                        GetLinearPixel<int>(seeds, GetOptionalIndex(seeds, i));
                }

                graph.ExecutionContext.AddSpawnRequest(new TtPgcSpawnRequest(
                    NodeId, Name, nodeType, MeshName, NamePrefix, transforms, requestSeeds));
                return true;
            }
            finally
            {
                graph.ReleaseInput(PointsPin);
                graph.ReleaseInput(NormalsPin);
                graph.ReleaseInput(TangentsPin);
                graph.ReleaseInput(ScalesPin);
                graph.ReleaseInput(SeedsPin);
            }
        }

        bool ValidateOptionalCount(TtPgcGraph graph, TtBufferComponent buffer, int pointCount, string inputName)
        {
            if (buffer == null)
                return true;
            int count = GetElementCount(buffer);
            if (count == 1 || count == pointCount)
                return true;
            return Fail(graph, $"{inputName} has {count} elements; expected 1 or {pointCount}");
        }

        bool Fail(TtPgcGraph graph, string message)
        {
            LastDiagnostic = message;
            graph.ExecutionContext?.InvalidateSpawnRequests($"Spawn Node '{Name}': {message}");
            Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Warning,
                $"Spawn Node '{Name}': {message}");
            return false;
        }

        static int GetElementCount(TtBufferComponent buffer)
        {
            if (buffer == null)
                return 0;
            long count = (long)buffer.Width * buffer.Height * buffer.Depth;
            return count > int.MaxValue ? int.MaxValue : (int)count;
        }

        static int GetOptionalIndex(TtBufferComponent buffer, int pointIndex)
        {
            return GetElementCount(buffer) == 1 ? 0 : pointIndex;
        }

        static Vector3 GetOptionalVector(TtSuperBuffer<Vector3, FFloat3Operator> buffer,
            int pointIndex, in Vector3 defaultValue)
        {
            return buffer == null ? defaultValue :
                GetLinearPixel<Vector3>(buffer, GetOptionalIndex(buffer, pointIndex));
        }

        static T GetLinearPixel<T>(TtBufferComponent buffer, int index) where T : unmanaged
        {
            int width = Math.Max(1, buffer.Width);
            int height = Math.Max(1, buffer.Height);
            int x = index % width;
            int y = (index / width) % height;
            int z = index / (width * height);
            return buffer.GetPixel<T>(x, y, z);
        }

        internal static Quaternion BuildRotation(in Vector3 inputTangent, in Vector3 inputNormal,
            bool hasTangent, bool hasNormal)
        {
            if (!hasTangent && !hasNormal)
                return Quaternion.Identity;

            var forward = hasTangent ? inputTangent : Vector3.Forward;
            if (!NormalizeFinite(ref forward))
                forward = Vector3.Forward;

            var up = hasNormal ? inputNormal : Vector3.Up;
            if (!NormalizeFinite(ref up))
                up = Vector3.Up;
            up -= forward * Vector3.Dot(in up, in forward);
            if (!NormalizeFinite(ref up))
            {
                up = Math.Abs(Vector3.Dot(in forward, in Vector3.Up)) < 0.999f ?
                    Vector3.Up : Vector3.Right;
                up -= forward * Vector3.Dot(in up, in forward);
                NormalizeFinite(ref up);
            }

            var right = Vector3.Cross(in up, in forward);
            if (!NormalizeFinite(ref right))
                return Quaternion.Identity;
            up = Vector3.Cross(in forward, in right);
            NormalizeFinite(ref up);

            var matrix = new Matrix(
                right.X, right.Y, right.Z, 0.0f,
                up.X, up.Y, up.Z, 0.0f,
                forward.X, forward.Y, forward.Z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            var result = Quaternion.RotationMatrix(in matrix);
            result.Normalize();
            return result;
        }

        static bool NormalizeFinite(ref Vector3 value)
        {
            float length = value.Normalize();
            return length > 1e-6f && float.IsFinite(value.X) &&
                float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }
    }

    [UnitTest.TtTest]
    public class TtTest_PgcSpawnNode
    {
        public void UnitTestEntrance()
        {
            var rotation = TtSpawnNode.BuildRotation(Vector3.Forward, Vector3.Up, true, true);
            var forward = Quaternion.RotateVector3(rotation, Vector3.Forward);
            var up = Quaternion.RotateVector3(rotation, Vector3.Up);
            UnitTest.TtUnitTestManager.TAssert(Vector3.Distance(forward, Vector3.Forward) <= 1e-4f &&
                Vector3.Distance(up, Vector3.Up) <= 1e-4f,
                "Spawn Node identity orientation is incorrect");

            rotation = TtSpawnNode.BuildRotation(Vector3.Right, Vector3.Up, true, true);
            forward = Quaternion.RotateVector3(rotation, Vector3.Forward);
            UnitTest.TtUnitTestManager.TAssert(Vector3.Distance(forward, Vector3.Right) <= 1e-4f,
                "Spawn Node did not align local forward with tangent");
        }
    }
}
