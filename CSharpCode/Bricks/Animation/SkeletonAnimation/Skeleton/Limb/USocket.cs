using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Skeleton.Limb
{
    public class USocketDesc : IO.BaseSerializer, ILimbDesc
    {
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public uint NameHash { get; set; }
        [Rtti.Meta]
        public string ParentName { get; set; }
        [Rtti.Meta]
        public uint ParentHash { get; set; }
        [Rtti.Meta]
        public EngineNS.Matrix InitMatrix { get; set; }
    }
    public class USocket : IO.BaseSerializer, ILimb
    {
        public USocket()
        {

        }
        public USocket(USocketDesc desc)
        {
            Desc = desc;
        }

        public List<ILimb> Children { get; set; } = new List<ILimb>();
        private USocketDesc mDesc = null;
        [Rtti.Meta]
        public ILimbDesc Desc { get => mDesc; set => mDesc = value as USocketDesc; }
        public IndexInSkeleton ParentIndex { get; set; } = IndexInSkeleton.Invalid;
        public IndexInSkeleton Index { get; set; } = IndexInSkeleton.Invalid;

        public AnimatablePose.IAnimatableLimbPose CreatePose()
        {
            return new AnimatablePose.UAnimatableSocketPose(mDesc);
        }
    }
}
