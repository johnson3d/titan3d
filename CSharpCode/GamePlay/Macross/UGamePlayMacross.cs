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
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Bricks.CodeBuilder.UMacross.AssetExt, MacrossType = typeof(UGameplayMacross))]
            public RName MacrossName
            {
                get
                {
                    if (mMcGamePlay == null)
                        return null;
                    return mMcGamePlay.Name;
                }
                set
                {
                    //if (value == null)
                    //{
                    //    var gameplayMacross = new EngineNS.GamePlay.GamePlayMacross.UTestGameplayMacross();
                    //    mMcGamePlay = Macross.UMacrossGetter<UGameplayMacross>.UnsafeNewInstance(UEngine.Instance.MacrossModule.Version, gameplayMacross, false);
                    //    //var task = gameplayMacross.ConstructAnimGraph(meshNode1);
                    //    return;
                    //}
                    if (mMcGamePlay == null)
                    {
                        mMcGamePlay = Macross.UMacrossGetter<UGameplayMacross>.NewInstance();
                    }
                    mMcGamePlay.Name = value;
                }
            }
            Macross.UMacrossGetter<UGameplayMacross> mMcGamePlay;
            public Macross.UMacrossGetter<UGameplayMacross> McGamePlay
            {
                get
                {
                    return mMcGamePlay;
                }
            }
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
        public override UNode Parent
        {
            set
            {
                base.Parent = value;
                var meshNode = value as UMeshNode;
                if (meshNode != null)
                {
                    var task = GetNodeData<UGamePlayMacrossNodeData>().McGamePlay.Get().ConstructAnimGraph(meshNode);
                }
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UGamePlayMacrossNode), nameof(TickLogic));
        public override void TickLogic(UWorld world, URenderPolicy policy)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var macrossNodeData = GetNodeData<UGamePlayMacrossNodeData>();
                var gameplay = macrossNodeData?.McGamePlay?.Get();
                if (gameplay == null)
                    return;
                gameplay.TickLogic(world.DeltaTimeSecond);
                gameplay.TickAnimation(world.DeltaTimeSecond);
                gameplay.EvaluateAnimation(world.DeltaTimeSecond);

                base.TickLogic(world, policy);
            }   
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);
            var animMesh = parent as UMeshNode;
            var data = GetNodeData<UGamePlayMacrossNodeData>();
            var mcrs = data.McGamePlay;
            var task = mcrs.Get().ConstructAnimGraph(animMesh);
        }
        //public static async System.Threading.Tasks.Task<UGamePlayMacrossNode> AddGamePlayMacrossNodeNode(GamePlay.UWorld world, UNode parent, UNodeData data, EBoundVolumeType bvType, Type placementType)
        //{
        //    var node = new UGamePlayMacrossNode();
        //    await node.InitializeNode(world, data, bvType, placementType);
        //    node.Parent = parent;

        //    return node;
        //}
    }
    [UMacross]
    public class UGameplayMacross
    {
        protected TtGamePlayStateMachine<UGameplayMacross> mGamePlayStateMachine = new TtGamePlayStateMachine<UGameplayMacross>();

        protected UMeshSpaceRuntimePose mMeshSpaceRuntimePose = null;

        public virtual void ConstructLogicGraph()
        {
        }
        public async virtual Task<bool> ConstructAnimGraph(UMeshNode animatedMeshNode)
        {
            //return false;
            var animatablePose = animatedMeshNode?.Mesh?.MaterialMesh?.Mesh?.PartialSkeleton?.CreatePose() as EngineNS.Animation.SkeletonAnimation.AnimatablePose.UAnimatableSkeletonPose;
            var skinMDfQueue = animatedMeshNode.Mesh.MdfQueue as EngineNS.Graphics.Mesh.UMdfSkinMesh;
            mMeshSpaceRuntimePose = URuntimePoseUtility.CreateMeshSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimeMeshSpacePose = mMeshSpaceRuntimePose;

            mGamePlayStateMachine = new TtGamePlayStateMachine<UGameplayMacross>();
            var idleState = new TtGamePlayState<UGameplayMacross>(mGamePlayStateMachine);
            idleState.LogicalState = new TtLogicalState<UGameplayMacross>(mGamePlayStateMachine);
            var idleAnimState = new TtAnimationState<UGameplayMacross>(mGamePlayStateMachine);
            idleAnimState.Animation = await EngineNS.UEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"));
            idleAnimState.Initialize();
            idleAnimState.mExtractPoseFromClipCommand.SetExtractedPose(ref animatablePose);
            idleState.AnimationState = idleAnimState;
            mGamePlayStateMachine.SetDefaultState(idleState);
            return true;
        }

        public virtual void TickLogic(float elapseSecnod)
        {
            mGamePlayStateMachine.Tick(elapseSecnod, this);

        }
        public virtual void TickAnimation(float elapseSecnod)
        {

        }
        public virtual void EvaluateAnimation(float elapseSecnod)
        {
            //FConstructAnimationCommandTreeContext context = new FConstructAnimationCommandTreeContext();
            //UAnimationCommand<ULocalSpaceRuntimePose> cmd = null;
            //var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            //context.CmdExecuteStack.Execute();

            //test code 
            if (mGamePlayStateMachine.CurrentState == null)
                return;
            FConstructAnimationCommandTreeContext context = new FConstructAnimationCommandTreeContext();
            context.Create();
            UAnimationCommand<ULocalSpaceRuntimePose> cmd = null;
            var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            context.CmdExecuteStack.Execute();
            URuntimePoseUtility.ConvetToMeshSpaceRuntimePose(ref mMeshSpaceRuntimePose, root.OutPose);
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

            mGamePlayStateMachine = new TtGamePlayStateMachine<UGameplayMacross>();
            var idleState = new TtGamePlayState<UGameplayMacross>(mGamePlayStateMachine);
            idleState.LogicalState = new TtLogicalState<UGameplayMacross>(mGamePlayStateMachine);
            var idleAnimState = new TtAnimationState<UGameplayMacross>(mGamePlayStateMachine);
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
