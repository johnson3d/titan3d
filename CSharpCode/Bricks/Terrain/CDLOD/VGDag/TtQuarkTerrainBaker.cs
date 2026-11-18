using System;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// 地形远景 cluster DAG 烘焙器: 原生 FQuarkDAGBuilder::BuildTerrain
    /// (METIS 分组 + meshopt 锁边简化 + 拓扑后验, LODError 为相对全分辨率高度场的精确竖直 L∞ 上界)
    /// 的托管封装, 把原生结果转换为 TtVgDagAsset。
    /// 线程模型: Bake 在后台线程执行; RequestCancel / Dispose 可从任意线程调用,
    /// 原生侧用原子标志在每层/每个 group 前检查, 被取消时 Bake 返回 null。
    /// </summary>
    public class TtQuarkTerrainBaker : IDisposable
    {
        public const uint MaxGroupSize = 32;
        public const uint ClusterSize = 128;
        /// <summary>简化不设误差预算, 只按三角形数目标折半; 误差由 M2 后验度量。</summary>
        public const float SimplifyTargetError = float.MaxValue;
        /// <summary>叶子网格顶点数上限(64M): 超过后原生侧邻接/分区内存达数十 GB, 直接拒绝而非 bad_alloc。</summary>
        public const long MaxLeafVertices = 1L << 26;

        readonly object mLock = new object();
        FQuarkDAGBuilder mBuilder;
        bool mCancelRequested;

        public TtQuarkTerrainBaker()
        {
            mBuilder = FQuarkDAGBuilder.CreateInstance();
        }

        public void RequestCancel()
        {
            lock (mLock)
            {
                mCancelRequested = true;
                if (mBuilder.IsValidPointer)
                    mBuilder.RequestCancel();
            }
        }

        /// <summary>只能由执行 Bake 的线程在 Bake 返回后调用; RequestCancel 可与之并发 (同锁串行化)。</summary>
        public void Dispose()
        {
            lock (mLock)
            {
                if (mBuilder.IsValidPointer)
                    mBuilder.NativeSuper.Release();
                mBuilder = default;
            }
        }

        /// <summary>
        /// heights: size×size 行主序 (z * size + x) 的最终高度。step: 叶子降采样步长 (texel)。
        /// 输入非法时抛 ArgumentException; 被取消时返回 null。同一实例只能 Bake 一次。
        /// </summary>
        public unsafe TtVgDagAsset Bake(float[] heights, int size, int step, Action<string> reportStage = null)
        {
            if (heights == null || size < 2 || size > 65536 || heights.Length != (long)size * size)
                throw new ArgumentException("heightfield size mismatch");
            if (step < 1 || step >= size)
                throw new ArgumentException("step out of range");
            long leafSide = (size - 1 + step - 1) / step + 1;
            if (leafSide * leafSide > MaxLeafVertices)
                throw new ArgumentException($"leaf grid {leafSide}x{leafSide} exceeds {MaxLeafVertices} vertices, increase step");
            if (!mBuilder.IsValidPointer)
                throw new ObjectDisposedException(nameof(TtQuarkTerrainBaker));
            // 先登记后取消的请求也必须生效: 原生标志在构造后才存在, 这里再同步一次。
            lock (mLock)
            {
                if (mCancelRequested)
                    return null;
            }

            reportStage?.Invoke($"DAG 后台开始: height={size}×{size}, step={step}, cluster={ClusterSize}, group≤{MaxGroupSize}");
            int numClusters;
            fixed (float* pHeights = heights)
            {
                numClusters = mBuilder.BuildTerrain(pHeights, (uint)size, (uint)step, MaxGroupSize, ClusterSize, SimplifyTargetError);
            }
            if (numClusters < 0 || mBuilder.IsCancelRequested())
                return null;
            if (numClusters == 0)
                throw new ArgumentException("FQuarkDAGBuilder.BuildTerrain rejected the heightfield (non-finite heights?)");

            int vertexCount = (int)mBuilder.GetVertexCount();
            int indexCount = (int)mBuilder.GetIndexCount();
            int groupCount = (int)mBuilder.GetGroupCount();
            var positions = new Vector3[vertexCount];
            var indices = new uint[indexCount];
            var mips = new int[numClusters];
            var errors = new float[numClusters];
            var starts = new uint[numClusters];
            var counts = new uint[numClusters];
            var genGroups = new uint[numClusters];
            var parentGroups = new uint[numClusters];
            var groupMips = new int[groupCount];
            var groupErrors = new float[groupCount];
            fixed (Vector3* p = positions)
                mBuilder.CopyPositions(p, (uint)vertexCount);
            fixed (uint* p = indices)
                mBuilder.CopyIndices(p, (uint)indexCount);
            fixed (int* pMip = mips)
            fixed (float* pErr = errors)
            fixed (uint* pStart = starts)
            fixed (uint* pCount = counts)
            fixed (uint* pGen = genGroups)
                mBuilder.CopyClusters(pMip, pErr, pStart, pCount, pGen, (uint)numClusters);
            fixed (uint* p = parentGroups)
                mBuilder.CopyClusterParents(p, (uint)numClusters);
            fixed (int* pMip = groupMips)
            fixed (float* pErr = groupErrors)
                mBuilder.CopyGroups(pMip, pErr, null, null, (uint)groupCount);

            var asset = Convert(heights, size, step, (int)mBuilder.GetMipLevelCount(), positions, indices,
                mips, errors, starts, counts, genGroups, parentGroups, groupMips, groupErrors);
            reportStage?.Invoke($"DAG 烘焙完成: mips={asset.MipLevelCount}, clusters={asset.Clusters.Length}, groups={asset.Groups.Length}, vertices={asset.Vertices.Length}");
            return asset;
        }

        static ushort ToTexel(float v, int size)
        {
            if (!(v >= 0.0f && v < size) || v != MathF.Floor(v))
                throw new InvalidOperationException($"Quark terrain vertex x/z {v} is not a texel lattice point");
            return (ushort)v;
        }

        static TtVgDagAsset Convert(float[] heights, int size, int step, int mipCount, Vector3[] positions, uint[] indices,
            int[] mips, float[] errors, uint[] starts, uint[] counts, uint[] genGroups, uint[] parentGroups,
            int[] groupMips, float[] groupErrors)
        {
            var asset = new TtVgDagAsset();
            asset.HeightfieldSize = size;
            asset.Step = step;
            asset.MipLevelCount = mipCount;
            float hMin = float.MaxValue, hMax = float.MinValue;
            foreach (var h in heights)
            {
                hMin = Math.Min(hMin, h);
                hMax = Math.Max(hMax, h);
            }
            asset.HeightMin = hMin;
            asset.HeightMax = hMax;

            // 原生导出时每个 cluster 的顶点连续存放, 索引是全局下标; 这里按引用范围重排为 BaseVertex + 局部 ushort。
            asset.Vertices = new FVgVertex[positions.Length];
            for (int i = 0; i < positions.Length; i++)
                asset.Vertices[i] = new FVgVertex(ToTexel(positions[i].X, size), ToTexel(positions[i].Z, size), positions[i].Y);
            asset.Indices = new ushort[indices.Length];
            asset.Clusters = new FVgCluster[mips.Length];
            for (int c = 0; c < mips.Length; c++)
            {
                long start = starts[c], count = counts[c];
                if (count == 0 || count % 3 != 0 || start + count > indices.Length)
                    throw new InvalidOperationException($"Quark cluster[{c}] index range invalid");
                uint lo = uint.MaxValue, hi = 0;
                for (long k = start; k < start + count; k++)
                {
                    lo = Math.Min(lo, indices[k]);
                    hi = Math.Max(hi, indices[k]);
                }
                if (hi >= positions.Length || hi - lo > ushort.MaxValue)
                    throw new InvalidOperationException($"Quark cluster[{c}] vertex range invalid");
                var cluster = new FVgCluster();
                cluster.MipLevel = mips[c];
                cluster.BaseVertex = (int)lo;
                cluster.VertexCount = (int)(hi - lo + 1);
                cluster.IndexStart = (int)start;
                cluster.IndexCount = (int)count;
                cluster.LODError = errors[c];
                cluster.GeneratingGroup = genGroups[c] == uint.MaxValue ? -1 : (int)genGroups[c];
                cluster.ParentGroup = parentGroups[c] == uint.MaxValue ? -1 : (int)parentGroups[c];
                cluster.MinTexelX = cluster.MinTexelZ = ushort.MaxValue;
                cluster.MinY = float.MaxValue;
                cluster.MaxY = float.MinValue;
                for (long k = start; k < start + count; k++)
                {
                    asset.Indices[k] = (ushort)(indices[k] - lo);
                    var v = asset.Vertices[indices[k]];
                    cluster.MinTexelX = Math.Min(cluster.MinTexelX, v.TexelX);
                    cluster.MinTexelZ = Math.Min(cluster.MinTexelZ, v.TexelZ);
                    cluster.MaxTexelX = Math.Max(cluster.MaxTexelX, v.TexelX);
                    cluster.MaxTexelZ = Math.Max(cluster.MaxTexelZ, v.TexelZ);
                    cluster.MinY = Math.Min(cluster.MinY, v.Y);
                    cluster.MaxY = Math.Max(cluster.MaxY, v.Y);
                }
                asset.Clusters[c] = cluster;
            }

            // group 包围盒在资产侧按"子 cluster ∪ 子 cluster 的生成 group"重算 (原生 group 按层追加, 生成 group 下标必小于父 group),
            // 与资产里的 cluster 包围盒同源, Validate 可逐项校验单调。
            asset.Groups = new FVgGroup[groupMips.Length];
            for (int g = 0; g < asset.Groups.Length; g++)
            {
                var group = new FVgGroup();
                group.MipLevel = groupMips[g];
                group.ParentLODError = groupErrors[g];
                group.MinTexelX = group.MinTexelZ = ushort.MaxValue;
                group.MinY = float.MaxValue;
                group.MaxY = float.MinValue;
                asset.Groups[g] = group;
            }
            void Merge(ref FVgGroup dst, ushort minX, ushort minZ, ushort maxX, ushort maxZ, float minY, float maxY)
            {
                dst.MinTexelX = Math.Min(dst.MinTexelX, minX);
                dst.MinTexelZ = Math.Min(dst.MinTexelZ, minZ);
                dst.MaxTexelX = Math.Max(dst.MaxTexelX, maxX);
                dst.MaxTexelZ = Math.Max(dst.MaxTexelZ, maxZ);
                dst.MinY = Math.Min(dst.MinY, minY);
                dst.MaxY = Math.Max(dst.MaxY, maxY);
            }
            var childrenOf = new System.Collections.Generic.List<int>[asset.Groups.Length];
            for (int c = 0; c < asset.Clusters.Length; c++)
            {
                int p = asset.Clusters[c].ParentGroup;
                if (p < 0)
                    continue;
                if (childrenOf[p] == null)
                    childrenOf[p] = new System.Collections.Generic.List<int>();
                childrenOf[p].Add(c);
            }
            for (int g = 0; g < asset.Groups.Length; g++)
            {
                if (childrenOf[g] == null)
                    throw new InvalidOperationException($"Quark group[{g}] has no children");
                foreach (int c in childrenOf[g])
                {
                    var cl = asset.Clusters[c];
                    Merge(ref asset.Groups[g], cl.MinTexelX, cl.MinTexelZ, cl.MaxTexelX, cl.MaxTexelZ, cl.MinY, cl.MaxY);
                    if (cl.GeneratingGroup >= 0)
                    {
                        if (cl.GeneratingGroup >= g)
                            throw new InvalidOperationException($"Quark group order violated: group[{g}] child from group[{cl.GeneratingGroup}]");
                        var gen = asset.Groups[cl.GeneratingGroup];
                        Merge(ref asset.Groups[g], gen.MinTexelX, gen.MinTexelZ, gen.MaxTexelX, gen.MaxTexelZ, gen.MinY, gen.MaxY);
                    }
                }
            }

            string error;
            if (!asset.Validate(out error))
                throw new InvalidOperationException($"Quark terrain DAG failed validation: {error}");
            return asset;
        }
    }
}
