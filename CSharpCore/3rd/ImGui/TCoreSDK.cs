using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct CoreSDK
    {
        public static void CopyString2Ansi(BigStackBuffer_PtrType buffer, string srcStr)
        {
            unsafe
            {
                var p = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(srcStr);
                StrCpy(buffer.GetBuffer(), p.ToPointer());
                System.Runtime.InteropServices.Marshal.FreeHGlobal(p);
            }
        }
    }
    public unsafe partial struct BigStackBuffer_PtrType
    {
        public string AsText()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)GetBuffer());
        }
    }
}
