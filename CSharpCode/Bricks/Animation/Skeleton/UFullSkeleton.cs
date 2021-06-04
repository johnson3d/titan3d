using EngineNS.Animation.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public interface ILimbDesc : IO.ISerializer
    {
        [Rtti.Meta]
        VNameString Name { get; set; }
    }
    public interface ILimb : IO.ISerializer
    {
        public ILimbDesc Desc { get; }
        [Rtti.Meta]
        public List<ILimb> Children { get; }
    }

    public class UFullSkeletonDesc : IO.BaseSerializer, ILimbDesc
    {
        public VNameString Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    }

    public class UFullSkeleton : IO.BaseSerializer, ILimb
    {
        public UFullSkeleton()
        {

        }
        public UFullSkeleton(UFullSkeletonDesc desc)
        {
            mDesc = desc;
        }

        [Rtti.Meta]
        public List<ILimb> Children { get; set; } = new List<ILimb>();
        public Dictionary<uint, IndexInSkeleton> HashDic { get; set; } = new Dictionary<uint, IndexInSkeleton>();
        private UFullSkeletonDesc mDesc = null;
        public ILimbDesc Desc { get => mDesc; }

        public void MergeWith(IPartialSkeleton partialSkeleton)
        {
            if (Children.Count == 0)
            {
                for (int i = 0; i < partialSkeleton.GetBonesNum(); ++i)
                {
                    IndexInSkeleton index = new IndexInSkeleton(i);
                    unsafe
                    {
                        IBone iBone = partialSkeleton.GetBone(&index);
                        UBoneDesc desc = UBoneDesc.Create(iBone.Desc);
                        UBone bone = new UBone(desc);
                        Children.Add(bone);
                    }
                }
            }
            else
            {
                LinkWith(partialSkeleton);
            }
            RefreshHierarchy();
        }

        public void AddLimb(ILimb limb)
        {
            Children.Add(limb);
            RefreshHierarchy();
        }
        private void LinkWith(IPartialSkeleton partialSkeleton)
        {

        }

        private void RefreshHierarchy()
        {

        }
    }


}
