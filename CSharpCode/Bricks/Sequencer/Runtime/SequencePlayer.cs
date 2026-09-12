using System;
using System.Collections.Generic;

namespace EngineNS.Sequencer
{
    public enum ESequencePlaybackState
    {
        Stopped,
        Playing,
        Paused,
    }
    public enum ESequenceLoopMode
    {
        /// <summary>播到末尾停住</summary>
        Once,
        /// <summary>回到起点继续</summary>
        Loop,
        /// <summary>到末尾反向播</summary>
        PingPong,
    }
    public enum ESequenceEvaluationType
    {
        /// <summary>按实际累计时间求值, 位置可以落在两个显示帧之间, 画面最平滑</summary>
        WithSubFrames,
        /// <summary>每次求值前把位置吸附到 DisplayRate 的整帧上, 结果可复现</summary>
        FrameLocked,
    }

    /// <summary>
    /// 序列播放器。一个播放器绑一条序列 + 一个场景, 编辑器 scrub 和游戏内播放走同一套代码。
    ///
    /// 核心设计是求值无状态: SetPosition(tick) 的结果只由 tick 决定, 不依赖上一帧的位置。
    /// 这样"拖动播放头到任意位置"和"顺序播放"走的是完全相同的路径, 不会出现编辑器里预览
    /// 正常、运行时却因为累积状态而不同的情况。Update 只负责推进 tick, 不参与求值。
    ///
    /// 时间累加用 double 秒而不是每帧把秒转成 tick 再累加 tick: 后者每帧都要取整, 误差会
    /// 逐帧累积, 长序列播到后面会明显偏慢。
    /// </summary>
    public class TtSequencePlayer
    {
        Asset.TtSequence mSequence = null;
        TtSequenceBindingResolver mResolver = new TtSequenceBindingResolver();
        TtSequenceEvalTable mEvalTable = new TtSequenceEvalTable();
        TtPreAnimatedStore mPreAnimated = new TtPreAnimatedStore();

        double mPositionSeconds = 0.0;
        long mPositionTick = 0;
        int mPlayDirection = 1;

        public Asset.TtSequence Sequence { get => mSequence; }
        public TtSequenceBindingResolver Resolver { get => mResolver; }
        public TtPreAnimatedStore PreAnimatedStore { get => mPreAnimated; }
        public ESequencePlaybackState State { get; private set; } = ESequencePlaybackState.Stopped;

        /// <summary>属性访问器来源, 默认用模块上的全局注册表</summary>
        public TtSequencePropertyRegistry Registry { get; set; } = null;
        public float PlayRate { get; set; } = 1.0f;
        public ESequenceLoopMode LoopMode { get; set; } = ESequenceLoopMode.Once;
        public ESequenceEvaluationType EvaluationType { get; set; } = ESequenceEvaluationType.WithSubFrames;
        /// <summary>
        /// Stop 时把所有被写过的属性恢复成播放前的值。编辑器预览必须开 —— 关掉序列编辑器后
        /// 场景不该留在播放头那一帧的姿态上。
        /// </summary>
        public bool RestoreStateOnStop { get; set; } = true;

        [System.ComponentModel.Browsable(false)]
        public long PositionTick { get => mPositionTick; }
        [System.ComponentModel.Browsable(false)]
        public double PositionSeconds { get => mPositionSeconds; }

        public void Initialize(Asset.TtSequence sequence, GamePlay.Scene.TtScene scene)
        {
            // 换序列前先恢复原值, 否则上一条序列改过的属性就永久留在场景里了
            if (mPreAnimated.Count > 0)
                mPreAnimated.RestoreAll();

            mSequence = sequence;
            mResolver.SetScene(scene);
            mEvalTable.Clear();
            State = ESequencePlaybackState.Stopped;
            mPlayDirection = 1;
            mPositionSeconds = 0.0;
            mPositionTick = sequence != null ? sequence.PlaybackStartTick : 0;
        }
        TtSequencePropertyRegistry GetRegistry()
        {
            if (Registry != null)
                return Registry;
            return TtEngine.Instance.SequencerModule.PropertyRegistry;
        }

