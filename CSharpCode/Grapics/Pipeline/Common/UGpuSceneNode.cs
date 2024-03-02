using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("GpuScene", "GpuScene", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public partial class UGpuSceneNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles");
        public TtRenderGraphPin GpuScenePinOut = TtRenderGraphPin.CreateOutput("GpuScene", false, EPixelFormat.PXF_UNKNOWN);
        public UGpuSceneNode()
        {
            Name = "GpuSceneNode";
        }
        public override void Dispose()
        {
            Dispose_Light();
            Dispose_Instance();

            GpuSceneDescBuffer?.Dispose();
            
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn, NxRHI.EBufferType.BFT_NONE);
            GpuScenePinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(GpuScenePinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            PointLightsPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(PointLightsPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            InstancePinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(InstancePinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public unsafe override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            GpuScenePinOut.Attachement.Height = 1;
            GpuScenePinOut.Attachement.Width = (uint)sizeof(FGpuSceneDesc);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(GpuScenePinOut);
            attachement.Buffer = GpuSceneDescBuffer.GpuResource;
            attachement.Srv = GpuSceneDescBuffer.Srv;
            attachement.Uav = GpuSceneDescBuffer.Uav;
            attachement.Cbv = PerGpuSceneCbv;

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
        public TtGpuBuffer<FGpuSceneDesc> GpuSceneDescBuffer;

        public NxRHI.UCbView PerGpuSceneCbv { get; set; }

        #region SceneConfig
        float mExposure = 1.0f;
        public float Exposure
        {
            get => mExposure;
            set
            {
                if (PerGpuSceneCbv != null)
                {
                    mExposure = value;
                    
                    PerGpuSceneCbv.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.Exposure, in mExposure);
                }
            }
        }
        float mEyeAdapterTimeRange = 4.0f;
        public float EyeAdapterTimeRange
        {
            get => mEyeAdapterTimeRange;
            set
            {
                if (PerGpuSceneCbv != null)
                {
                    mEyeAdapterTimeRange = value;
                    PerGpuSceneCbv.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.EyeAdapterTimeRange, in mEyeAdapterTimeRange);
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
                if (PerGpuSceneCbv != null)
                {
                    PerGpuSceneCbv.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMiddleGrey, in mHdrMiddleGrey);
                }
            }
        }
        float mHdrMaxLuminance = 16.0f;
        public float HdrMaxLuminance
        {
            get => mHdrMaxLuminance;
            set
            {
                if (PerGpuSceneCbv != null)
                {
                    mHdrMaxLuminance = value;
                    PerGpuSceneCbv.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMaxLuminance, in mHdrMaxLuminance);
                }
            }
        }
        float mHdrMinLuminance = 0.01f;
        public float HdrMinLuminance
        {
            get => mHdrMinLuminance;
            set
            {
                if (PerGpuSceneCbv != null)
                {
                    mHdrMinLuminance = value;
                    PerGpuSceneCbv.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.HdrMinLuminance, in mHdrMinLuminance);
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

        public TtCpuCullingNode CpuCullNode = null;
        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc, debugName + ".BasePass");

            GpuSceneDescBuffer = new TtGpuBuffer<FGpuSceneDesc>();
            unsafe
            {
                GpuSceneDescBuffer.SetSize(1, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            }
            PerGpuSceneCbv = rc.CreateCBV(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerGpuScene.Binder.mCoreObject);

            Initialize_Light(policy, debugName);
            await Initialize_Instance(policy, debugName);

            HdrMiddleGrey = 0.6f;
            HdrMinLuminance = 0.01f;
            HdrMaxLuminance = 16.0f;

            Exposure = 1.0f;
            EyeAdapterTimeRange = 5.0f;

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
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
                if (CpuCullNode.VisParameter.VisibleNodes == null)
                    return;

                var cmd = BasePass.DrawCmdList;
                using (new NxRHI.TtCmdListScope(cmd))
                {
                    TickLogic_Light(world, policy, cmd);
                    TickLogic_Instance(world, policy, cmd);
                }
                
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
            }   
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
