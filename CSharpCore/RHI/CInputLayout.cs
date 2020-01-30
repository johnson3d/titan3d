using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CInputLayoutDesc : AuxCoreObject<CInputLayoutDesc.NativePointer>
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

        public CInputLayoutDesc()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IInputLayoutDesc");
            SetEngineVertexLayout();
        }
        private void SetEngineVertexLayout()
        {
            SDK_IInputLayoutDesc_SetEngineVertexLayout(CoreObject);
        }
        public void AddElement(NativePointer self,
                string SemanticName,
                UInt32 SemanticIndex,
                EPixelFormat Format,
                UInt32 InputSlot,
                UInt32 AlignedByteOffset,
                bool IsInstanceData,
                UInt32 InstanceDataStepRate)
        {
            SDK_IInputLayoutDesc_AddElement(CoreObject, SemanticName, SemanticIndex, Format, InputSlot, AlignedByteOffset, IsInstanceData ? 1 : 0, InstanceDataStepRate);
        }
        public void SetShaderDesc(CShaderDesc shader)
        {
            SDK_IInputLayoutDesc_SetShaderDesc(CoreObject, shader.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IInputLayoutDesc_SetEngineVertexLayout(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IInputLayoutDesc_AddElement(NativePointer self, 
                string SemanticName,
                UInt32 SemanticIndex,
				EPixelFormat Format,
                UInt32 InputSlot,
                UInt32 AlignedByteOffset,
                Int32 IsInstanceData,
                UInt32 InstanceDataStepRate);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IInputLayoutDesc_SetShaderDesc(NativePointer self, CShaderDesc.NativePointer shader);
        #endregion
    }
    public class CInputLayout : AuxCoreObject<CInputLayout.NativePointer>
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

        public CInputLayout(NativePointer self)
        {
            mCoreObject = self;
        }

        #region SDK

        #endregion
    }
}
