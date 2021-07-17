using System;
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
        UInt32 mHitCheckRegion = 2;
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
        private unsafe UInt32 GetHitProxyIDImpl(UInt32 MouseX, UInt32 MouseY)
        {
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
                        var hitId = UHitProxy.ConvertCpuTexColorToHitProxyId(TempHitProxyIdCache[PointX]);
                        if (hitId != 0)
                            return hitId;
                        HitProxyArrayIdx++;
                    }
                }
            }
            return 0;
        }
        #endregion
        public UGraphicsBuffers GHitproxyBuffers { get; protected set; } = new UGraphicsBuffers();
        public Common.UHitproxyShading mHitproxyShading;
        public UDrawBuffers HitproxyPass = new UDrawBuffers();
        public RenderPassDesc HitproxyPassDesc = new RenderPassDesc();
        private RHI.CFence mReadHitproxyFence;
        bool CanDrawHitproxy = true;
        public async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, float x, float y)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            HitproxyPass.Initialize(rc);

            mHitproxyShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Common.UHitproxyShading>();

            GHitproxyBuffers.SwapChainIndex = -1;
            GHitproxyBuffers.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)x, (uint)y);
            GHitproxyBuffers.CreateGBuffer(0, EPixelFormat.PXF_R8G8B8A8_UNORM, (uint)x, (uint)y);
            GHitproxyBuffers.TargetViewIdentifier = policy.GBuffers.TargetViewIdentifier;
            GHitproxyBuffers.Camera = policy.GBuffers.Camera;
            HitproxyPassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            HitproxyPassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            HitproxyPassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
            HitproxyPassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            HitproxyPassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            HitproxyPassDesc.mDepthClearValue = 1.0f;
            HitproxyPassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            HitproxyPassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            HitproxyPassDesc.mStencilClearValue = 0u;

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
        }
        public void OnResize(float x, float y)
        {
            if (GHitproxyBuffers != null)
            {
                GHitproxyBuffers.OnResize(x, y);
            }
        }
        public unsafe void TickLogic(IRenderPolicy policy)
        {
            if (CanDrawHitproxy == false)
                return;
            var cmdlist_hp = HitproxyPass.DrawCmdList.mCoreObject;
            cmdlist_hp.ClearMeshDrawPassArray();
            cmdlist_hp.SetViewport(GHitproxyBuffers.ViewPort.mCoreObject);
            foreach (var i in policy.VisibleMeshes)
            {
                if (i.Atoms == null)
                    continue;

                if (i.IsDrawHitproxy)
                {
                    for (int j = 0; j < i.Atoms.Length; j++)
                    {
                        var hpDrawcall = i.GetDrawCall(GHitproxyBuffers, j, policy, IRenderPolicy.EShadingType.HitproxyPass);
                        if (hpDrawcall != null)
                        {
                            GHitproxyBuffers.SureCBuffer(hpDrawcall.Effect, "UMobileEditorFSPolicy.HitproxyBuffers");
                            hpDrawcall.BindGBuffer(GHitproxyBuffers);

                            cmdlist_hp.PushDrawCall(hpDrawcall.mCoreObject);
                        }
                    }
                }   
            }

            cmdlist_hp.BeginCommand();
            cmdlist_hp.BeginRenderPass(ref HitproxyPassDesc, GHitproxyBuffers.FrameBuffers.mCoreObject);
            cmdlist_hp.BuildRenderPass(0);
            cmdlist_hp.EndRenderPass();
            cmdlist_hp.EndCommand();

            fixed (ITexture2D** ppTexture = &mReadableHitproxyTexture)
            {
                cmdlist_hp.CreateReadableTexture2D(ppTexture, GHitproxyBuffers.GBufferSRV[0].mCoreObject, GHitproxyBuffers.FrameBuffers.mCoreObject);
            }
            cmdlist_hp.Signal(mReadHitproxyFence.mCoreObject, 0);
            CanDrawHitproxy = false;
        }
        public unsafe void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = HitproxyPass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe void TickSync(IRenderPolicy policy)
        {
            var cmdlist_hp = HitproxyPass.CommitCmdList.mCoreObject;
            if (mReadHitproxyFence.mCoreObject.IsCompletion())
            {
                if (mReadableHitproxyTexture != (ITexture2D*)0)
                {
                    void* pData;
                    uint rowPitch;
                    uint depthPitch;
                    if (ReadableHitproxyTexture.Map(cmdlist_hp, 0, &pData, &rowPitch, &depthPitch) != 0)
                    {
                        ReadableHitproxyTexture.BuildImageBlob(mHitProxyData.mCoreObject, pData, rowPitch);
                        ReadableHitproxyTexture.Unmap(cmdlist_hp, 0);
                    }
                }
                CanDrawHitproxy = true;
            }
            HitproxyPass.SwapBuffer();
        }
    }
}
