using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UGpuSceneNode : Graphics.Pipeline.Common.URenderGraphNode
    {
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
        public RHI.CGpuBuffer GpuSceneDescBuffer;
        public RHI.CUnorderedAccessView GpuSceneDescUAV;
        public RHI.CShaderResourceView GpuSceneDescSRV;

        public RHI.CConstantBuffer PerGpuSceneCBuffer;

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
                    var idx = PerGpuSceneCBuffer.mCoreObject.FindVar("Exposure");
                    PerGpuSceneCBuffer.SetValue(idx, in mExposure);
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
                    var idx = PerGpuSceneCBuffer.mCoreObject.FindVar("EyeAdapterTimeRange");
                    PerGpuSceneCBuffer.SetValue(idx, in mEyeAdapterTimeRange);
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
                    var idx = PerGpuSceneCBuffer.mCoreObject.FindVar("HdrMiddleGrey");
                    PerGpuSceneCBuffer.SetValue(idx, in mHdrMiddleGrey);
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
                    var idx = PerGpuSceneCBuffer.mCoreObject.FindVar("HdrMaxLuminance");
                    PerGpuSceneCBuffer.SetValue(idx, in mHdrMaxLuminance);
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
                    var idx = PerGpuSceneCBuffer.mCoreObject.FindVar("HdrMinLuminance");
                    PerGpuSceneCBuffer.SetValue(idx, in mHdrMinLuminance);
                }
            }
        }
        #endregion

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        public class UGpuDataArray<T> where T : unmanaged
        {
            public Support.UNativeArray<T> DataArray;
            private uint GpuCapacity = 0;
            public RHI.CGpuBuffer GpuBuffer;
            public bool IsGpuWrite { get; private set; } = false;
            public RHI.CUnorderedAccessView DataUAV;
            public RHI.CShaderResourceView DataSRV;
            private bool Dirty = false;
            public bool Initialize(bool gpuWrite)
            {
                IsGpuWrite = gpuWrite;
                DataArray = Support.UNativeArray<T>.CreateInstance();
                return true;
            }
            public void Cleanup()
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
            public unsafe void Flush2GPU(ICommandList cmd)
            {
                if (Dirty == false)
                    return;
                Dirty = false;
                if (DataArray.Count >= GpuCapacity)
                {
                    GpuBuffer?.Dispose();

                    GpuCapacity = (uint)DataArray.Count + GpuCapacity / 2 + 1;

                    var bfDesc = new IGpuBufferDesc();
                    if (IsGpuWrite)
                        bfDesc.SetMode(false, true);
                    else
                        bfDesc.SetMode(true, false);
                    bfDesc.m_ByteWidth = (uint)sizeof(T) * GpuCapacity;
                    bfDesc.m_StructureByteStride = (uint)sizeof(T);
                    GpuBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateGpuBuffer(in bfDesc, (IntPtr)DataArray.UnsafeGetElementAddress(0));
                    System.Diagnostics.Debug.Assert(GpuBuffer != null);

                    if (IsGpuWrite)
                    {
                        var uavDesc = new IUnorderedAccessViewDesc();
                        uavDesc.SetBuffer();
                        uavDesc.Buffer.NumElements = (uint)GpuCapacity;
                        DataUAV = UEngine.Instance.GfxDevice.RenderContext.CreateUnorderedAccessView(GpuBuffer, in uavDesc);
                    }
                    else
                    {
                        var srvDesc = new IShaderResourceViewDesc();
                        srvDesc.SetBuffer();
                        srvDesc.Buffer.NumElements = (uint)GpuCapacity;
                        srvDesc.mGpuBuffer = GpuBuffer.mCoreObject;
                        DataSRV = UEngine.Instance.GfxDevice.RenderContext.CreateShaderResourceView(in srvDesc);
                    }
                }
                else
                {
                    if (IsGpuWrite == false)
                    {
                        if (DataArray.Count > 0)
                            GpuBuffer.mCoreObject.UpdateBufferData(cmd, 0, DataArray.UnsafeGetElementAddress(0), (uint)(sizeof(T) * DataArray.Count));
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
        public async override System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName);

            var desc = new IGpuBufferDesc();
            desc.SetMode(false, true);
            unsafe
            {
                desc.m_ByteWidth = (uint)sizeof(FGpuSceneDesc);
                desc.m_StructureByteStride = (uint)sizeof(FGpuSceneDesc);
            }
            GpuSceneDescBuffer = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)1;
            GpuSceneDescUAV = rc.CreateUnorderedAccessView(GpuSceneDescBuffer, in uavDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetBuffer();
            srvDesc.Buffer.NumElements = (uint)1;
            srvDesc.mGpuBuffer = GpuSceneDescBuffer.mCoreObject;
            GpuSceneDescSRV = rc.CreateShaderResourceView(in srvDesc);

            PointLights.Initialize(false);            

            var cbIndex = UEngine.Instance.GfxDevice.EffectManager.DummyEffect.ShaderProgram.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            PerGpuSceneCBuffer = rc.CreateConstantBuffer(UEngine.Instance.GfxDevice.EffectManager.DummyEffect.ShaderProgram, cbIndex);

            HdrMiddleGrey = 0.6f;
            HdrMinLuminance = 0.01f;
            HdrMaxLuminance = 16.0f;

            Exposure = 1.0f;
            EyeAdapterTimeRange = 5.0f;
        }
        public void Cleanup()
        {
            PointLights.Cleanup();

            GpuSceneDescSRV?.Dispose();
            GpuSceneDescSRV = null;
            GpuSceneDescUAV?.Dispose();
            GpuSceneDescUAV = null;
            GpuSceneDescBuffer?.Dispose();
            GpuSceneDescBuffer = null;
        }
        public override unsafe void OnResize(IRenderPolicy policy, float x, float y)
        {

        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            if (policy.VisibleNodes == null)
                return;
            var cmd = BasePass.DrawCmdList.mCoreObject;

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
                PointLights.Flush2GPU(cmd);
            }
            //if PerFrameCBuffer dirty :flush
            UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject.UpdateDrawPass(cmd, 1);
            if (cmd.BeginCommand())
            {
                cmd.EndCommand();
            }
        }
        public unsafe override void TickRender(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
