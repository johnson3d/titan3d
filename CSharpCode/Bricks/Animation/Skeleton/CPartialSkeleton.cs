using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public class CPartialSkeleton : AuxPtrType<IPartialSkeleton>
    {
        public CPartialSkeleton()
        {
            mCoreObject = IPartialSkeleton.CreateInstance();
        }
        public CPartialSkeleton(IPartialSkeleton partialSkeleton)
        {
            mCoreObject = IPartialSkeleton.CreateInstance();

        }
        public CBone GetBone(IndexInSkeleton index)
        {
            if (index.ToInt() >= Bones.Count)
                return null;
            return Bones[index.ToInt()];
        }

        public CBone FindBone(VNameString name)
        {
            return FindBoneByNameHash(UniHash.APHash(name.ToString()));
        }
        public CBone FindBoneByNameHash(uint boneNameHashId)
        {
            return FindBone(boneNameHashId);
        }
        public CBone FindBone(uint boneNameHashId)
        {
            CBone bone;
            if (BonesDic.TryGetValue(boneNameHashId, out bone))
            {
                return bone;
            }
            return null;
        }
        public int AddBone(CBone bone)
        {
            if (BonesDic.ContainsKey(bone.Desc.NameHash))
                return -1;

            int index = Bones.Count;
            Bones.Add(bone);
            return index;
        }
        public bool RemoveBone(uint nameHash)
        {
            var bone = FindBone(nameHash);
            if(bone != null)
            {
                Bones.Remove(bone);
                BonesDic.Remove(nameHash);
                mCoreObject.RemoveBone(nameHash);
                return true;
            }
            return false;
        }
        public CBone GetRoot()
        {
            return GetBone(mRoot);
        }
        public bool SetRoot(VNameString name)
        {
            return false;
        }
        public void SetRootByIndex(IndexInSkeleton index)
        {
            mRoot = index;
        }
        public void GenerateHierarchy()
        {

        }

        protected IndexInSkeleton mRoot;
        public List<CBone> Bones { get; } = new List<CBone>();
        public Dictionary<uint, CBone> BonesDic { get; } = new Dictionary<uint, CBone>();


    }
}
