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
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.ToString()); }
        }
        private VNameString mTypeStr;
        [Rtti.Meta]
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        [Rtti.Meta]
        public VNameString Name { get => mName; set => mName = value; }

        [Rtti.Meta]
        public List<IAnimatablePropertyDesc> Properties { get; set; }

        public AnimatableObjectClassDesc()
        {
            unsafe
            {
                mTypeStr = *VNameString.CreateInstance();
                mName = *VNameString.CreateInstance();
            }
        }
    }
    public class AnimatableObjectPropertyDesc : IO.BaseSerializer, IAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr.ToString()); }
        }
        private VNameString mTypeStr;
        [Rtti.Meta]
        public VNameString TypeStr { get => mTypeStr; set => mTypeStr = value; }

        private VNameString mName;
        [Rtti.Meta]
        public VNameString Name { get => mName; set => mName = value; }
        private int mCurveIndex = -1;
        [Rtti.Meta]
        public int CurveIndex { get => mCurveIndex; set => mCurveIndex = value; }
    }
}
