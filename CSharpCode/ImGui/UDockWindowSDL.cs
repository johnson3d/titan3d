using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EngineNS.Graphics.Pipeline;
using SDL2;

namespace EngineNS.EGui
{
    public class UDockWindowSDL
    {
        #region SDL
        public static unsafe void ImGui_ImplSDL2_UpdateMonitors()
        {
            var platform_io = ImGuiAPI.GetPlatformIO();
            ImGuiAPI.PlatformIO_Monitor_Resize(platform_io, 0);
            int display_count = SDL.SDL_GetNumVideoDisplays();
            for (int n = 0; n < display_count; n++)
            {
                // Warning: the validity of monitor DPI information on Windows depends on the application DPI awareness settings, which generally needs to be set in the manifest or at runtime.
                ImGuiPlatformMonitor monitor = new ImGuiPlatformMonitor();
                monitor.UnsafeCallConstructor();
                SDL.SDL_Rect r;
                SDL.SDL_GetDisplayBounds(n, out r);
                monitor.MainPos = monitor.WorkPos = new Vector2((float)r.x, (float)r.y);
                monitor.MainSize = monitor.WorkSize = new Vector2((float)r.w, (float)r.h);
                SDL.SDL_GetDisplayUsableBounds(n, out r);
                monitor.WorkPos = new Vector2((float)r.x, (float)r.y);
                monitor.WorkSize = new Vector2((float)r.w, (float)r.h);
                float dpi = 0.0f;
                float hdpi, vdpi;
                if (SDL.SDL_GetDisplayDPI(n, out dpi, out hdpi, out vdpi) != 0)
                    monitor.DpiScale = dpi / 96.0f;
                ImGuiAPI.PlatformIO_Monitor_PushBack(platform_io, monitor);
            }
        }
        public static unsafe void ImGui_ImplSDL2_InitPlatformInterface()
        {
            // Register platform interface (will be coupled with a renderer interface)
            var platform_io = ImGuiAPI.GetPlatformIO();
            platform_io.Platform_CreateWindow = (ImGui_ImplSDL2_CreateWindow);
            platform_io.Platform_DestroyWindow = (ImGui_ImplSDL2_DestroyWindow);
            platform_io.Platform_ShowWindow = (ImGui_ImplSDL2_ShowWindow);
            platform_io.Platform_SetWindowPos = (ImGui_ImplSDL2_SetWindowPos);
            platform_io.Platform_GetWindowPos = (ImGui_ImplSDL2_GetWindowPos);
            platform_io.Platform_SetWindowSize = (ImGui_ImplSDL2_SetWindowSize);
            platform_io.Platform_GetWindowSize = (ImGui_ImplSDL2_GetWindowSize);
            platform_io.Platform_SetWindowFocus = (ImGui_ImplSDL2_SetWindowFocus);
            platform_io.Platform_GetWindowFocus = (ImGui_ImplSDL2_GetWindowFocus);
            platform_io.Platform_GetWindowMinimized = (ImGui_ImplSDL2_GetWindowMinimized);
            platform_io.Platform_SetWindowTitle = (ImGui_ImplSDL2_SetWindowTitle);
            platform_io.Platform_RenderWindow = (ImGui_ImplSDL2_RenderWindow);
            platform_io.Platform_SwapBuffers = (ImGui_ImplSDL2_SwapBuffers);
            platform_io.Platform_SetWindowAlpha = (ImGui_ImplSDL2_SetWindowAlpha);

            platform_io.Renderer_CreateWindow = (ImGui_Renderer_CreateWindow);
            platform_io.Renderer_DestroyWindow = (ImGui_Renderer_DestroyWindow);
            platform_io.Renderer_SetWindowSize = (ImGui_Renderer_SetWindowSize);
            platform_io.Renderer_RenderWindow = (ImGui_Renderer_RenderWindow);
            platform_io.Renderer_SwapBuffers = (ImGui_Renderer_SwapBuffers);
            
            //platform_io.Platform_CreateVkSurface = ImGui_ImplSDL2_CreateVkSurface;

            // SDL2 by default doesn't pass mouse clicks to the application when the click focused a window. This is getting in the way of our interactions and we disable that behavior.
            SDL.SDL_SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        }
        public static unsafe ImGuiIO.FDelegate_GetClipboardTextFn ImGui_ImplSDL2_GetClipboardText = ImGui_ImplSDL2_GetClipboardText_Impl;
        static unsafe sbyte* ImGui_ImplSDL2_GetClipboardText_Impl(void* dummy)
        {
            if (g_ClipboardTextData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(g_ClipboardTextData);
            }
            var text = SDL.SDL_GetClipboardText();
            g_ClipboardTextData = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(text);
            return (sbyte*)g_ClipboardTextData.ToPointer();
        }
        unsafe static IntPtr g_ClipboardTextData;
        public static unsafe ImGuiIO.FDelegate_SetClipboardTextFn ImGui_ImplSDL2_SetClipboardText = ImGui_ImplSDL2_SetClipboardText_Impl;
        static unsafe void ImGui_ImplSDL2_SetClipboardText_Impl(void* dummy, sbyte* text)
        {
            SDL.SDL_SetClipboardText(System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)text));
        }
        static bool[] g_MousePressed = new bool[]{ false, false, false };
        public static unsafe bool ImGui_ImplSDL2_ProcessEvent(in SDL.SDL_Event ev)
        {
            var io = ImGuiAPI.GetIO();
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    {
                        if (ev.wheel.x > 0) io.MouseWheelH += 1;
                        if (ev.wheel.x < 0) io.MouseWheelH -= 1;
                        if (ev.wheel.y > 0) io.MouseWheel += 1;
                        if (ev.wheel.y < 0) io.MouseWheel -= 1;
                        return true;
                    }
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        if (ev.button.button == SDL.SDL_BUTTON_LEFT) g_MousePressed[0] = true;
                        if (ev.button.button == SDL.SDL_BUTTON_RIGHT) g_MousePressed[1] = true;
                        if (ev.button.button == SDL.SDL_BUTTON_MIDDLE) g_MousePressed[2] = true;
                        return true;
                    }
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    {
                        var text = ev.text;
                        var pText = (sbyte*)text.text;
                        io.AddInputCharactersUTF8(pText);
                        return true;
                    }
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    {
                        var key = (int)ev.key.keysym.scancode;
                        bool* keysDown = (bool*)&io.UnsafeAsLayout->KeysDown;
                        //IM_ASSERT(key >= 0 && key<IM_ARRAYSIZE(io.KeysDown));
                        keysDown[key] = (ev.type == SDL.SDL_EventType.SDL_KEYDOWN);
                        io.KeyShift = ((SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_SHIFT) != 0);
                        io.KeyCtrl = ((SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_CTRL) != 0);
                        io.KeyAlt = ((SDL.SDL_GetModState() & SDL.SDL_Keymod.KMOD_ALT) != 0);
                        io.KeySuper = false;
                        //io.KeySuper = ((SDL_GetModState() & KMOD_GUI) != 0);
                        return true;
                    }
                // Multi-viewport support
                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        var window_event = ev.window.windowEvent;
                        if (window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE || window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED || window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                        {
                            ImGuiViewport* viewport = ImGuiAPI.FindViewportByPlatformHandle((void*)SDL.SDL_GetWindowFromID(ev.window.windowID));
                            if (viewport != (ImGuiViewport*)0)
                            {
                                if (window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                                {
                                    viewport->PlatformRequestClose = true;
                                    var closeEvent = new SDL.SDL_Event();
                                    closeEvent.type = SDL.SDL_EventType.SDL_QUIT;
                                    SDL.SDL_PushEvent(ref closeEvent);
                                }
                                if (window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED)
                                    viewport->PlatformRequestMove = true;
                                if (window_event == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                                    viewport->PlatformRequestResize = true;
                                return true;
                            }
                        }
                    }
                    break;
            }
            return false;
        }
        #endregion

        public class UImDrawDataRHI
        {
            public NxRHI.UCommandList CmdList;
            public NxRHI.UEffectBinder FontTextureBindInfo;
            public NxRHI.UCbView FontCBuffer;
            public NxRHI.UGraphicDraw Drawcall;

            public NxRHI.UGeomMesh GeomMesh;
            public Graphics.Mesh.UMeshPrimitives PrimitiveMesh;
            
            #region TriangleData
            public NxRHI.UVbView VertexBuffer;
            public NxRHI.UIbView IndexBuffer;
            public Support.UNativeArray<ImDrawVert> DataVB = Support.UNativeArray<ImDrawVert>.CreateInstance();
            public Support.UNativeArray<ushort> DataIB = Support.UNativeArray<ushort>.CreateInstance();
            #endregion
            public unsafe bool InitializeGraphics(EPixelFormat format, EPixelFormat dsFormat)
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                CmdList = rc.CreateCommandList();

                var renderer = UEngine.Instance.GfxDevice.SlateRenderer;

                GeomMesh = new NxRHI.UGeomMesh();
                GeomMesh.SetAtomNum(1);
                PrimitiveMesh = new Graphics.Mesh.UMeshPrimitives();
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
                    var pipeline = UEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(UEngine.Instance.GfxDevice.RenderContext, in pipeDesc);
                    Drawcall.BindPipeline(pipeline);

                    //Pipeline.mCoreObject.GetGpuProgram().BindInputLayout(renderer.InputLayout.mCoreObject);
                }
                return true;
            }
            public void Cleanup()
            {
                FontCBuffer?.Dispose();
                FontCBuffer = null;

                CmdList?.Dispose();
                CmdList = null;
                DataVB.Dispose();
                DataIB.Dispose();
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                IndexBuffer?.Dispose();
                IndexBuffer = null;
                GeomMesh?.Dispose();
                GeomMesh = null;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeRenderImDrawData = Profiler.TimeScopeManager.GetTimeScope(typeof(UDockWindowSDL), nameof(RenderImDrawData));
        public unsafe static void RenderImDrawData(ref ImDrawData draw_data, Graphics.Pipeline.UPresentWindow presentWindow, UImDrawDataRHI rhiData)
        {
            using (new Profiler.TimeScopeHelper(ScopeRenderImDrawData))
            {
                RenderImDrawDataImpl(ref draw_data, presentWindow, rhiData);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeDrawPass = Profiler.TimeScopeManager.GetTimeScope(typeof(UDockWindowSDL), "RenderImDrawData.DrawPass");
        private unsafe static void RenderImDrawDataImpl(ref ImDrawData draw_data, Graphics.Pipeline.UPresentWindow presentWindow, UImDrawDataRHI rhiData)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
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
                //var ibDesc = new NxRHI.FBufferDesc();
                //ibDesc.SetDefault();
                //ibDesc.Type = NxRHI.EBufferType.BFT_Index;
                //ibDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                //ibDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                //ibDesc.Size = (uint)(totalIBSize * 1.5f);
                //ibDesc.StructureStride = (uint)sizeof(ushort);
                var ibDesc = new NxRHI.FIbvDesc();
                ibDesc.m_Size = (uint)(totalIBSize * 1.5f);
                ibDesc.m_Stride = (uint)sizeof(ushort);
                ibDesc.m_CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                ibDesc.m_Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                rhiData.IndexBuffer = rc.CreateIBV(null, in ibDesc);
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }
            
            using (new Profiler.TimeScopeHelper(ScopeDrawPass))
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

                rhiData.VertexBuffer.UpdateGpuData(rhiData.CmdList, 0,
                    rhiData.DataVB.UnsafeAddressAt(0).ToPointer(), (uint)(vertexOffsetInVertices * sizeof(ImDrawVert)));

                rhiData.IndexBuffer.UpdateGpuData(rhiData.CmdList, 0,
                    rhiData.DataIB.UnsafeAddressAt(0).ToPointer(), (uint)(indexOffsetInElements * sizeof(ushort)));
                
                rhiData.GeomMesh.mCoreObject.GetVertexArray().BindVB(NxRHI.EVertexStreamType.VST_Position, rhiData.VertexBuffer.mCoreObject);
                rhiData.GeomMesh.mCoreObject.BindIndexBuffer(rhiData.IndexBuffer.mCoreObject);
            }

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

            var drawCmd = rhiData.CmdList.mCoreObject;
            presentWindow.BeginFrame();
            if (drawCmd.BeginCommand())
            {
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(1, 0, 0, 0));

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
                                    var rsv = handle.Target as NxRHI.USrView;
                                    if (rsv != null)
                                    {
                                        rhiData.Drawcall.BindSRV(rhiData.FontTextureBindInfo.mCoreObject, rsv);
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
                                rhiData.Drawcall.mCoreObject.Commit(drawCmd);
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
                drawCmd.EndCommand();
            }
            presentWindow.EndFrame();
            rc.CmdQueue.ExecuteCommandList(drawCmd);
            //drawCmd.Commit(rc.mCoreObject);
        }

        #region CallBack
        #region SDL
        unsafe static ImGuiPlatformIO.FDelegate_Platform_CreateWindow ImGui_ImplSDL2_CreateWindow = ImGui_ImplSDL2_CreateWindow_Impl;
        static unsafe void ImGui_ImplSDL2_CreateWindow_Impl(ImGuiViewport* viewport)
        {
            SDL.SDL_WindowFlags sdl_flags = 0;
            
            sdl_flags |= (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(UEngine.Instance.GfxDevice.SlateApplication.NativeWindow.Window) & SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
            sdl_flags |= SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN;
            sdl_flags |= ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoDecoration) != 0) ? SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS : 0;
            sdl_flags |= ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoDecoration) != 0) ? 0 : SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_TopMost) != 0 ? SDL.SDL_WindowFlags.SDL_WINDOW_ALWAYS_ON_TOP : 0;
            var myWindow = new Graphics.Pipeline.UPresentWindow();
            myWindow.IsCreatedByImGui = true;
            myWindow.CreateNativeWindow("No Title Yet", (int)viewport->Pos.X, (int)viewport->Pos.Y, (int)viewport->Size.X, (int)viewport->Size.Y, sdl_flags).ToPointer();            
            viewport->PlatformUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(myWindow)).ToPointer();
            viewport->PlatformHandle = myWindow.Window.ToPointer();
            viewport->PlatformHandleRaw = myWindow.HWindow.ToPointer();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_DestroyWindow ImGui_ImplSDL2_DestroyWindow = ImGui_ImplSDL2_DestroyWindow_Impl;
        static unsafe void ImGui_ImplSDL2_DestroyWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            if (myWindow.IsCreatedByImGui == false)
            {
                var closeEvent = new SDL.SDL_Event();
                closeEvent.type = SDL.SDL_EventType.SDL_QUIT;
                SDL.SDL_PushEvent(ref closeEvent);
                return;
            }
            myWindow.Cleanup();
            viewport->PlatformUserData = IntPtr.Zero.ToPointer();
            viewport->PlatformHandle = null;

            gcHandle.Free();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_ShowWindow ImGui_ImplSDL2_ShowWindow = ImGui_ImplSDL2_ShowWindow_Impl;
        unsafe static void ImGui_ImplSDL2_ShowWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;

