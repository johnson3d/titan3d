using EngineNS.Animation.Command;
using EngineNS.Animation.Montage;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtSlotCommandDesc : IAnimationCommandDesc
    {
        public string SlotName { get; set; } = "DefaultSlot";
    }

    /// <summary>
    /// 把Montage在指定Slot上的Pose按其混合权重叠加到Source Pose之上。
    /// RootMotion随Pose一起被BlendPoses按同一权重混合, 不需要额外通路。
    /// </summary>
    public class TtSlotCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> SourceCommand { get; set; } = null;
        public TtSlotCommandDesc Desc { get; set; } = null;
        public TtAnimMontageHost MontageHost { get; set; } = null;
        TtLocalSpaceRuntimePose mSlotPose = null;

        public void SetSlotPoseBuffer(TtLocalSpaceRuntimePose slotPose)
        {
            mSlotPose = slotPose;
        }

        public override void Execute()
        {
            var sourcePose = SourceCommand != null ? SourceCommand.OutPose : null;
            if (sourcePose != null)
                TtRuntimePoseUtility.CopyPose(ref mOutPose, sourcePose);

            if (MontageHost == null || Desc == null || mSlotPose == null)
                return;

            var instance = MontageHost.GetActiveInstanceForSlot(Desc.SlotName);
            if (instance == null || instance.Weight <= 0.0f)
                return;
            if (!instance.EvaluateSlotPose(Desc.SlotName, ref mSlotPose))
                return;

            if (sourcePose == null || instance.Weight >= 1.0f)
            {
                TtRuntimePoseUtility.CopyPose(ref mOutPose, mSlotPose);
                return;
            }
            TtRuntimePoseUtility.BlendPoses(ref mOutPose, sourcePose, mSlotPose, instance.Weight);
        }
    }

    /// <summary>
    /// Montage的Slot节点, 对标UE的AnimNode_Slot。放在基础Locomotion之后,
    /// Montage播放时按权重覆盖上层动作。
    /// </summary>
    public class TtBlendTree_Slot<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public string SlotName { get; set; } = "DefaultSlot";
        public IBlendTree<S, TtLocalSpaceRuntimePose> SourceNode { get; set; } = null;
        /// <summary>
        /// 是否由本节点驱动SourceNode的Tick。
        /// Source已经被其他者(如状态机)驱动时必须置false, 否则会双重推进混合计时。
        /// </summary>
        public bool TickSourceNode { get; set; } = true;

        TtSlotCommand<S> mSlotCommand = null;

        public override async Thread.Async.TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mSlotCommand = new TtSlotCommand<S>();
            mSlotCommand.Desc = new TtSlotCommandDesc();
            mSlotCommand.Desc.SlotName = SlotName;
            // 命令Execute阶段拿不到上下文, 宿主引用必须在这里捕获
            mSlotCommand.MontageHost = context.MontageHost;
            mSlotCommand.SetSlotPoseBuffer(TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose));
            mSlotCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);
            await base.Initialize(context);
            return true;
        }

        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            base.Tick(elapseSecond, ref context);
            mSlotCommand.Desc.SlotName = SlotName;
            if (mSlotCommand.MontageHost == null)
                mSlotCommand.MontageHost = context.MontageHost;
            if (TickSourceNode)
                SourceNode?.Tick(elapseSecond, ref context);
        }

        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            base.ConstructAnimationCommandTree(parentNode, ref context);
            context.AddCommand(context.TreeDepth, mSlotCommand);

            context.TreeDepth++;
            if (SourceNode != null)
            {
                mSlotCommand.SourceCommand = SourceNode.ConstructAnimationCommandTree(mSlotCommand, ref context);
            }
            return mSlotCommand;
        }
    }
}
