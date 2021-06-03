using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public struct IndexInSkeleton
    {
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
            return Value != -1 ? true : false ;
        }
        private int Value;
    }
    public class CBone : AuxPtrType<IBone>, ILimb
    {
        public CBone(IBoneDesc boneDesc)
        {
            unsafe
            {
                mCoreObject = IBone.CreateInstance(&boneDesc);
                Desc = boneDesc;
            }
        }
        IndexInSkeleton GetChild(int index)
        {
            if (index > Children.Count)
                return new IndexInSkeleton();
            return Children[index];
        }
        void AddChild(IndexInSkeleton childIndex)
        {
            Children.Add(childIndex);
            mCoreObject.AddChild(childIndex);
        }
        public IndexInSkeleton ParentIndex
        {
            get => mCoreObject.ParentIndex;
            set => mCoreObject.ParentIndex = value;
        }
        public IndexInSkeleton Index
        {
            get => mCoreObject.Index;
            set => mCoreObject.Index = value;
        }
        public List<IndexInSkeleton> Children { get; } = new List<IndexInSkeleton>();
        //std::vector<USHORT>			GrantChildren;
        public IBoneDesc Desc { get; }
    }
}
