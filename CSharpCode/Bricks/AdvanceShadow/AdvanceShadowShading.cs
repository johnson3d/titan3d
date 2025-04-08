using System;
using System.Collections.Generic;
using System.Net.Mail;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Org.BouncyCastle.Tsp;

namespace EngineNS.Bricks.AdvanceShadow
{
    public class TtAdvanceShadowShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtAdvanceShadowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/SSM.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, NxRHI.EVertexStreamType.VST_UV, };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
            };
        }
        public override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
            var shadowMapNode = policy.FindFirstNode<TtAdvanceShadowMapNode>();
            if (shadowMapNode == null)
                return;

            drawcall.mCoreObject.BindPipeline(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, shadowMapNode.DepthRaster.mCoreObject);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AdvShadow", "Shadow\\AdvShadow", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtAdvanceShadowMapNode : TtRenderGraphNode
    {
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D16_UNORM, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);//or D32
        bool mIsDepth32 = false;
        [Rtti.Meta]
        public bool IsDepth32
        {
            get => mIsDepth32;
            set
            {
                mIsDepth32 = value;
                DepthPinOut.Attachement.Format = value ? EPixelFormat.PXF_D32_FLOAT : EPixelFormat.PXF_D16_UNORM;
            }
        }
        [Rtti.Meta]
        public int PageResolution { get; set; } = 128;
        public NxRHI.TtTexture DepthTextureArray;
        public TtAdvanceShadowMapNode()
        {
            Name = "AdvShadowMap";
        }
        public override void InitNodePins()
        {
            AddOutput(DepthPinOut);
        }
        public override void Dispose()
        {
            if (mDSViews != null)
            {
                foreach (var view in mDSViews)
                {
                    view.Dispose();
                }
                mDSViews = null;
            }
            CoreSDK.DisposeObject(ref DepthTextureArray);
            base.Dispose();
        }
        public TtAdvanceShadowShading mShadowShading;
        public NxRHI.TtGpuPipeline DepthRaster;
        public TtGraphicsBuffers mGBuffer;
        public TtDepthStencilView[] mDSViews;
        public TtAttachBuffer DepthAttachment = new TtAttachBuffer();
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mShadowShading;
        }
        public TtQTree mShadowQTree = null;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mShadowShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAdvanceShadowShading>();

            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 0;
            PassDesc.m_AttachmentDepthStencil.Format = DepthPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            NxRHI.TtRenderPass RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            mGBuffer = new TtGraphicsBuffers();
            mGBuffer.Initialize(policy, RenderPass);
            //mGBuffer.SetDepthStencil(policy, DepthPinOut);
            mGBuffer.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
            mGBuffer.SetSize(PageResolution, PageResolution);

            var dpRastDesc = new NxRHI.FGpuPipelineDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.m_Rasterizer.m_DepthBias = 1;
            dpRastDesc.m_Rasterizer.m_SlopeScaledDepthBias = 2.0f;
            DepthRaster = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(rc, in dpRastDesc);
        }
        public GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            if(mShadowQTree==null)
                return;

            if (DepthTextureArray == null)
            {
                FTextureDesc desc = new FTextureDesc();
                desc.SetDefault();
                desc.Width = (uint)PageResolution;
                desc.Height = (uint)PageResolution;
                desc.ArraySize = (uint)mShadowQTree.ShadowPages.Length;
                desc.Format = DepthPinOut.Attachement.Format;
                DepthPinOut.Attachement.Width = desc.Width;
                DepthPinOut.Attachement.Height = desc.Height;
                DepthTextureArray = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);

                if (mDSViews != null)
                {
                    foreach (var view in mDSViews)
                    {
                        view.Dispose();
                    }
                    mDSViews = new TtDepthStencilView[mShadowQTree.ShadowPages.Length];

                    for (int i = 0; i < mDSViews.Length; i++)
                    {
                        var dsDesc = new FDsvDesc();
                        dsDesc.SetDefault();
                        dsDesc.Type = NxRHI.EDsvType.DSV_Texture2DArray;
                        dsDesc.Format = desc.Format;
                        dsDesc.Width = desc.Width;
                        dsDesc.Height = desc.Height;
                        dsDesc.MipLevel = (uint)0;
                        dsDesc.ArrayIndex = (uint)i;

                        mDSViews[i] = TtEngine.Instance.GfxDevice.RenderContext.CreateDSV(DepthTextureArray, in dsDesc);
                    }
                }
            }

            var attachment = ImportAttachment(DepthPinOut, DepthAttachment);
            attachment.GpuResource = DepthTextureArray;

            mVisParameter.CullType = GamePlay.TtWorld.TtVisParameter.EVisCull.Shadow;
            mVisParameter.IsBuildAABB = true;
            mVisParameter.World = world;
            mVisParameter.IsGatherVisibleNodes = false;

            foreach (var i in mShadowQTree.UpdateShadowMapNodes)
            {
                if (i.Leaf != null)
                {
                    mVisParameter.ClearVisibles();
                    mVisParameter.CullCamera = i.Leaf.ShadowCamera;
                    foreach (var j in i.ShadowObjects)
                    {
                        j.Value.SceneNode.OnGatherVisibleMeshes(mVisParameter);
                    }
                    var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
                    using (new NxRHI.TtCmdListScope(cmdlist))
                    {
                        mGBuffer.SetDepthStencil(mDSViews[i.Leaf.PageIndex]);
                        DrawDepth(cmdlist, world, policy);
                    }
                    policy.CommitCommandList(cmdlist);
                }
            }
            mVisParameter.ClearVisibles();
        }
        private void DrawDepth(NxRHI.TtCommandList cmdlist, GamePlay.TtWorld world, TtRenderPolicy policy)
        {
            foreach (var i in mVisParameter.VisibleMeshes)
            {
                if (i.Mesh.IsCastShadow == false)
                    continue;
                if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                    continue;
                foreach (var j in i.Mesh.SubMeshes)
                {
                    foreach (var k in j.Atoms)
                    {
                        var drawcall = k.GetDrawCall(cmdlist.mCoreObject, mGBuffer, policy, this);

                        if (drawcall != null)
                        {
                            drawcall.BindGBuffer(mVisParameter.CullCamera, mGBuffer);

                            cmdlist.PushGpuDraw(drawcall);
                        }
                    }
                }
            }

            var passClear = new NxRHI.FRenderPassClears();
            {
                passClear.SetDefault();
                passClear.SetClearColor(0, new Color4f(1, 0, 0, 0));
            }
            cmdlist.BeginPass(mGBuffer.FrameBuffers, in passClear, "AdvShadowDepth");
            cmdlist.FlushDraws();
            cmdlist.EndPass();
        }
    }
}
