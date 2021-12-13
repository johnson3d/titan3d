using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UGraphicsBuffers
    {
        public class UTargetViewIdentifier
        {
            static int CurrentTargetViewId = 0;
            public UTargetViewIdentifier()
            {
                TargetViewId = CurrentTargetViewId++;
                if (CurrentTargetViewId == int.MaxValue)
                    CurrentTargetViewId = 0;
            }
            ~UTargetViewIdentifier()
            {
                TargetViewId = -1;
            }
            public int TargetViewId;
        }
        public UTargetViewIdentifier TargetViewIdentifier;
        //既然是一个可以作为渲染目标的缓冲，那么总要一个摄像机观察才有意义
        public CCamera Camera { get; set; }
        public RHI.CViewPort ViewPort = new RHI.CViewPort();
        public RHI.CFrameBuffers FrameBuffers;
        public bool IsCreatedDepthStencil = true;
        public RHI.CDepthStencilView DepthStencilView;
        private RHI.CShaderResourceView DepthStencilSRV;
        private RHI.CShaderResourceView[] GBufferSRV;
        public RHI.CShaderResourceView GetDepthStencilSRV()
        {
            return DepthStencilSRV;
        }
        public RHI.CShaderResourceView GetGBufferSRV(int index)
        {
            if (GBufferSRV == null)
                return null;
            if (index >= GBufferSRV.Length)
                return null;
            return GBufferSRV[index];
        }
        public struct RenderTargetDesc
        {
            public uint Width;
            public uint Height;
            public bool IsWeakRef;
        }
        public RenderTargetDesc[] RTDesc;
        RHI.CConstantBuffer mPerViewportCBuffer;
        public RHI.CConstantBuffer PerViewportCBuffer
        {
            get
            {
                if (mPerViewportCBuffer == null)
                {
                    var effect = UEngine.Instance.GfxDevice.EffectManager.DummyEffect;
                    mPerViewportCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(effect.ShaderProgram, effect.CBPerViewportIndex);
                    PerViewportCBuffer.mCoreObject.NativeSuper.SetDebugName($"Viewport");
                    UpdateViewportCBuffer();
                }
                return mPerViewportCBuffer;
            }
        }
        public void Cleanup()
        {
            FrameBuffers?.Dispose();
            FrameBuffers = null;
            if (IsCreatedDepthStencil)
            {
                DepthStencilView?.Dispose();
                DepthStencilSRV?.Dispose();
            }
            DepthStencilView = null;
            DepthStencilSRV = null;

            if (GBufferSRV != null)
            {
                foreach(var i in GBufferSRV)
                {
                    i?.Dispose();
                }
                GBufferSRV = null;
            }
        }
        public virtual void Initialize(RHI.CRenderPass renderPass, CCamera camera, int NumOfGBuffer, EPixelFormat dsFormat, uint width, uint height, uint dsMipLevels = 1, uint dsRtv = 0)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            FrameBuffers?.Dispose();
            DepthStencilView?.Dispose();

            ViewPort.mCoreObject.TopLeftX = 0;
            ViewPort.mCoreObject.TopLeftY = 0;
            ViewPort.mCoreObject.Width = (float)width;
            ViewPort.mCoreObject.Height = (float)height;
            ViewPort.mCoreObject.MinDepth = 0;
            ViewPort.mCoreObject.MaxDepth = 1;

            GBufferSRV = new RHI.CShaderResourceView[NumOfGBuffer];
            RTDesc = new RenderTargetDesc[NumOfGBuffer];
            var fbDesc = new IFrameBuffersDesc();
            fbDesc.RenderPass = renderPass.mCoreObject;
            FrameBuffers = rc.CreateFrameBuffers(ref fbDesc);
            if (dsFormat != EPixelFormat.PXF_UNKNOWN)
            {
                if (false == CreateDepthStencilBuffer(dsFormat, width, height, dsMipLevels, dsRtv))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Graphics", $"CreateDepthStencilBuffer failed: Format->{dsFormat}, MipLevels->{dsMipLevels}");
                    return;
                }
            }
            Camera = camera;
            if (Camera != null)
                Camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, width, height, 0.3f, 1000.0f);

            //UpdateFrameBuffers();
            UpdateViewportCBuffer();
        }
        public virtual void Initialize(RHI.CRenderPass renderPass, CCamera camera, int NumOfGBuffer, RHI.CDepthStencilView depthStencilView, RHI.CShaderResourceView depthStencilSRV, uint width, uint height)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            FrameBuffers?.Dispose();
            DepthStencilView?.Dispose();

            ViewPort.mCoreObject.TopLeftX = 0;
            ViewPort.mCoreObject.TopLeftY = 0;
            ViewPort.mCoreObject.Width = (float)width;
            ViewPort.mCoreObject.Height = (float)height;
            ViewPort.mCoreObject.MinDepth = 0;
            ViewPort.mCoreObject.MaxDepth = 1;

            GBufferSRV = new RHI.CShaderResourceView[NumOfGBuffer];
            RTDesc = new RenderTargetDesc[NumOfGBuffer];
            var fbDesc = new IFrameBuffersDesc();
            fbDesc.RenderPass = renderPass.mCoreObject;
            FrameBuffers = rc.CreateFrameBuffers(ref fbDesc);

            IsCreatedDepthStencil = false;
            DepthStencilSRV = depthStencilSRV;
            DepthStencilView = depthStencilView;

            if (depthStencilView != null)
            {
                FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject);
            }

            Camera = camera;
            if (camera != null)
                Camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, width, height, 0.3f, 1000.0f);

            UpdateViewportCBuffer();
        }
        public bool UpdateFrameBuffers(float width, float height)
        {
            return FrameBuffers.mCoreObject.UpdateFrameBuffers(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, width, height);
        }
        public RHI.CSwapChain SwapChain;
        public int SwapChainIndex
        {
            get
            {
                if (FrameBuffers == null)
                    return -1;
                return FrameBuffers.mCoreObject.mSwapChainIndex;
            }
        }
        public bool BindSwapChain(int index, RHI.CSwapChain swapchain)
        {
            FrameBuffers.mCoreObject.BindSwapChain((uint)index, swapchain.mCoreObject);
            return true;
        }
        public bool CreateGBuffer(int index, ITexture2D showTarget)
        {
            if (GBufferSRV == null || index >= GBufferSRV.Length || index < 0)
                return false;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            GBufferSRV[index]?.Dispose();

            var rtDesc = new IRenderTargetViewDesc();
            rtDesc.SetTexture2D();
            unsafe
            {
                var tex2dDesc = showTarget.mTextureDesc;
                rtDesc.Format = tex2dDesc.Format;//EPixelFormat.PXF_B8G8R8A8_UNORM;
                rtDesc.mGpuBuffer = showTarget.NativeSuper;
                rtDesc.Width = tex2dDesc.Width;
                rtDesc.Height = tex2dDesc.Height;
                var SwapChainRT = rc.CreateRenderTargetView(ref rtDesc);
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)index, SwapChainRT.mCoreObject);

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetTexture2D();
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Format = tex2dDesc.Format;
                srvDesc.mGpuBuffer = showTarget.NativeSuper;
                srvDesc.Texture2D.MipLevels = 1;
                var SwapChainSRV = rc.CreateShaderResourceView(in srvDesc);
                GBufferSRV[index] = SwapChainSRV;

                RTDesc[index].Width = rtDesc.Width;
                RTDesc[index].Height = rtDesc.Height;
            }
            return true;
        }
        public bool SetGBuffer(int index, RHI.CShaderResourceView srv, bool IsWeakRef = true)
        {
            if (GBufferSRV == null || index >= GBufferSRV.Length || index < 0)
                return false;

            if (RTDesc[index].IsWeakRef == false)
                GBufferSRV[index]?.Dispose();
            GBufferSRV[index] = srv;
            unsafe
            {
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                var rtDesc = new IRenderTargetViewDesc();
                rtDesc.SetTexture2D();
                var texture2d = srv.mCoreObject.GetGpuBuffer();
                
                rtDesc.Format = srv.mCoreObject.mSrvDesc.Format;
                rtDesc.mGpuBuffer = texture2d;
                rtDesc.Width = (uint)srv.mCoreObject.mTxDesc.Width; //tex2dDesc.Width;
                rtDesc.Height = (uint)srv.mCoreObject.mTxDesc.Height; //tex2dDesc.Height;
                var rTarget = rc.CreateRenderTargetView(ref rtDesc);
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)index, rTarget.mCoreObject);

                RTDesc[index].IsWeakRef = IsWeakRef;
                RTDesc[index].Width = rtDesc.Width;
                RTDesc[index].Height = rtDesc.Height;
            }

            return true;
        }
        public void SetDepthStencilBuffer(RHI.CDepthStencilView depthStencilView, RHI.CShaderResourceView depthStencilSRV)
        {
            IsCreatedDepthStencil = false;
            DepthStencilSRV = depthStencilSRV;
            DepthStencilView = depthStencilView;

            if (FrameBuffers != null && depthStencilView != null)
            {
                FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject);
            }
        }
        public bool CreateGBuffer(int index, EPixelFormat format, uint width, uint height, uint mipLevels = 1, uint rtv = 0)
        {
            if (GBufferSRV == null || index >= GBufferSRV.Length || index < 0)
                return false;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            GBufferSRV[index]?.Dispose();

            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.MipLevels = mipLevels;
            dsTexDesc.Width = width;
            dsTexDesc.Height = height;
            dsTexDesc.Format = format;
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_RENDER_TARGET | EBindFlags.BF_UNORDERED_ACCESS);
            var showTarget = rc.CreateTexture2D(in dsTexDesc);

            var rtDesc = new IRenderTargetViewDesc();
            unsafe
            {
                rtDesc.SetTexture2D();
                rtDesc.Type = ERtvType.RTV_Texture2D;
                rtDesc.Format = format;
                rtDesc.mGpuBuffer = showTarget.mCoreObject.NativeSuper;
                rtDesc.Width = width;
                rtDesc.Height = height;
                rtDesc.Texture2D.MipSlice = rtv;
                var rt = rc.CreateRenderTargetView(ref rtDesc);
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)index, rt.mCoreObject);

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetTexture2D();
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Format = format;
                srvDesc.mGpuBuffer = showTarget.mCoreObject.NativeSuper;
                srvDesc.Texture2D.MipLevels = mipLevels;
                var srv = rc.CreateShaderResourceView(in srvDesc);
                GBufferSRV[index] = srv;

                RTDesc[index].Width = rtDesc.Width;
                RTDesc[index].Height = rtDesc.Height;
            }
            return true;
        }
        public bool CreateDepthStencilBuffer(EPixelFormat dsFormat, uint width, uint height, uint mipLevels = 1, uint rtv = 0)
        {                
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            DepthStencilView?.Dispose();
            DepthStencilSRV?.Dispose();

            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = width;
            dsTexDesc.Height = height;
            switch (dsFormat)
            {
                case EPixelFormat.PXF_D24_UNORM_S8_UINT:
                    dsTexDesc.Format = EPixelFormat.PXF_R24G8_TYPELESS;
                    break;
                case EPixelFormat.PXF_D32_FLOAT:
                    dsTexDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                    break;
                case EPixelFormat.PXF_D16_UNORM:
                    dsTexDesc.Format = EPixelFormat.PXF_R16_TYPELESS;
                    break;
                case EPixelFormat.PXF_UNKNOWN:
                    dsTexDesc.Format = EPixelFormat.PXF_R16_TYPELESS;
                    break;
                default:
                    dsTexDesc.Format = dsFormat;
                    break;
            }
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_DEPTH_STENCIL);// | EBindFlags.BF_UNORDERED_ACCESS);
            dsTexDesc.MipLevels = mipLevels;
            var DepthStencilTexture = rc.CreateTexture2D(in dsTexDesc);
            if (DepthStencilTexture == null)
                return false;

            var dsvDesc = new IDepthStencilViewDesc();
            dsvDesc.SetDefault();
            dsvDesc.Format = dsFormat;
            dsvDesc.Width = width;
            dsvDesc.Height = height;
            dsvDesc.MipLevel = rtv;
            unsafe
            {
                dsvDesc.m_pTexture2D = DepthStencilTexture.mCoreObject;
                DepthStencilView = rc.CreateDepthRenderTargetView(ref dsvDesc);
                if (DepthStencilView == null)
                    return false;
                FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject);

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.SetTexture2D();
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Format = dsFormat;
                srvDesc.ViewDimension = SRV_DIMENSION.SRV_DIMENSION_TEXTURE2D;
                srvDesc.Texture2D.MipLevels = mipLevels;
                srvDesc.Texture2D.MostDetailedMip = 0;
                srvDesc.mGpuBuffer = DepthStencilTexture.mCoreObject.NativeSuper;
                DepthStencilSRV = rc.CreateShaderResourceView(in srvDesc);
                if (DepthStencilSRV == null)
                    return false;
            }
            return true;
        }
        public RHI.CUnorderedAccessView CreateUAV(int index, uint mipmap = 0)
        {
            var srv = GBufferSRV[index];
            var desc = new IUnorderedAccessViewDesc();
            desc.SetTexture2D();
            desc.Format = srv.mCoreObject.mSrvDesc.Format;
            desc.Texture2D.MipSlice = mipmap;

            return UEngine.Instance.GfxDevice.RenderContext.CreateUnorderedAccessView(GBufferSRV[index].mCoreObject.GetGpuBuffer(), in desc);
        }
        public unsafe void OnResize(float x, float y)
        {
            if (x < 1.0f)
                x = 1.0f;
            if (y < 1.0f)
                y = 1.0f;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (rc == null)
                return;

            if (IsCreatedDepthStencil && DepthStencilSRV != null)
            {
                var tex = new ITexture2D(DepthStencilView.mCoreObject.GetTexture2D());
                var desc = tex.mTextureDesc;
                var scaleX = (float)desc.Width / ViewPort.mCoreObject.Width;
                var scaleY = (float)desc.Height / ViewPort.mCoreObject.Height;
                desc.Width = (uint)(x * scaleX);
                if (desc.Width == 0)
                {
                    desc.Width = 1;
                }
                desc.Height = (uint)(y * scaleY);
                if (desc.Height == 0)
                {
                    desc.Height = 1;
                }

                var DepthStencilTexture = rc.CreateTexture2D(in desc);
                //if(DepthStencilTexture!=null)
                {
                    var dsvDesc = new IDepthStencilViewDesc();
                    dsvDesc.SetDefault();
                    dsvDesc.Format = DepthStencilSRV.mCoreObject.mSrvDesc.Format;
                    dsvDesc.Width = desc.Width;
                    dsvDesc.Height = desc.Height;
                    dsvDesc.m_pTexture2D = DepthStencilTexture.mCoreObject;

                    DepthStencilView.Dispose();
                    DepthStencilView = rc.CreateDepthRenderTargetView(ref dsvDesc);
                    FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject);

                    var srvDesc = new IShaderResourceViewDesc();
                    srvDesc.SetTexture2D();
                    srvDesc.Type = ESrvType.ST_Texture2D;
                    srvDesc.Format = DepthStencilSRV.mCoreObject.mSrvDesc.Format;
                    srvDesc.mGpuBuffer = DepthStencilTexture.mCoreObject.NativeSuper;
                    srvDesc.Texture2D.MipLevels = 1;
                    DepthStencilSRV.Dispose();
                    DepthStencilSRV = rc.CreateShaderResourceView(in srvDesc);
                }
            }

            if (GBufferSRV != null)
            {
                for (int i = 0; i < GBufferSRV.Length; i++)
                {
                    if (GBufferSRV[i] == null)
                        continue;
                    if (RTDesc[i].IsWeakRef)
                        continue;
                    if (SwapChainIndex == i)
                        continue;

                    var tex = new ITexture2D(GBufferSRV[i].mCoreObject.GetGpuBuffer());
                    var desc = tex.mTextureDesc;
                    var scaleX = (float)desc.Width / ViewPort.mCoreObject.Width;
                    var scaleY = (float)desc.Height / ViewPort.mCoreObject.Height;
                    desc.Width = (uint)(x * scaleX);
                    if (desc.Width == 0)
                    {
                        desc.Width = 1;
                    }
                    desc.Height = (uint)(y * scaleY);
                    if (desc.Height == 0)
                    {
                        desc.Height = 1;
                    }

                    var showTarget = rc.CreateTexture2D(in desc);

                    var rtDesc = new IRenderTargetViewDesc();
                    rtDesc.SetTexture2D();
                    rtDesc.Type = ERtvType.RTV_Texture2D;
                    rtDesc.Format = desc.Format;
                    rtDesc.mGpuBuffer = showTarget.mCoreObject.NativeSuper;
                    rtDesc.Width = desc.Width;
                    rtDesc.Height = desc.Height;
                    var SwapChainRT = rc.CreateRenderTargetView(ref rtDesc);
                    FrameBuffers.mCoreObject.BindRenderTargetView((uint)i, SwapChainRT.mCoreObject);

                    var srvDesc = new IShaderResourceViewDesc();
                    srvDesc.SetTexture2D();
                    srvDesc.Type = ESrvType.ST_Texture2D;
                    srvDesc.Format = desc.Format;
                    srvDesc.mGpuBuffer = showTarget.mCoreObject.NativeSuper;
                    srvDesc.Texture2D.MipLevels = 1;
                    var SwapChainSRV = rc.CreateShaderResourceView(in srvDesc);
                    GBufferSRV[i].Dispose();
                    GBufferSRV[i] = SwapChainSRV;
                    RTDesc[i].Width = desc.Width;
                    RTDesc[i].Height = desc.Height;
                }
            }

            ViewPort.mCoreObject.Width = (float)x;
            ViewPort.mCoreObject.Height = (float)y;

            if (Camera != null)
                Camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, x, y, Camera.mCoreObject.mZNear, Camera.mCoreObject.mZFar);

            UpdateFrameBuffers(x, y);
            UpdateViewportCBuffer();
        }
        public void UpdateViewportCBuffer()
        {
            unsafe
            {
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;

                    Vector4 gViewportSizeAndRcp = new Vector4(ViewPort.mCoreObject.Width, ViewPort.mCoreObject.Height, 1 / ViewPort.mCoreObject.Width, 1 / ViewPort.mCoreObject.Height);
                    PerViewportCBuffer.SetValue(indexer.gViewportSizeAndRcp, in gViewportSizeAndRcp);
                }
            }
        }
    }
}


namespace EngineNS.RHI
{
    public partial class CDrawCall
    {
        public void BindGBuffer(Graphics.Pipeline.UGraphicsBuffers GBuffers)
        {
            unsafe
            {
                if (GBuffers.PerViewportCBuffer != null)
                    mCoreObject.BindShaderCBuffer(Effect.CBPerViewportIndex, GBuffers.PerViewportCBuffer.mCoreObject);
                if (GBuffers.Camera.PerCameraCBuffer != null)
                    mCoreObject.BindShaderCBuffer(Effect.CBPerCameraIndex, GBuffers.Camera.PerCameraCBuffer.mCoreObject);
            }
        }
    }
}