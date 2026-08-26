using EngineNS.Animation;
using EngineNS.Graphics.Mesh.PhysicsAsset;
using EngineNS.IO;
using System;

namespace EngineNS.Bricks.Animation.KawaiiPhysics
{
    /// <summary>
    /// Forward axis direction of the tip virtual bone.
    /// </summary>
    public enum EKawaiiTailBoneAxis : byte
    {
        X_Positive = 0,
        X_Negative,
        Y_Positive,
        Y_Negative,
        Z_Positive,
        Z_Negative,
    }

    /// <summary>
    /// 物理参数曲线的采样模式(与 native KawaiiTypes.h EKawaiiCurveEvalMode 一致)。
    /// </summary>
    public enum EKawaiiCurveEvalMode : byte
    {
        /// <summary>按关键点索引: i/(n-1)。</summary>
        IndexRate = 0,
        /// <summary>按链长归一化位置: LengthFromRoot/TotalLength。</summary>
        LengthRate,
        /// <summary>按节点内最长链归一化(保留与上游对齐, 目前同 LengthRate 处理)。</summary>
        AbsoluteLengthRate,
    }

    /// <summary>
    /// Managed, serializable wrapper that INTERNALLY holds the native value-struct
    /// EngineNS.KawaiiPhysics.FKawaiiPhySettings and exposes its fields as [Rtti.Meta]
    /// properties so the engine serializer can persist them.
    ///
    /// Why not AuxPtrType&lt;FKawaiiPhySettings&gt;: AuxPtrType is for ref-counted native heap
    /// objects (IUnknown, real NativePointer). FKawaiiPhySettings is a plain inline value
    /// struct whose NativePointer is always Zero, so there is nothing to ref-count/dispose.
    /// We therefore hold it by value and view its raw m_ fields (managed access, no P/Invoke).
    /// Convert to the native struct at the solver boundary via ToNative().
    /// </summary>
    public class TtKawaiiPhySettings : BaseSerializer
    {
        private EngineNS.KawaiiPhysics.FKawaiiPhySettings mSettings;

        public TtKawaiiPhySettings()
        {
            // A native value-struct constructed in managed memory is zero-initialised and
            // does NOT get the C++ default values; apply the engine defaults explicitly.
            // Lengths are in mesh space = METERS (see FKawaiiPhySettings in KawaiiPhySettings.h);
            // upstream KawaiiPhysics is a UE plugin and authored these in centimeters.
            mSettings.m_Stiffness = 0.05f;
            mSettings.m_Damping = 0.1f;
            mSettings.m_WorldDampingLocation = 0.8f;
            mSettings.m_WorldDampingRotation = 0.8f;
            mSettings.m_LimitAngle = 0.0f;
            mSettings.m_Radius = 0.03f;      // meters (upstream cm default was 3.0)
            mSettings.m_WindCoefficient = 1.0f;
            mSettings.m_DragCoefficient = 0.0f;
            mSettings.m_MaxFrameDisplacement = 0.0f;
        }

        /// <summary> Pull back toward the animated pose: 0 = pure physics (never returns to the
        /// animated shape), 1 = snap to animation. </summary>
        [Rtti.Meta]
        public float Stiffness { get => mSettings.m_Stiffness; set => mSettings.m_Stiffness = value; }
        [Rtti.Meta]
        public float Damping { get => mSettings.m_Damping; set => mSettings.m_Damping = value; }
        /// <summary> How much of the actor's world motion this particle does NOT follow:
        /// 1 = keeps its world position (max flow), 0 = rigidly glued to the actor. </summary>
        [Rtti.Meta]
        public float WorldDampingLocation { get => mSettings.m_WorldDampingLocation; set => mSettings.m_WorldDampingLocation = value; }
        [Rtti.Meta]
        public float WorldDampingRotation { get => mSettings.m_WorldDampingRotation; set => mSettings.m_WorldDampingRotation = value; }
        [Rtti.Meta]
        public float LimitAngle { get => mSettings.m_LimitAngle; set => mSettings.m_LimitAngle = value; }
        /// <summary> Collision radius in meters (upstream cm default was 3.0). </summary>
        [Rtti.Meta]
        public float Radius { get => mSettings.m_Radius; set => mSettings.m_Radius = value; }
        [Rtti.Meta]
        public float WindCoefficient { get => mSettings.m_WindCoefficient; set => mSettings.m_WindCoefficient = value; }
        [Rtti.Meta]
        public float DragCoefficient { get => mSettings.m_DragCoefficient; set => mSettings.m_DragCoefficient = value; }
        [Rtti.Meta]
        public float MaxFrameDisplacement { get => mSettings.m_MaxFrameDisplacement; set => mSettings.m_MaxFrameDisplacement = value; }

