using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EShaderType
    {
        EST_UnknownShader,
        EST_VertexShader,
        EST_PixelShader,
        EST_ComputeShader,
    };
    public class CVertexShader : AuxCoreObject<CVertexShader.NativePointer>
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

        public CVertexShader(NativePointer self)
        {
            mCoreObject = self;
        }

        public CShaderDesc Desc
        {
            get
            {
                var result = new CShaderDesc(SDK_IVertexShader_GetDesc(CoreObject));
                result.Core_AddRef();
                return result;
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderDesc.NativePointer SDK_IVertexShader_GetDesc(NativePointer self);
        #endregion
    }
}
