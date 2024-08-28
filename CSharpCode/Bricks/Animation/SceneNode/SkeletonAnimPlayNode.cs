using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SceneNode
{
    public class TtSkeletonAnimPlayNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtSkeletonAnimPlayNodeData : TtNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Animation.Asset.TtAnimationClip.AssetExt)]
            public RName AnimatinName { get; set; }
            public TtMeshNode AnimatedMeshNode { get; set; } = null;
        }
        public Animation.Player.TtSkeletonAnimationPlayer Player { get; set; }

        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            var animPlayNodeData = NodeData as TtSkeletonAnimPlayNodeData;
            var skeletonAnimClip = await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);
            Player = new Player.TtSkeletonAnimationPlayer(skeletonAnimClip);
            return true;
        }
        public void BindingTo(TtMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
            mAnimatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimePose = mAnimatedPose;
            Player.BindingPose(animatablePose);
        }
        TtLocalSpaceRuntimePose mAnimatedPose = null;
        public override void TickLogic(TtNodeTickParameters args)
        {
            Player.Update(args.World.DeltaTimeSecond);
            Player.Evaluate();
            TtRuntimePoseUtility.CopyPose(ref mAnimatedPose, Player.OutPose);
        }

        public static async Thread.Async.TtTask<TtSkeletonAnimPlayNode> AddSkeletonAnimPlayNode(GamePlay.UWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is TtMeshNode);
            var node = new Animation.SceneNode.TtSkeletonAnimPlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.BindingTo(parent as TtMeshNode);
            node.Parent = parent;

            return node;
        }
    }
    public class TtAnimStateMachinePlayNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        public Animation.Player.TtAnimStateMachinePlayer Player { get; set; }

        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            Player = new Player.TtAnimStateMachinePlayer();
            Player.Initialize();
            return true;
        }
        public async Thread.Async.TtTask BindingTo(TtMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
            mAnimatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimePose = mAnimatedPose;
            await Player.BindingPose(animatablePose);
        }
        TtLocalSpaceRuntimePose mAnimatedPose = null;
        public override void TickLogic(TtNodeTickParameters args)
        {
            Player.Update(args.World.DeltaTimeSecond);
            Player.Evaluate();
            if (Player.OutPose == null)
                return;
            TtRuntimePoseUtility.CopyPose(ref mAnimatedPose, Player.OutPose);
        }

        public static async System.Threading.Tasks.Task<TtAnimStateMachinePlayNode> Add(GamePlay.UWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is TtMeshNode);
            var node = new Animation.SceneNode.TtAnimStateMachinePlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.Parent = parent;
            await node.BindingTo(parent as TtMeshNode);

            return node;
        }
    }
}
