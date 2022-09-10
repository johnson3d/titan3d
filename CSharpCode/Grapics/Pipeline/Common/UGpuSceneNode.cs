using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UGpuSceneNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin GpuScenePinOut = Common.URenderGraphPin.CreateOutput("GpuScene", false, EPixelFormat.PXF_UNKNOWN);
        public Common.URenderGraphPin PointLightsPinOut = Common.URenderGraphPin.CreateOutput("PointLights", false, EPixelFormat.PXF_UNKNOWN);
        public UGpuSceneNode()
        {
            Name = "GpuSceneNode";
        }
        ~UGpuSceneNode()
        {
            Cleanup();
        }
        public override void Cleanup()
        {
            PointLights.Cleanup();

            GpuSceneDescSRV?.Dispose();
            GpuSceneDescSRV = null;
            GpuSceneDescUAV?.Dispose();
            GpuSceneDescUAV = null;
            GpuSceneDescBuffer?.Dispose();
            GpuSceneDescBuffer = null;

            base.Cleanup();
        }
        public override void InitNodePins()
        {
            GpuScenePinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(GpuScenePinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            PointLightsPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(PointLightsPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public unsafe override void FrameBuild()
        {
            GpuScenePinOut.Attachement.Height = 1;
            GpuScenePinOut.Attachement.Width = (uint)sizeof(FGpuSceneDesc);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(GpuScenePinOut);
            attachement.Buffer = GpuSceneDescBuffer;
            attachement.Srv = GpuSceneDescSRV;
            attachement.Uav = GpuSceneDescUAV;
            attachement.CBuffer = PerGpuSceneCBuffer;

            PointLightsPinOut.Attachement.Height = (uint)PointLights.DataArray.Count;
            PointLightsPinOut.Attachement.Width = (uint)sizeof(FPointLight);
            attachement = RenderGraph.AttachmentCache.ImportAttachment(PointLightsPinOut);
            //if (attachement.Buffer == null)
            //{
            //    PointLights.Flush2GPU(this.BasePass.DrawCmdList.mCoreObject);
            //}
            attachement.Buffer = PointLights.GpuBuffer;
            attachement.Srv = PointLights.DataSRV;
            attachement.Uav = PointLights.DataUAV;
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

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        public class UGpuDataArray<T> where T : unmanaged
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
                Cleanup();
                DataArray = Support.UNativeArray<T>.CreateInstance();
                return true;
            }
            public UGpuDataArray()
            {
                
            }
            ~UGpuDataArray()
            {
                Cleanup();
            }
            public void Cleanup()
            {
                if (GpuBuffer != null)
                {
                    DataArray.Clear();
                    DataArray.Dispose();

                    DataUAV?.Dispose();
                    DataUAV = null;

                    DataSRV?.Dispose();
                    DataSRV = null;

                    GpuBuffer?.Dispose();
                    GpuBuffer = null;
                }
            }
            public UInt16 PushData(in T data)
            {
                Dirty = true;
                UInt16 result = (UInt16)DataArray.Count;
                DataArray.Add(data);
                return result;
            }
            public void UpdateData(UInt16 index, in T data)
            {
                if (index >= DataArray.Count)
                    return;

                Dirty = true;
                DataArray[index] = data;
            }
            public void Clear()
            {
                Dirty = true;
                DataArray.Clear();
            }
            public unsafe void Flush2GPU(NxRHI.ICommandList cmd)
            {
                if (Dirty == false)
                    return;
                Dirty = false;
                if (DataArray.Count >= GpuCapacity)
                {
                    GpuBuffer?.Dispose();

                    GpuCapacity = (uint)DataArray.Count + GpuCapacity / 2 + 1;

                    var bfDesc = new NxRHI.FBufferDesc();
                    bfDesc.SetDefault();
                    bfDesc.Size = (uint)sizeof(T) * GpuCapacity;
                    bfDesc.StructureStride = (uint)sizeof(T);
                    bfDesc.InitData = DataArray.UnsafeGetElementAddress(0);
                    if (IsGpuWrite)
                    {
                        bfDesc.Type = NxRHI.EBufferType.BFT_UAV;
                        bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                    }
                    else
                    {
                        bfDesc.Type = NxRHI.EBufferType.BFT_SRV;
                        bfDesc.CpuAccess = NxRHI.ECpuAccess.CAS_WRITE;
                        bfDesc.Usage = NxRHI.EGpuUsage.USAGE_DYNAMIC;
                    }
                    GpuBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateBuffer(in bfDesc);
                    System.Diagnostics.Debug.Assert(GpuBuffer != null);

                    if (IsGpuWrite)
                    {
                        var uavDesc = new NxRHI.FUavDesc();
                        uavDesc.SetBuffer(0);
                        uavDesc.Buffer.NumElements = (uint)GpuCapacity;
                        uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                        DataUAV = UEngine.Instance.GfxDevice.RenderContext.CreateUAV(GpuBuffer, in uavDesc);
                    }
                    else
                    {
                        var srvDesc = new NxRHI.FSrvDesc();
                        srvDesc.SetBuffer(0);
                        srvDesc.Buffer.NumElements = (uint)GpuCapacity;
                        srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                        DataSRV = UEngine.Instance.GfxDevice.RenderContext.CreateSRV(GpuBuffer, in srvDesc);
                    }
                }
                else
                {
                    if (IsGpuWrite == false)
                    {
                        if (DataArray.Count > 0)
                            GpuBuffer.mCoreObject.UpdateGpuData(cmd, 0, DataArray.UnsafeGetElementAddress(0), (uint)(sizeof(T) * DataArray.Count));
                    }
                }
            }
        }

        public struct FPointLight
        {
            public Vector4 PositionAndRadius;
            public Vector4 ColorAndIntensity;
        }
        public UGpuDataArray<FPointLight> PointLights = new UGpuDataArray<FPointLight>();
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault();
            desc.Type = NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV;
            unsafe
            {
                desc.Size = (uint)sizeof(FGpuSceneDesc);
                desc.StructureStride = (uint)sizeof(FGpuSceneDesc);
            }
            GpuSceneDescBuffer = rc.CreateBuffer(in desc);
            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(0);
            uavDesc.Buffer.NumElements = (uint)1;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            GpuSceneDescUAV = rc.CreateUAV(GpuSceneDescBuffer, in uavDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetBuffer(0);
            srvDesc.Buffer.NumElements = (uint)1;
            unsafe
            {
                srvDesc.Buffer.StructureByteStride = (uint)sizeof(FGpuSceneDesc);
            }
            GpuSceneDescSRV = rc.CreateSRV(GpuSceneDescBuffer, in srvDesc);

            PointLights.Initialize(false);

            PerGpuSceneCBuffer = rc.CreateCBV(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.Binder.mCoreObject);

            HdrMiddleGrey = 0.6f;
            HdrMinLuminance = 0.01f;
            HdrMaxLuminance = 16.0f;

            Exposure = 1.0f;
            EyeAdapterTimeRange = 5.0f;
        }
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {

        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            if (policy.VisibleNodes == null)
                return;
            var cmd = BasePass.DrawCmdList;
            cmd.BeginCommand();
            PointLights.Clear();
            if (policy.DisablePointLight == false)
            {
                foreach (var i in policy.VisibleNodes)
                {
                    var pointLight = i as GamePlay.Scene.UPointLightNode;
                    if (pointLight == null)
                        continue;

                    var lightData = pointLight.NodeData as GamePlay.Scene.UPointLightNode.ULightNodeData;

                    FPointLight light;
                    var pos = pointLight.Placement.Position;
                    light.PositionAndRadius = new Vector4(pos.ToSingleVector3(), lightData.Radius);
                    light.ColorAndIntensity = new Vector4(lightData.Color.X, lightData.Color.Y, lightData.Color.Z, lightData.Intensity);
                    pointLight.IndexInGpuScene = PointLights.PushData(light);
                }                
                PointLights.Flush2GPU(cmd.mCoreObject);
            }
            //if PerFrameCBuffer dirty :flush
            UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject.FlushDirty(cmd.mCoreObject, false);
            cmd.EndCommand();

            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
