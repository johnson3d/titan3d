using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("GpuScene", "GpuScene", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UGpuSceneNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UGpuSceneNode" })]
    public partial class TtGpuSceneNode : TAuxRenderGraphNode<TtGpuSceneNode>
    {
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles", NxRHI.EBufferType.BFT_NONE);
        public TtRenderGraphPin GpuScenePinOut = TtRenderGraphPin.CreateOutput("GpuScene", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtGpuSceneNode()
        {
            Name = "GpuSceneNode";
        }
        public override void Dispose()
        {
            Dispose_Light();
            Dispose_Instance();

            CoreSDK.DisposeObject(ref GpuSceneDescBuffer);
            CoreSDK.DisposeObject(ref GpuSceneAttachement);

            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn);
            GpuScenePinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(GpuScenePinOut);
            PointLightsPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(PointLightsPinOut);
            InstancePinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(InstancePinOut);
        }
        TtAttachBuffer GpuSceneAttachement = new TtAttachBuffer();
        public unsafe override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            GpuScenePinOut.Attachement.Height = 1;
            GpuScenePinOut.Attachement.Width = (uint)sizeof(Shader.FGpuSceneDesc);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(GpuScenePinOut, GpuSceneAttachement);
            attachement.GpuResource = GpuSceneDescBuffer.GpuResource;
            attachement.Srv = GpuSceneDescBuffer.Srv;
            attachement.Uav = GpuSceneDescBuffer.Uav;
            attachement.Cbv = PerGpuSceneCbv;

            FrameBuild_Light();
            FrameBuild_Instance();
        }
        
        public TtGpuBuffer<Shader.FGpuSceneDesc> GpuSceneDescBuffer;

        public NxRHI.TtCbView PerGpuSceneCbv { get; set; }

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
                    
                    PerGpuSceneCbv.SetValue(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.Exposure, in mExposure);
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
                    PerGpuSceneCbv.SetValue(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.EyeAdapterTimeRange, in mEyeAdapterTimeRange);
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
                    PerGpuSceneCbv.SetValue(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.HdrMiddleGrey, in mHdrMiddleGrey);
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
                    PerGpuSceneCbv.SetValue(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.HdrMaxLuminance, in mHdrMaxLuminance);
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
                    PerGpuSceneCbv.SetValue(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.HdrMinLuminance, in mHdrMinLuminance);
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
        public async override System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            GpuSceneDescBuffer = new TtGpuBuffer<Shader.FGpuSceneDesc>();
            unsafe
            {
                GpuSceneDescBuffer.SetSize(1, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            }
            PerGpuSceneCbv = rc.CreateCBV(TtCoreShaderBinder.TtPerGpuSceneCBufferVarIndexer.Instance.Binder.mCoreObject);

            Initialize_Light(policy, debugName);
            Initialize_Instance(policy, debugName);

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
        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {

        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (CpuCullNode.VisParameter.VisibleNodes == null)
                return;

            var cmd = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "GpuScene"))
            {
                TickLogic_Light(world, policy, cmd);
                TickLogic_Instance(world, policy, cmd);
            }

            policy.CommitCommandList(cmd, "GpuScene");
        }
        public unsafe override void TickSync(Graphics.Pipeline.TtRenderPolicy policy)
        {
        }
    }
}
