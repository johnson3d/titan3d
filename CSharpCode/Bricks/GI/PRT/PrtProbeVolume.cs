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
        List<Graphics.Pipeline.GI.TtDelaunayTetrahedralization.TetrahedronData> TetrahedronData = null;
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
            TetrahedronData = Graphics.Pipeline.GI.TtDelaunayTetrahedralization.ComputeDelaunayTetrahedralization(lst);
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
            ptr->m_StartIndex = 0;
            ptr->m_NumPrimitives = 6;
            //mDebugMesh.OnBuildDrawcall = (drawcall) =>
            //{
            //    drawcall.mCoreObject.AtomDesc = mAtomDesc.mCoreObject;
            //};
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if (mDebugMesh != null)
            {
                rp.AddVisibleMesh(mDebugMesh);
            }
        }
    }
}
