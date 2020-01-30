using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Mesh
{
    public class CGfxMeshDataProvider : AuxCoreObject<CGfxMeshDataProvider.NativePointer>
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
        public Support.CBlobObject[] VertexBuffers = new Support.CBlobObject[(int)EVertexSteamType.VST_Number];
        public Support.CBlobObject IndexBuffer;
        public CGfxMeshDataProvider()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxMeshDataProvider");
        }
        public bool InitFromMesh(CRenderContext rc, CGfxMeshPrimitives mesh)
        {
            mesh.PreUse(true);
            if (false == SDK_GfxMeshDataProvider_InitFromMesh(CoreObject, rc.CoreObject, mesh.CoreObject))
                return false;

            var ptr = SDK_GfxMeshDataProvider_GetIndices(CoreObject);
            IndexBuffer = new Support.CBlobObject(ptr);

            for (int i = 0; i < (int)EVertexSteamType.VST_Number; i++)
            {
                ptr = SDK_GfxMeshDataProvider_GetStream(CoreObject, (EVertexSteamType)i);
                if (ptr.GetPointer() == IntPtr.Zero)
                    continue;
                VertexBuffers[i] = new Support.CBlobObject(ptr);
            }

            return true;
        }
        public bool Init(UInt32 streams, EIndexBufferType ibType, int atom)
        {
            if (false == SDK_GfxMeshDataProvider_Init(CoreObject, streams, ibType, atom))
                return false;

            var ptr = SDK_GfxMeshDataProvider_GetIndices(CoreObject);
            IndexBuffer = new Support.CBlobObject(ptr);

            for (int i = 0; i < (int)EVertexSteamType.VST_Number; i++)
            {
                ptr = SDK_GfxMeshDataProvider_GetStream(CoreObject, (EVertexSteamType)i);
                if (ptr.GetPointer() == IntPtr.Zero)
                    continue;
                VertexBuffers[i] = new Support.CBlobObject(ptr);
            }
            return true;
        }
        public int VertexNumber
        {
            get
            {
                return SDK_GfxMeshDataProvider_GetVertexNumber(CoreObject);
            }
        }
        public int TriangleNumber
        {
            get
            {
                return SDK_GfxMeshDataProvider_GetTriangleNumber(CoreObject);
            }
        }
        public int AtomNumber
        {
            get
            {
                return SDK_GfxMeshDataProvider_GetAtomNumber(CoreObject);
            }
        }
        public CDrawPrimitiveDesc this[UInt32 index, UInt32 lod]
        {
            get
            {
                CDrawPrimitiveDesc desc = new CDrawPrimitiveDesc();
                unsafe
                {
                    SDK_GfxMeshDataProvider_GetAtom(CoreObject, index, lod, &desc);
                }
                return desc;
            }
            set
            {
                unsafe
                {
                    SDK_GfxMeshDataProvider_SetAtom(CoreObject, index, lod, &value);
                }
            }
        }

        public void PushAtomLOD(UInt32 index, ref CDrawPrimitiveDesc desc)
        {
            unsafe
            {
                fixed (CDrawPrimitiveDesc* p = &desc)
                {
                    SDK_GfxMeshDataProvider_PushAtomLOD(CoreObject, index, p);
                }
            }
        }
        public UInt32 GetAtomLOD(UInt32 index)
        {
            return SDK_GfxMeshDataProvider_GetAtomLOD(CoreObject, index);
        }
        public bool GetTriangle(int index, ref UInt32 vA, ref UInt32 vB, ref UInt32 vC)
        {
            unsafe
            {
                fixed (UInt32* pA = &vA)
                fixed (UInt32* pB = &vB)
                fixed (UInt32* pC = &vC)
                {
                    return SDK_GfxMeshDataProvider_GetTriangle(CoreObject, index, pA, pB, pC);
                }
            }
        }
        public bool GetAtomTriangle(UInt32 atom, UInt32 index, ref UInt32 vA, ref UInt32 vB, ref UInt32 vC)
        {
            unsafe
            {
                fixed (UInt32* pA = &vA)
                fixed (UInt32* pB = &vB)
                fixed (UInt32* pC = &vC)
                {
                    return SDK_GfxMeshDataProvider_GetAtomTriangle(CoreObject, atom, index, pA, pB, pC);
                }
            }
        }
        public Vector3 GetPositionOrNormal(EVertexSteamType stream, UInt32 index)
        {
            unsafe
            {
                if (index >= VertexBuffers[(int)stream].Size / sizeof(Vector3))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var pPos = (Vector3*)VertexBuffers[(int)stream].Data.ToPointer();
                return pPos[index];
            }
        }
        public int IntersectTriangle(ref Vector3 vStart, ref Vector3 vEnd, out float dist)
        {
            unsafe
            {
                fixed (Vector3* pStart = &vStart)
                fixed (Vector3* pEnd = &vEnd)
                fixed(float* pDist = &dist)
                {
                    return SDK_GfxMeshDataProvider_IntersectTriangle(CoreObject, pStart, pEnd, pDist);
                }   
            }
        }
        public IntPtr GetVertexPtr(EVertexSteamType stream, UInt32 index)
        {
            return SDK_GfxMeshDataProvider_GetVertexPtr(CoreObject, stream, index);
        }
        //public UInt32 GetStreamStride(EVertexSteamType stream)
        //{
        //    return SDK_GfxMeshDataProvider_GetStreamStride(CoreObject, stream);
        //}
        //public UInt32 AddVertex(ref Vector3 pos, ref Vector3 nor, ref Vector2 uv, UInt32 color)
        //{
        //    unsafe
        //    {
        //        fixed(Vector3* pPos = &pos)
        //        fixed (Vector3* pNor = &nor)
        //        fixed (Vector2* pUV = &uv)
        //        {
        //            return SDK_GfxMeshDataProvider_AddVertex(CoreObject, pPos, pNor, pUV, color);
        //        }   
        //    }
        //}
        //public bool AddTriangle(UInt32 a, UInt32 b, UInt32 c)
        //{
        //    return SDK_GfxMeshDataProvider_AddTriangle(CoreObject, a, b, c);
        //}
        public bool ToMesh(CRenderContext rc, CGfxMeshPrimitives mesh)
        {
            return SDK_GfxMeshDataProvider_ToMesh(CoreObject, rc.CoreObject, mesh.CoreObject);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshDataProvider_InitFromMesh(NativePointer self, CRenderContext.NativePointer rc, CGfxMeshPrimitives.NativePointer mesh);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshDataProvider_Init(NativePointer self, UInt32 streams, EIndexBufferType ibType, int atom);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxMeshDataProvider_GetVertexNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxMeshDataProvider_GetTriangleNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxMeshDataProvider_GetAtomNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Support.CBlobObject.NativePointer SDK_GfxMeshDataProvider_GetStream(NativePointer self, EVertexSteamType index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Support.CBlobObject.NativePointer SDK_GfxMeshDataProvider_GetIndices(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshDataProvider_GetAtom(NativePointer self, UInt32 index, UInt32 lod, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshDataProvider_SetAtom(NativePointer self, UInt32 index, UInt32 lod, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxMeshDataProvider_PushAtomLOD(NativePointer self, UInt32 index, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_GfxMeshDataProvider_GetAtomLOD(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshDataProvider_GetTriangle(NativePointer self, int index, UInt32* vA, UInt32* vB, UInt32* vC);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_GfxMeshDataProvider_GetAtomTriangle(NativePointer self, UInt32 atom, UInt32 index, UInt32* vA, UInt32* vB, UInt32* vC);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe int SDK_GfxMeshDataProvider_IntersectTriangle(NativePointer self, Vector3* vStart, Vector3* vEnd, float* dist);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_GfxMeshDataProvider_GetVertexPtr(NativePointer self, EVertexSteamType stream, UInt32 index);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static UInt32 SDK_GfxMeshDataProvider_GetStreamStride(NativePointer self, EVertexSteamType stream);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static unsafe UInt32 SDK_GfxMeshDataProvider_AddVertex(NativePointer self, Vector3* pos, Vector3* nor, Vector2* uv, UInt32 color);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static vBOOL SDK_GfxMeshDataProvider_AddTriangle(NativePointer self, UInt32 a, UInt32 b, UInt32 c);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxMeshDataProvider_ToMesh(NativePointer self, CRenderContext.NativePointer rc, CGfxMeshPrimitives.NativePointer mesh);
        #endregion
    }
}
