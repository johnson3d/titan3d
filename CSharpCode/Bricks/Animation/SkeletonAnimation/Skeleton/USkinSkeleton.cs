using EngineNS.Animation.Base;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;

namespace EngineNS.Animation.SkeletonAnimation.Skeleton
{
    public interface ISkeleton
    {

    }
    [Rtti.Meta]
    public class USkinSkeletonDesc : IO.BaseSerializer, ILimbDesc
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

    [Rtti.Meta]
    public class USkinSkeleton : IO.BaseSerializer, ILimb, ISkeleton
    {
        public AnimatablePose.IAnimatableLimbPose CreatePose()
        {
            var pose = new AnimatablePose.UAnimatableSkeletonPose();
            foreach (var limb in Limbs)
            {
                pose.LimbPoses.Add(limb.CreatePose());
            }
            pose.ConstructHierarchy();
            return pose;
        }
        public USkinSkeleton()
        {

        }
        public USkinSkeleton(USkinSkeletonDesc desc)
        {
            Desc = desc;
        }
        [Rtti.Meta]
        public List<ILimb> Limbs { get; set; } = new List<ILimb>(); //all Limbs, like bone, sockets or others
        public List<ILimb> Children { get; set; } = new List<ILimb>(); //Roots
        private Dictionary<uint, ILimb> HashDic { get; set; } = new Dictionary<uint, ILimb>();
        public ILimbDesc Desc { get; set; } = null;
        public IndexInSkeleton ParentIndex { get; set; } = IndexInSkeleton.Invalid;
        public IndexInSkeleton Index { get; set; } = IndexInSkeleton.Invalid;
        public ILimb Root { get; set; } //Named "Root" bone in children

        public void AddLimb(ILimb limb)
        {
            var exist = FindLimb(limb.Desc.NameHash);
            if (exist == null)
            {
                Limbs.Add(limb);
                //RefreshHierarchy();
            }
        }
        public ILimb FindLimb(uint nameHash)
        {
            ILimb limb;
            if(HashDic.Count == 0)
            {
                ConstructHierarchy();
            }
            if (HashDic.TryGetValue(nameHash, out limb))
            {
                return limb;
            }
            return null;
        }
        public ILimb FindLimb(string name)
        {
            var nameHash = Standart.Hash.xxHash.xxHash32.ComputeHash(name);
            return FindLimb(nameHash);
        }
        public List<T> GetLimb<T>() where T : ILimb
        {
            List<T> temp = new List<T>();
            for (int i = 0; i < Limbs.Count; ++i)
            {
                if (Limbs[i].GetType() == typeof(T))
                {
                    temp.Add((T)Limbs[i]);
                }
            }
            return temp;
        }
        public void ConstructHierarchy()
        {
            Children.Clear();
            HashDic.Clear();
            for (int i = 0; i < Limbs.Count; ++i)
            {
                Limbs[i].Index = new IndexInSkeleton(i);
                Limbs[i].Children.Clear();
                HashDic.Add(Limbs[i].Desc.NameHash, Limbs[i]);
            }
            for (int i = 0; i < Limbs.Count; ++i)
            {
                var limb = Limbs[i];
                if (string.IsNullOrEmpty(limb.Desc.ParentName))
                {
                    Children.Add(limb);
                }
                else
                {
                    var parent = FindLimb(limb.Desc.ParentHash);
                    if (parent != null)
                    {
                        limb.ParentIndex = parent.Index;
                        parent.Children.Add(limb);
                    }
                }
            }
            foreach (var limb in Children)
            {
                if (limb.Desc.Name.ToLower() == "root")
                {
                    Root = limb;
                    break;
                }
            }
        }

        public override void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
            if (prop.Name == "Limbs")
            {
                ConstructHierarchy();
            }
            base.OnPropertyRead(tagObject, prop, fromXml);
        }
    }


}
