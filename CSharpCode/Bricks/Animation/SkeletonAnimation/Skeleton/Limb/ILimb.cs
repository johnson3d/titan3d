using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Skeleton
{
    public struct IndexInSkeleton
    {
        public static IndexInSkeleton Invalid { get; } = new IndexInSkeleton(-1);
        public IndexInSkeleton(int index)
        {
            mValue = index;
        }
        public int Value { get => mValue; }
        public bool IsValid()
        {
            return mValue != -1 ? true : false;
        }
        private int mValue;

        public static bool operator ==(IndexInSkeleton lhs, IndexInSkeleton rhs)
        {
            return lhs.Value == rhs.Value;
        }

        public static bool operator !=(IndexInSkeleton lhs, IndexInSkeleton rhs) => !(lhs == rhs);
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is IndexInSkeleton == false)
                return false;
            return this == (IndexInSkeleton)obj;
        }
        public override int GetHashCode()
        {
            return mValue;
        }
    }
}

namespace EngineNS.Animation.SkeletonAnimation.Skeleton.Limb
{
    public interface ILimbDesc : IO.ISerializer
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
    public interface ILimb : IO.ISerializer
    {
        public AnimatablePose.IAnimatableLimbPose CreatePose();
        [Rtti.Meta]
        public ILimbDesc Desc { get; set; }
        public List<ILimb> Children { get; set; }
        public Skeleton.IndexInSkeleton ParentIndex { get; set; }
        public Skeleton.IndexInSkeleton Index { get; set; }
    }
}
