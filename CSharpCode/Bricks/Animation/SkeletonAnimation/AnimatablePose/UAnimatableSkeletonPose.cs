using EngineNS.Animation.Curve;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.AnimatablePose
{
    public partial class UAnimatableSkeletonPose : IAnimatableLimbPose
    {
        public UAnimatableSkeletonPose()
        {
        }
        public UAnimatableSkeletonPose(Skeleton.USkinSkeletonDesc desc)
        {
            mDesc = desc;
        }
        private Skeleton.USkinSkeletonDesc mDesc;
        public ILimbDesc Desc => mDesc;

        public FTransform Transtorm { get; set; }
        public string Name { get => mDesc != null ? mDesc.Name : ""; }
        [Animatable.AnimatableProperty]
        public IAnimatableLimbPose Root { get; set; }
        [Animatable.AnimatableProperty]
        public List<IAnimatableLimbPose> Children { get; set; } = new List<IAnimatableLimbPose>();

        public List<IAnimatableLimbPose> LimbPoses = new List<IAnimatableLimbPose>();
        private Dictionary<uint, IAnimatableLimbPose> HashDic { get; set; } = new Dictionary<uint, IAnimatableLimbPose>();
        public void ConstructHierarchy()
        {
            Children.Clear();
            HashDic.Clear();
            for (int i = 0; i < LimbPoses.Count; ++i)
            {
                LimbPoses[i].Children.Clear();
                HashDic.Add(LimbPoses[i].Desc.NameHash, LimbPoses[i]);
            }
            for (int i = 0; i < LimbPoses.Count; ++i)
            {
                var limbPose = LimbPoses[i];
                var mat = limbPose.Desc.InitMatrix;
                FTransform matTT = FTransform.CreateTransform(mat.Translation.AsDVector(), mat.Scale, mat.Rotation);
                var bone = limbPose.Desc as UBoneDesc;
                var invMat = bone.InvInitMatrix;
                var invinvMat = invMat;
                invinvMat.Inverse();
                if (string.IsNullOrEmpty(limbPose.Desc.ParentName))
                {
                    Children.Add(limbPose);
                    Root = limbPose;
                }
                else
                {
                    var parent = FindLimbPose(limbPose.Desc.ParentHash);
                    if (parent != null)
                    {
                        parent.Children.Add(limbPose);
                        var parentMat = parent.Desc.InitMatrix;

                        FTransform parentT = FTransform.CreateTransform(parentMat.Translation.AsDVector(), parentMat.Scale, parentMat.Rotation);
                        parentMat.Inverse();
                        FTransform parentI = FTransform.CreateTransform(parentMat.Translation.AsDVector(), parentMat.Scale, parentMat.Rotation);
                        mat = mat * parentMat;
                        FTransform matT = FTransform.CreateTransform(mat.Translation.AsDVector(), mat.Scale, mat.Rotation);
                    }
                }
                var bonePose = limbPose as UAnimatableBonePose;
                bonePose.Position = NullableVector3.FromVector3(mat.Translation);
                bonePose.Rotation = NullableVector3.FromVector3(mat.Rotation.ToEuler());
                bonePose.Scale = NullableVector3.FromVector3(mat.Scale);
            }
        }
        public IAnimatableLimbPose FindLimbPose(uint nameHash)
        {
            IAnimatableLimbPose limbPose;
            if (HashDic.TryGetValue(nameHash, out limbPose))
            {
                return limbPose;
            }
            return null;
        }

        public IAnimatableLimbPose Clone()
        {
            var pose = new UAnimatableSkeletonPose(mDesc);
            for (int i = 0; i < LimbPoses.Count; ++i)
            {
                pose.LimbPoses.Add(LimbPoses[i].Clone());
            }
            pose.ConstructHierarchy();
            return pose;
        } 
    }
}
