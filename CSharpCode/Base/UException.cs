using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class TtException : System.Exception
    {
        public TtException(string info, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {

        }
    }
}
