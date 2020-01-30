using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public struct CViewPortDesc
    {
        public float TopLeftX;
        public float TopLeftY;
        public float Width;
        public float Height;
        public float MinDepth;
        public float MaxDepth;
    }
    public class CViewport : AuxCoreObject<CViewport.NativePointer>
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

        public CViewport()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IViewPort");
        }
        
        public float TopLeftX
        {
            get
            {
                return SDK_IViewPort_GetTopLeftX(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetTopLeftX(CoreObject, value);
            }
        }
        public float TopLeftY
        {
            get
            {
                return SDK_IViewPort_GetTopLeftY(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetTopLeftY(CoreObject, value);
            }
        }
        public float Width
        {
            get
            {
                return SDK_IViewPort_GetWidth(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetWidth(CoreObject, value);
            }
        }
        public float Height
        {
            get
            {
                return SDK_IViewPort_GetHeight(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetHeight(CoreObject, value);
            }
        }
        public float MinDepth
        {
            get
            {
                return SDK_IViewPort_GetMinDepth(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetMinDepth(CoreObject, value);
            }
        }
        public float MaxDepth
        {
            get
            {
                return SDK_IViewPort_GetMaxDepth(CoreObject);
            }
            set
            {
                SDK_IViewPort_SetMaxDepth(CoreObject, value);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetTopLeftX(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetTopLeftX(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetTopLeftY(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetTopLeftY(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetWidth(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetWidth(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetHeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetHeight(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetMinDepth(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetMinDepth(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IViewPort_GetMaxDepth(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IViewPort_SetMaxDepth(NativePointer self, float value);
        #endregion
    }
}
