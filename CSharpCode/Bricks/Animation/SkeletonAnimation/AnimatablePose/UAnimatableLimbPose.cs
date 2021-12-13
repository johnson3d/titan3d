using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.AnimatablePose
{
    public interface IAnimatableLimbPose : Animation.Animatable.IAnimatable
    {
        ILimbDesc Desc { get; }
        [Animatable.AnimatableProperty]
        List<IAnimatableLimbPose> Children { get; }
        FTransform Transtorm { get;}
    }

    public partial class UAnimatableBonePose : IAnimatableLimbPose
    {
        public UAnimatableBonePose()
        {
        }
        public UAnimatableBonePose(UBoneDesc desc)
        {
            mDesc = desc;
        }
        public string Name { get => mDesc != null? mDesc.Name : ""; }
        private Skeleton.Limb.UBoneDesc mDesc;
        public ILimbDesc Desc { get => mDesc; }
        [Animatable.AnimatableProperty]
        public List<IAnimatableLimbPose> Children { get; set; } = new List<IAnimatableLimbPose>();
        public FTransform Transtorm 
        {
            get
            {
                return FTransform.CreateTransform(Position.AsDVector(), Scale, Quaternion.FromEuler(Rotation));
            }
        }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Position { get; set; } = Vector3.Zero;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Scale { get; set; } = Vector3.One;
    }

    public partial class UAnimatableSocketPose : IAnimatableLimbPose
    {
        public UAnimatableSocketPose()
        {
        }
        public UAnimatableSocketPose(USocketDesc desc)
        {
            mDesc = desc;
        }
        public string Name { get => mDesc != null ? mDesc.Name : ""; }
        private Skeleton.Limb.USocketDesc mDesc;
        public ILimbDesc Desc { get => mDesc; }
        [Animatable.AnimatableProperty]
        public List<IAnimatableLimbPose> Children { get; set; } = new List<IAnimatableLimbPose>();
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public FTransform Transtorm
        {
            get
            {
                return FTransform.CreateTransform(Position.AsDVector(), Scale, Quaternion.FromEuler(Rotation));
            }
        }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Position { get; set; } = Vector3.Zero;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public Vector3 Scale { get; set; } = Vector3.One;
    }
}
