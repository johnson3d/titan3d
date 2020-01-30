using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EShaderVarType : int
    {
        SVT_Float1,
        SVT_Float2,
        SVT_Float3,
        SVT_Float4,

        SVT_Int1,
        SVT_Int2,
        SVT_Int3,
        SVT_Int4,
        
        SVT_Matrix4x4,
        SVT_Matrix3x3,

        SVT_Texture,
        SVT_Sampler,
        SVT_Struct,
        SVT_Unknown,
    };
    public struct ConstantVarDesc
    {
        public EShaderVarType Type;
        public UInt32 Offset;
        public UInt32 Size;
        public UInt32 Elements;
    };
    public class CConstantBuffer : AuxCoreObject<CConstantBuffer.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CConstantBuffer(NativePointer self)
        {
            mCoreObject = self;
        }

        public string Name
        {
            get
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(SDK_IConstantBuffer_GetName(CoreObject));
            }
        }
        public UInt32 Size
        {
            get
            {
                return SDK_IConstantBuffer_GetSize(CoreObject);
            }
        }

        public int FindVar(string name)
        {
            return SDK_IConstantBuffer_FindVar(CoreObject, name);
        }
        public bool GetVarDesc(int index, ref ConstantVarDesc desc)
        {
            unsafe
            {
                fixed (ConstantVarDesc* p = &desc)
                {
                    return SDK_IConstantBuffer_GetVarDesc(CoreObject, index, p) == 0 ? false : true;
                }
            }
        }
        public unsafe bool SetVarValue(int index, byte* data, int len, UInt32 elementIndex)
        {
            return SDK_IConstantBuffer_SetVarValuePtr(CoreObject, index, data, len, elementIndex) == 0 ? false : true;
        }
        //public unsafe void* GetVarValueAddress(int index, UInt32 elementIndex)
        //{
        //    return SDK_IConstantBuffer_GetVarValueAddress(CoreObject, index, elementIndex);
        //}
        public bool SetValue(int index, int value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(int), elementIndex);
            }
        }
        public bool SetValue(int index, uint value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(uint), elementIndex);
            }
        }

        public bool SetValue(int index, float value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(float), elementIndex);
            }
        }
        public bool SetValue(int index, Vector2 value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Vector2), elementIndex);
            }
        }
        public bool SetValue(int index, Vector3 value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Vector3), elementIndex);
            }
        }
        public bool SetValue(int index, Vector4 value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Vector4), elementIndex);
            }
        }
        public bool SetValue(int index, Plane value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Plane), elementIndex);
            }
        }
        public bool SetValue(int index, Quaternion value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Quaternion), elementIndex);
            }
        }
        public bool SetValue(int index, Color4 value, UInt32 elementIndex)
        {
            unsafe
            {
                return SetVarValue(index, (byte*)&value, sizeof(Color4), elementIndex);
            }
        }
        public bool SetValue(int index, Matrix value, UInt32 elementIndex)
        {
            Matrix ivtMatrix = Matrix.Transpose(ref value);
            unsafe
            {
                return SetVarValue(index, (byte*)&ivtMatrix, sizeof(Matrix), elementIndex);
            }
        }
        public void FlushContent(CCommandList cmd)
        {
            lock (cmd)
            {
                SDK_IConstantBuffer_FlushContent(CoreObject, cmd.CoreObject);
            }
        }
        public bool IsSameVars(CShaderProgram program, UInt32 cbIndex)
        {
            return SDK_IConstantBuffer_IsSameVars(CoreObject, program.CoreObject, cbIndex);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_IConstantBuffer_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IConstantBuffer_GetSize(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IConstantBuffer_IsSameVars(NativePointer self, CShaderProgram.NativePointer program, UInt32 cbIndex);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_IConstantBuffer_FindVar(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_IConstantBuffer_GetVarDesc(NativePointer self, int index, ConstantVarDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_IConstantBuffer_SetVarValuePtr(NativePointer self, int index, byte* data, int len, UInt32 elementIndex);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe void* SDK_IConstantBuffer_GetVarValueAddress(NativePointer self, int index, UInt32 elementIndex);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IConstantBuffer_FlushContent(NativePointer self, CCommandList.NativePointer cmd);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IConstantBuffer_FlushContent2(NativePointer self, CRenderContext.NativePointer cmd);
        #endregion
    }
}
