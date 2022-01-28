using EngineNS.Animation.Asset.BlendSpace;
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
    public class UBlendSpaceAnimPlayNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class UBlendSpaceAnimPlayNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Animation.Asset.BlendSpace.UBlendSpace2D.AssetExt)]
            public RName AnimatinName { get; set; }
            [Rtti.Meta]
            public bool OverrideAsset { get; set; } = false;
            [Rtti.Meta]
            public List<UBlendSpace_Axis> Axises { get; set; } = new List<UBlendSpace_Axis>();
            [Rtti.Meta]
            public List<FBlendSpacePoint> Points { get; set; } = new List<FBlendSpacePoint>();
        }
        public Animation.Player.UBlendSpace2DPlayer Player { get; set; }

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
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }

            UBlendSpace2D bs2D = null;
            var animPlayNodeData = data as UBlendSpaceAnimPlayNodeData;
            System.Diagnostics.Debug.Assert(animPlayNodeData != null);
            if (animPlayNodeData.OverrideAsset)
            {
                //var skeletonAnimClip = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.AnimatinName);
                bs2D = new UBlendSpace2D();
                for (int i = 0; i < animPlayNodeData.Axises.Count; ++i)
                {
                    bs2D.BlendAxises[i] = animPlayNodeData.Axises[i];
                }
                for (int i = 0; i < animPlayNodeData.Points.Count; ++i)
                {
                    var animation = await UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(animPlayNodeData.Points[i].Animation);
                    bs2D.AddPoint(animation, animPlayNodeData.Points[i].Value);
                }
                Player = new Player.UBlendSpace2DPlayer(bs2D);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false); //should be have asset editor and saveload manager
            }

            return true;
        }
        public void BindingTo(UMeshNode meshNode)
        {
            System.Diagnostics.Debug.Assert(meshNode != null);

            var pose = meshNode?.Mesh?.MaterialMesh?.Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
            var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;

            skinMDfQueue.SkinModifier.RuntimeMeshSpacePose = SkeletonAnimation.Runtime.Pose.URuntimePoseUtility.CreateMeshSpaceRuntimePose(pose);
            Player.BindingPose(pose);
            Player.RuntimePose = skinMDfQueue.SkinModifier.RuntimeMeshSpacePose;

        }
        GamePlay.Movemnet.UMovement Movement = null;
        public override void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            if (Movement == null)
            {
                Movement = Parent.Parent.FindFirstChild("Movement") as GamePlay.Movemnet.UMovement;
            }
            Player.Input = new Vector3((float)Movement.LinearVelocity.Length(), 0, 0);
            Player.Update(world.DeltaTimeSecond);
            Player.Evaluate();
        }

        public static async System.Threading.Tasks.Task<UBlendSpaceAnimPlayNode> AddBlendSpace2DAnimPlayNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            System.Diagnostics.Debug.Assert(parent is UMeshNode);
            var node = new Animation.SceneNode.UBlendSpaceAnimPlayNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.BindingTo(parent as UMeshNode);
            node.Parent = parent;
            return node;
        }
    }
}
