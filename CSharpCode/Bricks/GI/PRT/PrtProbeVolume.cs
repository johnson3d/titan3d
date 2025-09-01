using System;
using System.Collections.Generic;
using EngineNS.Bricks.Collision.Embree;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using EngineNS.Support;
using EngineNS.Thread.Async;

namespace EngineNS.Bricks.GI.PRT
{
    public unsafe struct FProbeData
    {
        public Vector3 Position;
        public FSHCoefficients Coeffs;
    }
    [Bricks.CodeBuilder.ContextMenu("PrtProbe", "GI\\PrtProbe", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPrtProbeVolume.TtPrtProbeVolumeData), DefaultNamePrefix = "PPV")]
    public class TtPrtProbeVolume : TtVisual
    {
        public class TtPrtProbeVolumeData : TtNodeData
        {
        }

        public TtCpu2GpuBuffer<FProbeData> ProbeBuffer = new TtCpu2GpuBuffer<FProbeData>();
        public TtCpu2GpuBuffer<FTetrahedron> TetraBuffer = new TtCpu2GpuBuffer<FTetrahedron>();
        List<Graphics.Pipeline.GI.TtTetrahedronData> TetrahedronData = new List<Graphics.Pipeline.GI.TtTetrahedronData>();
        public Graphics.Mesh.TtMesh mDebugMesh;
        public TtMeshAtomDesc mAtomDesc = new TtMeshAtomDesc();
        public Collision.Embree.TtEmbreeManager mEmbreeManager = new Collision.Embree.TtEmbreeManager();
        public Collision.Embree.TtEmbreeScene mEmbreeScene = new Collision.Embree.TtEmbreeScene();
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var result = await base.InitializeNode(world, data, bvType, placementType);

            BuildMesh();
            return result;
        }
        int NumOfVertices = 10;
        float Scale = 5.0f;
        int ShowTetrahedronIndex = 0;
        int ShowTetrahedronCount = 1;
        public unsafe void BuildMesh()
        {
            var lst = new List<Vector3>();
            ProbeBuffer.Initialize(EBufferType.BFT_SRV);
            for (int i = 0; i<NumOfVertices; i++)
            {
                var pos = new Vector3();
                var dir = MathHelper.RandomDirection();
                pos = dir * (MathHelper.RandomFloat() * Scale);
                lst.Add(pos);
                
                var pd = new FProbeData();
                pd.Position = pos;
                ProbeBuffer.PushData(in pd);
            }
            ProbeBuffer.Flush2GPU(null);

            var Tetra = new List<FTetrahedron>();
            TetraBuffer.Initialize(EBufferType.BFT_SRV);
            if (Meshly.TtPointCloud.BuildTetrahedron(lst.ToArray(), Tetra))
            {
                for (int i = 0; i < Tetra.Count; i++)
                {
                    var t = new Graphics.Pipeline.GI.TtTetrahedronData();
                    var v = Tetra[i].m_VertexIndex0;
                    t.ProbeIndices[0] = v;
                    t.Vertices[0] = lst[v];
                    v = Tetra[i].m_VertexIndex1;
                    t.ProbeIndices[1] = v;
                    t.Vertices[1] = lst[v];
                    v = Tetra[i].m_VertexIndex2;
                    t.ProbeIndices[2] = v;
                    t.Vertices[2] = lst[v];
                    v = Tetra[i].m_VertexIndex3;
                    t.ProbeIndices[3] = v;
                    t.Vertices[3] = lst[v];

                    t.Precompute();
                    if (t.Volume > 1e-10f)
                    {
                        TetrahedronData.Add(t);
                        TetraBuffer.PushData(Tetra[i]);
                    }
                }
                TetraBuffer.Flush2GPU(null);
            }
            var mMeshDataProvider = new Graphics.Mesh.TtMeshDataProvider();
            mMeshDataProvider.Init((1 << (int)NxRHI.EVertexStreamType.VST_Position), true, 0);
            var atom = new FMeshAtomDesc();
            atom.SetDefault();
            atom.m_PrimitiveType = EPrimitiveType.EPT_LineList;

            FMeshVertex* pVertices = stackalloc FMeshVertex[4];
            foreach (var i in TetrahedronData)
            {
                for (int j = 0; j < 4; j++)
                {
                    pVertices[j].Position = i.Vertices[j];
                }
                var start = mMeshDataProvider.NumOfVertex;
                mMeshDataProvider.AddVertex(pVertices, 4);

                mMeshDataProvider.AddLine(start + 0, start + 1);
                mMeshDataProvider.AddLine(start + 0, start + 2);
                mMeshDataProvider.AddLine(start + 0, start + 3);
                mMeshDataProvider.AddLine(start + 1, start + 2);
                mMeshDataProvider.AddLine(start + 2, start + 3);
                mMeshDataProvider.AddLine(start + 3, start + 1);
            }
            atom.m_NumPrimitives = mMeshDataProvider.mCoreObject.GetPrimitiveNumber();
            mMeshDataProvider.PushAtom(atom);
            mDebugMesh = mMeshDataProvider.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialManager.NavMeshDebugMaterial);
            var ptr = mAtomDesc.mCoreObject.GetAtomDescPtr();
            ptr->PrimitiveType = EPrimitiveType.EPT_LineList;
            ShowTetrahedronIndex = 0;
            ShowTetrahedronCount = TetrahedronData.Count;
            ptr->m_NumPrimitives = 6 * (uint)ShowTetrahedronCount;
            mDebugMesh.OnBuildDrawcall = (drawcall) =>
            {
                drawcall.mCoreObject.AtomDesc = mAtomDesc.mCoreObject;
            };
        }
        public unsafe override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            var ptr = mAtomDesc.mCoreObject.GetAtomDescPtr();
            ptr->m_StartIndex = 6 * 2 * (uint)ShowTetrahedronIndex;
            ptr->m_NumPrimitives = 6 * (uint)ShowTetrahedronCount; ;
            if (mDebugMesh != null)
            {
                rp.AddVisibleMesh(mDebugMesh);
            }
        }

        class TtGeometryUserData
        {
            public Graphics.Mesh.TtMaterialMesh.TtSubMaterialedMesh Mesh;
            public TtEmbreeGeometry Geometry;
            public TtBlobObject FaceBuffer;
        }
        private unsafe void BuildProbe()
        {
            Dictionary<IntPtr, TtGeometryUserData> meshUserBuffers = new Dictionary<IntPtr, TtGeometryUserData>();
            this.GetWorld().Root.IterateNodes((node, arg) =>
            {
                var meshNode = node as TtMeshNode;
                if (meshNode != null && meshNode.Mesh != null)
                {
                    var mesh = meshNode.Mesh;
                    Graphics.Mesh.TtMaterialMesh.TtSubMaterialedMesh subMesh = mesh.MaterialMesh.SubMeshes[0];
                    var meshdata = subMesh.Mesh;
                    var key = meshdata.MeshDataProvider.mCoreObject.NativePointer;
                    TtGeometryUserData geometryUserData;
                    if (meshUserBuffers.TryGetValue(key, out geometryUserData) == false)
                    {
                        geometryUserData = new TtGeometryUserData();
                        geometryUserData.Mesh = subMesh;
                        var faceData = meshdata.BuildFaceDataWithMaterialIds();
                        geometryUserData.FaceBuffer = faceData;
                        geometryUserData.Geometry = mEmbreeManager.CreateGeometry(meshdata.AssetName.Name, meshdata.MeshDataProvider);
                        meshUserBuffers.Add(key, geometryUserData);
                    }
                    var geomInst = mEmbreeManager.CreateGeometryInstance(geometryUserData.Geometry);
                    geomInst.SetTransform(meshNode.Placement.AbsTransform.ToMatrixWithScale(this.Placement.Position));
                    mEmbreeScene.AttachGeometryInstance(geomInst);
                }
                return true;
            }, null);
            mEmbreeScene.CommitScene();

            foreach (var i in ProbeBuffer.DataArray)
            {
                FHitResult hit = new FHitResult();
                if (mEmbreeScene.EmbreeRayTrace(i.Position, Vector3.UnitX, ref hit)==false)
                    continue;
                if (meshUserBuffers.TryGetValue(hit.m_Geometry->GetMeshProvider().NativePointer, out var geometryUserData))
                {
                    var ptr = (int*)geometryUserData.FaceBuffer.DataPointer;
                    var materialId = ptr[hit.m_PrimID];
                    var mtl = geometryUserData.Mesh.Materials[materialId];
                    //take albedo from mtl;
                }
                else
                {
                    var sky = Graphics.Pipeline.GI.FCubemapResult.DirectionToCubemap(Vector3.UnitX);
                    //take albedo from skybox
                }
                //
                //Graphics.Pipeline.GI.TtSHCoefficient.PrecomputeSHCoefficients
                //i.Coeffs = PrecomputeSHCoefficients(i.Position);
            }
        }
    }
}
