using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Skeleton.Limb
{
    public class TtSocketDesc : IO.BaseSerializer, ILimbDesc
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
    public class TtSocket : IO.BaseSerializer, ILimb
    {
        public TtSocket()
        {

        }
        public TtSocket(TtSocketDesc desc)
        {
            Desc = desc;
        }

        public List<ILimb> Children { get; set; } = new List<ILimb>();
        private TtSocketDesc mDesc = null;
        [Rtti.Meta]
        public ILimbDesc Desc { get => mDesc; set => mDesc = value as TtSocketDesc; }
        public IndexInSkeleton ParentIndex { get; set; } = IndexInSkeleton.Invalid;
        public IndexInSkeleton Index { get; set; } = IndexInSkeleton.Invalid;

        public AnimatablePose.IAnimatableLimbPose CreatePose()
        {
            return new AnimatablePose.TtAnimatableSocketPose(mDesc);
        }
    }
}
