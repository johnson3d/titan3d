using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineNS.NxRHI;

namespace EngineNS.Graphics.Pipeline.GI.ProbeVolume
{
    // =========================================================================
    // GPU 端数据结构 (与 ProbeVolumeCommon.cginc 一一对应)
    // =========================================================================

    [StructLayout(LayoutKind.Sequential)]
    public struct FPackedProbeSH
    {
        public Vector4 Data0;
        public Vector4 Data1;
        public Vector4 Data2;
        public Vector4 Data3;
        public Vector4 Data4;
        public Vector4 Data5;
        public Vector4 Data6;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FProbeInfo
    {
        public Vector3 Position;
        public float Radius;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FTetrahedronGpu
    {
        public Vector4ui ProbeIndices;
        public Vector4 BaryRow0;
        public Vector4 BaryRow1;
        public Vector4 BaryRow2;
        public Vector4 BaryRow3;
    }

    // BvhNodeGpu: 32 bytes, 与 HLSL BvhNodeGpu 一一对应
    // LeftChild bit31=1 → leaf: bit30..0 = tetraStart, Payload = tetraCount
    // LeftChild bit31=0 → internal: LeftChild = leftIdx, Payload = rightIdx
    [StructLayout(LayoutKind.Sequential)]
    public struct FBvhNodeGpu
    {
        public Vector3 AabbMin;
        public uint LeftChild;
        public Vector3 AabbMax;
        public uint Payload;

        public bool IsLeaf => (LeftChild & (1u << 31)) != 0;

        public void SetLeaf(uint tetraStart, uint tetraCount)
        {
            LeftChild = tetraStart | (1u << 31);
            Payload = tetraCount;
        }

        public void SetInternal(uint leftIdx, uint rightIdx)
        {
            LeftChild = leftIdx;
            Payload = rightIdx;
        }
    }

    // =========================================================================
    // TtProbeVolumeData
    //   CPU 端 probe volume 管理:
    //   - 维护 probe 位置列表
    //   - Delaunay 四面体化建立空间连接
    //   - 构建 BVH 加速四面体查询
    //   - 转换为 GPU 友好的 structured buffer 数据
    // =========================================================================
    public class TtProbeVolumeData : IDisposable
    {
        public List<FProbeInfo> Probes { get; private set; } = new List<FProbeInfo>();

        // CPU 端四面体化结果
        List<TtTetrahedronData> mTetrahedra = new List<TtTetrahedronData>();

        // GPU 上传数据 (构建后填充)
        FTetrahedronGpu[] mTetraGpuData;
        FBvhNodeGpu[] mBvhGpuData;
        FPackedProbeSH[] mProbeSHData;

        bool mIsBuilt = false;

        public int ProbeCount => Probes.Count;
        public int TetrahedronCount => mTetrahedra.Count;
        public bool IsBuilt => mIsBuilt;

        // 添加一个 probe
        public void AddProbe(Vector3 position, float radius = 0.0f)
        {
            Probes.Add(new FProbeInfo { Position = position, Radius = radius });
            mIsBuilt = false;
        }

        // 批量设置 probe 位置
        public void SetProbes(IList<Vector3> positions, float defaultRadius = 0.0f)
        {
            Probes.Clear();
            foreach (var pos in positions)
            {
                Probes.Add(new FProbeInfo { Position = pos, Radius = defaultRadius });
            }
            mIsBuilt = false;
        }

        // 在场景 AABB 内按网格均匀放置 probe
        public void GenerateGridProbes(in BoundingBox sceneAABB, float spacing)
        {
            Probes.Clear();
            var size = sceneAABB.GetSize();
            int countX = Math.Max(2, (int)MathF.Ceiling(size.X / spacing) + 1);
            int countY = Math.Max(2, (int)MathF.Ceiling(size.Y / spacing) + 1);
            int countZ = Math.Max(2, (int)MathF.Ceiling(size.Z / spacing) + 1);

            for (int iz = 0; iz < countZ; iz++)
            {
                for (int iy = 0; iy < countY; iy++)
                {
                    for (int ix = 0; ix < countX; ix++)
                    {
                        var pos = new Vector3(
                            sceneAABB.Minimum.X + ix * spacing,
                            sceneAABB.Minimum.Y + iy * spacing,
                            sceneAABB.Minimum.Z + iz * spacing);
                        Probes.Add(new FProbeInfo { Position = pos, Radius = 0.0f });
                    }
                }
            }
            mIsBuilt = false;
        }

        // 构建四面体化 + BVH (CPU 端, 可在后台线程执行)
        public void Build()
        {
            if (Probes.Count < 4)
            {
                mIsBuilt = false;
                return;
            }

            // 1. Delaunay 四面体化
            BuildTetrahedralization();

            // 2. 构建 GPU 四面体数据
            BuildTetraGpuData();

            // 3. 构建 BVH
            BuildBvh();

            // 4. 初始化 SH 数据 (全零)
            mProbeSHData = new FPackedProbeSH[Probes.Count];

            mIsBuilt = true;
        }

        void BuildTetrahedralization()
        {
            var points = new List<Vector3>(Probes.Count);
            foreach (var probe in Probes)
                points.Add(probe.Position);

            mTetrahedra = TtDelaunayTetrahedralization.ComputeDelaunayTetrahedralization(points);

            // 移除无效 (退化) 四面体
            mTetrahedra.RemoveAll(t => !t.IsValid());
        }

        void BuildTetraGpuData()
        {
            mTetraGpuData = new FTetrahedronGpu[mTetrahedra.Count];
            for (int i = 0; i < mTetrahedra.Count; i++)
            {
                var tetra = mTetrahedra[i];
                var gpuTetra = new FTetrahedronGpu();
                gpuTetra.ProbeIndices = new Vector4ui(
                    (uint)tetra.ProbeIndices[0],
                    (uint)tetra.ProbeIndices[1],
                    (uint)tetra.ProbeIndices[2],
                    (uint)tetra.ProbeIndices[3]);

                // BarycentricMatrix 是 4x4, 按行提取为 4 个 float4
                var m = tetra.BarycentricMatrix;
                gpuTetra.BaryRow0 = new Vector4(m.M11, m.M12, m.M13, m.M14);
                gpuTetra.BaryRow1 = new Vector4(m.M21, m.M22, m.M23, m.M24);
                gpuTetra.BaryRow2 = new Vector4(m.M31, m.M32, m.M33, m.M34);
                gpuTetra.BaryRow3 = new Vector4(m.M41, m.M42, m.M43, m.M44);

                mTetraGpuData[i] = gpuTetra;
            }
        }

        void BuildBvh()
        {
            if (mTetrahedra.Count == 0)
            {
                mBvhGpuData = Array.Empty<FBvhNodeGpu>();
                return;
            }

            // 用现有的 TtBVH 以四面体 AABB 为 primitive 构建 BVH
            var bvh = new TtBVH();
            var primitives = new List<TtBVH.TtBVHPrimitive>(mTetrahedra.Count);
            var volumeAABB = new BoundingBox(mTetrahedra[0].Bounds.Minimum, mTetrahedra[0].Bounds.Maximum);

            for (int i = 0; i < mTetrahedra.Count; i++)
            {
                var tetraBounds = mTetrahedra[i].Bounds;
                volumeAABB = BoundingBox.Merge(in volumeAABB, in tetraBounds);
            }

            // 稍微扩大 AABB 避免边界点恰好在 AABB 表面
            var expand = volumeAABB.GetSize() * 0.01f;
            volumeAABB = new BoundingBox(
                volumeAABB.Minimum - expand,
                volumeAABB.Maximum + expand);

            // TtBVHPrimitive 是引用类型 (class), Initialize 内部会就地 Morton sort primitives 列表.
            // 在 sort 前用 Dictionary<对象引用, 原始索引> 记录映射, sort 后通过引用追踪原始索引,
            // 实现 mTetraGpuData 与 sorted primitives 的同步重排.
            var primToOriginalIndex = new Dictionary<TtBVH.TtBVHPrimitive, int>(mTetrahedra.Count);
            for (int i = 0; i < mTetrahedra.Count; i++)
            {
                var prim = new TtBVH.TtBVHPrimitive();
                prim.Center = mTetrahedra[i].Bounds.GetCenter();
                prim.AABB = mTetrahedra[i].Bounds;
                primitives.Add(prim);
                primToOriginalIndex[prim] = i;
            }

            if (!bvh.Initialize(volumeAABB, primitives, 1))
            {
                mBvhGpuData = Array.Empty<FBvhNodeGpu>();
                return;
            }
            bvh.BuildBVH();

            // primitives 已被 Morton sort 重排, 同步重排 mTetraGpuData
            var sortedTetraGpu = new FTetrahedronGpu[mTetrahedra.Count];
            for (int i = 0; i < bvh.Primitives.Count; i++)
            {
                int originalIndex = primToOriginalIndex[bvh.Primitives[i]];
                sortedTetraGpu[i] = mTetraGpuData[originalIndex];
            }
            mTetraGpuData = sortedTetraGpu;

            // 转换 TtBVH.FBVHNode[] 为 FBvhNodeGpu[]
            // TtBVH 使用完全二叉树: RightChild = LeftChild + 1 (隐式)
            // GPU 端 FBvhNodeGpu 用 Payload 显式存右子节点索引
            mBvhGpuData = new FBvhNodeGpu[bvh.HbvNodes.Length];
            for (int i = 0; i < bvh.HbvNodes.Length; i++)
            {
                var src = bvh.HbvNodes[i];
                var dst = new FBvhNodeGpu();
                dst.AabbMin = src.AABB.Minimum;
                dst.AabbMax = src.AABB.Maximum;

                if (src.IsLeaf)
                {
                    uint leafStart = src.LeafPrimStartIndex;
                    uint leafCount = (leafStart < bvh.PrimitiveGroup) ? 1u : 0u;
                    dst.SetLeaf(leafStart, leafCount);
                }
                else
                {
                    dst.SetInternal(src.LeftChild, src.RightChild);
                }

                mBvhGpuData[i] = dst;
            }
        }

        // =====================================================================
        // GPU Buffer 上传
        // =====================================================================
        public unsafe void UploadToGpu(
            TtGpuBuffer<FProbeInfo> probeInfoBuffer,
            TtGpuBuffer<FPackedProbeSH> probeSHBuffer,
            TtGpuBuffer<FTetrahedronGpu> tetraBuffer,
            TtGpuBuffer<FBvhNodeGpu> bvhBuffer)
        {
            if (!mIsBuilt)
                return;

            // Probe info
            fixed (FProbeInfo* pProbes = Probes.ToArray())
            {
                probeInfoBuffer.SetSize((uint)Probes.Count, pProbes,
                    EBufferType.BFT_SRV);
            }

            // Probe SH (初始全零, 由 GPU compute shader 渐进填充)
            if (mProbeSHData == null || mProbeSHData.Length != Probes.Count)
                mProbeSHData = new FPackedProbeSH[Probes.Count];
            fixed (FPackedProbeSH* pSH = mProbeSHData)
            {
                probeSHBuffer.SetSize((uint)Probes.Count, pSH,
                    EBufferType.BFT_SRV | EBufferType.BFT_UAV);
            }

            // Tetrahedra
            if (mTetraGpuData.Length > 0)
            {
                fixed (FTetrahedronGpu* pTetra = mTetraGpuData)
                {
                    tetraBuffer.SetSize((uint)mTetraGpuData.Length, pTetra,
                        EBufferType.BFT_SRV);
                }
            }

            // BVH
            if (mBvhGpuData.Length > 0)
            {
                fixed (FBvhNodeGpu* pBvh = mBvhGpuData)
                {
                    bvhBuffer.SetSize((uint)mBvhGpuData.Length, pBvh,
                        EBufferType.BFT_SRV);
                }
            }
        }

        public uint GetBvhNodeCount() => mBvhGpuData != null ? (uint)mBvhGpuData.Length : 0u;
        public uint GetTetraCount() => mTetraGpuData != null ? (uint)mTetraGpuData.Length : 0u;

        public void Dispose()
        {
            Probes.Clear();
            mTetrahedra.Clear();
            mTetraGpuData = null;
            mBvhGpuData = null;
            mProbeSHData = null;
            mIsBuilt = false;
        }
    }
}
