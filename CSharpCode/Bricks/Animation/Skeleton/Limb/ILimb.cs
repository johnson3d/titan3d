using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public struct IndexInSkeleton
    {
        public static IndexInSkeleton Default { get; } = new IndexInSkeleton(-1);
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

namespace EngineNS.Animation.Skeleton.Limb
{
    public interface ILimbDesc : IO.ISerializer
    {
        [Rtti.Meta]
        VNameString Name { get; set; }
        [Rtti.Meta]
        public uint NameHash { get; set; }
        [Rtti.Meta]
        public VNameString ParentName { get; set; }
        [Rtti.Meta]
        public uint ParentHash { get; set; } 
        [Rtti.Meta]
        public EngineNS.Matrix InitMatrix { get; set; }
    }
    public interface ILimb : IO.ISerializer
    {
        public ILimbDesc Desc { get; }
        [Rtti.Meta]
        public List<ILimb> Children { get; }
        public IndexInSkeleton ParentIndex { get; set; }
        public IndexInSkeleton Index { get; set; }
    }
}
