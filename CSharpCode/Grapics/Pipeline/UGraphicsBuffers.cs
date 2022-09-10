using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class UAttachmentDesc
    {
        public FHashText AttachmentName;
        public NxRHI.EBufferType BufferViewTypes = NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV;
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
        public NxRHI.UGpuResource Buffer;
        public NxRHI.URenderTargetView Rtv;
        public NxRHI.UDepthStencilView Dsv;
        public NxRHI.UUaView Uav;
        public NxRHI.USrView Srv;
        public NxRHI.UCbView CBuffer;
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
        public bool CreateBufferViews(NxRHI.EBufferType types, UAttachmentDesc AttachmentDesc, bool isCpuRead = false)
        {
            LifeMode = ELifeMode.Transient;
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            if (AttachmentDesc.Format != EPixelFormat.PXF_UNKNOWN || (types & (NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_DSV)) != 0)
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                if (isCpuRead)
                {
                    desc.CpuAccess = (NxRHI.ECpuAccess.CAS_WRITE | NxRHI.ECpuAccess.CAS_READ);
                }
                desc.m_Width = AttachmentDesc.Width;
                desc.m_Height = AttachmentDesc.Height;
                desc.m_Format = AttachmentDesc.Format;

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
                Buffer = rc.CreateTexture(in desc);
                System.Diagnostics.Debug.Assert(Buffer != null);

                if ((types & NxRHI.EBufferType.BFT_RTV) != 0)
                {
                    var viewDesc = new NxRHI.FRtvDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Width = AttachmentDesc.Width;
                    viewDesc.Height = AttachmentDesc.Height;
                    viewDesc.Texture2D.MipSlice = 0;
                    Rtv = rc.CreateRTV(Buffer as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_DSV) != 0)
                {
                    var viewDesc = new NxRHI.FDsvDesc();
                    viewDesc.SetDefault();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Width = AttachmentDesc.Width;
                    viewDesc.Height = AttachmentDesc.Height;
                    viewDesc.MipLevel = 0;
                    Dsv = rc.CreateDSV(Buffer as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var viewDesc = new NxRHI.FSrvDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Texture2D.MipLevels = 1;
                    Srv = rc.CreateSRV(Buffer as NxRHI.UTexture, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var viewDesc = new NxRHI.FUavDesc();
                    viewDesc.SetTexture2D();
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Texture2D.MipSlice = 0;
                    Uav = rc.CreateUAV(Buffer as NxRHI.UTexture, in viewDesc);
                }
            }
            else
            {
                var desc = new NxRHI.FBufferDesc();
                desc.SetDefault();
                desc.Size = AttachmentDesc.Width * AttachmentDesc.Height;
                desc.StructureStride = AttachmentDesc.Width;
                if ((types & NxRHI.EBufferType.BFT_Vertex) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_Vertex;
                }
                if ((types & NxRHI.EBufferType.BFT_Index) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_Index;
                }
                if ((types & NxRHI.EBufferType.BFT_IndirectArgs) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_IndirectArgs;
                }
                if ((types & NxRHI.EBufferType.BFT_CBuffer) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_CBuffer;
                }
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_SRV;
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    desc.Type |= NxRHI.EBufferType.BFT_UAV;
                }
                Buffer = rc.CreateBuffer(in desc);
                if ((types & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var viewDesc = new NxRHI.FSrvDesc();
                    viewDesc.SetBuffer(0);
                    viewDesc.Type = NxRHI.ESrvType.ST_BufferSRV;
                    viewDesc.Format = AttachmentDesc.Format;
                    //viewDesc.Buffer.FirstElement = 1;
                    viewDesc.Buffer.NumElements = AttachmentDesc.Height;
                    viewDesc.Buffer.StructureByteStride = desc.StructureStride;
                    Srv = rc.CreateSRV(Buffer as NxRHI.UBuffer, in viewDesc);
                }
                if ((types & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var viewDesc = new NxRHI.FUavDesc();
                    viewDesc.SetBuffer(0);
                    viewDesc.Format = AttachmentDesc.Format;
                    viewDesc.Buffer.FirstElement = 0;
                    viewDesc.Buffer.NumElements = AttachmentDesc.Height;
                    viewDesc.Buffer.StructureByteStride = desc.StructureStride;
                    Uav = rc.CreateUAV(Buffer as NxRHI.UBuffer, in viewDesc);
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
        public NxRHI.FViewPort Viewport = new NxRHI.FViewPort();
        public NxRHI.UFrameBuffers FrameBuffers { get; set; }
        public Common.URenderGraphPin[] RenderTargets;
        public Common.URenderGraphPin DepthStencil;
        NxRHI.UCbView mPerViewportCBuffer;
        public NxRHI.UCbView PerViewportCBuffer
        {
            get
            {
                if (mPerViewportCBuffer == null)
                {
                    var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                    mPerViewportCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerViewport.Binder.mCoreObject);
                    PerViewportCBuffer.SetDebugName($"Viewport");
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
        public unsafe void Initialize(URenderPolicy policy, NxRHI.URenderPass renderPass)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            
            FrameBuffers = rc.CreateFrameBuffers(renderPass);

            var rpsDesc = renderPass.mCoreObject.Desc;
            RenderTargets = new Common.URenderGraphPin[renderPass.mCoreObject.Desc.m_NumOfMRT];

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
                RenderTargets[index] = new Common.URenderGraphPin();
            }
            RenderTargets[index].LifeMode = UAttachBuffer.ELifeMode.Imported;
            RenderTargets[index].ImportedBuffer = new UAttachBuffer();
            RenderTargets[index].ImportedBuffer.Rtv = rtv;
            FrameBuffers.BindRenderTargetView(index, rtv);
        }
        public void SetDepthStencil(NxRHI.UDepthStencilView dsv)
        {
            if (DepthStencil == null)
            {
                DepthStencil = new Common.URenderGraphPin();
            }
            DepthStencil.LifeMode = UAttachBuffer.ELifeMode.Imported;
            DepthStencil.ImportedBuffer = new UAttachBuffer();
            DepthStencil.ImportedBuffer.Dsv = dsv;
            FrameBuffers.BindDepthStencilView(dsv);
        }
        public bool SetRenderTarget(Common.URenderGraph policy, int index, Common.URenderGraphPin pin)
        {
            if (pin.PinType == Common.URenderGraphPin.EPinType.Input ||
                (pin.Attachement.BufferViewTypes & NxRHI.EBufferType.BFT_RTV) != NxRHI.EBufferType.BFT_RTV)
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
                (pin.Attachement.BufferViewTypes & NxRHI.EBufferType.BFT_DSV) != NxRHI.EBufferType.BFT_DSV)
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
                    var indexer = UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerViewport;

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
        public void BindGBuffer(Graphics.Pipeline.CCamera camera, Graphics.Pipeline.UGraphicsBuffers GBuffers)
        {
            //UEngine.Instance.GfxDevice.CoreShaderBinder.ShaderResource.cbPerViewport
            //UEngine.Instance.GfxDevice.CoreShaderBinder.ShaderResource.cbPerCamera

            if (GBuffers.PerViewportCBuffer != null && Effect.BindIndexer.cbPerViewport != null)
                mCoreObject.BindResource(Effect.BindIndexer.cbPerViewport.mCoreObject, GBuffers.PerViewportCBuffer.mCoreObject.NativeSuper);
            if (camera.PerCameraCBuffer != null && Effect.BindIndexer.cbPerCamera != null)
                mCoreObject.BindResource(Effect.BindIndexer.cbPerCamera.mCoreObject, camera.PerCameraCBuffer.mCoreObject.NativeSuper);
        }
    }
}