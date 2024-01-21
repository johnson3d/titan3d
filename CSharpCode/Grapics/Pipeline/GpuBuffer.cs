using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    public class TtCpu2GpuBuffer<T> : IDisposable where T : unmanaged
    {
        public Support.UNativeArray<T> DataArray;
        private uint GpuCapacity = 0;
        public NxRHI.UBuffer GpuBuffer;
        public NxRHI.EBufferType BufferTypes { get; private set; } = NxRHI.EBufferType.BFT_SRV;
        public NxRHI.UUaView Uav;
        public NxRHI.USrView Srv;
        public NxRHI.UCbView Cbv;
        private bool Dirty = false;
        public bool Initialize(NxRHI.EBufferType types)
        {
            BufferTypes = types;
            Dispose();
            DataArray = Support.UNativeArray<T>.CreateInstance();
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
                GpuBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                System.Diagnostics.Debug.Assert(GpuBuffer != null);

                if ((bfDesc.Type & NxRHI.EBufferType.BFT_UAV) != 0)
                {
                    var uavDesc = new NxRHI.FUavDesc();
                    uavDesc.SetBuffer(isRaw);
                    uavDesc.Buffer.NumElements = (uint)GpuCapacity;
                    uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Uav = UEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
                }
                if ((bfDesc.Type & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetBuffer(isRaw);
                    srvDesc.Buffer.NumElements = (uint)GpuCapacity;
                    srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    Srv = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
                }
            }
            else
            {
                if (DataArray.Count > 0)
                    GpuBuffer.UpdateGpuData(cmd, 0, DataArray.UnsafeGetElementAddress(0), (uint)(sizeof(T) * DataArray.Count));
            }
        }
    }
    public class TtGpuBuffer<T> : IDisposable where T : unmanaged
    {
        public NxRHI.UGpuResource GpuResource;
        public NxRHI.UBuffer GpuBuffer
        {
            get => GpuResource as NxRHI.UBuffer;
        }
        public NxRHI.UTexture GpuTexture
        {
            get => GpuResource as NxRHI.UTexture;
        }
        public uint NumElement { get; private set; }
        public NxRHI.UUaView Uav;
        public NxRHI.USrView Srv;
        public NxRHI.UCbView Cbv;
        public NxRHI.URenderTargetView Rtv;
        public NxRHI.UDepthStencilView Dsv;
        
        ~TtGpuBuffer()
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
        public unsafe void SetSize(uint Count, void* pInitData, NxRHI.EBufferType bufferType)
        {
            Dispose();

            bool isRaw = false;
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                isRaw = true;
            }

            NumElement = Count;

            var bfDesc = new NxRHI.FBufferDesc();
            bfDesc.SetDefault(isRaw, bufferType);
            bfDesc.Size = (uint)sizeof(T) * Count;
            bfDesc.StructureStride = (uint)sizeof(T);
            bfDesc.InitData = pInitData;
            bfDesc.Type = bufferType;

            GpuResource = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);

            if ((bufferType & NxRHI.EBufferType.BFT_UAV) != 0)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetBuffer(isRaw);
                uavDesc.Buffer.NumElements = (uint)Count;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                Uav = UEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
            }

            if ((bufferType & NxRHI.EBufferType.BFT_SRV) != 0)
            {
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(isRaw);
                srvDesc.Buffer.NumElements = (uint)Count;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                Srv = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
            }
        }
    }
}
