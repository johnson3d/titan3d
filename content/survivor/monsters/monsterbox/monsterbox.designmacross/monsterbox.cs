namespace NS_survivor.monsters.monsterbox
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/monsterbox/monsterbox.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_PoseOutput_2259889002 : EngineNS.Animation.BlendTree.Node.TtLocalSpaceBlendTree_PoseOutput<monsterbox>
    {
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/monsterbox/monsterbox.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.monsterbox
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/monsterbox/monsterbox.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class DMC_FinalBlendTree_0_971810588 : EngineNS.Animation.BlendTree.TtLocalSpacePoseFinalBlendTree<monsterbox>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("PoseOutput")]
        public DMC_PoseOutput_2259889002 PoseOutput { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_561274663 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/monsterbox/monsterbox.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize(EngineNS.Animation.BlendTree.FAnimBlendTreeContext context)
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_561274663))
            {
                System.Boolean returnedValue = default(System.Boolean);
                mFrame_Initialize_561274663.SetWatchVariable("context", context);
                await base.Initialize(context);
                PoseOutput = new DMC_PoseOutput_2259889002();
                PoseOutput.CenterData = CenterData;
                await PoseOutput.Initialize(context);
                FromNode = PoseOutput;
                returnedValue = true;
                return returnedValue;
            }
        }
    }
}
namespace NS_survivor.monsters.monsterbox
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/monsters/monsterbox/monsterbox.designmacross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class monsterbox : EngineNS.DesignMacross.TtDesignMacrossBase
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("FinalBlendTree_0")]
        public DMC_FinalBlendTree_0_971810588 FinalBlendTree_0 { get; set; }
        EngineNS.Macross.TtMacrossStackFrame mFrame_Initialize_3062514605 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/monsterbox/monsterbox.designmacross", EngineNS.RName.ERNameType.Game));
        public override async EngineNS.Thread.Async.TtTask<System.Boolean> Initialize()
        {
            using(var guard_Initialize = new EngineNS.Macross.TtMacrossStackGuard(mFrame_Initialize_3062514605))
            {
                System.Boolean returnedValue = default(System.Boolean);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                EngineNS.Animation.SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose animatableSkeletonPoseFinalBlendTree_0 = null;
                animatableSkeletonPoseFinalBlendTree_0 = EngineNS.Animation.TtAnimUtil.CreateAnimatableSkeletonPoseFromeNode(MacrossNode);
                blendTreeContextFinalBlendTree_0.AnimatableSkeletonPose = animatableSkeletonPoseFinalBlendTree_0;
                FinalBlendTree_0 = new DMC_FinalBlendTree_0_971810588();
                FinalBlendTree_0.CenterData = this;
                await FinalBlendTree_0.Initialize(blendTreeContextFinalBlendTree_0);
                EngineNS.Animation.SkeletonAnimation.Runtime.Pose.TtLocalSpaceRuntimePose runtimePose = null;
                runtimePose = EngineNS.Animation.TtAnimUtil.BindRuntimeSkeletonPoseToNode(MacrossNode);
                FinalBlendTree_0.SetPose(runtimePose);
                await base.Initialize();
                return returnedValue;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_AfterTick_4051512107 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/monsters/monsterbox/monsterbox.designmacross", EngineNS.RName.ERNameType.Game));
        public override void AfterTick(System.Single elapseSecond)
        {
            using(var guard_AfterTick = new EngineNS.Macross.TtMacrossStackGuard(mFrame_AfterTick_4051512107))
            {
                mFrame_AfterTick_4051512107.SetWatchVariable("elapseSecond", elapseSecond);
                EngineNS.Animation.BlendTree.FAnimBlendTreeContext blendTreeContextFinalBlendTree_0 = new EngineNS.Animation.BlendTree.FAnimBlendTreeContext();
                FinalBlendTree_0.Tick(elapseSecond,ref blendTreeContextFinalBlendTree_0);
            }
        }
    }
}
