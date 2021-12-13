﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHitproxyShading : Shader.UShadingEnv
    {
        public UHitproxyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/HitProxy.cginc", RName.ERNameType.Engine);
        }
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position};
        }
    }
    public class UHitproxyNode : URenderGraphNode
    {
        #region GetHitproxy
        private Support.CBlobObject mHitProxyData = new Support.CBlobObject();
        unsafe private ITexture2D* mReadableHitproxyTexture;
        public ITexture2D ReadableHitproxyTexture
        {
            get
            {
                unsafe
                {
                    return new ITexture2D(mReadableHitproxyTexture);
                }
            }
        }
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

            if (mReadableHitproxyTexture == (ITexture2D*)0)
                return 0;

            byte* pPixelData = (byte*)mHitProxyData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return 0;
            var pBitmapDesc = (PixelDesc*)pPixelData;
            pPixelData += sizeof(PixelDesc);

            IntColor* HitProxyIdArray = (IntColor*)pPixelData;

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
                    IntColor* TempHitProxyIdCache = &HitProxyIdArray[PointY * pBitmapDesc->Width];
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
        public UGraphicsBuffers GHitproxyBuffers { get; protected set; } = new UGraphicsBuffers();
        public UGraphicsBuffers GGizmosBuffers { get; protected set; } = new UGraphicsBuffers();
        public Common.UHitproxyShading mHitproxyShading;
        public UPassDrawBuffers HitproxyPass = new UPassDrawBuffers();
        public RHI.CRenderPass HitproxyRenderPass;
        public RHI.CRenderPass GizmosRenderPass;
        private RHI.CFence mReadHitproxyFence;
        bool CanDrawHitproxy = true;
        public float ScaleFactor { get; set; } = 0.5f;
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            HitproxyPass.Initialize(rc, debugName);

            var HitproxyPassDesc = new IRenderPassDesc();
            unsafe
            {
                HitproxyPassDesc.NumOfMRT = 1;
                HitproxyPassDesc.AttachmentMRTs[0].Format = fmt;
                HitproxyPassDesc.AttachmentMRTs[0].Samples = 1;
                HitproxyPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                HitproxyPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                HitproxyPassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                HitproxyPassDesc.m_AttachmentDepthStencil.Samples = 1;
                HitproxyPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                HitproxyPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                HitproxyPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                HitproxyPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //HitproxyPassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
                //HitproxyPassDesc.mDepthClearValue = 1.0f;
                //HitproxyPassDesc.mStencilClearValue = 0u;
            }            
            HitproxyRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in HitproxyPassDesc);

            var GizmosPassDesc = new IRenderPassDesc();
            unsafe
            {
                GizmosPassDesc.NumOfMRT = 1;
                GizmosPassDesc.AttachmentMRTs[0].Format = fmt;
                GizmosPassDesc.AttachmentMRTs[0].Samples = 1;
                GizmosPassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                GizmosPassDesc.m_AttachmentDepthStencil.Samples = 1;
                GizmosPassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                GizmosPassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //GizmosPassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
                //GizmosPassDesc.mDepthClearValue = 1.0f;
                //GizmosPassDesc.mStencilClearValue = 0u;
            }
            GizmosRenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in GizmosPassDesc);

            mHitproxyShading = shading as Common.UHitproxyShading;

            GHitproxyBuffers.Initialize(HitproxyRenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GHitproxyBuffers.CreateGBuffer(0, fmt, (uint)x, (uint)y);
            GHitproxyBuffers.TargetViewIdentifier = policy.GetBasePassNode().GBuffers.TargetViewIdentifier;
            GHitproxyBuffers.UpdateFrameBuffers(x, y);

            GGizmosBuffers.Initialize(GizmosRenderPass, policy.Camera, 1, dsFmt, (uint)x, (uint)y);
            GGizmosBuffers.SetGBuffer(0, GHitproxyBuffers.GetGBufferSRV(0), true);
            GGizmosBuffers.TargetViewIdentifier = GHitproxyBuffers.TargetViewIdentifier;
            GGizmosBuffers.UpdateFrameBuffers(x, y);

            mReadHitproxyFence = rc.CreateFence();
        }
        public unsafe void Cleanup()
        {
            mReadHitproxyFence?.Dispose();
            mReadHitproxyFence = null;

            if (mReadableHitproxyTexture != (ITexture2D*)0)
            {
                ReadableHitproxyTexture.NativeSuper.NativeSuper.NativeSuper.Release();
                mReadableHitproxyTexture = (ITexture2D*)0;
            }

            GHitproxyBuffers?.Cleanup();
            GHitproxyBuffers = null;

            GGizmosBuffers?.Cleanup();
            GGizmosBuffers = null;
        }
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (GHitproxyBuffers != null)
            {
                GHitproxyBuffers.OnResize(x * ScaleFactor, y * ScaleFactor);
            }
            if (GGizmosBuffers != null)
            {
                GGizmosBuffers.SetGBuffer(0, GHitproxyBuffers.GetGBufferSRV(0), true);
                GGizmosBuffers.OnResize(x, y);
            }
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            if (CanDrawHitproxy == false)
                return;
            
            HitproxyPass.ClearMeshDrawPassArray();
            HitproxyPass.SetViewport(GHitproxyBuffers.ViewPort);
            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                if (i.IsDrawHitproxy)
                {
                    for (int j = 0; j < i.Atoms.Length; j++)
                    {
                        var hpDrawcall = i.GetDrawCall(GHitproxyBuffers, j, policy, IRenderPolicy.EShadingType.HitproxyPass, this);
                        if (hpDrawcall != null)
                        {
                            hpDrawcall.BindGBuffer(GHitproxyBuffers);

                            var layer = i.Atoms[j].Material.RenderLayer;
                            HitproxyPass.PushDrawCall(layer, hpDrawcall);
                        }
                    }
                }   
            }

            var passClears = new IRenderPassClears();
            passClears.SetDefault();
            passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
            HitproxyPass.BuildRenderPass(in passClears, GHitproxyBuffers.FrameBuffers, GGizmosBuffers.FrameBuffers);

            var cmdlist_hp = HitproxyPass.PassBuffers[0].DrawCmdList.mCoreObject;
            fixed (ITexture2D** ppTexture = &mReadableHitproxyTexture)
            {
                cmdlist_hp.CreateReadableTexture2D(ppTexture, GHitproxyBuffers.GetGBufferSRV(0).mCoreObject, GHitproxyBuffers.FrameBuffers.mCoreObject);
            }
            cmdlist_hp.Signal(mReadHitproxyFence.mCoreObject, 0);
            CanDrawHitproxy = false;
        }
        public unsafe override void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            HitproxyPass.Commit(rc);
        }
        public unsafe override void TickSync(IRenderPolicy policy)
        {
            var cmdlist_hp = HitproxyPass.PassBuffers[0].CommitCmdList.mCoreObject;
            if (mReadHitproxyFence.mCoreObject.IsCompletion())
            {
                if (mReadableHitproxyTexture != (ITexture2D*)0)
                {
                    void* pData;
                    uint rowPitch;
                    uint depthPitch;
                    if (ReadableHitproxyTexture.MapMipmap(cmdlist_hp, 0, &pData, &rowPitch, &depthPitch) != 0)
                    {
                        ReadableHitproxyTexture.BuildImageBlob(mHitProxyData.mCoreObject, pData, rowPitch);
                        ReadableHitproxyTexture.UnmapMipmap(cmdlist_hp, 0);
                    }
                }
                CanDrawHitproxy = true;
            }
            HitproxyPass.SwapBuffer();
        }
    }
}