        /// <summary> Return the wrapped native value-struct for passing into the solver. </summary>
        public EngineNS.KawaiiPhysics.FKawaiiPhySettings ToNative() => mSettings;

        /// <summary> Deep copy (setups previously copied by value when this was a struct). </summary>
        public TtKawaiiPhySettings Clone()
        {
            var r = new TtKawaiiPhySettings();
            r.mSettings = mSettings; // struct value-copy
            return r;
        }
    }

    public static class TtKawaiiPhysicsSetupDefaults
    {
        public static TtKawaiiPhySettings CreatePhysicsSettings()
        {
            return new TtKawaiiPhySettings()
            {
                Stiffness = 0.05f,
                Damping = 0.1f,
                WorldDampingLocation = 0.8f,
                WorldDampingRotation = 0.8f,
                LimitAngle = 0.0f,
                Radius = 0.03f,          // meters (upstream cm default was 3.0)
                WindCoefficient = 1.0f,
                DragCoefficient = 0.0f,
                MaxFrameDisplacement = 0.0f,
            };
        }

        public static TtKawaiiPhySettings CreatePhysicsSettingsRandom()
        {
            return new TtKawaiiPhySettings()
            {
                Stiffness = 0.0f,
                Damping = 0.0f,
                WorldDampingLocation = 0.0f,
                WorldDampingRotation = 0.0f,
                LimitAngle = 0.0f,
                Radius = 0.0f,
                WindCoefficient = 0.0f,
                DragCoefficient = 0.0f,
                MaxFrameDisplacement = 0.0f,
            };
        }
    }