#if PWindow
            var hwnd = viewport->PlatformHandleRaw;

            // SDL hack: Hide icon from task bar
            // Note: SDL 2.0.6+ has a SDL_WINDOW_SKIP_TASKBAR flag which is supported under Windows but the way it create the window breaks our seamless transition.
            if ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoTaskBarIcon) != 0)
            {
                //LONG ex_style = ::GetWindowLong(hwnd, GWL_EXSTYLE);
                //ex_style &= ~WS_EX_APPWINDOW;
                //ex_style |= WS_EX_TOOLWINDOW;
                //::SetWindowLong(hwnd, GWL_EXSTYLE, ex_style);
            }

            // SDL hack: SDL always activate/focus windows :/
            if ((viewport->Flags & ImGuiViewportFlags_.ImGuiViewportFlags_NoFocusOnAppearing) != 0)
            {
                //::ShowWindow(hwnd, SW_SHOWNA);
                myWindow.ShowNativeWindow();
                return;
            }
#endif

            myWindow.ShowNativeWindow();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowPos ImGui_ImplSDL2_SetWindowPos = ImGui_ImplSDL2_SetWindowPos_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowPos_Impl(ImGuiViewport* viewport, Vector2 pos)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            myWindow.SetWindowPosition((int)pos.X, (int)pos.Y);
        }
        static unsafe ImGuiPlatformIO.FDelegate_Platform_GetWindowPos ImGui_ImplSDL2_GetWindowPos = ImGui_ImplSDL2_GetWindowPos_Impl;
        unsafe static Vector2 ImGui_ImplSDL2_GetWindowPos_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return new Vector2(0);
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            return myWindow.GetWindowPosition();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowSize ImGui_ImplSDL2_SetWindowSize = ImGui_ImplSDL2_SetWindowSize_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowSize_Impl(ImGuiViewport* viewport, Vector2 size)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            myWindow.SetWindowSize((int)size.X, (int)size.Y);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowSize ImGui_ImplSDL2_GetWindowSize = ImGui_ImplSDL2_GetWindowSize_Impl;
        unsafe static Vector2 ImGui_ImplSDL2_GetWindowSize_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return new Vector2(0);
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            return myWindow.GetWindowSize();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowFocus ImGui_ImplSDL2_SetWindowFocus = ImGui_ImplSDL2_SetWindowFocus_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowFocus_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            myWindow.SetWindowFocus();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowFocus ImGui_ImplSDL2_GetWindowFocus = ImGui_ImplSDL2_GetWindowFocus_Impl;
        unsafe static bool ImGui_ImplSDL2_GetWindowFocus_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return false;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            return myWindow.GetWindowFocus();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_GetWindowMinimized ImGui_ImplSDL2_GetWindowMinimized = ImGui_ImplSDL2_GetWindowMinimized_Impl;
        unsafe static bool ImGui_ImplSDL2_GetWindowMinimized_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return false;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            return myWindow.GetWindowMinimized();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowTitle ImGui_ImplSDL2_SetWindowTitle = ImGui_ImplSDL2_SetWindowTitle_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowTitle_Impl(ImGuiViewport* viewport, sbyte* title)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            myWindow.SetWindowTitle(System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)title));
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_RenderWindow ImGui_ImplSDL2_RenderWindow = ImGui_ImplSDL2_RenderWindow_Impl;
        unsafe static void ImGui_ImplSDL2_RenderWindow_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SwapBuffers ImGui_ImplSDL2_SwapBuffers = ImGui_ImplSDL2_SwapBuffers_Impl;
        unsafe static void ImGui_ImplSDL2_SwapBuffers_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
        }
        unsafe static ImGuiPlatformIO.FDelegate_Platform_SetWindowAlpha ImGui_ImplSDL2_SetWindowAlpha = ImGui_ImplSDL2_SetWindowAlpha_Impl;
        unsafe static void ImGui_ImplSDL2_SetWindowAlpha_Impl(ImGuiViewport* viewport, float alpha)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;
            myWindow.SetWindowOpacity(alpha);
        }
        #endregion
        #region Renderer
        public class ViewportData
        {
            public Graphics.Pipeline.UPresentWindow PresentWindow;
            
            public UImDrawDataRHI DrawData = new UImDrawDataRHI();

            public void Cleanup()
            {
                //PresentWindow?.Cleanup();
                PresentWindow = null;
                DrawData?.Cleanup();
                DrawData = null;
            }
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_CreateWindow ImGui_Renderer_CreateWindow = ImGui_Renderer_CreateWindow_Impl;
        unsafe static void ImGui_Renderer_CreateWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->PlatformUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->PlatformUserData);
            var myWindow = gcHandle.Target as Graphics.Pipeline.UPresentWindow;

            //Create SwapChain
            var vpData = new ViewportData();
            vpData.PresentWindow = myWindow;
            viewport->RendererUserData = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(vpData)).ToPointer();


            vpData.PresentWindow.InitSwapChain(UEngine.Instance.GfxDevice.RenderContext);            
            vpData.DrawData.InitializeGraphics(vpData.PresentWindow.GetSwapchainFormat(), vpData.PresentWindow.GetSwapchainDSFormat());
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_DestroyWindow ImGui_Renderer_DestroyWindow = ImGui_Renderer_DestroyWindow_Impl;
        unsafe static void ImGui_Renderer_DestroyWindow_Impl(ImGuiViewport* viewport)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;
            vpData.Cleanup();
            gcHandle.Free();
            viewport->RendererUserData = IntPtr.Zero.ToPointer();
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_SetWindowSize ImGui_Renderer_SetWindowSize = ImGui_Renderer_SetWindowSize_Impl;
        unsafe static void ImGui_Renderer_SetWindowSize_Impl(ImGuiViewport* viewport, Vector2 size)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;
            vpData.PresentWindow.OnResize(size.X, size.Y);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_RenderWindow ImGui_Renderer_RenderWindow = ImGui_Renderer_RenderWindow_Impl;
        unsafe static void ImGui_Renderer_RenderWindow_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;

            //ImGui_ImplOpenGL3_RenderDrawData(viewport->DrawData);
            var draw_data = viewport->DrawData;
            RenderImDrawData(ref *draw_data, vpData.PresentWindow, vpData.DrawData);
        }
        unsafe static ImGuiPlatformIO.FDelegate_Renderer_SwapBuffers ImGui_Renderer_SwapBuffers = ImGui_Renderer_SwapBuffers_Impl;
        unsafe static void ImGui_Renderer_SwapBuffers_Impl(ImGuiViewport* viewport, void* dummy)
        {
            if ((IntPtr)viewport->RendererUserData == IntPtr.Zero)
                return;
            var gcHandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)viewport->RendererUserData);
            var vpData = gcHandle.Target as ViewportData;
            
            vpData.PresentWindow.SwapChain.Present(0, 0);
        }
        #endregion
        #endregion
    }
}
