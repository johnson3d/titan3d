using System;
using System.Collections.Generic;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using EngineNS.GamePlay.Scene;
using EngineNS.Thread.Async;
using EngineNS.GamePlay;
using System.ComponentModel;

namespace EngineNS.Bricks.AdvanceShadow
{
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FAdvShadowLayerData")]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]//align 16 for cbv
    public struct FAdvShadowLayerData
    {
        public Vector2i mLayerStartAndSide;
        public Vector2 mLayerGridSize;
    }

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
            //var shadowMapNode = policy.FindFirstNode<TtAdvanceShadowMapNode>();
            //if (shadowMapNode == null)
            //    return;
            var shadowMapNode = drawcall.TagObject as TtAdvanceShadowMapNode;

            drawcall.mCoreObject.BindPipeline(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, shadowMapNode.DepthRaster.mCoreObject);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("AdvanceShadow", "Shadow\\AdvanceShadow", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtAdvanceShadowMapNode : TtRenderGraphNode
    {
        public TtRenderGraphPin DepthPinOut = TtRenderGraphPin.CreateOutput("Depth", false, EPixelFormat.PXF_D16_UNORM, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);//or D32
        public TtRenderGraphPin SelfNodePinOut = TtRenderGraphPin.CreateOutput("Self", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);
        bool mIsDepth32 = false;
        [Rtti.Meta]
        [Category("Option")]
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
        [Category("Option")]
        public int PageResolution { get; set; } = 128;
        public NxRHI.TtTexture DepthTextureArray;
        public NxRHI.TtSrView DepthTextureArraySRV;
        public TtAdvanceShadowShading mShadowShading;
        public NxRHI.TtGpuPipeline DepthRaster;
        public TtGraphicsBuffers mGBuffer;
        public TtDepthStencilView[] mDSViews;
        public TtSrView[] mDebuggerSRViews;
        public TtAttachBuffer DepthAttachment = new TtAttachBuffer();

        public TtCbView mDirLightingCBV = null;
        public TtAdvanceShadowMapNode()
        {
            Name = "AdvShadowMap";
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
        public override void InitNodePins()
        {
            AddOutput(DepthPinOut);
            AddOutput(SelfNodePinOut);
            SelfNodePinOut.LinkType = "AdvShadow";
        }
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mShadowShading;
        }
        public TtQTree mShadowQTree = null;
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mShadowShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtAdvanceShadowShading>();

            BuildGBuffer(policy);

            var dpRastDesc = new NxRHI.FGpuPipelineDesc();
            dpRastDesc.SetDefault();
            dpRastDesc.m_Rasterizer.m_DepthBias = 1;
            dpRastDesc.m_Rasterizer.m_SlopeScaledDepthBias = 2.0f;
            DepthRaster = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(rc, in dpRastDesc);

            if (this.Enable)
                this.Enable = true;
        }
        public unsafe void BuildGBuffer(TtRenderPolicy policy)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var PassDesc = new NxRHI.FRenderPassDesc();
            //PassDesc.NumOfMRT = 1;
            //PassDesc.AttachmentMRTs[0].Format = EPixelFormat.PXF_R10G10B10A2_UNORM;
            //PassDesc.AttachmentMRTs[0].Samples = 1;
            //PassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            //PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
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
            mGBuffer.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();
            mGBuffer.SetSize(PageResolution, PageResolution);
        }
        public GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        //public List<TtQNode> mLeafs = new List<TtQNode>();
        //public Dictionary<TtNode, TtShadowObject> mShadowObjects = new Dictionary<TtNode, TtShadowObject>();
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            if (mShadowQTree == null)
            {
                var node = world.Root.FindFirstChild<TtAdvanceShadowNode>();
                if (node != null) 
                {
                    node.mRenderGraphNode = this;
                    mShadowQTree = node.mShadowMapTree;
                }
                
                return;
            }

            if (DepthTextureArray == null)
            {
                FTextureDesc desc = new FTextureDesc();
                desc.SetDefault();
                desc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_DSV;
                desc.Width = (uint)PageResolution;
                desc.Height = (uint)PageResolution;
                desc.MipLevels = 1;
                desc.ArraySize = (uint)mShadowQTree.MaxPageCount;
                desc.Format = DepthPinOut.Attachement.Format;
                DepthPinOut.Attachement.Width = desc.Width;
                DepthPinOut.Attachement.Height = desc.Height;
                DepthTextureArray = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                DepthTextureArray.SetDebugName("AdvShadowDepthArray");

                FSrvDesc srvDesc = new FSrvDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                srvDesc.Texture2DArray.FirstArraySlice = 0;
                srvDesc.Texture2DArray.MostDetailedMip = 0;
                DepthTextureArraySRV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(DepthTextureArray, in srvDesc);

                if (mDebuggerSRViews != null)
                {
                    foreach (var view in mDebuggerSRViews)
                    {
                        view.Dispose();
                    }
                }
                mDebuggerSRViews = new TtSrView[mShadowQTree.MaxPageCount];
                for (int i = 0; i < mDebuggerSRViews.Length; i++)
                {
                    var srvDesc1 = new FSrvDesc();
                    srvDesc1.SetTexture2DArray();
                    srvDesc1.Format = desc.Format;
                    srvDesc1.Texture2DArray.MostDetailedMip = 0;
                    srvDesc1.Texture2DArray.MipLevels = 1;
                    srvDesc1.Texture2DArray.FirstArraySlice = (uint)i;
                    srvDesc1.Texture2DArray.ArraySize = 1;

                    mDebuggerSRViews[i] = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(DepthTextureArray, in srvDesc1);
                }

                if (mDSViews != null)
                {
                    foreach (var view in mDSViews)
                    {
                        view.Dispose();
                    }
                }
                mDSViews = new TtDepthStencilView[mShadowQTree.MaxPageCount];

                for (int i = 0; i < mDSViews.Length; i++)
                {
                    var dsDesc = new FDsvDesc();
                    dsDesc.SetDefault();
                    dsDesc.Type = NxRHI.EDsvType.DSV_Texture2DArray;
                    dsDesc.Format = desc.Format;
                    dsDesc.Width = desc.Width;
                    dsDesc.Height = desc.Height;
                    dsDesc.MipLevel = 0;
                    dsDesc.ArrayIndex = (uint)i;

                    mDSViews[i] = TtEngine.Instance.GfxDevice.RenderContext.CreateDSV(DepthTextureArray, in dsDesc);
                }
            }

