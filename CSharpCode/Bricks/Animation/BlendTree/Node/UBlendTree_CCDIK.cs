using EngineNS.Animation.SkeletonAnimation.Runtime.Control;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.BlendTree.Node
{
    public class UBlendTree_CCDIK : UBlendTree<ULocalSpaceRuntimePose>
    {
        public IBlendTree<ULocalSpaceRuntimePose> InPoseNode { get; set; }
        public UCCDIK CCDIK { get; set; } = new UCCDIK();
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
        //public override void InitializePose(ULocalSpaceRuntimePose pose)
        //{
        //    mOutPose = URuntimePoseUtility.CopyPose(pose);
        //    InPoseNode?.InitializePose(pose);
        //    CCDIK.OutPose = OutPose;
        //    CCDIK.InPose = InPoseNode.OutPose;
        //    CCDIK.RootBoneName = RootBoneName;
        //}
        //public override void Evaluate(float elapseSecond)
        //{
        //    if (EvaluateTargetPositionFunc != null)
        //        TargetPosition = EvaluateTargetPositionFunc.Invoke();
        //    if (EvaluateAlphaFunc != null)
        //        Alpha = EvaluateAlphaFunc.Invoke();
        //    if (InPoseNode != null)
        //        InPoseNode.Evaluate(elapseSecond);
        //    CCDIK.Tick(elapseSecond);
        //}
        //public override void Notifying()
        //{
        //    //TODO: Notify
        //    //InPoseNode?.Notifying(component);
        //}
    }
}