    /// <summary>
    /// Chain simulation setup data. Configures a single bone chain for physics simulation.
    /// </summary>
    [EGui.Controls.PropertyGrid.TtPropertyOrder(Order = EGui.Controls.PropertyGrid.TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtKawaiiChainSetup : BaseSerializer
    {
        [Rtti.Meta]
        public string Name { get; set; } = "Chain";

        [Rtti.Meta]
        [TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton RootBoneIndex { get; set; }

        [Rtti.Meta]
        [TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton EndBoneIndex { get; set; }

        [Rtti.Meta]
        public float TailBoneLength { get; set; } = 0.0f;

        [Rtti.Meta]
        public EKawaiiTailBoneAxis TailBoneAxis { get; set; } = EKawaiiTailBoneAxis.X_Positive;

        [Rtti.Meta]
        public bool ConstrainBoneLength { get; set; } = true;

        [Rtti.Meta]
        public float BoneLengthConstraintBlend { get; set; } = 1.0f;

        [Rtti.Meta]
        public bool RootCollision { get; set; } = false;

        /// <summary>
        /// LOD threshold. -1 means always active.
        /// </summary>
        [Rtti.Meta]
        public int LODThreshold { get; set; } = -1;

        [Rtti.Meta]
        public TtKawaiiPhySettings PhysicsSettings { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettings();

        [Rtti.Meta]
        public TtKawaiiPhySettings PhysicsSettingsRandom { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettingsRandom();

        // ─── 沿链逐骨骼参数曲线 ────────────────────────────────────────
        // 运行时每粒子按 PhysicsCurveMode 采样得到 rate, 将对应基准标量乘以
        // Curve.Eval(rate)(曲线为空时乘 1, 即全链一致, 与旧行为兼容)。
        //
        // 下面的 YMax 只是编辑器量程的**默认值**, 每条曲线可在面板上自己改(存在
        // TtKawaiiCurve.ViewYMin/ViewYMax 里随资产持久化)。
        // Stiffness/Damping/WorldDamping* 四项最终会被 clamp 到 0..1, 但曲线上限不能
        // 封在 1 —— 两个 0..1 相乘只会更小, 封顶就只能衰减。给 4 倍余量, 使
        // "base 取小值 + 曲线局部拉到 1" 这种用法可行。

        [Rtti.Meta]
        public EKawaiiCurveEvalMode PhysicsCurveMode { get; set; } = EKawaiiCurveEvalMode.LengthRate;

        /// <summary> Stiffness 沿链乘子(最终 clamp 到 0..1)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve StiffnessCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> Damping 沿链乘子(最终 clamp 到 0..1)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve DampingCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> WorldDampingLocation 沿链乘子(最终 clamp 到 0..1)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve WorldDampingLocationCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> WorldDampingRotation 沿链乘子(最终 clamp 到 0..1)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve WorldDampingRotationCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> LimitAngle 沿链乘子(0..2, 允许放大角度限制)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 2.0f)]
        public TtKawaiiCurve LimitAngleCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> Radius 沿链乘子(0..2, 尖端可比根部细/粗)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 2.0f)]
        public TtKawaiiCurve RadiusCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> Drag 沿链乘子(0..2)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 2.0f)]
        public TtKawaiiCurve DragCurve { get; set; } = new TtKawaiiCurve();

        /// <summary> Wind 沿链乘子(0..2)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 2.0f)]
        public TtKawaiiCurve WindCurve { get; set; } = new TtKawaiiCurve();

        public TtKawaiiChainSetup()
        {

        }
    }

    /// <summary>
    /// Cloth simulation setup data. Extends chain setup with cloth-specific settings.
    /// </summary>
    [EGui.Controls.PropertyGrid.TtPropertyOrder(Order = EGui.Controls.PropertyGrid.TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtKawaiiClothSetup : TtKawaiiChainSetup
    {
        [Rtti.Meta]
        public bool LoopChains { get; set; } = false;

        // ─── 布料结构约束刚度曲线(沿链位置采样, 值为刚度) ────────────────
        // native 将 (1 - Eval(rate)) 作为 XPBD compliance; 曲线为空时视为刚度 0(完全柔)。
        // 这里的值是刚度本身(不是乘子), 有意义区间就是 0..1, 所以默认量程保持 0..1。
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve VerticalShrinkStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve VerticalStretchStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve HorizontalShrinkStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve HorizontalStretchStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve VerticalBendStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve HorizontalBendStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve ShearShrinkStiffness { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public TtKawaiiCurve ShearStretchStiffness { get; set; } = new TtKawaiiCurve();

        public TtKawaiiClothSetup()
        {

        }
    }

    /// <summary>
    /// Cosserat rod simulation setup data.
    /// </summary>
    [EGui.Controls.PropertyGrid.TtPropertyOrder(Order = EGui.Controls.PropertyGrid.TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtKawaiiRodSetup : BaseSerializer
    {
        [Rtti.Meta]
        public string Name { get; set; } = "Rod";

        [Rtti.Meta]
  		[TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton RootBoneIndex { get; set; }

        [Rtti.Meta]
  		[TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton EndBoneIndex { get; set; }

        [Rtti.Meta]
        public float StretchShearStiffness { get; set; } = 1.0f;

        [Rtti.Meta]
        public float BendTwistStiffness { get; set; } = 0.05f;

        [Rtti.Meta]
        public float PointAttachStiffness { get; set; } = 0.10f;

        [Rtti.Meta]
        public float OrientAttachStiffness { get; set; } = 0.05f;

        [Rtti.Meta]
        public int LODThreshold { get; set; } = -1;

        [Rtti.Meta]
        public TtKawaiiPhySettings PhysicsSettings { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettings();

        [Rtti.Meta]
        public TtKawaiiPhySettings PhysicsSettingsRandom { get; set; } = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettingsRandom();

        // ─── Cosserat rod 逐段刚度曲线(与基准刚度相乘, 最终 clamp 到 0..1) ────────
        // 同样给 4 倍余量: 封顶在 1 的乘子只能衰减, 无法把某段拉得更硬。
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve StretchShearStiffnessCurve { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve BendTwistStiffnessCurve { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve PointAttachStiffnessCurve { get; set; } = new TtKawaiiCurve();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve OrientAttachStiffnessCurve { get; set; } = new TtKawaiiCurve();

        public TtKawaiiRodSetup()
        {

        }
    }

    /// <summary>
    /// Ribbon setup data. Configures a single bone chain driven by an analytical swing wave.
    ///
    /// A ribbon is a PURE KINEMATIC wave generator, not a solver. Its motion is a cascade of
    /// first-order low-pass filters along the chain (see native KawaiiRibbonSolver.h), so it is
    /// unconditionally stable and needs no tuning to avoid resonance.
    ///
    /// It deliberately has NO physics knobs — no gravity, collision, bone-length/angle
    /// constraints or world inertia. To get those, wire this ribbon node's pose output into a
    /// downstream KawaiiPhysics node: it treats the incoming waving pose as the animated pose,
    /// so its Stiffness pulls toward the wave while its own gravity / collision / WorldDamping
    /// act as corrections on top. Duplicating those parameters here would just be a second set
    /// of knobs for the same thing.
    ///
    /// All angles here are DEGREES; native converts to radians once when the ribbon is built.
    /// </summary>
    [EGui.Controls.PropertyGrid.TtPropertyOrder(Order = EGui.Controls.PropertyGrid.TtPropertyOrderAttribute.EPropertyOrder.DefinitionOrder)]
    public class TtKawaiiRibbonSetup : BaseSerializer
    {
        [Rtti.Meta]
        public string Name { get; set; } = "Ribbon";

        [Rtti.Meta]
        [TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton RootBoneIndex { get; set; }

        [Rtti.Meta]
        [TtSkeletonBoneIndexPickerEditorAttribute]
        public LimbIndexInSkeleton EndBoneIndex { get; set; }

        [Rtti.Meta]
        public float TailBoneLength { get; set; } = 0.0f;

        [Rtti.Meta]
        public EKawaiiTailBoneAxis TailBoneAxis { get; set; } = EKawaiiTailBoneAxis.X_Positive;

        /// <summary> LOD threshold. -1 means always active. </summary>
        [Rtti.Meta]
        public int LODThreshold { get; set; } = -1;

        // ─── 摆动 ──────────────────────────────────────────────────────

        /// <summary> 根关节的最大摆动角度(度)。10 轻微, 30 中等, 60 大幅。 </summary>
        [Rtti.Meta]
        public float SwingAngleDegrees { get; set; } = 30.0f;

        /// <summary> 每秒摆动次数(Hz), 越大越快。 </summary>
        [Rtti.Meta]
        public float SwayFrequency { get; set; } = 1.5f;

        /// <summary> 惯性延迟。0=即时跟随无延迟, 1=强烈拖尾/波浪感。 </summary>
        [Rtti.Meta]
        public float Inertia { get; set; } = 0.5f;

        /// <summary> 惯性沿链递增。0=全链均匀延迟, 1=末端延迟远大于根部。 </summary>
        [Rtti.Meta]
        public float InertiaFalloff { get; set; } = 0.3f;

        /// <summary> 末端振幅倍率。1=与根部相同, 2=末端摆幅翻倍。设置 SwingAmplitudeCurve 后本项失效。 </summary>
        [Rtti.Meta]
        public float TipAmplify { get; set; } = 1.5f;

        /// <summary> 振幅增长曲线形状。0.5=前段快, 1=线性, 2=越靠末端放大越快(最自然)。 </summary>
        [Rtti.Meta]
        public float AmplifyCurvePower { get; set; } = 2.0f;

        // ─── 摆动平面 ──────────────────────────────────────────────────
        // 两个角度都相对链自身的静止方向, 所以无论条带怎么绑骨含义都一致。

        /// <summary> 绕链自身旋转摆动平面(度)。0=前后摆, ±90=左右摆, 中间为斜向平面。 </summary>
        [Rtti.Meta]
        public float SwingPlaneAngleDegrees { get; set; } = 0.0f;

        /// <summary> 条带在该平面内的静止位置, 即摆动围绕的 0 度(度)。±90 把整条带在平面内偏转四分之一圈。 </summary>
        [Rtti.Meta]
        public float RestTiltAngleDegrees { get; set; } = 0.0f;

        /// <summary>
        /// 沿链长的振幅曲线(乘子), 设置后覆盖 TipAmplify / AmplifyCurvePower。
        /// 量程给到 4 倍: 封顶在 1 的乘子只能衰减, 无法把某段拉得更大。
        /// </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 4.0f)]
        public TtKawaiiCurve SwingAmplitudeCurve { get; set; } = new TtKawaiiCurve();

        // ─── 有机噪声(FBM) ─────────────────────────────────────────────

        /// <summary> 噪声混合比重。0=纯正弦, 1=纯噪声。 </summary>
        [Rtti.Meta]
        public float NoiseMix { get; set; } = 0.0f;

        /// <summary> 噪声叠加层数(1..8)。 </summary>
        [Rtti.Meta]
        public int NoiseLayers { get; set; } = 4;

        /// <summary> 高频细节强度。越低越平滑, 越高越粗糙。 </summary>
        [Rtti.Meta]
        public float NoiseRoughness { get; set; } = 0.5f;

        /// <summary> 噪声整体尺度。越大细节越密, 越小起伏越宽。 </summary>
        [Rtti.Meta]
        public float NoiseScale { get; set; } = 1.0f;

        // ─── 风 ────────────────────────────────────────────────────────

        /// <summary> 风力响应强度。0=忽略风。 </summary>
        [Rtti.Meta]
        public float WindResponse { get; set; } = 2.0f;

        /// <summary> 阵风强度, 叠加在稳定风之上。0=平滑风, 1=强湍流。 </summary>
        [Rtti.Meta]
        public float WindGustiness { get; set; } = 0.5f;

        /// <summary> 阵风循环频率。 </summary>
        [Rtti.Meta]
        public float GustFrequency { get; set; } = 2.0f;

        /// <summary> 沿链长的风力影响曲线(乘子)。 </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 2.0f)]
        public TtKawaiiCurve WindInfluenceCurve { get; set; } = new TtKawaiiCurve();

        public TtKawaiiRibbonSetup()
        {

        }
    }

    /// <summary>
    /// Collider definition for bone-driven colliders.
    /// Updated each frame from bone transforms.
    /// Shape geometry is defined by the TtCollisionShape派生类 (TtSphereShape, TtCapsuleShape, etc.)
    /// </summary>
    public class TtKawaiiColliderDef
    {
        /// <summary>
        /// Collision shape (TtSphereShape / TtCapsuleShape / TtBoxShape / TtPlaneShape).
        /// Offset and Rotation in the shape are relative to the attached bone.
        /// </summary>
        [Rtti.Meta]
        public TtCollisionShape Shape { get; set; } = new TtSphereShape() { Radius = 5.0f };

        /// <summary>
        /// Bone index that drives this collider's transform. -1 for world-space static colliders.
        /// </summary>
        [Rtti.Meta]
        public LimbIndexInSkeleton AttachBoneIndex { get; set; }
    }
}
