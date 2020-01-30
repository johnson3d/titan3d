using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerateSystem.Base
{
    public class AnimMacrossConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; }
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as AnimMacrossConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
}
