using EngineNS;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FBX
{
    public class CGfxFBXManager : AuxCoreObject<CGfxFBXManager .NativePointer>
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

        static CGfxFBXManager  mInstance = null;
        public static CGfxFBXManager  Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new CGfxFBXManager ();
                }
                return mInstance;
            }
        }
        public static List<string> SupportFileFormats = new List<string>() {".fbx", ".dxf", ".obj", ".3ds", ".dae", ".abc", ".bvh", ".htr", ".trc", ".asf", ".amc", ".c3d", ".aoa", ".mcd"};
        public bool IsSupportFileFormat(string format)
        {
            format = format.ToLower();
            if (SupportFileFormats.Contains(format))
                return true;
            return false;
        }
         private CGfxFBXManager ()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxFBXManager");
        }

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static EngineNS.vBOOL SDK_FBXManager_Init(NativePointer self);
        #endregion
    }
}
