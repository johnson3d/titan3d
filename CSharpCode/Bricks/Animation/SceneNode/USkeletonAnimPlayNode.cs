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
        }
        public Animation.Player.USkeletonAnimationPlayer Player { get; set; }

        public override UNode Parent
        {
            get => base.Parent;
            set
            {
                base.Parent = value;
            }
        }

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
        void BindingTo(UMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var pose = meshNode?.Mesh?.MaterialMesh?.Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
            skinMDfQueue.SkinModifier.RuntimeMeshSpacePose = SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.CreateMeshSpaceRuntimePose(pose);
            Player.BindingPose(pose);
            Player.RuntimePose = skinMDfQueue.SkinModifier.RuntimeMeshSpacePose;
        }
        public override void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            Player.Update(world.DeltaTimeSecond);
            Player.Evaluate();
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
