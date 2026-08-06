using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.Animation.KawaiiPhysics;
using EngineNS.DesignMacross;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class TtKawaiiPhysicsCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> FromCommand { get; set; } = null;

        public TtKawaiiPhysicsCommandDesc Desc { get; set; }

        private TtMeshSpaceRuntimePose FromMeshSpaceRuntimePose = new();
        private TtLocalSpaceRuntimePose KawaiiLocalPose = new();

        public override void Execute()
        {
            if (FromCommand == null)
                return;

            TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref FromMeshSpaceRuntimePose, FromCommand.OutPose);
            Desc.KawaiiComponent.UpdateFromMeshPose(FromMeshSpaceRuntimePose, Desc.ElapseSecond);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref KawaiiLocalPose, FromMeshSpaceRuntimePose);
            TtRuntimePoseUtility.BlendPoses(ref mOutPose, FromCommand.OutPose, KawaiiLocalPose, Desc.Alpha);
        }
    }
    public class TtKawaiiPhysicsCommandDesc : IAnimationCommandDesc
    {
        public float ElapseSecond { get; set; } = 0;
        public float Alpha { get; set; } = 1;
        public TtKawaiiPhysicsComponent KawaiiComponent { get; set; } = null;
    }
    public class TtLocalSpaceBlendTree_KawaiiPhysics<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> FromNode { get; set; }
        TtKawaiiPhysicsCommand<S> mAnimationCommand = null;
        public TtKawaiiPhysicsComponent KawaiiComponent { get; set; } = new();
        public TtKawaiiPhysicsCommandDesc CommandDesc { get; set; } = new();
        public List<TtKawaiiChainSetup> ChainSetups { get; set; } = null;
        public List<TtKawaiiClothSetup> ClothSetups { get; set; } = null;
        public List<TtKawaiiRodSetup> RodSetups { get; set; } = null;
        public override TtTask<bool> Initialize(FAnimBlendTreeContext context)
        {
            mAnimationCommand = new();
            mAnimationCommand.Desc = CommandDesc;
            mAnimationCommand.OutPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(context.AnimatableSkeletonPose);

            var animatablePose = context.AnimatableSkeletonPose;
            var localPose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            var meshSpacePose = TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(localPose);
            List<Vector3> bonePositions = new();
            List<Quaternion> boneRotations = new();
            List<Vector3> boneScales = new();
            List<int> parentIndices = new();
            for (int i = 0; i < meshSpacePose.Transforms.Count; i++)
            {
                var transforms = meshSpacePose.Transforms;
                bonePositions.Add(transforms[i].Position.ToSingleVector3());
                boneRotations.Add(transforms[i].Quat);
                boneScales.Add(transforms[i].Scale);
                var Descs = meshSpacePose.Descs;
                var Index = TtRuntimePoseUtility.GetIndex(Descs[i].ParentHash, localPose);
                parentIndices.Add(Index.Value);
            }

            KawaiiComponent.Initialize(bonePositions.ToArray(), boneRotations.ToArray(), boneScales.ToArray(), parentIndices.ToArray(), ChainSetups.ToArray(), ClothSetups.ToArray(), RodSetups.ToArray());
            mAnimationCommand.Desc.KawaiiComponent = KawaiiComponent;

            return base.Initialize(context);
        }
        public override TtAnimationCommand<S, TtLocalSpaceRuntimePose> ConstructAnimationCommandTree(IAnimationCommand parentNode, ref FConstructAnimationCommandTreeContext context)
        {
            System.Diagnostics.Debug.Assert(FromNode != null);
            if (FromNode == null)
                return null;

            context.AddCommand(context.TreeDepth, mAnimationCommand);

            context.TreeDepth++;

            mAnimationCommand.FromCommand = FromNode.ConstructAnimationCommandTree(mAnimationCommand, ref context);
            return mAnimationCommand;
        }
        public override void Tick(float elapseSecond, ref FAnimBlendTreeContext context)
        {
            mAnimationCommand.Desc.ElapseSecond = elapseSecond;
            if (KawaiiComponent != null && CenterData is TtDesignMacrossBase dmc && dmc.MacrossNode != null)
            {
                var abs = dmc.MacrossNode.Placement.AbsTransform;
                KawaiiComponent.SetComponentTransform(
                    abs.Position.ToSingleVector3(),
                    abs.Quat,
                    abs.Scale);
            }
        }
    }
}
