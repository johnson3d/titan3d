using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("ClearMRT", "ClearMRT", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtClearMRTNode : TtRenderGraphNode
    {
        public TtRenderGraphPin[] RtPinOut;
        public TtRenderGraphPin DepthStencilPinOut = TtRenderGraphPin.CreateInputOutput("DepthStencil", true, EPixelFormat.PXF_D24_UNORM_S8_UINT);
        public EPixelFormat[] RtDefaultFormat = new EPixelFormat[] {
            EPixelFormat.PXF_R16G16B16A16_FLOAT, 
            EPixelFormat.PXF_R10G10B10A2_UNORM, 
            EPixelFormat.PXF_R8G8B8A8_UNORM, 
            EPixelFormat.PXF_R16G16_UNORM 
        };
        public Color4f[] ClearColors = new Color4f[] {
            new Color4f(1, 0, 0, 0),
            new Color4f(1, 0, 0, 0),
            new Color4f(1, 0, 0, 0),
            new Color4f(1, 0, 0, 0)
        };
        public NxRHI.URenderPass[] RenderPass = new NxRHI.URenderPass[4];
        public TtGraphicsBuffers[] GBuffers = new TtGraphicsBuffers[4];
        public NxRHI.ERenderPassClearFlags[] ClearFlags = new NxRHI.ERenderPassClearFlags[4];

        [Rtti.Meta]
        public int OutputRT { get; set; } = 0;
        [Rtti.Meta]
        public bool OutputDS { get; set; } = true;
        public TtClearMRTNode()
        {
            Name = "ClearMRTNode";
        }
        public override void InitNodePins()
        {
            RtPinOut = new TtRenderGraphPin[4];
            for (int i = 0; i < 4; i++)
            {
                RtPinOut[i] = TtRenderGraphPin.CreateInputOutput($"MRT{i}", true, RtDefaultFormat[i]);
                AddInputOutput(RtPinOut[i], NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
                RtPinOut[i].IsAllowInputNull = true;
            }
            AddInputOutput(DepthStencilPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
            DepthStencilPinOut.IsAllowInputNull = true;
        }
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");

            ClearFlags[0] = NxRHI.ERenderPassClearFlags.CLEAR_RT0;
            ClearFlags[1] = ClearFlags[0] | NxRHI.ERenderPassClearFlags.CLEAR_RT1;
            ClearFlags[2] = ClearFlags[2] | NxRHI.ERenderPassClearFlags.CLEAR_RT2;
            ClearFlags[3] = ClearFlags[3] | NxRHI.ERenderPassClearFlags.CLEAR_RT3;
            CreateGBuffers(policy);
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            foreach(var i in GBuffers)
            {
                if (i != null)
                {
                    i.SetSize(x, y);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                RtPinOut[i].Attachement.Height = (uint)y;
                RtPinOut[i].Attachement.Width = (uint)x;
            }
        }
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            for (int i = 0; i < 4; i++)
            {
                PassDesc.AttachmentMRTs[i].Format = RtPinOut[i].Attachement.Format;
                PassDesc.AttachmentMRTs[i].Samples = 1;
                PassDesc.AttachmentMRTs[i].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[i].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            }
            PassDesc.m_AttachmentDepthStencil.Format = DepthStencilPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            for (int i = 0; i < 4; i++)
            {
                PassDesc.NumOfMRT = (uint)i + 1;
                RenderPass[i] = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);
                GBuffers[i] = new TtGraphicsBuffers();

                GBuffers[i].Initialize(policy, RenderPass[i]);
                for (int j = 0; j <= i; j++)
                {
                    GBuffers[i].SetRenderTarget(policy, j, RtPinOut[j]);
                }
                if (OutputDS)
                    GBuffers[i].SetDepthStencil(policy, DepthStencilPinOut);
                GBuffers[i].TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;
            }

            return GBuffers[0];
        }

        public override void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            int MrtNum = OutputRT;
            if (MrtNum == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (RtPinOut[i].FindInLinker() != null)
                    {
                        MrtNum++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            bool clearDS = OutputDS || DepthStencilPinOut.FindInLinker() != null;

            var cmdlist = BasePass.DrawCmdList;
            {
                using (new NxRHI.TtCmdListScope(cmdlist))
                {
                    var passClears = new NxRHI.FRenderPassClears();
                    passClears.SetDefault();
                    for (int i = 0; i < MrtNum; i++)
                    {
                        passClears.SetClearColor(0, in ClearColors[i]); 
                    }                    
                    GBuffers[MrtNum - 1].BuildFrameBuffers(policy);

                    {
                        cmdlist.SetViewport(in GBuffers[MrtNum - 1].Viewport);
                        passClears.ClearFlags = ClearFlags[MrtNum - 1];
                        if (clearDS)
                        {
                            passClears.ClearFlags |= (NxRHI.ERenderPassClearFlags.CLEAR_DEPTH | NxRHI.ERenderPassClearFlags.CLEAR_STENCIL);
                            passClears.DepthClearValue = 1.0f;
                        }
                        cmdlist.BeginPass(GBuffers[MrtNum - 1].FrameBuffers, in passClears, ERenderLayer.RL_Opaque.ToString());

                        cmdlist.EndPass();
                    }
                }

                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(BasePass.DrawCmdList, "ClearRT");
            }
        }
        public override void TickSync(URenderPolicy policy)
        {
            base.TickSync(policy);
        }
    }
}
