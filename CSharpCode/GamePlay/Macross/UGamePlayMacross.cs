using EngineNS.Animation.BlendTree;
using EngineNS.Animation.Command;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.StateMachine;
using EngineNS.Bricks.StateMachine;
using EngineNS.GamePlay.StateMachine.AnimationStateMachine;
using EngineNS.GamePlay.StateMachine.LogicalStateMachine;
using EngineNS.Graphics.Pipeline;
using EngineNS.Macross;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.GamePlayMacross
{
    public class UGamePlayMacrossNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class UGamePlayMacrossNodeData : UNodeData
        {
            public UGameplayMacross GamePlayMacross = null;
            public UMeshNode AnimatedMeshNode = null;
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }
            return true;
        }
        public override void TickLogic(UWorld world, URenderPolicy policy)
        {
            var macrossNodeData = NodeData as UGamePlayMacrossNodeData;
            macrossNodeData.GamePlayMacross.TickLogic(world.DeltaTimeSecond);
            macrossNodeData.GamePlayMacross.TickAnimation(world.DeltaTimeSecond);
            macrossNodeData.GamePlayMacross.EvaluateAnimation(world.DeltaTimeSecond);
            base.TickLogic(world, policy);
        }
        public static async System.Threading.Tasks.Task<UGamePlayMacrossNode> AddGamePlayMacrossNodeNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var node = new UGamePlayMacrossNode();
            await node.InitializeNode(world, data, bvType, placementType);
            node.Parent = parent;

            return node;
        }
    }
    [UMacross]
    public class UGameplayMacross
    {
        protected UGamePlayStateMachine mGamePlayStateMachine = new UGamePlayStateMachine();

        protected UMeshSpaceRuntimePose mMeshSpaceRuntimePose = null;

        public virtual void ConstructLogicGraph()
        {
        }
        public async virtual Task<bool> ConstructAnimGraph(UMeshNode animatedMeshNode)
        {
            return false;
        }

        public virtual void TickLogic(float elapseSecnod)
        {
            mGamePlayStateMachine.Tick(elapseSecnod);

        }
        public virtual void TickAnimation(float elapseSecnod)
        {

        }
        public virtual void EvaluateAnimation(float elapseSecnod)
        {
            FConstructAnimationCommandTreeContext context = new FConstructAnimationCommandTreeContext();
            UAnimationCommand<ULocalSpaceRuntimePose> cmd = null;
            var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            context.CmdExecuteStack.Execute();
        }
    }

    public class UTestGameplayMacross : UGameplayMacross
    {
        public override void ConstructLogicGraph()
        {

        }
        public async override Task<bool> ConstructAnimGraph(UMeshNode animatedMeshNode)
        {
            var animatablePose = animatedMeshNode?.Mesh?.MaterialMesh?.Mesh?.PartialSkeleton?.CreatePose() as EngineNS.Animation.SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
            var skinMDfQueue = animatedMeshNode.Mesh.MdfQueue as EngineNS.Graphics.Mesh.UMdfSkinMesh;
            mMeshSpaceRuntimePose = URuntimePoseUtility.CreateMeshSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimeMeshSpacePose = mMeshSpaceRuntimePose;

            mGamePlayStateMachine = new UGamePlayStateMachine();
            var idleState = new UGamePlayState(mGamePlayStateMachine);
            idleState.LogicalState = new ULogicalState(mGamePlayStateMachine);
            var idleAnimState = new UAnimationState(mGamePlayStateMachine);
            idleAnimState.Animation = await EngineNS.UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"));
            idleAnimState.Initialize();
            idleAnimState.mExtractPoseFromClipCommand.SetExtractedPose(ref animatablePose);
            idleState.AnimationState = idleAnimState;
            mGamePlayStateMachine.SetDefaultState(idleState);
            return true;
        }
        public override void EvaluateAnimation(float elapseSecnod)
        {
            FConstructAnimationCommandTreeContext context = new FConstructAnimationCommandTreeContext();
            context.Create();
            UAnimationCommand<ULocalSpaceRuntimePose> cmd = null;
            var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            context.CmdExecuteStack.Execute();
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref mMeshSpaceRuntimePose, root.OutPose);
        }
    }
}
