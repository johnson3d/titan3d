using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.Procedure.Node.GpuShading
{
    public class TtGpuSkinLUT3SGenShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
    {
        public override Vector3ui DispatchArg
        {
            get => new Vector3ui(32, 32, 1);
        }
        public TtGpuSkinLUT3SGenShading()
        {
            CodeName = RName.GetRName("Shaders/Bricks/Procedure/Lut3S/SkinLUT3SGen.cginc", RName.ERNameType.Engine);
            MainName = "CSMain";

            this.UpdatePermutation().AddWaitTask();
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            base.EnvShadingDefines(in id, defines);
        }
        public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
        {
            var node = drawcall.TagObject as TtWaterBasinNode;

            //var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "HeightTexture");
            //if (binder.IsValidPointer)
            //{
            //    var height = policy.AttachmentCache.FindAttachement(node.HeightPinIn);
            //    drawcall.BindSrv(binder, height.Srv);
            //}
            //binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "FlowTexture");
            //if (binder.IsValidPointer)
            //{
            //    var flow = policy.AttachmentCache.GetAttachement(node.FlowPinIn);
            //    drawcall.BindSrv(binder, flow.Srv);
            //}
            //binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "PrevWaterTexture");
            //if (binder.IsValidPointer)
            //{
            //    drawcall.BindSrv(binder, node.PrevWaterTexture.Srv);
            //}
            //binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "WaterTexture");
            //if (binder.IsValidPointer)
            //{
            //    var water = policy.AttachmentCache.GetAttachement(node.WaterPinInOut);
            //    drawcall.BindUav(binder, water.Uav);
            //}
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Skin3S", "PGC\\Skin3S", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtGpuSkinLUT3SGenNode : TAuxRenderGraphNode<TtGpuSkinLUT3SGenNode> 
    {
        public Graphics.Pipeline.TtRenderGraphPin HeightPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Height", NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin FlowPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Flow", NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin WaterPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Water", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

        public TtWaterBasinShading ShadingEnv;
        public NxRHI.TtCommandList mCmdList;
        private NxRHI.TtComputeDraw mDrawcall;
        private NxRHI.TtCopyDraw mCopyDrawcall;
        public Vector3ui DispatchThread = new Vector3ui(8, 8, 1);

        public Graphics.Pipeline.TtGpuBuffer<uint> PrevWaterTexture = new();
        [Rtti.Meta("")]
        public int Step { get; set; } = 16;
        public TtGpuSkinLUT3SGenNode()
        {
            Name = "Skin3S";
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mCopyDrawcall);
            CoreSDK.DisposeObject(ref mDrawcall);
            CoreSDK.DisposeObject(ref mCmdList);
        }
        public override void InitNodePins()
        {
            AddInput(HeightPinIn);
            AddInput(FlowPinIn);
            AddInputOutput(WaterPinInOut);

            base.InitNodePins();
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            ShadingEnv = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtWaterBasinShading>();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            mCmdList = rc.CreateCommandList();
            mDrawcall = rc.CreateComputeDraw();
            mDrawcall.TagObject = this;
            mCopyDrawcall = rc.CreateCopyDraw();
        }
        public unsafe override void BeforeTick(TtRenderPolicy policy)
        {
            var water = policy.AttachmentCache.FindAttachement(WaterPinInOut);

            var count = water.Buffer.mCoreObject.Desc.Size / sizeof(uint);
            if (PrevWaterTexture.NumElement != count)
            {
                PrevWaterTexture.SetSize(count, IntPtr.Zero.ToPointer(), NxRHI.EBufferType.BFT_SRV);
            }
            mCopyDrawcall.Copy(PrevWaterTexture.GpuBuffer, water.Buffer as NxRHI.TtBuffer);
        }
        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            using (new NxRHI.TtCmdListScope(mCmdList, "PCG.kinLUT3SGen"))
            {
                ShadingEnv.SetDrawcallDispatch(this, policy, mDrawcall, DispatchThread.X, DispatchThread.Y, DispatchThread.Z, true);

                for (int i = 0; i < Step; i++)
                {
                    mCmdList.PushGpuDraw(mCopyDrawcall);
                    mCmdList.PushGpuDraw(mDrawcall);
                }
                mCmdList.FlushDraws();
            }

            TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.ExecuteCommandList(mCmdList, NxRHI.EQueueType.QU_Compute);
        }
    }
}
