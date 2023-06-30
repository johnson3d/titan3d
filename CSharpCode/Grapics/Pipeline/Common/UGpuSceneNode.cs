using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtCpu2GpuBuffer<T> : IDisposable where T : unmanaged
    {
        public Support.UNativeArray<T> DataArray;
        private uint GpuCapacity = 0;
        public NxRHI.UBuffer GpuBuffer;
        public bool IsGpuWrite { get; private set; } = false;
        public NxRHI.UUaView DataUAV;
        public NxRHI.USrView DataSRV;
        private bool Dirty = false;
        public bool Initialize(bool gpuWrite)
        {
            IsGpuWrite = gpuWrite;
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
            DataArray.Clear();
            DataArray.Dispose();
            if (GpuBuffer != null)
            {
                DataUAV?.Dispose();
                DataUAV = null;

                DataSRV?.Dispose();
                DataSRV = null;

                GpuBuffer?.Dispose();
                GpuBuffer = null;
            }
        }
        public void SetSize(int Count)
        {
            Dirty = true;
            DataArray.SetSize(Count);
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
        public unsafe void Flush2GPU()
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
                bfDesc.SetDefault(isRaw);
                bfDesc.Size = (uint)sizeof(T) * GpuCapacity;
                bfDesc.StructureStride = (uint)sizeof(T);
                bfDesc.InitData = DataArray.UnsafeGetElementAddress(0);
                bfDesc.Type = NxRHI.EBufferType.BFT_SRV;
                if (IsGpuWrite)
                {
                    bfDesc.Type |= NxRHI.EBufferType.BFT_UAV;
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
                    DataUAV = UEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
                }
                if ((bfDesc.Type & NxRHI.EBufferType.BFT_SRV) != 0)
                {
                    var srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetBuffer(isRaw);
                    srvDesc.Buffer.NumElements = (uint)GpuCapacity;
                    srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                    DataSRV = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
                }
            }
            else
            {
                if (DataArray.Count > 0)
                    GpuBuffer.UpdateGpuData(0, DataArray.UnsafeGetElementAddress(0), (uint)(sizeof(T) * DataArray.Count));
            }
        }
    }
    public class TtGpuBuffer<T> : IDisposable where T : unmanaged
    {
        public NxRHI.UBuffer GpuBuffer;
        public NxRHI.UUaView DataUAV;
        public NxRHI.USrView DataSRV;

        ~TtGpuBuffer()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref DataUAV);
            CoreSDK.DisposeObject(ref DataSRV);
            CoreSDK.DisposeObject(ref GpuBuffer);
        }
        public unsafe void SetSize(uint Count, void* pInitData, NxRHI.EBufferType bufferType = NxRHI.EBufferType.BFT_UAV)
        {
            Dispose();

            bool isRaw = false;
            if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                isRaw = true;
            }

            var bfDesc = new NxRHI.FBufferDesc();
            bfDesc.SetDefault(isRaw);
            bfDesc.Size = (uint)sizeof(T) * Count;
            bfDesc.StructureStride = (uint)sizeof(T);
            bfDesc.InitData = pInitData;
            bfDesc.Type = bufferType;
            
            GpuBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);

            if ((bufferType & NxRHI.EBufferType.BFT_UAV) != 0)
            {
                var uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetBuffer(isRaw);
                uavDesc.Buffer.NumElements = (uint)Count;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                DataUAV = UEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
            }

            if ((bufferType & NxRHI.EBufferType.BFT_SRV) != 0)
            {
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(isRaw);
                srvDesc.Buffer.NumElements = (uint)Count;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                DataSRV = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
            }   
        }
    }
    public partial class UGpuSceneNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin GpuScenePinOut = Common.URenderGraphPin.CreateOutput("GpuScene", false, EPixelFormat.PXF_UNKNOWN);
        public UGpuSceneNode()
        {
            Name = "GpuSceneNode";
        }
        public override void Dispose()
        {
            Dispose_Light();
            Dispose_Instance();

            GpuSceneDescSRV?.Dispose();
            GpuSceneDescSRV = null;
            GpuSceneDescUAV?.Dispose();
            GpuSceneDescUAV = null;
            GpuSceneDescBuffer?.Dispose();
            GpuSceneDescBuffer = null;

            base.Dispose();
        }
        public override void InitNodePins()
        {
            GpuScenePinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(GpuScenePinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            PointLightsPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(PointLightsPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            InstancePinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(InstancePinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public unsafe override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            GpuScenePinOut.Attachement.Height = 1;
            GpuScenePinOut.Attachement.Width = (uint)sizeof(FGpuSceneDesc);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(GpuScenePinOut);
            attachement.Buffer = GpuSceneDescBuffer;
            attachement.Srv = GpuSceneDescSRV;
            attachement.Uav = GpuSceneDescUAV;
            attachement.CBuffer = PerGpuSceneCBuffer;

            FrameBuild_Light();
            FrameBuild_Instance();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FGpuSceneDesc
        {
            public uint ScreenAverageColorI;
            public uint AverageColorDivider;            
            public float ScreenAverageBrightness;
            public float PrevScreenAverageBrightness;

            public float EyeAdapterTime;
            public float EyeAdapter;
            public int FreeGroupNum;
        }
        public NxRHI.UBuffer GpuSceneDescBuffer;
        public NxRHI.UUaView GpuSceneDescUAV;
        public NxRHI.USrView GpuSceneDescSRV;

        public NxRHI.UCbView PerGpuSceneCBuffer { get; set; }

        #region SceneConfig
        float mExposure = 1.0f;
        public float Exposure
        {
            get => mExposure;
            set
            {
                if (PerGpuSceneCBuffer != null)
                {
                    mExposure = value;
                    
                    PerGpuSceneCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.Exposure, in mExposure);
                }
            }
        }
        float mEyeAdapterTimeRange = 4.0f;
        public float EyeAdapterTimeRange
        {
            get => mEyeAdapterTimeRange;
            set
            {
                if (PerGpuSceneCBuffer != null)
                {
                    mEyeAdapterTimeRange = value;
                    PerGpuSceneCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.EyeAdapterTimeRange, in mEyeAdapterTimeRange);
                }
            }
        }
        float mHdrMiddleGrey = 0.6f;
        public float HdrMiddleGrey
        {
            get => mHdrMiddleGrey;
            set
            {
                mHdrMiddleGrey = value;
                if (PerGpuSceneCBuffer != null)
                {
                    PerGpuSceneCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMiddleGrey, in mHdrMiddleGrey);
                }
            }
        }
        float mHdrMaxLuminance = 16.0f;
        public float HdrMaxLuminance
        {
            get => mHdrMaxLuminance;
            set
            {
                if (PerGpuSceneCBuffer != null)
                {
                    mHdrMaxLuminance = value;
                    PerGpuSceneCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMaxLuminance, in mHdrMaxLuminance);
                }
            }
        }
        float mHdrMinLuminance = 0.01f;
        public float HdrMinLuminance
        {
            get => mHdrMinLuminance;
            set
            {
                if (PerGpuSceneCBuffer != null)
                {
                    mHdrMinLuminance = value;
                    PerGpuSceneCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMinLuminance, in mHdrMinLuminance);
                }
            }
        }
        #endregion

        public struct TtClusteDrawArgs
        {
            public Int32 GpuSceneIndex;
            public UInt32 MaxInstance;
            public TtGpuBuffer<uint> IndirectArgsBuffer;
            public TtGpuBuffer<uint> IndirectCountBuffer;
        }


        public class TtClusterBuffer
        {
            public Int32 ClusterCount = 0;
        }
        

        
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");

            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault(false);
            desc.Type = NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV;
            unsafe
            {
                desc.Size = (uint)sizeof(FGpuSceneDesc);
                desc.StructureStride = (uint)sizeof(FGpuSceneDesc);
            }
            GpuSceneDescBuffer = rc.CreateBuffer(in desc);
            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(false);
            uavDesc.Buffer.NumElements = (uint)1;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            GpuSceneDescUAV = rc.CreateUAV(GpuSceneDescBuffer, in uavDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetBuffer(false);
            srvDesc.Buffer.NumElements = (uint)1;
            unsafe
            {
                srvDesc.Buffer.StructureByteStride = (uint)sizeof(FGpuSceneDesc);
            }
            GpuSceneDescSRV = rc.CreateSRV(GpuSceneDescBuffer, in srvDesc);

            PerGpuSceneCBuffer = rc.CreateCBV(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.Binder.mCoreObject);

            Initialize_Light(policy, debugName);
            Initialize_Instance(policy, debugName);

            HdrMiddleGrey = 0.6f;
            HdrMinLuminance = 0.01f;
            HdrMaxLuminance = 16.0f;

            Exposure = 1.0f;
            EyeAdapterTimeRange = 5.0f;
        }
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {

        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UGpuSceneNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (policy.VisibleNodes == null)
                    return;

                var cmd = BasePass.DrawCmdList;
                cmd.BeginCommand();

                TickLogic_Light(world, policy, cmd);
                TickLogic_Instance(world, policy, cmd);

                //if PerFrameCBuffer dirty :flush
                //UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject.FlushDirty(false);
                cmd.EndCommand();

                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
            }   
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
