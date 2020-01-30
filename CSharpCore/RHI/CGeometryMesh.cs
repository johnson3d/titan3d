using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EVertexSteamType
    {
        VST_Position,
        VST_Normal,
        VST_Tangent,
        VST_Color,
        VST_UV,
        VST_LightMap,
        VST_SkinIndex,
        VST_SkinWeight,
        VST_TerrainIndex,
        VST_TerrainGradient,//10
        VST_InstPos,
        VST_InstQuat,
        VST_InstScale,
        VST_F4_1,
        VST_F4_2,
        VST_F4_3,//16
        VST_Number,
    };

    public class CVertexArray : AuxCoreObject<CVertexArray.NativePointer>
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
        public CVertexArray()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IVertexArray");
        }
        public void BindVertexBuffer(EVertexSteamType index, CVertexBuffer vb)
        {
            SDK_IVertexArray_BindVertexBuffer(CoreObject, index, vb.CoreObject);
        }
        public CVertexBuffer GetVertexBuffer(EVertexSteamType index)
        {
            var ptr = SDK_IVertexArray_GetVertexBuffer(CoreObject, index);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var vb = new CVertexBuffer(ptr);
            vb.Core_AddRef();
            return vb;
        }
        public UInt32 NumInstances
        {
            get
            {
                return SDK_IVertexArray_GetNumInstances(CoreObject);
            }
            set
            {
                SDK_IVertexArray_SetNumInstances(CoreObject, value);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IVertexArray_BindVertexBuffer(NativePointer self, EVertexSteamType index, CVertexBuffer.NativePointer vb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_IVertexArray_GetVertexBuffer(NativePointer self, EVertexSteamType index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IVertexArray_SetNumInstances(NativePointer self, UInt32 num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IVertexArray_GetNumInstances(NativePointer self);
        #endregion
    }

    public class CGeometryMesh : AuxCoreObject<CGeometryMesh.NativePointer>
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

        public CGeometryMesh()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IGeometryMesh");
        }
        public CGeometryMesh(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        internal void UnsafeReInit(NativePointer self)
        {
            Core_Release();
            mCoreObject = self;
            Core_AddRef();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Core_Release(true);
        }
        public bool Dirty
        {
            get
            {
                return SDK_IGeometryMesh_GetIsDirty(CoreObject);
            }
            set
            {
                SDK_IGeometryMesh_SetIsDirty(CoreObject, vBOOL.FromBoolean(value));
            }
        }
        public void BindVertexBuffer(EVertexSteamType index, CVertexBuffer vb)
        {
            SDK_IGeometryMesh_BindVertexBuffer(CoreObject, index, vb.CoreObject);
        }
        public void BindIndexBuffer(CIndexBuffer ib)
        {
            SDK_IGeometryMesh_BindIndexBuffer(CoreObject, ib.CoreObject);
        }
        public CVertexBuffer GetVertexBuffer(EVertexSteamType index)
        {
            var ptr = SDK_IGeometryMesh_GetVertexBuffer(CoreObject, index);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var vb = new CVertexBuffer(ptr);
            vb.Core_AddRef();
            return vb;
        }
        public CIndexBuffer GetIndexBuffer()
        {
            var ptr = SDK_IGeometryMesh_GetIndexBuffer(CoreObject);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var ib = new CIndexBuffer(ptr);
            ib.Core_AddRef();
            return ib;
        }
        public static CGeometryMesh MergeGeoms(CRenderContext rc, List<Graphics.Mesh.CGfxMeshPrimitives> meshArray, List<Matrix> transforms, ref BoundingBox aabb)
        {
            CGeometryMesh.NativePointer[] usedArray = new CGeometryMesh.NativePointer[meshArray.Count];
            Matrix[] usedMatrix = transforms.ToArray();
            for (int i=0;i<meshArray.Count;i++)
            {
                meshArray[i].PreUse(true);
                usedArray[i] = meshArray[i].GeometryMesh.CoreObject;
            }
            unsafe
            {
                fixed (CGeometryMesh.NativePointer* p = &usedArray[0])
                fixed (Matrix* pMatrix = &usedMatrix[0])
                fixed (BoundingBox* pAABB = &aabb)
                {
                    var ptr = SDK_IGeometryMesh_MergeGeoms(rc.CoreObject, p, pMatrix, meshArray.Count, pAABB);
                    var result = new CGeometryMesh(ptr);
                    result.Core_Release();
                    return result;
                }
            }
        }

        public static Graphics.Mesh.CGfxMeshPrimitives MergeGeomsAsMeshSimple(CRenderContext rc, List<Graphics.Mesh.CGfxMeshPrimitives> meshArray, List<Matrix> transforms)
        {
            var aabb = new BoundingBox();
            var geom = MergeGeoms(rc, meshArray, transforms, ref aabb);
            var result = new Graphics.Mesh.CGfxMeshPrimitives();
            result.InitFromGeomtryMesh(rc, geom, 1, ref aabb);

            var desc = new CDrawPrimitiveDesc();
            desc.SetDefault();
            foreach(var i in meshArray)
            {
                var td = new CDrawPrimitiveDesc();
                i.GetAtom(0, 0, ref td);
                desc.NumPrimitives += td.NumPrimitives;
            }
            result.PushAtomLOD(0, ref desc);
            return result;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe CGeometryMesh.NativePointer SDK_IGeometryMesh_MergeGeoms(CRenderContext.NativePointer rc, CGeometryMesh.NativePointer* meshArray, Matrix* scaleArray, int count, BoundingBox* aabb);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IGeometryMesh_SetIsDirty(NativePointer self, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IGeometryMesh_GetIsDirty(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IGeometryMesh_BindVertexBuffer(NativePointer self, EVertexSteamType index, CVertexBuffer.NativePointer vb);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IGeometryMesh_BindIndexBuffer(NativePointer self, CIndexBuffer.NativePointer ib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_IGeometryMesh_GetVertexBuffer(NativePointer self, EVertexSteamType index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CIndexBuffer.NativePointer SDK_IGeometryMesh_GetIndexBuffer(NativePointer self);
        #endregion
    }
}
