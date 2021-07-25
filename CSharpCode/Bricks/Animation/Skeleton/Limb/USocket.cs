using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton.Limb
{
    public class USocketDesc : IO.BaseSerializer, ILimbDesc
    {
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
    public class USocket : IO.BaseSerializer, ILimb
    {
        public USocket()
        {

        }
        public USocket(USocketDesc desc)
        {
            mDesc = desc;
        }

        public List<ILimb> Children { get; set; } = new List<ILimb>();
        private USocketDesc mDesc = null;
        public ILimbDesc Desc => mDesc;
        public IndexInSkeleton ParentIndex { get; set; } = IndexInSkeleton.Default;
        public IndexInSkeleton Index { get; set; } = IndexInSkeleton.Default;
    }
}
