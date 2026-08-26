using EngineNS.Animation.Asset;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Montage
{
    /// <summary>
    /// Montage的宿主, 对标UE中AnimInstance承担的Montage管理职责:
    /// 持有当前活跃的Montage实例、驱动推进、处理同Slot抢占, 并为BlendTree的Slot节点提供查询入口。
    /// 由动画播放节点持有并每帧Tick。
    /// </summary>
    public class TtAnimMontageHost
    {
        TtAnimatableSkeletonPose mBindingPose = null;
        List<TtAnimMontageInstance> mInstances = new List<TtAnimMontageInstance>();

        /// <summary>
        /// RootMotion过滤模式, 由持有本宿主的节点同步
        /// </summary>
        public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
        public List<TtAnimMontageInstance> Instances { get => mInstances; }
        public bool HasActiveMontage
        {
            get
            {
                for (int i = 0; i < mInstances.Count; ++i)
                {
                    if (mInstances[i].IsActive)
                        return true;
                }
                return false;
            }
        }

        public void BindingPose(TtAnimatableSkeletonPose bindingPose)
        {
            mBindingPose = bindingPose;
            mInstances.Clear();
        }

        #region 播放接口

        /// <summary>
        /// 播放一条Montage。blendInTime小于0时用资产上的BlendInTime。
        /// 同一个Slot上已有的Montage会按新Montage的BlendIn时间淡出(抢占)。
        /// </summary>
        public TtAnimMontageInstance Play(TtAnimMontage montage, float playRate = 1.0f, float blendInTime = -1.0f, string startSectionName = null)
        {
            if (montage == null || mBindingPose == null)
                return null;

            float blendIn = blendInTime >= 0.0f ? blendInTime : montage.BlendInTime;
            InterruptOverlappedSlots(montage, blendIn);

            var instance = new TtAnimMontageInstance();
            if (!instance.Initialize(montage, mBindingPose))
                return null;

            float rate = Math.Abs(playRate) > MathHelper.Epsilon ? playRate : montage.DefaultPlayRate;
            instance.Play(rate, blendIn, startSectionName);
            mInstances.Add(instance);
            return instance;
        }

        /// <summary>
        /// 让与新Montage共用Slot的实例淡出, 避免同一个Slot上两条Montage同时贡献Pose
        /// </summary>
        void InterruptOverlappedSlots(TtAnimMontage montage, float blendOutTime)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                var instance = mInstances[i];
                if (!instance.IsActive || instance.Montage == null)
                    continue;

                bool overlapped = false;
                for (int j = 0; j < montage.SlotTracks.Count && !overlapped; ++j)
                {
                    overlapped = instance.Montage.FindSlotTrack(montage.SlotTracks[j].SlotName) != null;
                }
                if (overlapped)
                    instance.Stop(blendOutTime, true);
            }
        }

        public void Stop(TtAnimMontage montage, float blendOutTime = -1.0f)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                var instance = mInstances[i];
                if (instance.Montage != montage)
                    continue;
                instance.Stop(blendOutTime >= 0.0f ? blendOutTime : montage.BlendOutTime, true);
            }
        }

        public void StopSlot(string slotName, float blendOutTime = -1.0f)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                var instance = mInstances[i];
                if (instance.Montage == null || instance.Montage.FindSlotTrack(slotName) == null)
                    continue;
                instance.Stop(blendOutTime >= 0.0f ? blendOutTime : instance.Montage.BlendOutTime, true);
            }
        }

        public void StopAll(float blendOutTime = -1.0f)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                var instance = mInstances[i];
                if (instance.Montage == null)
                    continue;
                instance.Stop(blendOutTime >= 0.0f ? blendOutTime : instance.Montage.BlendOutTime, true);
            }
        }

        public void Pause(TtAnimMontage montage)
        {
            var instance = FindInstance(montage);
            instance?.Pause();
        }
        public void Resume(TtAnimMontage montage)
        {
            var instance = FindInstance(montage);
            instance?.Resume();
        }
        public bool JumpToSection(TtAnimMontage montage, string sectionName)
        {
            var instance = FindInstance(montage);
            if (instance == null)
                return false;
            return instance.JumpToSection(sectionName);
        }
        public void SetNextSectionName(TtAnimMontage montage, string sectionName, string nextSectionName)
        {
            var instance = FindInstance(montage);
            instance?.SetNextSectionName(sectionName, nextSectionName);
        }
        #endregion 播放接口

        #region 查询

        public TtAnimMontageInstance FindInstance(TtAnimMontage montage)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                if (mInstances[i].Montage == montage && mInstances[i].IsActive)
                    return mInstances[i];
            }
            return null;
        }
        public bool IsPlaying(TtAnimMontage montage)
        {
            return FindInstance(montage) != null;
        }
        public float GetPosition(TtAnimMontage montage)
        {
            var instance = FindInstance(montage);
            return instance != null ? instance.Position : 0.0f;
        }
        public string GetCurrentSection(TtAnimMontage montage)
        {
            var instance = FindInstance(montage);
            return instance != null ? instance.CurrentSectionName : null;
        }
        /// <summary>
        /// 取指定Slot上权重最高的活跃实例, Slot节点用它求值
        /// </summary>
        public TtAnimMontageInstance GetActiveInstanceForSlot(string slotName)
        {
            TtAnimMontageInstance result = null;
            float bestWeight = -1.0f;
            for (int i = 0; i < mInstances.Count; ++i)
            {
                var instance = mInstances[i];
                if (!instance.IsActive || instance.Montage == null)
                    continue;
                if (instance.Montage.FindSlotTrack(slotName) == null)
                    continue;
                if (instance.Weight > bestWeight)
                {
                    bestWeight = instance.Weight;
                    result = instance;
                }
            }
            return result;
        }
        #endregion 查询

        /// <summary>
        /// 推进所有实例并回收已结束的实例。必须在BlendTree求值之前调用。
        /// </summary>
        public void Tick(float elapseSecond)
        {
            for (int i = 0; i < mInstances.Count; ++i)
            {
                mInstances[i].Advance(elapseSecond, RootMotionMode);
            }
            for (int i = mInstances.Count - 1; i >= 0; --i)
            {
                if (!mInstances[i].IsActive)
                    mInstances.RemoveAt(i);
            }
        }
    }
}
