using EngineNS.Animation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Pose
{
    public interface ILimbPose : Animation.Animatable.IAnimatable
    {
        Skeleton.Limb.ILimbDesc Desc {get;}
        
        Transform Transtorm { get; set; }
    }

    public class UBonePose : ILimbPose
    {
        private Skeleton.Limb.UBoneDesc mDesc;
        public ILimbDesc Desc { get => mDesc;}
        
        public Transform Transtorm { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Position { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Quaternion Rotation { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Scale { get; set; }
    }

    public class USocketPose : ILimbPose
    {
        private Skeleton.Limb.USocketDesc mDesc;
        public ILimbDesc Desc { get => mDesc; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Transform Transtorm { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Position { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Quaternion Rotation { get; set; }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Scale { get; set; }
    }
}
