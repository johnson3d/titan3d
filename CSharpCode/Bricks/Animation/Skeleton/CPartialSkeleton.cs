using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public class CPartialSkeleton : AuxPtrType<IPartialSkeleton>
    {
        // IPartialSkeleton was created in c++
        public static CPartialSkeleton Create(IPartialSkeleton partialSkeleton)
        {
            var skeleton = new CPartialSkeleton(partialSkeleton);
            unsafe
            {
                for (int i = 0; i < partialSkeleton.GetBonesNum(); ++i)
                {
                    var index = new IndexInSkeleton(i);
                    var bonePtr = partialSkeleton.GetBone(in index);
                    var bone = CBone.Create(new IBone(bonePtr));
                    skeleton.mBones.Add(bone);
                    skeleton.mBonesDic.Add(bone.Desc.NameHash, bone);
                }
            }
            return skeleton;
        }

        protected CPartialSkeleton(IPartialSkeleton partialSkeleton)
        {
            mCoreObject = partialSkeleton;
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
        // we don't want to modify the partial skeleton for now
        //public int AddBone(CBone bone)
        //{
        //    if (BonesDic.ContainsKey(bone.Desc.NameHash))
        //        return -1;
            
        //    int index = Bones.Count;
        //    Bones.Add(bone);
        //    return index;
        //}
        
        //public CBone GetRoot()
        //{
        //    return GetBone(mRoot);
        //}
        //public bool SetRoot(VNameString name)
        //{
        //    mCoreObject.SetRoot(name);
        //    var bone = FindBone(name);
        //    if (bone == null)
        //        return false;
        //    mRoot = bone.Index;
        //    return true;
        //}

        //public void GenerateHierarchy()
        //{
        //    mCoreObject.GenerateHierarchy();
        //}

        protected IndexInSkeleton mRoot;
        protected List<CBone> mBones = new List<CBone>();
        public List<CBone> Bones { get => mBones; }
        protected Dictionary<uint, CBone> mBonesDic = new Dictionary<uint, CBone>();
        public Dictionary<uint, CBone> BonesDic { get => mBonesDic; } 


    }
}
