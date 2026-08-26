using EngineNS.Animation.RootMotion;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SceneNode
{
    public class TtSkeletonAnimPlayNode : GamePlay.Scene.TtLightWeightNodeBase, IRootMotionSource
    {
        public class TtSkeletonAnimPlayNodeData : TtNodeData
        {
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Animation.Asset.TtAnimationClip.AssetExt)]
            public RName AnimatinName { get; set; }
            [Rtti.Meta("")]
            public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
            public TtMeshNode AnimatedMeshNode { get; set; } = null;
        }
        public Animation.Player.TtSkeletonAnimationPlayer Player { get; set; }

        #region IRootMotionSource
        TtRootMotionAccumulator mRootMotionAccumulator = new TtRootMotionAccumulator();
        ERootMotionMode mRootMotionMode = ERootMotionMode.Ignore;
        public ERootMotionMode RootMotionMode
        {
            get => mRootMotionMode;
            set
            {
                mRootMotionMode = value;
                if (Player != null)
                    Player.RootMotionMode = value;
                if (NodeData is TtSkeletonAnimPlayNodeData data)
                    data.RootMotionMode = value;
            }
        }
        public bool ConsumeRootMotion(out FTransform delta)
        {
            return mRootMotionAccumulator.Consume(out delta);
        }
        #endregion IRootMotionSource

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            var animPlayNodeData = NodeData as TtSkeletonAnimPlayNodeData;
            var skeletonAnimClip = await animPlayNodeData.AnimatinName.GetAsset<Animation.Asset.TtAnimationClip>();
            Player = new Player.TtSkeletonAnimationPlayer(skeletonAnimClip);
            RootMotionMode = animPlayNodeData.RootMotionMode;
            return true;
        }
        public void BindingTo(TtMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            //var animatablePose = meshNode?.RenderMesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var animatablePose = meshNode?.RenderMesh?.MaterialMesh?.GetSkeletonAsset().GetResultUntilCompleted().Skeleton.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.RenderMesh.MdfQueue as Graphics.Mesh.TtMdfSkinMesh;
            mAnimatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            meshNode.RuntimePose = mAnimatedPose;
            Player.BindingPose(animatablePose);
        }
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtSkeletonAnimPlayNode>.Scope;
        }
        TtLocalSpaceRuntimePose mAnimatedPose = null;
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            Player.RootMotionMode = mRootMotionMode;
            Player.Update(args.World.DeltaTimeSecond);
            Player.Evaluate();
            TtRuntimePoseUtility.CopyPose(ref mAnimatedPose, Player.OutPose);
            if (Player.OutPose != null)
                mRootMotionAccumulator.Submit(Player.OutPose.RootMotion);
            return true;
        }

        public static async Thread.Async.TtTask<TtSkeletonAnimPlayNode> AddSkeletonAnimPlayNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is TtMeshNode);
            
            var node = await TtNode.SpawnNode<Animation.SceneNode.TtSkeletonAnimPlayNode>(parent, async (nd)=>
            {
                nd.BindingTo(parent as TtMeshNode);
            },
            data, bvType, placementType);
            
            return node;
        }
    }
    public class TtAnimStateMachinePlayNode : GamePlay.Scene.TtLightWeightNodeBase, IRootMotionSource
    {
        public class TtAnimStateMachinePlayNodeData : TtNodeData
        {
            [Rtti.Meta("")]
            public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
        }
        public Animation.Player.TtAnimStateMachinePlayer Player { get; set; }
        /// <summary>
        /// Montage宿主, 供Gameplay侧调用Montage_Play等接口
        /// </summary>
        public Montage.TtAnimMontageHost MontageHost { get => Player?.MontageHost; }

        #region IRootMotionSource
        TtRootMotionAccumulator mRootMotionAccumulator = new TtRootMotionAccumulator();
        ERootMotionMode mRootMotionMode = ERootMotionMode.Ignore;
        public ERootMotionMode RootMotionMode
        {
            get => mRootMotionMode;
            set
            {
                mRootMotionMode = value;
                if (Player != null)
                    Player.RootMotionMode = value;
                if (NodeData is TtAnimStateMachinePlayNodeData data)
                    data.RootMotionMode = value;
            }
        }
        public bool ConsumeRootMotion(out FTransform delta)
        {
            return mRootMotionAccumulator.Consume(out delta);
        }
        #endregion IRootMotionSource

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            Player = new Player.TtAnimStateMachinePlayer();
            Player.Initialize();
            if (NodeData is TtAnimStateMachinePlayNodeData nodeData)
                RootMotionMode = nodeData.RootMotionMode;
            return true;
        }
        public async Thread.Async.TtTask BindingTo(TtMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);
            var sklAsset = await meshNode.RenderMesh.MaterialMesh.GetSkeletonAsset();
            var animatablePose = sklAsset?.Skeleton.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.RenderMesh.MdfQueue as Graphics.Mesh.TtMdfSkinMesh;
            mAnimatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            meshNode.RuntimePose = mAnimatedPose;
            Player.RootMotionMode = mRootMotionMode;
            await Player.BindingPose(animatablePose);
        }
        TtLocalSpaceRuntimePose mAnimatedPose = null;
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtAnimStateMachinePlayNode>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            Player.Update(args.World.DeltaTimeSecond);
            Player.Evaluate();
            if (Player.OutPose == null)
                return true;
            TtRuntimePoseUtility.CopyPose(ref mAnimatedPose, Player.OutPose);
            mRootMotionAccumulator.Submit(Player.OutPose.RootMotion);
            return true;
        }

        public static async System.Threading.Tasks.Task<TtAnimStateMachinePlayNode> Add(GamePlay.TtWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is TtMeshNode);
            var node = await TtNode.SpawnNode<Animation.SceneNode.TtAnimStateMachinePlayNode>(parent, async (nd)=>
            {
                await nd.BindingTo(parent as TtMeshNode);
            }, data, bvType, placementType);
            
            return node;
        }
    }
}
