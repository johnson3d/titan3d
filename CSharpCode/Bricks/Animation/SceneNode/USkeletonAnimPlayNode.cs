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
                BindingToParent(value as UMeshNode);
                base.Parent = value;
            }
        }

        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var animPlayNodeData = data as USkeletonAnimPlayNodeData;
            Player = new Player.USkeletonAnimationPlayer();
            Player.SkeletonAnimClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);

            SetStyle(ENodeStyles.Invisible);
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        void BindingToParent(UMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var pose = meshNode?.Mesh?.MaterialMesh?.Mesh?.PartialSkeleton?.CreatePose();
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
            skinMDfQueue.SkinModifier.AnimatableSkeletonPose = pose as SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;

            Player.Binding(pose);
        }
        public override void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy)
        {
            Player.Update(world.DeltaTimeSecond);
            Player.Evaluate();
        }

        public static async System.Threading.Tasks.Task<USkeletonAnimPlayNode> AddSkeletonAnimPlayNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is UMeshNode);
            var scene = parent.GetNearestParentScene();
            var node = new Animation.SceneNode.USkeletonAnimPlayNode();
            //var node = await scene.NewNode(world, typeof(Animation.SceneNode.USkeletonAnimPlayNode), data, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement)) as USkeletonAnimPlayNode;

            await node.InitializeNode(world, data, bvType, placementType);
            node.Parent = parent;

            return node;
        }
    }
}
