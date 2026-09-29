using EngineNS.GamePlay;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtFogShading : Shader.TtGraphicsShadingEnv
    {
        public TtFogShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/FogShading.cginc", RName.ERNameType.Engine);

            TypeFog = this.PushPermutation<Graphics.Pipeline.TtRenderPolicy.ETypeFog>("ENV_FOGFACTOR_TYPE", (int)Graphics.Pipeline.TtRenderPolicy.ETypeFog.TypeCount);

            TypeFog.SetValue((int)Graphics.Pipeline.TtRenderPolicy.ETypeFog.None);

            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
            defines.AddDefine("TypeFog_None", (int)Graphics.Pipeline.TtRenderPolicy.ETypeFog.None);
            defines.AddDefine("TypeFog_ExpHeight", (int)Graphics.Pipeline.TtRenderPolicy.ETypeFog.ExpHeight);
        }

        public TtPermutationItem TypeFog
        {
            get;
            set;
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var aaNode = drawcall.TagObject as TtFogNode;
            if (aaNode == null)
                aaNode = policy.FindFirstNode<Common.TtFogNode>();
            if (aaNode == null)
                return;

            switch (policy.TypeFog)
            {
                case TtRenderPolicy.ETypeFog.None:
                    OnDrawcallEHF(drawcall, policy, aaNode);
                    break;
                case TtRenderPolicy.ETypeFog.ExpHeight:
                    OnDrawcallEHF(drawcall, policy, aaNode);
                    break;
            }
        }
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UFogNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UFogNode" })]
    public partial class TtFogNode : TAuxSceenSpaceNode<TtFogNode>
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin NoisePinIn = TtRenderGraphPin.CreateInput("Noise", NxRHI.EBufferType.BFT_SRV);
        public TtFogNode()
        {
            Name = "FogNode";
            TtFogNode_InitExpHeight();
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn);

            AddInput(DepthPinIn);

            // Noise 允许悬空: volume_cloud.rpolicy 不接 Noise, 不设此标记会让 BuildGraph 报 hasInputError -> CreateRenderPolicy 返回 null
            NoisePinIn.IsAllowInputNull = true;
            AddInput(NoisePinIn);

            base.InitNodePins();
        }
        public TtFogShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtFogShading>();
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public override void BeforeTick(TtRenderPolicy policy)
        {
            if (policy.TypeFog == TtRenderPolicy.ETypeFog.None)
            {
                this.MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }

            var buffer = this.FindAttachBuffer(ColorPinIn);
            if (buffer != null)
            {
                if (ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
                {
                    this.CreateGBuffers(policy, buffer.BufferDesc.Format);
                    ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;    
                }
            }
        }
        public NxRHI.TtCbView CBShadingEnv;
        public override void Tick(TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (policy.TypeFog == TtRenderPolicy.ETypeFog.None)
            {
                return;
            }
            // 每帧把世界主光方向同步到雾的内散射方向(约 1 帧延迟, 对雾无感知影响)
            SyncSunToFog(world);
            base.Tick(world, policy, frameCmdList, bClear);
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            if (policy.TypeFog == TtRenderPolicy.ETypeFog.None)
            {
                return;
            }
            base.TickSync(policy);
            switch (policy.TypeFog)
            {
                case TtRenderPolicy.ETypeFog.None:
                    break;
                case TtRenderPolicy.ETypeFog.ExpHeight:
                    TickSyncEHF(policy);
                    break;
            }
        }
    }
}
