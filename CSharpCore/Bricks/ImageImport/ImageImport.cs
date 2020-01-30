using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.ImageImport
{
    public class ImageImport : AuxCoreObject<ImageImport.NativePointer>
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

        public ImageImport()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("ImageImport");
        }

        public int GetWidth()
        {
            return SDK_ImageImport_GetWidth(CoreObject);
        }

        public int GetHeight()
        {
            return SDK_ImageImport_GetHeight(CoreObject);
        }

        public int GetChannels()
        {
            return SDK_ImageImport_GetChannels(CoreObject);
        }

        public void LoadTexture(string name, Support.CBlobObject blob)
        {
            SDK_ImageImport_LoadTexture(CoreObject, name, blob.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_ImageImport_GetWidth(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_ImageImport_GetHeight(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_ImageImport_GetChannels(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ImageImport_LoadTexture(NativePointer self, string name, Support.CBlobObject.NativePointer blob);
        #endregion
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public static bool IsWriteEtc = false;
    }
}
