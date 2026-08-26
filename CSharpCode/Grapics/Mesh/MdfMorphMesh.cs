using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    /// <summary>
    /// 蒙皮 + Morph target 的 MdfQueue: 角色表情/口型的主用类型。
    ///
    /// 为什么派生自 TtMdfSkinMesh 而不是直接写 TtMdfQueue2&lt;TtMorphModifier, TtSkinModifier&gt;:
    /// 引擎里有大量 "MdfQueue is TtMdfSkinMesh" / "as TtMdfSkinMesh" 的判断在给蒙皮喂数据
    /// (TtMeshNode 灌 PerSkinMeshCBuffer 骨骼矩阵、TtSkeletonAnimPlayNode / TtBlendSpaceAnimPlayNode
    /// 驱动动画)。如果做成 TtMdfSkinMesh 的兄弟类型, 这些判断全部落空, 表现为"挂上 morph 后
    /// 骨骼动画整体失效"。派生则天然复用 PerSkinMeshCBuffer 字段与其 OnDrawCall 里的 cbSkinMesh 绑定。
    ///
    /// modifier 顺序: 基类构造器先装入 TtSkinModifier, 这里把 TtMorphModifier 插到下标 0,
    /// 使 MdfQueueDoModifiers 里 DoMorphModifierVS 先于 DoSkinModifierVS 发射 —— morph delta
    /// 必须在骨骼加权之前叠加到顶点上。插入后必须重新 UpdateShaderCode() 让生成的 HLSL 反映新顺序。
    /// </summary>
    [Rtti.Meta("")]
    public class TtMdfSkinMorphMesh : TtMdfSkinMesh
    {
        public TtMdfSkinMorphMesh()
        {
            // 基类(TtMdfQueue1<TtSkinModifier>)构造时 Modifiers = [skin], 这里插到最前面
            Modifiers.Insert(0, new Mesh.Modifier.TtMorphModifier());
            UpdateShaderCode();
        }

        public Mesh.Modifier.TtMorphModifier MorphModifier
        {
            get => FindModifier<Mesh.Modifier.TtMorphModifier>();
        }

        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            // 基类负责搬 PerSkinMeshCBuffer
            base.CopyFrom(mdf);

            // 切换 MdfQueue 类型时保住当前表情, 否则切换瞬间表情会复位
            var srcMorph = mdf?.FindModifier<Mesh.Modifier.TtMorphModifier>();
            if (srcMorph != null)
            {
                MorphModifier?.CopyWeightsFrom(srcMorph);
            }
        }
    }

    /// <summary>
    /// 无骨骼的纯 Morph target MdfQueue: 用于可变形的静态网格(表情道具、形变机关等)。
    ///
    /// modifier 顺序上 morph 在前, TtStaticModifier 本身不产生 VS 函数, 只是与
    /// TtMdfStaticMesh 保持一致的组成结构。
    /// </summary>
    [Rtti.Meta("")]
    public class TtMdfMorphMesh : TtMdfQueue2<Mesh.Modifier.TtMorphModifier, Mesh.Modifier.TtStaticModifier>
    {
        public Mesh.Modifier.TtMorphModifier MorphModifier
        {
            get => FindModifier<Mesh.Modifier.TtMorphModifier>();
        }

        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);

            var srcMorph = mdf?.FindModifier<Mesh.Modifier.TtMorphModifier>();
            if (srcMorph != null)
            {
                MorphModifier?.CopyWeightsFrom(srcMorph);
            }
        }
    }
}
