using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SceneNode
{
    public class TtSkeletonAnimPlayNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class TtSkeletonAnimPlayNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Animation.Asset.TtAnimationClip.AssetExt)]
            public RName AnimatinName { get; set; }
            public UMeshNode AnimatedMeshNode { get; set; } = null;
        }
        public Animation.Player.TtSkeletonAnimationPlayer Player { get; set; }

        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            var animPlayNodeData = NodeData as TtSkeletonAnimPlayNodeData;
            var skeletonAnimClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);
            Player = new Player.TtSkeletonAnimationPlayer(skeletonAnimClip);
            return true;
        }
        public void BindingTo(UMeshNode meshNode)
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

        public static async System.Threading.Tasks.Task<TtSkeletonAnimPlayNode> AddSkeletonAnimPlayNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is UMeshNode);
            var node = new Animation.SceneNode.TtSkeletonAnimPlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.BindingTo(parent as UMeshNode);
            node.Parent = parent;

            return node;
        }
    }
}
