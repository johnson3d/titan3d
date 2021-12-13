using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.SkeletonAnimation.Skeleton
{
    public struct IndexInSkeleton
    {
        public static IndexInSkeleton Invalid { get; } = new IndexInSkeleton(-1);
        public IndexInSkeleton(int index)
        {
            Value = index;
        }
        public int ToInt()
        {
            return Value; 
        }
        public bool IsValid()
        {
            return Value != -1 ? true : false;
        }
        private int Value;
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
