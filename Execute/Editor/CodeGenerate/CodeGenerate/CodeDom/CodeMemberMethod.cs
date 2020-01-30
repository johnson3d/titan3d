using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace CodeGenerateSystem.CodeDom
{
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeMemberMethod : System.CodeDom.CodeMemberMethod
    {
        public bool IsUnsafe = false;
        public bool IsAsync = false;
    }
}
