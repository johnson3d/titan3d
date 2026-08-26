using EngineNS.Animation.Asset;
using EngineNS.Animation.Notify;
using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Montage
{
    /// <summary>
    /// Montage推进逻辑的单测: Section跳转链、通知不重不漏、自动淡出、抢占淡出。
    /// 段落用显式的ClipStartTime/ClipEndTime构造长度, 因此不需要真实动画资产,
    /// 只验证时间推进与状态机部分(Pose求值需要资产, 由手工验证覆盖)。
    /// </summary>
    [UnitTest.TtTest]
    public class UTest_AnimMontage
    {
        const float Tolerance = 1e-3f;

        static TtAnimMontage MakeMontage(float length, bool withSections)
        {
            var montage = new TtAnimMontage();
            montage.BlendInTime = 0.0f;
            montage.BlendOutTime = 0.2f;
            montage.BlendOutTriggerTime = -1.0f;
            montage.EnableAutoBlendOut = true;

            var track = montage.AddSlotTrack("DefaultSlot");
            var segment = new TtMontageSegment();
            segment.StartPos = 0.0f;
            segment.ClipStartTime = 0.0f;
            segment.ClipEndTime = length;
            track.Segments.Add(segment);

            if (withSections)
            {
                montage.Sections.Add(new TtMontageSection() { Name = "A", StartTime = 0.0f, NextSectionName = "B" });
                montage.Sections.Add(new TtMontageSection() { Name = "B", StartTime = length * 0.5f, NextSectionName = null });
            }
            return montage;
        }

        static TtAnimMontageInstance MakeInstance(TtAnimMontage montage)
        {
            var instance = new TtAnimMontageInstance();
            // 空骨骼Pose即可: 段落没有Clip时不会创建采样器, 推进逻辑不依赖采样器
            instance.Initialize(montage, new TtAnimatableSkeletonPose());
            instance.Play(1.0f, 0.0f, null);
            return instance;
        }

        public void UnitTestEntrance()
        {
            TestDurationAndSegmentMapping();
            TestSectionChain();
            TestSectionLoop();
            TestNotifyTriggeredOnce();
            TestAutoBlendOut();
            TestHostPreemption();
        }

        void TestDurationAndSegmentMapping()
        {
            var montage = MakeMontage(2.0f, false);
            TtUnitTestManager.TAssert(Math.Abs(montage.Duration - 2.0f) < Tolerance,
                $"Montage duration should come from segment length: {montage.Duration}");

            var segment = montage.SlotTracks[0].Segments[0];
            segment.PlayRate = 2.0f;
            TtUnitTestManager.TAssert(Math.Abs(segment.Length - 1.0f) < Tolerance,
                $"PlayRate=2 should halve the segment length on timeline: {segment.Length}");
            TtUnitTestManager.TAssert(Math.Abs(segment.ToClipTime(0.5f) - 1.0f) < Tolerance,
                $"Timeline 0.5s at PlayRate=2 should map to clip 1.0s: {segment.ToClipTime(0.5f)}");
            segment.PlayRate = 1.0f;
        }

        void TestSectionChain()
        {
            var montage = MakeMontage(2.0f, true);
            var instance = MakeInstance(montage);
            TtUnitTestManager.TAssert(instance.CurrentSectionName == "A",
                $"Montage should start at the earliest section: {instance.CurrentSectionName}");

            instance.Advance(0.5f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(Math.Abs(instance.Position - 0.5f) < Tolerance && instance.CurrentSectionName == "A",
                $"Position/section wrong after 0.5s: {instance.Position} {instance.CurrentSectionName}");

            // 跨过A的结尾(1.0)后应跳到B并把剩余时间用在B上
            instance.Advance(0.6f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(instance.CurrentSectionName == "B",
                $"Should switch to section B after crossing its boundary: {instance.CurrentSectionName}");
            TtUnitTestManager.TAssert(Math.Abs(instance.Position - 1.1f) < Tolerance,
                $"Remaining time should be consumed in the next section: {instance.Position}");
        }

        void TestSectionLoop()
        {
            var montage = MakeMontage(2.0f, true);
            montage.Sections[1].NextSectionName = "B";
            var instance = MakeInstance(montage);
            instance.JumpToSection("B");
            TtUnitTestManager.TAssert(Math.Abs(instance.Position - 1.0f) < Tolerance,
                $"JumpToSection should land on the section start: {instance.Position}");

            // B自指形成循环: 走到结尾(2.0)后应回到B的起点而不是结束
            instance.Advance(1.2f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(instance.IsActive,
                "Self-looping section must not end the montage");
            TtUnitTestManager.TAssert(Math.Abs(instance.Position - 1.2f) < Tolerance,
                $"Looped position wrong: {instance.Position}");
        }

        void TestNotifyTriggeredOnce()
        {
            var montage = MakeMontage(2.0f, false);
            var notify = new TtTransientAnimNotify();
            notify.Name = "Hit";
            notify.TriggerTime = 250;
            montage.Notifies.Add(notify);

            var instance = MakeInstance(montage);
            int count = 0;
            instance.OnNotify += (inst, n) => { ++count; };

            for (int i = 0; i < 8; ++i)
                instance.Advance(0.1f, ERootMotionMode.Ignore);

            TtUnitTestManager.TAssert(count == 1, $"Transient notify should fire exactly once, got {count}");
        }

        void TestAutoBlendOut()
        {
            var montage = MakeMontage(1.0f, false);
            var instance = MakeInstance(montage);
            TtUnitTestManager.TAssert(Math.Abs(instance.Weight - 1.0f) < Tolerance,
                $"BlendIn=0 should make weight 1 immediately: {instance.Weight}");

            bool blendingOutFired = false;
            bool endedFired = false;
            instance.OnBlendingOutStarted += (inst, interrupted) => { blendingOutFired = true; };
            instance.OnEnded += (inst, interrupted) => { endedFired = true; };

            // BlendOutTriggerTime<0 => 在 结尾-BlendOutTime(=0.8s) 处开始淡出
            instance.Advance(0.7f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(!blendingOutFired, "Should not blend out before the trigger time");
            instance.Advance(0.2f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(blendingOutFired, "Should start blending out near the end");

            for (int i = 0; i < 5; ++i)
                instance.Advance(0.1f, ERootMotionMode.Ignore);
            TtUnitTestManager.TAssert(endedFired && !instance.IsActive,
                $"Montage should end after blending out: active={instance.IsActive}");
        }

        void TestHostPreemption()
        {
            var host = new TtAnimMontageHost();
            host.BindingPose(new TtAnimatableSkeletonPose());

            var first = host.Play(MakeMontage(2.0f, false), 1.0f, 0.0f, null);
            TtUnitTestManager.TAssert(first != null && first.IsActive, "Host should start the first montage");

            var second = host.Play(MakeMontage(2.0f, false), 1.0f, 0.1f, null);
            TtUnitTestManager.TAssert(second != null && second.IsActive, "Host should start the second montage");
            TtUnitTestManager.TAssert(first.State == EMontageState.BlendingOut,
                $"Same-slot montage must be preempted into blending out: {first.State}");

            // 抢占者淡出结束后应被宿主回收
            for (int i = 0; i < 5; ++i)
                host.Tick(0.1f);
            TtUnitTestManager.TAssert(host.Instances.Count == 1,
                $"Finished instances should be recycled, remaining {host.Instances.Count}");

            var active = host.GetActiveInstanceForSlot("DefaultSlot");
            TtUnitTestManager.TAssert(active == second, "The newest montage should own the slot");
        }
    }
}
