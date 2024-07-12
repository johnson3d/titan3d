using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHitproxyShading : Shader.UGraphicsShadingEnv
    {
        public UHitproxyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/HitProxy.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position};
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Hitproxy", "Hitproxy", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UHitproxyNode : TtRenderGraphNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin HitIdPinOut = TtRenderGraphPin.CreateOutput("HitId", false, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public TtRenderGraphPin DepthPinInOut = TtRenderGraphPin.CreateInputOutput("Depth", false, EPixelFormat.PXF_D16_UNORM);
        public TtRenderGraphPin GizmosDepthPinOut = TtRenderGraphPin.CreateOutput("GizmosDepth", false, EPixelFormat.PXF_D16_UNORM);
        public UHitproxyNode()
        {
            Name = "Hitproxy";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            AddOutput(HitIdPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
            AddOutput(GizmosDepthPinOut, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV); 
            DepthPinInOut.IsAllowInputNull = true;
        }
        #region GetHitproxy
        private Support.UBlobObject mHitProxyData = new Support.UBlobObject();
        unsafe private NxRHI.UGpuResource mReadableHitproxyTexture;
        private NxRHI.UFence mCopyFence;
        private Int32 Clamp(Int32 ValueIn, Int32 MinValue, Int32 MaxValue)
        {
            return ValueIn < MinValue ? MinValue : ValueIn < MaxValue ? ValueIn : MaxValue;
        }
        public IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            var id = GetHitProxyID(MouseX, MouseY);
            if (id == 0)
                return null;
            return UEngine.Instance.GfxDevice.HitproxyManager.FindProxy(id);
        }

        public UInt32 GetHitProxyID(UInt32 MouseX, UInt32 MouseY)
        {
            unsafe
            {
                lock (this)
                {
                    return GetHitProxyIDImpl(MouseX, MouseY);
                }
            }
        }
        private const UInt32 mHitCheckRegion = 2;
        private const UInt32 mCheckRegionSamplePointCount = 25;
        private const UInt32 mCheckRegionCenter = 13;
        private UInt32[] mHitProxyIdArray = new UInt32[mCheckRegionSamplePointCount];
        private unsafe UInt32 GetHitProxyIDImpl(UInt32 MouseX, UInt32 MouseY)
        {
            MouseX = (UInt32)((float)MouseX * ScaleFactor);
            MouseY = (UInt32)((float)MouseY * ScaleFactor);

            if (mReadableHitproxyTexture == null)
                return 0;

            byte* pPixelData = (byte*)mHitProxyData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return 0;
            var pBitmapDesc = (NxRHI.FPixelDesc*)pPixelData;
            pPixelData += sizeof(NxRHI.FPixelDesc);

            var HitProxyIdArray = (Byte4*)pPixelData;

            Int32 RegionMinX = (Int32)(MouseX - mHitCheckRegion);
            Int32 RegionMinY = (Int32)(MouseY - mHitCheckRegion);
            Int32 RegionMaxX = (Int32)(MouseX + mHitCheckRegion);
            Int32 RegionMaxY = (Int32)(MouseY + mHitCheckRegion);

            RegionMinX = Clamp(RegionMinX, 0, (Int32)pBitmapDesc->Width - 1);
            RegionMinY = Clamp(RegionMinY, 0, (Int32)pBitmapDesc->Height - 1);
            RegionMaxX = Clamp(RegionMaxX, 0, (Int32)pBitmapDesc->Width - 1);
            RegionMaxY = Clamp(RegionMaxY, 0, (Int32)pBitmapDesc->Height - 1);

            Int32 RegionSizeX = RegionMaxX - RegionMinX + 1;
            Int32 RegionSizeY = RegionMaxY - RegionMinY + 1;

            if (RegionSizeX > 0 && RegionSizeY > 0)
            {
                if (HitProxyIdArray == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "@Graphic", $"HitProxyError: Null Ptr!!!", "");
                    return 0;
                }

                int max = pBitmapDesc->Width * pBitmapDesc->Height;

                UInt32 HitProxyArrayIdx = 0;
                for (Int32 PointY = RegionMinY; PointY < RegionMaxY; PointY++)
                {
                    var TempHitProxyIdCache = &HitProxyIdArray[PointY * pBitmapDesc->Width];
                    for (Int32 PointX = RegionMinX; PointX < RegionMaxX; PointX++)
                    {
                        mHitProxyIdArray[HitProxyArrayIdx] = UHitProxy.ConvertCpuTexColorToHitProxyId(TempHitProxyIdCache[PointX]);
                        HitProxyArrayIdx++;
                    }
                }
                if (mHitProxyIdArray[mCheckRegionCenter] != 0)
                {
                    return mHitProxyIdArray[mCheckRegionCenter];
                }
                else
                {
                    for (UInt32 idx = 0; idx < mCheckRegionSamplePointCount; idx++)
                    {
                        if (mHitProxyIdArray[idx] != 0)
                        {
                            return mHitProxyIdArray[idx];
                        }
                    }
                }
            }
            return 0;
        }
        #endregion
        public TtGraphicsBuffers GHitproxyBuffers { get; protected set; } = new TtGraphicsBuffers();
        public TtGraphicsBuffers GGizmosBuffers { get; protected set; } = new TtGraphicsBuffers();
        public Common.UHitproxyShading mHitproxyShading;
        public TtLayerDrawBuffers HitproxyPass = new TtLayerDrawBuffers();
        public NxRHI.URenderPass HitproxyRenderPass;
        public NxRHI.URenderPass GizmosRenderPass;
        [Rtti.Meta]
        public float ScaleFactor { get; set; } = 0.5f;
        public override UGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mHitproxyShading;
        }
        public TtCpuCullingNode CpuCullNode = null;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            HitproxyPass.Initialize(rc, debugName);

            CreateGBuffers(policy, DepthPinInOut.Attachement.Format, true);
            mCopyFence = rc.CreateFence(new NxRHI.FFenceDesc(), "Copy Hitproxy Texture");

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.GetNakedHostNode<TtCpuCullingNode>();
            }
        }
        public unsafe void CreateGBuffers(URenderPolicy policy, EPixelFormat DSFormat, bool bClearDS)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var HitproxyPassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                HitproxyPassDesc.NumOfMRT = 1;
                HitproxyPassDesc.AttachmentMRTs[0].Format = HitIdPinOut.Attachement.Format;
                HitproxyPassDesc.AttachmentMRTs[0].Samples = 1;
                HitproxyPassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                HitproxyPassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                HitproxyPassDesc.m_AttachmentDepthStencil.Format = DSFormat;
                HitproxyPassDesc.m_AttachmentDepthStencil.Samples = 1;
                HitproxyPassDesc.m_AttachmentDepthStencil.LoadAction = bClearDS ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare; 
                HitproxyPassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                HitproxyPassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                HitproxyPassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //HitproxyPassDesc.mFBClearColorRT0 = new Color4f(0, 0, 0, 0);
                //HitproxyPassDesc.mDepthClearValue = 1.0f;
                //HitproxyPassDesc.mStencilClearValue = 0u;
            }
            HitproxyRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in HitproxyPassDesc);

            var GizmosPassDesc = new NxRHI.FRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].Format = HitIdPinOut.Attachement.Format;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = GizmosDepthPinOut.Attachement.Format;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in GizmosPassDesc);

            mHitproxyShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UHitproxyShading>();

            GHitproxyBuffers.Initialize(policy, HitproxyRenderPass);
            GHitproxyBuffers.SetRenderTarget(policy, 0, HitIdPinOut);
            GHitproxyBuffers.SetDepthStencil(policy, DepthPinInOut);
            GHitproxyBuffers.TargetViewIdentifier = new TtGraphicsBuffers.TtTargetViewIdentifier();// policy.DefaultCamera.TargetViewIdentifier;

            GGizmosBuffers.Initialize(policy, GizmosRenderPass);
            GGizmosBuffers.SetRenderTarget(policy, 0, HitIdPinOut);
            GGizmosBuffers.SetDepthStencil(policy, GizmosDepthPinOut);
            GGizmosBuffers.TargetViewIdentifier = GHitproxyBuffers.TargetViewIdentifier;
        }
        public unsafe override void Dispose()
        {
            mReadableHitproxyTexture?.Dispose();
            mReadableHitproxyTexture = null;
            
            GHitproxyBuffers?.Dispose();
            GHitproxyBuffers = null;

            GGizmosBuffers?.Dispose();
            GGizmosBuffers = null;

            base.Dispose();
        }
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {
            HitIdPinOut.Attachement.Width = (uint)(x * ScaleFactor);
            HitIdPinOut.Attachement.Height = (uint)(y * ScaleFactor);
            DepthPinInOut.Attachement.Width = (uint)(x * ScaleFactor);
            DepthPinInOut.Attachement.Height = (uint)(y * ScaleFactor);
            GizmosDepthPinOut.Attachement.Width = (uint)(x * ScaleFactor);
            GizmosDepthPinOut.Attachement.Height = (uint)(y * ScaleFactor); 
            if (GHitproxyBuffers != null)
            {
                GHitproxyBuffers.SetSize(x * ScaleFactor, y * ScaleFactor);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetSize(x * ScaleFactor, y * ScaleFactor);
            }

            CopyTexDesc.SetDefault();
            CopyTexDesc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
            CopyTexDesc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
            CopyTexDesc.Width = HitIdPinOut.Attachement.Width;
            CopyTexDesc.Height = HitIdPinOut.Attachement.Height;
            CopyTexDesc.Depth = 0;
            CopyTexDesc.Format = HitIdPinOut.Attachement.Format;
            CopyTexDesc.ArraySize = 1;
            CopyTexDesc.BindFlags = 0;
            CopyTexDesc.MipLevels = 1;

            mReadableHitproxyTexture?.Dispose();
            mReadableHitproxyTexture = null;
        }
        NxRHI.FTextureDesc CopyTexDesc = new NxRHI.FTextureDesc();
        NxRHI.FSubResourceFootPrint CopyBufferFootPrint = new NxRHI.FSubResourceFootPrint();
        bool IsHitproxyBuilding = false;
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UHitproxyNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            if (IsHitproxyBuilding)
                return;

            IsHitproxyBuilding = true;

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                using(new TtLayerDrawBuffers.TtLayerDrawBuffersScope(HitproxyPass))
                {
                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                    foreach (var i in CpuCullNode.VisParameter.VisibleMeshes)
                    {
                        if (i.Mesh.IsDrawHitproxy == false)
                        {
                            continue;
                        }
                        if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                            continue;
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            foreach (var k in j.Atoms)
                            {
                                if (k.Material == null)
                                    continue;

                                var layer = k.Material.RenderLayer;
                                var cmd = HitproxyPass.GetCmdList(layer);
                                var hpDrawcall = k.GetDrawCall(cmd.mCoreObject, GHitproxyBuffers, policy, this);
                                if (hpDrawcall != null)
                                {
                                    hpDrawcall.BindGBuffer(camera, GHitproxyBuffers);

                                    cmd.PushGpuDraw(hpDrawcall);
                                }
                            }
                        }
                    }

                    {
                        //draw mesh first
                        var passClears = stackalloc NxRHI.FRenderPassClears[(int)ERenderLayer.RL_Num];
                        for (int i = 0; i < (int)ERenderLayer.RL_Num; i++)
                        {
                            passClears[i].SetDefault();
                            passClears[i].SetClearColor(0, new Color4f(0, 0, 0, 0));
                            passClears[i].ClearFlags = 0;
                        }
                        passClears[(int)ERenderLayer.RL_Background].ClearFlags = NxRHI.ERenderPassClearFlags.CLEAR_ALL;
                        passClears[(int)ERenderLayer.RL_Gizmos].ClearFlags = NxRHI.ERenderPassClearFlags.CLEAR_DEPTH;

                        GHitproxyBuffers.BuildFrameBuffers(policy);
                        GGizmosBuffers.BuildFrameBuffers(policy);
                        HitproxyPass.BuildRenderPass(policy, in GHitproxyBuffers.Viewport, passClears, (int)ERenderLayer.RL_Num, GHitproxyBuffers, GGizmosBuffers, "Hitproxy:");
                    }
                }
                
                HitproxyPass.ExecuteCommands(policy);
            }
            
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            //copy to sys memory after draw all meshesr
            var cmdlist_post = HitproxyPass.PostCmds.DrawCmdList.mCoreObject;
            var attachBuffer = RenderGraph.AttachmentCache.FindAttachement(GHitproxyBuffers.RenderTargets[0].Attachement.AttachmentName);
            attachBuffer.Srv.mCoreObject.GetBufferAsTexture().SetDebugName("Hitproxy Source Texture");

            if (mReadableHitproxyTexture == null)
            {
                var rtTex = attachBuffer.GpuResource as NxRHI.UTexture;
                mReadableHitproxyTexture = rtTex.CreateBufferData(0, NxRHI.ECpuAccess.CAS_READ, ref CopyBufferFootPrint);
            }
            var readTexture = mReadableHitproxyTexture;
            cmdlist_post.BeginCommand();
            fixed(NxRHI.FSubResourceFootPrint* pFootprint = &CopyBufferFootPrint)
            {
                var cpDraw = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                var dstTex = readTexture as NxRHI.UTexture;
                var dstBf = readTexture as NxRHI.UBuffer;
                if (dstTex != null)
                {
                    //cmdlist_post.CopyTextureRegion(dstTex.mCoreObject, 0, 0, 0, 0, attachBuffer.Srv.mCoreObject.GetBufferAsTexture(), 0, (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer());
                    cpDraw.mCoreObject.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                    cpDraw.BindTextureDest(dstTex);
                    cpDraw.mCoreObject.BindTextureSrc(attachBuffer.Srv.mCoreObject.GetBufferAsTexture());
                    cmdlist_post.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                    cmdlist_post.FlushDraws();
                }
                else if (dstBf != null)
                {
                    //cmdlist_post.CopyTextureToBuffer(dstBf.mCoreObject, pFootprint, attachBuffer.Srv.mCoreObject.GetBufferAsTexture(), 0);
                    cpDraw.mCoreObject.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
                    cpDraw.BindBufferDest(dstBf);
                    cpDraw.mCoreObject.BindTextureSrc(attachBuffer.Srv.mCoreObject.GetBufferAsTexture());
                    cpDraw.mCoreObject.FootPrint = CopyBufferFootPrint;
                    cmdlist_post.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                    cmdlist_post.FlushDraws();
                }
            }
            cmdlist_post.EndCommand();
            policy.CommitCommandList(HitproxyPass.PostCmds.DrawCmdList);

            var fence = mCopyFence;
            UEngine.Instance.GfxDevice.RenderSwapQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.FRCmdInfo info) =>
            {
                rc.GpuQueue.IncreaseSignal(fence);
                var targetValue = fence.ExpectValue;
                var postTime = Support.Time.GetTickCount();
                UEngine.Instance.EventPoster.PostTickSyncEvent((tickCount) =>
                {
                    if (readTexture != mReadableHitproxyTexture || tickCount - postTime > 1000)
                    {
                        IsHitproxyBuilding = false;
                        return true;
                    }
                    if (fence.CompletedValue >= targetValue)
                    {
                        var gpuDataBlob = new Support.UBlobObject();
                        readTexture.GetGpuBufferDataPointer().FetchGpuData(0, gpuDataBlob.mCoreObject);
                        //var ptr = (uint*)gpuDataBlob.mCoreObject.GetData();
                        //var num = gpuDataBlob.mCoreObject.GetSize() / 4;
                        //for (int i = 2; i < num; i++)
                        //{
                        //    if (ptr[i] != 0)
                        //    {
                        //        int xxx = 0;
                        //    }
                        //}
                        NxRHI.ITexture.BuildImage2DBlob(mHitProxyData.mCoreObject, gpuDataBlob.mCoreObject, in CopyTexDesc);
                        IsHitproxyBuilding = false;

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }, "Signal Ready");

            

            //UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((im_cmd, name) =>
            //{
            //    var bufferData = new Support.CBlobObject();
            //    mReadableHitproxyTexture.mCoreObject.FetchGpuDataAsImage2DBlob(im_cmd, 0, 0, mHitProxyData.mCoreObject);
            //    UEngine.Instance.GfxDevice.RenderContext.CmdQueue.SignalFence(mReadHitproxyFence, 0);

            //    IsHitproxyBuilding = false;
            //}, "Fetch Image");
        }

        public override void BeforeTickLogic(URenderPolicy policy)
        {
            var buffer = this.FindAttachBuffer(DepthPinInOut);
            if (buffer != null)
            {
                if (DepthPinInOut.Attachement.Format != buffer.BufferDesc.Format || 
                       HitproxyRenderPass.mCoreObject.Desc.AttachmentDepthStencil.LoadAction == NxRHI.EFrameBufferLoadAction.LoadActionClear)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format, false);
                    DepthPinInOut.Attachement.Format = buffer.BufferDesc.Format;
                }
            }
            //else
            //{
            //    this.CreateGBuffers(policy, DepthPinInOut.Attachement.Format, true);
            //}
        }
        public unsafe override void TickSync(URenderPolicy policy)
        {
            HitproxyPass.SwapBuffer();
        }
    }
}
