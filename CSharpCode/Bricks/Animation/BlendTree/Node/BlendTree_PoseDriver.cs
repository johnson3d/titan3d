using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Animation.RBF;
using EngineNS.Animation.Asset;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;

namespace EngineNS.Animation.BlendTree.Node
{
    /// <summary>
    /// PoseDriver 的驱动源: 用源骨骼的哪部分作为 RBF 输入。
    /// </summary>
    public enum EPoseDriverSource : byte
    {
        /// <summary> 用源骨骼的旋转(转成欧拉角进入 RBF 空间)。 </summary>
        Rotation,
        /// <summary> 用源骨骼的位移。 </summary>
        Translation,
    }

    /// <summary>
    /// PoseDriver 的一个 target: RBF 空间中的一个采样点, 以及它所驱动的 pose 名。
    ///
    /// 这是**可序列化**的编辑期数据(会经 DesignMacross GenCode 生成到资产代码里),
    /// 运行时会被转换成数学层的 TtRBFTarget。
    /// 注意: 该类型内不能出现 List/数组成员 —— GenCode 的 CreateItem 只处理
    /// primitive/enum/string 与可递归的复杂对象属性, 集合成员会静默丢数据。
    /// 多源骨骼因此用固定的 2/3 号字段而非数组。
    /// </summary>
    public class TtPoseDriverTarget
    {
        /// <summary> 该 target 激活时要驱动的 pose 名(对应 PoseAsset 里的命名 pose)。 </summary>
        [Rtti.Meta]
        public string DrivenPoseName { get; set; } = "";

        /// <summary>
        /// 源骨骼处于该 target 时的欧拉角, 单位为**度**(便于手填与阅读)。
        /// 驱动源为 Rotation 时生效; 运行时会转成弧度进入 RBF。
        /// </summary>
        [Rtti.Meta]
        public Vector3 TargetEulerDegrees { get; set; } = Vector3.Zero;

        /// <summary>
        /// 源骨骼处于该 target 时的位移。驱动源为 Translation 时生效。
        /// </summary>
        [Rtti.Meta]
        public Vector3 TargetTranslation { get; set; } = Vector3.Zero;

        /// <summary> 第 2 根源骨骼对应的欧拉角(度)。仅当节点配了 SourceBone2 时使用。 </summary>
        [Rtti.Meta]
        public Vector3 TargetEulerDegrees2 { get; set; } = Vector3.Zero;

        /// <summary> 第 2 根源骨骼对应的位移。 </summary>
        [Rtti.Meta]
        public Vector3 TargetTranslation2 { get; set; } = Vector3.Zero;

        /// <summary> 第 3 根源骨骼对应的欧拉角(度)。 </summary>
        [Rtti.Meta]
        public Vector3 TargetEulerDegrees3 { get; set; } = Vector3.Zero;

        /// <summary> 第 3 根源骨骼对应的位移。 </summary>
        [Rtti.Meta]
        public Vector3 TargetTranslation3 { get; set; } = Vector3.Zero;

        /// <summary> 该 target 的影响范围倍率。 </summary>
        [Rtti.Meta]
        public float ScaleFactor { get; set; } = 1.0f;

        /// <summary> 是否对该 target 的权重额外套一条曲线做重映射(仅 Additive 生效)。 </summary>
        [Rtti.Meta]
        public bool ApplyCustomCurve { get; set; } = false;

        /// <summary>
        /// 权重重映射曲线(输入原始权重 0..1, 输出重映射后的权重), ApplyCustomCurve 为真时生效。
        /// 复用 TtKawaiiCurve: 它内部用 string 承载关键点, 对 GenCode 安全。
        /// </summary>
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.TtKawaiiCurveEditor(YMin = 0.0f, YMax = 1.0f)]
        public Bricks.Animation.KawaiiPhysics.TtKawaiiCurve CustomCurve { get; set; }
            = new Bricks.Animation.KawaiiPhysics.TtKawaiiCurve();

