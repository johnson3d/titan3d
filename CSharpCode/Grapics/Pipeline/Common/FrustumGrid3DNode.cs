using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("FrustumGrid3D", "FrustumGrid3D", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtFrustumGrid3DNode : TAuxRenderGraphNode<TtFrustumGrid3DNode>
    {
        public TtRenderGraphPin DepthPinIn = TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PointLightsPinIn = TtRenderGraphPin.CreateInputOutput("PointLights", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin SpotLightsPinIn = TtRenderGraphPin.CreateInputOutput("SpotLights", NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin PointGridPinOut = TtRenderGraphPin.CreateOutput("PointGrid", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PointDataIndexPinOut = TtRenderGraphPin.CreateOutput("PointGridDataIndex", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin SpotGridPinOut = TtRenderGraphPin.CreateOutput("SpotGrid", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin SpotDataIndexPinOut = TtRenderGraphPin.CreateOutput("SpotGridDataIndex", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);

        public TtFrustumGrid3DNode()
        {
            Name = "FrustumGrid3DNode";
        }

        public override void InitNodePins()
        {
            PointGridPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            PointDataIndexPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            SpotGridPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            SpotDataIndexPinOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(PointGridPinOut);
            AddOutput(PointDataIndexPinOut);
            AddOutput(SpotGridPinOut);
            AddOutput(SpotDataIndexPinOut);
            AddInput(DepthPinIn);
            AddInputOutput(PointLightsPinIn);
            AddInputOutput(SpotLightsPinIn);
            //SpotLightsPinIn.IsAllowInputNull = true;
        }

        #region Grid Data
        public TtFrustumGrid3D FrustumGrid { get; private set; } = new TtFrustumGrid3D();
        public const uint MaxPerCellPoint = 32;
        public const uint MaxPerCellSpot = 8;

        private TtGpuBuffer<uint> PointDataIndexBuffer;
        private TtAttachBuffer PointGridAttachment = new TtAttachBuffer();
        private TtAttachBuffer PointDataIndexAttachment = new TtAttachBuffer();

        private TtGpuBuffer<uint> SpotDataIndexBuffer;
        private TtAttachBuffer SpotGridAttachment = new TtAttachBuffer();
        private TtAttachBuffer SpotDataIndexAttachment = new TtAttachBuffer();

        // SpotGrid reuses FrustumGrid's grid dimensions (same cell layout)
        public TtFrustumGrid3D SpotGrid { get; private set; } = new TtFrustumGrid3D();

        public NxRHI.TtCbView PerFrustumGridCbv { get; private set; }
        #endregion

        #region FrameBuild
        public unsafe override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            // PointLight grid
            PointGridPinOut.Attachement.Height = (uint)FrustumGrid.TotalCellCount;
            PointGridPinOut.Attachement.Width = (uint)sizeof(TtFrustumGrid3D.FCellHeader);
            var gridAttach = RenderGraph.AttachmentCache.ImportAttachment(PointGridPinOut, PointGridAttachment);
            var gridBuffer = FrustumGrid.GetGridBuffer();
            if (gridBuffer != null)
            {
                gridAttach.GpuResource = gridBuffer.GpuResource;
                gridAttach.Srv = gridBuffer.Srv;
                gridAttach.Uav = gridBuffer.Uav;
            }

            PointDataIndexPinOut.Attachement.Height = (uint)FrustumGrid.TotalCellCount * MaxPerCellPoint;
            PointDataIndexPinOut.Attachement.Width = sizeof(uint);
            var dataAttach = RenderGraph.AttachmentCache.ImportAttachment(PointDataIndexPinOut, PointDataIndexAttachment);
            if (PointDataIndexBuffer != null)
            {
                dataAttach.GpuResource = PointDataIndexBuffer.GpuResource;
                dataAttach.Srv = PointDataIndexBuffer.Srv;
                dataAttach.Uav = PointDataIndexBuffer.Uav;
            }

            // SpotLight grid
            SpotGridPinOut.Attachement.Height = (uint)SpotGrid.TotalCellCount;
            SpotGridPinOut.Attachement.Width = (uint)sizeof(TtFrustumGrid3D.FCellHeader);
            var spotGridAttach = RenderGraph.AttachmentCache.ImportAttachment(SpotGridPinOut, SpotGridAttachment);
            var spotGridBuffer = SpotGrid.GetGridBuffer();
            if (spotGridBuffer != null)
            {
                spotGridAttach.GpuResource = spotGridBuffer.GpuResource;
                spotGridAttach.Srv = spotGridBuffer.Srv;
                spotGridAttach.Uav = spotGridBuffer.Uav;
            }

            SpotDataIndexPinOut.Attachement.Height = (uint)SpotGrid.TotalCellCount * MaxPerCellSpot;
            SpotDataIndexPinOut.Attachement.Width = sizeof(uint);
            var spotDataAttach = RenderGraph.AttachmentCache.ImportAttachment(SpotDataIndexPinOut, SpotDataIndexAttachment);
            if (SpotDataIndexBuffer != null)
            {
                spotDataAttach.GpuResource = SpotDataIndexBuffer.GpuResource;
                spotDataAttach.Srv = SpotDataIndexBuffer.Srv;
                spotDataAttach.Uav = SpotDataIndexBuffer.Uav;
            }
        }
        #endregion

        #region Shading Environments
        public class TtClearGridShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);
            public TtClearGridShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/FrustumGrid3D.compute", RName.ERNameType.Engine);
                MainName = "CS_ClearGrid";
                this.UpdatePermutation().AddWaitTask();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtFrustumGrid3DNode;

                var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbFrustumGrid");
                if (binder.IsValidPointer && node.PerFrustumGridCbv != null)
                    drawcall.BindCBV(binder, node.PerFrustumGridCbv);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWPointGridHeaders");
                if (binder.IsValidPointer && node.FrustumGrid.Uav != null)
                    drawcall.BindUav(binder, node.FrustumGrid.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWPointGridDataIndices");
                if (binder.IsValidPointer && node.PointDataIndexBuffer?.Uav != null)
                    drawcall.BindUav(binder, node.PointDataIndexBuffer.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWSpotGridHeaders");
                if (binder.IsValidPointer && node.SpotGrid.Uav != null)
                    drawcall.BindUav(binder, node.SpotGrid.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWSpotGridDataIndices");
                if (binder.IsValidPointer && node.SpotDataIndexBuffer?.Uav != null)
                    drawcall.BindUav(binder, node.SpotDataIndexBuffer.Uav);
            }
        }

        public class TtInjectLightsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg => new Vector3ui(64, 1, 1);
            public TtInjectLightsShading()
            {
                CodeName = RName.GetRName("Shaders/Compute/ScreenSpace/FrustumGrid3D.compute", RName.ERNameType.Engine);
                MainName = "CS_InjectLights";
                this.UpdatePermutation().AddWaitTask();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
            }
            public override void OnDrawCall(NxRHI.TtComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as TtFrustumGrid3DNode;

                var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbFrustumGrid");
                if (binder.IsValidPointer && node.PerFrustumGridCbv != null)
                    drawcall.BindCBV(binder, node.PerFrustumGridCbv);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbPerCamera");
                if (binder.IsValidPointer)
                    drawcall.BindCBV(binder, policy.DefaultCamera.PerCameraCBuffer);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWPointGridHeaders");
                if (binder.IsValidPointer && node.FrustumGrid.Uav != null)
                    drawcall.BindUav(binder, node.FrustumGrid.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWPointGridDataIndices");
                if (binder.IsValidPointer && node.PointDataIndexBuffer?.Uav != null)
                    drawcall.BindUav(binder, node.PointDataIndexBuffer.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWSpotGridHeaders");
                if (binder.IsValidPointer && node.SpotGrid.Uav != null)
                    drawcall.BindUav(binder, node.SpotGrid.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "RWSpotGridDataIndices");
                if (binder.IsValidPointer && node.SpotDataIndexBuffer?.Uav != null)
                    drawcall.BindUav(binder, node.SpotDataIndexBuffer.Uav);

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GpuScene_PointLights");
                if (binder.IsValidPointer)
                {
                    var attachBuffer = node.GetAttachBuffer(node.PointLightsPinIn);
                    if (attachBuffer?.Srv != null)
                        drawcall.BindSrv(binder, attachBuffer.Srv);
                }

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GpuScene_SpotLights");
                if (binder.IsValidPointer)
                {
                    var attachBuffer = node.GetAttachBuffer(node.SpotLightsPinIn);
                    if (attachBuffer?.Srv != null)
                        drawcall.BindSrv(binder, attachBuffer.Srv);
                }

                binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "DepthBuffer");
                if (binder.IsValidPointer)
                {
                    var depth = node.GetAttachBuffer(node.DepthPinIn);
                    if (depth?.Srv != null)
                        drawcall.BindSrv(binder, depth.Srv);
                }
            }
        }

        private TtClearGridShading ClearGridShadingEnv;
        private NxRHI.TtComputeDraw ClearGridDrawcall;
        private TtInjectLightsShading InjectLightsShadingEnv;
        private NxRHI.TtComputeDraw InjectLightsDrawcall;
        #endregion

        #region Lifecycle
        public async override Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            ClearGridShadingEnv = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtClearGridShading>();
            InjectLightsShadingEnv = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtInjectLightsShading>();
        }

        private void EnsureCbvCreated(NxRHI.TtComputeDraw drawcall)
        {
            if (PerFrustumGridCbv != null)
                return;
            var binder = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBV, "cbFrustumGrid");
            if (binder.IsValidPointer)
            {
                PerFrustumGridCbv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }
        }

        private void ResetComputeDrawcalls()
        {
            if (ClearGridShadingEnv == null)
                return;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CoreSDK.DisposeObject(ref ClearGridDrawcall);
            ClearGridDrawcall = rc.CreateComputeDraw();

            CoreSDK.DisposeObject(ref InjectLightsDrawcall);
            InjectLightsDrawcall = rc.CreateComputeDraw();
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref PointGridAttachment);
            CoreSDK.DisposeObject(ref PointDataIndexAttachment);
            CoreSDK.DisposeObject(ref SpotGridAttachment);
            CoreSDK.DisposeObject(ref SpotDataIndexAttachment);
            CoreSDK.DisposeObject(ref ClearGridDrawcall);
            CoreSDK.DisposeObject(ref InjectLightsDrawcall);

            var cbv = PerFrustumGridCbv;
            CoreSDK.DisposeObject(ref cbv);
            PerFrustumGridCbv = null;

            PointDataIndexBuffer?.Dispose();
            PointDataIndexBuffer = null;

            SpotDataIndexBuffer?.Dispose();
            SpotDataIndexBuffer = null;

            FrustumGrid?.Dispose();
            SpotGrid?.Dispose();

            base.Dispose();
        }

        public override unsafe void OnResize(TtRenderPolicy policy, float x, float y)
        {
            var camera = policy.DefaultCamera;
            FrustumGrid.OnResize(x, y, camera.ZNear, camera.ZFar);
            SpotGrid.OnResize(x, y, camera.ZNear, camera.ZFar);

            PointDataIndexBuffer?.Dispose();
            uint pointTotalSlots = (uint)FrustumGrid.TotalCellCount * MaxPerCellPoint;
            if (pointTotalSlots > 0)
            {
                PointDataIndexBuffer = new TtGpuBuffer<uint>();
                PointDataIndexBuffer.SetSize(pointTotalSlots, null,
                    NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            }

            SpotDataIndexBuffer?.Dispose();
            uint spotTotalSlots = (uint)SpotGrid.TotalCellCount * MaxPerCellSpot;
            if (spotTotalSlots > 0)
            {
                SpotDataIndexBuffer = new TtGpuBuffer<uint>();
                SpotDataIndexBuffer.SetSize(spotTotalSlots, null,
                    NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            }

            ResetComputeDrawcalls();
        }
        #endregion

        #region Tick
        public override unsafe void Tick(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (FrustumGrid.TotalCellCount <= 0)
                return;

            var gpuScene = policy.GetGpuSceneNode();

            var cmd = NxRHI.TtCommandList.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmd, "FrustumGrid3D"))
            {
                uint totalCells = (uint)FrustumGrid.TotalCellCount;

                // Pass 1: Clear both grids (Point + Spot) in one dispatch
                ClearGridShadingEnv.SetDrawcallDispatch(this, policy, ClearGridDrawcall, totalCells, 1, 1, true);
                EnsureCbvCreated(ClearGridDrawcall);
                if (PerFrustumGridCbv != null)
                    UpdateCBufferFields(gpuScene);
                cmd.PushGpuDraw(ClearGridDrawcall);

                // Pass 2: Inject all lights (Point + Spot) in one dispatch
                if (policy.EnableLocalLights)
                {
                    var pointCount = gpuScene?.PointLights.DataArray.Count ?? 0;
                    var spotCount = gpuScene?.SpotLights.DataArray.Count ?? 0;
                    if (pointCount > 0 || spotCount > 0)
                    {
                        InjectLightsShadingEnv.SetDrawcallDispatch(this, policy, InjectLightsDrawcall, totalCells, 1, 1, true);
                        cmd.PushGpuDraw(InjectLightsDrawcall);
                    }
                }

                cmd.FlushDraws();
            }

            policy.CommitCommandList(cmd, "FrustumGrid3D");
        }

        private void UpdateCBufferFields(TtGpuSceneNode gpuScene)
        {
            var cbv = PerFrustumGridCbv;

            var idx = cbv.ShaderBinder.FindField("FrustumGridSize");
            if (idx.IsValidPointer)
            {
                Vector3i gridSize = new Vector3i(FrustumGrid.CulledGridSizeX, FrustumGrid.CulledGridSizeY, FrustumGrid.GridSizeZ);
                cbv.SetValue(idx, in gridSize);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridPixelSizeShift");
            if (idx.IsValidPointer)
            {
                uint shift = (uint)FrustumGrid.CellPixelSizeShift;
                cbv.SetValue(idx, in shift);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridZParams");
            if (idx.IsValidPointer)
            {
                var zParams = FrustumGrid.GridZParams;
                cbv.SetValue(idx, in zParams);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridTotalCells");
            if (idx.IsValidPointer)
            {
                uint totalCells = (uint)FrustumGrid.TotalCellCount;
                cbv.SetValue(idx, in totalCells);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridMaxPerCellPoint");
            if (idx.IsValidPointer)
            {
                uint maxPoint = MaxPerCellPoint;
                cbv.SetValue(idx, in maxPoint);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridMaxPerCellSpot");
            if (idx.IsValidPointer)
            {
                uint maxSpot = MaxPerCellSpot;
                cbv.SetValue(idx, in maxSpot);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridViewportSize");
            if (idx.IsValidPointer)
            {
                var vpSize = new Vector2(FrustumGrid.ViewportWidth, FrustumGrid.ViewportHeight);
                cbv.SetValue(idx, in vpSize);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridPointLightNum");
            if (idx.IsValidPointer)
            {
                uint pointLightNum = (uint)(gpuScene?.PointLights.DataArray.Count ?? 0);
                cbv.SetValue(idx, in pointLightNum);
            }

            idx = cbv.ShaderBinder.FindField("FrustumGridSpotLightNum");
            if (idx.IsValidPointer)
            {
                var spotAttach = GetAttachBuffer(SpotLightsPinIn);
                uint spotLightNum = (spotAttach?.Srv != null) ? (uint)(gpuScene?.SpotLights.DataArray.Count ?? 0) : 0u;
                cbv.SetValue(idx, in spotLightNum);
            }
        }
        #endregion
    }
}
