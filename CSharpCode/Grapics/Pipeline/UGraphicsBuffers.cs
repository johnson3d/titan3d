using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UAttachmentDesc
    {
        public FHashText AttachmentName;
        public EGpuBufferViewType BufferViewTypes = EGpuBufferViewType.GBVT_Rtv | EGpuBufferViewType.GBVT_Srv;
        public EPixelFormat Format;
        public uint Width;
        public uint Height;
    }
    public class UAttachment
    {
        public UAttachmentDesc AttachmentDesc;
        //public UAttachBuffer AttachBuffer;        
        public bool OnResize(float x, float y)
        {
            AttachmentDesc.Width = (uint)x;
            AttachmentDesc.Height = (uint)y;
            return true;
        }
    }
    public class UAttachBuffer
    {
        public enum ELifeMode
        {
            Imported,
            Transient,
        }
        public ELifeMode LifeMode = ELifeMode.Imported;
        public RHI.CGpuBuffer Buffer;
        public RHI.CRenderTargetView Rtv;
        public RHI.CDepthStencilView Dsv;
        public RHI.CUnorderedAccessView Uav;
        public RHI.CShaderResourceView Srv;
        public RHI.CConstantBuffer CBuffer;
        public void Cleanup()
        {
            if (LifeMode == ELifeMode.Imported)
            {
                Buffer = null;
                Rtv = null;
                Dsv = null;
                Uav = null;
                Srv = null;
            }
            else
            {
                Srv?.Dispose();
                Srv = null;
                Rtv?.Dispose();
                Rtv = null;
                Dsv?.Dispose();
                Dsv = null;
                Uav?.Dispose();
                Uav = null;
                Buffer?.Dispose();
                Buffer = null;
            }
        }
        public bool CreateBufferViews(EGpuBufferViewType types, UAttachmentDesc AttachmentDesc, bool isCpuRead = false)
        {
            LifeMode = ELifeMode.Transient;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (AttachmentDesc.Format != EPixelFormat.PXF_UNKNOWN || (types & (EGpuBufferViewType.GBVT_Rtv | EGpuBufferViewType.GBVT_Dsv)) != 0)
            {
                ITexture2DDesc desc = new ITexture2DDesc();
                desc.SetDefault();
                if (isCpuRead)
                {
                    desc.CPUAccess = (UInt32)(ECpuAccess.CAS_WRITE | ECpuAccess.CAS_READ);
                }
                desc.m_Width = AttachmentDesc.Width;
                desc.m_Height = AttachmentDesc.Height;
                desc.m_Format = AttachmentDesc.Format;

                if ((types & EGpuBufferViewType.GBVT_Dsv) != 0)
                {
                    switch (desc.m_Format)
                    {
                        case EPixelFormat.PXF_D24_UNORM_S8_UINT:
                            desc.m_Format = EPixelFormat.PXF_R24G8_TYPELESS;
                            break;
                        case EPixelFormat.PXF_D32_FLOAT:
                            desc.m_Format = EPixelFormat.PXF_R32_TYPELESS;
                            break;
                        case EPixelFormat.PXF_D16_UNORM:
                            desc.m_Format = EPixelFormat.PXF_R16_TYPELESS;
                            break;
                        case EPixelFormat.PXF_UNKNOWN:
                            desc.m_Format = EPixelFormat.PXF_R16_TYPELESS;
                            break;
                        default:
                            break;
                    }
                    desc.m_BindFlags = (uint)EBindFlags.BF_DEPTH_STENCIL;
                }
                if ((types & EGpuBufferViewType.GBVT_Rtv) != 0)
                {
                    desc.m_BindFlags = (uint)EBindFlags.BF_RENDER_TARGET;
                }
                if ((types & EGpuBufferViewType.GBVT_Srv) != 0)
                {
                    desc.m_BindFlags |= (uint)EBindFlags.BF_SHADER_RES;
                }
                if ((types & EGpuBufferViewType.GBVT_Uav) != 0)
                {
                    desc.m_BindFlags |= (uint)EBindFlags.BF_UNORDERED_ACCESS;
                }
                Buffer = rc.CreateTexture2D(in desc);

                if ((types & EGpuBufferViewType.GBVT_Rtv) != 0)
                {
                    var viewDesc = new IRenderTargetViewDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.mGpuBuffer = Buffer.mCoreObject;
                    viewDesc.Width = AttachmentDesc.Width;
                    viewDesc.Height = AttachmentDesc.Height;
                    viewDesc.Texture2D.MipSlice = 0;
                    Rtv = rc.CreateRenderTargetView(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_Dsv) != 0)
                {
                    var viewDesc = new IDepthStencilViewDesc();
                    viewDesc.SetDefault();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Texture2D = Buffer.mCoreObject;
                    viewDesc.Width = AttachmentDesc.Width;
                    viewDesc.Height = AttachmentDesc.Height;
                    viewDesc.MipLevel = 0;
                    Dsv = rc.CreateDepthRenderTargetView(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_Srv) != 0)
                {
                    var viewDesc = new IShaderResourceViewDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Type = ESrvType.ST_Texture2D;
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.mGpuBuffer = Buffer.mCoreObject;
                    viewDesc.Texture2D.MipLevels = 1;
                    Srv = rc.CreateShaderResourceView(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_Uav) != 0)
                {
                    var viewDesc = new IUnorderedAccessViewDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Texture2D.MipSlice = 0;
                    Uav = rc.CreateUnorderedAccessView(Buffer, in viewDesc);
                }
            }
            else
            {
                IGpuBufferDesc desc = new IGpuBufferDesc();
                desc.SetDefault();
                if (isCpuRead)
                {
                    desc.SetStaging();
                }
                else
                {
                    desc.SetMode(false, true);
                }
                desc.m_ByteWidth = AttachmentDesc.Width * AttachmentDesc.Height;
                desc.m_StructureByteStride = AttachmentDesc.Width;
                Buffer = rc.CreateGpuBuffer(in desc, IntPtr.Zero);

                if ((types & EGpuBufferViewType.GBVT_VertexBuffer) != 0)
                {
                    var viewDesc = new IVertexBufferDesc();
                    viewDesc.SetDefault();
                    viewDesc.ByteWidth = AttachmentDesc.Width * AttachmentDesc.Height;
                    viewDesc.Stride = AttachmentDesc.Width;
                    rc.CreateVertexBuffer(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_IndexBuffer) != 0)
                {
                    var viewDesc = new IIndexBufferDesc();
                    viewDesc.SetDefault();
                    viewDesc.ByteWidth = AttachmentDesc.Width * AttachmentDesc.Height;
                    viewDesc.Type = EIndexBufferType.IBT_Int32;//???
                    rc.CreateIndexBuffer(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_IndirectBuffer) != 0)
                {

                }
                if ((types & EGpuBufferViewType.GBVT_CBuffer) != 0)
                {
                    //rc.CreateConstantBuffer()
                }
                if ((types & EGpuBufferViewType.GBVT_Srv) != 0)
                {
                    var viewDesc = new IShaderResourceViewDesc();
                    viewDesc.SetBuffer();
                    viewDesc.Type = ESrvType.ST_BufferSRV;
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.mGpuBuffer = Buffer.mCoreObject;
                    //viewDesc.Buffer.FirstElement = 1;
                    viewDesc.Buffer.NumElements = AttachmentDesc.Height;
                    Srv = rc.CreateShaderResourceView(in viewDesc);
                }
                if ((types & EGpuBufferViewType.GBVT_Uav) != 0)
                {
                    var viewDesc = new IUnorderedAccessViewDesc();
                    viewDesc.SetBuffer();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Buffer.FirstElement = 0;
                    viewDesc.Buffer.NumElements = AttachmentDesc.Height;
                    Uav = rc.CreateUnorderedAccessView(Buffer, in viewDesc);
                }
            }

            return false;
        }
        public UAttachBuffer Clone(bool CpuAccess, UAttachmentDesc desc)
        {
            var result = new UAttachBuffer();
            result.CreateBufferViews(desc.BufferViewTypes, desc, true);
            return result;
        }
    }
    
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
        public RHI.CViewPort ViewPort = new RHI.CViewPort();
        public RHI.CFrameBuffers FrameBuffers;
        public RHI.CSwapChain SwapChain;
        public Common.URenderGraphPin[] RenderTargets;
        public Common.URenderGraphPin DepthStencil;
        RHI.CConstantBuffer mPerViewportCBuffer;
        public RHI.CConstantBuffer PerViewportCBuffer
        {
            get
            {
                if (mPerViewportCBuffer == null)
                {
                    var effect = UEngine.Instance.GfxDevice.EffectManager.DummyEffect;
                    mPerViewportCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(effect.ShaderProgram, effect.ShaderIndexer.cbPerViewport);
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
        }        
        public void BuildFrameBuffers(Common.URenderGraph policy)
        {
            for (int i = 0; i < RenderTargets.Length; i++)
            {
                var attachment = policy.AttachmentCache.GetAttachement(RenderTargets[i].Attachement.AttachmentName, RenderTargets[i].Attachement);
                //RenderTargets[i].AttachBuffer = attachment;
                FrameBuffers.mCoreObject.BindRenderTargetView((uint)i, attachment.Rtv.mCoreObject);
            }
            if (DepthStencil != null)
            {
                var attachment = policy.AttachmentCache.GetAttachement(DepthStencil.Attachement.AttachmentName, DepthStencil.Attachement);                
                //DepthStencil.AttachBuffer = attachment;
                if (FrameBuffers != null && attachment.Dsv != null)
                {
                    FrameBuffers.mCoreObject.BindDepthStencilView(attachment.Dsv.mCoreObject);
                }
            }
            //UpdateFrameBuffers(, y);
        }
        public unsafe void Initialize(URenderPolicy policy, RHI.CRenderPass renderPass)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            
            var fbDesc = new IFrameBuffersDesc();
            fbDesc.RenderPass = renderPass.mCoreObject;
            FrameBuffers = rc.CreateFrameBuffers(in fbDesc);

            var rpsDesc = renderPass.mCoreObject.mDesc;
            RenderTargets = new Common.URenderGraphPin[renderPass.mCoreObject.mDesc.m_NumOfMRT];
            
            //UpdateFrameBuffers();
            UpdateViewportCBuffer();
        }
        public bool UpdateFrameBuffers(float width, float height)
        {
            return FrameBuffers.mCoreObject.UpdateFrameBuffers(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, width, height);
        }        
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
        public bool SetRenderTarget(Common.URenderGraph policy, int index, Common.URenderGraphPin pin)
        {
            if (pin.PinType == Common.URenderGraphPin.EPinType.Input ||
                (pin.Attachement.BufferViewTypes & EGpuBufferViewType.GBVT_Rtv) != EGpuBufferViewType.GBVT_Rtv)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            RenderTargets[index] = pin;
            return true;
        }
        public bool SetDepthStencil(Common.URenderGraph policy, Common.URenderGraphPin pin)
        {
            if (pin.PinType == Common.URenderGraphPin.EPinType.Input ||
                (pin.Attachement.BufferViewTypes & EGpuBufferViewType.GBVT_Dsv) != EGpuBufferViewType.GBVT_Dsv)
            {
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
            DepthStencil = pin;            
            return true;
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

            ViewPort.mCoreObject.Width = (float)x;
            ViewPort.mCoreObject.Height = (float)y;

            //UpdateFrameBuffers(x, y);
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

    public class UComputeBuffers
    {

    }
}


namespace EngineNS.RHI
{
    public partial class CDrawCall
    {
        public void BindGBuffer(Graphics.Pipeline.CCamera camera, Graphics.Pipeline.UGraphicsBuffers GBuffers)
        {
            unsafe
            {
                if (GBuffers.PerViewportCBuffer != null)
                    mCoreObject.BindShaderCBuffer(Effect.ShaderIndexer.cbPerViewport, GBuffers.PerViewportCBuffer.mCoreObject);
                if (camera.PerCameraCBuffer != null)
                    mCoreObject.BindShaderCBuffer(Effect.ShaderIndexer.cbPerCamera, camera.PerCameraCBuffer.mCoreObject);
            }
        }
    }
}