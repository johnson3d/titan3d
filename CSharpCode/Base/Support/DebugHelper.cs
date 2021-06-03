using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public class DebugHelper
    {
        public static string TraceMessage(string message = "error code",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            return $"{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}";
        }
    }
}
