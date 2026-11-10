using System;
using System.Collections.Generic;
using EngineNS.UnitTest;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// Cluster DAG 烘焙器的确定性 / 接缝约定单元测试。
    /// 覆盖: 两次 bake 逐位一致、Level 外圈锁边、同层相邻 cluster 共享边一致、
    /// 相邻 Level 根边框逐位重合、误差预算、顶点池规模。
    /// </summary>
    [TtTest]
    public class UTest_TtVgClusterBaker
    {
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
        /// <summary>确定性多倍频噪声高度场(不依赖引擎运行时)。</summary>
        static float[] GenHeightfield(int size, int seed)
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
                    hf[z * size + x] = h;
                }
            }
            return hf;
        }
        /// <summary>从一块大高度场裁出子块(两块子块共享数据来源)。</summary>
        static float[] SliceHeightfield(float[] src, int srcSize, int x0, int z0, int size)
        {
            var dst = new float[size * size];
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                    dst[z * size + x] = src[(z0 + z) * srcSize + (x0 + x)];
            }
            return dst;
        }

        void TestBakeDeterminism()
        {
            const int Size = 65; // 64 quads, 采样数 = quads + 1
            const int LeafQuads = 4;
            var hf = GenHeightfield(Size, 7);
            var baker = new TtVgClusterBaker();
            var a = baker.Bake(hf, Size, LeafQuads, 1600.0f);
            var b = new TtVgClusterBaker().Bake(hf, Size, LeafQuads, 1600.0f);
            var ba = a.ToBytes();
            var bb = b.ToBytes();
            TtUnitTestManager.TAssert(ba.Length == bb.Length, $"determinism: byte length {ba.Length} != {bb.Length}");
            for (int i = 0; i < ba.Length; i++)
            {
                TtUnitTestManager.TAssert(ba[i] == bb[i], $"determinism: byte mismatch at {i}");
            }
        }

        void TestSeamConventions()
        {
            const int Size = 65;
            const int LeafQuads = 4;
            const float MaxErr = 1600.0f; // QEM cost 量纲 = texel^2
            var hf = GenHeightfield(Size, 7);
            var asset = new TtVgClusterBaker().Bake(hf, Size, LeafQuads, MaxErr);
            TtUnitTestManager.TAssert(asset.LevelCount == 5, $"level count {asset.LevelCount} != 5");

            string err;
            bool ok = FVgDagSeamValidator.CheckAdjacentClusterSeams(asset, out err);
            TtUnitTestManager.TAssert(ok, $"adjacent cluster seam: {err}");
            ok = FVgDagSeamValidator.CheckLevelBorderLocked(asset, hf, out err);
            TtUnitTestManager.TAssert(ok, $"level border locked: {err}");

            // 误差预算: 每层实际最大误差不得超过预算
            for (int level = 1; level < asset.LevelCount; level++)
            {
                TtUnitTestManager.TAssert(asset.LevelErrors[level] <= MaxErr * 1.0001f,
                    $"level{level} error {asset.LevelErrors[level]} exceeds budget {MaxErr}");
            }
        }

        // 顶点预算在接近生产的叶子尺寸下评估: 小叶子(如 4 quads)锁边环占比约 40%,
        // 多层重复存储下 40% 预算不适用; 生产 32x32 叶子环占比仅约 12%。
        // Quark 式 weld + 仅锁 heightfield 外圈后, 全密度锁边环税已消除, 启用 40% 硬断言。
        void TestVertexBudget()
        {
            const int Size = 129;
            const int LeafQuads = 16;
            const float MaxErr = 1600.0f;
            var hf = GenHeightfield(Size, 7);
            var asset = new TtVgClusterBaker().Bake(hf, Size, LeafQuads, MaxErr);
            int leafVertTotal = ((Size - 1) / LeafQuads) * ((Size - 1) / LeafQuads) * (LeafQuads + 1) * (LeafQuads + 1);
            float ratio = (float)asset.Vertices.Length / leafVertTotal;
            System.Diagnostics.Debug.WriteLine($"VGDag vertex pool = {asset.Vertices.Length}, leaf verts = {leafVertTotal}, ratio = {ratio:F3}");
            TtUnitTestManager.TAssert(asset.Vertices.Length <= leafVertTotal * 0.40f,
                $"vertex pool ratio {ratio:F3} exceeds 0.40 (pool={asset.Vertices.Length}, leaf={leafVertTotal})");
        }

        void TestAdjacentLevelSeam()
        {
            const int TileSize = 65; // 64 quads; 相邻 tile 共享边界采样
            const int LeafQuads = 4;
            const float MaxErr = 1600.0f;
            // 两块相邻 tile 来自同一块确定性高度场, 共享边列/行逐位一致
            int wideSize = TileSize * 2 - 1;
            var wide = GenHeightfield(wideSize, 11);
            var hfA = SliceHeightfield(wide, wideSize, 0, 0, TileSize);
            var hfB = SliceHeightfield(wide, wideSize, TileSize - 1, 0, TileSize);
            var assetA = new TtVgClusterBaker().Bake(hfA, TileSize, LeafQuads, MaxErr);
            var assetB = new TtVgClusterBaker().Bake(hfB, TileSize, LeafQuads, MaxErr);

            string err;
            bool ok = FVgDagSeamValidator.CheckAdjacentLevelSeam(assetA, assetB, true, out err);
            TtUnitTestManager.TAssert(ok, $"east seam: {err}");

            var hfC = SliceHeightfield(wide, wideSize, 0, TileSize - 1, TileSize);
            var assetC = new TtVgClusterBaker().Bake(hfC, TileSize, LeafQuads, MaxErr);
            ok = FVgDagSeamValidator.CheckAdjacentLevelSeam(assetA, assetC, false, out err);
            TtUnitTestManager.TAssert(ok, $"south seam: {err}");
        }

        public void UnitTestEntrance()
        {
            TestBakeDeterminism();
            TestSeamConventions();
            TestVertexBudget();
            TestAdjacentLevelSeam();
        }
    }
}
