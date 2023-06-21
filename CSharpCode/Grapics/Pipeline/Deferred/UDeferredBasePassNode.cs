using EngineNS.Bricks.VXGI;
using EngineNS.Graphics.Pipeline.Shadow;
using NPOI.HSSF.Record.AutoFilter;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredOpaque : Shader.UGraphicsShadingEnv
    {
        public UDeferredOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredOpaque.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_Color};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
            };
        }
    }
    public class UDeferredBasePassNode : Common.UBasePassNode
    {
        //public Common.URenderGraphPin Rt0PinOut = Common.URenderGraphPin.CreateOutput("MRT0", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);//rgb - metallicty
        //public Common.URenderGraphPin Rt1PinOut = Common.URenderGraphPin.CreateOutput("MRT1", true, EPixelFormat.PXF_R10G10B10A2_UNORM);//normal - Flags
        //public Common.URenderGraphPin Rt2PinOut = Common.URenderGraphPin.CreateOutput("MRT2", true, EPixelFormat.PXF_R8G8B8A8_UNORM);//Roughness,Emissive,Specular,unused
        //public Common.URenderGraphPin Rt3PinOut = Common.URenderGraphPin.CreateOutput("MRT3", true, EPixelFormat.PXF_R16G16_UNORM);//EPixelFormat.PXF_R10G10B10A2_UNORM//motionXY
        public Common.URenderGraphPin DepthStencilPinOut = Common.URenderGraphPin.CreateOutput("DepthStencil", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);

        public Common.URenderGraphPin Rt0PinOut = Common.URenderGraphPin.CreateInputOutput("MRT0", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);//rgb - metallicty
        public Common.URenderGraphPin Rt1PinOut = Common.URenderGraphPin.CreateInputOutput("MRT1", true, EPixelFormat.PXF_R10G10B10A2_UNORM);//normal - Flags
        public Common.URenderGraphPin Rt2PinOut = Common.URenderGraphPin.CreateInputOutput("MRT2", true, EPixelFormat.PXF_R8G8B8A8_UNORM);//Roughness,Emissive,Specular,unused
        public Common.URenderGraphPin Rt3PinOut = Common.URenderGraphPin.CreateInputOutput("MRT3", true, EPixelFormat.PXF_R16G16_UNORM);//EPixelFormat.PXF_R10G10B10A2_UNORM//motionXY
        //public Common.URenderGraphPin DepthStencilPinOut = Common.URenderGraphPin.CreateInputOutput("DepthStencil");

        public UDrawBuffers BackgroundPass = new UDrawBuffers();
        public UDeferredBasePassNode()
        {
            Name = "UDeferredBasePassNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(Rt0PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt1PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt2PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt3PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(DepthStencilPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
            Rt0PinOut.IsAutoResize = true;
            Rt1PinOut.IsAutoResize = true;
            Rt2PinOut.IsAutoResize = true;
            Rt3PinOut.IsAutoResize = true;

            Rt0PinOut.IsAllowInputNull = true;
            Rt1PinOut.IsAllowInputNull = true;
            Rt2PinOut.IsAllowInputNull = true;
            Rt3PinOut.IsAllowInputNull = true;
            //DepthStencilPinOut.IsAllowInputNull = true;
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }

            Rt0PinOut.Attachement.Height = (uint)y;
            Rt0PinOut.Attachement.Width = (uint)x;
        }
        public override void FrameBuild(URenderPolicy policy)
        {
        }
        
        public UDeferredOpaque mOpaqueShading;
        public NxRHI.URenderPass RenderPass;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");
            BackgroundPass.Initialize(rc, debugName + ".Background");

            CreateGBuffers(policy, Rt0PinOut.Attachement.Format);

            mOpaqueShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredOpaque>();
        }
        public virtual unsafe UGraphicsBuffers CreateGBuffers(URenderPolicy policy, EPixelFormat format)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 4;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[1].Format = Rt1PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[1].Samples = 1;
            PassDesc.AttachmentMRTs[1].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[2].Format = Rt2PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[2].Samples = 1;
            PassDesc.AttachmentMRTs[2].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[2].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[3].Format = Rt3PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[3].Samples = 1;
            PassDesc.AttachmentMRTs[3].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.AttachmentMRTs[3].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.Format = DepthStencilPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
            //PassDesc.mDepthClearValue = 1.0f;                
            //PassDesc.mStencilClearValue = 0u;
            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, Rt0PinOut);
            GBuffers.SetRenderTarget(policy, 1, Rt1PinOut);
            GBuffers.SetRenderTarget(policy, 2, Rt2PinOut);
            GBuffers.SetRenderTarget(policy, 3, Rt3PinOut);
            GBuffers.SetDepthStencil(policy, DepthStencilPinOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            return GBuffers;
        }
        public override void Dispose()
        {
            GBuffers?.Dispose();
            GBuffers = null;

            base.Dispose();
        }
        public override Shader.UGraphicsShadingEnv GetPassShading(Graphics.Pipeline.URenderPolicy.EShadingType type, Mesh.UMesh mesh, int atom)
        {
            return mOpaqueShading;
        }
        public override void BeforeTickLogic(URenderPolicy policy)
        {
            if (Rt0PinOut.FindInLinker() == null)
            {
                Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                Rt0PinOut.IsAutoResize = true;
            }
            if (Rt1PinOut.FindInLinker() == null)
            {
                Rt1PinOut.Attachement.Format = EPixelFormat.PXF_R10G10B10A2_UNORM;
                Rt1PinOut.IsAutoResize = true;
            }
            if (Rt2PinOut.FindInLinker() == null)
            {
                Rt2PinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                Rt2PinOut.IsAutoResize = true;
            }
            if (Rt3PinOut.FindInLinker() == null)
            {
                Rt3PinOut.Attachement.Format = EPixelFormat.PXF_R16G16_UNORM;
                Rt3PinOut.IsAutoResize = true;
            }
            //if (DepthStencilPinOut.FindInLinker() == null)
            //{
            //    DepthStencilPinOut.Attachement.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            //    DepthStencilPinOut.IsAutoResize = true;
            //}

            if (policy.DisableHDR)
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R8G8B8A8_UNORM)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R8G8B8A8_UNORM);
                }
            }
            else
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R16G16B16A16_FLOAT)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R16G16B16A16_FLOAT);
                }
            }
        }

        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UDeferredBasePassNode), nameof(TickLogic));
        [ThreadStatic]
        private static Profiler.TimeScope ScopePushGpuDraw = Profiler.TimeScopeManager.GetTimeScope(typeof(UDeferredBasePassNode), "PushGpuDraw");
        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                BasePass.DrawCmdList.BeginCommand();
                BackgroundPass.DrawCmdList.BeginCommand();

                using (new Profiler.TimeScopeHelper(ScopePushGpuDraw))
                {
                    //BasePass.DrawCmdList.SetViewport(GBuffers.ViewPort.mCoreObject);
                    foreach (var i in policy.VisibleMeshes)
                    {
                        if (i.Atoms == null)
                            continue;

                        for (int j = 0; j < i.Atoms.Count; j++)
                        {
                            if (i.Atoms[j].Material == null)
                                continue;
                            var layer = i.Atoms[j].Material.RenderLayer;
                            if (layer == ERenderLayer.RL_Background)
                            {
                                var drawcall = i.GetDrawCall(GBuffers, j, policy, URenderPolicy.EShadingType.BasePass, this);
                                if (drawcall != null)
                                {
                                    drawcall.BindGBuffer(policy.DefaultCamera, GBuffers);

                                    BackgroundPass.DrawCmdList.PushGpuDraw(drawcall);
                                }
                            }
                            else if (layer == ERenderLayer.RL_Opaque)
                            {
                                var drawcall = i.GetDrawCall(GBuffers, j, policy, URenderPolicy.EShadingType.BasePass, this);
                                if (drawcall != null)
                                {
                                    drawcall.BindGBuffer(policy.DefaultCamera, GBuffers);

                                    BasePass.DrawCmdList.PushGpuDraw(drawcall);
                                }
                            }
                        }
                    }
                }

                var bgCmdlist = BackgroundPass.DrawCmdList;
                var cmdlist = BasePass.DrawCmdList;
                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));
                passClears.SetClearColor(1, new Color4f(1, 0, 0, 0));
                passClears.SetClearColor(2, new Color4f(1, 0, 0, 0));
                if (Rt3PinOut.Attachement.Format == EPixelFormat.PXF_R16G16_FLOAT)
                    passClears.SetClearColor(3, new Color4f(0, 0, 0, 0));
                else
                    passClears.SetClearColor(3, new Color4f(0, 0.5f, 0.5f, 0));

                GBuffers.BuildFrameBuffers(policy);

                {
                    bgCmdlist.SetViewport(in GBuffers.Viewport);
                    bgCmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Background.ToString());
                    
                    bgCmdlist.FlushDraws();
                    
                    bgCmdlist.EndPass();
                    bgCmdlist.EndCommand();
                }

                {
                    cmdlist.SetViewport(in GBuffers.Viewport);
                    passClears.ClearFlags = (NxRHI.ERenderPassClearFlags)0;
                    cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());

                    cmdlist.FlushDraws();
                    
                    cmdlist.EndPass();
                    cmdlist.EndCommand();
                }

                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(bgCmdlist, "DSNodeBackground");
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmdlist, "DSNodeBase");
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            BackgroundPass.SwapBuffer();
            base.TickSync(policy);

            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                var preMatrix = i.PerMeshCBuffer.GetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
                i.PerMeshCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.PreWorldMatrix, preMatrix);
            }
        }
        
    }
}
