using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtCpu2GpuBuffer<T> : IDisposable where T : unmanaged
    {
        public Support.TtNativeArray<T> DataArray;
        private uint GpuCapacity = 0;
        public NxRHI.TtBuffer GpuBuffer;
        public NxRHI.EBufferType BufferTypes { get; private set; } = NxRHI.EBufferType.BFT_SRV;
        public NxRHI.TtUaView Uav;
        public NxRHI.TtSrView Srv;
        public NxRHI.TtCbView Cbv;
        private bool Dirty = false;
        public bool Initialize(NxRHI.EBufferType types)
        {
            BufferTypes = types;
            Dispose();
            DataArray = Support.TtNativeArray<T>.CreateInstance();
            return true;
        }
        public TtCpu2GpuBuffer()
        {

        }
        ~TtCpu2GpuBuffer()
        {
            Dispose();
        }
        public void Dispose()
        {
            GpuCapacity = 0;
            DataArray.Clear();
            DataArray.Dispose();
            if (GpuBuffer != null)
            {
                CoreSDK.DisposeObject(ref Uav);
                CoreSDK.DisposeObject(ref Srv);
                CoreSDK.DisposeObject(ref Cbv);

                CoreSDK.DisposeObject(ref GpuBuffer);
            }
        }
        public void SetSize(int Count)
        {
            Dirty = true;
            DataArray.SetSize(Count);
        }
        public void SetCapacity(int Count)
        {
            DataArray.SetCapacity(Count);
        }
        public int PushData(in T data)
        {
            Dirty = true;
            var result = DataArray.Count;
            DataArray.Add(data);
            return result;
        }
        public void UpdateData(int index, in T data)
        {
            if (index >= DataArray.Count || index < 0)
                return;

            Dirty = true;
            DataArray[index] = data;
        }
        public unsafe void UpdateData(int offset, void* pData, int size)
        {
            Dirty = true;
            var p = (byte*)DataArray.UnsafeAddressAt(0).ToPointer();
            p += offset;
            CoreSDK.MemoryCopy(p, pData, (uint)size);
        }
        public void Clear()
        {
            Dirty = true;
            DataArray.Clear();
        }
        public unsafe void Flush2GPU(NxRHI.UCommandList cmd)
        {
            if (cmd == null)
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Default, "TtCpu2GpuBuffer.Flush"))
                {
                    Flush2GPU(tsCmd.CmdList);
                }
            }
            else
            {
                Flush2GPU(cmd.mCoreObject);
            }
        }
        public unsafe void Flush2GPU(NxRHI.ICommandList cmd)
        {
            if (Dirty == false)
                return;
            Dirty = false;
            bool isRaw = false;
            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(float))
            {
                isRaw = true;
            }
            if (DataArray.Count >= GpuCapacity)
            {
                GpuBuffer?.Dispose();

                GpuCapacity = (uint)DataArray.Count + GpuCapacity / 2 + 1;

                var bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(isRaw, BufferTypes);
                bfDesc.Size = (uint)sizeof(T) * GpuCapacity;
                bfDesc.StructureStride = (uint)sizeof(T);
                bfDesc.InitData = DataArray.UnsafeGetElementAddress(0);
                bfDesc.Type = BufferTypes;
                if ((BufferTypes & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DEFAULT;
                }
                else
                {
                    bfDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                    bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                }
                GpuBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                System.Diagnostics.Debug.Assert(GpuBuffer != null);

                if ((bfDesc.Type & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var uavDesc = new NxRHI.FUavDesc();
                    uavDesc.SetBuffer(isRaw);
                    uavDesc.Buffer.NumElements = (uint)GpuCapacity;
                    uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Uav = TtEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
                }
                if ((bfDesc.Type & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetBuffer(isRaw);
                    srvDesc.Buffer.NumElements = (uint)GpuCapacity;
                    srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Srv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
                }
            }
            else
            {
                if (DataArray.Count > 0)
                    GpuBuffer.UpdateGpuData(cmd, 0, DataArray.UnsafeGetElementAddress(0), (uint)(sizeof(T) * DataArray.Count));
            }
        }
    }
    public class TtGpuBufferBase : IDisposable
    {
        public NxRHI.TtGpuResource GpuResource;
        public NxRHI.TtBuffer GpuBuffer
        {
            get => GpuResource as NxRHI.TtBuffer;
        }
        public NxRHI.TtTexture GpuTexture
        {
            get => GpuResource as NxRHI.TtTexture;
        }
        public uint NumElement { get; protected set; }
        public NxRHI.TtUaView Uav;
        public NxRHI.TtSrView Srv;
        public NxRHI.TtCbView Cbv;
        public NxRHI.TtRenderTargetView Rtv;
        public NxRHI.TtDepthStencilView Dsv;

        ~TtGpuBufferBase()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref Uav);
            CoreSDK.DisposeObject(ref Srv);
            CoreSDK.DisposeObject(ref Cbv);
            CoreSDK.DisposeObject(ref Rtv);
            CoreSDK.DisposeObject(ref Dsv);

            CoreSDK.DisposeObject(ref GpuResource);
        }
        public virtual unsafe void SetSize(uint Count, void* pInitData, NxRHI.EBufferType bufferType)
        {

        }
        public virtual unsafe void SetTexture2D(uint width, uint height, void* pInitData, NxRHI.EBufferType bufferType)
        {

        }
    }

    public class TtGpuBuffer<T> : TtGpuBufferBase where T : unmanaged
    {
        public override unsafe void SetSize(uint Count, void* pInitData, NxRHI.EBufferType bufferType)
        {
            var bfDesc = new NxRHI.FBufferDesc();
            bool isRaw = false;
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                isRaw = true;
            }
            //if (GpuResource == null)
            {
                Dispose();

                NumElement = Count;

                bfDesc.SetDefault(isRaw, bufferType);
                bfDesc.Size = (uint)sizeof(T) * Count;
                bfDesc.StructureStride = (uint)sizeof(T);
                bfDesc.InitData = pInitData;
                bfDesc.Type = bufferType;

                GpuResource = TtEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);

                if ((bufferType & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var uavDesc = new NxRHI.FUavDesc();
                    uavDesc.SetBuffer(isRaw);
                    uavDesc.Buffer.NumElements = (uint)Count;
                    uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Uav = TtEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
                    System.Diagnostics.Debug.Assert(Uav != null);
                }

                if ((bufferType & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetBuffer(isRaw);
                    srvDesc.Buffer.NumElements = (uint)Count;
                    srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Srv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
                    System.Diagnostics.Debug.Assert(Srv != null);
                }
            }
        }
        public override unsafe void SetTexture2D(uint width, uint height, void* pInitData, NxRHI.EBufferType bufferType)
        {
            Dispose();

            NumElement = width * height;

            var desc = new NxRHI.FTextureDesc();
            desc.SetDefault();
            desc.Usage = NxRHI.EGpuUsage.USAGE_DEFAULT;
            desc.BindFlags = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            var initData = new NxRHI.FMappedSubResource();
            initData.SetDefault();
            desc.InitData = &initData;
            
            if (typeof(T) == typeof(float))
            {
                desc.Format = EPixelFormat.PXF_R32_FLOAT;
                initData.m_RowPitch = desc.Width * (uint)sizeof(float);
            }

            initData.pData = pInitData;
            GpuResource = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);

            if ((bufferType & NxRHI.EBufferType.BFT_UAV) != 0)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetTexture2D();
                uavDesc.Format = desc.Format;
                //uavDesc.Texture2D.MipSlice = desc.MipLevels;
                Uav = TtEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuTexture, in uavDesc);
            }

            if ((bufferType & NxRHI.EBufferType.BFT_SRV) != 0)
            {
                var rsvDesc = new NxRHI.FSrvDesc();
                rsvDesc.SetTexture2D();
                rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                rsvDesc.Format = desc.Format;
                rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                Srv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuTexture, in rsvDesc);
            }
        }
    }
}
