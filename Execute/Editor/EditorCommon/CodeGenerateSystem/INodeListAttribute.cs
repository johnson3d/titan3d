using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.CodeGenerateSystem
{
    public interface INodeListAttribute
    {
        INodeConstructionParams CSParam
        {
            get;
            set;
        }
        Type NodeType
        {
            get;
            set;
        }
        string BindingFile
        {
            get;
            set;
        }
    }
}