        public void Play()
        {
            if (mSequence == null)
                return;
            State = ESequencePlaybackState.Playing;
        }
        public void Pause()
        {
            if (State == ESequencePlaybackState.Playing)
                State = ESequencePlaybackState.Paused;
        }
        public void Stop()
        {
            State = ESequencePlaybackState.Stopped;
            mPlayDirection = 1;
            if (RestoreStateOnStop)
                mPreAnimated.RestoreAll();
            if (mSequence != null)
            {
                mPositionTick = mSequence.PlaybackStartTick;
                mPositionSeconds = mSequence.TickResolution.AsSeconds(mPositionTick);
            }
        }
        /// <summary>推进播放位置。State 不是 Playing 时什么都不做。</summary>
        public void Update(float elapseSeconds)
        {
            if (mSequence == null || State != ESequencePlaybackState.Playing)
                return;

            var startSeconds = mSequence.TickResolution.AsSeconds(mSequence.PlaybackStartTick);
            var endSeconds = mSequence.TickResolution.AsSeconds(mSequence.PlaybackEndTick);
            var duration = endSeconds - startSeconds;

            mPositionSeconds += elapseSeconds * PlayRate * mPlayDirection;

            if (duration <= 0.0)
            {
                // 空序列或者播放范围没设: 停在起点求一次值就够, 不然 Loop 会除零
                mPositionSeconds = startSeconds;
                State = ESequencePlaybackState.Stopped;
            }
            else if (mPositionSeconds > endSeconds || mPositionSeconds < startSeconds)
            {
                switch (LoopMode)
                {
                    case ESequenceLoopMode.Once:
                        mPositionSeconds = mPlayDirection > 0 ? endSeconds : startSeconds;
                        State = ESequencePlaybackState.Stopped;
                        break;
                    case ESequenceLoopMode.Loop:
                        {
                            // 用取模而不是直接设回起点: 一帧跨过末尾时超出的那部分应该接着播,
                            // 否则高 PlayRate 下每圈都会丢掉一小段。
                            var offset = (mPositionSeconds - startSeconds) % duration;
                            if (offset < 0.0)
                                offset += duration;
                            mPositionSeconds = startSeconds + offset;
                        }
                        break;
                    case ESequenceLoopMode.PingPong:
                        {
                            var over = mPlayDirection > 0 ? mPositionSeconds - endSeconds : startSeconds - mPositionSeconds;
                            if (over > duration)
                                over = duration;
                            mPlayDirection = -mPlayDirection;
                            mPositionSeconds = mPlayDirection > 0 ? startSeconds + over : endSeconds - over;
                        }
                        break;
                }
            }

            var tick = mSequence.TickResolution.FromSeconds(mPositionSeconds);
            if (EvaluationType == ESequenceEvaluationType.FrameLocked)
                tick = mSequence.TickResolution.SnapTo(tick, mSequence.DisplayRate);
            SetPosition(tick);
        }
        /// <summary>
        /// 把播放头放到 tick 并立即求值。无状态: 同一个 tick 反复调结果一样。
        /// 编辑器拖动播放头直接调这个, 不要走 Update。
        /// </summary>
        public void SetPosition(long tick)
        {
            if (mSequence == null)
                return;

            var clamped = mSequence.ClampTick(tick);
            mPositionTick = clamped;
            mPositionSeconds = mSequence.TickResolution.AsSeconds(clamped);
            Evaluate(clamped);
        }
        void Evaluate(long tick)
        {
            var registry = GetRegistry();
            if (registry == null)
                return;

            mEvalTable.Clear();

            var ctx = new Asset.FSectionEvalContext()
            {
                Time = tick,
                Weight = 1.0f,
                TickResolution = mSequence.TickResolution,
                Table = mEvalTable,
                Registry = registry,
            };

            var bindings = mSequence.Bindings;
            for (int i = 0; i < bindings.Count; ++i)
            {
                var binding = bindings[i];
                if (binding == null)
                    continue;
                var target = mResolver.Resolve(binding);
                if (target == null)
                    continue;
                var tracks = binding.Tracks;
                for (int j = 0; j < tracks.Count; ++j)
                {
                    if (tracks[j] == null)
                        continue;
                    tracks[j].Evaluate(in ctx, target);
                }
            }

            mEvalTable.Flush(mPreAnimated);
        }
        /// <summary>
        /// 恢复原值但不改播放状态。编辑器关闭序列面板时用 —— 此时不该顺带把播放头也重置,
        /// 否则下次打开面板位置就丢了。
        /// </summary>
        public void RestorePreAnimatedState()
        {
            mPreAnimated.RestoreAll();
        }
    }
}
