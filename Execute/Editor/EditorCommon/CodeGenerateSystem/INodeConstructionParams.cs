using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.CodeGenerateSystem
{
    public interface INodeConstructionParams
    {
        string ConstructParam
        {
            get;
            set;
        }
        EngineNS.ECSType CSType { get; set; }
        INodeConstructionParams Duplicate();
    }
}
