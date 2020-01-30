using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GpuDriven.Cluster
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct GpuSceneVertex
    {
        public Vector3 Position;
        public float DiffuseU;
        public Vector3 Normal;
        public float DiffuseV;
        public Vector4 Tangent;

        public void Reset()
        {
            Normal.SetValue(0, 0, 1);
            Tangent.SetValue(0, 1, 0, 0);
        }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GpuCluster
    {
        public enum ECubeFace : byte
        {
            CubeFace_X = 0,
            CubeFace_Y = 1,
            CubeFace_Z = 2,
            CubeFace_NX = 3,
            CubeFace_NY = 4,
            CubeFace_NZ = 5,
            CubeFace_Num = 6,
        }

        public Vector3 BoundCenter;
        public uint FaceCount;

        public Vector3 BoundExtent;
        public uint StartFaceIndex;
        
        public fixed UInt64 CubeFaces[6];

        public uint InstanceId;
        uint Pad0;
        uint Pad1;
        uint Pad2;

        public void Reset()
        {
            CubeFaces[0] = 0;
            CubeFaces[1] = 0;
            CubeFaces[2] = 0;
            CubeFaces[3] = 0;
            CubeFaces[4] = 0;
            CubeFaces[5] = 0;
        }
    }

    public class ClusteredMesh : IResource
    {
        public class Cluster
        {
            public GpuCluster Data;
            public unsafe void Proccess(Vector3* pPosPtr, List<uint> IndexBuffer)
            {
                Data.Reset();
                BoundingBox box = new BoundingBox();
                box.InitEmptyBox();
                for (int i = 0; i < Data.FaceCount; i++)
                {
                    int faceIndex = (int)Data.StartFaceIndex + i;
                    var a = pPosPtr[IndexBuffer[faceIndex * 3 + 0]];
                    var b = pPosPtr[IndexBuffer[faceIndex * 3 + 1]];
                    var c = pPosPtr[IndexBuffer[faceIndex * 3 + 2]];

                    box.Merge(ref a);
                    box.Merge(ref b);
                    box.Merge(ref c);

                    var nor = Vector3.CalcFaceNormal(ref a, ref b, ref c);
                    if( Math.Abs(nor.X)<0.001f )
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_X] |= (((ulong)1) << i);
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NX] |= (((ulong)1) << i);
                    }
                    else if(nor.X > 0)
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_X] |= (((ulong)1) << i);
                    }
                    else
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NX] |= (((ulong)1) << i);
                    }

                    if (Math.Abs(nor.Y) < 0.001f)
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_Y] |= (((ulong)1) << i);
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NY] |= (((ulong)1) << i);
                    }
                    else if (nor.Y > 0)
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_Y] |= (((ulong)1) << i);
                    }
                    else
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NY] |= (((ulong)1) << i);
                    }

                    if (Math.Abs(nor.Y) < 0.001f)
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_Z] |= (((ulong)1) << i);
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NZ] |= (((ulong)1) << i);
                    }
                    else if (nor.Z > 0)
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_Z] |= (((ulong)1) << i);
                    }
                    else
                    {
                        Data.CubeFaces[(int)GpuCluster.ECubeFace.CubeFace_NZ] |= (((ulong)1) << i);
                    }
                }

                Data.BoundCenter = box.GetCenter();
                Data.BoundExtent = box.GetSize() * 0.5f;
            }
        }
        public RName Name
        {
            get;
            set;
        }
        public List<GpuSceneVertex> MeshVertices
        {
            get;
        } = new List<GpuSceneVertex>();
        public List<uint> IndexBuffer
        {
            get;
        } = new List<uint>();
        public List<GpuCluster> ClusterDatas
        {
            get;
        }= new List<GpuCluster>();

        public CResourceState ResourceState
        {
            get;
        } = CResourceState.CreateResourceState();

        ~ClusteredMesh()
        {
            Cleanup();

            CResourceState.DestroyResourceState(ResourceState);
        }
        public void Cleanup()
        {
            MeshVertices.Clear();
            IndexBuffer.Clear();
            ClusterDatas.Clear();

            ResourceState.ResourceSize = 0;
        }
        
        public bool LoadClusteredMesh(RName name)
        {
            Name = name;

            Cleanup();

            var xnd = IO.XndHolder.SyncLoadXND(name.Address);
            IO.XndNode node = xnd.Node;

            var attr = node.FindAttrib("Vertices");
            if (attr == null)
                return false;
            attr.BeginRead();
            int count = 0;
            attr.Read(out count);
            for (int i = 0; i < count; i++)
            {
                GpuSceneVertex vert;
                attr.Read(out vert);
                MeshVertices.Add(vert);
            }
            attr.EndRead();

            attr = node.FindAttrib("ClusterDatas");
            if (attr == null)
                return false;
            attr.BeginRead();
            attr.Read(out count);
            for (int i = 0; i < count; i++)
            {
                GpuCluster cluster;
                attr.Read(out cluster);
                ClusterDatas.Add(cluster);
            }
            attr.EndRead();

            attr = node.FindAttrib("IndexBuffer");
            attr.BeginRead();
            attr.Read(out count);
            for (int i = 0; i < count; i++)
            {
                uint index;
                attr.Read(out index);
                IndexBuffer.Add(index);
            }
            attr.EndRead();

            unsafe
            {
                ResourceState.ResourceSize = (uint)(MeshVertices.Count * sizeof(GpuSceneVertex) + ClusterDatas.Count * sizeof(GpuCluster) + IndexBuffer.Count * sizeof(uint));
            }

            return true;
        }

        public void SaveClusteredMesh(RName name)
        {
            var xnd = IO.XndHolder.NewXNDHolder();

            IO.XndNode node = xnd.Node;
            var attr = node.AddAttrib("Vertices");
            attr.BeginWrite();
            attr.Write(MeshVertices.Count);
            for (int i = 0; i < MeshVertices.Count; i++)
            {
                attr.Write(MeshVertices[i]);
            }
            attr.EndWrite();

            attr = node.AddAttrib("ClusterDatas");
            attr.BeginWrite();
            attr.Write(ClusterDatas.Count);
            for (int i = 0; i < ClusterDatas.Count; i++)
            {
                attr.Write(ClusterDatas[i]);
            }
            attr.EndWrite();

            attr = node.AddAttrib("IndexBuffer");
            attr.BeginWrite();
            attr.Write(IndexBuffer.Count);
            for (int i = 0; i < IndexBuffer.Count; i++)
            {
                attr.Write(IndexBuffer[i]);
            }
            attr.EndWrite();

            IO.XndHolder.SaveXND(name.Address, xnd);
        }

        public unsafe bool BuildClusterFromMeshSource(CRenderContext rc, RName meshSourceName)
        {
            Graphics.Mesh.CGfxMeshDataProvider meshSource = new Graphics.Mesh.CGfxMeshDataProvider();
            var meshPrimitive = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, meshSourceName, true);
            meshSource.InitFromMesh(rc, meshPrimitive);

            uint a, b, c;
            a = 0;
            b = 0;
            c = 0;
            List<Cluster> clusterArray = new List<Cluster>();
            int vertNum = meshSource.VertexNumber;

            Vector3* pPos = (Vector3*)meshSource.GetVertexPtr(EVertexSteamType.VST_Position, 0).ToPointer();
            Vector3* pNor = (Vector3*)meshSource.GetVertexPtr(EVertexSteamType.VST_Normal, 0).ToPointer();
            Vector4* pTan = (Vector4*)meshSource.GetVertexPtr(EVertexSteamType.VST_Tangent, 0).ToPointer();
            Vector2* pDiffUV = (Vector2*)meshSource.GetVertexPtr(EVertexSteamType.VST_UV, 0).ToPointer();

            GpuSceneVertex vert = new GpuSceneVertex();
            for (int i = 0; i < vertNum; i++)
            {
                vert.Reset();
                if (pPos != null)
                {
                    vert.Position = pPos[i];
                }
                if (pNor != null)
                {
                    vert.Normal = pNor[i];
                }
                if (pTan != null)
                {
                    vert.Tangent = pTan[i];
                }
                if (pDiffUV != null)
                {
                    vert.DiffuseU = pDiffUV[i].X;
                    vert.DiffuseV = pDiffUV[i].Y;
                }

                MeshVertices.Add(vert);
            }

            for (int i = 0; i < meshSource.TriangleNumber; i++)
            {
                meshSource.GetTriangle(i, ref a, ref b, ref c);
                IndexBuffer.Add(a);
                IndexBuffer.Add(b);
                IndexBuffer.Add(c);
            }

            Cluster curCluster = null;
            Vector3* pPosPtr = (Vector3*)meshSource.GetVertexPtr(EVertexSteamType.VST_Position, 0).ToPointer();
            for (uint i = 0; i < meshSource.AtomNumber; i++)
            {
                CDrawPrimitiveDesc desc = meshSource[i, 0];

                if (desc.PrimitiveType != EPrimitiveType.EPT_TriangleList)
                    return false;

                uint startFace = desc.StartIndex / 3;

                for (uint j = 0; j < desc.NumPrimitives; j++)
                {
                    if (curCluster == null || curCluster.Data.FaceCount >= 64)
                    {
                        curCluster = new Cluster();
                        curCluster.Data.InstanceId = i;
                        curCluster.Data.StartFaceIndex = startFace;
                        curCluster.Data.FaceCount = 0;

                        clusterArray.Add(curCluster);
                    }
                    startFace++;
                    curCluster.Data.FaceCount++;
                }
            }

            ClusterDatas.Clear();
            for (int i = 0; i < clusterArray.Count; i++)
            {
                clusterArray[i].Proccess(pPosPtr, IndexBuffer);

                ClusterDatas.Add(clusterArray[i].Data);
            }
            return true;
        }
    }

    public class ClusteredMeshManager : CResourceManager<ClusteredMesh>
    {
        public override ClusteredMesh GetResource(CRenderContext rc, RName name, bool firstLoad = false)
        {
            var result = new ClusteredMesh();
            result.LoadClusteredMesh(name);
            result.ResourceState.StreamState = EStreamingState.SS_Valid;
            Resources.Add(name, result);

            return result;
        }
        protected override bool CanRemove(ClusteredMesh res) 
        {
            return false;
        }
        protected override void OnRemove(ClusteredMesh res)
        {

        }
        protected override void InvalidateResource(ClusteredMesh res)
        {
            res.Cleanup();
        }
        protected override void RestoreResource(ClusteredMesh res)
        {
            res.LoadClusteredMesh(res.Name);
        }
        protected override void OnStreamingTick(ClusteredMesh res)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        public EngineNS.Bricks.GpuDriven.Cluster.ClusteredMeshManager ClusteredMeshManager
        {
            get;
        } = new Bricks.GpuDriven.Cluster.ClusteredMeshManager();
    }
}