using System;
using System.Collections.Generic;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
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

        //EngineNS.Graphics.Pipeline.GI.TtTetrahedron
        public TtGpuBuffer<FProbeData>[] ProbeBuffer;
        List<Graphics.Pipeline.GI.TtTetrahedronData> TetrahedronData = new List<Graphics.Pipeline.GI.TtTetrahedronData>();
        public Graphics.Mesh.TtMesh mDebugMesh;
        public TtMeshAtomDesc mAtomDesc = new TtMeshAtomDesc();
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var result = await base.InitializeNode(world, data, bvType, placementType);

            BuildMesh();
            return result;
        }
        int NumOfVertices = 5;
        float Scale = 5.0f;
        int ShowTetrahedronIndex = 0;
        int ShowTetrahedronCount = 1;
        public unsafe void BuildMesh()
        {
            var lst = new List<Vector3>();
            for (int i = 0; i<NumOfVertices; i++)
            {
                var pos = new Vector3();
                var dir = MathHelper.RandomDirection();
                pos = dir * (MathHelper.RandomFloat() * Scale);
                lst.Add(pos);
            }
            var Tetra = new List<FTetrahedron>();
            if (Meshly.TtPointCloud.BuildTetrahedron(lst.ToArray(), Tetra))
            {
                for (int i = 0; i < Tetra.Count; i++)
                {
                    var t = new Graphics.Pipeline.GI.TtTetrahedronData();
                    var v = Tetra[i].m_VertexIndex0;
                    t.Vertices[0] = lst[v];
                    v = Tetra[i].m_VertexIndex1;
                    t.Vertices[1] = lst[v];
                    v = Tetra[i].m_VertexIndex2;
                    t.Vertices[2] = lst[v];
                    v = Tetra[i].m_VertexIndex3;
                    t.Vertices[3] = lst[v];

                    t.Precompute();
                    if (t.Volume > 1e-10f)
                    {
                        TetrahedronData.Add(t);
                    }
                }
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
    }
}
