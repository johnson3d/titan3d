using System;
using System.IO;

namespace EngineNS.Bricks.Terrain.CDLOD.VGDag
{
    /// <summary>
    /// 全局三角形对角约定:
    /// 每个 quad 的四个角记 a=(x,z), b=(x+1,z), c=(x+1,z+1), d=(x,z+1),
    /// 对角线固定为 a→c, 两个三角形为 (a,b,c) 与 (a,c,d)。
    /// 原生 FQuarkDAGBuilder::BuildTerrain 的输入网格与 M2 误差参考面 (TerrainError) 都遵守此约定,
    /// 运行时若用 heightmap 重建全分辨率网格, 也必须用同一剖分, 否则 LODError 不再是上界。
    /// </summary>
    public static class FVgDiagonalConvention
    {
        /// <summary>对角线方向: (x,z) → (x+1,z+1)</summary>
        public const bool DiagonalToLowerRight = true;
    }

    /// <summary>
    /// DAG 顶点: xz 是 heightfield texel 格点 (Quark 地形简化只删点不移点),
    /// y 是 bake 时定死的高度 (与源采样位级一致)。
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
    /// Quark cluster。顶点在 Vertices[BaseVertex, BaseVertex + VertexCount) 连续存放,
    /// Indices 是相对 BaseVertex 的局部下标。
    /// 运行时选层 (组一致判定, 同一 group 的兄弟 cluster 必然同进同退):
    ///   visible = (GeneratingGroup &lt; 0 || Proj(Groups[GeneratingGroup]) ≤ τ)
    ///          &amp;&amp; (ParentGroup &lt; 0 || Proj(Groups[ParentGroup]) &gt; τ)
    /// Proj 使用 group 的 ParentLODError 与 group 包围盒; 两者沿 DAG 单调, 保证无缝无重叠。
    /// </summary>
    public struct FVgCluster
    {
        public int MipLevel;
        public int BaseVertex;
        public int VertexCount;
        public int IndexStart;
        public int IndexCount;
        /// <summary>相对全分辨率高度场的竖直 L∞ 上界 (高度单位); 叶子在 Step == 1 时为 0。</summary>
        public float LODError;
        /// <summary>生成本 cluster 的 group; 叶子为 -1。</summary>
        public int GeneratingGroup;
        /// <summary>以本 cluster 为子节点的 group; 根为 -1。</summary>
        public int ParentGroup;
        public ushort MinTexelX;
        public ushort MinTexelZ;
        public ushort MaxTexelX;
        public ushort MaxTexelZ;
        public float MinY;
        public float MaxY;
    }

    /// <summary>
    /// cluster group。ParentLODError = max(本组简化误差, 子 cluster 最大误差), 沿 DAG 单调;
    /// 包围盒 = 子 cluster 包围盒 ∪ 子 cluster 的生成 group 包围盒, 同样单调。
    /// </summary>
    public struct FVgGroup
    {
        public int MipLevel;
        public float ParentLODError;
        public ushort MinTexelX;
        public ushort MinTexelZ;
        public ushort MaxTexelX;
        public ushort MaxTexelZ;
        public float MinY;
        public float MaxY;
    }

    /// <summary>
    /// 单块 heightfield 的 Quark cluster DAG 资产 (纯数据, 不含引擎运行时依赖)。
    /// 由 TtQuarkTerrainBaker 产出; 确定性 bake 保证同输入两次产出逐位一致。
    /// </summary>
    public class TtVgDagAsset
    {
        /// <summary>'QVGD' (小端)。</summary>
        public const uint Magic = 0x44475651;
        public const int FormatVersion = 1;
        const int MaxHeightfieldSize = 65536;

        public int HeightfieldSize;
        /// <summary>叶子网格的降采样步长 (texel); 行列取 {0, Step, 2Step, …} ∪ {Size-1}。</summary>
        public int Step = 1;
        public int MipLevelCount;
        public float HeightMin;
        public float HeightMax;
        public FVgVertex[] Vertices;
        public ushort[] Indices;
        public FVgCluster[] Clusters;
        public FVgGroup[] Groups;

