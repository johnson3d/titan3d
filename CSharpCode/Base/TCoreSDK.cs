using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct CoreSDK
    {
        public const string CoreModule = "Core.Window.dll";

        public static void CopyString2Ansi(BigStackBuffer buffer, string srcStr)
        {
            unsafe
            {
                var p = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(srcStr);
                SDK_StrCpy(buffer.GetBuffer(), p.ToPointer(), (uint)buffer.GetSize());
                System.Runtime.InteropServices.Marshal.FreeHGlobal(p);
            }
        }
        public static void IUnknown_Release(IntPtr unk)
        {
            IUnknown_Release(unk.ToPointer());
        }
    }
    public unsafe partial struct BigStackBuffer : IDisposable
    {
        public void Dispose()
        {
            DestroyMe();
        }
        public string AsText()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)GetBuffer());
        }
        public void SetText(string txt)
        {
            CoreSDK.CopyString2Ansi(this, txt);
        }
    }
}

public unsafe partial struct VNameString
{
    public string Text
    {
        get
        {
            return GetString();
        }
    }
}

