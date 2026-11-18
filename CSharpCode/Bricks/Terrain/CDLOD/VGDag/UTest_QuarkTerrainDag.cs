using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using EngineNS.UnitTest;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// 地形 Quark cluster DAG (TtQuarkTerrainBaker → TtVgDagAsset) 验收。顶点全在 texel 格点上, xz 判定全部 int64 精确:
    ///   每个 mip 层与若干一致 cut (按 group 误差阈值 τ 选 cluster, 与运行时规则相同) 都必须是完整 heightfield 网格:
    ///     y 位级等于源采样; 内部有向边正反各恰好一次; 外圈开边数 = 叶子网格外圈边数且绕向一致;
    ///     Euler = 1; xz 有向面积全为正且 Σ2A = 2(n-1)²; cell 中心按一致 fill rule 恰好覆盖一次。
    ///   误差: 相对全分辨率 f0 的精确竖直 L∞ (顶点 + 三角形内格点 + 边与 x=i / z=j / x-z=c 交点),
    ///     逐 cluster ≤ 声明 LODError (保守), 且 group 误差不超过实测 + 舍入 padding (紧致, 不虚高)。
    ///   另测: 确定性、序列化往返、损坏数据拒绝、取消 (原生/前置/中途)、进度回调不影响产物。
    /// </summary>
    [TtTest(Enable = false)] // 启动时全量跑耗时过长, 默认关闭; 需要回归时临时改回 true 再 RunTests
    public class UTest_TtQuarkTerrainDag
    {
        #region heightfields
        static uint Hash2D(int x, int z, int seed, int period)
        {
            uint h = 2166136261u ^ (uint)seed ^ (uint)period;
            h = (h ^ (uint)x) * 16777619u;
            h = (h ^ (uint)(x >> 16)) * 16777619u;
            h = (h ^ (uint)z) * 16777619u;
            h = (h ^ (uint)(z >> 16)) * 16777619u;
            h ^= h >> 15;
            h *= 0x2545f491u;
            h ^= h >> 13;
            return h;
        }
        static float HashToUnit(uint h)
        {
            return (h & 0x00ffffffu) / 16777215.0f;
        }
        static float ValueNoise(int x, int z, int seed, int period)
        {
            int x0 = x / period, z0 = z / period;
            float fx = (x % period) / (float)period;
            float fz = (z % period) / (float)period;
            float v00 = HashToUnit(Hash2D(x0, z0, seed, period));
            float v10 = HashToUnit(Hash2D(x0 + 1, z0, seed, period));
            float v01 = HashToUnit(Hash2D(x0, z0 + 1, seed, period));
            float v11 = HashToUnit(Hash2D(x0 + 1, z0 + 1, seed, period));
            float a = v00 + (v10 - v00) * fx;
            float b = v01 + (v11 - v01) * fx;
            return a + (b - a) * fz;
        }
        /// <summary>确定性多倍频值噪声高度场 (不依赖引擎运行时)。</summary>
        internal static float[] GenHeightfield(int size, int seed, float scale = 1.0f)
        {
            var hf = new float[size * size];
            int[] periods = new int[] { 8, 16, 32, 64 };
            float[] amps = new float[] { 40f, 18f, 8f, 3f };
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = 0.0f;
                    for (int o = 0; o < periods.Length; o++)
                    {
                        if (periods[o] >= size * 2)
                            continue;
                        h += (ValueNoise(x, z, seed + o, periods[o]) - 0.5f) * 2.0f * amps[o];
                    }
                    hf[z * size + x] = h * scale;
                }
            }
            return hf;
        }
        static float[] GenAnalytic(int size, Func<double, double, double> f)
        {
            var hf = new float[size * size];
            double c = (size - 1) * 0.5;
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                    hf[z * size + x] = (float)f(x - c, z - c);
            }
            return hf;
        }
        static float[] GenFlat(int size) => GenAnalytic(size, (x, z) => 3.5);
        static float[] GenPeak(int size)
        {
            double sigma = size / 6.0;
            return GenAnalytic(size, (x, z) => 60.0 * Math.Exp(-(x * x + z * z) / (2.0 * sigma * sigma)));
        }
        static float[] GenSaddle(int size)
        {
            double c = (size - 1) * 0.5;
            return GenAnalytic(size, (x, z) => (x * x - z * z) * 40.0 / (c * c));
        }
        /// <summary>噪声叠加一道 30 单位的竖直断崖, 考察陡变处的误差界与拓扑。</summary>
        static float[] GenCliff(int size)
        {
            var hf = GenHeightfield(size, 11, 0.25f);
            for (int z = 0; z < size; z++)
            {
                for (int x = size / 2; x < size; x++)
                    hf[z * size + x] += 30.0f;
            }
            return hf;
        }
        #endregion

        class FMeshStats
        {
            public string Name;
            public int Clusters;
            public int Tris;
            public int StoredVerts;
            public float DeclaredError;
            public double MaxDy;
            public double RmsDy;
            public int YNotSource;
            public int PosTris;
            public int NegTris;
            public int ZeroVertical;
            public int Zero3D;
            public long Sum2A;
            public int CoverZero;
            public int CoverMulti;
            public int OpenBorder;
            public int BorderBadWinding;
            public int BorderYMismatch;
            public int OpenInterior;
            public int DupDirected;
            public long Euler;
        }

        static long FloorDiv(long a, long b)
        {
            long q = a / b;
            if (a % b != 0 && ((a < 0) != (b < 0)))
                q--;
            return q;
        }

        static bool OnBorder(long x, long z, int size)
        {
            return x == 0 || z == 0 || x == size - 1 || z == size - 1;
        }

        /// <summary>位级顶点 key: 格点下标 + y 位型。</summary>
        static long VertKey(long x, long z, float y, int size)
        {
            return ((z * size + x) << 32) | (uint)BitConverter.SingleToInt32Bits(y);
        }

        /// <summary>fill rule 的平局判定: 等价于把采样点沿固定方向做无穷小扰动, 共享边/顶点恰好归属一侧。</summary>
        static bool TieInclude(long dx, long dz)
        {
            return dz > 0 || (dz == 0 && dx < 0);
        }

        /// <summary>
        /// 度量 clusters 组成的网格; clusterMaxDy 非空时按全局 cluster 下标记录实测竖直误差。
        /// </summary>
        static FMeshStats MeasureMesh(string name, TtVgDagAsset asset, List<int> clusters, float[] hf, double[] clusterMaxDy)
        {
            var st = new FMeshStats() { Name = name, Clusters = clusters.Count };
            int n = asset.HeightfieldSize;
            int cells = n - 1;
            var vertKeys = new HashSet<long>();
            var directed = new Dictionary<(long, long), int>();
            var coverage = new int[cells * cells];
            double maxDy = 0.0, sumSq = 0.0;
            long samples = 0;
            var X = new long[3];
            var Z = new long[3];
            var Y = new float[3];
            var K = new long[3];

            double H(long x, long z)
            {
                return hf[z * n + x];
            }

            foreach (int ci in clusters)
            {
                var cl = asset.Clusters[ci];
                st.DeclaredError = Math.Max(st.DeclaredError, cl.LODError);
                st.StoredVerts += cl.VertexCount;
                st.Tris += cl.IndexCount / 3;
                double localMax = 0.0;
                void Track(double dy)
                {
                    if (dy > localMax)
                        localMax = dy;
                }

                for (int t = 0; t < cl.IndexCount; t += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        var v = asset.Vertices[cl.BaseVertex + asset.Indices[cl.IndexStart + t + k]];
                        X[k] = v.TexelX; Z[k] = v.TexelZ; Y[k] = v.Y;
                        K[k] = VertKey(X[k], Z[k], Y[k], n);
                    }
                    for (int k = 0; k < 3; k++)
                    {
                        vertKeys.Add(K[k]);
                        float src = hf[Z[k] * n + X[k]];
                        if (BitConverter.SingleToInt32Bits(Y[k]) != BitConverter.SingleToInt32Bits(src))
                        {
                            st.YNotSource++;
                            if (OnBorder(X[k], Z[k], n))
                                st.BorderYMismatch++;
                        }
                        Track(Math.Abs((double)Y[k] - src));
                        var ek = (K[k], K[(k + 1) % 3]);
                        directed.TryGetValue(ek, out int ec);
                        directed[ek] = ec + 1;
                    }

                    long a2 = (X[1] - X[0]) * (Z[2] - Z[0]) - (X[2] - X[0]) * (Z[1] - Z[0]);
                    st.Sum2A += a2;
                    if (a2 == 0)
                    {
                        // xz 共线: 3D 叉积也为零才是真退化, 否则是竖直面片 (heightfield 被破坏)
                        double ux = X[1] - X[0], uy = (double)Y[1] - Y[0], uz = Z[1] - Z[0];
                        double vx = X[2] - X[0], vy = (double)Y[2] - Y[0], vz = Z[2] - Z[0];
                        double cx = uy * vz - uz * vy, cy = uz * vx - ux * vz, cz = ux * vy - uy * vx;
                        if (cx == 0.0 && cy == 0.0 && cz == 0.0)
                            st.Zero3D++;
                        else
                            st.ZeroVertical++;
                        continue;
                    }
                    if (a2 > 0)
                        st.PosTris++;
                    else
                        st.NegTris++;

                    // 边与三族网格线的非格点交点 (格点由下面的内点扫描覆盖)
                    for (int k = 0; k < 3; k++)
                    {
                        long x0 = X[k], z0 = Z[k], x1 = X[(k + 1) % 3], z1 = Z[(k + 1) % 3];
                        double y0 = Y[k], y1 = Y[(k + 1) % 3];
                        long dx = x1 - x0, dz = z1 - z0;
                        if (dx != 0)
                        {
                            for (long i = Math.Min(x0, x1) + 1; i < Math.Max(x0, x1); i++)
                            {
                                long num = (i - x0) * dz;
                                if (num % dx == 0)
                                    continue;
                                long j = z0 + FloorDiv(num, dx);
                                double fz = z0 + (double)num / dx - j;
                                double f0 = H(i, j) + (H(i, j + 1) - H(i, j)) * fz;
                                Track(Math.Abs(y0 + (y1 - y0) * ((double)(i - x0) / dx) - f0));
                            }
                        }
                        if (dz != 0)
                        {
                            for (long j = Math.Min(z0, z1) + 1; j < Math.Max(z0, z1); j++)
                            {
                                long num = (j - z0) * dx;
                                if (num % dz == 0)
                                    continue;
                                long i = x0 + FloorDiv(num, dz);
                                double fx = x0 + (double)num / dz - i;
                                double f0 = H(i, j) + (H(i + 1, j) - H(i, j)) * fx;
                                Track(Math.Abs(y0 + (y1 - y0) * ((double)(j - z0) / dz) - f0));
                            }
                        }
                        long u0 = x0 - z0, u1 = x1 - z1, du = u1 - u0;
                        if (du != 0)
                        {
                            for (long c = Math.Min(u0, u1) + 1; c < Math.Max(u0, u1); c++)
                            {
                                long num = (c - u0) * dx;
                                if (num % du == 0)
                                    continue;
                                long i = x0 + FloorDiv(num, du);
                                double fx = x0 + (double)num / du - i;
                                double f0 = H(i, i - c) + (H(i + 1, i + 1 - c) - H(i, i - c)) * fx;
                                Track(Math.Abs(y0 + (y1 - y0) * ((double)(c - u0) / du) - f0));
                            }
                        }
                    }

                    // 统一成正向再做内点判定 (翻面三角形同样计入覆盖, 用于发现重叠)
                    long ax = X[0], az = Z[0], bx = X[1], bz = Z[1], cxx = X[2], czz = Z[2];
                    double ay = Y[0], by = Y[1], cy2 = Y[2];
                    if (a2 < 0)
                    {
                        bx = X[2]; bz = Z[2]; by = Y[2];
                        cxx = X[1]; czz = Z[1]; cy2 = Y[1];
                        a2 = -a2;
                    }
                    long minX = Math.Min(ax, Math.Min(bx, cxx)), maxX = Math.Max(ax, Math.Max(bx, cxx));
                    long minZ = Math.Min(az, Math.Min(bz, czz)), maxZ = Math.Max(az, Math.Max(bz, czz));

                    // 闭三角形内的格点
                    for (long pz = minZ; pz <= maxZ; pz++)
                    {
                        for (long px = minX; px <= maxX; px++)
                        {
                            long e0 = (cxx - bx) * (pz - bz) - (czz - bz) * (px - bx);
                            long e1 = (ax - cxx) * (pz - czz) - (az - czz) * (px - cxx);
                            long e2 = (bx - ax) * (pz - az) - (bz - az) * (px - ax);
                            if (e0 < 0 || e1 < 0 || e2 < 0)
                                continue;
                            double yi = (e0 * ay + e1 * by + e2 * cy2) / a2;
                            Track(Math.Abs(yi - H(px, pz)));
                        }
                    }

                    // cell 中心 (i+0.5, j+0.5), 坐标 ×2 后全整数; 中心落在叶子对角线 a→c 上
                    long AX = ax * 2, AZ = az * 2, BX = bx * 2, BZ = bz * 2, CX = cxx * 2, CZ = czz * 2;
                    long a4 = a2 * 4;
                    for (long j = minZ; j < maxZ; j++)
                    {
                        for (long i = minX; i < maxX; i++)
                        {
                            long px = i * 2 + 1, pz = j * 2 + 1;
                            long e0 = (CX - BX) * (pz - BZ) - (CZ - BZ) * (px - BX);
                            long e1 = (AX - CX) * (pz - CZ) - (AZ - CZ) * (px - CX);
                            long e2 = (BX - AX) * (pz - AZ) - (BZ - AZ) * (px - AX);
                            if (e0 < 0 || (e0 == 0 && TieInclude(CX - BX, CZ - BZ) == false))
                                continue;
                            if (e1 < 0 || (e1 == 0 && TieInclude(AX - CX, AZ - CZ) == false))
                                continue;
                            if (e2 < 0 || (e2 == 0 && TieInclude(BX - AX, BZ - AZ) == false))
                                continue;
                            coverage[j * cells + i]++;
                            double yi = (e0 * ay + e1 * by + e2 * cy2) / a4;
                            double f0 = (H(i, j) + H(i + 1, j + 1)) * 0.5;
                            double dy = Math.Abs(yi - f0);
                            Track(dy);
                            sumSq += dy * dy;
                            samples++;
                        }
                    }
                }
                maxDy = Math.Max(maxDy, localMax);
                if (clusterMaxDy != null)
                    clusterMaxDy[ci] = localMax;
            }

            foreach (var c in coverage)
            {
                if (c == 0)
                    st.CoverZero++;
                else if (c > 1)
                    st.CoverMulti++;
            }

            long undirected = 0;
            foreach (var kv in directed)
            {
                var (u, v) = kv.Key;
                if (kv.Value > 1)
                    st.DupDirected += kv.Value - 1;
                bool hasReverse = directed.ContainsKey((v, u));
                if (hasReverse == false || u < v)
                    undirected++;
                if (hasReverse)
                    continue;
                long ui = u >> 32, vi = v >> 32;
                long ux = ui % n, uz = ui / n, vx = vi % n, vz = vi / n;
                bool border = true;
                bool goodWinding;
                if (uz == 0 && vz == 0)
                    goodWinding = vx > ux;
                else if (ux == n - 1 && vx == n - 1)
                    goodWinding = vz > uz;
                else if (uz == n - 1 && vz == n - 1)
                    goodWinding = vx < ux;
                else if (ux == 0 && vx == 0)
                    goodWinding = vz < uz;
                else
                {
                    border = false;
                    goodWinding = true;
                }
                if (border)
                {
                    st.OpenBorder++;
                    if (goodWinding == false)
                        st.BorderBadWinding++;
                }
                else
                    st.OpenInterior++;
            }
            st.Euler = vertKeys.Count - undirected + st.Tris;
            st.MaxDy = maxDy;
            st.RmsDy = samples > 0 ? Math.Sqrt(sumSq / samples) : 0.0;
            return st;
        }

        /// <summary>叶子网格行列数: {0, step, 2step, …} ∪ {size-1}。</summary>
        static int LeafSamples(int size, int step)
        {
            return (size - 1) / step + 1 + ((size - 1) % step != 0 ? 1 : 0);
        }

        static void CheckMesh(string tag, FMeshStats s, int size, int borderEdges, List<string> failures)
        {
            long expect2A = 2L * (size - 1) * (size - 1);
            var bad = new List<string>();
            if (s.YNotSource != 0) bad.Add($"yNotSource={s.YNotSource}");
            if (s.NegTris != 0) bad.Add($"flipped={s.NegTris}");
            if (s.ZeroVertical != 0) bad.Add($"vertical={s.ZeroVertical}");
            if (s.Zero3D != 0) bad.Add($"degenerate3D={s.Zero3D}");
            if (s.Sum2A != expect2A) bad.Add($"sum2A={s.Sum2A}/{expect2A}");
            if (s.CoverZero != 0 || s.CoverMulti != 0) bad.Add($"cover0={s.CoverZero} coverN={s.CoverMulti}");
            if (s.OpenBorder != borderEdges || s.BorderBadWinding != 0 || s.BorderYMismatch != 0) bad.Add($"border={s.OpenBorder}/{borderEdges} badWinding={s.BorderBadWinding} yMismatch={s.BorderYMismatch}");
            if (s.OpenInterior != 0 || s.DupDirected != 0) bad.Add($"innerOpen={s.OpenInterior} dupDirected={s.DupDirected}");
            if (s.Euler != 1) bad.Add($"euler={s.Euler}");
            if (bad.Count > 0)
                failures.Add($"{tag} {s.Name}: {string.Join(" ", bad)}");
        }

        static void AppendStats(StringBuilder sb, FMeshStats s, long expect2A)
        {
            sb.AppendLine($"    {s.Name,-14} {s.Clusters,5} {s.Tris,7} {s.StoredVerts,7} {s.DeclaredError,8:F4} {s.MaxDy,8:F4} {s.RmsDy,7:F4} | {s.PosTris,6} {s.NegTris,4} {s.ZeroVertical,5} {s.Zero3D,3} {s.Sum2A - expect2A,7} {s.CoverZero,4} {s.CoverMulti,4} | {s.OpenBorder,5} {s.BorderBadWinding,5} {s.BorderYMismatch,5} {s.OpenInterior,5} {s.DupDirected,6} {s.Euler,5} {s.YNotSource,7}");
        }

        /// <summary>运行时同款组一致选择 (阈值为纯误差, 省去投影)。</summary>
        static List<int> SelectCut(TtVgDagAsset a, float tau)
        {
            var result = new List<int>();
            for (int c = 0; c < a.Clusters.Length; c++)
            {
                var cl = a.Clusters[c];
                bool fineEnough = cl.GeneratingGroup < 0 || a.Groups[cl.GeneratingGroup].ParentLODError <= tau;
                bool parentTooCoarse = cl.ParentGroup < 0 || a.Groups[cl.ParentGroup].ParentLODError > tau;
                if (fineEnough && parentTooCoarse)
                    result.Add(c);
            }
            return result;
        }

        /// <summary>原生 RoundUpError 的上界镜像: 实测 + 两倍 padding 后向上取整到 float。</summary>
        static float RoundUpBound(double measured, double pad)
        {
            double v = measured + pad * 2.0;
            if (v == 0.0)
                return 0.0f;
            float f = (float)v;
            if (f < v)
                f = MathF.BitIncrement(f);
            return MathF.BitIncrement(f);
        }

        static ulong Fnv64(byte[] data)
        {
            ulong h = 14695981039346656037ul;
            foreach (var b in data)
            {
                h ^= b;
                h *= 1099511628211ul;
            }
            return h;
        }

        static TtVgDagAsset Bake(float[] hf, int size, int step, Action<string> report = null)
        {
            using (var baker = new TtQuarkTerrainBaker())
                return baker.Bake(hf, size, step, report);
        }

        static bool SameBytes(byte[] a, byte[] b)
        {
            return a.AsSpan().SequenceEqual(b);
        }

        static void RunCase(string tag, float[] hf, int size, int step, List<string> failures)
        {
            var sb = new StringBuilder();
            var sw = Stopwatch.StartNew();
            TtVgDagAsset asset;
            try
            {
                asset = Bake(hf, size, step);
            }
            catch (Exception e)
            {
                failures.Add($"{tag}: bake threw {e.GetType().Name}: {e.Message}");
                return;
            }
            sw.Stop();
            if (asset == null)
            {
                failures.Add($"{tag}: bake returned null without cancel");
                return;
            }
            var bytes = asset.ToBytes();
            var bytes2 = Bake(hf, size, step).ToBytes();
            if (!SameBytes(bytes, bytes2))
                failures.Add($"{tag}: bake not deterministic");
            if (!SameBytes(bytes, TtVgDagAsset.FromBytes(bytes).ToBytes()))
                failures.Add($"{tag}: serialization round trip differs");

            int leafSamples = LeafSamples(size, step);
            int borderEdges = 4 * (leafSamples - 1);
            long expect2A = 2L * (size - 1) * (size - 1);
            double maxAbs = 0.0;
            foreach (var h in hf)
                maxAbs = Math.Max(maxAbs, Math.Abs((double)h));
            double pad = Math.ScaleB(maxAbs, -40);

            sb.AppendLine($"== {tag}: {size}x{size} step={step} ({leafSamples}^2 leaf samples), {sw.Elapsed.TotalMilliseconds:F1} ms, mips={asset.MipLevelCount}, clusters={asset.Clusters.Length}, groups={asset.Groups.Length}, verts={asset.Vertices.Length}, bytes={bytes.Length}, fnv64={Fnv64(bytes):x16} ==");
            sb.AppendLine($"    (expect: borderOpen={borderEdges}, Σ2A={expect2A}, euler=1)");
            sb.AppendLine("    mesh            clus    tris   verts  declErr    maxDy   rmsDy |    pos  neg zVert z3D Σ2A-exp cov0 covN | bOpen bBadW bYmis iOpen dupDir euler yNotSrc");

            // 逐层: 完整网格 + 实测每 cluster 误差
            var clusterMaxDy = new double[asset.Clusters.Length];
            var levels = new List<FMeshStats>();
            for (int level = 0; level < asset.MipLevelCount; level++)
            {
                var list = new List<int>();
                for (int c = 0; c < asset.Clusters.Length; c++)
                {
                    if (asset.Clusters[c].MipLevel == level)
                        list.Add(c);
                }
                var st = MeasureMesh($"mip {level}", asset, list, hf, clusterMaxDy);
                levels.Add(st);
                AppendStats(sb, st, expect2A);
                CheckMesh(tag, st, size, borderEdges, failures);
                if (level > 0 && (st.Tris >= levels[level - 1].Tris || st.Clusters >= levels[level - 1].Clusters))
                    failures.Add($"{tag} mip {level}: stalled tris {levels[level - 1].Tris}->{st.Tris} clusters {levels[level - 1].Clusters}->{st.Clusters}");
            }
            if (levels[levels.Count - 1].Clusters > (int)TtQuarkTerrainBaker.MaxGroupSize)
                failures.Add($"{tag}: top mip has {levels[levels.Count - 1].Clusters} clusters (> MaxGroupSize, DAG stalled)");

            // 保守: 实测 ≤ 声明 (无额外容差); 紧致: 声明不超过实测 + 舍入 padding
            int overErr = 0, loose = 0;
            double worstRatio = 0.0;
            for (int c = 0; c < asset.Clusters.Length; c++)
            {
                var cl = asset.Clusters[c];
                if (clusterMaxDy[c] > cl.LODError)
                {
                    if (overErr++ < 4)
                        failures.Add($"{tag}: cluster[{c}] mip {cl.MipLevel} measured {clusterMaxDy[c]:R} > LODError {cl.LODError:R}");
                }
                if (cl.LODError > 0.0f)
                    worstRatio = Math.Max(worstRatio, clusterMaxDy[c] / cl.LODError);
                if (cl.GeneratingGroup < 0 && cl.LODError > RoundUpBound(clusterMaxDy[c], pad))
                {
                    if (loose++ < 4)
                        failures.Add($"{tag}: leaf cluster[{c}] LODError {cl.LODError:R} looser than measured {clusterMaxDy[c]:R}");
                }
            }
            if (step == 1)
            {
                foreach (var cl in asset.Clusters)
                {
                    if (cl.MipLevel == 0 && cl.LODError != 0.0f)
                    {
                        failures.Add($"{tag}: full-res leaf has LODError {cl.LODError}");
                        break;
                    }
                }
            }
            var genMax = new double[asset.Groups.Length];
            var childMax = new float[asset.Groups.Length];
            for (int c = 0; c < asset.Clusters.Length; c++)
            {
                var cl = asset.Clusters[c];
                if (cl.GeneratingGroup >= 0)
                    genMax[cl.GeneratingGroup] = Math.Max(genMax[cl.GeneratingGroup], clusterMaxDy[c]);
                if (cl.ParentGroup >= 0)
                    childMax[cl.ParentGroup] = Math.Max(childMax[cl.ParentGroup], cl.LODError);
            }
            for (int g = 0; g < asset.Groups.Length; g++)
            {
                float allowed = Math.Max(childMax[g], RoundUpBound(genMax[g], pad));
                if (asset.Groups[g].ParentLODError > allowed)
                {
                    if (loose++ < 8)
                        failures.Add($"{tag}: group[{g}] ParentLODError {asset.Groups[g].ParentLODError:R} > max(child {childMax[g]:R}, measured {genMax[g]:R})");
                }
            }
            if (overErr > 4)
                failures.Add($"{tag}: {overErr} clusters with LODError below measured error in total");
            if (loose > 8)
                failures.Add($"{tag}: {loose} loose errors in total");

            // 一致 cut: 叶子 / 若干分位数 / 根
            var errors = new List<float>();
            foreach (var g in asset.Groups)
                errors.Add(g.ParentLODError);
            errors.Sort();
            var taus = new List<float>() { -1.0f };
            if (errors.Count > 0)
            {
                foreach (var q in new[] { 0.1, 0.35, 0.6, 0.85 })
                    taus.Add(errors[(int)(q * (errors.Count - 1))]);
                taus.Add(errors[errors.Count - 1]);
            }
            float maxLeafError = 0.0f;
            foreach (var cl in asset.Clusters)
            {
                if (cl.MipLevel == 0)
                    maxLeafError = Math.Max(maxLeafError, cl.LODError);
            }
            for (int i = 0; i < taus.Count; i++)
            {
                float tau = taus[i];
                if (i > 0 && tau == taus[i - 1])
                    continue;
                var cut = SelectCut(asset, tau);
                var st = MeasureMesh($"cut τ={tau:G4}", asset, cut, hf, null);
                AppendStats(sb, st, expect2A);
                CheckMesh(tag, st, size, borderEdges, failures);
                if (st.MaxDy > Math.Max(tau, maxLeafError))
                    failures.Add($"{tag} {st.Name}: measured {st.MaxDy:R} exceeds threshold");
                if (i == 0 && st.Clusters != levels[0].Clusters)
                    failures.Add($"{tag}: τ=-1 cut selects {st.Clusters} clusters, leaf level has {levels[0].Clusters}");
                if (i == taus.Count - 1 && st.Clusters != levels[levels.Count - 1].Clusters)
                    failures.Add($"{tag}: max τ cut selects {st.Clusters} clusters, root level has {levels[levels.Count - 1].Clusters}");
            }
            sb.AppendLine($"    worst measured/LODError={worstRatio:F4}, root declared={levels[levels.Count - 1].DeclaredError:G6}");
            Flush(sb);
        }

        static bool Rejects(byte[] data)
        {
            try
            {
                TtVgDagAsset.FromBytes(data);
                return false;
            }
            catch (InvalidDataException)
            {
                return true;
            }
            catch (EndOfStreamException)
            {
                return true;
            }
        }

        static byte[] SaveRaw(TtVgDagAsset a)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                a.Save(w);
                w.Flush();
                return ms.ToArray();
            }
        }

        static void CheckSerialization(List<string> failures)
        {
            int size = 129;
            var hf = GenHeightfield(size, 3);
            var bytes = Bake(hf, size, 1).ToBytes();

            var magic = (byte[])bytes.Clone();
            magic[0] ^= 1;
            if (!Rejects(magic)) failures.Add("serialization: bad magic accepted");
            var version = (byte[])bytes.Clone();
            version[4] ^= 1;
            if (!Rejects(version)) failures.Add("serialization: bad version accepted");
            foreach (int len in new[] { 0, 7, 20, bytes.Length / 2, bytes.Length - 1 })
            {
                if (!Rejects(bytes.AsSpan(0, len).ToArray()))
                    failures.Add($"serialization: truncated to {len} accepted");
            }
            var trailing = new byte[bytes.Length + 1];
            bytes.CopyTo(trailing, 0);
            if (!Rejects(trailing)) failures.Add("serialization: trailing byte accepted");

            // 语义损坏: ToBytes 拒写, 原样写出后 Load 拒读
            void Tamper(string what, Action<TtVgDagAsset> edit)
            {
                var a = TtVgDagAsset.FromBytes(bytes);
                edit(a);
                bool writeRejected = false;
                try
                {
                    a.ToBytes();
                }
                catch (InvalidOperationException)
                {
                    writeRejected = true;
                }
                if (!writeRejected)
                    failures.Add($"serialization: ToBytes accepted {what}");
                if (!Rejects(SaveRaw(a)))
                    failures.Add($"serialization: Load accepted {what}");
            }
            int parent = -1, leaf = -1;
            {
                var a = TtVgDagAsset.FromBytes(bytes);
                for (int c = 0; c < a.Clusters.Length; c++)
                {
                    if (a.Clusters[c].GeneratingGroup >= 0 && parent < 0) parent = c;
                    if (a.Clusters[c].GeneratingGroup < 0 && leaf < 0) leaf = c;
                }
            }
            if (parent < 0 || leaf < 0)
            {
                failures.Add("serialization: test asset has no parent/leaf cluster");
                return;
            }
            Tamper("lowered LODError", a => a.Clusters[parent].LODError = MathF.BitDecrement(a.Clusters[parent].LODError));
            Tamper("index past VertexCount", a => a.Indices[a.Clusters[leaf].IndexStart] = (ushort)a.Clusters[leaf].VertexCount);
            Tamper("stale cluster bounds", a => a.Clusters[leaf].MaxY += 1.0f);
            Tamper("vertex outside height range", a => a.Vertices[0].Y = a.HeightMax + 1.0f);
            Tamper("self-parent", a => a.Clusters[parent].ParentGroup = a.Clusters[parent].GeneratingGroup);
            Tamper("shrunk group bounds", a => a.Groups[a.Clusters[parent].GeneratingGroup].MaxTexelX = 0);
            Tamper("leaf with generating group", a => a.Clusters[leaf].GeneratingGroup = a.Clusters[parent].GeneratingGroup);
            Tamper("orphan group", a =>
            {
                var g = a.Groups;
                Array.Resize(ref g, g.Length + 1);
                g[g.Length - 1] = g[0];
                a.Groups = g;
            });
            Tamper("zero step", a => a.Step = 0);
        }

        static unsafe void CheckCancelAndReporting(List<string> failures)
        {
            // 原生: 先取消再构建 → -1 且无输出
            {
                int size = 65;
                var hf = GenHeightfield(size, 5);
                var builder = FQuarkDAGBuilder.CreateInstance();
                try
                {
                    builder.RequestCancel();
                    int r;
                    fixed (float* p = hf)
                        r = builder.BuildTerrain(p, (uint)size, 1, TtQuarkTerrainBaker.MaxGroupSize, TtQuarkTerrainBaker.ClusterSize, TtQuarkTerrainBaker.SimplifyTargetError);
                    if (r != -1 || builder.GetClusterCount() != 0 || builder.GetVertexCount() != 0)
                        failures.Add($"cancel: native pre-cancelled BuildTerrain returned {r}, clusters={builder.GetClusterCount()}");
                }
                finally
                {
                    builder.NativeSuper.Release();
                }
            }
            // 托管: 先取消再 Bake → null
            {
                var hf = GenHeightfield(65, 5);
                using (var baker = new TtQuarkTerrainBaker())
                {
                    baker.RequestCancel();
                    if (baker.Bake(hf, 65, 1) != null)
                        failures.Add("cancel: pre-cancelled Bake returned an asset");
                }
            }
            // 托管: 中途取消 → null, 且在合理时间内返回
            {
                int size = 1025;
                var hf = GenHeightfield(size, 9);
                TtVgDagAsset result = null;
                Exception error = null;
                var baker = new TtQuarkTerrainBaker();
                var started = new System.Threading.ManualResetEventSlim(false);
                var worker = new System.Threading.Thread(() =>
                {
                    try
                    {
                        result = baker.Bake(hf, size, 1, s => started.Set());
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }
                    finally
                    {
                        started.Set();
                    }
                });
                worker.Start();
                started.Wait();
                System.Threading.Thread.Sleep(50);
                var sw = Stopwatch.StartNew();
                baker.RequestCancel();
                worker.Join();
                sw.Stop();
                baker.Dispose();
                if (error != null)
                    failures.Add($"cancel: mid-build Bake threw {error.GetType().Name}: {error.Message}");
                else if (result != null)
                    failures.Add("cancel: mid-build Bake finished despite RequestCancel");
                // 叶子分区(邻接/METIS)也有取消检查点, 延迟应在毫秒级; 修复前实测 ~8 s
                if (sw.Elapsed.TotalMilliseconds > 2000)
                    failures.Add($"cancel: latency {sw.Elapsed.TotalMilliseconds:F0} ms > 2000 ms");
                var sb = new StringBuilder();
                sb.AppendLine($"== cancel latency (1025^2, step 1): {sw.Elapsed.TotalMilliseconds:F1} ms ==");
                Flush(sb);
            }
            // 进度回调只观察, 不改变产物; 开始与完成都要报告
            {
                int size = 129;
                var hf = GenHeightfield(size, 13);
                var stages = new List<string>();
                var withReport = Bake(hf, size, 2, s => stages.Add(s)).ToBytes();
                var without = Bake(hf, size, 2).ToBytes();
                if (!SameBytes(withReport, without))
                    failures.Add("report: reporting changed baked bytes");
                if (stages.Count < 2 || !stages[0].StartsWith("DAG 后台开始") || !stages[stages.Count - 1].StartsWith("DAG 烘焙完成"))
                    failures.Add($"report: unexpected stages [{string.Join(" | ", stages)}]");
            }
            // 非法输入抛 ArgumentException
            {
                bool threw = false;
                try
                {
                    Bake(new float[10], 4, 1);
                }
                catch (ArgumentException)
                {
                    threw = true;
                }
                if (!threw)
                    failures.Add("args: heightfield size mismatch not rejected");
                threw = false;
                try
                {
                    Bake(new float[16], 4, 4);
                }
                catch (ArgumentException)
                {
                    threw = true;
                }
                if (!threw)
                    failures.Add("args: step >= size not rejected");
                threw = false;
                try
                {
                    var hf = new float[16];
                    hf[5] = float.NaN;
                    Bake(hf, 4, 1);
                }
                catch (ArgumentException)
                {
                    threw = true;
                }
                if (!threw)
                    failures.Add("args: NaN height not rejected");
            }
        }

        /// <summary>
        /// 渲染原型的无 GPU 验收: 远景 prepare 迷你图必须能从 RootNode 回溯到 cull 节点
        /// (否则 NodeLayers 为 null, BeginTick 直接 NRE); VGDag compute shader 必须能编译通过。
        /// </summary>
        static void CheckRenderPrototype(List<string> failures)
        {
            var far = new TtVgDagFarTerrain();
            try
            {
                var policy = far.BuildPrepareGraph(out var hasInputError);
                if (hasInputError)
                    failures.Add("render: prepare graph reports unlinked inputs");
                if (policy.NodeLayers == null)
                    failures.Add("render: prepare graph NodeLayers is null (RootNode not reachable)");
                if (far.PrepareNode == null || !far.PrepareNode.IsUsed)
                    failures.Add("render: prepare node not reachable from RootNode, would never Tick");
            }
            finally
            {
                far.Dispose();
            }

            var rc = TtEngine.Instance?.GfxDevice?.RenderContext;
            if (rc == null)
            {
                var skip = new StringBuilder();
                skip.AppendLine("== render: no RenderContext, shader compile check skipped ==");
                Flush(skip);
                return;
            }
            // 与 EffectManager.GetComputeEffect 同一编译路径(同步调用), IsOnlyBuildMode 不写 shader 缓存
            var envs = new Graphics.Pipeline.Shader.TtComputeShadingEnv[]
            {
                new TtVgDagFarTerrain.TtVgDagClearShading(),
                new TtVgDagFarTerrain.TtVgDagCullShading(),
                new TtVgVisClearShading(),
                new TtVgVisRasterShading(),
            };
            var sb = new StringBuilder();
            foreach (var env in envs)
            {
                env.IsOnlyBuildMode = true;
                var label = $"{env.CodeName}:{env.MainName}";
                var compiler = new Editor.ShaderCompiler.TtHLSLCompiler();
                compiler.MdfQueue = null;
                NxRHI.TtShaderDesc desc = null;
                try
                {
                    var defines = new NxRHI.TtShaderDefinitions();
                    env.GetShaderDefines(env.CurrentPermutationId, defines);
                    desc = compiler.CompileShader(env.CodeName.Address, env.MainName, NxRHI.EShaderType.SDT_ComputeShader, env,
                        env.CurrentPermutationId, null, null, defines, env.ShaderIncludeProvider);
                }
                catch (Exception e)
                {
                    failures.Add($"render: compile {label} threw {e.GetType().Name}: {e.Message}");
                    continue;
                }
                if (desc == null)
                    failures.Add($"render: compile {label} failed (see ShaderCompiler log)");
                else
                    sb.AppendLine($"   shader ok: {label}");
            }
            Flush(sb);
        }

        static void Flush(StringBuilder sb)
        {
            var report = sb.ToString();
            sb.Clear();
            Console.WriteLine(report);
            Console.Out.Flush();
            foreach (var line in report.Split('\n'))
                Profiler.Log.WriteLine<TtTestCategory>(Profiler.ELogTag.Info, line.TrimEnd('\r'));
        }

        public void UnitTestEntrance()
        {
            var failures = new List<string>();
            RunCase("tiny", GenHeightfield(2, 7), 2, 1, failures);
            RunCase("tiny", GenHeightfield(5, 7), 5, 1, failures);
            RunCase("tiny", GenHeightfield(9, 7), 9, 3, failures);
            RunCase("noise", GenHeightfield(129, 7), 129, 1, failures);
            RunCase("noise", GenHeightfield(129, 7), 129, 2, failures);
            // (size-1) 不整除 step: 最后一行/列间距不均匀
            RunCase("noise", GenHeightfield(129, 7), 129, 3, failures);
            RunCase("noise", GenHeightfield(257, 7), 257, 1, failures);
            // 缓坡: 高度 ×0.25 (2 的幂, 逐位精确)
            RunCase("noise x0.25", GenHeightfield(257, 7, 0.25f), 257, 1, failures);
            RunCase("noise", GenHeightfield(257, 7), 257, 2, failures);
            // 生产默认 VgDagStep = 2
            RunCase("noise", GenHeightfield(513, 7), 513, 2, failures);
            RunCase("flat", GenFlat(129), 129, 1, failures);
            RunCase("peak", GenPeak(129), 129, 1, failures);
            RunCase("saddle", GenSaddle(129), 129, 2, failures);
            RunCase("cliff", GenCliff(129), 129, 1, failures);
            CheckSerialization(failures);
            CheckCancelAndReporting(failures);
            CheckRenderPrototype(failures);

            var report = new StringBuilder();
            report.AppendLine($"== {failures.Count} failure(s) ==");
            foreach (var f in failures)
                report.AppendLine("  " + f);
            Flush(report);
            TtUnitTestManager.TAssert(failures.Count == 0, $"{failures.Count} failure(s), see log");
        }
    }
}