        public void Save(BinaryWriter w)
        {
            w.Write(Magic);
            w.Write(FormatVersion);
            w.Write(HeightfieldSize);
            w.Write(Step);
            w.Write(MipLevelCount);
            w.Write(HeightMin);
            w.Write(HeightMax);
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
                w.Write(c.MipLevel);
                w.Write(c.BaseVertex);
                w.Write(c.VertexCount);
                w.Write(c.IndexStart);
                w.Write(c.IndexCount);
                w.Write(c.LODError);
                w.Write(c.GeneratingGroup);
                w.Write(c.ParentGroup);
                w.Write(c.MinTexelX);
                w.Write(c.MinTexelZ);
                w.Write(c.MaxTexelX);
                w.Write(c.MaxTexelZ);
                w.Write(c.MinY);
                w.Write(c.MaxY);
            }
            w.Write(Groups.Length);
            foreach (var g in Groups)
            {
                w.Write(g.MipLevel);
                w.Write(g.ParentLODError);
                w.Write(g.MinTexelX);
                w.Write(g.MinTexelZ);
                w.Write(g.MaxTexelX);
                w.Write(g.MaxTexelZ);
                w.Write(g.MinY);
                w.Write(g.MaxY);
            }
        }
        public byte[] ToBytes()
        {
            string error;
            if (!Validate(out error))
                throw new InvalidOperationException($"Invalid VGDag asset: {error}");
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                Save(w);
                w.Flush();
                return ms.ToArray();
            }
        }

        static int ReadCount(BinaryReader r, int elementSize)
        {
            int count = r.ReadInt32();
            long remaining = r.BaseStream.Length - r.BaseStream.Position;
            if (count < 0 || (long)count * elementSize > remaining)
                throw new InvalidDataException($"VGDag array count {count} exceeds stream");
            return count;
        }

        /// <summary>格式或内容非法时抛 InvalidDataException, 不返回半成品。</summary>
        public static TtVgDagAsset Load(BinaryReader r)
        {
            if (r.ReadUInt32() != Magic)
                throw new InvalidDataException("VGDag magic mismatch");
            int version = r.ReadInt32();
            if (version != FormatVersion)
                throw new InvalidDataException($"VGDag format version {version} unsupported");
            var asset = new TtVgDagAsset();
            asset.HeightfieldSize = r.ReadInt32();
            asset.Step = r.ReadInt32();
            asset.MipLevelCount = r.ReadInt32();
            asset.HeightMin = r.ReadSingle();
            asset.HeightMax = r.ReadSingle();
            asset.Vertices = new FVgVertex[ReadCount(r, 8)];
            for (int i = 0; i < asset.Vertices.Length; i++)
            {
                FVgVertex v;
                v.TexelX = r.ReadUInt16();
                v.TexelZ = r.ReadUInt16();
                v.Y = r.ReadSingle();
                asset.Vertices[i] = v;
            }
            asset.Indices = new ushort[ReadCount(r, 2)];
            for (int i = 0; i < asset.Indices.Length; i++)
                asset.Indices[i] = r.ReadUInt16();
            asset.Clusters = new FVgCluster[ReadCount(r, 48)];
            for (int i = 0; i < asset.Clusters.Length; i++)
            {
                FVgCluster c;
                c.MipLevel = r.ReadInt32();
                c.BaseVertex = r.ReadInt32();
                c.VertexCount = r.ReadInt32();
                c.IndexStart = r.ReadInt32();
                c.IndexCount = r.ReadInt32();
                c.LODError = r.ReadSingle();
                c.GeneratingGroup = r.ReadInt32();
                c.ParentGroup = r.ReadInt32();
                c.MinTexelX = r.ReadUInt16();
                c.MinTexelZ = r.ReadUInt16();
                c.MaxTexelX = r.ReadUInt16();
                c.MaxTexelZ = r.ReadUInt16();
                c.MinY = r.ReadSingle();
                c.MaxY = r.ReadSingle();
                asset.Clusters[i] = c;
            }
            asset.Groups = new FVgGroup[ReadCount(r, 24)];
            for (int i = 0; i < asset.Groups.Length; i++)
            {
                FVgGroup g;
                g.MipLevel = r.ReadInt32();
                g.ParentLODError = r.ReadSingle();
                g.MinTexelX = r.ReadUInt16();
                g.MinTexelZ = r.ReadUInt16();
                g.MaxTexelX = r.ReadUInt16();
                g.MaxTexelZ = r.ReadUInt16();
                g.MinY = r.ReadSingle();
                g.MaxY = r.ReadSingle();
                asset.Groups[i] = g;
            }
            if (r.BaseStream.Position != r.BaseStream.Length)
                throw new InvalidDataException("VGDag trailing bytes");
            string error;
            if (!asset.Validate(out error))
                throw new InvalidDataException($"Invalid VGDag asset: {error}");
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

        static bool IsFiniteNonNegative(float v)
        {
            return float.IsFinite(v) && v >= 0.0f;
        }

        static bool BoxContains(ushort minX, ushort minZ, ushort maxX, ushort maxZ, float minY, float maxY,
            ushort iMinX, ushort iMinZ, ushort iMaxX, ushort iMaxZ, float iMinY, float iMaxY)
        {
            return minX <= iMinX && minZ <= iMinZ && maxX >= iMaxX && maxZ >= iMaxZ && minY <= iMinY && maxY >= iMaxY;
        }

        /// <summary>
        /// 结构与 DAG 不变量校验: 范围/越界、cluster 包围盒与顶点一致、
        /// group 引用合法且父子层号一致、误差与包围盒沿 DAG 单调。
        /// 加载与写盘前都会调用, 运行时可以无条件信任通过校验的资产。
        /// </summary>
        public bool Validate(out string error)
        {
            error = null;
            if (HeightfieldSize < 2 || HeightfieldSize > MaxHeightfieldSize || Step < 1 || Step >= HeightfieldSize)
            {
                error = $"bad header size={HeightfieldSize} step={Step}";
                return false;
            }
            if (Vertices == null || Indices == null || Clusters == null || Groups == null || Clusters.Length == 0)
            {
                error = "missing arrays";
                return false;
            }
            if (MipLevelCount < 1 || !float.IsFinite(HeightMin) || !float.IsFinite(HeightMax) || HeightMin > HeightMax)
            {
                error = $"bad header mips={MipLevelCount} height=[{HeightMin},{HeightMax}]";
                return false;
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                var v = Vertices[i];
                if (v.TexelX >= HeightfieldSize || v.TexelZ >= HeightfieldSize || !float.IsFinite(v.Y) || v.Y < HeightMin || v.Y > HeightMax)
                {
                    error = $"vertex[{i}] out of range";
                    return false;
                }
            }

            var childCount = new int[Groups.Length];
            var parentCount = new int[Groups.Length];
            for (int i = 0; i < Clusters.Length; i++)
            {
                var c = Clusters[i];
                if (c.MipLevel < 0 || c.MipLevel >= MipLevelCount || !IsFiniteNonNegative(c.LODError))
                {
                    error = $"cluster[{i}] bad mip/error";
                    return false;
                }
                if (c.BaseVertex < 0 || c.VertexCount <= 0 || c.VertexCount > ushort.MaxValue + 1 ||
                    (long)c.BaseVertex + c.VertexCount > Vertices.Length ||
                    c.IndexStart < 0 || c.IndexCount <= 0 || c.IndexCount % 3 != 0 ||
                    (long)c.IndexStart + c.IndexCount > Indices.Length)
                {
                    error = $"cluster[{i}] range out of bounds";
                    return false;
                }
                ushort minX = ushort.MaxValue, minZ = ushort.MaxValue, maxX = 0, maxZ = 0;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (int k = 0; k < c.IndexCount; k++)
                {
                    int local = Indices[c.IndexStart + k];
                    if (local >= c.VertexCount)
                    {
                        error = $"cluster[{i}] index out of range";
                        return false;
                    }
                    var v = Vertices[c.BaseVertex + local];
                    minX = Math.Min(minX, v.TexelX);
                    minZ = Math.Min(minZ, v.TexelZ);
                    maxX = Math.Max(maxX, v.TexelX);
                    maxZ = Math.Max(maxZ, v.TexelZ);
                    minY = Math.Min(minY, v.Y);
                    maxY = Math.Max(maxY, v.Y);
                }
                if (c.MinTexelX != minX || c.MinTexelZ != minZ || c.MaxTexelX != maxX || c.MaxTexelZ != maxZ ||
                    c.MinY != minY || c.MaxY != maxY)
                {
                    error = $"cluster[{i}] bounds mismatch";
                    return false;
                }
                if (c.GeneratingGroup < -1 || c.GeneratingGroup >= Groups.Length ||
                    c.ParentGroup < -1 || c.ParentGroup >= Groups.Length)
                {
                    error = $"cluster[{i}] group reference out of range";
                    return false;
                }
                if ((c.GeneratingGroup < 0) != (c.MipLevel == 0))
                {
                    error = $"cluster[{i}] leaf/generating group mismatch";
                    return false;
                }
                if (c.GeneratingGroup >= 0)
                {
                    var gen = Groups[c.GeneratingGroup];
                    parentCount[c.GeneratingGroup]++;
                    if (gen.MipLevel + 1 != c.MipLevel || gen.ParentLODError != c.LODError ||
                        !BoxContains(gen.MinTexelX, gen.MinTexelZ, gen.MaxTexelX, gen.MaxTexelZ, gen.MinY, gen.MaxY,
                            c.MinTexelX, c.MinTexelZ, c.MaxTexelX, c.MaxTexelZ, c.MinY, c.MaxY))
                    {
                        error = $"cluster[{i}] inconsistent with generating group {c.GeneratingGroup}";
                        return false;
                    }
                }
                if (c.ParentGroup >= 0)
                {
                    if (c.ParentGroup == c.GeneratingGroup)
                    {
                        error = $"cluster[{i}] group cycle";
                        return false;
                    }
                    var par = Groups[c.ParentGroup];
                    childCount[c.ParentGroup]++;
                    if (par.MipLevel != c.MipLevel || c.LODError > par.ParentLODError ||
                        !BoxContains(par.MinTexelX, par.MinTexelZ, par.MaxTexelX, par.MaxTexelZ, par.MinY, par.MaxY,
                            c.MinTexelX, c.MinTexelZ, c.MaxTexelX, c.MaxTexelZ, c.MinY, c.MaxY))
                    {
                        error = $"cluster[{i}] not monotonic under parent group {c.ParentGroup}";
                        return false;
                    }
                    if (c.GeneratingGroup >= 0)
                    {
                        var gen = Groups[c.GeneratingGroup];
                        if (gen.ParentLODError > par.ParentLODError ||
                            !BoxContains(par.MinTexelX, par.MinTexelZ, par.MaxTexelX, par.MaxTexelZ, par.MinY, par.MaxY,
                                gen.MinTexelX, gen.MinTexelZ, gen.MaxTexelX, gen.MaxTexelZ, gen.MinY, gen.MaxY))
                        {
                            error = $"group {c.GeneratingGroup} not monotonic under group {c.ParentGroup}";
                            return false;
                        }
                    }
                }
            }
            for (int g = 0; g < Groups.Length; g++)
            {
                var gr = Groups[g];
                // 无子或无父产物的 group 会让 cut 出现空洞
                if (childCount[g] == 0 || parentCount[g] == 0 || gr.MipLevel < 0 || gr.MipLevel + 1 >= MipLevelCount || !IsFiniteNonNegative(gr.ParentLODError) ||
                    gr.MinTexelX > gr.MaxTexelX || gr.MinTexelZ > gr.MaxTexelZ || gr.MaxTexelX >= HeightfieldSize ||
                    gr.MaxTexelZ >= HeightfieldSize || !float.IsFinite(gr.MinY) || !float.IsFinite(gr.MaxY) || gr.MinY > gr.MaxY)
                {
                    error = $"group[{g}] invalid";
                    return false;
                }
            }
            return true;
        }
    }
}
