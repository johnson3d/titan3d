using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{

    public class AnimatableObjectClassDesc : IO.BaseSerializer, IAnimatableClassDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.GetString()); }
            set { TypeStr =VNameString.FromString(value.TypeString); }
        }
        private VNameString mTypeStr;
        [Rtti.Meta]
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        [Rtti.Meta]
        public VNameString Name { get => mName; set => mName = value; }

        [Rtti.Meta]
        public List<IAnimatablePropertyDesc> Properties { get; set; } = new List<IAnimatablePropertyDesc>();

        public AnimatableObjectClassDesc()
        {
            mTypeStr = new VNameString();
            mName = new VNameString();
        }
    }
    public class AnimatableObjectPropertyDesc : IO.BaseSerializer, IAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.GetString()); }
            set { TypeStr = VNameString.FromString(value.TypeString); }
        }
        private VNameString mTypeStr;
        [Rtti.Meta]
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        [Rtti.Meta]
        public VNameString Name { get => mName; set => mName = value; }
        private Guid mCurveId = Guid.Empty;
        [Rtti.Meta]
        public Guid CurveId { get => mCurveId; set => mCurveId = value; }
    }
}
