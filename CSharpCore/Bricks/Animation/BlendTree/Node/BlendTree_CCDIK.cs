using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_CCDIK : IBlendTree
    {
        protected Pose.CGfxSkeletonPose mOutPose = null;
        public Pose.CGfxSkeletonPose OutPose
        {
            get => mOutPose;
            set
            {
                mOutPose = value;
            }
        }
        public IBlendTree InPoseNode { get; set; }
        public Animation.PoseControl.IK.CCDIK CCDIK { get; set; } = new PoseControl.IK.CCDIK();
        public float Alpha
        {
            get => CCDIK.Alpha;
            set => CCDIK.Alpha = value;
        }
        public uint Iteration
        {
            get => CCDIK.Iteration;
            set => CCDIK.Iteration = value;
        }
        public float LimitAngle
        {
            get => CCDIK.LimitAngle;
            set => CCDIK.LimitAngle = value;
        }
        public string RootBoneName
        {
            get => CCDIK.RootBoneName;
            set
            {
                CCDIK.RootBoneName = value;
            }
        }
        public string EndEffecterBoneName
        {
            get => CCDIK.EndEffecterBoneName;
            set => CCDIK.EndEffecterBoneName = value;
        }
        public string TargetBoneName
        {
            get => CCDIK.TargetBoneName;
            set
            {
                CCDIK.TargetBoneName = value;
            }
        }
        public Func<Vector3> EvaluateTargetPositionFunc { get; set; } = null;
        public Func<float> EvaluateAlphaFunc { get; set; } = null;
        public Vector3 TargetPosition
        {
            get => CCDIK.TargetPosition;
            set => CCDIK.TargetPosition = value;
        }
        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
            InPoseNode?.InitializePose(pose);
            CCDIK.OutPose = OutPose;
            CCDIK.InPose = InPoseNode.OutPose;
            CCDIK.RootBoneName = RootBoneName;
        }
        public void Evaluate(float timeInSecond)
        {
            if (EvaluateTargetPositionFunc != null)
                TargetPosition = EvaluateTargetPositionFunc.Invoke();
            if (EvaluateAlphaFunc != null)
                Alpha = EvaluateAlphaFunc.Invoke();
            if (InPoseNode != null)
                InPoseNode.Evaluate(timeInSecond);
            CCDIK.Tick();
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            InPoseNode?.Notifying(component);
        }
    }
}