            var attachment = ImportAttachment(DepthPinOut, DepthAttachment);
            attachment.GpuResource = DepthTextureArray;

            mVisParameter.CullType = GamePlay.TtWorld.TtVisParameter.EVisCull.Shadow;
            mVisParameter.IsBuildAABB = true;
            mVisParameter.World = world;
            mVisParameter.IsGatherVisibleNodes = false;

            policy.QueueCmd((ICommandList ImCmdlist, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent("AdvShadowDrawShadowMap");
            }, "BeginAdvShadowDrawShadowMap");
            foreach (var i in mShadowQTree.UpdateShadowMapNodes)
            {
                DrawShadowObjects(world, policy, i, i.ShadowObjects);
            }
            policy.QueueCmd((ICommandList ImCmdlist, ref FRCmdInfo info) =>
            {
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent("AdvShadowDrawShadowMap");
            }, "EndAdvShadowDrawShadowMap");
            mVisParameter.ClearVisibles();
        }
        private void DrawShadowObjects(GamePlay.TtWorld world, TtRenderPolicy policy, TtQNode node, List<TtShadowObject> shadowObjects)
        {
            if (node.Leaf.IsDirty == false)
                return;
            node.Leaf.UpdateShadowMatrix(world);
            mVisParameter.ClearVisibles();
            mVisParameter.CullCamera = node.Leaf.ShadowCamera;
            foreach (var j in shadowObjects)
            {
                j.SceneNode.OnGatherVisibleMeshes(mVisParameter);
            }
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist))
            {
                mGBuffer.SetDepthStencil(mDSViews[node.PageIndex]);
                //mGBuffer.SetDepthStencil(Dsv);
                mGBuffer.FlushModify();
                DrawDepth(cmdlist, world, policy);
            }
            policy.CommitCommandList(cmdlist);
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

            FViewPort Viewport = new FViewPort();
            Viewport.MinDepth = mGBuffer.Viewport.MinDepth;
            Viewport.MaxDepth = mGBuffer.Viewport.MaxDepth;
            Viewport.TopLeftX = 0;
            Viewport.TopLeftY = 0;
            Viewport.Width = (mGBuffer.Viewport.Width);// GBuffers.Viewport.Width;
            Viewport.Height = mGBuffer.Viewport.Height;

            cmdlist.SetViewport(in Viewport);
            var scissor = new NxRHI.FScissorRect();
            scissor.MinX = 0;
            scissor.MinY = 0;
            scissor.MaxX = (int)Viewport.Width;
            scissor.MaxY = (int)Viewport.Height;
            cmdlist.SetScissor(in scissor);

            var passClear = new NxRHI.FRenderPassClears();
            {
                passClear.SetDefault();
                passClear.SetClearColor(0, new Color4f(1, 1, 0, 0));
            }
            cmdlist.BeginPass(mGBuffer.FrameBuffers, in passClear, "AdvShadowDepth");
            cmdlist.FlushDraws();
            cmdlist.EndPass();
        }

        public unsafe void OnDirLightingDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            if (mShadowQTree == null)
                return;

            var index = drawcall.FindBinder("GShadowMapArray");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, DepthTextureArraySRV);
            }
            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }
            index = drawcall.FindBinder("QTreeNodeBuffer");
            if (index.IsValidPointer)
            {
                mShadowQTree.AdvShadowNodeDatas.Flush2GPU(cmd);
                drawcall.BindSRV(index, mShadowQTree.AdvShadowNodeDatas.Srv);
            }
            index = drawcall.FindBinder("cbAdvanceShadow");
            if (index.IsValidPointer)
            {
                if (mDirLightingCBV == null)
                {
                    mDirLightingCBV = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                    mDirLightingCBV.SetValue("BoxMin", mShadowQTree.Root.AABB.Minimum.AsSingleVector());
                    mDirLightingCBV.SetValue("BoxMax", mShadowQTree.Root.AABB.Maximum.AsSingleVector());
                    mDirLightingCBV.SetValue("NodeCount", mShadowQTree.QNodes.Length);
                    mDirLightingCBV.SetValue("PageCount", mShadowQTree.MaxPageCount);
                    mDirLightingCBV.SetValue("MaxShadowDistance", mShadowQTree.MaxShadowDistance);
                    mDirLightingCBV.SetValue("MaxDeepLevel", mShadowQTree.MaxDeepLevel);

                    var indexLayerData = index.FindField("LayerData");
                    for (int i = 0; i < mShadowQTree.QTreeBuilder.Layers.Length; i++)
                    {
                        var layer = mShadowQTree.QTreeBuilder.Layers[i];
                        FAdvShadowLayerData layerData;
                        layerData.mLayerStartAndSide.X = layer.LayerStartIndex;
                        layerData.mLayerStartAndSide.Y = layer.Side;
                        layerData.mLayerGridSize = layer.GridSize.AsSingleVector();

                        mDirLightingCBV.SetValue(indexLayerData, i, in layerData); 
                    }
                }
                drawcall.BindCBV(index, mDirLightingCBV);
            }
        }
    }
}
