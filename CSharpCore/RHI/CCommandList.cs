using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    //public struct CMRTClearColor
    //{
    //    public CMRTClearColor(byte index, UInt32 color)
    //    {
    //        Index = index;
    //        Color = color;
    //    }
    //    public byte Index;
    //    public UInt32 Color;
    //}

    public struct CCommandListDesc
    {

    }
    public class CCommandList : AuxCoreObject<CCommandList.NativePointer>
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

        public CCommandList(NativePointer self)
        {
            mCoreObject = self;
        }

        public static CCommandList CreateFromPtr(NativePointer self)
        {
            var result = new CCommandList(self);
            result.Core_AddRef();
            return result;
        }

        public string DebugName
        {
            get
            {
                var ptr = SDK_ICommandList_GetDebugName(CoreObject);
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            }
            set
            {
                SDK_ICommandList_SetDebugName(CoreObject, value);
            }
        }
        int CommandMonitorIndex = -1;
        public void BeginCommand()
        {
            CommandMonitorIndex = EngineNS.Thread.ContextThread.CurrentContext.MonitorEnter(this);
            SDK_ICommandList_BeginCommand(CoreObject);
        }
        
        public void EndCommand()
        {
            SDK_ICommandList_EndCommand(CoreObject);
            EngineNS.Thread.ContextThread.CurrentContext.MonitorExit(CommandMonitorIndex);
        }

        //public void SetRenderTargets(CFrameBuffer FrameBuffer)
        //{
        //    SDK_ICommandList_SetRenderTargets(CoreObject, FrameBuffer.CoreObject);
        //}
        //public void ClearMRT(CMRTClearColor[] ClearColors, bool bClearDepth, float Depth, bool bClearStencil, UInt32 Stencil)
        //{
        //    unsafe
        //    {
        //        fixed (CMRTClearColor* p = &ClearColors[0])
        //        {
        //            SDK_ICommandList_ClearMRT(CoreObject, p, ClearColors.Length, vBOOL.FromBooleam(bClearDepth), Depth, vBOOL.FromBooleam(bClearStencil), Stencil);
        //        }
        //    }
        //}

        public void SetGraphicsProfiler(CGraphicsProfiler profiler)
        {
            if (profiler == null)
            {
                SDK_ICommandList_SetGraphicsProfiler(CoreObject, CGraphicsProfiler.GetEmptyNativePointer());
            }
            else
            {
                SDK_ICommandList_SetGraphicsProfiler(CoreObject, profiler.CoreObject);
            }
        }

        public void SetPassBuiltCallBack(FOnPassBuilt fun)
        {
            SDK_ICommandList_SetPassBuiltCallBack(CoreObject, fun);
        }

        public static FOnPassBuilt OnPassBuilt_WireFrame = OnPassBuilt_WireFrameCB;
        private static void OnPassBuilt_WireFrameCB(CCommandList.NativePointer cmdPtr, CPass.NativePointer ptr)
        {
            var pass = CPass.CreatePassFromPtr(ptr);
            if (pass.UserFlags == 0)
            {
                return;
            }
            var rsState = pass.RenderPipeline.RasterizerState;
            var desc = rsState.Desc;
            desc.FillMode = EFillMode.FMD_WIREFRAME;

            var rc = CEngine.Instance.RenderContext;
            var smp = CEngine.Instance.RasterizerStateManager.GetRasterizerState(rc, desc);

            CCommandList cmd = CCommandList.CreateFromPtr(cmdPtr);
            cmd.SetRasterizerState(smp);
        }

        public static FOnPassBuilt OnPassBuilt_WireFrameAnNoCull = OnPassBuilt_WireFrameAndNoCullCB;
        private static void OnPassBuilt_WireFrameAndNoCullCB(CCommandList.NativePointer cmdPtr, CPass.NativePointer ptr)
        {
            var pass = CPass.CreatePassFromPtr(ptr);
            if (pass.UserFlags == 0)
            {
                return;
            }
            var rsState = pass.RenderPipeline.RasterizerState;
            var desc = rsState.Desc;
            desc.FillMode = EFillMode.FMD_WIREFRAME;
            desc.CullMode = ECullMode.CMD_NONE;

            var rc = CEngine.Instance.RenderContext;
            var smp = CEngine.Instance.RasterizerStateManager.GetRasterizerState(rc, desc);

            CCommandList cmd = CCommandList.CreateFromPtr(cmdPtr);
            cmd.SetRasterizerState(smp);
        }

        public void BeginRenderPass(CRenderPassDesc RenderPassDesc, CFrameBuffer FrameBuffer)
        {
            unsafe
            {
                SDK_ICommandList_BeginRenderPass(CoreObject, &RenderPassDesc, FrameBuffer.CoreObject);
            }
        }

        public static Profiler.TimeScope ScopeBuildRenderPass = Profiler.TimeScopeManager.GetTimeScope(typeof(CCommandList), nameof(BuildRenderPass));
        public CPass BuildRenderPass(int limitter = int.MaxValue, bool lastestPass = false, bool bImmCBuffer = false)
        {
            ScopeBuildRenderPass.Begin();

            CPass pass = null;
            unsafe
            {
                if (lastestPass)
                {
                    CPass.NativePointer passPtr = new CPass.NativePointer();
                    SDK_ICommandList_BuildRenderPass(CoreObject, vBOOL.FromBoolean(bImmCBuffer), limitter, &passPtr);

                    if (passPtr.GetPointer() != IntPtr.Zero)
                        pass = CPass.CreatePassFromPtr(passPtr);
                }
                else
                {
                    SDK_ICommandList_BuildRenderPass(CoreObject, vBOOL.FromBoolean(bImmCBuffer), limitter, null);
                }
            }

            ScopeBuildRenderPass.End();

            return pass;
        }

        public void EndRenderPass()
        {
            mDrawCall = SDK_ICommandList_GetDrawCall(CoreObject);
            mDrawTriangle = SDK_ICommandList_GetDrawTriangle(CoreObject);
            mCmdCount = SDK_ICommandList_GetCmdCount(CoreObject);

            SDK_ICommandList_EndRenderPass(CoreObject);
        }

        public void Blit2DefaultFrameBuffer(CFrameBuffer FrameBuffer, int dstWidth, int dstHeight)
        {
            SDK_ICommandList_Blit2DefaultFrameBuffer(CoreObject, FrameBuffer.CoreObject, dstWidth, dstHeight);
        }

        public void PushPass(CPass Pass)
        {
            if (Pass.PreUse())
            {
                SDK_ICommandList_PushPass(CoreObject, Pass.CoreObject);
            }
        }

        public static Profiler.TimeScope ScopeExecute = Profiler.TimeScopeManager.GetTimeScope(typeof(CCommandList), nameof(Commit));
        public void Commit(CRenderContext RenderContext)
        {
            ScopeExecute.Begin();

            lock (this)
            {
                SDK_ICommandList_Commit(CoreObject, RenderContext.CoreObject);
            }

            ScopeExecute.End();
        }
        public void SetRasterizerState(CRasterizerState State)
        {
            SDK_ICommandList_SetRasterizerState(CoreObject, State.CoreObject);
        }
        public void SetDepthStencilState(CDepthStencilState State)
        {
            SDK_ICommandList_SetDepthStencilState(CoreObject, State.CoreObject);
        }
        public unsafe void SetBlendState(CBlendState State, float* blendFactor, UInt32 samplerMask)
        {
            SDK_ICommandList_SetBlendState(CoreObject, State.CoreObject, blendFactor, samplerMask);
        }
        public UInt32 PassNumber
        {
            get
            {
                return SDK_ICommandList_GetPassNumber(CoreObject);
            }
        }
        public int mDrawCall;
        public int DrawCall
        {
            get
            {
                return mDrawCall;
            }
        }
        public int mDrawTriangle;
        public int DrawTriangle
        {
            get
            {
                return mDrawTriangle;
            }
        }
        public UInt32 mCmdCount;
        public UInt32 CmdCount
        {
            get
            {
                return mCmdCount;
            }
        }

        #region ComputeShader
        public void SetComputeShader(CComputeShader ComputeShader)
        {
            if(ComputeShader==null)
                SDK_ICommandList_SetComputeShader(CoreObject, CComputeShader.GetEmptyNativePointer());
            else
                SDK_ICommandList_SetComputeShader(CoreObject, ComputeShader.CoreObject);
        }
        public void CSSetShaderResource(UInt32 Index, CShaderResourceView Texture)
        {
            SDK_ICommandList_CSSetShaderResource(CoreObject, Index, Texture.CoreObject);
        }
        public void CSSetUnorderedAccessView(UInt32 Index, CUnorderedAccessView view, UInt32[] pUAVInitialCounts)
        {
            unsafe
            {
                fixed(UInt32* p = &pUAVInitialCounts[0])
                {
                    if(view==null)
                        SDK_ICommandList_CSSetUnorderedAccessView(CoreObject, Index, CUnorderedAccessView.GetEmptyNativePointer(), p);
                    else
                        SDK_ICommandList_CSSetUnorderedAccessView(CoreObject, Index, view.CoreObject, p);
                }
            }
        }
        public void CSSetConstantBuffer(UInt32 Index, CConstantBuffer cbuffer)
        {
            SDK_ICommandList_CSSetConstantBuffer(CoreObject, Index, cbuffer.CoreObject);
        }
        public void CSDispatch(UInt32 x, UInt32 y, UInt32 z)
        {
            SDK_ICommandList_CSDispatch(CoreObject, x, y, z);
        }
        public void CreateReadableTexture2D(ref CTexture2D pTexture, CShaderResourceView src, CFrameBuffer pFrameBuffers)
        {
            unsafe
            {
                CTexture2D.NativePointer oldPtr = CTexture2D.GetEmptyNativePointer();
                if (pTexture != null)
                {
                    oldPtr = pTexture.CoreObject;
                }
                SDK_ICommandList_CreateReadableTexture2D(CoreObject, &oldPtr, src.CoreObject, pFrameBuffers.CoreObject);
                if (pTexture != null)
                {
                    pTexture.UnsafeSetNativePointer(oldPtr);
                }
                else
                {
                    pTexture = new CTexture2D(oldPtr);
                }
            }
        }
        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetDebugName(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static IntPtr SDK_ICommandList_GetDebugName(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_BeginCommand(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_EndCommand(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_ClearMeshDrawPassArray(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_ICommandList_GetPassNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_ICommandList_GetDrawCall(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_ICommandList_GetDrawTriangle(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_ICommandList_GetCmdCount(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetGraphicsProfiler(NativePointer self, CGraphicsProfiler.NativePointer profiler);

        public delegate void FOnPassBuilt(CCommandList.NativePointer cmd, CPass.NativePointer pass);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetPassBuiltCallBack(NativePointer self, FOnPassBuilt fun);
        
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_BeginRenderPass(NativePointer self, CRenderPassDesc* pRenderPassDesc, CFrameBuffer.NativePointer FrameBuffer);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_BuildRenderPass(NativePointer self, vBOOL bImmCBuffer, int limitter, CPass.NativePointer* ppPass);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_EndRenderPass(NativePointer self);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_Blit2DefaultFrameBuffer(NativePointer self, CFrameBuffer.NativePointer FrameBuffers, int dstWidth, int dstHeight);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_PushPass(NativePointer self, CPass.NativePointer Pass);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_Commit(NativePointer self, CRenderContext.NativePointer pRHICtx);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetComputeShader(NativePointer self, CComputeShader.NativePointer ComputeShader);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_CSSetShaderResource(NativePointer self, UInt32 Index, CShaderResourceView.NativePointer Texture);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_CSSetUnorderedAccessView(NativePointer self, UInt32 Index, CUnorderedAccessView.NativePointer view, UInt32* pUAVInitialCounts);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_CSSetConstantBuffer(NativePointer self, UInt32 Index, CConstantBuffer.NativePointer cbuffer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_CSDispatch(NativePointer self, UInt32 x, UInt32 y, UInt32 z);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern unsafe static vBOOL SDK_ICommandList_CreateReadableTexture2D(NativePointer self, CTexture2D.NativePointer* ppTexture, CShaderResourceView.NativePointer src, CFrameBuffer.NativePointer pFrameBuffers);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetRasterizerState(NativePointer self, CRasterizerState.NativePointer State);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_ICommandList_SetDepthStencilState(NativePointer self, CDepthStencilState.NativePointer State);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_ICommandList_SetBlendState(NativePointer self, CBlendState.NativePointer State, float* blendFactor, UInt32 samplerMask);
        #endregion
    }
}
