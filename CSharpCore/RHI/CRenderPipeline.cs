using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public struct CRenderPipelineDesc
    {

    }
    public class CRenderPipeline : AuxCoreObject<CRenderPipeline.NativePointer>
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

        public override void Cleanup()
        {
            //if(mShaderProgram!=null)
            //{
            //    mShaderProgram.Cleanup();
            //    mShaderProgram = null;
            //}
            //if (mRasterizerState != null)
            //{
            //    mRasterizerState.Cleanup();
            //    mRasterizerState = null;
            //}
            //if (mDepthStencilState != null)
            //{
            //    mDepthStencilState.Cleanup();
            //    mDepthStencilState = null;
            //}
            //if (mBlendState != null)
            //{
            //    mBlendState.Cleanup();
            //    mBlendState = null;
            //}
            base.Cleanup();
            mRasterizerState = null;
            mDepthStencilState = null;
            mBlendState = null;
            Core_Release(true);
        }

        public CRenderPipeline(NativePointer self, bool fromPtr = false)
        {
            mCoreObject = self;

            if(fromPtr)
            {
                Core_AddRef();
                
                var rs_ptr = SDK_IRenderPipeline_GetRasterizerState(CoreObject);
                if (rs_ptr.Pointer != IntPtr.Zero)
                {
                    mRasterizerState = new CRasterizerState(rs_ptr);
                    mRasterizerState.Core_AddRef();
                }
                var ds_ptr = SDK_IRenderPipeline_GetDepthStencilState(CoreObject);
                if (ds_ptr.Pointer != IntPtr.Zero)
                {
                    mDepthStencilState = new CDepthStencilState(ds_ptr);
                    mDepthStencilState.Core_AddRef();
                }
                var bs_ptr = SDK_IRenderPipeline_GetBindBlendState(CoreObject);
                if (bs_ptr.Pointer != IntPtr.Zero)
                {
                    mBlendState = new CBlendState(bs_ptr);
                    mBlendState.Core_AddRef();
                }
            }
        }
        
        CRasterizerState mRasterizerState;
        public CRasterizerState RasterizerState
        {
            get { return mRasterizerState; }
            set
            {
                mRasterizerState = value;
                if (value == null)
                {
                    CRasterizerState.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IRenderPipeline_BindRasterizerState(CoreObject, tmp);
                }
                else
                    SDK_IRenderPipeline_BindRasterizerState(CoreObject, value.CoreObject);
            }
        }
        CDepthStencilState mDepthStencilState;
        public CDepthStencilState DepthStencilState
        {
            get { return mDepthStencilState; }
            set
            {
                mDepthStencilState = value;
                if (value == null)
                {
                    CDepthStencilState.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IRenderPipeline_BindDepthStencilState(CoreObject, tmp);
                }
                else
                    SDK_IRenderPipeline_BindDepthStencilState(CoreObject, value.CoreObject);
            }
        }
        CBlendState mBlendState;
        public CBlendState BlendState
        {
            get { return mBlendState; }
            set
            {
                mBlendState = value;
                if (value == null)
                {
                    CBlendState.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IRenderPipeline_BindBlendState(CoreObject, tmp);
                }
                else
                    SDK_IRenderPipeline_BindBlendState(CoreObject, value.CoreObject);
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IRenderPipeline_BindRasterizerState(NativePointer self, CRasterizerState.NativePointer State);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IRenderPipeline_BindDepthStencilState(NativePointer self, CDepthStencilState.NativePointer State);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IRenderPipeline_BindBlendState(NativePointer self, CBlendState.NativePointer State);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CRasterizerState.NativePointer SDK_IRenderPipeline_GetRasterizerState(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CDepthStencilState.NativePointer SDK_IRenderPipeline_GetDepthStencilState(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CBlendState.NativePointer SDK_IRenderPipeline_GetBindBlendState(NativePointer self);

        #endregion
    }
}
