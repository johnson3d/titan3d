
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.MotionMatching;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [Obsolete("Deprecated")]
    public class GLogicMacrossComponent : GComponentMacross
    {
        [Rtti.MetaClass]
        public class GLogicMacrossComponentInitializer : GComponentInitializer
        {

        }
        bool mIsDebug = true;
        MotionField motionField = new MotionField();
        CGfxSkeletonPose pose = null;
        public override void OnBeginPlay(GPlacementComponent placement)
        {
            var mesh = Host.GetComponent<GMeshComponent>();
            var skinModifier = mesh.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            var SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
            pose = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext, SkeletonAssetName).CreateSkeletonPose();
            //var Goodrun = CreateAnim(RName.GetRName("TitanDemo/Character/test_girl/animation/goodrun.vanims"), pose);
            //motionField.Add(Goodrun);
            //var walk2jogneutral = CreateAnim(RName.GetRName("TitanDemo/Character/test_girl/animation/ntsc walk to jog neutral.vanims"), pose);
            //motionField.Add(walk2jogneutral);
            var animationFilePath = RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation");
            //var files = CEngine.Instance.FileManager.GetFiles(animationFilePath.Address);
            //for (int i = 0; i < files.Length; ++i)
            //{

            //    if (CEngine.Instance.FileManager.GetFileExtension(files[i], true) == CEngineDesc.AnimationSequenceExtension)
            //    {
            //        var anim = CreateAnim(RName.EditorOnly_GetRNameFromAbsFile(files[i]), pose);
            //        motionField.Add(anim);
            //    }
            //}
            //var walktorun = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/ntsc_walk_to_run.anim"), pose);
            //motionField.Add(walktorun);
            //var walktorun2 = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/ntsc_walk_to_run2.anim"), pose);
            //motionField.Add(walktorun2);
            var RunLoop = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runloop.anim"), pose);
            motionField.Add(RunLoop);
            //var RunLoopC = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runloopc.anim"), pose);
            //motionField.Add(RunLoopC);
            var RunStart = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runstart.anim"), pose);
            motionField.Add(RunStart);
            var RunStop = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runstop.anim"), pose);
            motionField.Add(RunStop);
            var RunTurnL = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runturnl.anim"), pose);
            motionField.Add(RunTurnL);
            var RunTurnR = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runturnr.anim"), pose);
            motionField.Add(RunTurnR);
            //var Runx = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/runx.anim"), pose);
            //motionField.Add(Runx);
            var ntsc_ambeint2 = CreateAnim(RName.GetRName("TitanDemo/Character/puppet_low_ue4/animation/ntsc_ambeint2.anim"), pose);
            motionField.Add(ntsc_ambeint2);
            skinModifier.AnimationPose = pose;

            if (!mIsDebug)
                return;
            if (Host.Scene.World == null)
                return;
            Action action = async () =>
            {
                await InitDebug();
            };
            action.Invoke();
        }

        int currentAnimaHash = 0;
        MotionPose currentBestMotionPose = null;
        long currentAnimTime = 0;
        CGfxSkeletonPose curPose = null;
        MatchingGoal mGoal = new MatchingGoal();
        public override void Update(GPlacementComponent placement)
        {
            var mesh = Host.GetComponent<GMeshComponent>();
            var skinModifier = mesh.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            if (curPose == null)
                curPose = CEngine.Instance.SkeletonAssetManager.GetSkeleton(CEngine.Instance.RenderContext, RName.GetRName(skinModifier.SkeletonAsset)).CreateSkeletonPose();
            var movement = Host.GetComponent<GMovementComponent>();
            mGoal.HostPlacement = Host.Placement;
            mGoal.Trajectory = movement.FutureTrajectory;
            mGoal.CurrentPose = curPose;
            var bestMotionPose = MotionMatchingSystem.Instance.GetBestMatchingPose(motionField, mGoal);
            if (bestMotionPose != null)
            {
                if (currentBestMotionPose != null && currentBestMotionPose.HashName == bestMotionPose.HashName)
                {
                    if (currentBestMotionPose.PlayTime - bestMotionPose.PlayTime < 5)
                    {
                        currentAnimTime += CEngine.Instance.EngineElapseTime;
                        var time = currentAnimTime % (motionField.Motions[currentAnimaHash].DurationInMilliSecond + 1);
                        curPose = motionField.Motions[currentAnimaHash].GetAnimationSkeletonPose(time * 0.001f);
                    }
                    else
                    {
                        currentBestMotionPose = bestMotionPose;
                        currentAnimaHash = bestMotionPose.HashName;
                        currentAnimTime = bestMotionPose.PlayTime;
                    }
                }
                else
                {
                    currentBestMotionPose = bestMotionPose;
                    currentAnimaHash = bestMotionPose.HashName;
                    currentAnimTime = bestMotionPose.PlayTime;
                }
            }
            //curPose.ExtractRootMotionPosition(true);
            //curPose.BlendWithTargetPose(skinModifier.AnimationPose, 0.5f);
            skinModifier.AnimationPose = curPose;
            if (!mIsDebug)
                return;
            DebugTick(placement);
        }
        public AnimationClip CreateAnim(RName name, CGfxSkeletonPose pose)
        {
            var seq = AnimationClip.CreateSync(name);
            seq.Bind(pose);
            return seq;
        }

        #region debug
        TrajectoryDebug mPoseTrajectoryDebug = new TrajectoryDebug();
        bool mIsInitDebug = false;
        protected async System.Threading.Tasks.Task InitDebug()
        {
            await mPoseTrajectoryDebug.Init(RName.GetRName("TitanDemo/yellowtest.instmtl"), RName.GetRName("TitanDemo/Character/test_girl/material/yellowwireframe.instmtl"));
            mPoseTrajectoryDebug.Add2World(Host.Scene.World);
            mIsInitDebug = true;
        }
        void DebugTick(GPlacementComponent placement)
        {

            if (!mIsInitDebug)
                return;
            if (currentBestMotionPose == null)
                return;
            var debugTrajectory = new Support.NativeQueueForArray<Vector3>();

            var hostLoc = Host.Placement.Location;

            foreach (var point in currentBestMotionPose.Trajectory)
            {
                var p = point.Position;
                p.Y = 0;
                debugTrajectory.Enqueue(Host.Placement.Rotation * point.Position + hostLoc + Vector3.UnitY * 0.1f);
            }
            mPoseTrajectoryDebug.SetVector3Points(debugTrajectory);
            mPoseTrajectoryDebug.Tick(placement);
        }
    }
    #endregion
}

