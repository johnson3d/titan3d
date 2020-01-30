using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineNS;
using EngineNS.Graphics.Mesh;
namespace FBX
{
    /// <summary>
    /// 对应于FBX的一个文件，相当于FBXSDK里面的一个FBXScene
    /// </summary>
    class FBXAnalyzer : AuxCoreObject<FBXAnalyzer.NativePointer>
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
        public FBXAnalyzer()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::FBXAnalyzer");
        }
        public override void Cleanup()
        {
            primitivesMesh.Cleanup();
            base.Cleanup();
        }
        public bool LoadFile(CGfxFBXManager  fbxManager, string fileName)
        {
            return SDK_FBXAnalyzer_LoadFile(mCoreObject, fbxManager.CoreObject, fileName);
        }

        CGfxMeshPrimitives primitivesMesh = null;
        public CGfxMesh GetCGfxMesh(CRenderContext rc, RName meshName, RName meshPrimitiveName)
        {
            //primitivesMesh?.Cleanup();
            //primitivesMesh = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(rc, meshPrimitiveName, 1);

            ////max中的世界位置的偏移没有到，应该做成选项，是否做偏移
            //SetGeomtryMeshStream(rc, primitivesMesh);

            //CDrawPrimitiveDesc dcDesc = new CDrawPrimitiveDesc();
            //dcDesc.SetDefault();
            //dcDesc.NumPrimitives = (uint)GetPrimitivesNum();  // 直接从primitivesMesh 获取？
            //primitivesMesh.PushAtomLOD(0, ref dcDesc);
            //var mesh = CEngine.Instance.MeshManager.NewMesh(rc, meshName, meshPrimitiveName/*, CEngine.Instance.ShadingEnvManager.DefaultShadingEnv*/);
            //var mtl = CEngine.Instance.MaterialInstanceManager.GetMaterialInstance(rc, RName.GetRName("Material/testmaterial.instmtl"));
            //var shadingEnv = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(RName.GetRName("ShadingEnv/fsbase.senv"));
            //mesh.Init(rc, meshName, primitivesMesh/*, shadingEnv*/);
            //mesh.SetMaterial(rc, 0, mtl);
            //primitivesMesh.SaveMesh();     
            //mesh.SaveMesh();
            return null;
        }
        void SetGeomtryMeshStream(CRenderContext rc, CGfxMeshPrimitives primitivesMesh)
        {
            SDK_FBXAnalyzer_SetGeomtryMeshStream(mCoreObject, rc.CoreObject, primitivesMesh.CoreObject);
        }

        int GetPrimitivesNum()
        {
            return SDK_FBXAnalyzer_GetPrimitivesNum(mCoreObject);
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EngineNS.vBOOL SDK_FBXAnalyzer_LoadFile(NativePointer self, CGfxFBXManager .NativePointer nativePointer, string fileName);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public unsafe extern static EngineNS.Graphics.Mesh.CGfxMesh.NativePointer SDK_FBXAnalyzer_SetGeomtryMeshStream(NativePointer self, CRenderContext.NativePointer rc, CGfxMeshPrimitives.NativePointer primitiveMesh);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_FBXAnalyzer_GetPrimitivesNum(NativePointer self);

        #endregion
    }
}
