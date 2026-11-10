using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// Cluster DAG 烘焙器(纯 CPU, 不依赖引擎运行时)。
    /// 输入一块 HeightfieldSize² 高度场, 产出四叉 cluster DAG:
    /// 第 0 层为隐式叶子网格(LeafClusterQuads² quads), 每层整块合并 +
    /// 锁边 QEM 简化, 直到根覆盖全图。
    ///
    /// 层级构建采用 Quark(NativeCode\Bricks\Quark) 式 weld + 临时锁边:
    /// 每层先把整块 heightfield weld 成一张连续网格(同层 cluster 间不存在
    /// 预先锁死的接缝), simplify 只锁 heightfield 真实外圈(跨 tile 接缝,
    /// 等价于 meshopt_SimplifyLockBorder 的开放边), 简化完按 cluster 网格
    /// 切分面归属。切分只是划分, 共享边链两侧引用同一批顶点,
    /// 逐层不需要保留全密度边框环。
    ///
    /// 确定性: 固定遍历次序 + 稳定优先队列(代价并列时按顶点 id 决出),
    /// 同一输入两次 Bake 逐位一致(由单元测试保证)。
    ///
    /// 接缝三硬约定在本类中的落点:
    /// ① 锁边: 仅 heightfield 外圈顶点在简化全程禁止塌陷, 恒为原始采样点;
    ///    同层 cluster 间接缝是同一张连续网格上的切分线, 两侧天然逐位一致;
    /// ② 对角: 网格按 FVgDiagonalConvention 生成, 简化只删不改既有剖分方向;
    /// ③ 逐位: 锁边顶点 y 直接取自 heightfield 采样, xz 格点编码, 无浮点重算。
    /// </summary>
    public class TtVgClusterBaker
    {
        /// <summary>bake 中间网格: 顶点在 texel 格点上(xz 整数), 三角形为顶点下标三元组。</summary>
        class BakeMesh
        {
            public int MinX;
            public int MinZ;
            public int Extent;
            public List<int> VX = new List<int>();
            public List<int> VZ = new List<int>();
            public List<float> VY = new List<float>();
            /// <summary>用引用相等哈希而非 List: 塌陷删除路径 m.Tris.Remove(t) 原为 O(T)
            /// 线性扫描+移位, 整层 weld 网格 T 百万级时塌陷主循环退化成 O(T²) (实测小时级);
            /// HashSet 后 Remove/Count 均 O(1), 消费点 (Add/foreach/Count/Remove, 无索引访问) 全兼容。
            /// 注: foreach 遍历序不再是插入序, 输出与旧版不同但仍确定性 (同输入同序列)。</summary>
            public HashSet<int[]> Tris = new HashSet<int[]>();
            /// <summary>每个三角形在简化期间承受的最大塌陷代价(SimplifyCluster 填写,
            /// cluster 切分后按面归集 MaxError)。</summary>
            public Dictionary<int[], float> FaceErr = new Dictionary<int[], float>();
            public float MinY = float.MaxValue;
            public float MaxY = float.MinValue;

            public bool IsLocked(int vi)
            {
                int x = VX[vi];
                int z = VZ[vi];
                return x == MinX || x == MinX + Extent || z == MinZ || z == MinZ + Extent;
            }

            public BakeMesh Clone()
            {
                var m = new BakeMesh();
                m.MinX = MinX; m.MinZ = MinZ; m.Extent = Extent;
                m.MinY = MinY; m.MaxY = MaxY;
                m.VX = new List<int>(VX);
                m.VZ = new List<int>(VZ);
                m.VY = new List<float>(VY);
                foreach (var t in Tris)
                    m.Tris.Add(new int[3] { t[0], t[1], t[2] });
                return m;
            }
        }

        class LvlMesh
        {
            public BakeMesh Mesh;
            public float Err;
        }

        struct HeapEntry
        {
            public double Cost;
            public int A;
            public int B;
            public int VersionA;
            public int VersionB;
        }

        /// <summary>稳定二叉最小堆: 代价并列时按 (A, B) 决出, 保证确定性。</summary>
        class CostHeap
        {
            List<HeapEntry> mItems = new List<HeapEntry>();
            static bool Less(HeapEntry l, HeapEntry r)
            {
                if (l.Cost != r.Cost)
                    return l.Cost < r.Cost;
                if (l.A != r.A)
                    return l.A < r.A;
                return l.B < r.B;
            }
            public int Count
            {
                get { return mItems.Count; }
            }
            public void Push(HeapEntry e)
            {
                mItems.Add(e);
                int i = mItems.Count - 1;
                while (i > 0)
                {
                    int p = (i - 1) >> 1;
                    if (Less(mItems[i], mItems[p]))
                    {
                        var t = mItems[p]; mItems[p] = mItems[i]; mItems[i] = t;
                        i = p;
                    }
                    else
                        break;
                }
            }
            public HeapEntry Pop()
            {
                var top = mItems[0];
                int last = mItems.Count - 1;
                mItems[0] = mItems[last];
                mItems.RemoveAt(last);
                int i = 0;
                while (true)
                {
                    int l = i * 2 + 1;
                    int r = l + 1;
                    int m = i;
                    if (l < mItems.Count && Less(mItems[l], mItems[m]))
                        m = l;
                    if (r < mItems.Count && Less(mItems[r], mItems[m]))
                        m = r;
                    if (m == i)
                        break;
                    var t = mItems[m]; mItems[m] = mItems[i]; mItems[i] = t;
                    i = m;
                }
                return top;
            }
        }

        // QEM: 对称 4x4 矩阵压缩为 10 个 double
        // [ m0 m1 m2  m3 ]
        // [ m1 m4 m5  m7 ]
        // [ m2 m5 m6  m8 ]
        // [ m3 m7 m8  m9 ]

        static void AddPlane(double[] q, float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz)
        {
            float e1x = bx - ax, e1y = by - ay, e1z = bz - az;
            float e2x = cx - ax, e2y = cy - ay, e2z = cz - az;
            double nx = (double)e1y * e2z - (double)e1z * e2y;
            double ny = (double)e1z * e2x - (double)e1x * e2z;
            double nz = (double)e1x * e2y - (double)e1y * e2x;
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len < 1e-12)
                return;
            nx /= len; ny /= len; nz /= len;
            double d = -(nx * ax + ny * ay + nz * az);
            q[0] += nx * nx; q[1] += nx * ny; q[2] += nx * nz; q[3] += nx * d;
            q[4] += ny * ny; q[5] += ny * nz; q[7] += ny * d;
            q[6] += nz * nz; q[8] += nz * d;
            q[9] += d * d;
        }
        static void AddTriQuadric(double[] q, BakeMesh m, int[] t)
        {
            AddPlane(q,
                m.VX[t[0]], m.VY[t[0]], m.VZ[t[0]],
                m.VX[t[1]], m.VY[t[1]], m.VZ[t[1]],
                m.VX[t[2]], m.VY[t[2]], m.VZ[t[2]]);
        }

        static double EvalQuadric(double[] q, double x, double y, double z)
        {
            return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x
                 + q[4] * y * y + 2 * q[5] * y * z + 2 * q[7] * y
                 + q[6] * z * z + 2 * q[8] * z
                 + q[9];
        }

        static double[] SumQuadric(double[] a, double[] b)
        {
            var r = new double[10];
            for (int i = 0; i < 10; i++)
                r[i] = a[i] + b[i];
            return r;
        }

        /// <summary>解 Q0+Q1 的最优塌陷位置; 矩阵退化时在两端点/中点里取代价最小者。</summary>
        static void OptimalPosition(double[] qa, double[] qb, BakeMesh m, int a, int b, out double ox, out double oy, out double oz)
        {
            double[] q = SumQuadric(qa, qb);
            double a11 = q[0], a12 = q[1], a13 = q[2];
            double a22 = q[4], a23 = q[5];
            double a33 = q[6];
            double b1 = -q[3], b2 = -q[7], b3 = -q[8];
            double det = a11 * (a22 * a33 - a23 * a23) - a12 * (a12 * a33 - a23 * a13) + a13 * (a12 * a23 - a22 * a13);
            double ax = m.VX[a], ay = (double)m.VY[a], az = m.VZ[a];
            double bx = m.VX[b], by = (double)m.VY[b], bz = m.VZ[b];
            double mx = (ax + bx) * 0.5, my = (ay + by) * 0.5, mz = (az + bz) * 0.5;
            ox = mx; oy = my; oz = mz;
            if (Math.Abs(det) > 1e-9)
            {
                double i11 = (a22 * a33 - a23 * a23) / det;
                double i12 = (a13 * a23 - a12 * a33) / det;
                double i13 = (a12 * a23 - a13 * a22) / det;
                double i22 = (a11 * a33 - a13 * a13) / det;
                double i23 = (a13 * a12 - a11 * a23) / det;
                double i33 = (a11 * a22 - a12 * a12) / det;
                double sx = i11 * b1 + i12 * b2 + i13 * b3;
                double sy = i12 * b1 + i22 * b2 + i23 * b3;
                double sz = i13 * b1 + i23 * b2 + i33 * b3;
                if (EvalQuadric(q, sx, sy, sz) < EvalQuadric(q, ox, oy, oz))
                { ox = sx; oy = sy; oz = sz; }
            }
            if (EvalQuadric(q, ax, ay, az) < EvalQuadric(q, ox, oy, oz))
            { ox = ax; oy = ay; oz = az; }
            if (EvalQuadric(q, bx, by, bz) < EvalQuadric(q, ox, oy, oz))
            { ox = bx; oy = by; oz = bz; }
        }

        static void TriNormal(BakeMesh m, int[] t, out double nx, out double ny, out double nz)
        {
            double e1x = m.VX[t[1]] - m.VX[t[0]], e1y = (double)m.VY[t[1]] - m.VY[t[0]], e1z = m.VZ[t[1]] - m.VZ[t[0]];
            double e2x = m.VX[t[2]] - m.VX[t[0]], e2y = (double)m.VY[t[2]] - m.VY[t[0]], e2z = m.VZ[t[2]] - m.VZ[t[0]];
            nx = e1y * e2z - e1z * e2y;
            ny = e1z * e2x - e1x * e2z;
            nz = e1x * e2y - e1y * e2x;
            double len = Math.Sqrt(nx * nx + ny * ny + nz * nz);
            if (len > 1e-12)
            { nx /= len; ny /= len; nz /= len; }
        }
        static bool TriValid(int[] t)
        {
            return t[0] != t[1] && t[1] != t[2] && t[0] != t[2];
        }
        /// <summary>顶点三元组的无序 key(排序后打包; 全局网格顶点数 < 2²¹)。</summary>
        static long TriKey(int[] t)
        {
            int x = t[0], y = t[1], z = t[2], tmp;
            if (y < x) { tmp = x; x = y; y = tmp; }
            if (z < x) { tmp = x; x = z; z = tmp; }
            if (z < y) { tmp = y; y = z; z = tmp; }
            return ((long)x << 42) | ((long)y << 21) | (uint)z;
        }
        static int IndexOfVert(int[] t, int v)
        {
            for (int i = 0; i < 3; i++)
            {
                if (t[i] == v)
                    return i;
            }
            return -1;
        }

        float[] mHeightfield;
        int mSize;
        int mLeafQuads;
        int mPreStep = 1; // 规则网格预塌陷步长 (Bake 入口赋值), 见 BuildFullGrid 注释

        static void EmitQuad(BakeMesh m, int a, int b, int c, int d)
        {
            // FVgDiagonalConvention: a=(x,z), b=(x+1,z), c=(x+1,z+1), d=(x,z+1), 对角线 a→c
            m.Tris.Add(new int[3] { a, b, c });
            m.Tris.Add(new int[3] { a, c, d });
        }

        BakeMesh BuildLeaf(int cx, int cz)
        {
            var m = new BakeMesh();
            m.MinX = cx * mLeafQuads;
            m.MinZ = cz * mLeafQuads;
            m.Extent = mLeafQuads;
            int side = mLeafQuads + 1;
            for (int z = 0; z < side; z++)
            {
                for (int x = 0; x < side; x++)
                {
                    int tx = m.MinX + x;
                    int tz = m.MinZ + z;
                    float y = mHeightfield[tz * mSize + tx];
                    m.VX.Add(tx);
                    m.VZ.Add(tz);
                    m.VY.Add(y);
                    if (y < m.MinY) m.MinY = y;
                    if (y > m.MaxY) m.MaxY = y;
                }
            }
            for (int z = 0; z + 1 < side; z++)
            {
                for (int x = 0; x + 1 < side; x++)
                {
                    int a = z * side + x;
                    int b = z * side + (x + 1);
                    int c = (z + 1) * side + (x + 1);
                    int d = (z + 1) * side + x;
                    EmitQuad(m, a, b, c, d);
                }
            }
            return m;
        }

        /// <summary>
        /// 整块 heightfield 的连续规则网格(等价于 Quark ReduceGroup 里组内 weld 后的大 mesh):
        /// 同层 cluster 间在这个阶段不存在接缝, 内部顶点均可自由塌陷;
        /// 仅 heightfield 外圈(IsLocked)锁边, 对应跨 tile 的永久接缝。
        /// </summary>
        /// <param name="preStep">
        /// 规则网格预塌陷步长: 每 preStep×preStep 原始 texel 块合并为一个顶点
        /// (y 取块内最大高度, 保守防远景被地形遮蔽穿模), 面数降 preStep² 倍。
        /// heightfield 是规则网格, 第一级粗化无需 QEM 挑选 —— 直接结构化降采样把
        /// QEM 的建堆/塌陷规模都省掉 (实测整层 2M 面时 QEM 路径分钟级的主因就是 T 太大)。
        /// 引入的几何偏差 ≤ 1 texel 高度, 相对 maxErrorPerLevel (默认 1600 = 40 texel²) 可忽略。
        /// 外圈仍落在格点上且被锁, 接缝约定不受影响; 叶子层 (level 0) 不走此路径, 仍原分辨率。
        /// </param>
        BakeMesh BuildFullGrid(int preStep = 1)
        {
            if (preStep < 1) preStep = 1;
            var m = new BakeMesh();
            m.MinX = 0;
            m.MinZ = 0;
            int srcSide = mSize;
            int dstSide = (srcSide - 1) / preStep + 1;
            m.Extent = dstSide - 1;
            for (int z = 0; z < dstSide; z++)
            {
                for (int x = 0; x < dstSide; x++)
                {
                    int bx = x * preStep;
                    int bz = z * preStep;
                    int ex = Math.Min(bx + preStep, srcSide);
                    int ez = Math.Min(bz + preStep, srcSide);
                    float y = float.MinValue;
                    for (int sz = bz; sz < ez; sz++)
                    {
                        for (int sx = bx; sx < ex; sx++)
                        {
                            float h = mHeightfield[sz * srcSide + sx];
                            if (h > y) y = h;
                        }
                    }
                    m.VX.Add(bx);
                    m.VZ.Add(bz);
                    m.VY.Add(y);
                    if (y < m.MinY) m.MinY = y;
                    if (y > m.MaxY) m.MaxY = y;
                }
            }
            for (int z = 0; z + 1 < dstSide; z++)
            {
                for (int x = 0; x + 1 < dstSide; x++)
                {
                    int a = z * dstSide + x;
                    int b = z * dstSide + (x + 1);
                    int c = (z + 1) * dstSide + (x + 1);
                    int d = (z + 1) * dstSide + x;
                    EmitQuad(m, a, b, c, d);
                }
            }
            return m;
        }

        /// <summary>
        /// 把简化后的整块网格按 cluster 网格切分面归属(等价于 Quark 的 Split):
        /// 三角形按质心归入唯一 cluster; 切分线两侧引用的是同一张网格的同一批顶点,
        /// 同层相邻 cluster 的共享边顶点集合由构造保证逐位一致(接缝约定①③)。
        /// 每 cluster 的 MaxError 取其面集合在简化期间承受的最大代价。
        /// </summary>
        List<LvlMesh> SplitLevelToClusters(BakeMesh g, int sideClusters, int extentTexels)
        {
            var buckets = new List<int[]>[sideClusters * sideClusters];
            for (int i = 0; i < buckets.Length; i++)
                buckets[i] = new List<int[]>();
            foreach (var t in g.Tris)
            {
                float cx = (g.VX[t[0]] + g.VX[t[1]] + g.VX[t[2]]) * (1.0f / 3.0f);
                float cz = (g.VZ[t[0]] + g.VZ[t[1]] + g.VZ[t[2]]) * (1.0f / 3.0f);
                int gx = (int)(cx / extentTexels);
                int gz = (int)(cz / extentTexels);
                if (gx >= sideClusters) gx = sideClusters - 1;
                if (gz >= sideClusters) gz = sideClusters - 1;
                buckets[gz * sideClusters + gx].Add(t);
            }

            var outList = new List<LvlMesh>(buckets.Length);
            for (int gz = 0; gz < sideClusters; gz++)
            {
                for (int gx = 0; gx < sideClusters; gx++)
                {
                    var bucket = buckets[gz * sideClusters + gx];
                    var cm = new BakeMesh();
                    cm.MinX = gx * extentTexels;
                    cm.MinZ = gz * extentTexels;
                    cm.Extent = extentTexels;
                    var remap = new Dictionary<int, int>();
                    float err = 0.0f;
                    foreach (var t in bucket)
                    {
                        var nt = new int[3];
                        for (int k = 0; k < 3; k++)
                        {
                            int vi = t[k];
                            int li;
                            if (remap.TryGetValue(vi, out li) == false)
                            {
                                li = cm.VX.Count;
                                remap.Add(vi, li);
                                cm.VX.Add(g.VX[vi]);
                                cm.VZ.Add(g.VZ[vi]);
                                cm.VY.Add(g.VY[vi]);
                                if (g.VY[vi] < cm.MinY) cm.MinY = g.VY[vi];
                                if (g.VY[vi] > cm.MaxY) cm.MaxY = g.VY[vi];
                            }
                            nt[k] = li;
                        }
                        cm.Tris.Add(nt);
                        float fe;
                        if (g.FaceErr.TryGetValue(t, out fe) && fe > err)
                            err = fe;
                    }
                    outList.Add(new LvlMesh { Mesh = cm, Err = err });
                }
            }
            return outList;
        }

        static long EdgeKey(int a, int b)
        {
            // 全局网格顶点数可超过 2^15(生产 1024² 顶点池), 用 64bit 组合 key 保证 (a,b) 无序唯一
            return a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
        }

        void PushEdge(BakeMesh m, CostHeap heap, double[][] quadrics, int[] versions, bool[] locked, int a, int b)
        {
            if (locked[a] || locked[b])
                return;
            double ox, oy, oz;
            OptimalPosition(quadrics[a], quadrics[b], m, a, b, out ox, out oy, out oz);
            var e = new HeapEntry();
            e.Cost = EvalQuadric(SumQuadric(quadrics[a], quadrics[b]), ox, oy, oz);
            e.A = a; e.B = b;
            e.VersionA = versions[a];
            e.VersionB = versions[b];
            heap.Push(e);
        }

        void RebuildEdgesAround(BakeMesh m, CostHeap heap, double[][] quadrics, int[] versions, bool[] locked, List<int[]> faces, int center, HashSet<long> exclude = null)
        {
            var seen = new HashSet<long>();
            foreach (var t in faces)
            {
                for (int k = 0; k < 3; k++)
                {
                    int p = t[k];
                    int q = t[(k + 1) % 3];
                    if (locked[p] || locked[q])
                        continue;
                    if (p != center && q != center)
                        continue;
                    long ek = EdgeKey(p, q);
                    if (exclude != null && exclude.Contains(ek))
                        continue;
                    if (seen.Add(ek) == false)
                        continue;
                    PushEdge(m, heap, quadrics, versions, locked, p, q);
                }
            }
        }

        /// <summary>
        /// 翻折检查: 把 movedVert 移动到新位置后, 其周边三角形法线不得翻转。
        /// otherCollapsedVert 所在三角形将被删除, 跳过不计。
        /// </summary>
        bool WouldFlip(BakeMesh m, List<int[]> faces, int movedVert, int otherCollapsedVert, int nx, int nz, float ny)
        {
            foreach (var t in faces)
            {
                if (IndexOfVert(t, otherCollapsedVert) >= 0)
                    continue;
                int mi = IndexOfVert(t, movedVert);
                if (mi < 0)
                    continue;
                double onx, ony, onz;
                TriNormal(m, t, out onx, out ony, out onz);
                int ox = m.VX[t[mi]];
                int oz = m.VZ[t[mi]];
                float oy = m.VY[t[mi]];
                m.VX[t[mi]] = nx;
                m.VZ[t[mi]] = nz;
                m.VY[t[mi]] = ny;
                double nnx, nny, nnz;
                TriNormal(m, t, out nnx, out nny, out nnz);
                m.VX[t[mi]] = ox;
                m.VZ[t[mi]] = oz;
                m.VY[t[mi]] = oy;
                if (onx * nnx + ony * nny + onz * nnz < 0.0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 就地简化 mesh 到 targetTriCount(或误差预算用尽)。
        /// 返回实际施加的最大塌陷代价, 作为本 cluster 的 MaxError;
        /// 返回 -1 表示被取消 (cancelState 轮询命中), 调用方应立即中止 bake。
        /// 注意: 单次调用处理的是整层 weld 网格, 塌陷主循环可达分钟级, 是取消响应延迟的主要来源。
        /// </summary>
        float SimplifyCluster(BakeMesh m, int targetTriCount, float maxError, Thread.Async.TtAsyncTaskStateBase cancelState = null)
        {
#if VG_DEBUG
            int dbgLocked = 0;
            for (int i = 0; i < m.VX.Count; i++)
                if (m.IsLocked(i)) dbgLocked++;
            Console.WriteLine($"  Simplify in: min=({m.MinX},{m.MinZ}) ext={m.Extent} verts={m.VX.Count} locked={dbgLocked} tris={m.Tris.Count} target={targetTriCount}");
#endif
            int vertCount = m.VX.Count;
            var quadrics = new double[vertCount][];
            for (int i = 0; i < vertCount; i++)
                quadrics[i] = new double[10];
            foreach (var t in m.Tris)
            {
                AddTriQuadric(quadrics[t[0]], m, t);
                AddTriQuadric(quadrics[t[1]], m, t);
                AddTriQuadric(quadrics[t[2]], m, t);
            }

            var versions = new int[vertCount];
            var alive = new bool[vertCount];
            for (int i = 0; i < vertCount; i++)
                alive[i] = true;

            var vFaces = new List<List<int[]>>(vertCount);
            for (int i = 0; i < vertCount; i++)
                vFaces.Add(new List<int[]>());
            foreach (var t in m.Tris)
            {
                vFaces[t[0]].Add(t);
                vFaces[t[1]].Add(t);
                vFaces[t[2]].Add(t);
            }

            var locked = new bool[vertCount];
            for (int i = 0; i < vertCount; i++)
                locked[i] = m.IsLocked(i);

            float maxAppliedCost = 0.0f;
            // 多轮重扫: 每轮从当前拓扑全量重建堆; 轮内被翻折/锁边保护排除的边
            // 在下一轮新拓扑下重获机会; 固定轮数保持确定性
            for (int pass = 0; pass < 3 && m.Tris.Count > targetTriCount; pass++)
            {
                if (cancelState?.IsCancelRequested == true)
                    return -1; // 取消检查点: pass 重建堆之前
                var heap = new CostHeap();
                var seen = new HashSet<long>();
                // 轮内永久排除集: 被翻折/锁边/边框 sliver 保护拒绝的边不再重试,
                // 否则拒绝路径重建邻边会互相重推形成活锁(耗尽 guard 停摆)
                var exclEdges = new HashSet<long>();
                var faceErr = new Dictionary<int[], float>();
                m.FaceErr = faceErr;
                int buildCancelCounter = 0;
                foreach (var t in m.Tris)
                {
                    // 建堆本身 O(T) (整层可达百万级), 取消时也要能提前退出
                    if ((++buildCancelCounter & 4095) == 0 && cancelState?.IsCancelRequested == true)
                        return -1;
                    for (int k = 0; k < 3; k++)
                    {
                        int a = t[k];
                        int b = t[(k + 1) % 3];
                        if (locked[a] || locked[b])
                            continue;
                        if (seen.Add(EdgeKey(a, b)) == false)
                            continue;
                        PushEdge(m, heap, quadrics, versions, locked, a, b);
                    }
                }

                int guard = m.Tris.Count * 8 + 64;
                int cancelCounter = 0;
                while (m.Tris.Count > targetTriCount && heap.Count > 0 && guard-- > 0)
                {
                    if ((++cancelCounter & 255) == 0 && cancelState?.IsCancelRequested == true)
                        return -1; // 取消检查点: 塌陷主循环内 (单次调用可达分钟级, 必须可中断)
                    var e = heap.Pop();
#if VG_DEBUG
                    if (maxAppliedCost == 0.0f && dbgFirstCost < 0)
                        Console.WriteLine($"  first pop cost={e.Cost} heapLeft={heap.Count}");
                    dbgFirstCost = e.Cost;
#endif
                    int a = e.A;
                    int b = e.B;
                    if (alive[a] == false || alive[b] == false)
                        continue;
                    if (exclEdges.Contains(EdgeKey(a, b)))
                        continue;
                    if (versions[a] != e.VersionA || versions[b] != e.VersionB)
                    {
#if VG_DEBUG
                        dbgStale++;
#endif
                        continue; // 过期条目; 当前状态下的边已随周边三角形重新入堆
                    }
                    if (locked[a] || locked[b])
                        continue;

                    double ox, oy, oz;
                    OptimalPosition(quadrics[a], quadrics[b], m, a, b, out ox, out oy, out oz);
                    double cost = EvalQuadric(SumQuadric(quadrics[a], quadrics[b]), ox, oy, oz);
                    if (cost > (double)maxError)
                    {
#if VG_DEBUG
                        dbgCostOver++;
#endif
                        break; // 误差预算用尽
                    }

                    int nx = 0, nz = 0;
                    float ny = 0;

                    // 位置候选: 依序尝试 snap 最优点/中点/两端点, 取首个不翻折者;
                    // 自由最优位置 snap 到格点后常跨越陡峭特征, 直接拒绝会大量浪费预算
                    double ax = m.VX[a], ay = (double)m.VY[a], az = m.VZ[a];
                    double bx = m.VX[b], by = (double)m.VY[b], bz = m.VZ[b];
                    var candX = new double[4] { ox, (ax + bx) * 0.5, ax, bx };
                    var candY = new double[4] { oy, (ay + by) * 0.5, ay, by };
                    var candZ = new double[4] { oz, (az + bz) * 0.5, az, bz };
                    bool placed = false;
                    for (int ci = 0; ci < 4 && placed == false; ci++)
                    {
                        int cnx = (int)Math.Round(candX[ci]);
                        int cnz = (int)Math.Round(candZ[ci]);
                        if (cnx < m.MinX + 1) cnx = m.MinX + 1;
                        if (cnx > m.MinX + m.Extent - 1) cnx = m.MinX + m.Extent - 1;
                        if (cnz < m.MinZ + 1) cnz = m.MinZ + 1;
                        if (cnz > m.MinZ + m.Extent - 1) cnz = m.MinZ + m.Extent - 1;
                        float cny = (float)candY[ci];
                        if (cny < m.MinY) cny = m.MinY;
                        if (cny > m.MaxY) cny = m.MaxY;
                        if (WouldFlip(m, vFaces[a], a, b, cnx, cnz, cny))
                            continue;
                        // b 的面重指到 a 位置后同样不得翻转(退化面除外)
                        if (WouldFlip(m, vFaces[b], b, a, cnx, cnz, cny))
                            continue;
                        nx = cnx; nz = cnz; ny = cny;
                        placed = true;
                    }
                    if (placed == false)
                    {
                        // 边翻转解锁: 若 (a,b) 是某个共享 quad 的对角线,
                        // 翻到另一条对角后塌陷可能合法; 否则排除该边
                        int[] t1 = null, t2 = null;
                        foreach (var t in vFaces[b])
                        {
                            if (IndexOfVert(t, a) < 0)
                                continue;
                            if (t1 == null) t1 = t;
                            else if (t2 == null) { t2 = t; break; }
                        }
                        int x = -1, y = -1;
                        if (t1 != null && t2 != null)
                        {
                            int p1 = IndexOfVert(t1, a);
                            int p2 = IndexOfVert(t2, b);
                            // 旋转使共享边同向: t1[p1]=a,t1[p1+1]=b; t2[p2]=b,t2[p2+1]=a
                            if (t1[(p1 + 1) % 3] != b) p1 = (p1 + 2) % 3;
                            if (t2[(p2 + 1) % 3] != a) p2 = (p2 + 2) % 3;
                            if (t1[(p1 + 1) % 3] == b && t2[(p2 + 1) % 3] == a)
                            {
                                x = t1[(p1 + 2) % 3];
                                y = t2[(p2 + 2) % 3];
                            }
                        }
                        bool canFlip = false;
                        // 锁边顶点(含 heightfield 外圈)不参与翻折:
                        // 翻折可能使边框边引用计数变为 2(表面跨边框折叠), 破坏跨 tile 接缝
                        if (x >= 0 && y >= 0 && x != y && locked[a] == false && locked[b] == false &&
                            locked[x] == false && locked[y] == false)
                        {
                            // 翻转后的新面不得与既有面顶点三元组重复,
                            // 否则产生非流形重复面, 破坏边界链/塌陷判定
                            var localKeys = new HashSet<long>();
                            foreach (var f in vFaces[a])
                            {
                                if (f != t1 && f != t2)
                                    localKeys.Add(TriKey(f));
                            }
                            foreach (var f in vFaces[b])
                            {
                                if (f != t1 && f != t2)
                                    localKeys.Add(TriKey(f));
                            }
                            // 新面含 a 或 b, 其潜在重复面必在 a/b 的邻面内, 局部检查完整
                            var flipTri = new int[3];
                            flipTri[0] = a; flipTri[1] = x; flipTri[2] = y;
                            canFlip = localKeys.Add(TriKey(flipTri));
                            if (canFlip)
                            {
                                flipTri[0] = b; flipTri[1] = y; flipTri[2] = x;
                                canFlip = localKeys.Add(TriKey(flipTri));
                            }
                        }
                        if (canFlip)
                        {
                            // 对角线 (a,b) → (x,y): 新三角形 (a,x,y) 与 (b,y,x)
                            // t1: a,b,x → a,x,y (b 移除, y 添加); t2: a,b,y → b,y,x (a 移除, x 添加)
                            t1[0] = a; t1[1] = x; t1[2] = y;
                            t2[0] = b; t2[1] = y; t2[2] = x;
                            vFaces[b].Remove(t1); vFaces[y].Add(t1);
                            vFaces[a].Remove(t2); vFaces[x].Add(t2);
                            versions[a]++; versions[b]++; versions[x]++; versions[y]++;
                            // 翻转后的 (a,b) 可能已合法, 重新入堆重试
                            PushEdge(m, heap, quadrics, versions, locked, a, b);
#if VG_DEBUG
                            dbgFlipCount++;
#endif
                            continue;
                        }
#if VG_DEBUG
                    dbgFlipReject++;
#endif
                        // 所有候选位置均翻折且无法翻转: 排除该边防死循环
                        exclEdges.Add(EdgeKey(a, b));
                        versions[a]++;
                        var fexcl = new HashSet<long>();
                        fexcl.Add(EdgeKey(a, b));
                        RebuildEdgesAround(m, heap, quadrics, versions, locked, vFaces[a], a, fexcl);
                        continue;
                    }

                    // 预收集本次塔陷将删除的面(不改动拓扑):
                    // ① 含 a,b 双顶点的共享面(重指后退化为无效);
                    // ② 重指后与现存面顶点三元组相同的重复面(塔陷合拢产生,
                    //    不删除会破坏流形性进而破坏边界链/翻折判定)
                    var willDie = new List<int[]>();
                    foreach (var t in vFaces[b])
                    {
                        if (IndexOfVert(t, a) >= 0)
                            willDie.Add(t);
                    }
                    var existKeys = new HashSet<long>();
                    foreach (var t in vFaces[a])
                    {
                        if (IndexOfVert(t, b) < 0)
                            existKeys.Add(TriKey(t));
                    }
                    var tmpTri = new int[3];
                    foreach (var t in vFaces[b])
                    {
                        if (IndexOfVert(t, a) >= 0)
                            continue;
                        for (int k = 0; k < 3; k++)
                            tmpTri[k] = (t[k] == b) ? a : t[k];
                        if (existKeys.Add(TriKey(tmpTri)) == false)
                            willDie.Add(t);
                    }

                    // 锁边保护: 仅当某个锁边顶点会因删除上述面(含重复面删除路径!)
                    // 而失去全部邻面孤立时才拒绝(违反接缝硬约定①)
                    bool killsLockedFace = false;
                    foreach (var t in willDie)
                    {
                        for (int k = 0; k < 3 && killsLockedFace == false; k++)
                        {
                            int p = t[k];
                            if (p == a || p == b || locked[p] == false)
                                continue;
                            bool isolated = true;
                            foreach (var f in vFaces[p])
                            {
                                if (willDie.Contains(f) == false)
                                {
                                    isolated = false;
                                    break;
                                }
                            }
                            if (isolated)
                                killsLockedFace = true;
                        }
                    }
                    if (killsLockedFace)
                    {
                        exclEdges.Add(EdgeKey(a, b));
#if VG_DEBUG
                        dbgLockReject++;
#endif
                        versions[a]++;
                        var excl = new HashSet<long>();
                        excl.Add(EdgeKey(a, b));
                        RebuildEdgesAround(m, heap, quadrics, versions, locked, vFaces[a], a, excl);
                        continue;
                    }

                    // 全边框面保护: 塌陷后若某存活面三个顶点全落在边框线上,
                    // 会形成贴在边框上的竖直 sliver, 使边框边引用计数变 2,
                    // 破坏跨 tile 接缝的边界链(同 lockReject 路径拒绝)
                    bool aBorderAfter = nx == m.MinX || nx == m.MinX + m.Extent || nz == m.MinZ || nz == m.MinZ + m.Extent;
                    bool makesBorderSliver = false;
                    foreach (var t in vFaces[b])
                    {
                        if (willDie.Contains(t))
                            continue;
                        bool allBorder = true;
                        for (int k = 0; k < 3 && allBorder; k++)
                        {
                            int p = (t[k] == b) ? a : t[k];
                            if (p == a)
                                allBorder = aBorderAfter || locked[a];
                            else
                                allBorder = locked[p];
                        }
                        if (allBorder)
                        {
                            makesBorderSliver = true;
                            break;
                        }
                    }
                    if (makesBorderSliver == false && aBorderAfter)
                    {
                        foreach (var t in vFaces[a])
                        {
                            if (willDie.Contains(t) || IndexOfVert(t, b) >= 0)
                                continue;
                            if (locked[t[0]] && locked[t[1]] && locked[t[2]])
                            {
                                makesBorderSliver = true;
                                break;
                            }
                        }
                    }
                    if (makesBorderSliver)
                    {
                        exclEdges.Add(EdgeKey(a, b));
#if VG_DEBUG
                        dbgLockReject++;
#endif
                        versions[a]++;
                        var bexcl = new HashSet<long>();
                        bexcl.Add(EdgeKey(a, b));
                        RebuildEdgesAround(m, heap, quadrics, versions, locked, vFaces[a], a, bexcl);
                        continue;
                    }

                    // 执行塌陷 b → a(移动到 target)
                    m.VX[a] = nx;
                    m.VZ[a] = nz;
                    m.VY[a] = ny;
                    for (int i = 0; i < 10; i++)
                        quadrics[a][i] += quadrics[b][i];
                    alive[b] = false;
                    versions[b]++;
                    versions[a]++;

                    // 记录本次塌陷对各面的误差贡献(切分后按面归集 cluster MaxError);
                    // 翻转不移动顶点, 不计入误差
                    foreach (var t in vFaces[a])
                    {
                        float old;
                        faceErr.TryGetValue(t, out old);
                        if ((float)cost > old)
                            faceErr[t] = (float)cost;
                    }
                    foreach (var t in vFaces[b])
                    {
                        float old;
                        faceErr.TryGetValue(t, out old);
                        if ((float)cost > old)
                            faceErr[t] = (float)cost;
                    }

                    // 执行删除: b 的面重指到 a, willDie 中的面(退化面 + 重复面)
                    // 从网格与面列表移除
                    var deadTris = willDie;
                    foreach (var t in vFaces[b])
                    {
                        int bi = IndexOfVert(t, b);
                        t[bi] = a;
                    }
                    vFaces[a].RemoveAll(t => deadTris.Contains(t));
                    // 死面必须从其全部三个顶点的面表中剔除:
                    // 否则残留的僵尸面会让孤立判定把死面当活面,
                    // 锁边顶点的最后一个真面被误删(破坏接缝硬约定①)
                    foreach (var t in deadTris)
                    {
                        m.Tris.Remove(t);
                        vFaces[t[0]].Remove(t);
                        vFaces[t[1]].Remove(t);
                        vFaces[t[2]].Remove(t);
                    }
                    // 合并 b 的面列表到 a(重指后 a 可能在 b 的面中出现两次的已被删)
                    foreach (var t in vFaces[b])
                    {
                        if (vFaces[a].Contains(t) == false)
                            vFaces[a].Add(t);
                    }
                    vFaces[b].Clear();

                    if (maxAppliedCost < (float)cost)
                        maxAppliedCost = (float)cost;

                    RebuildEdgesAround(m, heap, quadrics, versions, locked, vFaces[a], a);
                }
            } // pass 循环
#if VG_DEBUG
            int dbgAlive = 0;
            for (int i = 0; i < vertCount; i++)
                if (alive[i]) dbgAlive++;
            Console.WriteLine($"  Simplify out: tris={m.Tris.Count} alive={dbgAlive} maxErr={maxAppliedCost} flipRej={dbgFlipReject} lockRej={dbgLockReject} stale={dbgStale} costOver={dbgCostOver}");
#endif
            return maxAppliedCost;
        }

        static double dbgFirstCost = -1;
        static int dbgFlipReject = 0;
        static int dbgLockReject = 0;
        static int dbgFlipCount = 0;
        static int dbgStale = 0;
        static int dbgCostOver = 0;

        /// <summary>
        /// 从 CDLOD level 高度场直接烘焙。
        /// CDLOD level 采样数(如 1024)不满足 (size-1)%leafQuads==0 的 DAG 约定
        /// (1023 不能被 16 整除), 这里先双线性重采样到 nLeaf*leafQuads+1 (1025)
        /// 再 bake —— 纵向把 level 拉伸不到 1 个 texel, 远景尺度下可忽略。
        /// cancelState 非空时在重采样/叶子构建/层级简化检查点轮询取消标志, 被取消时返回 null (不落盘)。
        /// </summary>
        public static TtVgDagAsset BakeFromLevel(float[] heightfield, int srcSize, int leafQuads, float maxErrorPerLevel,
            Thread.Async.TtAsyncTaskStateBase cancelState = null, int preStep = 1)
        {
            if (heightfield == null || heightfield.Length != srcSize * srcSize)
                throw new ArgumentException("heightfield size mismatch");
            int nLeaf = Math.Max(1, (srcSize - 1 + leafQuads - 1) / leafQuads);
            int dstSize = nLeaf * leafQuads + 1;
            if (dstSize == srcSize)
                return new TtVgClusterBaker().Bake(heightfield, srcSize, leafQuads, maxErrorPerLevel, cancelState, preStep);

            var hf = new float[dstSize * dstSize];
            for (int z = 0; z < dstSize; z++)
            {
                if (cancelState?.IsCancelRequested == true)
                    return null; // 取消检查点: 重采样行循环
                float fz = Math.Min(z * (float)(srcSize - 1) / (dstSize - 1), srcSize - 1);
                int z0 = (int)fz;
                int z1 = Math.Min(z0 + 1, srcSize - 1);
                float tz = fz - z0;
                for (int x = 0; x < dstSize; x++)
                {
                    float fx = Math.Min(x * (float)(srcSize - 1) / (dstSize - 1), srcSize - 1);
                    int x0 = (int)fx;
                    int x1 = Math.Min(x0 + 1, srcSize - 1);
                    float tx = fx - x0;
                    float h00 = heightfield[z0 * srcSize + x0];
                    float h10 = heightfield[z0 * srcSize + x1];
                    float h01 = heightfield[z1 * srcSize + x0];
                    float h11 = heightfield[z1 * srcSize + x1];
                    hf[z * dstSize + x] = (h00 * (1 - tx) + h10 * tx) * (1 - tz)
                                        + (h01 * (1 - tx) + h11 * tx) * tz;
                }
            }
            return new TtVgClusterBaker().Bake(hf, dstSize, leafQuads, maxErrorPerLevel, cancelState, preStep);
        }

        /// <summary>
        /// 烘焙一块 heightfield 的 cluster DAG。
        /// maxErrorPerLevel: 每层 QEM 误差预算(代价超过即停止简化, 供 LOD 选择按误差分级)。
        /// cancelState 非空时在叶子构建/每层简化检查点轮询取消标志, 被取消时返回 null。
        /// </summary>
        public TtVgDagAsset Bake(float[] heightfield, int size, int leafQuads, float maxErrorPerLevel,
            Thread.Async.TtAsyncTaskStateBase cancelState = null, int preStep = 1)
        {
            if (heightfield == null || heightfield.Length != size * size)
                throw new ArgumentException("heightfield size mismatch");
            if (preStep < 1) preStep = 1;
            mPreStep = preStep;
            // 采样数 = quads + 1: 每边 (size-1) 个 quad, 相邻 cluster/Level 共享边界采样
            if ((size - 1) % leafQuads != 0)
                throw new ArgumentException("(size-1) must be a multiple of leafQuads");
            if ((leafQuads & (leafQuads - 1)) != 0)
                throw new ArgumentException("leafQuads must be power of two");

            mHeightfield = heightfield;
            mSize = size;
            mLeafQuads = leafQuads;

            int c0 = (size - 1) / leafQuads;
            int levelCount = 1;
            while ((c0 >> levelCount) >= 1 && (c0 >> (levelCount - 1)) > 1)
                levelCount++;

            var asset = new TtVgDagAsset();
            asset.HeightfieldSize = size;
            asset.LeafClusterQuads = leafQuads;
            asset.LevelCount = levelCount;
            asset.LevelErrors = new float[levelCount];

            float hMin = float.MaxValue, hMax = float.MinValue;
            foreach (var h in heightfield)
            {
                if (h < hMin) hMin = h;
                if (h > hMax) hMax = h;
            }
            asset.HeightMin = hMin;
            asset.HeightMax = hMax;

            // Quark 式层级构建(weld + 临时锁边):
            // 每层先把整块 heightfield 作为一张连续网格(等价于组内 weld, 同层
            // cluster 间不存在预先锁死的接缝), 只锁 heightfield 真实外圈
            // (跨 tile 接缝, 永久锁边), 全局简化后按 cluster 网格切分面归属。
            // 同层相邻 cluster 的共享边是同一条网格边链, 两侧引用逐位一致,
            // 三条接缝约定由构造保证; 每层都从完整规则网格独立简化,
            // 上层不继承子层窄三角形。
            var fullGrid = BuildFullGrid(mPreStep);
            int fullTris = fullGrid.Tris.Count;
            var levels = new List<List<LvlMesh>>(levelCount);
            {
                var leaves = new List<LvlMesh>(c0 * c0);
                for (int cz = 0; cz < c0; cz++)
                {
                    if (cancelState?.IsCancelRequested == true)
                        return null; // 取消检查点: 叶子构建 (c0² 次, 占总耗时大头)
                    for (int cx = 0; cx < c0; cx++)
                    {
                        leaves.Add(new LvlMesh { Mesh = BuildLeaf(cx, cz), Err = 0.0f });
                    }
                }
                levels.Add(leaves);
            }
            for (int level = 1; level < levelCount; level++)
            {
                if (cancelState?.IsCancelRequested == true)
                    return null; // 取消检查点: 每层 weld+QEM 简化 (次耗时大头)
                int n = c0 >> level;
                var simplified = fullGrid.Clone();
                // Quark 语义: cluster 三角形数恒定(≈ 叶子 cluster 的一半),
                // 覆盖面积逐层 ×4, 密度逐层 ÷4 → 每层总量 = fullTris/4^level,
                // 上层真正变粗且层级间拉开误差梯度; maxErrorPerLevel 仍作上限
                int targetTris = fullTris >> (2 * level);
                float lvMaxErr = SimplifyCluster(simplified, targetTris, maxErrorPerLevel, cancelState);
                if (lvMaxErr < 0)
                    return null; // 被取消: 中止整层, 不再继续上层
                var cur = SplitLevelToClusters(simplified, n, mLeafQuads << level);
                // 父误差单调: ParentErr = max(本层简化误差, 4 子误差)
                int pn = n * 2;
                for (int gz = 0; gz < n; gz++)
                {
                    for (int gx = 0; gx < n; gx++)
                    {
                        var lm = cur[gz * n + gx];
                        float childMax = Math.Max(
                            Math.Max(levels[level - 1][(gz * 2) * pn + gx * 2].Err,
                                     levels[level - 1][(gz * 2) * pn + gx * 2 + 1].Err),
                            Math.Max(levels[level - 1][(gz * 2 + 1) * pn + gx * 2].Err,
                                     levels[level - 1][(gz * 2 + 1) * pn + gx * 2 + 1].Err));
                        if (lm.Err < childMax)
                            lm.Err = childMax;
                        if (lm.Err > asset.LevelErrors[level])
                            asset.LevelErrors[level] = lm.Err;
                    }
                }
                levels.Add(cur);
            }

            // 组装资产: 层序(自底向上) row-major; 子层先入数组, 父层FirstChild可直算
            // 顶点池全局去重: 锁边顶点在相邻 cluster / 上下层间逐位一致, 共享同一池条目
            var vertList = new List<FVgVertex>();
            var idxList = new List<uint>();
            var clusters = new List<TtVgCluster>();
            var globalVerts = new Dictionary<long, int>();
            for (int level = 0; level < levelCount; level++)
            {
                int n = c0 >> level;
                var meshes = levels[level];
                for (int cz = 0; cz < n; cz++)
                {
                    for (int cx = 0; cx < n; cx++)
                    {
                        var lm = meshes[cz * n + cx];
                        var mesh = lm.Mesh;
                        var cluster = new TtVgCluster();
                        cluster.Level = level;
                        cluster.MinTexelX = mesh.MinX;
                        cluster.MinTexelZ = mesh.MinZ;
                        cluster.ExtentTexel = mesh.Extent;
                        cluster.MinY = mesh.MinY;
                        cluster.MaxY = mesh.MaxY;
                        cluster.MaxError = lm.Err;
                        if (level + 1 < levelCount)
                        {
                            // 子层在本层之前已写入数组
                            int childN = c0 >> (level + 1);
                            cluster.FirstChild = asset.GetLevelStart(level + 1) + (cz * 2) * childN + (cx * 2);
                        }
                        else
                            cluster.FirstChild = -1;
                        if (level == 0)
                        {
                            cluster.BaseVertex = -1;
                            cluster.IndexStart = 0;
                            cluster.IndexCount = 0;
                        }
                        else
                        {
                            var used = new Dictionary<int, int>();
                            cluster.BaseVertex = 0;
                            cluster.IndexStart = idxList.Count;
                            foreach (var t in mesh.Tris)
                            {
                                for (int k = 0; k < 3; k++)
                                {
                                    int vi = t[k];
                                    int outIdx;
                                    if (used.TryGetValue(vi, out outIdx) == false)
                                    {
                                        // 全局 key: xz 格点 + y 位型(锁边保证共享顶点逐位一致)
                                        long gkey = ((long)mesh.VX[vi] << 42) | ((long)mesh.VZ[vi] << 32)
                                                  | (uint)System.BitConverter.SingleToInt32Bits(mesh.VY[vi]);
                                        int gi;
                                        if (globalVerts.TryGetValue(gkey, out gi) == false)
                                        {
                                            gi = vertList.Count;
                                            globalVerts.Add(gkey, gi);
                                            vertList.Add(new FVgVertex((ushort)mesh.VX[vi], (ushort)mesh.VZ[vi], mesh.VY[vi]));
                                        }
                                        outIdx = gi;
                                        used.Add(vi, outIdx);
                                    }
                                    idxList.Add((uint)outIdx);
                                }
                            }
                            cluster.IndexCount = idxList.Count - cluster.IndexStart;
                        }
                        clusters.Add(cluster);
                    }
                }
            }
            asset.Vertices = vertList.ToArray();
            asset.Indices = idxList.ToArray();
            asset.Clusters = clusters.ToArray();
            return asset;
        }
    }
}
