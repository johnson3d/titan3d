using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Mesh
{
   
    public abstract class CGfxModifier : AuxCoreObject<CGfxModifier.NativePointer>
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
        public CGfxModifier()
        {

        }
        public CGfxModifier(NativePointer self)
        {
            mCoreObject = self;
        }
        public string Name
        {
            get
            {
                return SDK_GfxModifier_GetName(CoreObject);
            }
            protected set
            {
                SDK_GfxModifier_SetName(CoreObject, value);
            }
        }
        public RName ShaderModuleName
        {
            get
            {
                return RName.GetRName(SDK_GfxModifier_GetShaderModuleName(CoreObject));
            }
            protected set
            {
                SDK_GfxModifier_SetShaderModuleName(CoreObject, value.Name);
            }
        }
        public virtual string FunctionName
        {
            get { return ""; }
        }
        public virtual void Save2Xnd(IO.XndNode node)
        {
            SDK_GfxModifier_Save2Xnd(CoreObject, node.CoreObject);
        }
        public virtual bool LoadXnd(IO.XndNode node)
        {
            return (bool)SDK_GfxModifier_LoadXnd(CoreObject, node.CoreObject);
        }
        public virtual async System.Threading.Tasks.Task<bool> LoadXndAsync(IO.XndNode node)
        {
            var ret = await CEngine.Instance.EventPoster.Post(() =>
            {
                return SDK_GfxModifier_LoadXnd(CoreObject, node.CoreObject);
            });
            return ret;
        }
        public virtual Profiler.TimeScope GetTickTimeLogicScope()
        {
            return null;
        }
        public virtual void TickLogic(CRenderContext rc, CGfxMesh mesh, Int64 time)
        {
            SDK_GfxModifier_TickLogic(CoreObject, rc.CoreObject, mesh.CoreObject, time);
        }
        public virtual void TickSync(CRenderContext rc, CGfxMesh mesh, Int64 time)
        {
            SDK_GfxModifier_TickSync(CoreObject, rc.CoreObject, mesh.CoreObject, time);
        }
        public virtual void OnSetPassData(CPass pass, bool shadow)
        {

        }
        public virtual CGfxModifier CloneModifier(CRenderContext rc)
        {
            return null;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxModifier_GetName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxModifier_SetName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public extern static string SDK_GfxModifier_GetShaderModuleName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxModifier_SetShaderModuleName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxModifier_TickLogic(NativePointer self, CRenderContext.NativePointer rc, CGfxMesh.NativePointer mesh, Int64 time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxModifier_TickSync(NativePointer self, CRenderContext.NativePointer rc, CGfxMesh.NativePointer mesh, Int64 time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxModifier_Save2Xnd(NativePointer self, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxModifier_LoadXnd(NativePointer self, IO.XndNode.NativePointer node);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxModifier.NativePointer SDK_GfxModifier_CloneModifier(NativePointer self, CRenderContext.NativePointer rc);
        #endregion
    }
}
