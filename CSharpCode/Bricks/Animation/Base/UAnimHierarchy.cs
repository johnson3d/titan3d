using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Base
{
    public interface IAnimatableClassDesc : IO.ISerializer
    {
        public Rtti.UTypeDesc ClassType { get; }
        public string TypeStr { get; set; }
        public string Name { get; set; }
        public List<IAnimatablePropertyDesc> Properties { get;}
    }
    public interface IAnimatablePropertyDesc : IO.ISerializer
    {
        public Rtti.UTypeDesc ClassType { get; }
        public string TypeStr { get; set; }
        public string Name { get; set; }
        public Guid CurveId { get; set; }
    }

    //for now UAnimHierarchy  is a full Hierarchy 
    public class UAnimHierarchy : IO.BaseSerializer
    {
        [Rtti.Meta]
        public IAnimatableClassDesc Node { get; set; } = null;
        public UAnimHierarchy Root { get; set; } = null;
        public UAnimHierarchy Parent { get; set; } = null;
        [Rtti.Meta]
        public List<UAnimHierarchy> Children { get; set; } = new List<UAnimHierarchy>();

    }
}
