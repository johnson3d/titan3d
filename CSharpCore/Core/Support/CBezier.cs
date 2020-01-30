using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Support
{
    public class CBezier : AuxCoreObject<CBezier.NativePointer>
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
        public CBezier(bool Is2D=true)
        {
            mCoreObject = SDK_v3dxBezier_New(vBOOL.FromBoolean(Is2D));
        }
        public CBezier CloneObject()
        {
            var clone = new CBezier(this.Is2D);
            SDK_v3dxBezier_CopyTo(mCoreObject, clone.mCoreObject);
            return clone;
        }
        public void AddNode(ref Vector3 pos, ref Vector3 ctrlPos1, ref Vector3 ctrlPos2)
        {
            unsafe
            {
                fixed (Vector3* ptr1 = &pos)
                fixed (Vector3* ptr2 = &ctrlPos1)
                fixed (Vector3* ptr3 = &ctrlPos2)
                {
                    SDK_v3dxBezier_AddNode(mCoreObject, ptr1, ptr2, ptr3);
                }
            }
        }
        public void InsertNode(int idx, ref Vector3 pos, ref Vector3 ctrlPos1, ref Vector3 ctrlPos2)
        {
            unsafe
            {
                fixed (Vector3* ptr1 = &pos)
                fixed (Vector3* ptr2 = &ctrlPos1)
                fixed (Vector3* ptr3 = &ctrlPos2)
                {
                    SDK_v3dxBezier_InsertNode(mCoreObject, idx, ptr1, ptr2, ptr3);
                }
            }
        }
        public void DeleteNode(int idx)
        {
            SDK_v3dxBezier_DeleteNode(CoreObject, idx);
        }
        // fTime 0-1
        public Vector3 GetValue(float fTime)
        {
            Vector3 value;
            unsafe
            {
                SDK_v3dxBezier_GetValue(mCoreObject, fTime, &value);
            }
            return value;
        }
        public bool IsInRangeX(float value)
        {
            return SDK_v3dxBezier_IsInRangeX(mCoreObject, value);
        }
        public bool IsInRangeY(float value)
        {
            return SDK_v3dxBezier_IsInRangeY(mCoreObject, value);
        }
        public bool IsInRangeZ(float value)
        {
            return SDK_v3dxBezier_IsInRangeZ(mCoreObject, value);
        }
        public void GetValueRangeX(out float begin, out float end)
        {
            unsafe
            {
                fixed(float* beginPtr = &begin)
                fixed(float* endPtr = &end)
                {
                    SDK_v3dxBezier_GetRangeX(mCoreObject, beginPtr, endPtr);
                }
            }
        }
        public void GetValueRangeY(out float begin, out float end)
        {
            unsafe
            {
                fixed (float* beginPtr = &begin)
                fixed (float* endPtr = &end)
                {
                    SDK_v3dxBezier_GetRangeY(mCoreObject, beginPtr, endPtr);
                }
            }
        }
        public void GetValueRangeZ(out float begin, out float end)
        {
            unsafe
            {
                fixed (float* beginPtr = &begin)
                fixed (float* endPtr = &end)
                {
                    SDK_v3dxBezier_GetRangeZ(mCoreObject, beginPtr, endPtr);
                }
            }
        }
        public Vector3 GetPosition(int idx)
        {
            Vector3 value;
            unsafe
            {
                SDK_v3dxBezier_GetPosition(mCoreObject, idx, &value);
            }
            return value;
        }
        public void SetPosition(int idx, Vector3 value)
        {
            unsafe
            {
                SDK_v3dxBezier_SetPosition(mCoreObject, idx, &value);
            }
        }
        public void SetControlPos1(int idx, Vector3 pos, bool bWithPos2)
        {
            unsafe
            {
                SDK_v3dxBezier_SetControlPos1(mCoreObject, idx, &pos, vBOOL.FromBoolean(bWithPos2));
            }
        }
        public void SetControlPos2(int idx, Vector3 pos, bool bWithPos1)
        {
            unsafe
            {
                SDK_v3dxBezier_SetControlPos2(mCoreObject, idx, &pos, vBOOL.FromBoolean(bWithPos1));
            }
        }
        public Vector3 GetControlPos1(int idx)
        {
            Vector3 value;
            unsafe
            {
                SDK_v3dxBezier_GetControlPos1(mCoreObject, idx, &value);
            }
            return value;
        }
        public Vector3 GetControlPos2(int idx)
        {
            Vector3 value;
            unsafe
            {
                SDK_v3dxBezier_GetControlPos2(mCoreObject, idx, &value);
            }
            return value;
        }
        public void ClearNodes()
        {
            SDK_v3dxBezier_ClearNodes(mCoreObject);
        }
        public int NodeCount
        {
            get
            {
                return SDK_v3dxBezier_GetNodesCount(mCoreObject);
            }
        }
        public bool Is2D
        {
            get
            {
                return SDK_v3dxBezier_GetIs2D(mCoreObject);
            }
        }
        
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static NativePointer SDK_v3dxBezier_New(vBOOL Is2d);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_v3dxBezier_GetIs2D(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_AddNode(NativePointer self, Vector3* pos, Vector3* ctrlPos1, Vector3* ctrlPos2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_InsertNode(NativePointer self, int idx, Vector3* pos, Vector3* ctrlPos1, Vector3* ctrlPos2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_v3dxBezier_DeleteNode(NativePointer self, int idx);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetValue(NativePointer self, float fTime, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetPosition(NativePointer self, int idx, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_SetPosition(NativePointer self, int idx, Vector3* value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_SetControlPos1(NativePointer self, int idx, Vector3* pos, vBOOL bWithPos2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_SetControlPos2(NativePointer self, int idx, Vector3* pos, vBOOL bWithPos1);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetControlPos1(NativePointer self, int idx, Vector3* pos);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetControlPos2(NativePointer self, int idx, Vector3* pos);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_v3dxBezier_GetNodesCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_v3dxBezier_CopyTo(NativePointer self, NativePointer cloneTo);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_v3dxBezier_ClearNodes(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_v3dxBezier_IsInRangeX(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetRangeX(NativePointer self, float* begin, float* end);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_v3dxBezier_IsInRangeY(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetRangeY(NativePointer self, float* begin, float* end);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_v3dxBezier_IsInRangeZ(NativePointer self, float value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_v3dxBezier_GetRangeZ(NativePointer self, float* begin, float* end);
        #endregion
    }
}
