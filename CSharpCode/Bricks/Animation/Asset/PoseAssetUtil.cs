using EngineNS.Animation.Asset;
using EngineNS.Animation.Animatable;
using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;

namespace EngineNS.Animation.Asset
{
    /// <summary>
    /// TtPoseAsset 相关的采样/导入工具。抽成独立静态方法而非埋在编辑器里, 便于单测,
    /// 且 PoseDriver 节点也可能复用同一采样逻辑。
    /// </summary>
    public static class TtPoseAssetUtil
    {
        /// <summary>
        /// 在给定时刻从 AnimationClip 采样出一份 local space pose。
        ///
        /// 采样链路与运行时 TtExtractPoseFromClipCommand 一致:
        ///   绑定曲线 -> 在 time 处求值(会写入 animatablePose 的骨骼变换) -> 转成 LocalSpaceRuntimePose。
        /// 因此结果与实际播放到该时刻时的姿势一致。
        /// </summary>
        /// <param name="clip">动画片段</param>
        /// <param name="bindPose">与该 clip 骨架匹配的可动画 pose(通常由预览 mesh 的骨架 CreatePose 得到)</param>
        /// <param name="time">采样时刻(秒)</param>
        /// <returns>采样出的 local space pose; 失败返回 null</returns>
        public static TtLocalSpaceRuntimePose SamplePoseFromClip(TtAnimationClip clip,
            TtAnimatableSkeletonPose bindPose, float time)
        {
            if (clip == null || bindPose == null)
                return null;

            // 绑定曲线到 bindPose 的骨骼属性上, 然后在指定时刻求值
            var bindedCurves = TtBindedCurveUtil.BindingCurves(clip, bindPose);
            for (int i = 0; i < bindedCurves.Count; ++i)
                bindedCurves[i].Evaluate(time);

            var pose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(bindPose);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref pose, bindPose);
            return pose;
        }

        /// <summary>
        /// 从一份 RuntimePose 取出骨骼名列表(按 Descs 顺序)。
        /// </summary>
        public static List<string> ExtractBoneNames(IRuntimePose pose)
        {
            var names = new List<string>();
            if (pose == null || pose.Descs == null)
                return names;
            for (int i = 0; i < pose.Descs.Count; ++i)
                names.Add(pose.Descs[i].Name);
            return names;
        }

        /// <summary>
        /// 把在某时刻采样到的 clip pose 作为一个命名 pose 导入到 PoseAsset。
        /// pose 的骨骼名取自采样结果的 Descs, 由 TtPoseAsset.AddOrReplacePose 按名对齐。
        /// </summary>
        /// <returns>导入的 pose; 失败返回 null</returns>
        public static TtPoseAssetPose ImportPoseFromClip(TtPoseAsset asset, TtAnimationClip clip,
            TtAnimatableSkeletonPose bindPose, float time, string poseName)
        {
            if (asset == null)
                return null;

            var sampled = SamplePoseFromClip(clip, bindPose, time);
            if (sampled == null)
                return null;

            var boneNames = ExtractBoneNames(sampled);
            if (boneNames.Count != sampled.Transforms.Count)
                return null;

            var finalName = string.IsNullOrEmpty(poseName)
                ? asset.MakeUniquePoseName(clip != null && clip.AssetName != null ? clip.AssetName.Name : "Pose")
                : poseName;

            return asset.AddOrReplacePose(finalName, boneNames, sampled.Transforms,
                clip != null ? clip.AssetName : null, time);
        }

        /// <summary>
        /// 把 PoseAsset 中某个命名 pose 应用到一份 RuntimePose 上(按骨骼名对齐), 供预览使用。
        /// 目标 pose 中在资产里找不到对应骨骼的, 保持原样不动。
        /// </summary>
        /// <returns>写入的骨骼数量</returns>
        public static int ApplyPoseToRuntimePose(TtPoseAsset asset, string poseName, IRuntimePose target)
        {
            if (asset == null || target == null || target.Descs == null)
                return 0;

            var pose = asset.FindPose(poseName);
            if (pose == null)
                return 0;

            int written = 0;
            for (int i = 0; i < target.Descs.Count && i < target.Transforms.Count; ++i)
            {
                int boneIndex = asset.BoneNames.IndexOf(target.Descs[i].Name);
                if (boneIndex < 0 || boneIndex >= pose.Transforms.Count)
                    continue;
                target.Transforms[i] = pose.Transforms[boneIndex];
                ++written;
            }
            return written;
        }
    }
}
