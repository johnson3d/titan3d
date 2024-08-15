using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.EGui
{
    public class TtImDrawCmdParameters : IDisposable
    {
        public unsafe static T CreateInstance<T>() where T : TtImDrawCmdParameters, new()
        {
            var result = new T();
            result.NativeHandle = System.Runtime.InteropServices.GCHandle.Alloc(result);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            result.Drawcall = rc.CreateGraphicDraw();
            {
                var pipeDesc = new NxRHI.FGpuPipelineDesc();
                pipeDesc.SetDefault();

                ref var rstDesc = ref pipeDesc.m_Rasterizer;
                rstDesc.ScissorEnable = 1;
                rstDesc.FillMode = NxRHI.EFillMode.FMD_SOLID;
                rstDesc.CullMode = NxRHI.ECullMode.CMD_NONE;

                ref var dssDesc = ref pipeDesc.m_DepthStencil;
                dssDesc.DepthEnable = 0;
                dssDesc.DepthWriteMask = NxRHI.EDepthWriteMask.DSWM_ZERO;
                dssDesc.DepthFunc = NxRHI.EComparisionMode.CMP_ALWAYS;

                ref var bldDesc = ref pipeDesc.m_Blend;
                var pRenderTarget = bldDesc.RenderTarget;
                pRenderTarget[0].SetDefault();
                pRenderTarget[0].SrcBlendAlpha = NxRHI.EBlend.BLD_INV_SRC_ALPHA;
                pRenderTarget[0].DestBlendAlpha = NxRHI.EBlend.BLD_ONE;
                pRenderTarget[0].BlendEnable = 1;
                var pipeline = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in pipeDesc);
                result.Drawcall.BindPipeline(pipeline);

                //Pipeline.mCoreObject.GetGpuProgram().BindInputLayout(renderer.InputLayout.mCoreObject);
            }

            return result;
        }
        public static unsafe TtImDrawCmdParameters FromNativeHandle(void* ptr)
        {
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)ptr);
            return handle.Target as TtImDrawCmdParameters;
        }
        public virtual void Dispose()
        {
            CoreSDK.DisposeObject(ref Drawcall);
            NativeHandle.Free();
        }
        public NxRHI.UGraphicDraw Drawcall;
        private System.Runtime.InteropServices.GCHandle NativeHandle;
        public unsafe void* GetHandle()
        {
            return System.Runtime.InteropServices.GCHandle.ToIntPtr(NativeHandle).ToPointer();
        }
        public virtual void OnDraw(in Matrix mvp)
        {

        }
    }

    public class UImDrawDataRHI : IDisposable
    {
        public NxRHI.UCommandList CmdList;
        public NxRHI.TtEffectBinder FontTextureBindInfo;
        public NxRHI.UCbView FontCBuffer;
        public NxRHI.UGraphicDraw Drawcall;

        public NxRHI.UGeomMesh GeomMesh;
        public Graphics.Mesh.TtMeshPrimitives PrimitiveMesh;

        #region TriangleData
        public NxRHI.UVbView VertexBuffer;
        public NxRHI.UIbView IndexBuffer;
        public Support.UNativeArray<ImDrawVert> DataVB = Support.UNativeArray<ImDrawVert>.CreateInstance();
        public Support.UNativeArray<ushort> DataIB = Support.UNativeArray<ushort>.CreateInstance();
        #endregion
        public unsafe bool InitializeGraphics(EPixelFormat format, EPixelFormat dsFormat)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            CmdList = rc.CreateCommandList();

            var renderer = TtEngine.Instance.GfxDevice.SlateRenderer;

            GeomMesh = rc.CreateGeomMesh();
            GeomMesh.SetAtomNum(1);
            PrimitiveMesh = new Graphics.Mesh.TtMeshPrimitives();
            PrimitiveMesh.mCoreObject.Init(rc.mCoreObject, GeomMesh.mCoreObject, new BoundingBox());
            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            PrimitiveMesh.PushAtom(0, in dpDesc);

            var shaderProg = renderer.SlateEffect.ShaderEffect;
            Drawcall = rc.CreateGraphicDraw();
            Drawcall.BindShaderEffect(renderer.SlateEffect);
            Drawcall.BindGeomMesh(GeomMesh);

            var cbBinder = shaderProg.FindBinder("ProjectionMatrixBuffer");
            FontCBuffer = rc.CreateCBV(cbBinder);

            Drawcall.BindCBuffer(cbBinder.mCoreObject, FontCBuffer);

            var smp = shaderProg.FindBinder("Samp_FontTexture");
            Drawcall.BindSampler(smp.mCoreObject, renderer.SamplerState);

            FontTextureBindInfo = shaderProg.FindBinder("FontTexture");

            {
                var pipeDesc = new NxRHI.FGpuPipelineDesc();
                pipeDesc.SetDefault();

                ref var rstDesc = ref pipeDesc.m_Rasterizer;
                rstDesc.ScissorEnable = 1;
                rstDesc.FillMode = NxRHI.EFillMode.FMD_SOLID;
                rstDesc.CullMode = NxRHI.ECullMode.CMD_NONE;

                ref var dssDesc = ref pipeDesc.m_DepthStencil;
                dssDesc.DepthEnable = 0;
                dssDesc.DepthWriteMask = NxRHI.EDepthWriteMask.DSWM_ZERO;
                dssDesc.DepthFunc = NxRHI.EComparisionMode.CMP_ALWAYS;

                ref var bldDesc = ref pipeDesc.m_Blend;
                var pRenderTarget = bldDesc.RenderTarget;
                pRenderTarget[0].SetDefault();
                pRenderTarget[0].SrcBlendAlpha = NxRHI.EBlend.BLD_INV_SRC_ALPHA;
                pRenderTarget[0].DestBlendAlpha = NxRHI.EBlend.BLD_ONE;
                pRenderTarget[0].BlendEnable = 1;
                var pipeline = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in pipeDesc);
                var count = pipeline.UnsafeRefCount;
                Drawcall.BindPipeline(pipeline);

                //Pipeline.mCoreObject.GetGpuProgram().BindInputLayout(renderer.InputLayout.mCoreObject);
            }
            return true;
        }
        public void Dispose()
        {
            DataVB.Dispose();
            DataIB.Dispose();

            CoreSDK.DisposeObject(ref FontCBuffer);
            CoreSDK.DisposeObject(ref Drawcall);
            CoreSDK.DisposeObject(ref CmdList);
            CoreSDK.DisposeObject(ref VertexBuffer);
            CoreSDK.DisposeObject(ref IndexBuffer);
            CoreSDK.DisposeObject(ref GeomMesh);
        }

        [ThreadStatic]
        private static Profiler.TimeScope ScopeRenderImDrawData = Profiler.TimeScopeManager.GetTimeScope(typeof(UImDrawDataRHI), nameof(RenderImDrawData));
        public unsafe static void RenderImDrawData(ref ImDrawData draw_data, Graphics.Pipeline.UPresentWindow presentWindow, UImDrawDataRHI rhiData)
        {
            using (new Profiler.TimeScopeHelper(ScopeRenderImDrawData))
            {
                RenderImDrawDataImpl(ref draw_data, presentWindow, rhiData);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeUpdateBuffer = Profiler.TimeScopeManager.GetTimeScope(typeof(UImDrawDataRHI), "RenderImDrawData.UpdateBuffer");
        [ThreadStatic]
        private static Profiler.TimeScope ScopeDrawPass = Profiler.TimeScopeManager.GetTimeScope(typeof(UImDrawDataRHI), "RenderImDrawData.DrawPass");
        private unsafe static void RenderImDrawDataImpl(ref ImDrawData draw_data, Graphics.Pipeline.UPresentWindow presentWindow, UImDrawDataRHI rhiData)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var drawCmd = rhiData.CmdList.mCoreObject;
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (rhiData.VertexBuffer == null || totalVBSize > rhiData.VertexBuffer.mCoreObject.Desc.Size)
            {
                rhiData.VertexBuffer?.Dispose();
                //var vbDesc = new NxRHI.FBufferDesc();
                //vbDesc.SetDefault();
                //vbDesc.Type = NxRHI.EBufferType.BFT_Vertex;
                //vbDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                //vbDesc.StructureStride = (uint)sizeof(ImDrawVert);
                //vbDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                //vbDesc.Size = (uint)(totalVBSize * 1.5f);
                //vbDesc.StructureStride = (uint)sizeof(ImDrawVert);
                var vbDesc = new NxRHI.FVbvDesc();
                vbDesc.m_Size = (uint)(totalVBSize * 1.5f);
                vbDesc.m_Stride = (uint)sizeof(ImDrawVert);
                vbDesc.m_CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                vbDesc.m_Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                rhiData.VertexBuffer = rc.CreateVBV(null, in vbDesc);
                rhiData.GeomMesh.mCoreObject.GetVertexArray().BindVB(NxRHI.EVertexStreamType.VST_Position, rhiData.VertexBuffer.mCoreObject);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (rhiData.IndexBuffer == null || totalIBSize > rhiData.IndexBuffer.mCoreObject.Desc.Size)
            {
                rhiData.IndexBuffer?.Dispose();
                var ibDesc = new NxRHI.FIbvDesc();
                ibDesc.m_Size = (uint)(totalIBSize * 1.5f);
                ibDesc.m_Stride = (uint)sizeof(ushort);
                ibDesc.m_CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                ibDesc.m_Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                rhiData.IndexBuffer = rc.CreateIBV(null, in ibDesc);
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }

            using (new Profiler.TimeScopeHelper(ScopeUpdateBuffer))
            {
                rhiData.DataVB.Clear(false);
                rhiData.DataIB.Clear(false);
                for (int i = 0; i < draw_data.CmdListsCount; i++)
                {
                    var cmd_list = new ImDrawList(draw_data.CmdLists[i]);

                    rhiData.DataVB.Append(cmd_list.VtxBufferData, cmd_list.VtxBufferSize);
                    rhiData.DataIB.Append(cmd_list.IdxBufferData, cmd_list.IdxBufferSize);

                    vertexOffsetInVertices += (uint)cmd_list.VtxBufferSize;
                    indexOffsetInElements += (uint)cmd_list.IdxBufferSize;
                }

                rhiData.VertexBuffer.UpdateGpuData(0,
                    rhiData.DataVB.UnsafeAddressAt(0).ToPointer(), (uint)(vertexOffsetInVertices * sizeof(ImDrawVert)));

                rhiData.IndexBuffer.UpdateGpuData(0,
                    rhiData.DataIB.UnsafeAddressAt(0).ToPointer(), (uint)(indexOffsetInElements * sizeof(ushort)));

                rhiData.GeomMesh.mCoreObject.GetVertexArray().BindVB(NxRHI.EVertexStreamType.VST_Position, rhiData.VertexBuffer.mCoreObject);
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }

            presentWindow.BeginFrame();

            using (new Profiler.TimeScopeHelper(ScopeDrawPass))
            {
                // Setup orthographic projection matrix into our constant buffer
                var io = ImGuiAPI.GetIO();
                float L = draw_data.DisplayPos.X;
                float R = draw_data.DisplayPos.X + draw_data.DisplaySize.X;
                float T = draw_data.DisplayPos.Y;
                float B = draw_data.DisplayPos.Y + draw_data.DisplaySize.Y;
                var mvp = Matrix.CreateOrthographicOffCenter(L,
                    R,
                    B,
                    T,
                    -1.0f,
                    1.0f);

                rhiData.FontCBuffer.SetValue("ProjectionMatrix", in mvp);

                var fb_scale = io.DisplayFramebufferScale;
                draw_data.ScaleClipRects(in fb_scale);

                drawCmd.BeginCommand();
                {
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));

                    var swapChain = presentWindow.SwapChain;
                    if (drawCmd.BeginPass(swapChain.BeginFrameBuffers(drawCmd).mCoreObject, in passClears, "ImGui"))
                    {
                        if (swapChain.Viewport.Width != 0 && swapChain.Viewport.Height != 0)
                            drawCmd.SetViewport(swapChain.Viewport);

                        // Render command lists
                        int vtx_offset = 0;
                        int idx_offset = 0;
                        Vector2 clip_off = draw_data.DisplayPos;
                        for (int n = 0; n < draw_data.CmdListsCount; n++)
                        {
                            NxRHI.UGraphicDraw drawcall = null;
                            var cmd_list = new ImDrawList(draw_data.CmdLists[n]);
                            for (int cmd_i = 0; cmd_i < cmd_list.CmdBufferSize; cmd_i++)
                            {
                                ImDrawCmd* pcmd = &cmd_list.CmdBufferData[cmd_i];
                                if (pcmd->UserCallback != null)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)pcmd->TextureId);
                                    if (handle.IsAllocated)
                                    {
                                        drawcall = rhiData.Drawcall;
                                        var rsv = handle.Target as NxRHI.USrView;
                                        if (rsv != null)
                                        {
                                            drawcall.BindSRV(rhiData.FontTextureBindInfo.mCoreObject, rsv);
                                        }
                                        else
                                        {
                                            var parameters = handle.Target as TtImDrawCmdParameters;
                                            if (parameters != null)
                                            {
                                                drawcall = parameters.Drawcall;
                                                drawcall.BindGeomMesh(rhiData.GeomMesh);
                                                parameters.OnDraw(in mvp);
                                            }
                                        }
                                    }
                                }

                                var ScissorRect = new NxRHI.FScissorRect();
                                ScissorRect.m_MinX = (int)(pcmd->ClipRect.X - clip_off.X);
                                ScissorRect.m_MinY = (int)(pcmd->ClipRect.Y - clip_off.Y);
                                ScissorRect.m_MaxX = (int)(pcmd->ClipRect.Z - clip_off.X);
                                ScissorRect.m_MaxY = (int)(pcmd->ClipRect.W - clip_off.Y);
                                //rhiData.ScissorRect.SetSCRect(0,
                                //    (int)(pcmd->ClipRect.X - clip_off.X),
                                //    (int)(pcmd->ClipRect.Y - clip_off.Y),
                                //    (int)(pcmd->ClipRect.Z - clip_off.X),
                                //    (int)(pcmd->ClipRect.W - clip_off.Y));
                                drawCmd.SetScissor(in ScissorRect);

                                var dpDesc = new NxRHI.FMeshAtomDesc();
                                dpDesc.SetDefault();
                                dpDesc.m_BaseVertexIndex = (uint)(vtx_offset + pcmd->VtxOffset);
                                dpDesc.m_StartIndex = (uint)(idx_offset + pcmd->IdxOffset);
                                dpDesc.m_NumPrimitives = pcmd->ElemCount / 3;
                                rhiData.PrimitiveMesh.mCoreObject.SetAtom(0, 0, in dpDesc);
                                if (ScissorRect.m_MinX >= ScissorRect.m_MaxX || ScissorRect.m_MinY >= ScissorRect.m_MaxY)
                                {

                                }
                                else
                                {
                                    drawCmd.DirectGpuDraw(drawcall.mCoreObject.NativeSuper);
                                }
                            }
                            idx_offset += (int)cmd_list.IdxBufferSize;
                            vtx_offset += cmd_list.VtxBufferSize;
                        }

                        drawCmd.EndPass();
                    }

                    NxRHI.FScissorRect fullRect;
                    var fwSize = presentWindow.GetWindowSize();
                    fullRect.m_MinX = 0;
                    fullRect.m_MinY = 0;
                    fullRect.m_MaxX = (int)fwSize.X;
                    fullRect.m_MaxY = (int)fwSize.X;
                    drawCmd.SetScissor(in fullRect);
                    swapChain.EndFrameBuffers(drawCmd);
                }
                drawCmd.EndCommand();

                rc.GpuQueue.ExecuteCommandList(drawCmd);
            }
            
            presentWindow.EndFrame();
        }
    }
}
