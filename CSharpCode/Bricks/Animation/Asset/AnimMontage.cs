using EngineNS.Animation.Notify;
using EngineNS.IO;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Animation.Asset
{
    /// <summary>
    /// Montage中的一个动画段落: 把AnimationClip的[ClipStartTime, ClipEndTime]区间
    /// 铺到Montage时间轴的[StartPos, StartPos + Length]上。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtMontageSegment : IO.BaseSerializer
    {
        [Rtti.Meta, Category("Option")]
        [RName.PGRName(FilterExts = TtAnimationClip.AssetExt)]
        public RName ClipName { get; set; }
        /// <summary>
        /// 该段落在Montage时间轴上的起始位置(秒)
        /// </summary>
        [Rtti.Meta, Category("Option")]
        public float StartPos { get; set; } = 0.0f;
        /// <summary>
        /// 使用动画自身的起始时刻(秒)
        /// </summary>
        [Rtti.Meta, Category("Option")]
        public float ClipStartTime { get; set; } = 0.0f;
        /// <summary>
        /// 使用动画自身的结束时刻(秒), 小于等于ClipStartTime时表示用到动画结尾
        /// </summary>
        [Rtti.Meta, Category("Option")]
        public float ClipEndTime { get; set; } = 0.0f;
        [Rtti.Meta, Category("Option")]
        public float PlayRate { get; set; } = 1.0f;

        /// <summary>
        /// 运行时解析出的动画资产, 由TtAnimMontage.LoadReferencedAssets填充
        /// </summary>
        [Browsable(false)]
        public TtAnimationClip Clip { get; set; } = null;

        /// <summary>
        /// 该段落使用的动画时长(未计PlayRate)。
        /// 显式指定了ClipEndTime时以其为准, 否则用动画自身时长(未加载时为0)。
        /// </summary>
        [Browsable(false)]
        public float ClipRange
        {
            get
            {
                float end;
                if (ClipEndTime > ClipStartTime)
                    end = ClipEndTime;
                else if (Clip != null)
                    end = Clip.Duration;
                else
                    end = ClipStartTime;
                return Math.Max(0.0f, end - ClipStartTime);
            }
        }
        /// <summary>
        /// 该段落在Montage时间轴上占用的长度
        /// </summary>
        [Browsable(false)]
        public float Length
        {
            get
            {
                var rate = Math.Abs(PlayRate) > MathHelper.Epsilon ? Math.Abs(PlayRate) : 1.0f;
                return ClipRange / rate;
            }
        }
        [Browsable(false)]
        public float EndPos { get => StartPos + Length; }

        public bool Contains(float montagePosition)
        {
            return montagePosition >= StartPos && montagePosition < EndPos;
        }
        /// <summary>
        /// 把Montage时间轴位置换算为该段落内的动画采样时刻
        /// </summary>
        public float ToClipTime(float montagePosition)
        {
            var rate = Math.Abs(PlayRate) > MathHelper.Epsilon ? Math.Abs(PlayRate) : 1.0f;
            var localTime = (montagePosition - StartPos) * rate;
            var range = ClipRange;
            if (range > 0.0f)
                localTime = MathHelper.Clamp(localTime, 0.0f, range);
            return ClipStartTime + localTime;
        }
    }

    /// <summary>
    /// Montage的一条Slot轨道。BlendTree中同名的Slot节点会取这条轨道的Pose。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtMontageSlotTrack : IO.BaseSerializer
    {
        [Rtti.Meta, Category("Option")]
        public string SlotName { get; set; } = "DefaultSlot";
        [Rtti.Meta, Category("Option")]
        public List<TtMontageSegment> Segments { get; set; } = new List<TtMontageSegment>();

        [Browsable(false)]
        public float Duration
        {
            get
            {
                float duration = 0.0f;
                for (int i = 0; i < Segments.Count; ++i)
                {
                    duration = Math.Max(duration, Segments[i].EndPos);
                }
                return duration;
            }
        }
    }

    /// <summary>
    /// Montage的一个Section。NextSectionName构成跳转链: 段落播完后跳到下一段,
    /// 指向自己即为循环, 为空则进入BlendOut。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtMontageSection : IO.BaseSerializer
    {
        [Rtti.Meta, Category("Option")]
        public string Name { get; set; } = "Default";
        [Rtti.Meta, Category("Option")]
        public float StartTime { get; set; } = 0.0f;
        [Rtti.Meta, Category("Option")]
        public string NextSectionName { get; set; } = null;
    }

    [Rtti.Meta("")]
    public class TtAnimMontageAMeta : IO.IAssetMeta
    {
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName PreviewMeshName { get; set; }

        public override string TypeExt
        {
            get => TtAnimMontage.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "AnimMontage";
        }
        public override async TtTask<IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.AnimationModule.AnimMontageManager.GetMontage(GetAssetName());
        }
    }

    /// <summary>
    /// 蒙太奇资产, 对标UE的UAnimMontage: 多条Slot轨道 + Section跳转链 + 通知 + 进出混合设置。
    /// </summary>
    [TtAnimMontage.MontageCreate]
    [IO.AssetCreateMenu(MenuName = "Anim/AnimMontage")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtAnimMontage : IO.BaseSerializer, IAnimationCompositeAsset
    {
        public const string AssetExt = ".animmontage";
        public string TypeExt { get => AssetExt; }

        [Rtti.Meta("")]
        [Browsable(false)]
        public RName AssetName { get; set; }
        [Rtti.Meta, Category("Option")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName PreviewMeshName { get; set; }

        [Rtti.Meta, Category("Slot")]
        public List<TtMontageSlotTrack> SlotTracks { get; set; } = new List<TtMontageSlotTrack>();
        [Rtti.Meta, Category("Section")]
        public List<TtMontageSection> Sections { get; set; } = new List<TtMontageSection>();
        [Rtti.Meta, Category("Notify")]
        public List<IAnimNotify> Notifies { get; set; } = new List<IAnimNotify>();

        [Rtti.Meta, Category("BlendOption")]
        public float BlendInTime { get; set; } = 0.25f;
        [Rtti.Meta, Category("BlendOption")]
        public float BlendOutTime { get; set; } = 0.25f;
        /// <summary>
        /// 距段落结束多久开始BlendOut。小于0表示用BlendOutTime倒推, 使混合正好在结束时完成。
        /// </summary>
        [Rtti.Meta, Category("BlendOption")]
        public float BlendOutTriggerTime { get; set; } = -1.0f;
        /// <summary>
        /// 播到结尾自动BlendOut。关闭时会保持最后一帧直到显式Stop。
        /// </summary>
        [Rtti.Meta, Category("BlendOption")]
        public bool EnableAutoBlendOut { get; set; } = true;
        [Rtti.Meta, Category("Option")]
        public float DefaultPlayRate { get; set; } = 1.0f;

        /// <summary>
        /// 整条Montage的时长, 取所有Slot轨道的最大结束位置
        /// </summary>
        [Browsable(false)]
        public float Duration
        {
            get
            {
                float duration = 0.0f;
                for (int i = 0; i < SlotTracks.Count; ++i)
                {
                    duration = Math.Max(duration, SlotTracks[i].Duration);
                }
                return duration;
            }
        }

        #region Slot & Section 查询

        public TtMontageSlotTrack FindSlotTrack(string slotName)
        {
            for (int i = 0; i < SlotTracks.Count; ++i)
            {
                if (SlotTracks[i].SlotName == slotName)
                    return SlotTracks[i];
            }
            return null;
        }
        public TtMontageSlotTrack AddSlotTrack(string slotName)
        {
            var track = FindSlotTrack(slotName);
            if (track != null)
                return track;

            track = new TtMontageSlotTrack();
            track.SlotName = slotName;
            SlotTracks.Add(track);
            return track;
        }
        public int FindSectionIndex(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                return -1;
            for (int i = 0; i < Sections.Count; ++i)
            {
                if (Sections[i].Name == sectionName)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 按StartTime排序后返回Section索引序列, 用于求每个Section的结束时刻
        /// </summary>
        public List<int> GetSectionIndicesOrderByTime()
        {
            var indices = new List<int>();
            for (int i = 0; i < Sections.Count; ++i)
                indices.Add(i);
            indices.Sort((a, b) => Sections[a].StartTime.CompareTo(Sections[b].StartTime));
            return indices;
        }
        /// <summary>
        /// Section的结束时刻 = 时间轴上下一个Section的起点, 没有则为Montage结尾
        /// </summary>
        public float GetSectionEndTime(int sectionIndex)
        {
            if (sectionIndex < 0 || sectionIndex >= Sections.Count)
                return Duration;

            float start = Sections[sectionIndex].StartTime;
            float end = Duration;
            for (int i = 0; i < Sections.Count; ++i)
            {
                if (i == sectionIndex)
                    continue;
                var otherStart = Sections[i].StartTime;
                if (otherStart > start && otherStart < end)
                    end = otherStart;
            }
            return end;
        }
        /// <summary>
        /// 取指定时间轴位置所在的Section索引, 没有任何Section时返回-1
        /// </summary>
        public int GetSectionIndexFromPosition(float position)
        {
            int result = -1;
            float bestStart = float.MinValue;
            for (int i = 0; i < Sections.Count; ++i)
            {
                var start = Sections[i].StartTime;
                if (start <= position && start > bestStart)
                {
                    bestStart = start;
                    result = i;
                }
            }
            if (result == -1 && Sections.Count > 0)
            {
                // position在第一个Section之前, 归到时间最早的Section
                var ordered = GetSectionIndicesOrderByTime();
                result = ordered[0];
            }
            return result;
        }
        #endregion Slot & Section 查询

        /// <summary>
        /// 加载所有段落引用的AnimationClip。资产加载完成后必须调用一次。
        /// </summary>
        public async TtTask<bool> LoadReferencedAssets()
        {
            for (int i = 0; i < SlotTracks.Count; ++i)
            {
                var segments = SlotTracks[i].Segments;
                for (int j = 0; j < segments.Count; ++j)
                {
                    if (segments[j].ClipName == null)
                        continue;
                    segments[j].Clip = await segments[j].ClipName.GetAsset<TtAnimationClip>();
                }
            }
            return true;
        }

        #region IAsset
        public IAssetMeta CreateAMeta()
        {
            return new TtAnimMontageAMeta();
        }
        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            for (int i = 0; i < SlotTracks.Count; ++i)
            {
                var segments = SlotTracks[i].Segments;
                for (int j = 0; j < segments.Count; ++j)
                {
                    if (segments[j].ClipName != null)
                        ameta.RefAssetRNames.Add(segments[j].ClipName);
                }
            }
        }
        public void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("AnimMontage", 0, 0))
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
        public static TtAnimMontage LoadXnd(TtAnimMontageManager manager, IO.TtXndHolder holder)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = holder.RootNode.TryGetAttribute("AnimMontage");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }
                return result as TtAnimMontage;
            }
        }
        public class MontageCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                PGAsset.Target = mAsset;
            }
        }
        #endregion IAsset
    }

    public class TtAnimMontageManager
    {
        Dictionary<RName, TtAnimMontage> Montages = new Dictionary<RName, TtAnimMontage>();

        public async TtTask<TtAnimMontage> GetMontage(RName name)
        {
            TtAnimMontage result;
            if (Montages.TryGetValue(name, out result))
                return result;

            result = await CreateMontage(name);
            if (result == null)
                return null;

            await result.LoadReferencedAssets();
            Montages[name] = result;
            return result;
        }
        public async TtTask<TtAnimMontage> CreateMontage(RName name)
        {
            var result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd == null)
                        return null;

                    var montage = TtAnimMontage.LoadXnd(this, xnd);
                    if (montage == null)
                        return null;

                    montage.AssetName = name;
                    return montage;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }
        public void Remove(RName name)
        {
            Montages.Remove(name);
        }
    }
}

namespace EngineNS.Animation
{
    public partial class TtAnimationModule
    {
        public Animation.Asset.TtAnimMontageManager AnimMontageManager { get; } = new Animation.Asset.TtAnimMontageManager();
    }
}
