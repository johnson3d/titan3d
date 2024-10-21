using EngineNS.Animation.Asset.BlendSpace;
using EngineNS.GamePlay.Controller;
using EngineNS.GamePlay.Movemnet;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SceneNode
{
    public struct FBlendSpacePoint
    {
        public Vector3 Value { get; set; }
        public RName Animation;
        public FBlendSpacePoint(RName animationName, Vector3 value)
        {
            Animation = animationName;
            Value = value;
        }
    }
    public class TtBlendSpaceAnimPlayNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class TtBlendSpaceAnimPlayNodeData : TtNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Animation.Asset.BlendSpace.TtBlendSpace2D.AssetExt)]
            public RName AnimatinName { get; set; }
            [Rtti.Meta]
            public bool OverrideAsset { get; set; } = false;
            [Rtti.Meta]
            public List<TtBlendSpace_Axis> Axises { get; set; } = new List<TtBlendSpace_Axis>();
            [Rtti.Meta]
            public List<FBlendSpacePoint> Points { get; set; } = new List<FBlendSpacePoint>();
        }
        public Animation.Player.TtBlendSpace2DPlayer Player { get; set; }

        public override TtNode Parent
        {
            get => base.Parent;
            set
            {
                base.Parent = value;

            }
        }

        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            TtBlendSpace2D bs2D = null;
            var animPlayNodeData = data as TtBlendSpaceAnimPlayNodeData;
            System.Diagnostics.Debug.Assert(animPlayNodeData != null);
            if (animPlayNodeData.OverrideAsset)
            {
                //var skeletonAnimClip = await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);
                bs2D = new TtBlendSpace2D();
                for (int i = 0; i < animPlayNodeData.Axises.Count; ++i)
                {
                    bs2D.BlendAxises[i] = animPlayNodeData.Axises[i];
                }
                for (int i = 0; i < animPlayNodeData.Points.Count; ++i)
                {
                    var animation = await TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.Points[i].Animation);
                    bs2D.AddPoint(animation, animPlayNodeData.Points[i].Value);
                }
                Player = new Player.TtBlendSpace2DPlayer(bs2D);
            }
            else
            {
                bs2D = await TtEngine.Instance.AnimationModule.BlendSpaceClipManager.GetAnimation(animPlayNodeData.AnimatinName);
                Player = new Player.TtBlendSpace2DPlayer(bs2D);
            }

            return true;
        }
        public void BindingTo(TtMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);

            var pose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;

            skinMDfQueue.SkinModifier.RuntimePose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(pose);
            Player.BindingPose(pose);
            Player.RuntimePose = skinMDfQueue.SkinModifier.RuntimePose;

        }
        GamePlay.Movemnet.TtMovement Movement = null;
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtBlendSpaceAnimPlayNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override void TickLogic(TtNodeTickParameters args)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (Movement == null)
                {
                    Movement = Parent.Parent.FindFirstChild<TtMovement>() as GamePlay.Movemnet.TtMovement;
                }
                Player.Input = new Vector3(Movement.LinearVelocity.Length(), 0, 0);
                Player.Update(args.World.DeltaTimeSecond);
                Player.Evaluate();
            }
        }

        public static async System.Threading.Tasks.Task<TtBlendSpaceAnimPlayNode> AddBlendSpace2DAnimPlayNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is TtMeshNode);
            var node = new Animation.SceneNode.TtBlendSpaceAnimPlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.BindingTo(parent as TtMeshNode);
            node.Parent = parent;
            return node;
        }
    }
}
