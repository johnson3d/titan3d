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
        public unsafe static bool IsNullPointer<T>(T* ptr) where T : unmanaged
        {
            return ptr == (T*)0;
        }
        public static int GetShaderVarTypeSize(NxRHI.EShaderVarType type)
        {
            switch (type)
            {
                case NxRHI.EShaderVarType.SVT_Float:
                    return 4;
                case NxRHI.EShaderVarType.SVT_Int:
                    return 4;
                default:
                    return -1;
            }
        }
    }
    public unsafe partial struct BigStackBuffer : IDisposable
    {
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
    public static VNameString FromString(string name)
    {
        var result = new VNameString();
        result.Index = VNameString.GetIndexFromString(name);
        return result;
    }
    public string Text
    {
        get
        {
            return c_str();
        }
    }
}

