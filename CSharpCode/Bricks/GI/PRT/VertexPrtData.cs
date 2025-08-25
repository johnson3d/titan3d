using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EngineNS.Bricks.GI.PRT
{
    public unsafe struct FSHCoefficients
    {
        public fixed float Coeffs[9 * 3];//RGB coeffs
    }
    public class TtVertexPrtNode : TtLightWeightNodeBase
    {
        public class TtPrtNodeData : TtNodeData
        {
            public class MeshCoeffsSerializerAttribute : IO.TtCustomSerializerAttribute
            {
                public override unsafe void Save(IO.IWriter ar, object host, string propName)
                {
                    System.Diagnostics.Debug.Assert(propName == "MeshCoeffs");
                    var rbData = host as TtPrtNodeData;
                    //ar.Write(rbData.Vertices.Length);
                    //foreach (var i in rbData.Vertices)
                    //{
                    //    ar.Write(i);
                    //}
                }
                public override unsafe object Load(IO.IReader ar, object host, string propName)
                {
                    System.Diagnostics.Debug.Assert(propName == "MeshCoeffs");
                    //var rbData = host as TtPrtNodeData;
                    //int count = 0;
                    //ar.Read(out count);
                    //rbData.mVertices = new FSHCoefficients[count];
                    //fixed (FSHCoefficients* ptr = &rbData.MeshCoeffs[0])
                    //{
                    //    for (int i = 0; i < count; i++)
                    //    {
                    //        ar.ReadPtr(&ptr[i], sizeof(FSHCoefficients));
                    //    }
                    //}
                    //return rbData.mVertices;
                    return null;
                }
            }
            [MeshCoeffsSerializer]
            [Rtti.Meta("")]
            public List<FSHCoefficients[]> MeshCoeffs { get; set; } = new List<FSHCoefficients[]>();
        }
        public FSHCoefficients[] GetMeshCoeffs(int index)
        {
            return GetNodeData<TtPrtNodeData>().MeshCoeffs[index];
        }
        public TtGpuBuffer<FSHCoefficients>[] VertexCoeffsBuffer;
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            var meshCoeffs = GetNodeData<TtPrtNodeData>().MeshCoeffs;
            VertexCoeffsBuffer = new TtGpuBuffer<FSHCoefficients>[meshCoeffs.Count];
            UpdateGpuBuffer();
            return ret;
        }
        public void ResetPrtData()
        {
            var meshCoeffs = GetNodeData<TtPrtNodeData>().MeshCoeffs;
            meshCoeffs.Clear();
            for (int i = 0; i<HostMeshNode.Mesh.MaterialMesh.SubMeshes.Count; i++)
            {
                var coeffs = new FSHCoefficients[HostMeshNode.Mesh.MaterialMesh.SubMeshes[i].Mesh.VertexNumber];
                meshCoeffs.Add(coeffs);
            }
        }
        public float ComputeBRDFWeight(Vector3 normal, Vector3 lightDir)
        {
            // Lambert
            float ndotl = Vector3.Dot(normal, lightDir);
            return Math.Max(0, ndotl); //只接收半球光照
        }
        public void PrecomputePRT()
        {
            var meshCoeffs = GetNodeData<TtPrtNodeData>().MeshCoeffs;
            for (int i = 0; i<HostMeshNode.Mesh.MaterialMesh.SubMeshes.Count; i++)
            {
                for (int j = 0; j<HostMeshNode.Mesh.MaterialMesh.SubMeshes[i].Mesh.VertexNumber; j++)
                {
                    //Graphics.Pipeline.GI.TtSHCoefficient.PrecomputeSHCoefficients
                    //calc prt for meshCoeffs[i][j] = vis * brdf * abeldoRGB
                }
            }
        }
        public unsafe void UpdateGpuBuffer()
        {
            var meshCoeffs = GetNodeData<TtPrtNodeData>().MeshCoeffs;
            VertexCoeffsBuffer = new TtGpuBuffer<FSHCoefficients>[meshCoeffs.Count];
            for (int i = 0; i<meshCoeffs.Count; i++)
            {
                fixed (FSHCoefficients* ptr = &meshCoeffs[i][0])
                {
                    VertexCoeffsBuffer[i].SetSize((uint)meshCoeffs[i].Length, ptr, NxRHI.EBufferType.BFT_SRV);
                }
            }
        }
        public TtMeshNode HostMeshNode
        {
            get
            {
                return this.Parent as TtMeshNode;
            }
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            if (cur!=null)
            {
                System.Diagnostics.Debug.Assert(cur is TtMeshNode);
            }
            base.OnParentChanged(prev, cur);
        }
    }
}
