using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class Convex
    {
        IntPtr mCoreObject;
        public IntPtr CoreObject
        {
            get { return mCoreObject; }
        }
        public Convex()
        {
            mCoreObject = SDK_v3dxConvex_New();
        }
        ~Convex()
        {
            SDK_v3dxConvex_Delete(mCoreObject);
            mCoreObject = IntPtr.Zero;
        }
        public bool IsContain(ref Vector3 pos)
        {
            unsafe
            {
                fixed (Vector3* p = &pos)
                {
                    return SDK_v3dxConvex_IsContain(mCoreObject, p);
                }
            }
        }
        #region SDK
        const string ModuleNC = CoreObjectBase.ModuleNC;
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static IntPtr SDK_v3dxConvex_New();
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_v3dxConvex_Delete(IntPtr p);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_v3dxConvex_IsContain(IntPtr self, Vector3* pos);
        #endregion
    }
}