        /// <summary> 覆盖该 target 的距离度量(DefaultMethod 表示沿用节点设置)。 </summary>
        [Rtti.Meta]
        public ERBFDistanceMethod DistanceMethod { get; set; } = ERBFDistanceMethod.DefaultMethod;

        /// <summary> 覆盖该 target 的衰减函数(DefaultFunction 表示沿用节点设置)。 </summary>
        [Rtti.Meta]
        public ERBFFunctionType FunctionType { get; set; } = ERBFFunctionType.DefaultFunction;

        /// <summary> 按源骨骼下标取欧拉角(度)。 </summary>
        public Vector3 GetEulerDegrees(int sourceIndex)
        {
            switch (sourceIndex)
            {
                default:
                case 0: return TargetEulerDegrees;
                case 1: return TargetEulerDegrees2;
                case 2: return TargetEulerDegrees3;
            }
        }

        /// <summary> 按源骨骼下标取位移。 </summary>
        public Vector3 GetTranslation(int sourceIndex)
        {
            switch (sourceIndex)
            {
                default:
                case 0: return TargetTranslation;
                case 1: return TargetTranslation2;
                case 2: return TargetTranslation3;
            }
        }
    }

    public class TtPoseDriverCommandDesc : IAnimationCommandDesc
    {
        public float Alpha { get; set; } = 1.0f;

        /// <summary> RBF 参数(由节点在 Initialize 时装配)。 </summary>
        public TtRBFParams RBFParams { get; set; } = null;

        /// <summary> 数学层 target(由节点从编辑期 target 转换而来)。 </summary>
        public List<TtRBFTarget> RBFTargets { get; set; } = null;

        /// <summary> Interpolative 的预计算数据。 </summary>
        public TtRBFSolverData SolverData { get; set; } = null;

        /// <summary>
        /// 每个 target 对应的、已从 PoseAsset 预填好的 driven pose。
        /// 与 RBFTargets 下标一一对应; PoseAsset 的 pose 是静态数据, 故只在 Initialize 填一次。
        /// </summary>
        public List<TtLocalSpaceRuntimePose> TargetPoses { get; set; } = null;

        /// <summary> 源骨骼名列表(按顺序拼成 RBF 输入, 每根占 3 个分量)。 </summary>
        public List<string> SourceBoneNames { get; set; } = new List<string>();

        /// <summary>
        /// bone mask: 为 true 的骨骼才写入驱动结果。null 表示不限制(全骨骼驱动)。
        /// 下标与输出 pose 的骨骼下标一致。
        /// </summary>
        public bool[] BoneMask { get; set; } = null;

        public EPoseDriverSource DriveSource { get; set; } = EPoseDriverSource.Rotation;
    }

    public class TtPoseDriverCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> FromCommand { get; set; } = null;
        public TtPoseDriverCommandDesc Desc { get; set; }

        // 复用缓冲, 避免每帧分配
        readonly TtRBFEntry mInput = new TtRBFEntry();
        readonly List<TtRBFOutputWeight> mWeights = new List<TtRBFOutputWeight>();
        readonly List<TtLocalSpaceRuntimePose> mPosesToBlend = new List<TtLocalSpaceRuntimePose>();
        readonly List<float> mBlendWeights = new List<float>();
        TtLocalSpaceRuntimePose mDrivenPose = null;
        TtLocalSpaceRuntimePose mMaskedBlendPose = null;

        public override void Execute()
        {
            if (FromCommand == null)
                return;

            // 默认原样输出输入 pose; 后面若求解出权重再叠加驱动结果
            TtRuntimePoseUtility.CopyPose(ref mOutPose, FromCommand.OutPose);

            var desc = Desc;
            if (desc == null || desc.RBFTargets == null || desc.RBFTargets.Count == 0
                || desc.TargetPoses == null || desc.SolverData == null || mDrivenPose == null)
                return;
            if (desc.Alpha <= 0.0f)
                return;

            // 1. 读源骨骼当前变换, 构建 RBF 输入
            if (!BuildInput(desc, FromCommand.OutPose))
                return;

            // 2. RBF 求解各 target 权重
            TtRBFSolver.Solve(desc.SolverData, desc.RBFParams, desc.RBFTargets, mInput, mWeights);
            if (mWeights.Count == 0)
                return;

            // 3. 按权重混合各 target 的 driven pose
            mPosesToBlend.Clear();
            mBlendWeights.Clear();
            for (int i = 0; i < mWeights.Count; ++i)
            {
                int targetIndex = mWeights[i].TargetIndex;
                if (targetIndex < 0 || targetIndex >= desc.TargetPoses.Count)
                    continue;
                var targetPose = desc.TargetPoses[targetIndex];
                if (targetPose == null)
                    continue;
                mPosesToBlend.Add(targetPose);
                mBlendWeights.Add(mWeights[i].TargetWeight);
            }
            if (mPosesToBlend.Count == 0)
                return;

            TtRuntimePoseUtility.BlendPoses(ref mDrivenPose, mPosesToBlend, mBlendWeights);

            // 4. 与输入 pose 按 Alpha 混合
            var mask = desc.BoneMask;
            if (mask == null)
            {
                TtRuntimePoseUtility.BlendPoses(ref mOutPose, FromCommand.OutPose, mDrivenPose, desc.Alpha);
            }
            else
            {
                // 只驱动 mask 内的骨骼: 先整体混到临时 pose, 再按 mask 挑骨骼写回。
                // 这样可以直接复用已验证的 BlendPoses, 不另写一份带 mask 的混合实现。
                TtRuntimePoseUtility.BlendPoses(ref mMaskedBlendPose, FromCommand.OutPose, mDrivenPose, desc.Alpha);
                int count = mOutPose.Transforms.Count;
                for (int i = 0; i < count && i < mMaskedBlendPose.Transforms.Count; ++i)
                {
                    if (i < mask.Length && mask[i])
                        mOutPose.Transforms[i] = mMaskedBlendPose.Transforms[i];
                }
            }
        }

        bool BuildInput(TtPoseDriverCommandDesc desc, TtLocalSpaceRuntimePose fromPose)
        {
            var boneNames = desc.SourceBoneNames;
            if (boneNames == null || boneNames.Count == 0)
                return false;

            mInput.Reset();
            for (int i = 0; i < boneNames.Count; ++i)
            {
                var transform = TtRuntimePoseUtility.GetTransform(boneNames[i], fromPose);
                if (desc.DriveSource == EPoseDriverSource.Translation)
                {
                    var pos = transform.Position.ToSingleVector3();
                    mInput.AddFromVector(in pos);
                }
                else
                {
                    var quat = transform.Quat;
                    mInput.AddFromQuat(in quat);
                }
            }
            return mInput.Dimensions == desc.RBFParams.TargetDimensions;
        }

        /// <summary>
        /// 分配混合用的中间 pose(骨骼数与骨架一致)。
        /// </summary>
        public void SetupBuffers(TtLocalSpaceRuntimePose templatePose)
        {
            mDrivenPose = TtRuntimePoseUtility.CopyPose(templatePose);
            mMaskedBlendPose = TtRuntimePoseUtility.CopyPose(templatePose);
        }
    }

    public class TtLocalSpaceBlendTree_PoseDriver<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> FromNode { get; set; }
        TtPoseDriverCommand<S> mAnimationCommand = null;
        public TtPoseDriverCommandDesc CommandDesc { get; set; } = new();

        // ─── 由 ClassDescription 生成的代码赋值 ────────────────────────────
        /// <summary> 提供 driven pose 的 PoseAsset。 </summary>
        public RName PoseAssetName { get; set; } = null;
        /// <summary> 源骨骼(其当前变换作为 RBF 输入)。 </summary>
        public string SourceBoneName { get; set; } = "";
        /// <summary> 第 2 根源骨骼(可空)。多源时每根向 RBF 输入贡献一组三维量。 </summary>
        public string SourceBoneName2 { get; set; } = "";
        /// <summary> 第 3 根源骨骼(可空)。 </summary>
        public string SourceBoneName3 { get; set; } = "";
        public EPoseDriverSource DriveSource { get; set; } = EPoseDriverSource.Rotation;
        public List<TtPoseDriverTarget> PoseTargets { get; set; } = null;

        /// <summary> 为 true 时只驱动下面指定的骨骼子树, 其它骨骼保持输入姿势。 </summary>
        public bool OnlyDriveSelectedBones { get; set; } = false;
        /// <summary> 驱动范围根骨骼(含其全部子骨骼)。 </summary>
        public string DriveBoneRoot1 { get; set; } = "";
        public string DriveBoneRoot2 { get; set; } = "";
        public string DriveBoneRoot3 { get; set; } = "";

        // RBF 参数(逐项暴露, 便于 GenCode 直接赋值)
        public ERBFSolverType SolverType { get; set; } = ERBFSolverType.Additive;
        public float Radius { get; set; } = 45.0f;
        public bool AutomaticRadius { get; set; } = false;
        public ERBFFunctionType Function { get; set; } = ERBFFunctionType.Gaussian;
        public ERBFDistanceMethod DistanceMethod { get; set; } = ERBFDistanceMethod.SwingAngle;
        public ERBFTwistAxis TwistAxis { get; set; } = ERBFTwistAxis.X;
        public float WeightThreshold { get; set; } = 1e-4f;
        public ERBFNormalizeMethod NormalizeMethod { get; set; } = ERBFNormalizeMethod.OnlyNormalizeAboveOne;

       public override async TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new();
            mAnimationCommand.Desc = CommandDesc;
            mAnimationCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);
            mAnimationCommand.SetupBuffers(mAnimationCommand.OutPose);

            // 源骨骼: 按顺序收集非空的, 求解维度 = 3 * 数量
            CommandDesc.SourceBoneNames = CollectNonEmpty(SourceBoneName, SourceBoneName2, SourceBoneName3);
            CommandDesc.DriveSource = DriveSource;
            CommandDesc.RBFParams = BuildRBFParams(CommandDesc.SourceBoneNames.Count);
            CommandDesc.BoneMask = BuildBoneMask(mAnimationCommand.OutPose);

            // 载入 PoseAsset 并为每个 target 预填 driven pose
            TtPoseAsset poseAsset = null;
            if (PoseAssetName != null)
                poseAsset = await TtEngine.Instance.AnimationModule.PoseAssetManager.GetPoseAsset(PoseAssetName);

            BuildTargets(context, poseAsset, CommandDesc.SourceBoneNames.Count);

            return true;
        }

        static List<string> CollectNonEmpty(params string[] names)
        {
            var result = new List<string>();
            for (int i = 0; i < names.Length; ++i)
            {
                if (!string.IsNullOrEmpty(names[i]))
                    result.Add(names[i]);
            }
            return result;
        }

        /// <summary>
        /// 构建 bone mask: 从指定的根骨骼出发, 标记它们及其全部子骨骼。
        /// 用"根骨骼 + 子树"而不是逐骨骼列表, 是因为后者需要集合字段(GenCode 不支持),
        /// 而且实际用法里"只驱动某条肢体"本质就是一个子树。
        /// </summary>
        bool[] BuildBoneMask(TtLocalSpaceRuntimePose templatePose)
        {
            if (!OnlyDriveSelectedBones)
                return null;

            var roots = CollectNonEmpty(DriveBoneRoot1, DriveBoneRoot2, DriveBoneRoot3);
            if (roots.Count == 0)
                return null;

            int boneCount = templatePose.Descs.Count;
            var mask = new bool[boneCount];

            // 骨骼名 -> 下标
            var nameToIndex = new Dictionary<string, int>(boneCount);
            for (int i = 0; i < boneCount; ++i)
                nameToIndex[templatePose.Descs[i].Name] = i;

            for (int r = 0; r < roots.Count; ++r)
            {
                if (nameToIndex.TryGetValue(roots[r], out int rootIndex))
                    mask[rootIndex] = true;
            }

            // 逐轮传播: 父骨骼在 mask 内则子骨骼也在。Descs 不保证父在子之前,
            // 所以反复扫描直到不再新增(骨骼数量量级小, 开销可忽略)。
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < boneCount; ++i)
                {
                    if (mask[i])
                        continue;
                    var parentName = templatePose.Descs[i].ParentName;
                    if (string.IsNullOrEmpty(parentName))
                        continue;
                    if (nameToIndex.TryGetValue(parentName, out int pi) && mask[pi])
                    {
                        mask[i] = true;
                        changed = true;
                    }
                }
            }

            return mask;
        }

        TtRBFParams BuildRBFParams(int sourceBoneCount)
        {
            var prm = new TtRBFParams();
            prm.SolverType = SolverType;
            prm.Radius = Radius;
            prm.AutomaticRadius = AutomaticRadius;
            prm.Function = Function;
            prm.DistanceMethod = DistanceMethod;
            prm.TwistAxis = TwistAxis;
            prm.WeightThreshold = WeightThreshold;
            prm.NormalizeMethod = NormalizeMethod;
            // 每根源骨骼贡献一组三维量
            prm.TargetDimensions = 3 * Math.Max(1, sourceBoneCount);
            return prm;
        }

        void BuildTargets(FAnimBlendTreeContext context, TtPoseAsset poseAsset, int sourceBoneCount)
        {
            var rbfTargets = new List<TtRBFTarget>();
            var targetPoses = new List<TtLocalSpaceRuntimePose>();
            int sourceCount = Math.Max(1, sourceBoneCount);

            if (PoseTargets != null)
            {
                for (int i = 0; i < PoseTargets.Count; ++i)
                {
                    var src = PoseTargets[i];

                    var rt = new TtRBFTarget();
                    // 每根源骨骼依次贡献一组, 顺序必须与 BuildInput 一致
                    for (int s = 0; s < sourceCount; ++s)
                    {
                        if (DriveSource == EPoseDriverSource.Translation)
                        {
                            var v = src.GetTranslation(s);
                            rt.AddFromVector(in v);
                        }
                        else
                        {
                            // 编辑期以度存放, 这里转成弧度(与 FRotator/RBF 的弧度约定一致)
                            var deg = src.GetEulerDegrees(s);
                            var rotator = new FRotator();
                            rotator.Roll = deg.X * TtRBFMath.DegToRad;
                            rotator.Pitch = deg.Y * TtRBFMath.DegToRad;
                            rotator.Yaw = deg.Z * TtRBFMath.DegToRad;
                            rt.AddFromRotator(in rotator);
                        }
                    }
                    rt.ScaleFactor = src.ScaleFactor;
                    rt.ApplyCustomCurve = src.ApplyCustomCurve;
                    if (src.ApplyCustomCurve && src.CustomCurve != null)
                    {
                        var curve = src.CustomCurve;
                        rt.CustomCurveEvaluator = (w) => curve.Evaluate(w);
                    }
                    rt.DistanceMethod = src.DistanceMethod;
                    rt.FunctionType = src.FunctionType;
                    rbfTargets.Add(rt);

                    // 预填该 target 驱动的 pose
                    var pose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);
                    if (poseAsset != null && !string.IsNullOrEmpty(src.DrivenPoseName))
                        TtPoseAssetUtil.ApplyPoseToRuntimePose(poseAsset, src.DrivenPoseName, pose);
                    targetPoses.Add(pose);
                }
            }

            CommandDesc.RBFTargets = rbfTargets;
            CommandDesc.TargetPoses = targetPoses;
            CommandDesc.SolverData = (rbfTargets.Count > 0)
                ? TtRBFSolver.InitSolver(CommandDesc.RBFParams, rbfTargets)
                : null;
        }

        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(
            IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(FromNode != null);
            if (FromNode == null)
                return null;

            context.AddCommand(context.TreeDepth, mAnimationCommand);
            context.TreeDepth++;
            mAnimationCommand.FromCommand = FromNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }

        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
        }
    }
}
