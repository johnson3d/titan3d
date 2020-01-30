using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public struct CFrameBuffersDesc
    {
        public vBOOL IsSwapChainBuffer;
        public vBOOL UseDSV;
    }

    public enum FrameBufferLoadAction
    {
        LoadActionDontCare = 0,
        LoadActionLoad = 1,
        LoadActionClear = 2
    };

    public enum FrameBufferStoreAction
    {
        StoreActionDontCare = 0,
        StoreActionStore = 1,
        StoreActionMultisampleResolve = 2,
        StoreActionStoreAndMultisampleResolve= 3,
        StoreActionUnknown  = 4
    };

    public struct FrameBufferClearColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
    };

    public struct CRenderPassDesc
    {
        public FrameBufferLoadAction mFBLoadAction_Color;
        public FrameBufferStoreAction mFBStoreAction_Color;
        public FrameBufferClearColor mFBClearColorRT0;
        public FrameBufferClearColor mFBClearColorRT1;
        public FrameBufferClearColor mFBClearColorRT2;
        public FrameBufferClearColor mFBClearColorRT3;
        public FrameBufferClearColor mFBClearColorRT4;
        public FrameBufferClearColor mFBClearColorRT5;
        public FrameBufferClearColor mFBClearColorRT6;
        public FrameBufferClearColor mFBClearColorRT7;

        public FrameBufferLoadAction mFBLoadAction_Depth;
        public FrameBufferStoreAction mFBStoreAction_Depth;
        public float mDepthClearValue;

        public FrameBufferLoadAction mFBLoadAction_Stencil;
        public FrameBufferStoreAction mFBStoreAction_Stencil;
        public UInt32 mStencilClearValue;
        
    }

    public class CFrameBuffer : AuxCoreObject<CFrameBuffer.NativePointer>
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

        public CFrameBuffer(NativePointer self)
        {
            mCoreObject = self;
        }
        ~CFrameBuffer()
        {

        }
        public void BindRenderTargetView(UInt32 index, CRenderTargetView RTV)
        {
            SDK_IFrameBuffers_BindRenderTargetView(CoreObject, index, RTV.CoreObject);
        }

        public void BindSRV_RT(UInt32 index, CShaderResourceView SRV)
        {
            mSRVArray_RT[index] = SRV;
        }

        public void BindSRV_DS(CShaderResourceView SRV)
        {
            mSRV_DS = SRV;
        }

        public void BindDepthStencilView(CDepthStencilView DSV)
        {
            SDK_IFrameBuffers_BindDepthStencilView(CoreObject, DSV.CoreObject);
        }
        
        private CShaderResourceView[] mSRVArray_RT = new CShaderResourceView[8];
        public CShaderResourceView GetSRV_RenderTarget(UInt32 index)
        {
            return mSRVArray_RT[index];
        }

        private CShaderResourceView mSRV_DS;
        public CShaderResourceView GetSRV_DepthStencil()
        {
            return mSRV_DS;
        }


        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IFrameBuffers_BindRenderTargetView(NativePointer self, UInt32 index, CRenderTargetView.NativePointer rt);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IFrameBuffers_BindDepthStencilView(NativePointer self, CDepthStencilView.NativePointer ds);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CRenderTargetView.NativePointer SDK_IFrameBuffers_GetRenderTargetView(NativePointer self, UInt32 index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CDepthStencilView.NativePointer SDK_IFrameBuffers_GetDepthStencilView(NativePointer self);
        #endregion
    }
}
