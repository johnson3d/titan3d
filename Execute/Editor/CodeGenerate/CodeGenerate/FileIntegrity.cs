using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CodeGenerateSystem
{
    internal class FileIntegrity
    {
        public static bool IsEnabled = false;
        public static void MarkAsTrusted(SafeFileHandle handle)
        {

        }
        public static bool IsTrusted(SafeFileHandle handle)
        {
            return true;
        }
    }
}
