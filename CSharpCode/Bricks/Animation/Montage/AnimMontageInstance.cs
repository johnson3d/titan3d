using EngineNS.Animation.Asset;
using EngineNS.Animation.Notify;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.Sampler;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Montage
{
    public enum EMontageState
    {
        Stopped,
        Playing,
        /// <summary>
        /// 正在淡出, 仍然贡献Pose与位移, 权重到0后转为Stopped
        /// </summary>
        BlendingOut,
        Paused,
    }

    public delegate void FMontageEndedHandle(TtAnimMontageInstance instance, bool interrupted);
    public delegate void FMontageBlendingOutHandle(TtAnimMontageInstance instance, bool interrupted);
    public delegate void FMontageNotifyHandle(TtAnimMontageInstance instance, IAnimNotify notify);
    public delegate void FMontageSectionChangedHandle(TtAnimMontageInstance instance, string sectionName);

    /// <summary>
    /// 一次Montage播放的运行时实例, 对标UE的FAnimMontageInstance。
    /// 负责时间推进(按Section与段落边界子步进)、Section跳转链、通知派发、
    /// 进出混合权重, 以及RootMotion的逐帧累积。
    /// </summary>
    public class TtAnimMontageInstance
    {
        const float TimeEpsilon = 1e-5f;
        const int MaxSubStepPerFrame = 32;

        public TtAnimMontage Montage { get; private set; } = null;
        /// <summary>
        /// 当前在Montage时间轴上的位置(秒)
        /// </summary>
        public float Position { get; private set; } = 0.0f;
        public float PlayRate { get; set; } = 1.0f;
        /// <summary>
        /// 当前混合权重, Slot节点用它把Montage的Pose叠到基础Pose上
        /// </summary>
        public float Weight { get; private set; } = 0.0f;
        public float DesiredWeight { get => mBlendTargetWeight; }
        public EMontageState State { get; private set; } = EMontageState.Stopped;
        public string CurrentSectionName { get; private set; } = null;
        public bool IsActive { get => State != EMontageState.Stopped; }
        /// <summary>
        /// 本帧累积的RootMotion(来自第一条Slot轨道)
        /// </summary>
        public FRootMotionData RootMotion { get => mRootMotion; }

        public event FMontageEndedHandle OnEnded;
        public event FMontageBlendingOutHandle OnBlendingOutStarted;
        public event FMontageNotifyHandle OnNotify;
        public event FMontageSectionChangedHandle OnSectionChanged;

        float mBlendStartWeight = 0.0f;
        float mBlendTargetWeight = 0.0f;
        float mBlendDuration = 0.0f;
        float mBlendElapsed = 0.0f;
        bool mInterrupted = false;
        FRootMotionData mRootMotion = FRootMotionData.Empty;
        Int64 mLastNotifyScanMS = -1;

        Dictionary<string, string> mNextSectionOverrides = new Dictionary<string, string>();
        Dictionary<TtMontageSegment, TtClipPoseSampler> mSamplers = new Dictionary<TtMontageSegment, TtClipPoseSampler>();
        HashSet<Guid> mTriggeredNotifiesThisAdvance = new HashSet<Guid>();
        TtLocalSpaceRuntimePose mBlendTempPose = null;

        public bool Initialize(TtAnimMontage montage, TtAnimatableSkeletonPose bindingPose)
        {
            Montage = montage;
            mSamplers.Clear();
            mBlendTempPose = null;
            if (montage == null || bindingPose == null)
                return false;

            for (int i = 0; i < montage.SlotTracks.Count; ++i)
            {
                var segments = montage.SlotTracks[i].Segments;
                for (int j = 0; j < segments.Count; ++j)
                {
                    var segment = segments[j];
                    if (segment.Clip == null || mSamplers.ContainsKey(segment))
                        continue;

                    var sampler = new TtClipPoseSampler();
                    if (sampler.Initialize(segment.Clip, bindingPose))
                    {
                        mSamplers.Add(segment, sampler);
                        if (mBlendTempPose == null)
                            mBlendTempPose = sampler.CreateOutPose();
                    }
                }
            }
            return true;
        }

        #region 播放控制

        public void Play(float playRate, float blendInTime, string startSectionName)
        {
            if (Montage == null)
                return;

            PlayRate = Math.Abs(playRate) > MathHelper.Epsilon ? playRate : 1.0f;
            State = EMontageState.Playing;
            mInterrupted = false;
            mNextSectionOverrides.Clear();
            mLastNotifyScanMS = -1;
            mRootMotion = FRootMotionData.Empty;

            var sectionIndex = Montage.FindSectionIndex(startSectionName);
            if (sectionIndex < 0 && Montage.Sections.Count > 0)
                sectionIndex = Montage.GetSectionIndicesOrderByTime()[0];

            if (sectionIndex >= 0)
            {
                CurrentSectionName = Montage.Sections[sectionIndex].Name;
                Position = Montage.Sections[sectionIndex].StartTime;
            }
            else
            {
                CurrentSectionName = null;
                Position = 0.0f;
            }

            StartBlend(0.0f, 1.0f, blendInTime);
        }

        /// <summary>
        /// 停止播放。blendOutTime为0时立即结束。
        /// </summary>
        public void Stop(float blendOutTime, bool interrupted)
        {
            if (State == EMontageState.Stopped)
                return;

            mInterrupted = interrupted;
            if (blendOutTime <= 0.0f)
            {
                Weight = 0.0f;
                State = EMontageState.Stopped;
                OnBlendingOutStarted?.Invoke(this, interrupted);
                OnEnded?.Invoke(this, interrupted);
                return;
            }

            if (State == EMontageState.BlendingOut)
                return;

            State = EMontageState.BlendingOut;
            StartBlend(Weight, 0.0f, blendOutTime);
            OnBlendingOutStarted?.Invoke(this, interrupted);
        }

        public void Pause()
        {
            if (State == EMontageState.Playing)
                State = EMontageState.Paused;
        }
        public void Resume()
        {
            if (State == EMontageState.Paused)
                State = EMontageState.Playing;
        }

        /// <summary>
        /// 立即跳到指定Section的起点(或终点)。跳转不产生RootMotion。
        /// </summary>
        public bool JumpToSection(string sectionName, bool toEndOfSection = false)
        {
            if (Montage == null)
                return false;

            var index = Montage.FindSectionIndex(sectionName);
            if (index < 0)
                return false;

            CurrentSectionName = Montage.Sections[index].Name;
            Position = toEndOfSection ? Montage.GetSectionEndTime(index) : Montage.Sections[index].StartTime;
            mLastNotifyScanMS = -1;
            OnSectionChanged?.Invoke(this, CurrentSectionName);
            return true;
        }

        /// <summary>
        /// 覆盖某个Section播完后的跳转目标, 传null表示走完就结束(可用于打断循环)
        /// </summary>
        public void SetNextSectionName(string sectionName, string nextSectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                return;
            mNextSectionOverrides[sectionName] = nextSectionName;
        }

        /// <summary>
        /// 覆盖当前位置。供编辑器时间轴拖播放头使用, 不产生RootMotion也不触发通知。
        /// </summary>
        public void SetPosition(float position)
        {
            if (Montage == null)
                return;

            Position = MathHelper.Clamp(position, 0.0f, Montage.Duration);
            mLastNotifyScanMS = -1;
            mRootMotion = FRootMotionData.Empty;
            var sectionIndex = Montage.GetSectionIndexFromPosition(Position);
            if (sectionIndex >= 0)
                CurrentSectionName = Montage.Sections[sectionIndex].Name;
        }

        /// <summary>
        /// 覆盖当前权重。供编辑器预览时强制满权重使用。
        /// </summary>
        public void SetWeightForPreview(float weight)
        {
            Weight = MathHelper.Clamp(weight, 0.0f, 1.0f);
            mBlendStartWeight = Weight;
            mBlendTargetWeight = Weight;
            mBlendDuration = 0.0f;
            mBlendElapsed = 0.0f;
        }

        /// <summary>
        /// 覆盖当前状态。供编辑器预览控制播放/暂停使用。
        /// </summary>
        public void SetStateForPreview(EMontageState state)
        {
            State = state;
        }

        string GetNextSectionName(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
                return null;

            string overrideName;
            if (mNextSectionOverrides.TryGetValue(sectionName, out overrideName))
                return overrideName;

            var index = Montage.FindSectionIndex(sectionName);
            if (index < 0)
                return null;
            return Montage.Sections[index].NextSectionName;
        }

        void StartBlend(float fromWeight, float toWeight, float duration)
        {
            mBlendStartWeight = fromWeight;
            mBlendTargetWeight = toWeight;
            mBlendDuration = Math.Max(0.0f, duration);
            mBlendElapsed = 0.0f;
            if (mBlendDuration <= 0.0f)
                Weight = toWeight;
            else
                Weight = fromWeight;
        }
        #endregion 播放控制

        #region 推进

        /// <summary>
        /// 推进一帧: 更新权重、按边界子步进推进位置、派发通知、累积RootMotion
        /// </summary>
        public void Advance(float deltaSeconds, ERootMotionMode rootMotionMode)
        {
            mRootMotion = FRootMotionData.Empty;
            mTriggeredNotifiesThisAdvance.Clear();

            if (Montage == null || State == EMontageState.Stopped)
                return;

            UpdateBlendWeight(deltaSeconds);
            if (State == EMontageState.Stopped || State == EMontageState.Paused)
                return;

            float remaining = deltaSeconds * PlayRate;
            if (remaining <= 0.0f)
                return;

            int subStep = 0;
            while (remaining > TimeEpsilon && State != EMontageState.Stopped && subStep < MaxSubStepPerFrame)
            {
                ++subStep;

                int sectionIndex = Montage.FindSectionIndex(CurrentSectionName);
                float sectionEnd = sectionIndex >= 0 ? Montage.GetSectionEndTime(sectionIndex) : Montage.Duration;
                if (sectionEnd <= Position + TimeEpsilon)
                    sectionEnd = Montage.Duration;

                // 段落边界也要作为子步进的切分点, 否则跨段落求RootMotion会用错的动画时刻
                float boundary = GetNextBoundary(Position, sectionEnd);
                float step = Math.Min(remaining, boundary - Position);
                if (step <= TimeEpsilon)
                {
                    // 已经贴在边界上, 直接走跳转判定, 避免死循环
                    step = 0.0f;
                }

                float prevPosition = Position;
                Position += step;
                remaining -= step;

                if (step > 0.0f)
                {
                    AccumulateRootMotion(prevPosition, Position, rootMotionMode);
                    TriggerNotifies(prevPosition, Position);
                }

                CheckAutoBlendOut(sectionEnd);

                if (Position < sectionEnd - TimeEpsilon)
                    continue;

                // 到达Section结尾, 按跳转链决定下一步
                var nextSectionName = GetNextSectionName(CurrentSectionName);
                var nextIndex = Montage.FindSectionIndex(nextSectionName);
                if (nextIndex >= 0)
                {
                    CurrentSectionName = Montage.Sections[nextIndex].Name;
                    Position = Montage.Sections[nextIndex].StartTime;
                    mLastNotifyScanMS = -1;
                    OnSectionChanged?.Invoke(this, CurrentSectionName);
                    continue;
                }

                Position = sectionEnd;
                if (State == EMontageState.Playing)
                {
                    if (Montage.EnableAutoBlendOut)
                        Stop(Montage.BlendOutTime, false);
                }
                // 关闭自动淡出时保持最后一帧, 等显式Stop
                break;
            }
        }

        void UpdateBlendWeight(float deltaSeconds)
        {
            if (mBlendDuration <= 0.0f)
            {
                Weight = mBlendTargetWeight;
            }
            else
            {
                mBlendElapsed += deltaSeconds;
                float alpha = MathHelper.Clamp(mBlendElapsed / mBlendDuration, 0.0f, 1.0f);
                Weight = mBlendStartWeight + (mBlendTargetWeight - mBlendStartWeight) * alpha;
            }

            if (State == EMontageState.BlendingOut && Weight <= 0.0f)
            {
                Weight = 0.0f;
                State = EMontageState.Stopped;
                OnEnded?.Invoke(this, mInterrupted);
            }
        }

        /// <summary>
        /// 取position之后最近的一个切分点: Section结尾或RootMotion轨道上段落的起止位置
        /// </summary>
        float GetNextBoundary(float position, float sectionEnd)
        {
            float boundary = sectionEnd;
            var track = GetRootMotionTrack();
            if (track != null)
            {
                for (int i = 0; i < track.Segments.Count; ++i)
                {
                    var segment = track.Segments[i];
                    if (segment.StartPos > position + TimeEpsilon && segment.StartPos < boundary)
                        boundary = segment.StartPos;
                    if (segment.EndPos > position + TimeEpsilon && segment.EndPos < boundary)
                        boundary = segment.EndPos;
                }
            }
            return boundary;
        }

        /// <summary>
        /// RootMotion只从第一条Slot轨道提取。多条轨道都带位移会导致重复累加,
        /// 与UE只从SyncSlot取位移的做法一致。
        /// </summary>
        TtMontageSlotTrack GetRootMotionTrack()
        {
            if (Montage == null || Montage.SlotTracks.Count == 0)
                return null;
            return Montage.SlotTracks[0];
        }

        void AccumulateRootMotion(float prevPosition, float curPosition, ERootMotionMode rootMotionMode)
        {
            if (!TtClipPoseSampler.IsRootMotionEnabled(rootMotionMode, true))
                return;

            var track = GetRootMotionTrack();
            if (track == null)
                return;

            // 子步进已保证区间落在同一个段落内, 用区间中点定位段落
            float mid = (prevPosition + curPosition) * 0.5f;
            var segment = FindSegment(track, mid);
            if (segment == null)
                return;

            TtClipPoseSampler sampler;
            if (!mSamplers.TryGetValue(segment, out sampler) || !sampler.HasRootMotion)
                return;

            var clipPrev = segment.ToClipTime(prevPosition);
            var clipCur = segment.ToClipTime(curPosition);
            var data = sampler.ExtractRootMotion(clipPrev, clipCur, false, true);
            if (!data.HasRootMotion)
                return;

            if (mRootMotion.HasRootMotion)
            {
                mRootMotion.Delta = TtRootMotionUtil.Combine(mRootMotion.Delta, data.Delta);
                mRootMotion.FromMontage = true;
            }
            else
            {
                mRootMotion = data;
            }
        }

        void TriggerNotifies(float prevPosition, float curPosition)
        {
            if (Montage.Notifies.Count == 0)
                return;

            Int64 beforeMS = (Int64)(prevPosition * 1000);
            Int64 afterMS = (Int64)(curPosition * 1000);
            // 上一次已经扫描过的毫秒不再作为闭区间左端, 否则边界上的瞬时通知会被触发两次
            if (mLastNotifyScanMS >= 0 && beforeMS <= mLastNotifyScanMS)
                beforeMS = mLastNotifyScanMS + 1;
            mLastNotifyScanMS = afterMS;
            if (beforeMS > afterMS)
                return;

            for (int i = 0; i < Montage.Notifies.Count; ++i)
            {
                var notify = Montage.Notifies[i];
                if (notify == null || mTriggeredNotifiesThisAdvance.Contains(notify.ID))
                    continue;
                if (!notify.CanTrigger(beforeMS, afterMS))
                    continue;

                mTriggeredNotifiesThisAdvance.Add(notify.ID);
                notify.Trigger(beforeMS, afterMS);
                OnNotify?.Invoke(this, notify);
            }
        }

        void CheckAutoBlendOut(float sectionEnd)
        {
            if (State != EMontageState.Playing || !Montage.EnableAutoBlendOut)
                return;
            // 还有下一段要跳, 不能在这里淡出
            if (Montage.FindSectionIndex(GetNextSectionName(CurrentSectionName)) >= 0)
                return;

            float blendOutTime = Montage.BlendOutTime;
            float triggerTime = Montage.BlendOutTriggerTime >= 0.0f
                ? sectionEnd - Montage.BlendOutTriggerTime
                : sectionEnd - blendOutTime;
            if (Position >= triggerTime - TimeEpsilon && blendOutTime > 0.0f)
                Stop(blendOutTime, false);
        }
        #endregion 推进

        #region 求值

        static TtMontageSegment FindSegment(TtMontageSlotTrack track, float position)
        {
            for (int i = 0; i < track.Segments.Count; ++i)
            {
                if (track.Segments[i].Clip != null && track.Segments[i].Contains(position))
                    return track.Segments[i];
            }
            return null;
        }

        /// <summary>
        /// 求指定Slot在当前位置的Pose。位置落在段落之外(段落间空隙)时返回false,
        /// 由Slot节点保持Source Pose。
        /// </summary>
        public bool EvaluateSlotPose(string slotName, ref TtLocalSpaceRuntimePose outPose)
        {
            if (Montage == null || outPose == null)
                return false;

            var track = Montage.FindSlotTrack(slotName);
            if (track == null)
                return false;

            TtMontageSegment first = null;
            TtMontageSegment second = null;
            for (int i = 0; i < track.Segments.Count; ++i)
            {
                var segment = track.Segments[i];
                if (segment.Clip == null || !segment.Contains(Position))
                    continue;
                if (!mSamplers.ContainsKey(segment))
                    continue;

                if (first == null)
                    first = segment;
                else if (second == null)
                    second = segment;
            }
            if (first == null)
                return false;

            if (second == null || mBlendTempPose == null)
            {
                SampleSegment(first, ref outPose);
            }
            else
            {
                // 段落重叠区做线性交叉淡化, 后开始的段落权重从0涨到1
                var earlier = first.StartPos <= second.StartPos ? first : second;
                var later = first.StartPos <= second.StartPos ? second : first;
                float overlap = earlier.EndPos - later.StartPos;
                float alpha = overlap > TimeEpsilon ? MathHelper.Clamp((Position - later.StartPos) / overlap, 0.0f, 1.0f) : 1.0f;

                SampleSegment(earlier, ref outPose);
                SampleSegment(later, ref mBlendTempPose);
                TtRuntimePoseUtility.BlendPoses(ref outPose, outPose, mBlendTempPose, alpha);
            }

            // RootMotion由Advance阶段累积, 只挂在提取位移的那条轨道上
            outPose.RootMotion = (track == GetRootMotionTrack()) ? mRootMotion : FRootMotionData.Empty;
            return true;
        }

        void SampleSegment(TtMontageSegment segment, ref TtLocalSpaceRuntimePose outPose)
        {
            TtClipPoseSampler sampler;
            if (!mSamplers.TryGetValue(segment, out sampler))
                return;

            var clipTime = segment.ToClipTime(Position);
            sampler.Sample(clipTime, ref outPose);
            // Montage的位移由实例统一累积, 这里必须锁根骨骼防止位移被应用两次
            sampler.ApplyRootLock(ref outPose, sampler.HasRootMotion);
        }
        #endregion 求值
    }
}
