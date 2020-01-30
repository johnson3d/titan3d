using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CScissorRect : AuxCoreObject<CScissorRect.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public readonly static NativePointer NullPointer = new NativePointer();
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
        public struct SRRect
        {
            public int MinX;
            public int MinY;
            public int MaxX;
            public int MaxY;
        };
        public CScissorRect()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("IScissorRect");
        }
        public UInt32 RectNumber
        {
            get
            {
                return SDK_IScissorRect_GetRectNumber(CoreObject);
            }
            set
            {
                SDK_IScissorRect_SetRectNumber(CoreObject, value);
            }
        }
        public void SetSCRect(UInt32 idx, int left, int top, int right, int bottom)
        {
            SDK_IScissorRect_SetSCRect(CoreObject, idx, left, top, right, bottom);
        }
        public void GetSCRect(UInt32 idx, ref SRRect rc)
        {
            unsafe
            {
                fixed (SRRect* pRC = &rc)
                {
                    SDK_IScissorRect_GetSCRect(CoreObject, idx, pRC);
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IScissorRect_SetRectNumber(NativePointer self, UInt32 num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IScissorRect_GetRectNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IScissorRect_SetSCRect(NativePointer self, UInt32 idx, int left, int top, int right, int bottom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static UInt32 SDK_IScissorRect_GetSCRect(NativePointer self, UInt32 idx, SRRect* pRect);
        #endregion
    }
}
