using EngineNS.Animation.Curve;
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

        IAnimatableLimbPose Clone();
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
                return FTransform.CreateTransform(GetPositionVector3().AsDVector(), GetScaleVector3(), Quaternion.FromEuler(GetRotationVector3()));
            }
        }
        private Vector3 GetPositionVector3()
        {
            var descInitTranslation = Desc.InitMatrix.Translation;
            Vector3 result = Vector3.Zero;
            result.X = Position.X.HasValue ? Position.X.Value : descInitTranslation.X;
            result.Y = Position.Y.HasValue ? Position.Y.Value : descInitTranslation.Y;
            result.Z = Position.Z.HasValue ? Position.Z.Value : descInitTranslation.Z;
            return result;
        }
        private Vector3 GetRotationVector3()
        {
            var descInitRotation = Desc.InitMatrix.Rotation.ToEuler();
            Vector3 result = Vector3.Zero;
            result.X = Rotation.X.HasValue ? Rotation.X.Value : descInitRotation.X;
            result.Y = Rotation.Y.HasValue ? Rotation.Y.Value : descInitRotation.Y;
            result.Z = Rotation.Z.HasValue ? Rotation.Z.Value : descInitRotation.Z;
            return result;
        }
        private Vector3 GetScaleVector3()
        {
            var descInitScale = Desc.InitMatrix.Scale;
            Vector3 result = Vector3.Zero;
            result.X = Scale.X.HasValue ? Scale.X.Value : descInitScale.X;
            result.Y = Scale.Y.HasValue ? Scale.Y.Value : descInitScale.Y;
            result.Z = Scale.Z.HasValue ? Scale.Z.Value : descInitScale.Z;
            return result;
        }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Position { get; set; } = NullableVector3.Empty;
        
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Rotation { get; set; } = NullableVector3.Empty;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Scale { get; set; } = NullableVector3.One;

        public IAnimatableLimbPose Clone()
        {
            return new UAnimatableBonePose(mDesc);
        }
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
                return FTransform.CreateTransform(GetPositionVector3().AsDVector(), GetScaleVector3(), Quaternion.FromEuler(GetRotationVector3()));
            }
        }
        private Vector3 GetPositionVector3()
        {
            var descInitTranslation = Desc.InitMatrix.Translation;
            Vector3 result = Vector3.Zero;
            result.X = Position.X.HasValue ? Position.X.Value : descInitTranslation.X;
            result.Y = Position.Y.HasValue ? Position.Y.Value : descInitTranslation.Y;
            result.Z = Position.Z.HasValue ? Position.Z.Value : descInitTranslation.Z;
            return result;
        }
        private Vector3 GetRotationVector3()
        {
            var descInitRotation = Desc.InitMatrix.Rotation.ToEuler();
            Vector3 result = Vector3.Zero;
            result.X = Rotation.X.HasValue ? Rotation.X.Value : descInitRotation.X;
            result.Y = Rotation.Y.HasValue ? Rotation.Y.Value : descInitRotation.Y;
            result.Z = Rotation.Z.HasValue ? Rotation.Z.Value : descInitRotation.Z;
            return result;
        }
        private Vector3 GetScaleVector3()
        {
            var descInitScale = Desc.InitMatrix.Scale;
            Vector3 result = Vector3.Zero;
            result.X = Scale.X.HasValue ? Scale.X.Value : descInitScale.X;
            result.Y = Scale.Y.HasValue ? Scale.Y.Value : descInitScale.Y;
            result.Z = Scale.Z.HasValue ? Scale.Z.Value : descInitScale.Z;
            return result;
        }
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Position { get; set; } = NullableVector3.Empty;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Rotation { get; set; } = NullableVector3.Empty;
        [EngineNS.Animation.Animatable.AnimatableProperty]
        public NullableVector3 Scale { get; set; } = NullableVector3.One;

        public IAnimatableLimbPose Clone()
        {
            return new UAnimatableSocketPose(mDesc);
        }
    }
}
