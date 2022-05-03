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
        public static int GetShaderVarTypeSize(EShaderVarType type)
        {
            switch (type)
            {
                case EShaderVarType.SVT_Float1:
                    return 4;
                case EShaderVarType.SVT_Float2:
                    return 8;
                case EShaderVarType.SVT_Float3:
                    return 12;
                case EShaderVarType.SVT_Float4:
                    return 16;
                case EShaderVarType.SVT_Int1:
                    return 4;
                case EShaderVarType.SVT_Int2:
                    return 8;
                case EShaderVarType.SVT_Int3:
                    return 12;
                case EShaderVarType.SVT_Int4:
                    return 16;
                case EShaderVarType.SVT_Matrix4x4:
                    return 64;
                case EShaderVarType.SVT_Matrix3x3:
                    return 36;
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

