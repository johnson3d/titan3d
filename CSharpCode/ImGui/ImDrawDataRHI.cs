using System;
using System.Collections.Generic;
using System.Reflection;
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
        public NxRHI.TtGraphicDraw Drawcall;
        private System.Runtime.InteropServices.GCHandle NativeHandle;
        public unsafe void* GetHandle()
        {
            return System.Runtime.InteropServices.GCHandle.ToIntPtr(NativeHandle).ToPointer();
        }
        public virtual void OnDraw(in Matrix mvp)
        {

        }
    }

    public class TtImDrawDataRHI : IDisposable
    {
        const int ImTextureStatus_OK = 0;
        const int ImTextureStatus_Destroyed = 1;
        const int ImTextureStatus_WantCreate = 2;
        const int ImTextureStatus_WantUpdates = 3;
        const int ImTextureStatus_WantDestroy = 4;
        const int ImTextureFormat_RGBA32 = 0;
        const int ImTextureFormat_Alpha8 = 1;

        sealed class ImGuiTextureBinding : IDisposable
        {
            public NxRHI.TtTexture Texture;
            public NxRHI.TtSrView SRV;

            private System.Runtime.InteropServices.GCHandle mHandle;
            public IntPtr HandlePtr { get; private set; }

            public ImGuiTextureBinding()
            {
                mHandle = System.Runtime.InteropServices.GCHandle.Alloc(this);
                HandlePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(mHandle);
            }

            public void SetResources(NxRHI.TtTexture texture, NxRHI.TtSrView srv)
            {
                CoreSDK.DisposeObject(ref SRV);
                CoreSDK.DisposeObject(ref Texture);
                Texture = texture;
                SRV = srv;
            }

            public void Dispose()
            {
                SetResources(null, null);
                if (mHandle.IsAllocated)
                {
                    mHandle.Free();
                    HandlePtr = IntPtr.Zero;
                }
            }
        }

        static readonly List<ImGuiTextureBinding> mImGuiTextureBindings = new List<ImGuiTextureBinding>();

        public NxRHI.TtEffectBinder SlateCBufferBindInfo;
        public NxRHI.TtEffectBinder SlateTextureBindInfo;
        public NxRHI.TtEffectBinder SlateSamplerBindInfo;
        public NxRHI.TtCbView SlateCBuffer;
        public NxRHI.TtGpuPipeline Pipeline;

        public NxRHI.TtGeomMesh GeomMesh;
        public Graphics.Mesh.TtMeshPrimitives PrimitiveMesh;

        public List<NxRHI.TtGraphicDraw> Drawcalls = new List<NxRHI.TtGraphicDraw>();
        public int UsedDrawcall = 0;
        public NxRHI.TtGraphicDraw CreateGraphicDraw()
        {
            if (UsedDrawcall == Drawcalls.Count)
            {
                var rc = TtEngine.Instance.GfxDevice.RenderContext;
                var renderer = TtEngine.Instance.GfxDevice.SlateRenderer;
                var shaderProg = renderer.SlateEffect.ShaderEffect;
                var result = rc.CreateGraphicDraw();
                result.BindShaderEffect(renderer.SlateEffect, renderer.SlateEffect.ShadingEnv);
                result.BindGeomMesh(GeomMesh);
                result.BindCBV(SlateCBufferBindInfo, SlateCBuffer);
                result.BindSampler(SlateSamplerBindInfo, renderer.SamplerState);
                result.BindPipeline(Pipeline);

                Drawcalls.Add(result);
            }

            var dc = Drawcalls[UsedDrawcall++];
            return dc;
        }
        public void ResetGraphicDraw()
        {
            UsedDrawcall = 0;
        }
        #region TriangleData
        public NxRHI.TtVbView VertexBuffer;
        public NxRHI.TtIbView IndexBuffer;
        private NxRHI.TtVbView[] mFrameVertexBuffers;
        private NxRHI.TtIbView[] mFrameIndexBuffers;
        public Support.TtNativeArray<ImDrawVert> DataVB = Support.TtNativeArray<ImDrawVert>.CreateInstance();
        public Support.TtNativeArray<ushort> DataIB = Support.TtNativeArray<ushort>.CreateInstance();
        #endregion
        public unsafe bool InitializeGraphics(EPixelFormat format, EPixelFormat dsFormat)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            var renderer = TtEngine.Instance.GfxDevice.SlateRenderer;

            GeomMesh = rc.CreateGeomMesh();
            GeomMesh.SetAtomNum(1);
            PrimitiveMesh = new Graphics.Mesh.TtMeshPrimitives();
            PrimitiveMesh.mCoreObject.Init(rc.mCoreObject, GeomMesh.mCoreObject, new BoundingBox());
            var dpDesc = new NxRHI.FMeshAtomDesc();
            dpDesc.SetDefault();
            PrimitiveMesh.PushAtom(0, in dpDesc);

            var shaderProg = renderer.SlateEffect.ShaderEffect;

            SlateCBufferBindInfo = shaderProg.FindBinder("ProjectionMatrixBuffer");
            SlateCBuffer = rc.CreateCBV(SlateCBufferBindInfo);
            SlateTextureBindInfo = shaderProg.FindBinder("FontTexture");
            SlateSamplerBindInfo = shaderProg.FindBinder("Samp_FontTexture");

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
                Pipeline = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in pipeDesc);

                //Pipeline.mCoreObject.GetGpuProgram().BindInputLayout(renderer.InputLayout.mCoreObject);
            }
            return true;
        }
        public void Dispose()
        {
            foreach (var i in Drawcalls)
            {
                i.Dispose();
            }
            Drawcalls.Clear();
            DataVB.Dispose();
            DataIB.Dispose();
            DisposeFrameBuffers();

            CoreSDK.DisposeObject(ref SlateCBuffer);
            CoreSDK.DisposeObject(ref GeomMesh);
            CoreSDK.DisposeObject(ref PrimitiveMesh);
        }

        private void DisposeFrameBuffers()
        {
            if (mFrameVertexBuffers != null)
            {
                for (int i = 0; i < mFrameVertexBuffers.Length; i++)
                {
                    CoreSDK.DisposeObject(ref mFrameVertexBuffers[i]);
                }
                mFrameVertexBuffers = null;
            }

            if (mFrameIndexBuffers != null)
            {
                for (int i = 0; i < mFrameIndexBuffers.Length; i++)
                {
                    CoreSDK.DisposeObject(ref mFrameIndexBuffers[i]);
                }
                mFrameIndexBuffers = null;
            }

            VertexBuffer = null;
            IndexBuffer = null;
        }

        private void EnsureFrameBufferSlots(int frameBufferCount)
        {
            frameBufferCount = Math.Max(frameBufferCount, 1);
            if (mFrameVertexBuffers?.Length == frameBufferCount && mFrameIndexBuffers?.Length == frameBufferCount)
                return;

            DisposeFrameBuffers();
            mFrameVertexBuffers = new NxRHI.TtVbView[frameBufferCount];
            mFrameIndexBuffers = new NxRHI.TtIbView[frameBufferCount];
        }

        public static void DisposeAllImGuiTextureBindings()
        {
            for (int i = 0; i < mImGuiTextureBindings.Count; i++)
            {
                mImGuiTextureBindings[i].Dispose();
            }
            mImGuiTextureBindings.Clear();
        }

        [ThreadStatic]
        private static Profiler.TimeScope mScopeRenderImDrawData;
        private static Profiler.TimeScope ScopeRenderImDrawData
        {
            get
            {
                if (mScopeRenderImDrawData == null)
                    mScopeRenderImDrawData = new Profiler.TimeScope(typeof(TtImDrawDataRHI), nameof(RenderImDrawData));
                return mScopeRenderImDrawData;
            }
        }
        public unsafe static void RenderImDrawData(ref ImDrawData draw_data, Graphics.Pipeline.TtPresentWindow presentWindow, TtImDrawDataRHI rhiData)
        {
            using (new Profiler.TimeScopeHelper(ScopeRenderImDrawData))
            {
                RenderImDrawDataImpl(ref draw_data, presentWindow, rhiData);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeUpdateBuffer;
        private static Profiler.TimeScope ScopeUpdateBuffer
        {
            get
            {
                if (mScopeUpdateBuffer == null)
                    mScopeUpdateBuffer = new Profiler.TimeScope(typeof(TtImDrawDataRHI), "RenderImDrawData.UpdateBuffer");
                return mScopeUpdateBuffer;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeDrawPass;
        private static Profiler.TimeScope ScopeDrawPass
        {
            get
            {
                if (mScopeDrawPass == null)
                    mScopeDrawPass = new Profiler.TimeScope(typeof(TtImDrawDataRHI), "RenderImDrawData.DrawPass");
                return mScopeDrawPass;
            }
        }
        private unsafe static void RenderImDrawDataImpl(ref ImDrawData draw_data, Graphics.Pipeline.TtPresentWindow presentWindow, TtImDrawDataRHI rhiData)
        {
            if (presentWindow?.CanRender != true)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var drawCmd = rc.CmdListManager.GetCmdList();
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            ProcessImGuiTextureRequests(ref draw_data);

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            presentWindow.BeginFrame();

            var swapChain = presentWindow.SwapChain;
            var frameBufferCount = Math.Max((int)swapChain.BackBufferCount, 1);
            rhiData.EnsureFrameBufferSlots(frameBufferCount);
            var frameBufferIndex = (int)(swapChain.CurrentBackBuffer % (uint)frameBufferCount);
            rhiData.VertexBuffer = rhiData.mFrameVertexBuffers[frameBufferIndex];
            rhiData.IndexBuffer = rhiData.mFrameIndexBuffers[frameBufferIndex];

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (rhiData.VertexBuffer == null || totalVBSize > rhiData.VertexBuffer.mCoreObject.Desc.Size)
            {
                CoreSDK.DisposeObject(ref rhiData.mFrameVertexBuffers[frameBufferIndex]);
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
                rhiData.mFrameVertexBuffers[frameBufferIndex] = rc.CreateVBV(null, in vbDesc);
                rhiData.VertexBuffer = rhiData.mFrameVertexBuffers[frameBufferIndex];
                rhiData.GeomMesh.mCoreObject.GetVertexArray().BindVB(NxRHI.EVertexStreamType.VST_Position, rhiData.VertexBuffer.mCoreObject);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (rhiData.IndexBuffer == null || totalIBSize > rhiData.IndexBuffer.mCoreObject.Desc.Size)
            {
                CoreSDK.DisposeObject(ref rhiData.mFrameIndexBuffers[frameBufferIndex]);
                var ibDesc = new NxRHI.FIbvDesc();
                ibDesc.m_Size = (uint)(totalIBSize * 1.5f);
                ibDesc.m_Stride = (uint)sizeof(ushort);
                ibDesc.m_CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                ibDesc.m_Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                rhiData.mFrameIndexBuffers[frameBufferIndex] = rc.CreateIBV(null, in ibDesc);
                rhiData.IndexBuffer = rhiData.mFrameIndexBuffers[frameBufferIndex];
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }

            using (new Profiler.TimeScopeHelper(ScopeUpdateBuffer))
            {
                rhiData.DataVB.Clear(false);
                rhiData.DataIB.Clear(false);
                for (int i = 0; i < draw_data.CmdListsCount; i++)
                {
                    var cmd_list = new ImDrawList(draw_data.GetCmdLists()[i]);

                    rhiData.DataVB.Append(cmd_list.VtxBufferData, cmd_list.VtxBufferSize);
                    rhiData.DataIB.Append(cmd_list.IdxBufferData, cmd_list.IdxBufferSize);

                    vertexOffsetInVertices += (uint)cmd_list.VtxBufferSize;
                    indexOffsetInElements += (uint)cmd_list.IdxBufferSize;
                }

                rhiData.VertexBuffer.UpdateGpuData(drawCmd.mCoreObject, 0,
                    rhiData.DataVB.UnsafeAddressAt(0).ToPointer(), (uint)(vertexOffsetInVertices * sizeof(ImDrawVert)));

                rhiData.IndexBuffer.UpdateGpuData(drawCmd.mCoreObject, 0,
                    rhiData.DataIB.UnsafeAddressAt(0).ToPointer(), (uint)(indexOffsetInElements * sizeof(ushort)));

                rhiData.GeomMesh.mCoreObject.GetVertexArray().BindVB(NxRHI.EVertexStreamType.VST_Position, rhiData.VertexBuffer.mCoreObject);
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }

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

                rhiData.SlateCBuffer.SetValue("ProjectionMatrix", in mvp);

                var fb_scale = io.DisplayFramebufferScale;
                draw_data.ScaleClipRects(in fb_scale);

                rhiData.ResetGraphicDraw();
                drawCmd.BeginCommand();
                {
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));

                    drawCmd.mCoreObject.mIsDirectGpuDraw = true;

                    if (drawCmd.BeginPass(swapChain.BeginFrameBuffers(drawCmd), in passClears, "ImGui"))
                    {
                        if (swapChain.Viewport.Width != 0 && swapChain.Viewport.Height != 0)
                        {
                            drawCmd.SetViewport(swapChain.Viewport);
                        }

                        // Render command lists
                        int vtx_offset = 0;
                        int idx_offset = 0;
                        Vector2 clip_off = draw_data.DisplayPos;
                        for (int n = 0; n < draw_data.CmdListsCount; n++)
                        {
                            var cmd_list = new ImDrawList(draw_data.GetCmdLists()[n]);
                            for (int cmd_i = 0; cmd_i < cmd_list.CmdBufferSize; cmd_i++)
                            {
                                NxRHI.TtGraphicDraw drawcall = null;
                                ImDrawCmd* pcmd = &cmd_list.CmdBufferData[cmd_i];
                                TtImDrawCmdParameters parameters = null;
                                if (pcmd->UserCallback != null)
                                {
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    var texId = pcmd->GetTexID();
                                    if (texId != 0)
                                    {
                                        var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)texId);
                                        if (handle.IsAllocated)
                                        {
                                            var rsv = handle.Target as NxRHI.TtSrView;
                                            if (rsv != null)
                                            {
                                                drawcall = rhiData.CreateGraphicDraw();
                                                drawcall.BindSRV(rhiData.SlateTextureBindInfo.mCoreObject, rsv);
                                            }
                                            else
                                            {
                                                var textureBinding = handle.Target as ImGuiTextureBinding;
                                                if (textureBinding != null)
                                                {
                                                    drawcall = rhiData.CreateGraphicDraw();
                                                    drawcall.BindSRV(rhiData.SlateTextureBindInfo.mCoreObject, textureBinding.SRV);
                                                }
                                                else
                                                {
                                                    parameters = handle.Target as TtImDrawCmdParameters;
                                                    if (parameters != null)
                                                    {
                                                        drawcall = parameters.Drawcall;
                                                        drawcall.BindGeomMesh(rhiData.GeomMesh);
                                                        parameters.OnDraw(in mvp);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (drawcall == null)
                                    {
                                        drawcall = rhiData.CreateGraphicDraw();
                                        drawcall.BindSRV(rhiData.SlateTextureBindInfo.mCoreObject, null);
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
                                    drawCmd.DirectGpuDraw(drawcall);
                                }
                            }
                            idx_offset += (int)cmd_list.IdxBufferSize;
                            vtx_offset += cmd_list.VtxBufferSize;
                        }

                        drawCmd.EndPass();
                    }

                    drawCmd.mCoreObject.mIsDirectGpuDraw = false;
                    NxRHI.FScissorRect fullRect;
                    var fwSize = presentWindow.GetWindowSize();
                    fullRect.m_MinX = 0;
                    fullRect.m_MinY = 0;
                    fullRect.m_MaxX = (int)fwSize.X;
                    fullRect.m_MaxY = (int)fwSize.Y;
                    drawCmd.SetScissor(in fullRect);
                    swapChain.EndFrameBuffers(drawCmd);
                }
                drawCmd.EndCommand();

                rc.GpuQueue.ExecuteCommandList(drawCmd);
            }
            
            presentWindow.EndFrame();
        }

        private unsafe static void ProcessImGuiTextureRequests(ref ImDrawData drawData)
        {
            var drawDataPtr = (ImDrawData*)Unsafe.AsPointer(ref drawData);
            int textureCount = ImGuiAPI.DrawData_Textures_Size(drawDataPtr);
            for (int i = 0; i < textureCount; i++)
            {
                void* texture = ImGuiAPI.DrawData_Textures_Get(drawDataPtr, i);
                ProcessImGuiTextureRequest(texture);
            }
        }

        private unsafe static void ProcessImGuiTextureRequest(void* texture)
        {
            if (texture == null)
                return;

            var status = ImGuiAPI.ImTextureData_GetStatus(texture);
            switch (status)
            {
                case ImTextureStatus_WantCreate:
                case ImTextureStatus_WantUpdates:
                    UploadImGuiTexture(texture);
                    break;
                case ImTextureStatus_WantDestroy:
                    DestroyImGuiTexture(texture);
                    break;
                case ImTextureStatus_Destroyed:
                    if ((IntPtr)ImGuiAPI.ImTextureData_GetBackendUserData(texture) != IntPtr.Zero)
                        DestroyImGuiTexture(texture);
                    break;
            }
        }

        private unsafe static ImGuiTextureBinding GetImGuiTextureBinding(void* texture)
        {
            var backendData = (IntPtr)ImGuiAPI.ImTextureData_GetBackendUserData(texture);
            if (backendData == IntPtr.Zero)
                return null;

            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(backendData);
            return handle.IsAllocated ? handle.Target as ImGuiTextureBinding : null;
        }

        private unsafe static void UploadImGuiTexture(void* texture)
        {
            NxRHI.TtTexture newTexture;
            NxRHI.TtSrView newSrv;
            if (CreateImGuiTextureResources(texture, out newTexture, out newSrv) == false)
                return;

            var binding = GetImGuiTextureBinding(texture);
            if (binding == null)
            {
                binding = new ImGuiTextureBinding();
                mImGuiTextureBindings.Add(binding);
                ImGuiAPI.ImTextureData_SetBackendUserData(texture, binding.HandlePtr.ToPointer());
                ImGuiAPI.ImTextureData_SetTexID(texture, (ulong)binding.HandlePtr);
            }

            binding.SetResources(newTexture, newSrv);
            ImGuiAPI.ImTextureData_SetStatus(texture, ImTextureStatus_OK);
        }

        private unsafe static bool CreateImGuiTextureResources(void* texture, out NxRHI.TtTexture resultTexture, out NxRHI.TtSrView resultSrv)
        {
            resultTexture = null;
            resultSrv = null;

            int width = ImGuiAPI.ImTextureData_GetWidth(texture);
            int height = ImGuiAPI.ImTextureData_GetHeight(texture);
            int format = ImGuiAPI.ImTextureData_GetFormat(texture);
            int pitch = ImGuiAPI.ImTextureData_GetPitch(texture);
            byte* pixels = (byte*)ImGuiAPI.ImTextureData_GetPixels(texture);
            if (width <= 0 || height <= 0 || pixels == null)
                return false;

            if (format == ImTextureFormat_RGBA32)
            {
                CreateImGuiRgbaTextureResources(pixels, (uint)pitch, width, height, out resultTexture, out resultSrv);
                return resultTexture != null && resultSrv != null;
            }

            if (format == ImTextureFormat_Alpha8)
            {
                var rgba = new byte[width * height * 4];
                for (int y = 0; y < height; y++)
                {
                    byte* src = pixels + y * pitch;
                    int dstOffset = y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        byte alpha = src[x];
                        rgba[dstOffset + 0] = 255;
                        rgba[dstOffset + 1] = 255;
                        rgba[dstOffset + 2] = 255;
                        rgba[dstOffset + 3] = alpha;
                        dstOffset += 4;
                    }
                }

                fixed (byte* rgbaPtr = rgba)
                {
                    CreateImGuiRgbaTextureResources(rgbaPtr, (uint)(width * 4), width, height, out resultTexture, out resultSrv);
                }
                return resultTexture != null && resultSrv != null;
            }

            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(
                Profiler.ELogTag.Warning,
                $"Unsupported ImGui texture format: {format}");
            return false;
        }

        private unsafe static void CreateImGuiRgbaTextureResources(void* pixels, uint rowPitch, int width, int height, out NxRHI.TtTexture texture, out NxRHI.TtSrView srv)
        {
            var initData = new NxRHI.FMappedSubResource();
            initData.pData = pixels;
            initData.RowPitch = rowPitch;
            initData.DepthPitch = rowPitch * (uint)height;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var txDesc = new NxRHI.FTextureDesc();
            txDesc.SetDefault();
            txDesc.Width = (uint)width;
            txDesc.Height = (uint)height;
            txDesc.MipLevels = 1;
            txDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            txDesc.InitData = &initData;
            texture = rc.CreateTexture(in txDesc);
            if (texture == null)
            {
                srv = null;
                return;
            }

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
            srvDesc.Format = txDesc.Format;
            srvDesc.Texture2D.MipLevels = 1;
            srv = rc.CreateSRV(texture, in srvDesc);
        }

        private unsafe static void DestroyImGuiTexture(void* texture)
        {
            var binding = GetImGuiTextureBinding(texture);
            if (binding != null)
            {
                mImGuiTextureBindings.Remove(binding);
                binding.Dispose();
            }

            ImGuiAPI.ImTextureData_SetTexID(texture, 0);
            ImGuiAPI.ImTextureData_SetBackendUserData(texture, null);
            ImGuiAPI.ImTextureData_SetStatus(texture, ImTextureStatus_Destroyed);
        }
    }
}
