using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public struct FAttachBufferDesc : IComparable<FAttachBufferDesc>
    {
        public NxRHI.EBufferType BufferViewTypes;// = NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV
        public EPixelFormat Format;
        public uint Width;
        public uint Height;
        public bool IsMatch(in FAttachBufferDesc desc)
        {
            return (Format == desc.Format) && (Width == desc.Width) && (Height == desc.Height);
        }
        public bool IsMatchSize(in FAttachBufferDesc desc)
        {
            return (Width == desc.Width) && (Height == desc.Height);
        }
        public override string ToString()
        {
            return $"{Format}({Width},{Height})";
        }
        public int CompareTo(FAttachBufferDesc other)
        {
            var cmp = Format.CompareTo(other.Format);
            if (cmp == 0)
            {
                cmp = Width.CompareTo(other.Width);
                if (cmp == 0)
                {
                    return Height.CompareTo(other.Height);
                }
                else
                {
                    return cmp;
                }
            }
            else
            {
                return cmp;
            }
        }
    }
    public class TtAttachmentDesc
    {
        public FHashText AttachmentName;
        public FAttachBufferDesc BufferDesc;
        public void SetDesc(TtAttachmentDesc desc)
        {
            AttachmentName = desc.AttachmentName;
            BufferDesc = desc.BufferDesc;
        }
        public EPixelFormat Format
        {
            get => BufferDesc.Format;
            set => BufferDesc.Format = value;
        }
        public NxRHI.EBufferType BufferViewTypes
        {
            get => BufferDesc.BufferViewTypes;
            set => BufferDesc.BufferViewTypes = value;
        }
        public uint Width
        {
            get => BufferDesc.Width;
            set => BufferDesc.Width = value;
        }
        public uint Height
        {
            get => BufferDesc.Height;
            set => BufferDesc.Height = value;
        }
    }
    public class TtAttachBuffer : IPooledObject, IDisposable
    {
        public bool IsAlloc { get; set; } = false;
        int mRefCount = 0;
        public int RefCount
        {
            get => mRefCount;
        }
        public void AddRef()
        {
            System.Threading.Interlocked.Increment(ref mRefCount);
        }
        public void AddRef(int Num)
        {
            System.Threading.Interlocked.Add(ref mRefCount, Num);
        }
        public int Release()
        {
            return System.Threading.Interlocked.Decrement(ref mRefCount);           
        }
        public void FreeBuffer()
        {
            if (LifeMode == TtAttachBuffer.ELifeMode.Imported)
                return;
            var manager = TtEngine.Instance.GfxDevice.AttachBufferManager;
            mRefCount = 0;
            manager.Free(this);
        }
        public enum ELifeMode
        {
            Imported,
            Transient,
        }
        public ELifeMode LifeMode = ELifeMode.Imported;
        public FAttachBufferDesc BufferDesc;
        public NxRHI.UGpuResource GpuResource;
        public NxRHI.UBuffer Buffer
        {
            get { return GpuResource as NxRHI.UBuffer; }
        }
        public NxRHI.UTexture Texture
        {
            get { return GpuResource as NxRHI.UTexture; }
        }
        public NxRHI.URenderTargetView Rtv;
        public NxRHI.UDepthStencilView Dsv;
        public NxRHI.UUaView Uav;
        public NxRHI.USrView Srv;
        public NxRHI.UCbView Cbv;
        public void SetImportedBuffer(TtGpuBufferBase gpuBuffer)
        {
            GpuResource = gpuBuffer.GpuResource;
            Srv = gpuBuffer.Srv;
            Uav = gpuBuffer.Uav;
            Cbv = gpuBuffer.Cbv;
            Rtv = gpuBuffer.Rtv;
            Dsv = gpuBuffer.Dsv;
        }
        public void Dispose()
        {
            if (LifeMode == ELifeMode.Imported)
            {
                GpuResource = null;
                Rtv = null;
                Dsv = null;
                Uav = null;
                Srv = null;
            }
            else
            {
                CoreSDK.DisposeObject(ref Srv);
                CoreSDK.DisposeObject(ref Rtv);
                CoreSDK.DisposeObject(ref Dsv);
                CoreSDK.DisposeObject(ref Uav);
                CoreSDK.DisposeObject(ref GpuResource);
            }
        }
        public bool CreateBufferViews(in FAttachBufferDesc abfdesc)
        {
            LifeMode = ELifeMode.Transient;
            BufferDesc = abfdesc;
            var types = BufferDesc.BufferViewTypes;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            if (BufferDesc.Format != EPixelFormat.PXF_UNKNOWN || (BufferDesc.BufferViewTypes & (NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_DSV)) != 0)
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                //if (isCpuRead)
                //{
                //    desc.CpuAccess = (NxRHI.ECpuAccess.CAS_WRITE | NxRHI.ECpuAccess.CAS_READ);
                //}
                desc.m_Width = BufferDesc.Width;
                desc.m_Height = BufferDesc.Height;
                desc.m_Format = BufferDesc.Format;

                if ((types & NxRHI.EBufferType.BFT_DSV) != 0)
                {
                    desc.m_BindFlags |= NxRHI.EBufferType.BFT_DSV;
                }
                if ((types & NxRHI.EBufferType.BFT_RTV) != 0)
                {
                    desc.m_BindFlags |= NxRHI.EBufferType.BFT_RTV;
                }
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    desc.m_BindFlags |= NxRHI.EBufferType.BFT_SRV;
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    desc.m_BindFlags |= NxRHI.EBufferType.BFT_UAV;
                }
                if (desc.Format == EPixelFormat.PXF_D24_UNORM_S8_UINT ||
                    desc.Format == EPixelFormat.PXF_D16_UNORM ||
                    desc.Format == EPixelFormat.PXF_D32_FLOAT ||
                    desc.Format == EPixelFormat.PXF_D32_FLOAT_S8X24_UINT)
                {
                    desc.m_BindFlags |= NxRHI.EBufferType.BFT_DSV;
                }
                GpuResource = rc.CreateTexture(in desc);
                System.Diagnostics.Debug.Assert(GpuResource != null);

                if ((types & NxRHI.EBufferType.BFT_RTV) != 0)
                {
                    var viewDesc = new NxRHI.FRtvDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = BufferDesc.Format;
                    viewDesc.Width = BufferDesc.Width;
                    viewDesc.Height = BufferDesc.Height;
                    viewDesc.Texture2D.MipSlice = 0;
                    Rtv = rc.CreateRTV(GpuResource as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_DSV) != 0)
                {
                    var viewDesc = new NxRHI.FDsvDesc();
                    viewDesc.SetDefault();
                    viewDesc.Format = BufferDesc.Format;
                    viewDesc.Width = BufferDesc.Width;
                    viewDesc.Height = BufferDesc.Height;
                    viewDesc.MipLevel = 0;
                    Dsv = rc.CreateDSV(GpuResource as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var viewDesc = new NxRHI.FSrvDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                    viewDesc.Format = BufferDesc.Format;
                    viewDesc.Texture2D.MipLevels = 1;
                    Srv = rc.CreateSRV(GpuResource as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var viewDesc = new NxRHI.FUavDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = BufferDesc.Format;
                    viewDesc.Texture2D.MipSlice = 0;
                    Uav = rc.CreateUAV(GpuResource as NxRHI.UTexture, in viewDesc);
                }
            }
            else
            {
                var desc = new NxRHI.FBufferDesc();
                desc.SetDefault(false, types);
                desc.Size = BufferDesc.Width * BufferDesc.Height;
                desc.StructureStride = BufferDesc.Width;
                //if ((types & NxRHI.EBufferType.BFT_Vertex) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_Vertex;
                //}
                //if ((types & NxRHI.EBufferType.BFT_Index) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_Index;
                //}
                //if ((types & NxRHI.EBufferType.BFT_IndirectArgs) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_IndirectArgs;
                //}
                //if ((types & NxRHI.EBufferType.BFT_CBuffer) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_CBuffer;
                //}
                //if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_SRV;
                //}
                //if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                //{
                //    desc.Type |= NxRHI.EBufferType.BFT_UAV;
                //}
                GpuResource = rc.CreateBuffer(in desc);
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var viewDesc = new NxRHI.FSrvDesc();
                    viewDesc.SetBuffer(false);
                    viewDesc.Type = NxRHI.ESrvType.ST_BufferSRV;
                    viewDesc.Format = BufferDesc.Format;
                    //viewDesc.Buffer.FirstElement = 1;
                    viewDesc.Buffer.NumElements = BufferDesc.Height;
                    viewDesc.Buffer.StructureByteStride = desc.StructureStride;
                    Srv = rc.CreateSRV(GpuResource as NxRHI.UBuffer, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var viewDesc = new NxRHI.FUavDesc();
                    viewDesc.SetBuffer(false);
                    viewDesc.Format = BufferDesc.Format;
                    viewDesc.Buffer.FirstElement = 0;
                    viewDesc.Buffer.NumElements = BufferDesc.Height;
                    viewDesc.Buffer.StructureByteStride = desc.StructureStride;
                    Uav = rc.CreateUAV(GpuResource as NxRHI.UBuffer, in viewDesc);
                }
            }

            return false;
        }
        public TtAttachBuffer Clone()
        {
            var result = new TtAttachBuffer();
            result.BufferDesc = BufferDesc;
            result.CreateBufferViews(in BufferDesc);
            return result;
        }
    }

    public class TtAttachBufferManager : IDisposable
    {
        public class TtAttachBufferPool : TtObjectPool<TtAttachBuffer>, IDisposable
        {
            public void Dispose()
            {
                this.Cleanup();
            }
            public string Name;
            public FAttachBufferDesc BufferDesc;
            public int FrameMaxLiveCount = 0;
            public int FrameLiveCount = 0;
            public int FrameAllocCount = 0;
            protected override TtAttachBuffer CreateObjectSync()
            {
                var result = new TtAttachBuffer();
                result.CreateBufferViews(in BufferDesc);
                return result;
            }
            protected override void OnFinalObject(TtAttachBuffer obj)
            {
                obj.Dispose();
                base.OnFinalObject(obj);
            }
            protected override void OnObjectQuery(TtAttachBuffer obj)
            {
                System.Diagnostics.Debug.Assert(obj.IsAlloc == false);
                FrameAllocCount++;
                FrameLiveCount++;
                if (FrameMaxLiveCount < FrameLiveCount)
                    FrameMaxLiveCount = FrameLiveCount;
            }
            protected override bool OnObjectRelease(TtAttachBuffer obj)
            {
                FrameLiveCount--;
                return true;
            }

        }
        ~TtAttachBufferManager()
        {
            Dispose();
        }
        public void Dispose()
        {
            foreach (var i in Pools)
            {
                i.Value.Dispose();
            }
            Pools.Clear();
        }
        public Dictionary<FAttachBufferDesc, TtAttachBufferPool> Pools { get; } = new Dictionary<FAttachBufferDesc, TtAttachBufferPool>();
        public TtAttachBuffer Alloc(in FAttachBufferDesc desc)
        {
            TtAttachBufferPool pool;
            if (false == Pools.TryGetValue(desc, out pool))
            {
                pool = new TtAttachBufferPool();
                pool.GrowStep = 1;
                pool.BufferDesc = desc;
                Pools.Add(desc, pool);
            }
            return pool.QueryObjectSync();
        }
        public void Free(TtAttachBuffer buffer)
        {
            TtAttachBufferPool pool;
            if (Pools.TryGetValue(buffer.BufferDesc, out pool))
            {
                pool.ReleaseObject(buffer);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
        private List<FAttachBufferDesc> mRmvPools = new List<FAttachBufferDesc>();
        public void Tick()
        {
            TryPrintCachedBuffer();
            mRmvPools.Clear();
            foreach (var i in Pools)
            {
                if (i.Value.FrameMaxLiveCount == 0)
                {
                    i.Value.Dispose();
                    mRmvPools.Add(i.Key);
                    continue;
                }
                else if(i.Value.PoolSize > i.Value.FrameMaxLiveCount + 3)
                {
                    //i.Value.Shrink(i.Value.FrameMaxLiveCount);
                }
                i.Value.FrameMaxLiveCount = 0;
                i.Value.FrameLiveCount = 0;
                i.Value.FrameAllocCount = 0;
            }
            foreach (var i in mRmvPools)
            {
                Pools.Remove(i);
            }
            mRmvPools.Clear();
        }
        public bool PrintCachedBuffer { get; set; } = false;
        public void TryPrintCachedBuffer()
        {
            if (PrintCachedBuffer == false)
                return;
            PrintCachedBuffer = false;
            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, "Begin AttachBuffer====");
            foreach (var i in Pools)
            {
                Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, $"{i.Key.ToString()} X {i.Value.PoolSize} => Max({i.Value.FrameMaxLiveCount}) / Alloc({i.Value.FrameAllocCount})");
            }
            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Info, "End AttachBuffer====");
        }
    }
    
    public class TtGraphicsBuffers
        : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref mFrameBuffers);
            FrameBuffers = null;
        }
        public class TtTargetViewIdentifier
        {
            static int CurrentTargetViewId = 0;
            public TtTargetViewIdentifier()
            {
                TargetViewId = CurrentTargetViewId++;
                if (CurrentTargetViewId == int.MaxValue)
                    CurrentTargetViewId = 0;
            }
            ~TtTargetViewIdentifier()
            {
                TargetViewId = -1;
            }
            public int TargetViewId;
        }
        public TtTargetViewIdentifier TargetViewIdentifier;
        public NxRHI.FViewPort Viewport = new NxRHI.FViewPort();
        NxRHI.UFrameBuffers mFrameBuffers;
        public NxRHI.UFrameBuffers FrameBuffers { get => mFrameBuffers; set => mFrameBuffers = value; }
        public TtRenderGraphPin[] RenderTargets;
        public TtRenderGraphPin DepthStencil;
        NxRHI.UCbView mPerViewportCBuffer;
        public NxRHI.UCbView PerViewportCBuffer
        {
            get
            {
                if (mPerViewportCBuffer == null)
                {
                    var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
                    mPerViewportCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerViewport.Binder.mCoreObject);
                    PerViewportCBuffer.SetDebugName($"Viewport");
                    UpdateViewportCBuffer();
                }
                return mPerViewportCBuffer;
            }
        }
        public void SetViewportCBuffer(GamePlay.TtWorld world, TtRenderPolicy mobilePolicy)
        {
            NxRHI.UCbView cBuffer = PerViewportCBuffer;
            if (cBuffer == null)
                return;
            var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
            var shadowNode = mobilePolicy.FindFirstNode<Shadow.UShadowMapNode>();
            if (shadowNode != null)
            {
                cBuffer.SetValue(coreBinder.CBPerViewport.gFadeParam, in shadowNode.mFadeParam);
                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScale, in shadowNode.mShadowTransitionScale);
                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowMapSizeAndRcp, in shadowNode.mShadowMapSizeAndRcp);
                cBuffer.SetValue(coreBinder.CBPerViewport.gViewer2ShadowMtx, in shadowNode.mViewer2ShadowMtx);

                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowDistance, in shadowNode.mShadowDistance);

                cBuffer.SetValue(coreBinder.CBPerViewport.gCsmDistanceArray, in shadowNode.mSumDistanceFarVec);

                cBuffer.SetMatrix(coreBinder.CBPerViewport.gViewer2ShadowMtxArray, 0, in shadowNode.mViewer2ShadowMtxArray[0]);
                cBuffer.SetMatrix(coreBinder.CBPerViewport.gViewer2ShadowMtxArray, 1, in shadowNode.mViewer2ShadowMtxArray[1]);
                cBuffer.SetMatrix(coreBinder.CBPerViewport.gViewer2ShadowMtxArray, 2, in shadowNode.mViewer2ShadowMtxArray[2]);
                cBuffer.SetMatrix(coreBinder.CBPerViewport.gViewer2ShadowMtxArray, 3, in shadowNode.mViewer2ShadowMtxArray[3]);

                cBuffer.SetValue(coreBinder.CBPerViewport.gShadowTransitionScaleArray, in shadowNode.mShadowTransitionScaleVec);
                cBuffer.SetValue(coreBinder.CBPerViewport.gCsmNum, in shadowNode.mCsmNum);
            }

            var dirLight = world.DirectionLight;
            //dirLight.mDirection = MathHelper.RandomDirection();
            var dir = dirLight.Direction;
            var gDirLightDirection_Leak = new Vector4(dir.X, dir.Y, dir.Z, dirLight.mSunLightLeak);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightDirection_Leak, in gDirLightDirection_Leak);
            var gDirLightColor_Intensity = new Vector4(dirLight.SunLightColor.X, dirLight.SunLightColor.Y, dirLight.SunLightColor.Z, dirLight.mSunLightIntensity);
            cBuffer.SetValue(coreBinder.CBPerViewport.gDirLightColor_Intensity, in gDirLightColor_Intensity);

            cBuffer.SetValue(coreBinder.CBPerViewport.mSkyLightColor, in dirLight.mSkyLightColor);
            cBuffer.SetValue(coreBinder.CBPerViewport.mGroundLightColor, in dirLight.mGroundLightColor);

            float EnvMapMaxMipLevel = 10.0f;
            cBuffer.SetValue(coreBinder.CBPerViewport.gEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
            cBuffer.SetValue(coreBinder.CBPerViewport.gEyeEnvMapMaxMipLevel, in EnvMapMaxMipLevel);
        }
        public void BuildFrameBuffers(TtRenderGraph policy)
        {
            for (int i = 0; i < RenderTargets.Length; i++)
            {
                var attachment = policy.AttachmentCache.GetAttachement(RenderTargets[i].Attachement.AttachmentName, RenderTargets[i].Attachement);
                //RenderTargets[i].AttachBuffer = attachment;
                FrameBuffers.BindRenderTargetView((uint)i, attachment.Rtv);
            }
            if (DepthStencil != null)
            {
                var attachment = policy.AttachmentCache.GetAttachement(DepthStencil.Attachement.AttachmentName, DepthStencil.Attachement);                
                //DepthStencil.AttachBuffer = attachment;
                if (FrameBuffers != null && attachment.Dsv != null)
                {
                    FrameBuffers.BindDepthStencilView(attachment.Dsv);
                }
            }
            FrameBuffers.FlushModify();
            //UpdateFrameBuffers(, y);
        }
        public unsafe void Initialize(TtRenderPolicy policy, NxRHI.URenderPass renderPass)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            FrameBuffers = rc.CreateFrameBuffers(renderPass);

            var rpsDesc = renderPass.mCoreObject.Desc;
            RenderTargets = new TtRenderGraphPin[renderPass.mCoreObject.Desc.m_NumOfMRT];

            Viewport.m_TopLeftX = 0;
            Viewport.m_TopLeftY = 0;
            Viewport.m_MinDepth = 0;
            Viewport.m_MaxDepth = 1.0f;
            //UpdateFrameBuffers();
        }
        public void SetRenderTarget(uint index, NxRHI.URenderTargetView rtv)
        {
            if (RenderTargets[index] == null)
            {
                RenderTargets[index] = new TtRenderGraphPin();
            }
            RenderTargets[index].LifeMode = TtAttachBuffer.ELifeMode.Imported;
            RenderTargets[index].ImportedBuffer = new TtAttachBuffer();
            RenderTargets[index].ImportedBuffer.Rtv = rtv;
            FrameBuffers.BindRenderTargetView(index, rtv);
        }
        public void SetDepthStencil(NxRHI.UDepthStencilView dsv)
        {
            if (DepthStencil == null)
            {
                DepthStencil = new TtRenderGraphPin();
            }
            DepthStencil.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            DepthStencil.ImportedBuffer = new TtAttachBuffer();
            DepthStencil.ImportedBuffer.Dsv = dsv;
            FrameBuffers.BindDepthStencilView(dsv);
        }
        public bool SetRenderTarget(TtRenderGraph policy, int index, TtRenderGraphPin pin)
        {
            if (pin.PinType == TtRenderGraphPin.EPinType.Input ||
                (pin.Attachement.BufferViewTypes & NxRHI.EBufferType.BFT_RTV) != NxRHI.EBufferType.BFT_RTV)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            RenderTargets[index] = pin;
            return true;
        }
        public bool SetDepthStencil(TtRenderGraph policy, TtRenderGraphPin pin)
        {
            if (pin.PinType == TtRenderGraphPin.EPinType.Input ||
                (pin.Attachement.BufferViewTypes & NxRHI.EBufferType.BFT_DSV) != NxRHI.EBufferType.BFT_DSV)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            DepthStencil = pin;            
            return true;
        }
        public unsafe void SetSize(float x, float y)
        {
            if (x < 1.0f)
                x = 1.0f;
            if (y < 1.0f)
                y = 1.0f;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            if (rc == null)
                return;

            Viewport.Width = (float)x;
            Viewport.Height = (float)y;

            //UpdateFrameBuffers(x, y);
            UpdateViewportCBuffer();
        }
        public void UpdateViewportCBuffer()
        {
            unsafe
            {
                if (PerViewportCBuffer != null)
                {
                    var indexer = TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerViewport;

                    Vector4 gViewportSizeAndRcp = new Vector4(Viewport.Width, Viewport.Height, 1 / Viewport.Width, 1 / Viewport.Height);
                    PerViewportCBuffer.SetValue(indexer.gViewportSizeAndRcp, in gViewportSizeAndRcp);
                }
            }
        }
    }
}


namespace EngineNS.NxRHI
{
    public partial class UGraphicDraw
    {
        public void BindGBuffer(Graphics.Pipeline.UCamera camera, Graphics.Pipeline.TtGraphicsBuffers GBuffers)
        {
            //TtEngine.Instance.GfxDevice.CoreShaderBinder.ShaderResource.cbPerViewport
            //TtEngine.Instance.GfxDevice.CoreShaderBinder.ShaderResource.cbPerCamera

            if (GBuffers.PerViewportCBuffer != null && Effect.BindIndexer.cbPerViewport != null)
                mCoreObject.BindResource(Effect.BindIndexer.cbPerViewport.mCoreObject, GBuffers.PerViewportCBuffer.mCoreObject.NativeSuper);
            if (camera.PerCameraCBuffer != null && Effect.BindIndexer.cbPerCamera != null)
                mCoreObject.BindResource(Effect.BindIndexer.cbPerCamera.mCoreObject, camera.PerCameraCBuffer.mCoreObject.NativeSuper);
        }
    }
}