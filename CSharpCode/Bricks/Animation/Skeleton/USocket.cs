using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Skeleton
{
    public partial class USocketDesc : IO.BaseSerializer, ILimbDesc
    {
        public VNameString Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
    public partial class USocket : IO.BaseSerializer, ILimb
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
    }
}
