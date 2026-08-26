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
    public class TtKawaiiRibbonCommand<S> : TtAnimationCommand<S, TtLocalSpaceRuntimePose>
    {
        public TtAnimationCommand<S, TtLocalSpaceRuntimePose> FromCommand { get; set; } = null;

        public TtKawaiiRibbonCommandDesc Desc { get; set; }

        private TtMeshSpaceRuntimePose FromMeshSpaceRuntimePose = new();
        private TtLocalSpaceRuntimePose RibbonLocalPose = new();

        public override void Execute()
        {
            if (FromCommand == null)
                return;

            TtRuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref FromMeshSpaceRuntimePose, FromCommand.OutPose);
            Desc.RibbonComponent.UpdateFromMeshPose(FromMeshSpaceRuntimePose, Desc.ElapseSecond);
            TtRuntimePoseUtility.ConvetToLocalSpaceRuntimePose(ref RibbonLocalPose, FromMeshSpaceRuntimePose);
            TtRuntimePoseUtility.BlendPoses(ref mOutPose, FromCommand.OutPose, RibbonLocalPose, Desc.Alpha);
        }
    }

    public class TtKawaiiRibbonCommandDesc : IAnimationCommandDesc
    {
        public float ElapseSecond { get; set; } = 0;
        public float Alpha { get; set; } = 1;
        public TtKawaiiPhysicsComponent RibbonComponent { get; set; } = null;
    }

    /// <summary>
    /// Ribbon blend tree node: overlays a procedural swing/flutter wave onto the incoming pose
    /// and outputs the result. Deliberately kinematic-only.
    ///
    /// It is a SEPARATE node from KawaiiPhysics rather than a mode inside it, so that physics is
    /// obtained by COMPOSITION instead of by duplicated flags: wire this node's pose output into
    /// a downstream KawaiiPhysics node and that node treats the waving pose as the animated pose
    /// (ApplyPoseStiffness pulls toward it, ApplyAngleLimit cones around it, while the length
    /// constraint keeps segment lengths from LengthFromRoot), so gravity / collision / inertia
    /// become corrections layered on the authored wave. The ribbon output can equally feed
    /// anything else in the graph.
    ///
    /// The native container is still KawaiiPhysicsContext, reused with only its ribbon solver
    /// populated - the chain/cloth/rod solvers stay empty, which costs nothing and avoids a
    /// second parallel binding surface.
    /// </summary>
    public class TtLocalSpaceBlendTree_KawaiiRibbon<S> : TtBlendTree<S, TtLocalSpaceRuntimePose>
    {
        public IBlendTree<S, TtLocalSpaceRuntimePose> FromNode { get; set; }
        TtKawaiiRibbonCommand<S> mAnimationCommand = null;
        public TtKawaiiPhysicsComponent RibbonComponent { get; set; } = new();
        public TtKawaiiRibbonCommandDesc CommandDesc { get; set; } = new();
        public List<TtKawaiiRibbonSetup> RibbonSetups { get; set; } = null;

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

            // Only ribbons are passed: chain / cloth / rod stay null so their solvers are never
            // built. ApplyComponentPhysicsSettings is irrelevant here (ribbons have no physics
            // settings), so it is left at its default.
            RibbonComponent.Initialize(
                bonePositions.ToArray(), boneRotations.ToArray(), boneScales.ToArray(), parentIndices.ToArray(),
                null, null, null, RibbonSetups?.ToArray());
            mAnimationCommand.Desc.RibbonComponent = RibbonComponent;

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
            // The component transform is still pushed even though the ribbon itself ignores actor
            // motion: it keeps the native context's Prev/Cur transforms coherent for whatever the
            // downstream node derives from them.
            if (RibbonComponent != null && CenterData is TtDesignMacrossBase dmc && dmc.MacrossNode != null)
            {
                var abs = dmc.MacrossNode.Placement.AbsTransform;
                RibbonComponent.SetComponentTransform(
                    abs.Position.ToSingleVector3(),
                    abs.Quat,
                    abs.Scale);
            }
        }
    }
}
