using EngineNS.Animation.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EngineNS.Animation.Skeleton.Limb;

namespace EngineNS.Animation.Skeleton
{
    public class UFullSkeletonDesc : IO.BaseSerializer, ILimbDesc
    {
        [Rtti.Meta]
        public VNameString Name { get; set; }
        [Rtti.Meta]
        public uint NameHash { get; set; }
        [Rtti.Meta]
        public VNameString ParentName { get; set; }
        [Rtti.Meta]
        public uint ParentHash { get; set; }
        [Rtti.Meta]
        public EngineNS.Matrix InitMatrix { get; set; }
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
        public List<ILimb> Children { get;} = new List<ILimb>();
        private Dictionary<uint, ILimb> HashDic { get; set; } = new Dictionary<uint, ILimb>();
        private UFullSkeletonDesc mDesc = null;
        public ILimbDesc Desc { get => mDesc; }
        public IndexInSkeleton ParentIndex { get; set; } = IndexInSkeleton.Default;
        public IndexInSkeleton Index { get; set; } = IndexInSkeleton.Default;
        public ILimb Root { get; set; }
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
        private void LinkWith(IPartialSkeleton partialSkeleton)
        {

        }
        public void AddLimb(ILimb limb)
        {
            var exist = FindLimb(limb.Desc.NameHash);
            if (exist == null)
            {
                Children.Add(limb);
                RefreshHierarchy();
            }
        }
        public ILimb FindLimb(uint nameHash)
        {
            ILimb limb;
            if(HashDic.TryGetValue(nameHash, out limb))
            {
                return limb;
            }
            return null;
        }


        private void RefreshHierarchy()
        {
            for (int i = 0; i < Children.Count; ++i)
            {
                Children[i].Index = new IndexInSkeleton(i);
                HashDic.Add(Children[i].Desc.NameHash, Children[i]);
            }
            List<ILimb> noParentLimbs = new List<ILimb>();
            for (int i = 0; i < Children.Count; ++i)
            {
                var limb = Children[i];
                if(limb.Desc.ParentName.Index == -1)
                {
                    noParentLimbs.Add(limb);
                }
                else
                {
                    var parent = FindLimb(limb.Desc.ParentHash);
                    if(parent!=null)
                    {
                        limb.ParentIndex = parent.Index;
                        parent.Children.Add(limb);
                    }
                }
            }
            if(noParentLimbs.Count == 1)
            {
                Root = noParentLimbs[0];
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }


}
