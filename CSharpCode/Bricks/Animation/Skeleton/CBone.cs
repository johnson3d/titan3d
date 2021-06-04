using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{

    public class CBone : AuxPtrType<IBone>
    {
        //ibone was created in c++
        public static CBone Create(IBone iBone)
        {
            CBone bone = new CBone(iBone);
            for(int i = 0; i< iBone.GetChildNum(); ++i)
            {
                bone.mChildren.Add(iBone.GetChild(i));
            }
            return bone;
        }
        public CBone(IBoneDesc boneDesc)
        {
            unsafe
            {
                mCoreObject = IBone.CreateInstance(&boneDesc);
                Desc = boneDesc;
            }
        }
        protected CBone(IBone iBone)
        {
            mCoreObject = iBone;
            unsafe
            {
                Desc = iBone.Desc;
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
        protected List<IndexInSkeleton> mChildren = new List<IndexInSkeleton>();
        public List<IndexInSkeleton> Children { get => mChildren; }
        //std::vector<USHORT>			GrantChildren;
        public IBoneDesc Desc { get; }
    }
}
