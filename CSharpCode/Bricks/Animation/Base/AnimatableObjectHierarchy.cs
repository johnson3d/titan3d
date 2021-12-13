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
            get { return Rtti.UTypeDesc.TypeOf(TypeStr); }
            set { TypeStr =value.TypeString; }
        }
        [Rtti.Meta]
        public string TypeStr { get; set; } = null;
        [Rtti.Meta]
        public string Name { get; set; } = null;

        [Rtti.Meta]
        public List<IAnimatablePropertyDesc> Properties { get; set; } = new List<IAnimatablePropertyDesc>();

        public AnimatableObjectClassDesc()
        {

        }
    }
    public class AnimatableObjectPropertyDesc : IO.BaseSerializer, IAnimatablePropertyDesc
    {
        public Rtti.UTypeDesc ClassType
        {
            get { return Rtti.UTypeDesc.TypeOf(TypeStr); }
            set { TypeStr = value.TypeString; }
        }
        [Rtti.Meta]
        public string TypeStr { get; set; }
        [Rtti.Meta]
        public string Name { get; set; }
        [Rtti.Meta]
        public Guid CurveId { get; set; }
    }
}
