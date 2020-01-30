using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CShaderSamplers : AuxCoreObject<CShaderSamplers.NativePointer>
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

        public CShaderSamplers()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IShaderSamplers");
        }
        public override void Cleanup()
        {
            base.Cleanup();
            Core_Release(true);
        }
        public void VSBindSampler(UInt32 slot, CSamplerState sampler)
        {
            SDK_IShaderSamplers_VSBindSampler(CoreObject, (byte)slot, sampler.CoreObject);
        }
        public void PSBindSampler(UInt32 slot, CSamplerState sampler)
        {
            SDK_IShaderSamplers_PSBindSampler(CoreObject, (byte)slot, sampler.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderSamplers_VSBindSampler(NativePointer self, byte slot, CSamplerState.NativePointer sampler);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderSamplers_PSBindSampler(NativePointer self, byte slot, CSamplerState.NativePointer sampler);
        #endregion
    }
}
