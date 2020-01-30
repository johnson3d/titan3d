using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.GpuDriven.GpuScene
{
    public partial class SceneDataManager
    {
        #region Fallback to SM3
        CSamplerState mSamplerState;

        CTexture2D mVertexTexture;
        CShaderResourceView mVertexTextureView;
        Vector4[] mVertexTexData = new Vector4[512 * 512];

        CTexture2D mInstanceDataTexture;
        CShaderResourceView mInstanceDataTextureView;
        Vector4[] mInstTexData = new Vector4[256 * 256];

        CIndexBuffer mCpuDrawIndexBuffer;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        unsafe struct CBMeshBatch
        {
            public Plane GpuDrivenCameraPlane0;
            public Plane GpuDrivenCameraPlane1;
            public Plane GpuDrivenCameraPlane2;
            public Plane GpuDrivenCameraPlane3;
            public Plane GpuDrivenCameraPlane4;
            public Plane GpuDrivenCameraPlane5;

            public Vector3 GpuDrivenCameraPosition;
            public uint MeshBatchVertexStride;

            public Vector3 GpuDrivenFrustumMinPoint;
            public uint ClusterNumber;

            public Vector3 GpuDrivenFrustumMaxPoint;
            public uint EnableGpuCulling;
        }
        #endregion

        private unsafe void FlipV(Vector4[] pixels, uint w, uint h)
        {
            for (int i = 0; i < h/2; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var save = pixels[i * w + j];
                    pixels[i * w + j] = pixels[(w - i - 2) * w + j];
                    pixels[(w - i - 2) * w + j] = save;
                }
            }
        }

        public unsafe void UpdateGpuBufferVTF(CRenderContext rc, EngineNS.CCommandList cmd, Graphics.CGfxCamera Camera)
        {
            CTexture2DDesc desc = new CTexture2DDesc();
            desc.Init();

            var spDesc = new CSamplerStateDesc();
            spDesc.SetDefault();
            spDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
            mSamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(rc, spDesc);

            int vertStride = sizeof(Cluster.GpuSceneVertex);
            int size = mAllVertices.Count * vertStride;
            int side = (int)Math.Sqrt((float)size) + 1;
            desc.Width = 512;
            desc.Height = 512;
            desc.MipLevels = 1;
            desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
            mVertexTexture = rc.CreateTexture2D(desc);
            CShaderResourceViewDesc srvDesc = new CShaderResourceViewDesc();
            srvDesc.mFormat = desc.Format;
            srvDesc.mTexture2D = mVertexTexture.CoreObject;
            mVertexTextureView = rc.CreateShaderResourceView(srvDesc);
            mVertexTextureView.ResourceState.StreamState = EStreamingState.SS_Valid;

            {
                var copyArray = mAllVertices.ToArray();
                fixed (Vector4* p = &mVertexTexData[0])
                fixed (Cluster.GpuSceneVertex* v = &copyArray[0])
                {
                    CoreSDK.SDK_Memory_Copy(p, v, (uint)size);

                    mVertexTexture.UpdateMipData(cmd, 0, p, 512, 512, 512 * 4 * 4);
                }
            }

            int InstStride = sizeof(GpuMeshInstanceData);
            size = mGpuInstanceDatas.Count * InstStride;
            side = (int)Math.Sqrt((float)size) + 1;
            desc.Width = 256;
            desc.Height = 256;
            desc.MipLevels = 1;
            desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
            mInstanceDataTexture = rc.CreateTexture2D(desc);
            srvDesc.mFormat = desc.Format;
            srvDesc.mTexture2D = mInstanceDataTexture.CoreObject;
            mInstanceDataTextureView = rc.CreateShaderResourceView(srvDesc);
            mInstanceDataTextureView.ResourceState.StreamState = EStreamingState.SS_Valid;

            {
                var copyArray = mGpuInstanceDatas.ToArray();
                fixed (Vector4* p = &mInstTexData[0])
                fixed (GpuMeshInstanceData* v = &copyArray[0])
                {
                    CoreSDK.SDK_Memory_Copy(p, v, (uint)size);
                    mInstanceDataTexture.UpdateMipData(cmd, 0, p, 256, 256, 256 * 4 * 4);
                }
            }
        }
        private bool TestBit(UInt64 InBits, int index)
        {
            return (InBits & (UInt64)((UInt64)1 << index)) != 0;
        }
        private unsafe bool FrustumCull(ref Cluster.GpuCluster cluster, ref CBMeshBatch cbMesh)
        {
	        Vector3 position = cluster.BoundCenter;
            Vector3 extent = cluster.BoundExtent;
            Vector3 minPos = position - extent;
            Vector3 maxPos = position + extent;
            if(cbMesh.GpuDrivenFrustumMinPoint.X > maxPos.X ||
                cbMesh.GpuDrivenFrustumMinPoint.Y > maxPos.Y ||
                cbMesh.GpuDrivenFrustumMinPoint.Z > maxPos.Z)
            {
                return true;
            }

            if (cbMesh.GpuDrivenFrustumMaxPoint.X < minPos.X ||
                cbMesh.GpuDrivenFrustumMaxPoint.Y < minPos.Y ||
                cbMesh.GpuDrivenFrustumMaxPoint.Z < minPos.Z)
            {
                return true;
            }

	        fixed(CBMeshBatch* pCBuffer = &cbMesh)
            {
                Vector4* planes = (Vector4*)pCBuffer;
                Vector3 absNormal = new Vector3();
                for (uint i = 0; i < 6; ++i)
                {
                    absNormal.X = (planes->X > 0) ? (planes->X) : (-planes->X);
                    absNormal.Y = (planes->Y > 0) ? (planes->Y) : (-planes->Y);
                    absNormal.Z = (planes->Z > 0) ? (planes->Z) : (-planes->Z);
                    Vector3* planeNormal = (Vector3*)planes;
                    float dist = Vector3.Dot(position, *planeNormal) + planes->W;
                    float df = Vector3.Dot(absNormal, extent);
                    if (dist - df > 0)
                    {
                        return true;
                    }
                    planes++;
                }
            }
	        return false;
        }
        EngineNS.Bricks.GpuDriven.GpuScene.GpuDrawArgs mDrawArgs;
        Support.NativeList<UInt32_3> mDrawIndices = new Support.NativeList<UInt32_3>();
        private unsafe void UpdateIndexBufferCPU(CCommandList cmd, Graphics.CGfxCamera Camera)
        {
            if (mCpuDrawIndexBuffer == null)
                return;
            CBMeshBatch cbMesh = new CBMeshBatch();
            {
                var pPlanes = stackalloc Plane[6];
                Camera.CullingFrustum.GetPlanes(pPlanes);
                CBMeshBatch* pCBuffer = &cbMesh;
                Plane* planesTar = (Plane*)pCBuffer;
                for (uint i = 0; i < 6; i++)
                {
                    planesTar[i] = pPlanes[i];
                }
                cbMesh.GpuDrivenCameraPosition = Camera.CullingFrustum.TipPos;
                BoundingBox box = new BoundingBox();
                Camera.CullingFrustum.GetBoundBox(ref box);
                cbMesh.GpuDrivenFrustumMinPoint = box.Minimum;
                cbMesh.GpuDrivenFrustumMaxPoint = box.Maximum;
                cbMesh.MeshBatchVertexStride = (uint)AllVertices.Count;
                cbMesh.ClusterNumber = (uint)GpuClusters.Count;

                UpdateCBMeshbatch(cmd, Camera);
            }

            UInt32_3 tri = new UInt32_3();
            mDrawIndices.Clear();
            for (int i = 0; i < GpuClusters.Count; i++)
            {
                var cluster = GpuClusters[i];
                var GpuDrivenCameraDirection = GpuClusters[i].BoundCenter - cbMesh.GpuDrivenCameraPosition;
                var cameraDirInBoundSpace = Vector3.TransposeTransformNormal(GpuDrivenCameraDirection, GpuInstanceDatas[(int)cluster.InstanceId].InvMatrix);
                if (FrustumCull(ref cluster, ref cbMesh) == false)
                {
                    UInt64 visBits = 0;
                    if (cameraDirInBoundSpace.X >= 0)
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_NX];
                    }
                    else
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_X];
                    }
                    if (cameraDirInBoundSpace.Y >= 0)
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_NY];
                    }
                    else
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_Y];
                    }
                    if (cameraDirInBoundSpace.Z >= 0)
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_NZ];
                    }
                    else
                    {
                        visBits |= cluster.CubeFaces[(int)Cluster.GpuCluster.ECubeFace.CubeFace_Z];
                    }

                    uint InstanceStartIndex = cbMesh.MeshBatchVertexStride * cluster.InstanceId;
                    for (int j = 0; j < cluster.FaceCount; j++)
                    {
                        if (TestBit(visBits, j) == false)
                        {
                            continue;
                        }
                        int srcIndex = (int)(cluster.StartFaceIndex + j) * 3;
                        tri.x = InstanceStartIndex + AllIndices[srcIndex];
                        tri.y = InstanceStartIndex + AllIndices[srcIndex + 1];
                        tri.z = InstanceStartIndex + AllIndices[srcIndex + 2];
                        mDrawIndices.Add(tri);
                    }   
                }
            }

            mDrawArgs.InstanceCount = 1;
            mDrawArgs.IndexCountPerInstance = (uint)mDrawIndices.Count * 3;
            uint size = (uint)(mDrawIndices.Count * sizeof(UInt32_3));
            if (mCpuDrawIndexBuffer.Desc.ByteWidth > size)
                mCpuDrawIndexBuffer.UpdateBuffData(cmd, mDrawIndices.GetBufferPtr(), size);
        }
    }
}
