using System;
using System.Collections.Generic;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.Procedure.Node.GpuShading
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
                drawcall.BindSampler(binder, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "RainTexture");
            if (binder.IsValidPointer)
            {
                drawcall.BindSrv(binder, node.mRainTexture);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPgc");
            if (binder.IsValidPointer)
            {
                drawcall.BindCBuffer(binder, node.GetCBuffer(binder));
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("IncWater", "PGC\\Erosion\\IncWater", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtErosionIncWaterNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin WaterPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Water", false, EPixelFormat.PXF_R32_FLOAT);

        public TtErosionIncWaterShading ShadingEnv;
        public NxRHI.UCommandList mCmdList;
        private NxRHI.UComputeDraw mDrawcall;
        public Vector3ui DispatchThread = new Vector3ui(1, 1, 1);
        public RName mRain;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.USrView.AssetExt)]
        public RName Rain 
        {
            get => mRain;
            set
            {
                mRain = value;
                var action = async () =>
                {
                    mRainTexture = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                action();
            }
        }
        internal NxRHI.USrView mRainTexture;
        [Rtti.Meta]
        public float RainScalar { get; set; } = 10.0f;
        public int TextureWidth { get; set; } = 1;
        public int TextureHeight { get; set; } = 1;
        public NxRHI.UCbView CBuffer = null;
        public NxRHI.UCbView GetCBuffer(NxRHI.FShaderBinder binder)
        {
            if (CBuffer == null)
            {
                CBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }
            CBuffer.SetValue("TextureWidth", TextureWidth);
            CBuffer.SetValue("TextureHeight", TextureHeight);
            CBuffer.SetValue("RainScalar", RainScalar);
            return CBuffer;
        }
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
                ShadingEnv.SetDrawcallDispatch(this, policy, mDrawcall, DispatchThread.X, DispatchThread.Y, DispatchThread.Z, true);
                mCmdList.PushGpuDraw(mDrawcall);

                if (CBuffer != null)
                    CBuffer.FlushDirty(mCmdList.mCoreObject);
                mCmdList.FlushDraws();
            }

            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
        }
    }

    public class TtHeigh2FlowMapShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(32, 32, 1);
        }
        public TtHeigh2FlowMapShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Procedure/Height2FlowMap.compute", RName.ERNameType.Engine);
            MainName = "CS_Heigh2FlowMapMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtHeigh2FlowMapNode;
            
            var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "HeightTexture");
            if (binder.IsValidPointer)
            {
                var height = policy.AttachmentCache.FindAttachement(node.HeightPinIn);
                drawcall.BindSrv(binder, height.Srv);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "FlowTexture");
            if (binder.IsValidPointer)
            {
                var flow = policy.AttachmentCache.GetAttachement(node.FlowMapPinOut);
                drawcall.BindUav(binder, flow.Uav);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GapTexture");
            if (binder.IsValidPointer)
            {
                var gap = policy.AttachmentCache.GetAttachement(node.GapMapPinOut);
                drawcall.BindUav(binder, gap.Uav);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Height2Flow", "PGC\\Height2Flow", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtHeigh2FlowMapNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin HeightPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Height");
        public Graphics.Pipeline.TtRenderGraphPin FlowMapPinOut = Graphics.Pipeline.TtRenderGraphPin.CreateOutput("Flow", true, EPixelFormat.PXF_R32G32_FLOAT);
        public Graphics.Pipeline.TtRenderGraphPin GapMapPinOut = Graphics.Pipeline.TtRenderGraphPin.CreateOutput("Gap", true, EPixelFormat.PXF_R32_FLOAT);

        public TtHeigh2FlowMapShading ShadingEnv;
        public NxRHI.UCommandList mCmdList;
        private NxRHI.UComputeDraw mDrawcall;
        public Vector3ui DispatchThread = new Vector3ui(1,1,1);
        public TtHeigh2FlowMapNode()
        {
            Name = "Heigh2FlowMap";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawcall);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override void InitNodePins()
        {
            AddInputOutput(HeightPinIn, NxRHI.EBufferType.BFT_SRV);
            HeightPinIn.IsAllowInputNull = true;
            AddOutput(FlowMapPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            AddOutput(GapMapPinOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ShadingEnv = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtHeigh2FlowMapShading>();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            mCmdList = rc.CreateCommandList();
            mDrawcall = rc.CreateComputeDraw();
            mDrawcall.TagObject = this;
        }
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList))
            {
                ShadingEnv.SetDrawcallDispatch(this, policy, mDrawcall, DispatchThread.X, DispatchThread.Y, DispatchThread.Z, true);
                mCmdList.PushGpuDraw(mDrawcall);
                mCmdList.FlushDraws();
            }

            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
        }
    }

    public class TtWaterBasinShading : Graphics.Pipeline.Shader.UComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(32, 32, 1);
        }
        public TtWaterBasinShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Procedure/WaterBasin.compute", RName.ERNameType.Engine);
            MainName = "CS_WaterBasinMain";

            this.UpdatePermutation();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.URenderPolicy policy)
        {
            var node = drawcall.TagObject as TtWaterBasinNode;

            var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "HeightTexture");
            if (binder.IsValidPointer)
            {
                var height = policy.AttachmentCache.FindAttachement(node.HeightPinIn);
                drawcall.BindSrv(binder, height.Srv);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "FlowTexture");
            if (binder.IsValidPointer)
            {
                var flow = policy.AttachmentCache.GetAttachement(node.FlowPinIn);
                drawcall.BindSrv(binder, flow.Srv);
            }
            binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "WaterTexture");
            if (binder.IsValidPointer)
            {
                var water = policy.AttachmentCache.GetAttachement(node.WaterPinInOut);
                drawcall.BindUav(binder, water.Uav);
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("WaterBasin", "PGC\\WaterBasin", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtWaterBasinNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin HeightPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Height");
        public Graphics.Pipeline.TtRenderGraphPin FlowPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Flow");
        public Graphics.Pipeline.TtRenderGraphPin WaterPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Water");

        public TtWaterBasinShading ShadingEnv;
        public NxRHI.UCommandList mCmdList;
        private NxRHI.UComputeDraw mDrawcall;
        public Vector3ui DispatchThread = new Vector3ui(1, 1, 1);
        [Rtti.Meta]
        public int Step { get; set; } = 16;
        public TtWaterBasinNode()
        {
            Name = "WaterBasin";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDrawcall);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override void InitNodePins()
        {
            AddInput(HeightPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(FlowPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(WaterPinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            base.InitNodePins();
        }
        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ShadingEnv = UEngine.Instance.ShadingEnvManager.GetShadingEnv<TtWaterBasinShading>();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            mCmdList = rc.CreateCommandList();
            mDrawcall = rc.CreateComputeDraw();
            mDrawcall.TagObject = this;
        }
        public unsafe override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList))
            {
                ShadingEnv.SetDrawcallDispatch(this, policy, mDrawcall, DispatchThread.X, DispatchThread.Y, DispatchThread.Z, true);
                mCmdList.PushGpuDraw(mDrawcall);

                for (int i = 0; i < Step; i++)
                {
                    mCmdList.FlushDraws();
                }
            }

            UEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
        }
    }
}
