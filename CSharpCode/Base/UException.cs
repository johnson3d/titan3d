using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class UException : System.Exception
    {
        public UException(string info, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {

        }
    }
}
