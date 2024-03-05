using Assimp;
using EngineNS.Bricks.RenderPolicyEditor;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Node
{
    public class TtErosionIncWaterShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(32, 32, 1);
        }
        public TtErosionIncWaterShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Procedure/Erosion/IncWater.compute", RName.ERNameType.Engine);
            MainName = "CS_IncWaterMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtErosionIncWaterNode;
            var uav = policy.AttachmentCache.FindAttachement(node.WaterPinInOut).Uav;
            drawcall.BindUav("WaterTexture", uav);

            var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_Sampler, "Samp_RainTexture");
            if (binder.IsValidPointer)
            {

            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "RainTexture");
            if (binder.IsValidPointer)
            {

            }
            drawcall.BindSrv(binder, null);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("IncWater", "PGC\\Erosion\\IncWater", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtErosionIncWaterNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin WaterPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Water", false, EPixelFormat.PXF_R32_FLOAT);

        public TtErosionIncWaterShading ShadingEnv;
        public NxRHI.UCommandList mCmdList;
        private NxRHI.UComputeDraw mDrawcall;
        public TtErosionIncWaterNode()
        {
            Name = "ErosionIncWater";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawcall);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override void InitNodePins()
        {
            AddInputOutput(WaterPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            WaterPinInOut.IsAllowInputNull = true;

            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ShadingEnv = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtErosionIncWaterShading>();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            mCmdList = rc.CreateCommandList();
            mDrawcall = rc.CreateComputeDraw();
            mDrawcall.TagObject = this;
        }
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList))
            {
                ShadingEnv.SetDrawcallDispatch(this, policy, mDrawcall, 1, 1, 1, true);
                mCmdList.PushGpuDraw(mDrawcall);
                //mCmdList.PushAction(static (EngineNS.NxRHI.ICommandList cmd, void* arg1) =>
                //{
                    
                //}, IntPtr.Zero.ToPointer());
                mCmdList.FlushDraws();
            }
            
            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Ending", "PGC\\Erosion\\Ending", UPolicyGraph.RGDEditorKeyword)]
    public class TtErosionEndingNode : Graphics.Pipeline.Common.TtEndingNode
    {
        public Graphics.Pipeline.TtRenderGraphPin HeightPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Height");
        public TtErosionEndingNode()
        {
            Name = "ErosionEndingNode";
        }
        public override void InitNodePins()
        {
            AddInput(HeightPinIn, NxRHI.EBufferType.BFT_SRV);
            HeightPinIn.IsAllowInputNull = true;
        }
        public override void Dispose()
        {
            CoreSDK.DisposePtr(ref ReadableTexture);
            CoreSDK.DisposeObject(ref mFinishFence);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            mFinishFence = rc.CreateFence(in fenceDesc, "TtErosionEndingNode");
            mCmdList = rc.CreateCommandList();
        }
        public NxRHI.UFence mFinishFence;
        public NxRHI.UCommandList mCmdList;
        public NxRHI.IBuffer ReadableTexture;
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList))
            {
                var cpDraw = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                var texture = policy.AttachmentCache.FindAttachement(HeightPinIn).Buffer as NxRHI.UTexture;
                CoreSDK.DisposePtr(ref ReadableTexture);
                ReadableTexture = texture.CreateReadable(0, cpDraw.mCoreObject);
                mCmdList.PushGpuDraw(cpDraw);
                mCmdList.FlushDraws();
            }
            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(mFinishFence, NxRHI.EQueueType.QU_Compute);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("GpuErosion", "Float1\\GpuErosion", UPgcGraph.PgcEditorKeyword)]
    public class TtGpuErosionNode : Node.UAnyTypeMonocular
    {
        RName mPolicyName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = URenderPolicyAsset.AssetExt)]
        public RName PolicyName
        {
            get
            {
                return mPolicyName;
            }
            set
            {
                var action = async () =>
                {
                    var policy = URenderPolicyAsset.LoadAsset(value).CreateRenderPolicy(null, "ErosionEndingNode");
                    await policy.Initialize(null);
                    Policy = policy;
                    mPolicyName = value;
                };
                action();
            }
        }
        public bool IsCapture { get; set; } = false;
        Graphics.Pipeline.URenderPolicy Policy;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (Policy == null)
                return false;
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            var buffer = Input.GetGpuTexture2D<float>();
            //Input.Upload2GpuTexture2D()
            var incWater = Policy.FindNode<TtErosionIncWaterNode>("ErosionIncWater");
            var waterAttachement = Policy.AttachmentCache.ImportAttachment(incWater.WaterPinInOut);
            waterAttachement.SetImportedBuffer(buffer);
            var ending = Policy.RootNode as TtErosionEndingNode;
            var processor = new TtPgcGpuProcessor();
            processor.Policy = Policy;
            //processor.OnBufferRemoved = (Graphics.Pipeline.TtRenderGraphNode node, Graphics.Pipeline.TtRenderGraphPin pin, Graphics.Pipeline.TtAttachBuffer bf)=>
            //{
            //    if (ending == node && pin == ending.HeightPinIn)
            //    {

            //    }
            //};

            if (IsCapture)
            {
                UEngine.Instance.GfxDevice.RenderCmdQueue.CaptureRenderDocFrame = true;
                UEngine.Instance.GfxDevice.RenderCmdQueue.BeginFrameCapture();
            }
            processor.Process();
            if (IsCapture)
            {
                UEngine.Instance.GfxDevice.RenderCmdQueue.EndFrameCapture(this.Name);
            }

            ending.mFinishFence.WaitToExpect();
            var readTexture = ending.ReadableTexture;
            var blob = new Support.UBlobObject();
            readTexture.FetchGpuData(0, blob.mCoreObject);
            using (var reader = IO.UMemReader.CreateInstance((byte*)blob.DataPointer, blob.Size))
            {
                uint rowPitch, depthPitch;
                reader.Read(out rowPitch);
                reader.Read(out depthPitch);
                var pImage = ((byte*)blob.DataPointer + reader.GetPosition());

                for (int y = 0; y < Output.Height; y++)
                {
                    for (int x = 0; x < Output.Width; x++)
                    {
                        Output.SetFloat1(x, y, 0, ((float*)pImage)[x]);
                    }
                    pImage += rowPitch;
                }
            }   
            return true;
        }
    }
}
