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
            return Value != -1 ? true : false;
        }
        private int Value;
    }
    public class UBoneDesc : IO.BaseSerializer, ILimbDesc
    {
        public static UBoneDesc Create(IBoneDesc iBoneDesc)
        {
            UBoneDesc desc = new UBoneDesc();
            desc.Name = iBoneDesc.m_Name;
            desc.NameHash = iBoneDesc.m_NameHash;
            desc.ParentName = iBoneDesc.m_ParentName;
            desc.ParentHash= iBoneDesc.m_ParentHash;
            desc.InitMatrix= iBoneDesc.m_InitMatrix;
            desc.InvInitMatrix= iBoneDesc.m_InvInitMatrix;
            desc.InvPos= iBoneDesc.m_InvPos;
            desc.InvQuat= iBoneDesc.m_InvQuat;
            desc.InvScale= iBoneDesc.m_InvScale;
            return desc;
        }
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
        [Rtti.Meta]
        public EngineNS.Matrix InvInitMatrix { get; set; }
        [Rtti.Meta]
        public EngineNS.Vector3 InvPos { get; set; }
        [Rtti.Meta]
        public EngineNS.Vector3 InvScale { get; set; }
        [Rtti.Meta]
        public EngineNS.Quaternion InvQuat { get; set; }
    }
    class UBone : IO.BaseSerializer, ILimb
    {
        public UBone(UBoneDesc boneDesc)
        {
            mDesc = boneDesc;
        }

        ILimb GetChild(int index)
        {
            if (index > Children.Count)
                return null;
            return Children[index];
        }
        void AddChild(ILimb child)
        {
            Children.Add(child);
        }
        private UBoneDesc mDesc = null;
        [Rtti.Meta]
        public ILimbDesc Desc { get => mDesc; }
        public IndexInSkeleton ParentIndex
        {
            get;
            set;
        }
        public IndexInSkeleton Index
        {
            get;
            set;
        }
        protected List<ILimb> mChildren = new List<ILimb>();
        [Rtti.Meta]
        public List<ILimb> Children { get => mChildren; set => mChildren =value; }
        //std::vector<USHORT>			GrantChildren;

        
    }
}
