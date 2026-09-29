using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Support;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.FX.Weather
{
    public class TtSkyAtmosphereShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtSkyAtmosphereShading()
        {
            CodeName = RName.GetRName("shaders/bricks/fx/skyatmosphere.cginc", RName.ERNameType.Engine);
            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtSkyAtmosphereNode;
            if (aaNode == null || aaNode.SceneNode == null)
            {
                base.OnDrawCall(cmd, drawcall, policy, atom);
                return;
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                if (attachBuffer != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("cbSkyAtmosphere");
            if (index.IsValidPointer)
            {
                if (aaNode.SkyCbv == null)
                {
                    // §1.1: CreateCBV 后立即全字段 SetValue + MarkDirty + FlushDirty
                    aaNode.SkyCbv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                    aaNode.SkyCbv.SetValue("Sky", in aaNode.SceneNode.SkyStruct);
                    aaNode.SkyCbv.MarkDirty();
                    aaNode.SkyCbv.FlushDirty();
                }
                drawcall.BindCBV(index, aaNode.SkyCbv);
            }
            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("SkyAtmosphere", "Post\\SkyAtmosphere", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtSkyAtmosphereNode : Graphics.Pipeline.Common.TAuxSceenSpaceNode<TtSkyAtmosphereNode>
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin DepthPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_SRV);
        public TtSkyAtmosphereNode()
        {
            Name = "SkyAtmosphere";
        }
        public override void Dispose()
        {
            SceneNode = null;
            CoreSDK.DisposeObject(ref SkyCbv);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinIn);
            // Depth 允许悬空: 不设 IsAllowInputNull 会让 BuildGraph 报 hasInputError
            // -> CreateRenderPolicy 返回 null. 悬空时 OnDrawCall 已做空绑定保护.
            DepthPinIn.IsAllowInputNull = true;
            AddInput(DepthPinIn);
            base.InitNodePins();
        }
        public TtSkyAtmosphereShading mBasePassShading;
        public override Graphics.Pipeline.Shader.TtGraphicsShadingEnv GetPassShading(Graphics.Mesh.TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(Graphics.Pipeline.TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtSkyAtmosphereShading>();
        }
        public override void Tick(TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (SceneNode == null)
            {
                MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }
            if (SkyCbv != null)
            {
                SkyCbv.SetValue("Sky", in SceneNode.SkyStruct);
            }
            base.Tick(world, policy, frameCmdList, bClear);
        }
        #region Rhi Resouces
        public NxRHI.TtCbView SkyCbv;
        public TtSkyAtmosphereSceneNode SceneNode;
        #endregion
    }

    [Bricks.CodeBuilder.ContextMenu("SkyAtmosphere", "Graphics\\SkyAtmosphere", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSkyAtmosphereSceneNode.TtThisNodeData), DefaultNamePrefix = "SkyAtmosphere")]
    public class TtSkyAtmosphereSceneNode : GamePlay.Scene.TtVisual
    {
        public class TtThisNodeData : TtNodeData
        {
            public TtThisNodeData()
            {
                mSkyStruct.SetDefault();
            }
            internal FSkyAtmosphereStruct mSkyStruct = new FSkyAtmosphereStruct();
            [Rtti.Meta("")]
            public FSkyAtmosphereStruct SkyStruct
            {
                get => mSkyStruct;
                set => mSkyStruct = value;
            }
        }

        // 与 HLSL SkyAtmosphere.cginc 的 FSkyAtmosphereStruct 逐字段严格对齐(Pack=16)。引擎单位 = 米。
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FSkyAtmosphereStruct
        {
            public void SetDefault()
            {
                SunDirection = Vector3.Down;
                SunIntensity = 20.0f;

                SunColor = new Vector3(1.0f, 1.0f, 1.0f);
                PlanetRadius = 6360000.0f;    // 米

                GroundAlbedo = new Vector3(0.3f, 0.3f, 0.3f);
                AtmosphereHeight = 100000.0f; // 米 (100km)

                MieAnisotropy = 0.8f;
                SunDiskSize = 0.9998f;        // cos 阈值
                RayleighScale = 1.0f;
                MieScale = 1.0f;

                OzoneScale = 1.0f;
                Exposure = 10.0f;
                SampleCount = 32;
                LightSampleCount = 8;
            }
            public Vector3 SunDirection;
            public float SunIntensity;

            public Vector3 SunColor;
            public float PlanetRadius;

            public Vector3 GroundAlbedo;
            public float AtmosphereHeight;

            public float MieAnisotropy;
            public float SunDiskSize;
            public float RayleighScale;
            public float MieScale;

            public float OzoneScale;
            public float Exposure;
            public int SampleCount;
            public int LightSampleCount;
        }
        public ref FSkyAtmosphereStruct SkyStruct => ref GetNodeData<TtThisNodeData>().mSkyStruct;

        [EGui.Controls.PropertyGrid.TtColor3PickerEditor]
        [System.ComponentModel.Category("Sun")]
        public Vector3 SunColor
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunColor;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunColor = value;
        }
        [System.ComponentModel.Category("Sun")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.1f)]
        public float SunIntensity
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunIntensity;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunIntensity = value;
        }
        [System.ComponentModel.Category("Sun")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float SunDiskSize
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunDiskSize;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunDiskSize = value;
        }
        [EGui.Controls.PropertyGrid.TtColor3PickerEditor]
        [System.ComponentModel.Category("Ground")]
        public Vector3 GroundAlbedo
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.GroundAlbedo;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.GroundAlbedo = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        public float PlanetRadius
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.PlanetRadius;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.PlanetRadius = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        public float AtmosphereHeight
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.AtmosphereHeight;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.AtmosphereHeight = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float MieAnisotropy
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.MieAnisotropy;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.MieAnisotropy = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float RayleighScale
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.RayleighScale;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.RayleighScale = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float MieScale
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.MieScale;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.MieScale = value;
        }
        [System.ComponentModel.Category("Atmosphere")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float OzoneScale
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.OzoneScale;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.OzoneScale = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.1f)]
        public float Exposure
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.Exposure;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.Exposure = value;
        }
        [System.ComponentModel.Category("Option")]
        public int SampleCount
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SampleCount;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SampleCount = value;
        }
        [System.ComponentModel.Category("Option")]
        public int LightSampleCount
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.LightSampleCount;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.LightSampleCount = value;
        }

        public TtSkyAtmosphereNode RenderNode = null;
        public override void Dispose()
        {
            if (RenderNode != null)
            {
                RenderNode.SceneNode = null;
            }
            base.Dispose();
        }
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            this.IsNoTick = false;
            this.IsParallelTick = false;
            return ret;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (RenderNode == null)
            {
                RenderNode = this.GetWorld().ViewportSlate.RenderPolicy?.FindFirstNode<TtSkyAtmosphereNode>();
                if (RenderNode != null)
                {
                    RenderNode.SceneNode = this;
                }
            }
            else
            {
                var sun = GetWorld().GetSun();
                if (sun != null)
                {
                    var dirLight = sun.DirectionLight;
                    this.SunDirection = dirLight.Direction;
                    this.SunColorInternal = dirLight.SunLightColor;
                    this.SunIntensityInternal = dirLight.SunLightIntensity;
                }
            }
            return base.OnTickLogic(args);
        }
        [Browsable(false)]
        public Vector3 SunDirection
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunDirection;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunDirection = value;
        }
        [Browsable(false)]
        public Vector3 SunColorInternal
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunColor;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunColor = value;
        }
        [Browsable(false)]
        public float SunIntensityInternal
        {
            get => GetNodeData<TtThisNodeData>().mSkyStruct.SunIntensity;
            set => GetNodeData<TtThisNodeData>().mSkyStruct.SunIntensity = value;
        }
    }
}
