using NPOI.SS.Formula.Functions;
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

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FClusterInfo
    {
        public BoundingBox AABB;
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

        public FClusterInfo Desc;
    }
    public class TtClusteredMesh
    {
        public bool InitFromMesh(Graphics.Mesh.UMeshPrimitives mesh)
        {
            Mesh = mesh;
            return true;
        }
        public void SaveClusteredMesh(RName meshName)
        {
            var file = meshName.Address + ".clustermesh";
            var xnd = new IO.TtXndHolder("Cluster", 0, 0);

            var rc = UEngine.Instance?.GfxDevice.RenderContext;
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
        public static TtClusteredMesh LoadClusteredMesh(RName meshName, Graphics.Mesh.UMeshPrimitives mesh)
        {
            var file = meshName.Address + ".clustermesh";
            var xnd = IO.TtXndHolder.LoadXnd(file);
            var result = new TtClusteredMesh();
            // load vbs, ib
            mesh.mCoreObject.LoadClusters(xnd.mCoreObject);

            return result;
        }
        public NxRHI.UVertexArray VBs;
        public NxRHI.UIbView IB;
        public int NumVertices;
        public int NumIndices;
        public List<TtCluster> Clusters = new List<TtCluster>();
        public BoundingBox AABB = new BoundingBox();

        private Graphics.Mesh.UMeshPrimitives Mesh;
        public static unsafe TtClusteredMesh Merge(List<TtClusteredMesh> meshes, NxRHI.UCommandList cmdlist)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var result = new TtClusteredMesh();
            uint Streams = 0;
            int TotalVertices = 0;
            int TotalIndices = 0;
            var cpVbDrawcalls = new List<KeyValuePair<NxRHI.EVertexStreamType, NxRHI.UCopyDraw>>();
            var cpIbDrawcalls = new List<NxRHI.UCopyDraw>();
            NxRHI.FSubResourceFootPrint footPrint = new NxRHI.FSubResourceFootPrint();
            foreach (var mesh in meshes)
            {
                for (NxRHI.EVertexStreamType i = 0; i < NxRHI.EVertexStreamType.VST_Number; i++)
                {
                    var vb = mesh.VBs.mCoreObject.GetVB(i);
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

                        var t = new KeyValuePair<NxRHI.EVertexStreamType, NxRHI.UCopyDraw>(i, drawcall);
                        cpVbDrawcalls.Add(t);
                    }
                }
                //copy IndexBuffer
                {
                    var drawcall = rc.CreateCopyDraw();
                    drawcall.mCoreObject.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                    drawcall.mCoreObject.BindBufferSrc(mesh.IB.mCoreObject.Buffer);
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
            
            result.VBs = new NxRHI.UVertexArray();
            for (NxRHI.EVertexStreamType i = 0; i < NxRHI.EVertexStreamType.VST_Number; i++)
            {
                if ((Streams & (1u << (int)i)) != 0)
                {
                    var bfDesc = new NxRHI.FBufferDesc();
                    bfDesc.SetDefault(false);
                    var buffer = rc.CreateBuffer(in bfDesc);

                    var vbvDesc = new NxRHI.FVbvDesc();
                    vbvDesc.SetDefault();
                    var vb = rc.CreateVBV(buffer, in vbvDesc);
                    result.VBs.mCoreObject.BindVB(i, vb.mCoreObject);
                }
            }
            {
                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(false);
                var buffer = rc.CreateBuffer(in bfDesc);
                var ibvDesc = new NxRHI.FIbvDesc();
                ibvDesc.SetDefault();
                result.IB = rc.CreateIBV(buffer, ibvDesc);
            }

            result.NumVertices = TotalVertices;
            result.NumIndices = TotalIndices;

            foreach (var i in cpVbDrawcalls)
            {
                var vb = result.VBs.mCoreObject.GetVB(i.Key);
                i.Value.mCoreObject.BindBufferDest(vb.Buffer);

                cmdlist.PushGpuDraw(i.Value);
                //i.Value.mCoreObject.Commit(cmdlist.mCoreObject);
            }

            foreach (var i in cpIbDrawcalls)
            {
                var ib = result.IB.mCoreObject;
                i.mCoreObject.BindBufferDest(ib.Buffer);
                cmdlist.PushGpuDraw(i);
                //i.mCoreObject.Commit(cmdlist.mCoreObject);
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
        public async Thread.Async.TtTask<TtClusteredMesh> GetClusteredMesh(RName name, Graphics.Mesh.UMeshPrimitives mesh)
        {
            TtClusteredMesh result;
            if (ClusteredMeshes.TryGetValue(name, out result))
                return result;

            if (IO.TtFileManager.FileExists(name.Address + ".clustermesh") == false)
            {
                return null;
            }

            result = await UEngine.Instance.EventPoster.Post((state) =>
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
}

namespace EngineNS.Graphics.Mesh
{
    public partial class UMeshPrimitives
    {
        Bricks.GpuDriven.TtClusteredMesh mClusteredMesh;
        public Bricks.GpuDriven.TtClusteredMesh ClusteredMesh { get => mClusteredMesh; }
        public async Thread.Async.TtTask<Bricks.GpuDriven.TtClusteredMesh> TryLoadClusteredMesh(bool bForce = false)
        {
            if (mClusteredMesh == null || bForce)
            {
                var meshMeta = GetAMeta() as UMeshPrimitivesAMeta;
                if (meshMeta == null || meshMeta.IsClustered == false)
                    return null;
                mClusteredMesh = await UEngine.Instance.GfxDevice.ClusteredMeshManager.GetClusteredMesh(AssetName, this);
            }
            return mClusteredMesh;
        }
        public Bricks.GpuDriven.TtClusteredMesh BuildClusteredMesh()
        {
            var result = new Bricks.GpuDriven.TtClusteredMesh();
            result.InitFromMesh(this);
            result.SaveClusteredMesh(this.AssetName);
            
            return result;
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice
    {
        public Bricks.GpuDriven.TtClusteredMeshManager ClusteredMeshManager { get; } = new Bricks.GpuDriven.TtClusteredMeshManager();
    }
}