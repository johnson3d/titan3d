using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common.ColorGrading;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class TtHdrShading : Shader.TtGraphicsShadingEnv
    {
        // Permutation: ENABLE_COLOR_GRADING_LUT (0 = legacy tonemap, 1 = LUT path)
        public TtPermutationItem EnableColorGradingLUT { get; set; }

        [Category("Option")]
        public bool IsEnableColorGradingLUT
        {
            get { return EnableColorGradingLUT.GetValue() == (int)EPermutation_Bool.TrueValue; }
            set { EnableColorGradingLUT.SetValue(value); this.UpdatePermutation().AddWaitTask(); }
        }

        public TtHdrShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/screenspace/hdr.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();
            EnableColorGradingLUT = this.PushPermutation<EPermutation_Bool>("ENABLE_COLOR_GRADING_LUT", (int)EPermutation_Bool.BitWidth);
            EnableColorGradingLUT.SetValue((int)EPermutation_Bool.FalseValue);

            UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtRenderMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var node = drawcall.TagObject as TtHdrNode;
            var lightSRV = node.GetAttachBuffer(node.ColorPinIn).Srv;
            var gpuSceneDescSRV = node.GetAttachBuffer(node.GpuScenePinIn).Srv;
            
            var index = drawcall.FindBinder("GSourceTarget");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, lightSRV);
            index = drawcall.FindBinder("Samp_GSourceTarget");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("GpuSceneDescSRV");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, gpuSceneDescSRV);

            index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
                drawcall.BindCBV(index, policy.GetGpuSceneNode().PerGpuSceneCbv);

            // Color Grading dual-LUT blend (only bound when ENABLE_COLOR_GRADING_LUT=1)
            if (IsEnableColorGradingLUT && node.LutSrv0 != null)
            {
                index = drawcall.FindBinder("ColorGradingLUT0");
                if (index.IsValidPointer)
                    drawcall.BindSRV(index, node.LutSrv0);

                if (node.LutSrv1 != null)
                {
                    index = drawcall.FindBinder("ColorGradingLUT1");
                    if (index.IsValidPointer)
                        drawcall.BindSRV(index, node.LutSrv1);
                }

                index = drawcall.FindBinder("Samp_ColorGradingLUT0");
                if (index.IsValidPointer)
                    drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

                // Blend weight cbuffer
                index = drawcall.FindBinder("cbColorGradingBlend");
                if (index.IsValidPointer)
                {
                    if (node.mCBColorGradingBlend == null)
                        node.mCBColorGradingBlend = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);

                    node.mCBColorGradingBlend.SetValue("LutBlendWeight", node.LutBlendWeight);
                    node.mCBColorGradingBlend.SetValue("HasSecondLut", node.LutSrv1 != null ? 1.0f : 0.0f);
                    node.mCBColorGradingBlend.FlushDirty();
                    drawcall.BindCBV(index, node.mCBColorGradingBlend);
                }
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Hdr", "Post\\Hdr", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UHdrNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UHdrNode" })]
    public class TtHdrNode : TAuxSceenSpaceNode<TtHdrNode>
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin GpuScenePinIn = TtRenderGraphPin.CreateInput("GpuScene", NxRHI.EBufferType.BFT_SRV);

        // ── Dual-LUT blend data (collected from active PostProcessVolumes each frame) ──
        internal NxRHI.TtSrView LutSrv0;
        internal NxRHI.TtSrView LutSrv1;
        internal float LutBlendWeight;
        internal NxRHI.TtCbView mCBColorGradingBlend;

        public TtHdrNode()
        {
            Name = "HdrNode";
        }
        public override void InitNodePins()
        {
            AddInput(ColorPinIn);
            AddInput(GpuScenePinIn);

            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            base.InitNodePins();
        }
        public TtHdrShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtShadingEnv.CreateShadingEnv<TtHdrShading>();
        }
        public override void Tick(TtWorld world, TtRenderPolicy policy, TtCommandList frameCmdList, bool bClear)
        {
            // Collect top-2 PostProcessVolume LUTs; each Volume updates its own LUT only when dirty
            bool hasActiveLut = CollectVolumeLuts(world, policy);

            if (hasActiveLut)
            {
                if (!mBasePassShading.IsEnableColorGradingLUT)
                    mBasePassShading.IsEnableColorGradingLUT = true;
            }
            else
            {
                LutSrv0 = null;
                LutSrv1 = null;
                LutBlendWeight = 0f;

                if (mBasePassShading.IsEnableColorGradingLUT)
                    mBasePassShading.IsEnableColorGradingLUT = false;
            }

            base.Tick(world, policy, frameCmdList, bClear);
        }

        /// <summary>
        /// Query PostProcessVolumes overlapping the camera, pick top-2 by (priority, influence),
        /// tell each to update its LUT if dirty, then store their SRVs + blend weight.
        /// Camera movement only changes the blend weight — no LUT rebuild occurs.
        /// </summary>
        readonly List<TtPostProcessVolumeNode> mQueryResults = new List<TtPostProcessVolumeNode>();

        bool CollectVolumeLuts(TtWorld world, TtRenderPolicy policy)
        {
            if (world == null || policy?.DefaultCamera == null)
                return false;

            var cameraPos = policy.DefaultCamera.GetPosition();

            mQueryResults.Clear();
            world.QueryVolumes(in cameraPos, mQueryResults);
            if (mQueryResults.Count == 0)
                return false;

            // Find top-2 volumes by (priority, influence)
            TtPostProcessVolumeNode bestVolume = null;
            float bestInfluence = 0f;
            int bestPriority = int.MinValue;

            TtPostProcessVolumeNode secondVolume = null;
            float secondInfluence = 0f;
            int secondPriority = int.MinValue;

            for (int i = 0; i < mQueryResults.Count; i++)
            {
                var vol = mQueryResults[i];
                float influence = vol.ComputeInfluence(in cameraPos);
                if (influence <= 0f)
                    continue;

                var volData = vol.GetNodeData<TtVolumeBaseNode.TtVolumeBaseData>();
                int priority = volData?.Priority ?? 0;

                if (priority > bestPriority || (priority == bestPriority && influence > bestInfluence))
                {
                    secondVolume = bestVolume;
                    secondInfluence = bestInfluence;
                    secondPriority = bestPriority;

                    bestVolume = vol;
                    bestInfluence = influence;
                    bestPriority = priority;
                }
                else if (priority > secondPriority || (priority == secondPriority && influence > secondInfluence))
                {
                    secondVolume = vol;
                    secondInfluence = influence;
                    secondPriority = priority;
                }
            }

            if (bestVolume == null)
                return false;

            // Let each active volume update its own LUT (only dispatches compute if settings changed)
            bestVolume.EnsureLutUpToDate(policy);
            LutSrv0 = bestVolume.LutSrv;

            if (secondVolume != null && secondInfluence > 0f)
            {
                secondVolume.EnsureLutUpToDate(policy);
                LutSrv1 = secondVolume.LutSrv;

                // Weight: how much of LUT1 to blend in (0 = 100% LUT0, 1 = 100% LUT1)
                float totalInfluence = bestInfluence + secondInfluence;
                LutBlendWeight = secondInfluence / totalInfluence;
            }
            else
            {
                LutSrv1 = null;
                // When partially inside a single volume, fade between identity (no LUT) and LUT0
                // weight=0 means fully use LUT0; for partial influence we'll handle in shader
                // by checking if LUT1 is bound
                LutBlendWeight = bestInfluence;
            }

            // Release stale LUTs on ALL volumes (not just query results) to avoid leaking idle ones
            world.TickVolumeLutCleanup();

            return LutSrv0 != null;
        }

        public override void Dispose()
        {
            LutSrv0 = null;
            LutSrv1 = null;
            CoreSDK.DisposeObject(ref mCBColorGradingBlend);
            base.Dispose();
        }
    }
}
