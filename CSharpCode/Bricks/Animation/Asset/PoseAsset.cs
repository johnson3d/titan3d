using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using EngineNS.Thread.Async;

namespace EngineNS.Animation.Asset
{
    [Rtti.Meta("")]
    public class TtPoseAssetAMeta : IO.IAssetMeta
    {
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName PreviewMeshName { get; set; }

        // 下面两项是纯编辑期辅助数据(运行时不需要), 所以放在 ameta 而不是资产本体:
        // 这样 .poseasset 只包含运行时真正需要的 pose 数据, cook 时只处理 ameta 即可。
        // 与 AnimationClip 把 PreviewMeshName 放在 ameta 的做法一致。

        /// <summary> 上次导入 pose 所用的 AnimationClip。 </summary>
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = TtAnimationClip.AssetExt)]
        public RName ImportClipName { get; set; }

        /// <summary> 上次导入 pose 所用的时刻(秒)。 </summary>
        [Rtti.Meta("")]
        public float ImportTime { get; set; } = 0.0f;

        public override string TypeExt
        {
            get => TtPoseAsset.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "PoseAsset";
        }
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.AnimationModule.PoseAssetManager.GetPoseAsset(GetAssetName());
        }
        public override async TtTask<IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.AnimationModule.PoseAssetManager.CreatePoseAsset(GetAssetName());
        }
    }

    /// <summary>
    /// PoseAsset 中的一个命名 pose。
    ///
    /// Transforms 为 local space, 与所属 TtPoseAsset.BoneNames 一一对应(同一资产内所有
    /// pose 共用一套骨骼顺序)。之所以存骨骼名而非仅存索引, 是为了在骨架顺序变化或换骨架
    /// 时仍能按名字正确映射。
    /// </summary>
    [Rtti.Meta("")]
    public class TtPoseAssetPose : IO.BaseSerializer
    {
        /// <summary>
        /// pose 名。在属性面板里只读: 改名必须走 TtPoseAsset.RenamePose(编辑器里的
        /// Rename 按钮), 否则会绕过重名检查 —— PoseDriver 的 target 是按名引用 pose 的,
        /// 同名会让引用变得不确定。
        /// </summary>
        [Rtti.Meta("")]
        [System.ComponentModel.ReadOnly(true)]
        public string Name { get; set; } = "NewPose";

        /// <summary> 该 pose 来源(从哪个 AnimationClip 的哪个时刻导入), 仅作记录便于回溯。 </summary>
        [Rtti.Meta("")]
        public RName SourceClipName { get; set; } = null;

        /// <summary> 导入时所用的时间点(秒), 仅作记录。 </summary>
        [Rtti.Meta("")]
        public float SourceClipTime { get; set; } = 0.0f;

        [Rtti.Meta("")]
        public List<FTransform> Transforms { get; set; } = new List<FTransform>();

        public TtPoseAssetPose Clone()
        {
            var r = new TtPoseAssetPose();
            r.Name = Name;
            r.SourceClipName = SourceClipName;
            r.SourceClipTime = SourceClipTime;
            r.Transforms = new List<FTransform>(Transforms);
            return r;
        }
    }

    /// <summary>
    /// Pose 资产: 一个资产内保存多个命名的骨骼 pose 快照。
    ///
    /// 典型用法(对标 UE PoseAsset 在 PoseDriver 中的角色): 先从 AnimationClip 的某个时刻
    /// 导入若干姿势并命名, 之后可在编辑器里对单根骨骼继续微调; 运行时由 PoseDriver 按 RBF
    /// 权重把这些 pose 混合起来。
    ///
    /// 序列化走 Xnd 二进制(pose 数据量大), 与 AnimationClip 一致。
    /// </summary>
    [Rtti.Meta("")]
    [TtPoseAsset.PoseAssetCreate]
    [IO.AssetCreateMenu(MenuName = "Anim/PoseAsset")]
    public partial class TtPoseAsset : IO.BaseSerializer, IO.IAsset
    {
        public const string AssetExt = ".poseasset";
        public string TypeExt { get => AssetExt; }

        [Rtti.Meta("")]
        [System.ComponentModel.Browsable(false)]
        public RName AssetName { get; set; }

        /// <summary> 预览用的骨骼模型。导入/编辑 pose 时以它的骨架为准。 </summary>
        [Rtti.Meta("")]
        [System.ComponentModel.Category("PoseAsset")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName PreviewMeshName { get; set; }

        /// <summary>
        /// 骨骼名列表。所有 pose 的 Transforms 都按这个顺序对齐。
        /// 由预览骨架自动建立, 不该在面板里手改(改了会让已有 pose 错位)。
        /// </summary>
        [Rtti.Meta("")]
        [System.ComponentModel.Browsable(false)]
        public List<string> BoneNames { get; set; } = new List<string>();

        /// <summary>
        /// 该资产包含的全部命名 pose。
        /// 编辑器里有专门的 pose 列表 UI, 所以不在属性面板里展开(数据量大)。
        /// </summary>
        [Rtti.Meta("")]
        [System.ComponentModel.Browsable(false)]
        public List<TtPoseAssetPose> Poses { get; set; } = new List<TtPoseAssetPose>();

        public int BoneCount => BoneNames.Count;
        public int PoseCount => Poses.Count;

        #region IAsset

        public IAssetMeta CreateAMeta()
        {
            return new TtPoseAssetAMeta();
        }

        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            if (PreviewMeshName != null)
                ameta.RefAssetRNames.Add(PreviewMeshName);
            // 各 pose 的来源 clip 也计入引用, 便于资产引用查询与回溯
            for (int i = 0; i < Poses.Count; ++i)
            {
                var src = Poses[i].SourceClipName;
                if (src != null && !ameta.RefAssetRNames.Contains(src))
                    ameta.RefAssetRNames.Add(src);
            }
        }

        public void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("PoseAsset", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }
            xnd.SaveXnd(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);

            var ameta = name.AMeta;
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
        }

        public static TtPoseAsset LoadXnd(TtPoseAssetManager manager, IO.TtXndHolder holder)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = holder.RootNode.TryGetAttribute("PoseAsset");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }
                return result as TtPoseAsset;
            }
        }

        #endregion IAsset

        #region 创建流程

        public class PoseAssetCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                PGAsset.Target = mAsset;

                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
            }
        }

        #endregion 创建流程

        #region pose 读写

        public int FindPoseIndex(string poseName)
        {
            if (string.IsNullOrEmpty(poseName))
                return -1;
            for (int i = 0; i < Poses.Count; ++i)
            {
                if (Poses[i].Name == poseName)
                    return i;
            }
            return -1;
        }

        public TtPoseAssetPose FindPose(string poseName)
        {
            int index = FindPoseIndex(poseName);
            return (index >= 0) ? Poses[index] : null;
        }

        /// <summary>
        /// 生成一个不与现有 pose 重名的名字。
        /// </summary>
        public string MakeUniquePoseName(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
                baseName = "NewPose";
            if (FindPoseIndex(baseName) < 0)
                return baseName;
            for (int i = 1; i < 10000; ++i)
            {
                var candidate = $"{baseName}_{i}";
                if (FindPoseIndex(candidate) < 0)
                    return candidate;
            }
            return baseName + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        /// <summary>
        /// 设置资产的骨骼顺序。若已有 pose 且骨骼数不匹配, 会按名字重排各 pose 的
        /// Transforms(缺失的骨骼填单位变换), 以保证所有 pose 与新骨骼顺序对齐。
        /// </summary>
        public void SetBoneNames(IReadOnlyList<string> boneNames)
        {
            var newNames = new List<string>(boneNames.Count);
            for (int i = 0; i < boneNames.Count; ++i)
                newNames.Add(boneNames[i]);

            if (Poses.Count > 0 && BoneNames.Count > 0)
            {
                // 旧骨骼名 -> 旧下标
                var oldIndexMap = new Dictionary<string, int>(BoneNames.Count);
                for (int i = 0; i < BoneNames.Count; ++i)
                    oldIndexMap[BoneNames[i]] = i;

                for (int p = 0; p < Poses.Count; ++p)
                {
                    var oldTransforms = Poses[p].Transforms;
                    var remapped = new List<FTransform>(newNames.Count);
                    for (int b = 0; b < newNames.Count; ++b)
                    {
                        if (oldIndexMap.TryGetValue(newNames[b], out int oldIndex)
                            && oldIndex < oldTransforms.Count)
                            remapped.Add(oldTransforms[oldIndex]);
                        else
                            remapped.Add(FTransform.Identity);
                    }
                    Poses[p].Transforms = remapped;
                }
            }

            BoneNames = newNames;
        }

        /// <summary>
        /// 新增或覆盖一个命名 pose。transforms 需与传入的 boneNames 一一对应。
        /// 若资产尚未确定骨骼顺序, 则以本次传入的 boneNames 作为资产骨骼顺序。
        /// </summary>
        public TtPoseAssetPose AddOrReplacePose(string poseName, IReadOnlyList<string> boneNames,
            IReadOnlyList<FTransform> transforms, RName sourceClip = null, float sourceTime = 0.0f)
        {
            if (boneNames == null || transforms == null)
                return null;

            if (BoneNames.Count == 0)
                SetBoneNames(boneNames);

            // 按资产骨骼顺序重排传入数据
            var srcIndexMap = new Dictionary<string, int>(boneNames.Count);
            for (int i = 0; i < boneNames.Count; ++i)
                srcIndexMap[boneNames[i]] = i;

            var ordered = new List<FTransform>(BoneNames.Count);
            for (int b = 0; b < BoneNames.Count; ++b)
            {
                if (srcIndexMap.TryGetValue(BoneNames[b], out int srcIndex) && srcIndex < transforms.Count)
                    ordered.Add(transforms[srcIndex]);
                else
                    ordered.Add(FTransform.Identity);
            }

            int existing = FindPoseIndex(poseName);
            var pose = (existing >= 0) ? Poses[existing] : new TtPoseAssetPose();
            pose.Name = poseName;
            pose.Transforms = ordered;
            pose.SourceClipName = sourceClip;
            pose.SourceClipTime = sourceTime;
            if (existing < 0)
                Poses.Add(pose);

            return pose;
        }

        public bool RemovePose(string poseName)
        {
            int index = FindPoseIndex(poseName);
            if (index < 0)
                return false;
            Poses.RemoveAt(index);
            return true;
        }

        public bool RenamePose(string oldName, string newName)
        {
            int index = FindPoseIndex(oldName);
            if (index < 0 || string.IsNullOrEmpty(newName))
                return false;
            // 重名则拒绝, 避免出现两个同名 pose 让按名查找失效
            if (FindPoseIndex(newName) >= 0)
                return false;
            Poses[index].Name = newName;
            return true;
        }

        /// <summary>
        /// 取某个 pose 中指定骨骼的变换。骨骼不存在时返回 false。
        /// </summary>
        public bool TryGetBoneTransform(string poseName, string boneName, out FTransform transform)
        {
            transform = FTransform.Identity;
            var pose = FindPose(poseName);
            if (pose == null)
                return false;
            int boneIndex = BoneNames.IndexOf(boneName);
            if (boneIndex < 0 || boneIndex >= pose.Transforms.Count)
                return false;
            transform = pose.Transforms[boneIndex];
            return true;
        }

        /// <summary>
        /// 修改某个 pose 中指定骨骼的变换(供编辑器在导入后继续微调使用)。
        /// </summary>
        public bool SetBoneTransform(string poseName, string boneName, in FTransform transform)
        {
            var pose = FindPose(poseName);
            if (pose == null)
                return false;
            int boneIndex = BoneNames.IndexOf(boneName);
            if (boneIndex < 0)
                return false;
            // Transforms 可能短于骨骼列表(旧数据), 补齐后再写
            while (pose.Transforms.Count < BoneNames.Count)
                pose.Transforms.Add(FTransform.Identity);
            pose.Transforms[boneIndex] = transform;
            return true;
        }

        #endregion pose 读写
    }

    public class TtPoseAssetManager
    {
        Dictionary<RName, TtPoseAsset> mPoseAssets = new Dictionary<RName, TtPoseAsset>();

        public async TtTask<TtPoseAsset> GetPoseAsset(RName name)
        {
            TtPoseAsset result;
            if (mPoseAssets.TryGetValue(name, out result))
                return result;

            result = await CreatePoseAsset(name);
            if (result != null)
            {
                mPoseAssets[name] = result;
                return result;
            }
            return null;
        }

        public async TtTask<TtPoseAsset> CreatePoseAsset(RName name)
        {
            var result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd == null)
                        return null;

                    var asset = TtPoseAsset.LoadXnd(this, xnd);
                    if (asset == null)
                        return null;

                    asset.AssetName = name;
                    return asset;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }

        public void Remove(RName name)
        {
            mPoseAssets.Remove(name);
        }
    }
}

namespace EngineNS.Animation
{
    public partial class TtAnimationModule
    {
        public Animation.Asset.TtPoseAssetManager PoseAssetManager { get; } = new Animation.Asset.TtPoseAssetManager();
    }
}
