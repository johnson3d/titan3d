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
using EngineNS.Bricks.StateMachine.TimedSM;

namespace EngineNS.GamePlay.GamePlayMacross
{
    public class UGamePlayMacrossNode : GamePlay.Scene.TtLightWeightNodeBase
    {
        public class UGamePlayMacrossNodeData : TtNodeData
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
                    //    mMcGamePlay = Macross.UMacrossGetter<UGameplayMacross>.UnsafeNewInstance(TtEngine.Instance.MacrossModule.Version, gameplayMacross, false);
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
            public TtMeshNode AnimatedMeshNode = null;
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            SetStyle(ENodeStyles.Invisible);
            if (!await base.InitializeNode(world, data, bvType, placementType))
            {
                return false;
            }
            return true;
        }
        public override TtNode Parent
        {
            set
            {
                base.Parent = value;
                var meshNode = value as TtMeshNode;
                if (meshNode != null)
                {
                    var task = GetNodeData<UGamePlayMacrossNodeData>().McGamePlay.Get().ConstructAnimGraph(meshNode);
                }
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UGamePlayMacrossNode), nameof(TickLogic));
        public override void TickLogic(TtNodeTickParameters args)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var macrossNodeData = GetNodeData<UGamePlayMacrossNodeData>();
                var gameplay = macrossNodeData?.McGamePlay?.Get();
                if (gameplay == null)
                    return;
                gameplay.TickLogic(args.World.DeltaTimeSecond);
                gameplay.TickAnimation(args.World.DeltaTimeSecond);
                gameplay.EvaluateAnimation(args.World.DeltaTimeSecond);

                base.TickLogic(args);
            }   
        }
        public override void OnNodeLoaded(TtNode parent)
        {
            base.OnNodeLoaded(parent);
            var animMesh = parent as TtMeshNode;
            var data = GetNodeData<UGamePlayMacrossNodeData>();
            var mcrs = data.McGamePlay;
            var task = mcrs.Get()?.ConstructAnimGraph(animMesh);
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

        protected TtLocalSpaceRuntimePose mRuntimePose = null;

        public virtual void ConstructLogicGraph()
        {
        }
        public async virtual Task<bool> ConstructAnimGraph(TtMeshNode animatedMeshNode)
        {
            //return false;
            var animatablePose = animatedMeshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = animatedMeshNode.Mesh.MdfQueue as EngineNS.Graphics.Mesh.UMdfSkinMesh;
            mRuntimePose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimePose = mRuntimePose;

            mGamePlayStateMachine = new TtGamePlayStateMachine<UGameplayMacross>();
            var idleState = new TtGamePlayState<UGameplayMacross>(mGamePlayStateMachine);
            idleState.LogicalState = new TtLogicalState<UGameplayMacross>(mGamePlayStateMachine);
            var idleAnimState = new TtAnimationState<UGameplayMacross>(mGamePlayStateMachine);
            idleAnimState.Animation = await EngineNS.TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"));
            //idleAnimState.Initialize();
            idleAnimState.mExtractPoseFromClipCommand?.SetExtractedPose(ref animatablePose);
            idleState.AnimationState = idleAnimState;
            mGamePlayStateMachine.SetDefaultState(idleState);
            return true;
        }

        public virtual void TickLogic(float elapseSecnod)
        {
            TtStateMachineContext context = new TtStateMachineContext();
            mGamePlayStateMachine.Tick(elapseSecnod, context);

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
            TtAnimationCommand<TtLocalSpaceRuntimePose> cmd = null;
            var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            if (root == null)
                return;
            context.CmdExecuteStack.Execute();
            TtRuntimePoseUtility.CopyPose(ref mRuntimePose, root.OutPose);
        }
    }

    public class UTestGameplayMacross : UGameplayMacross
    {
        public override void ConstructLogicGraph()
        {

        }
        public async override Task<bool> ConstructAnimGraph(TtMeshNode animatedMeshNode)
        {
            var animatablePose = animatedMeshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
            var skinMDfQueue = animatedMeshNode.Mesh.MdfQueue as EngineNS.Graphics.Mesh.UMdfSkinMesh;
            mRuntimePose = TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
            skinMDfQueue.SkinModifier.RuntimePose = mRuntimePose;

            mGamePlayStateMachine = new TtGamePlayStateMachine<UGameplayMacross>();
            var idleState = new TtGamePlayState<UGameplayMacross>(mGamePlayStateMachine);
            idleState.LogicalState = new TtLogicalState<UGameplayMacross>(mGamePlayStateMachine);
            var idleAnimState = new TtAnimationState<UGameplayMacross>(mGamePlayStateMachine);
            idleAnimState.Animation = await EngineNS.TtEngine.Instance.AnimationModule.AnimationClipManager.GetAnimationClip(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"));
            //idleAnimState.Initialize();
            idleAnimState.mExtractPoseFromClipCommand.SetExtractedPose(ref animatablePose);
            idleState.AnimationState = idleAnimState;
            mGamePlayStateMachine.SetDefaultState(idleState);
            return true;
        }
        public override void EvaluateAnimation(float elapseSecnod)
        {
            FConstructAnimationCommandTreeContext context = new FConstructAnimationCommandTreeContext();
            context.Create();
            TtAnimationCommand<TtLocalSpaceRuntimePose> cmd = null;
            var root = mGamePlayStateMachine.ConstructAnimationCommandTree(cmd, ref context);
            context.CmdExecuteStack.Execute();
            TtRuntimePoseUtility.CopyPose(ref mRuntimePose, root.OutPose);
        }
    }
}
