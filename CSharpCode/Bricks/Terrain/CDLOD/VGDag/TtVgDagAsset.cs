using System;
using System.Collections.Generic;
using System.IO;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// 全局三角形对角约定(接缝硬约定②):
    /// 每个 quad 的四个角记 a=(x,z), b=(x+1,z), c=(x+1,z+1), d=(x,z+1),
    /// 对角线固定为 a→c, 两个三角形为 (a,b,c) 与 (a,c,d)。
    /// 相邻 cluster / 相邻 Level 的共享边两侧必须遵守同一约定,
    /// 否则即使顶点逐位重合, 对角方向相反也会留下零面积细缝(背面剔除后闪烁)。
    /// </summary>
    public static class FVgDiagonalConvention
    {
        /// <summary>对角线方向: (x,z) → (x+1,z+1)</summary>
        public const bool DiagonalToLowerRight = true;
    }

    /// <summary>
    /// 合并层顶点编码(格点模式): xz snap 到 heightfield texel 格点(每轴 10bit),
    /// y 是 bake 时定死的世界高度, 运行期远景不回采 heightmap。
    /// 每顶点 8 字节定长池。叶子 cluster 不进此池(隐式 grid, 同 CDLOD mip 模式)。
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct FVgVertex
    {
        public ushort TexelX;
        public ushort TexelZ;
        public float Y;
        public FVgVertex(ushort x, ushort z, float y)
        {
            TexelX = x;
            TexelZ = z;
            Y = y;
        }
    }

    /// <summary>
    /// cluster 元数据。层级序(BFS)存放: 第 0 层为叶子(32×32 quads, 隐式网格),
    /// 每往上一层由 2×2 子 cluster 合并 + 锁边 QEM 简化而来, 直到根覆盖整块 heightfield。
    /// </summary>
    public class TtVgCluster
    {
        public int Level;
        public int MinTexelX;
        public int MinTexelZ;
        /// <summary>边长(texel 数)。叶子 = LeafClusterQuads, 每上层翻倍。</summary>
        public int ExtentTexel;
        /// <summary>四个子 cluster 在 Clusters[] 中的起始下标; 叶子为 -1。</summary>
        public int FirstChild;
        /// <summary>显式顶点池起始下标; 叶子为 -1(隐式 grid)。</summary>
        public int BaseVertex;
        public int IndexStart;
        /// <summary>叶子恒为 0(索引由 VS 顶点 ID 隐式生成)。</summary>
        public int IndexCount;
        public float MinY;
        public float MaxY;
        /// <summary>bake 时本层简化施加的最大 QEM 代价(cluster LOD 选择的误差度量)。</summary>
        public float MaxError;

        public bool IsLeaf
        {
            get { return FirstChild < 0; }
        }
    }

    /// <summary>
    /// 单块 heightfield 的 cluster DAG 资产(纯数据, 不含引擎运行时依赖)。
    /// 与 .trlvl 同内容 hash 缓存; 确定性 bake 保证同输入两次产出逐位一致。
    /// </summary>
    public class TtVgDagAsset
    {
        public int HeightfieldSize = 1024;
        /// <summary>叶子 cluster 边长(quads)。约定: 采样数 = quads + 1,
        /// 相邻 cluster / 相邻 Level 共享边界采样(UE Landscape 同款约定)。</summary>
        public int LeafClusterQuads = 32;
        public int LevelCount = 1;
        public float HeightMin;
        public float HeightMax;
        /// <summary>每层的实际最大 QEM 误差(索引 = 层号)。</summary>
        public float[] LevelErrors;
        public FVgVertex[] Vertices;
        public uint[] Indices;
        public TtVgCluster[] Clusters;

        int mClustersPerSide0 = -1;
        /// <summary>第 0 层(叶子)每边的 cluster 数。</summary>
        public int ClustersPerSide0
        {
            get
            {
                if (mClustersPerSide0 < 0)
                    mClustersPerSide0 = (HeightfieldSize - 1) / LeafClusterQuads;
                return mClustersPerSide0;
            }
        }
        public int ClustersPerSide(int level)
        {
            return ClustersPerSide0 >> level;
        }
        /// <summary>指定层在 Clusters[] 中的起始下标(层内 row-major: cz * n + cx)。</summary>
        public int GetLevelStart(int level)
        {
            int start = 0;
            for (int i = 0; i < level; i++)
            {
                int n = ClustersPerSide(i);
                start += n * n;
            }
            return start;
        }
        public TtVgCluster GetCluster(int level, int cx, int cz)
        {
            int n = ClustersPerSide(level);
            return Clusters[GetLevelStart(level) + cz * n + cx];
        }

        public void Save(BinaryWriter w)
        {
            w.Write(HeightfieldSize);
            w.Write(LeafClusterQuads);
            w.Write(LevelCount);
            w.Write(HeightMin);
            w.Write(HeightMax);
            w.Write(LevelErrors.Length);
            foreach (var e in LevelErrors)
                w.Write(e);
            w.Write(Vertices.Length);
            foreach (var v in Vertices)
            {
                w.Write(v.TexelX);
                w.Write(v.TexelZ);
                w.Write(v.Y);
            }
            w.Write(Indices.Length);
            foreach (var i in Indices)
                w.Write(i);
            w.Write(Clusters.Length);
            foreach (var c in Clusters)
            {
                w.Write(c.Level);
                w.Write(c.MinTexelX);
                w.Write(c.MinTexelZ);
                w.Write(c.ExtentTexel);
                w.Write(c.FirstChild);
                w.Write(c.BaseVertex);
                w.Write(c.IndexStart);
                w.Write(c.IndexCount);
                w.Write(c.MinY);
                w.Write(c.MaxY);
                w.Write(c.MaxError);
            }
        }
        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                Save(w);
                w.Flush();
                return ms.ToArray();
            }
        }
        public static TtVgDagAsset Load(BinaryReader r)
        {
            var asset = new TtVgDagAsset();
            asset.HeightfieldSize = r.ReadInt32();
            asset.LeafClusterQuads = r.ReadInt32();
            asset.LevelCount = r.ReadInt32();
            asset.HeightMin = r.ReadSingle();
            asset.HeightMax = r.ReadSingle();
            asset.LevelErrors = new float[r.ReadInt32()];
            for (int i = 0; i < asset.LevelErrors.Length; i++)
                asset.LevelErrors[i] = r.ReadSingle();
            asset.Vertices = new FVgVertex[r.ReadInt32()];
            for (int i = 0; i < asset.Vertices.Length; i++)
            {
                FVgVertex v;
                v.TexelX = r.ReadUInt16();
                v.TexelZ = r.ReadUInt16();
                v.Y = r.ReadSingle();
                asset.Vertices[i] = v;
            }
            asset.Indices = new uint[r.ReadInt32()];
            for (int i = 0; i < asset.Indices.Length; i++)
                asset.Indices[i] = r.ReadUInt32();
            asset.Clusters = new TtVgCluster[r.ReadInt32()];
            for (int i = 0; i < asset.Clusters.Length; i++)
            {
                var c = new TtVgCluster();
                c.Level = r.ReadInt32();
                c.MinTexelX = r.ReadInt32();
                c.MinTexelZ = r.ReadInt32();
                c.ExtentTexel = r.ReadInt32();
                c.FirstChild = r.ReadInt32();
                c.BaseVertex = r.ReadInt32();
                c.IndexStart = r.ReadInt32();
                c.IndexCount = r.ReadInt32();
                c.MinY = r.ReadSingle();
                c.MaxY = r.ReadSingle();
                c.MaxError = r.ReadSingle();
                asset.Clusters[i] = c;
            }
            return asset;
        }
        public static TtVgDagAsset FromBytes(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var r = new BinaryReader(ms))
            {
                return Load(r);
            }
        }
    }

    /// <summary>
    /// 接缝三硬约定的校验器(纯 CPU, 供 bake 自检与单元测试使用):
    /// ① Level 外圈锁边顶点 == 原始高度场采样;
    /// ② 全局对角约定(bake 时强制, 此处校验共享边剖分一致性);
    /// ③ 锁边顶点高度逐位一致(xz 由格点编码保证, y 由本文件校验)。
    /// </summary>
    public static class FVgDagSeamValidator
    {
        /// <summary>根 cluster 锁边环上的顶点必须逐位等于原始高度场采样。</summary>
        public static bool CheckLevelBorderLocked(TtVgDagAsset asset, float[] heightfield, out string error)
        {
            error = null;
            int size = asset.HeightfieldSize;
            for (int ci = 0; ci < asset.Clusters.Length; ci++)
            {
                var c = asset.Clusters[ci];
                if (c.IsLeaf)
                    continue;
                if (c.Level != asset.LevelCount - 1)
                    continue;
                for (int ii = 0; ii < c.IndexCount; ii++)
                {
                    var v = asset.Vertices[c.BaseVertex + (int)asset.Indices[c.IndexStart + ii]];
                    bool onBorder = v.TexelX == c.MinTexelX || v.TexelX == c.MinTexelX + c.ExtentTexel ||
                                    v.TexelZ == c.MinTexelZ || v.TexelZ == c.MinTexelZ + c.ExtentTexel;
                    if (onBorder == false)
                        continue;
                    float src = heightfield[v.TexelZ * size + v.TexelX];
                    if (src != v.Y)
                    {
                        error = $"root cluster[{ci}] border vert({v.TexelX},{v.TexelZ}) y={v.Y} != source {src}";
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>收集 cluster 的边界边链顶点下标(无向边引用次数 != 2 的边的端点)。
        /// 简化后的网格可能有顶点恰好落在切分线上但只被单侧 cluster 内部引用,
        /// 它不属于任何一侧的边界链, 不构成 T-junction, 不应参与接缝比对。</summary>
        static void CollectClusterBorderVerts(TtVgDagAsset asset, TtVgCluster c, HashSet<int> outBorderVerts)
        {
            var edgeUse = new Dictionary<long, int>();
            int triCount = c.IndexCount / 3;
            for (int t = 0; t < triCount; t++)
            {
                int b = c.IndexStart + t * 3;
                for (int k = 0; k < 3; k++)
                {
                    uint u = asset.Indices[b + k];
                    uint v = asset.Indices[b + (k + 1) % 3];
                    long key = u < v ? ((long)u << 32) | v : ((long)v << 32) | u;
                    int n;
                    edgeUse.TryGetValue(key, out n);
                    edgeUse[key] = n + 1;
                }
            }
            foreach (var kv in edgeUse)
            {
                if (kv.Value == 2)
                    continue; // 内部边
                outBorderVerts.Add((int)(kv.Key >> 32));
                outBorderVerts.Add((int)(kv.Key & 0xffffffff));
            }
        }

        /// <summary>收集 cluster 边界链上落在给定 texelX 竖线上的顶点, 打包为 (texelZ<<32)|yBitPattern。</summary>
        static void CollectBorderVertsOnX(TtVgDagAsset asset, TtVgCluster c, int texelX, List<long> outZY)
        {
            var borderVerts = new HashSet<int>();
            CollectClusterBorderVerts(asset, c, borderVerts);
            for (int ii = 0; ii < c.IndexCount; ii++)
            {
                int vi = c.BaseVertex + (int)asset.Indices[c.IndexStart + ii];
                if (borderVerts.Contains(vi - c.BaseVertex) == false)
                    continue;
                var v = asset.Vertices[vi];
                if (v.TexelX == texelX)
                    outZY.Add(((long)v.TexelZ << 32) | (uint)BitConverter.SingleToInt32Bits(v.Y));
            }
        }
        /// <summary>收集 cluster 边界链上落在给定 texelZ 横线上的顶点, 打包为 (texelX<<32)|yBitPattern。</summary>
        static void CollectBorderVertsOnZ(TtVgDagAsset asset, TtVgCluster c, int texelZ, List<long> outXZ)
        {
            var borderVerts = new HashSet<int>();
            CollectClusterBorderVerts(asset, c, borderVerts);
            for (int ii = 0; ii < c.IndexCount; ii++)
            {
                int vi = c.BaseVertex + (int)asset.Indices[c.IndexStart + ii];
                if (borderVerts.Contains(vi - c.BaseVertex) == false)
                    continue;
                var v = asset.Vertices[vi];
                if (v.TexelZ == texelZ)
                    outXZ.Add(((long)v.TexelX << 32) | (uint)BitConverter.SingleToInt32Bits(v.Y));
            }
        }
        static bool SamePackedSet(List<long> a, List<long> b)
        {
            if (a.Count != b.Count)
                return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
        /// <summary>去重 + 排序: 键边比对语义是"顶点集合一致", 与引用次数无关。</summary>
        static void NormalizePacked(List<long> l)
        {
            l.Sort();
            int w = 0;
            for (int i = 0; i < l.Count; i++)
            {
                if (w == 0 || l[w - 1] != l[i])
                    l[w++] = l[i];
            }
            l.RemoveRange(w, l.Count - w);
        }

        /// <summary>收集 cluster 的无向边集(全局顶点池下标打包 key)。</summary>
        static void CollectClusterEdges(TtVgDagAsset asset, TtVgCluster c, HashSet<long> outEdges)
        {
            int triCount = c.IndexCount / 3;
            for (int t = 0; t < triCount; t++)
            {
                int b = c.IndexStart + t * 3;
                for (int k = 0; k < 3; k++)
                {
                    uint u = asset.Indices[b + k];
                    uint v = asset.Indices[b + (k + 1) % 3];
                    outEdges.Add(u < v ? ((long)u << 32) | v : ((long)v << 32) | u);
                }
            }
        }

        /// <summary>
        /// 同层相邻 cluster 的共享接口校验(Quark 式切分架构下的 T-junction 判定):
        /// 两 cluster 的共享接口 = 本 cluster 边界边链 ∩ 对方边集(全局顶点池下标相同
        /// 才视为同一条边); 接口两侧取到的边链与顶点(格点坐标+y 位型)必须逐位一致。
        /// 注: 简化后 cluster 可能引用超出自身范围的远处顶点(大跨度三角形),
        /// 因此不能再用"几何切分线上的顶点集合"比对, 必须按真实接口边链判定。
        /// </summary>
        public static bool CheckAdjacentClusterSeams(TtVgDagAsset asset, out string error)
        {
            error = null;
            for (int level = 0; level < asset.LevelCount; level++)
            {
                int n = asset.ClustersPerSide(level);
                if (n < 2)
                    continue;
                for (int cz = 0; cz < n; cz++)
                {
                    for (int cx = 0; cx + 1 < n; cx++)
                    {
                        var left = asset.GetCluster(level, cx, cz);
                        var right = asset.GetCluster(level, cx + 1, cz);
                        if (left.IsLeaf || right.IsLeaf)
                            continue; // 叶子是隐式规则网格, 天然一致
                        int edgeX = left.MinTexelX + left.ExtentTexel;
                        var la = new List<long>();
                        var ra = new List<long>();
                        CollectInterfaceVertsOnX(asset, left, right, edgeX, la);
                        CollectInterfaceVertsOnX(asset, right, left, edgeX, ra);
                        NormalizePacked(la);
                        NormalizePacked(ra);
                        if (SamePackedSet(la, ra) == false)
                        {
                            var only = new List<long>();
                            foreach (var e in la)
                                if (ra.Contains(e) == false) only.Add(e);
                            foreach (var e in ra)
                                if (la.Contains(e) == false) only.Add(e);
                            var sb = new System.Text.StringBuilder();
                            foreach (var e in only)
                                sb.Append($"(z={e >> 32},yBits={e & 0xffffffff:x8}) ");
                            error = $"level{level} cluster({cx},{cz})|({cx + 1},{cz}) X-edge vert sets differ ({la.Count} vs {ra.Count}) diff: {sb}";
                            return false;
                        }
                    }
                }
                for (int cx = 0; cx < n; cx++)
                {
                    for (int cz = 0; cz + 1 < n; cz++)
                    {
                        var up = asset.GetCluster(level, cx, cz);
                        var down = asset.GetCluster(level, cx, cz + 1);
                        if (up.IsLeaf || down.IsLeaf)
                            continue;
                        int edgeZ = up.MinTexelZ + up.ExtentTexel;
                        var ua = new List<long>();
                        var da = new List<long>();
                        CollectInterfaceVertsOnZ(asset, up, down, edgeZ, ua);
                        CollectInterfaceVertsOnZ(asset, down, up, edgeZ, da);
                        NormalizePacked(ua);
                        NormalizePacked(da);
                        if (SamePackedSet(ua, da) == false)
                        {
                            error = $"level{level} cluster({cx},{cz})|({cx},{cz + 1}) Z-edge vert sets differ ({ua.Count} vs {da.Count})";
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>收集 a 与 b 的共享接口上位于 texelX 竖线的顶点(打包 (texelZ<<32)|yBitPattern)。
        /// 接口 = a 的边界边链 ∩ b 的边集(顶点池下标相同才算同一条边)。</summary>
        static void CollectInterfaceVertsOnX(TtVgDagAsset asset, TtVgCluster a, TtVgCluster b, int texelX, List<long> outZY)
        {
            var edgesB = new HashSet<long>();
            CollectClusterEdges(asset, b, edgesB);
            var edgeUseA = new Dictionary<long, int>();
            CountClusterEdgeUse(asset, a, edgeUseA);
            foreach (var kv in edgeUseA)
            {
                if (kv.Value == 2)
                    continue; // a 的内部边
                if (edgesB.Contains(kv.Key) == false)
                    continue; // 不与 b 共享的边界边
                uint u = (uint)(kv.Key >> 32);
                uint v = (uint)(kv.Key & 0xffffffff);
                CheckInterfaceVertOnX(asset, a, (int)u, texelX, outZY);
                CheckInterfaceVertOnX(asset, a, (int)v, texelX, outZY);
            }
        }

        static void CheckInterfaceVertOnX(TtVgDagAsset asset, TtVgCluster c, int localVi, int texelX, List<long> outZY)
        {
            var v = asset.Vertices[c.BaseVertex + localVi];
            if (v.TexelX == texelX)
                outZY.Add(((long)v.TexelZ << 32) | (uint)BitConverter.SingleToInt32Bits(v.Y));
        }

        /// <summary>收集 a 与 b 的共享接口上位于 texelZ 横线的顶点(打包 (texelX<<32)|yBitPattern)。</summary>
        static void CollectInterfaceVertsOnZ(TtVgDagAsset asset, TtVgCluster a, TtVgCluster b, int texelZ, List<long> outXZ)
        {
            var edgesB = new HashSet<long>();
            CollectClusterEdges(asset, b, edgesB);
            var edgeUseA = new Dictionary<long, int>();
            CountClusterEdgeUse(asset, a, edgeUseA);
            foreach (var kv in edgeUseA)
            {
                if (kv.Value == 2)
                    continue;
                if (edgesB.Contains(kv.Key) == false)
                    continue;
                uint u = (uint)(kv.Key >> 32);
                uint v = (uint)(kv.Key & 0xffffffff);
                CheckInterfaceVertOnZ(asset, a, (int)u, texelZ, outXZ);
                CheckInterfaceVertOnZ(asset, a, (int)v, texelZ, outXZ);
            }
        }

        static void CheckInterfaceVertOnZ(TtVgDagAsset asset, TtVgCluster c, int localVi, int texelZ, List<long> outXZ)
        {
            var v = asset.Vertices[c.BaseVertex + localVi];
            if (v.TexelZ == texelZ)
                outXZ.Add(((long)v.TexelX << 32) | (uint)BitConverter.SingleToInt32Bits(v.Y));
        }

        /// <summary>统计 cluster 内无向边引用次数(顶点池下标打包 key)。</summary>
        static void CountClusterEdgeUse(TtVgDagAsset asset, TtVgCluster c, Dictionary<long, int> outEdgeUse)
        {
            int triCount = c.IndexCount / 3;
            for (int t = 0; t < triCount; t++)
            {
                int b = c.IndexStart + t * 3;
                for (int k = 0; k < 3; k++)
                {
                    uint u = asset.Indices[b + k];
                    uint v = asset.Indices[b + (k + 1) % 3];
                    long key = u < v ? ((long)u << 32) | v : ((long)v << 32) | u;
                    int n;
                    outEdgeUse.TryGetValue(key, out n);
                    outEdgeUse[key] = n + 1;
                }
            }
        }

        /// <summary>
        /// 相邻两块 heightfield 各自 bake 后, 根 cluster 面向对方的边框顶点
        /// 必须逐位重合(接缝硬约定①③)。调用方需保证两块高度场共享边列/行的
        /// 采样值逐位一致(由 PGC 确定性生成 + overlay 边缘联动保证)。
        /// bIsEastOfA: b 位于 a 的 +X 方向; 否则 b 位于 a 的 +Z 方向。
        /// </summary>
        public static bool CheckAdjacentLevelSeam(TtVgDagAsset a, TtVgDagAsset b, bool bIsEastOfA, out string error)
        {
            error = null;
            var rootA = a.Clusters[a.Clusters.Length - 1];
            var rootB = b.Clusters[b.Clusters.Length - 1];
            var ea = new List<long>();
            var eb = new List<long>();
            if (bIsEastOfA)
            {
                int edgeX = a.HeightfieldSize - 1;
                CollectBorderVertsOnX(a, rootA, edgeX, ea);
                CollectBorderVertsOnX(b, rootB, 0, eb);
            }
            else
            {
                int edgeZ = a.HeightfieldSize - 1;
                CollectBorderVertsOnZ(a, rootA, edgeZ, ea);
                CollectBorderVertsOnZ(b, rootB, 0, eb);
            }
            NormalizePacked(ea);
            NormalizePacked(eb);
            if (SamePackedSet(ea, eb) == false)
            {
                var only = new List<long>();
                foreach (var e in ea)
                    if (eb.Contains(e) == false) only.Add(e);
                foreach (var e in eb)
                    if (ea.Contains(e) == false) only.Add(e);
                var sb = new System.Text.StringBuilder();
                foreach (var e in only)
                    sb.Append($"(x={e >> 32},yBits={e & 0xffffffff:x8}) ");
                error = $"adjacent level root seam differs ({ea.Count} vs {eb.Count} border verts) diff: {sb}";
                return false;
            }
            return true;
        }
    }
}
