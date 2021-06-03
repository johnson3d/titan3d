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
        public RHI.CDepthStencilView DepthStencilView;
        public RHI.CShaderResourceView[] GBufferSRV;
        public RHI.CConstantBuffer PerViewportCBuffer;
        public int SwapChainIndex = -1;
        public void SureCBuffer(Shader.UEffect effect, string debugName)
        {
            if (effect.CBPerViewportIndex != 0xFFFFFFFF)
            {
                var gpuProgram = effect.ShaderProgram;
                if (PerViewportCBuffer == null)
                {
                    PerViewportCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, effect.CBPerViewportIndex);
                    PerViewportCBuffer.mCoreObject.NativeSuper.SetDebugName($"{debugName}: Viewport");
                    UpdateViewportCBuffer();
                }
            }
            Camera.SureCBuffer(effect, debugName);
        }
        public void Cleanup()
        {
            FrameBuffers?.Dispose();
            FrameBuffers = null;
            DepthStencilView?.Dispose();
            DepthStencilView = null;

            if (GBufferSRV != null)
            {
                foreach(var i in GBufferSRV)
                {
                    i?.Dispose();
                }
                GBufferSRV = null;
            }
        }
        public virtual void Initialize(int NumOfGBuffer, EPixelFormat dsFormat, uint width, uint height)
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
            var fbDesc = new IFrameBuffersDesc();
            fbDesc.IsSwapChainBuffer = 1;
            fbDesc.UseDSV = 1;
            FrameBuffers = rc.CreateFrameBuffers(ref fbDesc);

            if (dsFormat == EPixelFormat.PXF_UNKNOWN)
                return;

            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = width;
            dsTexDesc.Height = height;
            dsTexDesc.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_DEPTH_STENCIL);
            var DepthStencilTexture = rc.CreateTexture2D(ref dsTexDesc);

            var dsvDesc = new IDepthStencilViewDesc();
            dsvDesc.SetDefault();
            dsvDesc.Width = width;
            dsvDesc.Height = height;
            unsafe
            {
                dsvDesc.m_pTexture2D = DepthStencilTexture.mCoreObject.Ptr;
                DepthStencilView = rc.CreateDepthRenderTargetView(ref dsvDesc);
                FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject.Ptr);
            }

            Camera = new CCamera();
            Camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, width, height, 0.3f, 1000.0f);
            var eyePos = new Vector3(0, 0, -10);
            Camera.mCoreObject.LookAtLH(ref eyePos, ref Vector3.Zero, ref Vector3.Up);

            SunLightColor = new Vector3(1, 1, 1);
            SunLightDirection = new Vector3(0, 0, 240);
            SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            UpdateViewportCBuffer();
        }
        public bool CreateGBuffer(int index, ITexture2D showTarget)
        {
            if (GBufferSRV == null || index >= GBufferSRV.Length || index < 0)
                return false;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            GBufferSRV[index]?.Dispose();

            var rtDesc = new IRenderTargetViewDesc();
            rtDesc.SetDefault();
            unsafe
            {
                var texture2d = new ITexture2D(showTarget.CppPointer);
                var tex2dDesc = texture2d.mDesc;
                rtDesc.Format = tex2dDesc.Format;//EPixelFormat.PXF_B8G8R8A8_UNORM;
                rtDesc.m_pTexture2D = showTarget.CppPointer;
                rtDesc.Width = tex2dDesc.Width;
                rtDesc.Height = tex2dDesc.Height;
                var SwapChainRT = rc.CreateRenderTargetView(ref rtDesc);
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)index, SwapChainRT.mCoreObject.Ptr);

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.mFormat = tex2dDesc.Format;
                srvDesc.m_pTexture2D = showTarget.CppPointer;
                var SwapChainSRV = rc.CreateShaderResourceView(ref srvDesc);
                GBufferSRV[index] = SwapChainSRV;
                return true;
            }
        }
        public bool CreateGBuffer(int index, EPixelFormat format, uint width, uint height)
        {
            if (GBufferSRV == null || index >= GBufferSRV.Length || index < 0)
                return false;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            GBufferSRV[index]?.Dispose();

            var dsTexDesc = new ITexture2DDesc();
            dsTexDesc.SetDefault();
            dsTexDesc.Width = width;
            dsTexDesc.Height = height;
            dsTexDesc.Format = format;
            dsTexDesc.BindFlags = (UInt32)(EBindFlags.BF_SHADER_RES | EBindFlags.BF_RENDER_TARGET);
            var showTarget = rc.CreateTexture2D(ref dsTexDesc);

            var rtDesc = new IRenderTargetViewDesc();
            unsafe
            {
                rtDesc.SetDefault();
                rtDesc.Format = format;
                rtDesc.m_pTexture2D = showTarget.mCoreObject.Ptr;
                rtDesc.Width = width;
                rtDesc.Height = height;
                var SwapChainRT = rc.CreateRenderTargetView(ref rtDesc);
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)index, SwapChainRT.mCoreObject.Ptr);

                var srvDesc = new IShaderResourceViewDesc();
                srvDesc.mFormat = format;
                srvDesc.m_pTexture2D = showTarget.mCoreObject.Ptr;
                var SwapChainSRV = rc.CreateShaderResourceView(ref srvDesc);
                GBufferSRV[index] = SwapChainSRV;
                return true;
            }
        }
        public unsafe void OnResize(float x, float y)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (rc == null)
                return;

            if (DepthStencilView != null)
            {
                var tex = new ITexture2D(DepthStencilView.mCoreObject.GetTexture2D());
                var desc = tex.mDesc;
                var scaleX = (float)desc.Width / ViewPort.mCoreObject.Width;
                var scaleY = (float)desc.Height / ViewPort.mCoreObject.Height;
                desc.Width = (uint)(x * scaleX);
                desc.Height = (uint)(y * scaleY);

                var DepthStencilTexture = rc.CreateTexture2D(ref desc);

                var dsvDesc = new IDepthStencilViewDesc();
                dsvDesc.SetDefault();
                dsvDesc.Width = desc.Width;
                dsvDesc.Height = desc.Height;
                dsvDesc.m_pTexture2D = DepthStencilTexture.mCoreObject.Ptr;

                DepthStencilView.Dispose();
                DepthStencilView = rc.CreateDepthRenderTargetView(ref dsvDesc);
                FrameBuffers.mCoreObject.BindDepthStencilView(DepthStencilView.mCoreObject.Ptr);
            }

            if (GBufferSRV != null)
            {
                for (int i = 0; i < GBufferSRV.Length; i++)
                {
                    if (GBufferSRV[i] == null)
                        continue;
                    if (SwapChainIndex == i)
                        continue;

                    var tex = new ITexture2D(GBufferSRV[i].mCoreObject.GetTexture2D());
                    var desc = tex.mDesc;
                    var scaleX = (float)desc.Width / ViewPort.mCoreObject.Width;
                    var scaleY = (float)desc.Height / ViewPort.mCoreObject.Height;
                    desc.Width = (uint)(x * scaleX);
                    desc.Height = (uint)(y * scaleY);

                    var showTarget = rc.CreateTexture2D(ref desc);

                    var rtDesc = new IRenderTargetViewDesc();
                    rtDesc.SetDefault();
                    rtDesc.Format = desc.Format;
                    rtDesc.m_pTexture2D = showTarget.mCoreObject.Ptr;
                    rtDesc.Width = desc.Width;
                    rtDesc.Height = desc.Height;
                    var SwapChainRT = rc.CreateRenderTargetView(ref rtDesc);
                    FrameBuffers.mCoreObject.BindRenderTargetView((uint)i, SwapChainRT.mCoreObject.Ptr);

                    var srvDesc = new IShaderResourceViewDesc();
                    srvDesc.mFormat = desc.Format;
                    srvDesc.m_pTexture2D = showTarget.mCoreObject.Ptr;
                    var SwapChainSRV = rc.CreateShaderResourceView(ref srvDesc);
                    GBufferSRV[i].Dispose();
                    GBufferSRV[i] = SwapChainSRV;
                }
            }

            ViewPort.mCoreObject.Width = (float)x;
            ViewPort.mCoreObject.Height = (float)y;

            if (Camera != null)
                Camera.mCoreObject.PerspectiveFovLH(3.14f / 4f, x, y, Camera.mCoreObject.mZNear, Camera.mCoreObject.mZFar);

            UpdateViewportCBuffer();
        }
        
        #region SunLigt
        Vector3 mSunLightDirection;
        public Vector3 SunLightDirection
        {
            get => mSunLightDirection;
            set
            {
                mSunLightDirection = value;

                var mx = Matrix.RotationX(mSunLightDirection.X * MathHelper.Deg2Rad);
                var my = Matrix.RotationY(mSunLightDirection.Y * MathHelper.Deg2Rad);
                var mz = Matrix.RotationZ(mSunLightDirection.Z * MathHelper.Deg2Rad);
                var rotMatrix = mz * my * mx;
                var dir = Vector3.TransformNormal(Vector3.UnitX, rotMatrix);

                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, mSunLightLeak);
                    PerViewportCBuffer.SetValue(indexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
                }
            }
        }
        float mSunLightLeak = 0.05f;
        public float SunLightLeak
        {
            get => mSunLightLeak;
            set
            {
                mSunLightLeak = value;
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    var mx = Matrix.RotationX(mSunLightDirection.X * MathHelper.Deg2Rad);
                    var my = Matrix.RotationY(mSunLightDirection.Y * MathHelper.Deg2Rad);
                    var mz = Matrix.RotationZ(mSunLightDirection.Z * MathHelper.Deg2Rad);
                    var rotMatrix = mz * my * mx;
                    var dir = Vector3.TransformNormal(Vector3.UnitX, rotMatrix);
                    var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, mSunLightLeak);
                    PerViewportCBuffer.SetValue(indexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
                }
            }
        }
        Vector3 mSunLightColor;
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 SunLightColor
        {
            get => mSunLightColor;
            set
            {
                mSunLightColor = value;
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    var gDirLightColor_Intensity = new Vector4(mSunLightColor.X, mSunLightColor.Y, mSunLightColor.Z, mSunLightIntensity);
                    PerViewportCBuffer.SetValue(indexer.gDirLightColor_Intensity, ref gDirLightColor_Intensity);
                }
            }
        }
        float mSunLightIntensity = 2.5f;
        public float SunLightIntensity
        {
            get => mSunLightIntensity;
            set
            {
                mSunLightIntensity = value;
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    var gDirLightDirection_Leak = new Vector4(mSunLightDirection.X, mSunLightDirection.Y, mSunLightDirection.Z, mSunLightLeak);
                    PerViewportCBuffer.SetValue(indexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
                }
            }
        }
        Vector3 mSkyLightColor;
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 SkyLightColor
        {
            get => mSkyLightColor;
            set
            {
                mSkyLightColor = value;
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    PerViewportCBuffer.SetValue(indexer.mSkyLightColor, ref mSkyLightColor);
                }
            }
        }
        Vector3 mGroundLightColor;
        [EGui.Controls.PropertyGrid.Color3PickerEditor]
        public Vector3 GroundLightColor
        {
            get => mGroundLightColor;
            set
            {
                mGroundLightColor = value;
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;
                    PerViewportCBuffer.SetValue(indexer.mGroundLightColor, ref mGroundLightColor);
                }
            }
        }
        #endregion

        public void UpdateViewportCBuffer()
        {
            unsafe
            {
                if (PerViewportCBuffer != null)
                {
                    var indexer = PerViewportCBuffer.PerViewportIndexer;

                    Vector4 gViewportSizeAndRcp = new Vector4(ViewPort.mCoreObject.Width, ViewPort.mCoreObject.Height, 1 / ViewPort.mCoreObject.Width, 1 / ViewPort.mCoreObject.Height);
                    PerViewportCBuffer.SetValue(indexer.gViewportSizeAndRcp, ref gViewportSizeAndRcp);

                    PerViewportCBuffer.SetValue(indexer.mSkyLightColor, ref mSkyLightColor);
                    PerViewportCBuffer.SetValue(indexer.mGroundLightColor, ref mGroundLightColor);

                    var gDirLightColor_Intensity = new Vector4(mSunLightColor.X, mSunLightColor.Y, mSunLightColor.Z, mSunLightIntensity);
                    PerViewportCBuffer.SetValue(indexer.gDirLightColor_Intensity, ref gDirLightColor_Intensity);

                    var mx = Matrix.RotationX(mSunLightDirection.X * MathHelper.Deg2Rad);
                    var my = Matrix.RotationY(mSunLightDirection.Y * MathHelper.Deg2Rad);
                    var mz = Matrix.RotationZ(mSunLightDirection.Z * MathHelper.Deg2Rad);
                    var rotMatrix = mz * my * mx;
                    var dir = Vector3.TransformNormal(Vector3.UnitX, rotMatrix);

                    var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, mSunLightLeak);
                    PerViewportCBuffer.SetValue(indexer.gDirLightDirection_Leak, ref gDirLightDirection_Leak);
                }
            }
        }
    }
}
