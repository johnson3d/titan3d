using Assimp;
using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using EngineNS.Support;
//using Microsoft.Toolkit.HighPerformance.Buffers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EngineNS.Bricks.GpuDriven
{

    //public struct TtClusteDrawArgs
    //{
    //    public Int32 PrimitiveId;
    //    public UInt32 MaxInstance;
    //    public NxRHI.FBufferDesc IndirectArgsBuffer;
    //    public NxRHI.FBufferDesc IndirectCountBuffer;
    //}

    public struct TtClusterIndexInfo
    {
        public UInt64 GPUAddress;
        public UInt32 IndexSizeInBytes;
        public UInt32 Format;
        public UInt32 IndexCountPerInstance;
        public UInt32 padding; //TODO
    }

    public struct TtVisibleInstance
    {
        public uint NumVisibleInstance;
        public uint TotalClusterCount;
    };

    //TODO.. for CS shader
    public struct TtDrawIndexedIndirectParameters
    {
        public uint IndexCountPerInstance;
        public uint InstanceCount;
        public uint StartIndexLocation;
        public int BaseVertexLocation;
        public uint StartInstanceLocation;
    };

    public class TtCluster
    {
        public int VertexStart;
        public int VertexCount;
        public int IndexStart;
        public int IndexCount;

        public BoundingBox AABB;
    };
    public struct FQuarkVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
    };
    public class TtClusteredMesh
    {
        public bool InitFromMesh(Graphics.Mesh.TtMeshPrimitives mesh)
        {
            Mesh = mesh;
            return true;
        }
        public void SaveClusteredMesh(RName meshName)
        {
            var file = meshName.Address + ".clustermesh";
            var xnd = new IO.TtXndHolder("Cluster", 0, 0);

            var rc = TtEngine.Instance?.GfxDevice.RenderContext;
            var count = Mesh.mCoreObject.ClusterizeTriangles(rc.mCoreObject);
            if (count > 0 && Mesh.mCoreObject.SaveClusters(xnd.RootNode.mCoreObject))
            {
                xnd.SaveXnd(file);
            }
            else
            {
                // error
            }            
        }
        public static TtClusteredMesh LoadClusteredMesh(RName meshName, Graphics.Mesh.TtMeshPrimitives mesh)
        {
            var file = meshName.Address + ".clustermesh";
            var xnd = IO.TtXndHolder.LoadXnd(file);
            var result = new TtClusteredMesh();

            // load vb, ib
            var rc = TtEngine.Instance?.GfxDevice.RenderContext;
            int count = mesh.mCoreObject.LoadClusters(xnd.mCoreObject, rc.mCoreObject);
            for (int i = 0; i < count; i++)
            {
                TtCluster cluster = new TtCluster();
                var cppCluster = mesh.mCoreObject.GetCluster(i);
                cluster.VertexStart = cppCluster.VertexStart;
                cluster.VertexCount = cppCluster.VertexCount;
                cluster.IndexStart = cppCluster.IndexStart;
                cluster.IndexCount = cppCluster.IndexCount;
                cluster.AABB = cppCluster.Bounds;

                result.Clusters.Add(cluster);
            }

            var vbCount = mesh.mCoreObject.GetClustersVBCount();
            var ibCount = mesh.mCoreObject.GetClustersIBCount();            
            unsafe
            {
                result.Vertices = new Vector3[vbCount];
                EngineNS.Vector3* vb = mesh.mCoreObject.GetClustersVB();
                
                fixed (Vector3* dest = &result.Vertices[0])
                {
                    CoreSDK.MemoryCopy(dest, vb, vbCount * (uint)sizeof(Vector3));
                }

                result.Indices = new uint[ibCount];
                uint* ib = mesh.mCoreObject.GetClustersIB();
                fixed (uint* dest = &result.Indices[0])
                {
                    CoreSDK.MemoryCopy(dest, ib, ibCount * (uint)sizeof(uint));
                }
            }
            
            return result;
        }
       
        public NxRHI.TtVertexArray ClusterVertexArray;
        public NxRHI.TtIbView ClusterIndexView;
        public int NumVertices;
        public int NumIndices;
        public BoundingBox AABB = new BoundingBox();

        public List<TtCluster> Clusters = new List<TtCluster>();
        public Vector3[] Vertices = null;
        public uint[] Indices = null;

        private Graphics.Mesh.TtMeshPrimitives Mesh;
        public static unsafe TtClusteredMesh Merge(List<TtClusteredMesh> meshes, NxRHI.TtCommandList cmdlist)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var result = new TtClusteredMesh();

            uint Streams = 0;
            int TotalVertices = 0;
            int TotalIndices = 0;
            var cpVbDrawcalls = new List<KeyValuePair<NxRHI.EVertexStreamType, NxRHI.TtCopyDraw>>();
            var cpIbDrawcalls = new List<NxRHI.TtCopyDraw>();
            NxRHI.FSubResourceFootPrint footPrint = new NxRHI.FSubResourceFootPrint();
            foreach (var mesh in meshes)
            {
                for (NxRHI.EVertexStreamType i = 0; i < NxRHI.EVertexStreamType.VST_Number; i++)
                {
                    var vb = mesh.ClusterVertexArray.mCoreObject.GetVB(i);
                    if (vb.IsValidPointer)
                    {
                        Streams |= (1u << (int)i);
                        var drawcall = rc.CreateCopyDraw();
                        drawcall.mCoreObject.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                        drawcall.mCoreObject.BindBufferSrc(vb.Buffer);
                        uint stride = 0;
                        NxRHI.FVertexArray.GetStreamInfo(i, &stride, (uint*)IntPtr.Zero.ToPointer(), (int*)IntPtr.Zero.ToPointer());
                        drawcall.mCoreObject.DstX = (uint)TotalVertices * stride;
                        footPrint.SetDefault();
                        footPrint.Width = (uint)mesh.NumVertices * stride;
                        drawcall.mCoreObject.FootPrint = footPrint;

                        var t = new KeyValuePair<NxRHI.EVertexStreamType, NxRHI.TtCopyDraw>(i, drawcall);
                        cpVbDrawcalls.Add(t);
                    }
                }
                //copy IndexBuffer
                {
                    var drawcall = rc.CreateCopyDraw();
                    drawcall.mCoreObject.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                    drawcall.mCoreObject.BindBufferSrc(mesh.ClusterIndexView.mCoreObject.Buffer);
                    drawcall.mCoreObject.DstX = (uint)TotalIndices * sizeof(uint);
                    footPrint.SetDefault();
                    footPrint.Width = (uint)mesh.NumIndices * sizeof(uint);
                    drawcall.mCoreObject.FootPrint = footPrint;

                    cpIbDrawcalls.Add(drawcall);
                }
                foreach (var cluster in mesh.Clusters)
                {
                    cluster.VertexStart += TotalVertices;
                    cluster.IndexStart += TotalIndices;
                }
                TotalVertices += mesh.NumVertices;
                TotalIndices += mesh.NumIndices;
            }
            
            result.ClusterVertexArray = rc.CreateVertexArray();
            for (NxRHI.EVertexStreamType i = 0; i < NxRHI.EVertexStreamType.VST_Number; i++)
            {
                if ((Streams & (1u << (int)i)) != 0)
                {
                    var bfDesc = new NxRHI.FBufferDesc();
                    bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_Vertex);
                    var buffer = rc.CreateBuffer(in bfDesc);

                    var vbvDesc = new NxRHI.FVbvDesc();
                    vbvDesc.SetDefault();
                    var vb = rc.CreateVBV(buffer, in vbvDesc);
                    result.ClusterVertexArray.mCoreObject.BindVB(i, vb.mCoreObject);
                }
            }
            {
                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_Index);
                var buffer = rc.CreateBuffer(in bfDesc);
                var ibvDesc = new NxRHI.FIbvDesc();
                ibvDesc.SetDefault();
                result.ClusterIndexView = rc.CreateIBV(buffer, ibvDesc);
            }

            result.NumVertices = TotalVertices;
            result.NumIndices = TotalIndices;

            foreach (var i in cpVbDrawcalls)
            {
                var vb = result.ClusterVertexArray.mCoreObject.GetVB(i.Key);
                i.Value.mCoreObject.BindBufferDest(vb.Buffer);

                cmdlist.PushGpuDraw(i.Value);
                //i.Value.mCoreObject.Commit(cmdlist.mCoreObject);
                i.Value.Dispose();
            }

            foreach (var i in cpIbDrawcalls)
            {
                var ib = result.ClusterIndexView.mCoreObject;
                i.mCoreObject.BindBufferDest(ib.Buffer);
                cmdlist.PushGpuDraw(i);
                //i.mCoreObject.Commit(cmdlist.mCoreObject);
                i.Dispose();
            }

            //notice:PushGpuDraw replace drawcall.Commit, user need cmdlist.FlushDraws at EndPass
            //cmdlist.FlushDraws();
            return result;
        }
    }

    public class TtClusteredMeshManager
    {
        public Dictionary<RName, TtClusteredMesh> ClusteredMeshes { get; } = new Dictionary<RName, TtClusteredMesh>();
        public async System.Threading.Tasks.Task Initialize()
        {
            return;
        }
        public async Thread.Async.TtTask<TtClusteredMesh> GetClusteredMesh(RName name, Graphics.Mesh.TtMeshPrimitives mesh)
        {
            TtClusteredMesh result;
            if (ClusteredMeshes.TryGetValue(name, out result))
                return result;

            if (IO.TtFileManager.FileExists(name.Address + ".clustermesh") == false)
            {
                return null;
            }

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return TtClusteredMesh.LoadClusteredMesh(name, mesh);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                ClusteredMeshes[name] = result;
                return result;
            }

            return null;
        }
    }

    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FMeshlet")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FMeshlet
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VertexOffset")]
        public uint VertexOffset;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "TriangleOffset")]
        public uint TriangleOffset;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "VertexCount")]
        public uint VertexCount;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "TriangleCount")]
        public uint TriangleCount;
    }
    public class TtMeshlets : IDisposable
    {
        public TtGpuBuffer<FMeshlet> MeshLetsBuffer;
        public TtGpuBuffer<uint> VerticesBuffer;
        public TtGpuBuffer<uint> TrianglesBuffer;
        public TtGeomMesh GeomMesh = null;
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref MeshLetsBuffer);
            CoreSDK.DisposeObject(ref VerticesBuffer);
            CoreSDK.DisposeObject(ref TrianglesBuffer);
        }
        public unsafe void Init()
        {
            if (GeomMesh != null)
                return;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            GeomMesh = rc.CreateGeomMesh();

            FIbvDesc ivd = new FIbvDesc();
            ivd.SetDefault();
            ivd.Stride = sizeof(ushort);
            ivd.Size = sizeof(ushort) * 256 * 3;
            var pIBData = (ushort*)CoreSDK.Alloc(sizeof(ushort) * 256 * 3, null, 0);
            for (int i = 0; i < 256 * 3; i++)
            {
                pIBData[i] = (ushort)i;
            }
            var initData = new NxRHI.FMappedSubResource();
            initData.m_pData = pIBData;
            initData.m_RowPitch = ivd.Size;

            ivd.InitData = &initData;
            var ib = rc.CreateIBV(null, in ivd);
            CoreSDK.Free(pIBData);
            GeomMesh.BindIndexBuffer(ib);
           
        }
        public unsafe void BuildMeshlets(NxRHI.FMeshDataProvider mesh, uint max_vertices, uint max_triangles, float cone_weight)
        {
            Init();

            using (var Meshlets = new Support.TtBlobObject())
            using (var Materials = new Support.TtBlobObject())
            using (var Vertices = new Support.TtBlobObject())
            using (var Triangles = new Support.TtBlobObject())
            {
                var numOfMeshlets = IMeshOptimizer.BuildMeshlets(Meshlets.mCoreObject,
                    Materials.mCoreObject,
                    Vertices.mCoreObject,
                    Triangles.mCoreObject,
                    mesh, max_vertices, max_triangles, cone_weight);

                var pPos = (Vector3*)mesh.GetVertexPtr(NxRHI.EVertexStreamType.VST_Position, 0);
                FMeshlet* pMeshlets = (FMeshlet*)Meshlets.DataPointer;
                uint* pVertices = (uint*)Vertices.DataPointer;
                byte* pTriangles = (byte*)Triangles.DataPointer;
                uint maxVertex = 0;
                uint maxIndex = 0;
                uint MaxTri = 0;
                uint totalTri = 0;
                for (uint i = 0; i < numOfMeshlets; i++)
                {
                    maxVertex = MathHelper.Max(maxVertex, pMeshlets[i].VertexOffset + pMeshlets[i].VertexCount);
                    maxIndex = MathHelper.Max(maxIndex, pMeshlets[i].TriangleOffset + pMeshlets[i].TriangleCount);
                    MaxTri = MathHelper.Max(MaxTri, pMeshlets[i].TriangleCount);
                    totalTri += pMeshlets[i].TriangleCount;
                    //System.Diagnostics.Debug.Assert(pMeshlets[i].TriangleCount % 3 == 0);
                    for (uint j = 0; j < pMeshlets[i].TriangleCount; j++)
                    {
                        //这里可以用一个byte4作为一个triangle face，3个字节作为顶点索引，第四个作为材质id使用，一个mesh内部不超过256个材质是可以接受的 
                        var a = pTriangles[pMeshlets[i].TriangleOffset + j * 3 + 0];
                        var b = pTriangles[pMeshlets[i].TriangleOffset + j * 3 + 1];
                        var c = pTriangles[pMeshlets[i].TriangleOffset + j * 3 + 2];

                        var v_idx = pVertices[pMeshlets[i].VertexOffset + a];
                        System.Diagnostics.Debug.Assert(v_idx < mesh.VertexNumber);
                        var va = pPos[v_idx];

                        v_idx = pVertices[pMeshlets[i].VertexOffset + b];
                        System.Diagnostics.Debug.Assert(v_idx < mesh.VertexNumber);
                        var vb = pPos[v_idx];

                        v_idx = pVertices[pMeshlets[i].VertexOffset + c];
                        System.Diagnostics.Debug.Assert(v_idx < mesh.VertexNumber);
                        var vc = pPos[v_idx];
                    }
                }
                uint variance = 0;
                uint expect = totalTri / numOfMeshlets;
                for (uint i = 0; i < numOfMeshlets; i++)
                {
                    variance += (pMeshlets[i].TriangleCount - expect) * (pMeshlets[i].TriangleCount - expect);
                }
                variance /= numOfMeshlets;
                MeshLetsBuffer = new TtGpuBuffer<FMeshlet>();
                MeshLetsBuffer.SetSize(numOfMeshlets, pMeshlets, NxRHI.EBufferType.BFT_SRV);
                VerticesBuffer = new TtGpuBuffer<uint>();
                VerticesBuffer.SetSize(maxVertex, pVertices, NxRHI.EBufferType.BFT_SRV);
                TrianglesBuffer = new TtGpuBuffer<uint>();
                TrianglesBuffer.SetSize(maxIndex / 4 + 1, pTriangles, NxRHI.EBufferType.BFT_SRV);
            }
        }
        public unsafe void LoadXnd(XndNode node)
        {
            Init();

            CoreSDK.DisposeObject(ref MeshLetsBuffer);
            CoreSDK.DisposeObject(ref VerticesBuffer);
            CoreSDK.DisposeObject(ref TrianglesBuffer);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var attr = node.TryGetAttribute("MeshLets");
            if (attr.IsValidPointer)
            {
                using (var ar = attr.GetReader(null))
                {
                    var ptr = CoreSDK.Alloc((uint)attr.GetReaderLength(), null, 0);
                    ar.ReadPtr(ptr, (int)attr.GetReaderLength());
                    MeshLetsBuffer = new TtGpuBuffer<FMeshlet>();
                    MeshLetsBuffer.SetSize((uint)((int)attr.GetReaderLength() / sizeof(FMeshlet)), ptr, NxRHI.EBufferType.BFT_SRV);
                    CoreSDK.Free(ptr);
                }
            }
            attr = node.TryGetAttribute("Vertices");
            if (attr.IsValidPointer)
            {
                using (var ar = attr.GetReader(null))
                {
                    var ptr = CoreSDK.Alloc((uint)attr.GetReaderLength(), null, 0);
                    ar.ReadPtr(ptr, (int)attr.GetReaderLength());
                    VerticesBuffer = new TtGpuBuffer<uint>();
                    VerticesBuffer.SetSize((uint)((int)attr.GetReaderLength() / sizeof(uint)), ptr, NxRHI.EBufferType.BFT_SRV);
                    CoreSDK.Free(ptr);
                }
            }
            attr = node.TryGetAttribute("Triangles");
            if (attr.IsValidPointer)
            {
                using (var ar = attr.GetReader(null))
                {
                    var ptr = CoreSDK.Alloc((uint)attr.GetReaderLength(), null, 0);
                    ar.ReadPtr(ptr, (int)attr.GetReaderLength());
                    TrianglesBuffer = new TtGpuBuffer<uint>();
                    TrianglesBuffer.SetSize((uint)((int)attr.GetReaderLength() / sizeof(uint)), ptr, NxRHI.EBufferType.BFT_SRV);
                    CoreSDK.Free(ptr);
                }
            }
        }
        public unsafe void SaveXnd(XndNode node)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var attr = node.GetOrAddAttribute("MeshLets", 0, 0, true);
            
            using (var blob = new Support.TtBlobObject())
            {
                //var desc = MeshLetsBuffer.GpuBuffer.mCoreObject.Desc;
                //desc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
                //desc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
                //var cpBuffer = rc.CreateBuffer(in desc);
                //var cpDraw = rc.CreateCopyDraw();
                //cpDraw.Copy(cpBuffer, MeshLetsBuffer.GpuBuffer);
                //using (var cmd = new FTransientCmd(EQueueType.QU_Transfer, ""))
                //{
                //    cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                //}
                //cpDraw.Dispose();
                //cpBuffer.FetchGpuData(0, blob.mCoreObject);
                MeshLetsBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);
                using (var ar = attr.GetWriter(blob.Size))
                {
                    var p = (FMeshlet*)((byte*)blob.DataPointer + 8);
                    ar.WritePtr(p, (int)blob.Size - 8);
                }
                //cpBuffer.Dispose();
            }
            attr = node.GetOrAddAttribute("Vertices", 0, 0, true);
            using (var blob = new Support.TtBlobObject())
            {
                //var desc = VerticesBuffer.GpuBuffer.mCoreObject.Desc;
                //desc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
                //desc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
                //var cpBuffer = rc.CreateBuffer(in desc);
                //var cpDraw = rc.CreateCopyDraw();
                //cpDraw.Copy(cpBuffer, VerticesBuffer.GpuBuffer);
                //using (var cmd = new FTransientCmd(EQueueType.QU_Transfer, ""))
                //{   
                //    cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                //}
                //cpDraw.Dispose();
                //cpBuffer.FetchGpuData(0, blob.mCoreObject);
                VerticesBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);
                using (var ar = attr.GetWriter(blob.Size))
                {
                    var p = (uint*)((byte*)blob.DataPointer + 8);
                    ar.WritePtr(p, (int)blob.Size - 8);
                }
                //cpBuffer.Dispose();
            }
            attr = node.GetOrAddAttribute("Triangles", 0, 0, true);
            using (var blob = new Support.TtBlobObject())
            {
                //var desc = TrianglesBuffer.GpuBuffer.mCoreObject.Desc;
                //desc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
                //desc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
                //var cpBuffer = rc.CreateBuffer(in desc);
                //var cpDraw = rc.CreateCopyDraw();
                //cpDraw.Copy(cpBuffer, TrianglesBuffer.GpuBuffer);
                //using (var cmd = new FTransientCmd(EQueueType.QU_Transfer, ""))
                //{
                //    cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                //}
                //cpDraw.Dispose();
                //cpBuffer.FetchGpuData(0, blob.mCoreObject);
                TrianglesBuffer.GpuBuffer.FetchGpuData(0, blob.mCoreObject);
                using (var ar = attr.GetWriter(blob.Size))
                {
                    var p = (uint*)((byte*)blob.DataPointer + 8);
                    ar.WritePtr(p, (int)blob.Size - 8);
                }
                //cpBuffer.Dispose();
            }
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public partial class TtMeshPrimitives
    {
        Bricks.GpuDriven.TtClusteredMesh mClusteredMesh;
        public Bricks.GpuDriven.TtClusteredMesh ClusteredMesh { get => mClusteredMesh; }
        public async Thread.Async.TtTask<Bricks.GpuDriven.TtClusteredMesh> TryLoadClusteredMesh(bool bForce = false)
        {
            if (mClusteredMesh == null || bForce)
            {
                var meshMeta = GetAMeta() as TtMeshPrimitivesAMeta;
                if (meshMeta == null || meshMeta.IsClustered == false)
                    return null;
                mClusteredMesh = await TtEngine.Instance.GfxDevice.ClusteredMeshManager.GetClusteredMesh(AssetName, this);
            }
            return mClusteredMesh;
        }
        public Bricks.GpuDriven.TtClusteredMesh BuildClusteredMesh()
        {
            mClusteredMesh = new Bricks.GpuDriven.TtClusteredMesh();
            mClusteredMesh.InitFromMesh(this);
            mClusteredMesh.SaveClusteredMesh(this.AssetName);
            
            return mClusteredMesh;
        }
        public bool LoadClusterMesh()
        {
            mClusteredMesh = Bricks.GpuDriven.TtClusteredMesh.LoadClusteredMesh(this.AssetName, this);
            
            return mClusteredMesh != null;
        }

        #region Meshlets
        //bool HasMeshLets = false;
        Bricks.GpuDriven.TtMeshlets mMeshlets;
        public Bricks.GpuDriven.TtMeshlets Meshlets
        {
            get
            {
                return mMeshlets;
            }
        }
        public void BuildMeshlets()
        {
            CoreSDK.DisposeObject(ref mMeshlets);
            mMeshlets = new Bricks.GpuDriven.TtMeshlets();
            var mesh = NxRHI.FMeshDataProvider.CreateInstance();
            mesh.InitFromMesh(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, mCoreObject);
            mesh.ConvertToIndex32();
            mMeshlets.BuildMeshlets(mesh, 128, 256, 0);
            CoreSDK.PtrType_Release(mesh);
        }
        public unsafe void LoadMeshlets(XndNode node)
        {
            mMeshlets = new Bricks.GpuDriven.TtMeshlets();
            mMeshlets.LoadXnd(node);

            var vb = mCoreObject.GetGeomtryMesh().GetVertexArray().GetVB(EVertexStreamType.VST_Position);
            var desc = new FSrvDesc();
            desc.SetBuffer(true);
            desc.Format = EPixelFormat.PXF_R32_TYPELESS;
            desc.Buffer.FirstElement = 0;
            desc.Buffer.NumElements = (uint)(mCoreObject.GetVertexNumber() * 3);
            var srv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(vb.Buffer, in desc);
            if (srv != null)
                srv.Dispose();
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class TtGfxDevice
    {
        public Bricks.GpuDriven.TtClusteredMeshManager ClusteredMeshManager { get; } = new Bricks.GpuDriven.TtClusteredMeshManager();
    }
}