using EngineNS.IO;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Sequencer.Asset
{
    /// <summary>
    /// 一个绑定: 序列里的一个"被动画对象"占位, 加上挂在它上面的轨道。
    ///
    /// 绑定不直接存节点引用, 而是存 (父节点 Guid + 相对路径) 二元组:
    /// - 直接存目标节点 Guid 的话, 同一条序列没法复用到另一份场景/另一个预制实例上,
    ///   因为那边的节点 Guid 是新生成的。
    /// - 只存名字路径的话, 从场景根开始找, 一旦有同名节点或者把整棵子树挪了位置就找错。
    /// 存相对路径后, 播放器把 ParentNodeId 解析成一个"参照节点"(通常是挂了播放器的那个
    /// 节点), 再从它往下按路径逐级精确匹配名字。ParentNodeId 为 Guid.Empty 表示从场景根算起。
    /// </summary>
    [Rtti.Meta("")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtSequenceBinding : IO.BaseSerializer
    {
        /// <summary>绑定自身的稳定标识, 编辑器里引用绑定用它而不是下标 (下标会因增删而错位)</summary>
        [Rtti.Meta("")]
        [Browsable(false)]
        public Guid BindingId { get; set; } = Guid.NewGuid();
        /// <summary>参照节点的 Guid, Guid.Empty 表示相对场景根</summary>
        [Rtti.Meta("")]
        [Browsable(false)]
        public Guid ParentNodeId { get; set; } = Guid.Empty;
        /// <summary>
        /// 目标节点自己的 Guid, 解析时优先于 RelativePath。
        ///
        /// Guid.Empty 表示没有, 有两种成因: ① 早期存的资产 (那时还没这个字段), 解析成功
        /// 后会被自动补写；② 目标节点的类不 override NodeId —— TtLightWeightNodeBase 系
        /// (相机 / Movement / SpringArm / AnimPlayNode) 全是这种, NodeId 恒为 Empty, 只能靠名字
        /// 路径, 也就永远怕改名。
        /// </summary>
        [Rtti.Meta("")]
        [Browsable(false)]
        public Guid TargetNodeId { get; set; } = Guid.Empty;
        /// <summary>相对参照节点的节点名路径, 用 '/' 分隔, 空串表示参照节点自己</summary>
        [Rtti.Meta, Category("Binding")]
        public string RelativePath { get; set; } = "";
        /// <summary>轨道树上显示的名字, 只影响界面</summary>
        [Rtti.Meta, Category("Binding")]
        public string DisplayName { get; set; } = "Binding";

        [Rtti.Meta, Browsable(false)]
        public List<TtSequenceTrack> Tracks { get; set; } = new List<TtSequenceTrack>();

        public TtSequenceTrack FindTrack(string trackTypeName)
        {
            for (int i = 0; i < Tracks.Count; ++i)
            {
                if (Tracks[i] != null && Tracks[i].TrackTypeName == trackTypeName)
                    return Tracks[i];
            }
            return null;
        }
        /// <summary>拿到 Transform 轨道, 没有就建一条。一个绑定上只允许一条 Transform 轨。</summary>
        public TtTransformTrack GetOrCreateTransformTrack()
        {
            var track = FindTrack("Transform") as TtTransformTrack;
            if (track != null)
                return track;
            track = new TtTransformTrack();
            Tracks.Add(track);
            return track;
        }
        /// <summary>
        /// 按属性 Id 找属性轨。不能用 FindTrack("Property") 代替: 所有属性轨的 TrackTypeName
        /// 都是 "Property", 一个绑定上挂了多条属性轨时它只会返回最靠前的那条 —— 打点会全部
        /// 打到同一条轨上。
        /// </summary>
        public TtPropertyTrack FindPropertyTrack(string propertyId)
        {
            if (string.IsNullOrEmpty(propertyId))
                return null;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                var track = Tracks[i] as TtPropertyTrack;
                if (track != null && track.PropertyId == propertyId)
                    return track;
            }
            return null;
        }
        /// <summary>
        /// 拿到某条属性的轨道, 没有就建一条。一条属性只允许一条轨: 两条轨写同一个属性时,
        /// 最终值取决于轨道遍历顺序, 界面上看不出是哪条在生效。
        /// </summary>
        public TtPropertyTrack GetOrCreatePropertyTrack(string propertyId, string adapterId, string displayName)
        {
            var track = FindPropertyTrack(propertyId);
            if (track != null)
                return track;
            track = new TtPropertyTrack()
            {
                PropertyId = propertyId,
                AdapterId = adapterId,
                DisplayName = string.IsNullOrEmpty(displayName) ? propertyId : displayName,
            };
            Tracks.Add(track);
            return track;
        }
        public long GetMaxTick()
        {
            long max = 0;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                if (Tracks[i] == null)
                    continue;
                max = Math.Max(max, Tracks[i].GetMaxTick());
            }
            return max;
        }
    }

    [Rtti.Meta("")]
    public class TtSequenceAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtSequence.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "Sequence";
        }
        public override async TtTask<IAsset> GetAsset(params object[] args)
        {
            return await TtEngine.Instance.SequencerModule.SequenceManager.GetSequence(GetAssetName());
        }
    }

    /// <summary>
    /// 序列资产, 对标 UE 的 UMovieSceneSequence。四层结构的最外层:
    /// Sequence -> Binding -> Track -> Section -> Channel。
    ///
    /// 时间全部用整数 tick 表示, 两个帧率的分工:
    /// - TickResolution 是内部时基, 所有 tick 都按它计秒。定得很高 (默认 24000) 是为了让
    ///   常见的 24/25/30/60 fps 都能整除, 换帧率显示时关键帧不会因取整而漂移。
    /// - DisplayRate 只影响界面上"一格是多少"和吸附粒度, 不参与求值。
    /// 存盘后改 TickResolution 会让已有关键帧的时刻含义整体变化, 所以它是资产级的一次性
    /// 决定, 编辑器不提供随手改的入口。
    /// </summary>
    [TtSequence.SequenceCreate]
    [IO.AssetCreateMenu(MenuName = "Sequence/Sequence")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtSequence : IO.BaseSerializer, IO.IAsset
    {
        public const string AssetExt = ".sequence";
        public string TypeExt { get => AssetExt; }

        [Rtti.Meta("")]
        [Browsable(false)]
        public RName AssetName { get; set; }

        [Rtti.Meta, Category("TimeBase")]
        [Browsable(false)]
        public TtFrameRate TickResolution { get; set; } = TtFrameRate.DefaultTickResolution;
        [Rtti.Meta, Category("TimeBase")]
        public TtFrameRate DisplayRate { get; set; } = TtFrameRate.DefaultDisplayRate;

        [Rtti.Meta, Category("Playback")]
        public long PlaybackStartTick { get; set; } = 0;
        [Rtti.Meta, Category("Playback")]
        public long PlaybackEndTick { get; set; } = 0;

        /// <summary>
        /// 编辑器预览用的场景资产。绑定只记"场景里那个名字/路径的节点", 序列自己不保
        /// 存节点; 而预览视口是每次重建的临时世界, 拖进去的节点不落盘 —— 不指定预览场景
        /// 的话, 重开编辑器后所有绑定都会变成 &lt;Unresolved&gt;。
        /// 只影响编辑期预览: 运行时播放时被驱动的节点由序列所在的关卡提供。
        /// </summary>
        [Rtti.Meta, Category("Preview")]
        [RName.PGRName(FilterExts = GamePlay.Scene.TtScene.AssetExt)]
        public RName PreviewSceneName { get; set; }

        [Rtti.Meta, Browsable(false)]
        public List<TtSequenceBinding> Bindings { get; set; } = new List<TtSequenceBinding>();

        [Browsable(false)]
        public long PlaybackDuration { get => PlaybackEndTick - PlaybackStartTick; }
        [Browsable(false)]
        public double PlaybackDurationSeconds { get => TickResolution.AsSeconds(PlaybackDuration); }

        public TtSequenceBinding FindBinding(in Guid bindingId)
        {
            for (int i = 0; i < Bindings.Count; ++i)
            {
                if (Bindings[i] != null && Bindings[i].BindingId == bindingId)
                    return Bindings[i];
            }
            return null;
        }
        /// <summary>
        /// 按目标节点 Guid 找绑定。同一个节点不能有两条绑定: 两条绑定上的轨道会写同一个
        /// 目标, 谁赢取决于遍历顺序。targetNodeId 为 Empty 时直接返回 null —— 那不是一个
        /// 能用来区分节点的值, 拿它去匹配会把所有轻量节点绑定当成同一个。
        /// </summary>
        public TtSequenceBinding FindBindingByNode(in Guid targetNodeId)
        {
            if (targetNodeId == Guid.Empty)
                return null;
            for (int i = 0; i < Bindings.Count; ++i)
            {
                if (Bindings[i] != null && Bindings[i].TargetNodeId == targetNodeId)
                    return Bindings[i];
            }
            return null;
        }
        /// <summary>
        /// 找已有绑定或新建一个。同一个 (参照节点, 相对路径) 只该有一个绑定 —— 否则两个绑定
        /// 上的轨道会写同一个目标, 谁赢取决于遍历顺序。
        /// </summary>
        public TtSequenceBinding GetOrCreateBinding(in Guid parentNodeId, string relativePath, string displayName)
        {
            return GetOrCreateBinding(Guid.Empty, in parentNodeId, relativePath, displayName);
        }
        /// <summary>
        /// 同上, 但同时存目标节点 Guid。去重必须先比 Guid 再比路径: 节点改名后路径变了,
        /// 光比路径会把同一个节点当成新节点而多建一条绑定。
        /// </summary>
        public TtSequenceBinding GetOrCreateBinding(in Guid targetNodeId, in Guid parentNodeId, string relativePath, string displayName)
        {
            var byNode = FindBindingByNode(in targetNodeId);
            if (byNode != null)
                return byNode;
            var path = relativePath != null ? relativePath : "";
            for (int i = 0; i < Bindings.Count; ++i)
            {
                var binding = Bindings[i];
                if (binding != null && binding.ParentNodeId == parentNodeId && binding.RelativePath == path)
                    return binding;
            }
            var result = new TtSequenceBinding()
            {
                TargetNodeId = targetNodeId,
                ParentNodeId = parentNodeId,
                RelativePath = path,
                DisplayName = string.IsNullOrEmpty(displayName) ? path : displayName,
            };
            Bindings.Add(result);
            return result;
        }
        public bool RemoveBinding(in Guid bindingId)
        {
            for (int i = 0; i < Bindings.Count; ++i)
            {
                if (Bindings[i] != null && Bindings[i].BindingId == bindingId)
                {
                    Bindings.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        /// <summary>所有绑定所有轨道的最大 EndTick</summary>
        public long GetMaxTick()
        {
            long max = 0;
            for (int i = 0; i < Bindings.Count; ++i)
            {
                if (Bindings[i] == null)
                    continue;
                max = Math.Max(max, Bindings[i].GetMaxTick());
            }
            return max;
        }
        /// <summary>
        /// 按内容把播放范围撑到至少能覆盖所有 Section。只放大不缩小: 用户可能故意把播放范围
        /// 设得比内容长 (末尾留一段静止), 自动缩回去会把这个意图抹掉。
        /// </summary>
        public void ExpandPlaybackRangeToContent()
        {
            var max = GetMaxTick();
            if (max > PlaybackEndTick)
                PlaybackEndTick = max;
        }
        /// <summary>把 tick 夹到播放范围内</summary>
        public long ClampTick(long tick)
        {
            if (tick < PlaybackStartTick)
                return PlaybackStartTick;
            if (tick > PlaybackEndTick)
                return PlaybackEndTick;
            return tick;
        }

        #region IAsset
        public IAssetMeta CreateAMeta()
        {
            return new TtSequenceAMeta();
        }
        public IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("Sequence", 0, 0))
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
                ameta.SaveAMeta(this);
            }
        }
        public static TtSequence LoadXnd(TtSequenceManager manager, IO.TtXndHolder holder)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = holder.RootNode.TryGetAttribute("Sequence");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }
                return result as TtSequence;
            }
        }
        public class SequenceCreateAttribute : IO.CommonCreateAttribute
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

    public class TtSequenceManager
    {
        Dictionary<RName, TtSequence> Sequences = new Dictionary<RName, TtSequence>();

        public async TtTask<TtSequence> GetSequence(RName name)
        {
            TtSequence result;
            if (Sequences.TryGetValue(name, out result))
                return result;

            result = await CreateSequence(name);
            if (result == null)
                return null;

            Sequences[name] = result;
            return result;
        }
        public async TtTask<TtSequence> CreateSequence(RName name)
        {
            var result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd == null)
                        return null;

                    var sequence = TtSequence.LoadXnd(this, xnd);
                    if (sequence == null)
                        return null;

                    sequence.AssetName = name;
                    return sequence;
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return result;
        }
        public void Remove(RName name)
        {
            Sequences.Remove(name);
        }
    }
}

namespace EngineNS.Sequencer
{
    public partial class TtSequencerModule
    {
        public Asset.TtSequenceManager SequenceManager { get; } = new Asset.TtSequenceManager();
    }
}
