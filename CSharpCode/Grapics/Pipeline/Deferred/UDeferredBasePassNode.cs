﻿using EngineNS.Bricks.VXGI;
using EngineNS.Graphics.Pipeline.Shadow;
using NPOI.HSSF.Record.AutoFilter;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;
using System.ComponentModel;
using EngineNS.GamePlay;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredOpaque : Shader.TtGraphicsShadingEnv
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
    [Bricks.CodeBuilder.ContextMenu("BassPass", "Deferred\\BassPass", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UDeferredBasePassNode : Common.UBasePassNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin GpuCullPinIn = TtRenderGraphPin.CreateInput("GpuCull");
        public TtRenderGraphPin Rt0PinOut = TtRenderGraphPin.CreateInputOutput("MRT0", true, EPixelFormat.PXF_R16G16B16A16_FLOAT);//rgb - metallicty
        public TtRenderGraphPin Rt1PinOut = TtRenderGraphPin.CreateInputOutput("MRT1", true, EPixelFormat.PXF_R10G10B10A2_UNORM);//normal - Flags
        public TtRenderGraphPin Rt2PinOut = TtRenderGraphPin.CreateInputOutput("MRT2", true, EPixelFormat.PXF_R8G8B8A8_UNORM);//Roughness,Emissive,Specular,unused
        public TtRenderGraphPin Rt3PinOut = TtRenderGraphPin.CreateInputOutput("MRT3", true, EPixelFormat.PXF_R16G16_UNORM);//EPixelFormat.PXF_R10G10B10A2_UNORM//motionXY
        public TtRenderGraphPin DepthStencilPinOut = TtRenderGraphPin.CreateInputOutput("DepthStencil", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);

        public TtCpuCullingNode CpuCullNode = null;
        public TtGpuCullingNode GpuCullNode = null;
        public UDrawBuffers BackgroundPass = new UDrawBuffers();
        [Category("Option")]
        [Rtti.Meta]
        public bool ClearMRT
        {
            get;
            set;
        } = true;
        public UDeferredBasePassNode()
        {
            Name = "UDeferredBasePassNode";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddInput(GpuCullPinIn, NxRHI.EBufferType.BFT_NONE);
            GpuCullPinIn.IsAllowInputNull = true;
            AddInputOutput(Rt0PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt1PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt2PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt3PinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthStencilPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);

            Rt0PinOut.IsAllowInputNull = true;
            Rt1PinOut.IsAllowInputNull = true;
            Rt2PinOut.IsAllowInputNull = true;
            Rt3PinOut.IsAllowInputNull = true;
            DepthStencilPinOut.IsAllowInputNull = true;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }

            Rt0PinOut.Attachement.Height = (uint)y;
            Rt0PinOut.Attachement.Width = (uint)x;
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            
        }
        
        public UDeferredOpaque mOpaqueShading;
        public NxRHI.URenderPass RenderPass;

        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");
            BackgroundPass.Initialize(rc, debugName + ".Background");

            CreateGBuffers(policy, Rt0PinOut.Attachement.Format);
            
            mOpaqueShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredOpaque>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            linker = GpuCullPinIn.FindInLinker();
            if (linker != null)
            {
                GpuCullNode = linker.OutPin.HostNode as TtGpuCullingNode;
            }
        }
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 4;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[1].Format = Rt1PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[1].Samples = 1;
            PassDesc.AttachmentMRTs[1].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[2].Format = Rt2PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[2].Samples = 1;
            PassDesc.AttachmentMRTs[2].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[2].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[3].Format = Rt3PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[3].Samples = 1;
            PassDesc.AttachmentMRTs[3].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[3].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.Format = DepthStencilPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
            //PassDesc.mDepthClearValue = 1.0f;                
            //PassDesc.mStencilClearValue = 0u;
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

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
        public override Shader.TtGraphicsShadingEnv GetPassShading(Mesh.TtMesh.TtAtom atom)
        {
            return mOpaqueShading;
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
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
        public unsafe override void TickLogic(GamePlay.UWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                using (new NxRHI.TtCmdListScope(BasePass.DrawCmdList))
                using (new NxRHI.TtCmdListScope(BackgroundPass.DrawCmdList))
                {
                    using (new Profiler.TimeScopeHelper(ScopePushGpuDraw))
                    {
                        if (GpuCullNode != null)
                        {
                            GpuCullNode.Commit(policy, BasePass.DrawCmdList, GBuffers);
                        }

                        var visibleMeshes = CpuCullNode.VisParameter.VisibleMeshes;
                        var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                        //todo:ParrallelFor
                        foreach (var i in visibleMeshes)
                        {
                            if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                                continue;
                            foreach (var j in i.Mesh.SubMeshes)
                            {
                                foreach (var k in j.Atoms)
                                {
                                    if (k == null || k.Material == null)
                                        continue;

                                    var layer = k.Material.RenderLayer;
                                    if (layer == ERenderLayer.RL_Background)
                                    {
                                        var cmd = BackgroundPass.DrawCmdList;
                                        var drawcall = k.GetDrawCall(cmd.mCoreObject, GBuffers, policy, this);
                                        if (drawcall != null)
                                        {
                                            drawcall.BindGBuffer(camera, GBuffers);
                                            cmd.PushGpuDraw(drawcall);
                                        }
                                    }
                                    else if (layer == ERenderLayer.RL_Opaque)
                                    {
                                        if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                                            continue;

                                        var cmd = BasePass.DrawCmdList;
                                        var drawcall = k.GetDrawCall(cmd.mCoreObject, GBuffers, policy, this);
                                        if (drawcall != null)
                                        {
                                            drawcall.BindGBuffer(camera, GBuffers);
                                            cmd.PushGpuDraw(drawcall);
                                        }
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
                        cmdlist.SetScissor(0, (NxRHI.FScissorRect*)0);
                        bgCmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Background.ToString());

                        bgCmdlist.FlushDraws();

                        bgCmdlist.EndPass();
                    }

                    {
                        cmdlist.SetViewport(in GBuffers.Viewport);
                        cmdlist.SetScissor(0, (NxRHI.FScissorRect*)0);
                        passClears.ClearFlags = (NxRHI.ERenderPassClearFlags)0;
                        cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());

                        cmdlist.FlushDraws();

                        cmdlist.EndPass();
                    }
                }

                policy.CommitCommandList(BackgroundPass.DrawCmdList, "DSNodeBackground");
                policy.CommitCommandList(BasePass.DrawCmdList, "DSNodeBase");
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            BackgroundPass.SwapBuffer();
            base.TickSync(policy);

            foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
            {
                var preMatrix = i.Mesh.PerMeshCBuffer.GetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
                i.Mesh.PerMeshCBuffer.SetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.PreWorldMatrix, preMatrix);
            }
        }
        
    }
}
