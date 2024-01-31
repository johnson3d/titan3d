using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SceneNode
{
    public class USkeletonAnimPlayNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class USkeletonAnimPlayNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Animation.Asset.UAnimationClip.AssetExt)]
            public RName AnimatinName { get; set; }
            public UMeshNode AnimatedMeshNode { get; set; } = null;
        }
        public Animation.Player.USkeletonAnimationPlayer Player { get; set; }

        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if(!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            var animPlayNodeData = NodeData as USkeletonAnimPlayNodeData;
            var skeletonAnimClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);
            Player = new Player.USkeletonAnimationPlayer(skeletonAnimClip);
            return true;
        }
        public void BindingTo(UMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
            mAnimatedPose = SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.CreateMeshSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimeMeshSpacePose = mAnimatedPose;
            Player.BindingPose(animatablePose);
        }
        UMeshSpaceRuntimePose mAnimatedPose = null;
        public override void TickLogic(TtNodeTickParameters args)
        {
            Player.Update(args.World.DeltaTimeSecond);
            Player.Evaluate();
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref mAnimatedPose, Player.OutPose);
        }

        public static async System.Threading.Tasks.Task<USkeletonAnimPlayNode> AddSkeletonAnimPlayNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is UMeshNode);
            var node = new Animation.SceneNode.USkeletonAnimPlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.BindingTo(parent as UMeshNode);
            node.Parent = parent;

            return node;
        }
    }
}
