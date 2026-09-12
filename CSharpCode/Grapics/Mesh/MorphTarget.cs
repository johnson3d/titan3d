using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    /// <summary>
    /// GPU 端逐顶点稠密累加后的 morph 偏移量。
    ///
    /// 由 TtMorphModifier 每帧(权重变化时)把所有激活 morph 的稀疏 delta 加权累加成一张
    /// 与顶点数等长的稠密表, VS 端按 vVertexID 单次索引读取。
    ///
    /// 字段必须以 m 开头: 引擎在 ShaderCode.cs 里 Debug.Assert(name[0]=='m') 并剥掉首字母
    /// 生成 HLSL 字段名 (mDeltaPosition -> DeltaPosition), 详见 CodingGuidelines.md §3.2。
    /// HLSL 端不要手写同名 struct, 由反射注入。
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FMorphDelta")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct FMorphDelta
    {
        public Vector3 mDeltaPosition;
        public Vector3 mDeltaNormal;
        /// <summary>
        /// 切线 delta。与法线同源(导入器用形变后的位置 + UV 重算), VS 端相加后再沿 morph
        /// 后的法线重正交化。没有它的话法线贴图会一直用基础网格的切线基, 形变越大偏得越多。
        /// </summary>
        public Vector3 mDeltaTangent;
    }

    /// <summary>
    /// 资产端的稀疏 morph 条目: 只记录被该 morph 真正影响到的顶点。
    ///
    /// 一个表情 morph 通常只影响网格的几百个顶点, 全量存副本会让资产体积按 morph 数量
    /// 线性膨胀, 所以按稀疏存。这不是 GPU 结构, 字段名无需 m 前缀。
    ///
    /// 长度单位为米 (CodingGuidelines.md §6)。
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    public struct FMorphVertexDelta
    {
        /// <summary> 展开后的顶点索引(不是 FBX 控制点索引)。 </summary>
        public uint VertexIndex;
        public Vector3 DeltaPosition;
        public Vector3 DeltaNormal;
        /// <summary>
        /// 切线 delta (只有 xyz; 手性 w 不随 morph 改变, 因为 morph 不改 UV 拓扑)。
        /// 加于 Version 2, v1 资产读进来时留零向量。
        /// </summary>
        public Vector3 DeltaTangent;
    }

    /// <summary>
    /// 一个命名的 morph target (BlendShape)。名字来源于 DCC 中的 blendshape channel 名,
    /// 运行时按名字引用 —— 不要按索引引用, 重导入后索引会漂移。
    /// </summary>
    public class TtMorphTarget
    {
        public string Name { get; set; } = "NewMorph";

        /// <summary> 稀疏 delta 条目。按 VertexIndex 升序存放(导入期排好), 便于将来改 CSR。 </summary>
        public FMorphVertexDelta[] Deltas { get; set; } = Array.Empty<FMorphVertexDelta>();

        public int DeltaCount { get => Deltas != null ? Deltas.Length : 0; }
    }

    /// <summary>
    /// 一个 mesh 的全部 morph target 集合, 作为 mesh 资产的附属数据存在 XND attribute 里
    /// (与 PartialSkeleton 同一模式, 见 TtMeshPrimitives.SaveAssetTo / LoadXnd)。
    ///
    /// 序列化走显式二进制而不是反射: morph 数据量大, 且稀疏条目是纯 POD 数组, 直接
    /// WritePtr 一次搬完比逐属性反射快得多。
    /// </summary>
    public class TtMorphTargetSet
    {
        /// <summary> XND attribute 名, 读写两端共用。 </summary>
        public const string AttributeName = "MorphTargets";

        /// <summary>
        /// 数据版本。加字段时递增, 并在 Load 里按版本分支, 保证旧资产仍可读。
        ///
        /// v2: FMorphVertexDelta 增加 DeltaTangent。
        /// </summary>
        public const int CurrentVersion = 2;

        /// <summary>
        /// Version 1 的稀疏条目布局(没有切线 delta)。
        ///
        /// 保留它是硬性要求, 不是为了好看: Save/Load 走的是 WritePtr/ReadPtr 整块搬运,
        /// 结构体一加字段 sizeof 就变, 拿新布局去读 v1 数据会让每条记录整体错位 ——
        /// 而且不会抛任何异常, 只会得到一堆乱跳的顶点和随机法线, 极难往"序列化布局"上想。
        /// 以后每次给 FMorphVertexDelta 加字段, 都必须同步新增一个这样的旧布局副本。
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
        private struct FMorphVertexDeltaV1
        {
            public uint VertexIndex;
            public Vector3 DeltaPosition;
            public Vector3 DeltaNormal;
        }

        /// <summary>
        /// 该 mesh 展开后的顶点总数。稠密累加表按这个长度分配; 同时用于加载时校验
        /// morph 数据与当前 mesh 是否匹配(顶点数不一致说明 mesh 被重导入过而 morph 没跟上)。
        /// </summary>
        public int VertexCount { get; set; } = 0;

        public List<TtMorphTarget> Targets { get; set; } = new List<TtMorphTarget>();

        public bool IsValid
        {
            get => VertexCount > 0 && Targets != null && Targets.Count > 0;
        }

        /// <summary>
        /// 按名字查 morph 索引, 找不到返回 -1。运行时设权重的唯一入口, 不要让外部按索引猜。
        /// </summary>
        public int FindTargetIndex(string name)
        {
            if (Targets == null || string.IsNullOrEmpty(name))
                return -1;
            for (int i = 0; i < Targets.Count; i++)
            {
                if (Targets[i] != null && Targets[i].Name == name)
                    return i;
            }
            return -1;
        }

        public unsafe void Save<TW>(IO.AuxWriter<TW> ar) where TW : IO.ICoreWriter
        {
            ar.Write(CurrentVersion);
            ar.Write(VertexCount);

            int count = Targets != null ? Targets.Count : 0;
            ar.Write(count);
            for (int i = 0; i < count; i++)
            {
                var target = Targets[i];
                ar.Write(target.Name);

                int deltaCount = target.DeltaCount;
                ar.Write(deltaCount);
                if (deltaCount > 0)
                {
                    fixed (FMorphVertexDelta* p = &target.Deltas[0])
                    {
                        ar.WritePtr(p, deltaCount * sizeof(FMorphVertexDelta));
                    }
                }
            }
        }

        /// <summary>
        /// 读取失败(版本不认识/数据损坏)时返回 null, 由调用方按"没有 morph"处理,
        /// 不要抛异常打断整个 mesh 的加载。
        /// </summary>
        public static unsafe TtMorphTargetSet Load<TR>(IO.AuxReader<TR> ar) where TR : IO.ICoreReader
        {
            ar.Read(out int version);
            if (version <= 0 || version > CurrentVersion)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "Mesh",
                    $"TtMorphTargetSet: unsupported version {version}, morph data ignored");
                return null;
            }

            var result = new TtMorphTargetSet();
            ar.Read(out int vertexCount);
            result.VertexCount = vertexCount;

            ar.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ar.Read(out string name);
                ar.Read(out int deltaCount);

                var target = new TtMorphTarget();
                target.Name = name;
                if (deltaCount > 0)
                {
                    var deltas = new FMorphVertexDelta[deltaCount];
                    if (version >= 2)
                    {
                        fixed (FMorphVertexDelta* p = &deltas[0])
                        {
                            ar.ReadPtr(p, deltaCount * sizeof(FMorphVertexDelta));
                        }
                    }
                    else
                    {
                        // v1: 按旧布局读进来再逐条搬字段。DeltaTangent 留零向量 ——
                        // VS 端的重正交化在 delta 为零时依然工作, 所以旧资产不会因为
                        // 缺这个字段而比升级前更差, 只是拿不到切线绕法线旋转那部分精度。
                        var legacy = new FMorphVertexDeltaV1[deltaCount];
                        fixed (FMorphVertexDeltaV1* p = &legacy[0])
                        {
                            ar.ReadPtr(p, deltaCount * sizeof(FMorphVertexDeltaV1));
                        }
                        for (int d = 0; d < deltaCount; d++)
                        {
                            deltas[d].VertexIndex = legacy[d].VertexIndex;
                            deltas[d].DeltaPosition = legacy[d].DeltaPosition;
                            deltas[d].DeltaNormal = legacy[d].DeltaNormal;
                        }
                    }
                    target.Deltas = deltas;
                }
                result.Targets.Add(target);
            }
            return result;
        }
    }
}
