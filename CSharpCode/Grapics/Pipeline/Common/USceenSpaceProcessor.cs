using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class USceenSpaceProcessor
    {
        public Graphics.Mesh.UMesh ScreenMesh;
        private Shader.CommanShading.UBasePassPolicy ScreenDrawPolicy;
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();
        public unsafe void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        public virtual async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, float x, float y)
        {
            ScreenDrawPolicy = new Shader.CommanShading.UBasePassPolicy();
            await ScreenDrawPolicy.Initialize(x, y);
            ScreenDrawPolicy.mBasePassShading = shading;

            GBuffers.Initialize(1, EPixelFormat.PXF_UNKNOWN, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, EPixelFormat.PXF_R16G16_FLOAT, (uint)x, (uint)y);
            GBuffers.SwapChainIndex = -1;
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc);

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionDontCare;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionDontCare;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionDontCare;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = UEngine.Instance.GfxDevice.MaterialManager.NullMaterial;
            if (materials[0] == null)
                return;

            var mesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.CMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.5F, false);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                ScreenMesh = mesh;
            }
        }
        public void OnResize(float x, float y)
        {
            if (GBuffers != null)
                GBuffers.OnResize(x, y);
        }
        public unsafe void ClearGBuffer()
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            cmdlist.BeginCommand();
            cmdlist.BeginRenderPass(ref PassDesc, GBuffers.FrameBuffers.mCoreObject);
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public unsafe void TickLogic()
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            if (ScreenMesh != null)
            {
                cmdlist.ClearMeshDrawPassArray();
                cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
                for (int j = 0; j < ScreenMesh.Atoms.Length; j++)
                {
                    var drawcall = ScreenMesh.GetDrawCall(GBuffers, j, ScreenDrawPolicy, Graphics.Pipeline.IRenderPolicy.EShadingType.BasePass);
                    if (drawcall == null)
                        continue;
                    if (drawcall.Effect.CBPerViewportIndex != 0xFFFFFFFF)
                    {
                        var gpuProgram = drawcall.Effect.ShaderProgram;
                        if (GBuffers.PerViewportCBuffer == null)
                        {
                            GBuffers.PerViewportCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, drawcall.Effect.CBPerViewportIndex);
                            GBuffers.PerViewportCBuffer.mCoreObject.NativeSuper.SetDebugName("USceenSpaceProcessor Viewport");
                            GBuffers.UpdateViewportCBuffer();
                        }
                        drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerViewportIndex, GBuffers.PerViewportCBuffer.mCoreObject);
                    }
                    if (drawcall.Effect.CBPerCameraIndex != 0xFFFFFFFF)
                    {
                        var gpuProgram = drawcall.Effect.ShaderProgram;
                        if (GBuffers.Camera.PerCameraCBuffer == null)
                        {
                            GBuffers.Camera.PerCameraCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, drawcall.Effect.CBPerCameraIndex);
                            GBuffers.Camera.PerCameraCBuffer.mCoreObject.NativeSuper.SetDebugName("USceenSpaceProcessor Camera");
                        }
                        drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerCameraIndex, GBuffers.Camera.PerCameraCBuffer.mCoreObject);
                    }
                    cmdlist.PushDrawCall(drawcall.mCoreObject);
                }
            }
            cmdlist.BeginCommand();
            cmdlist.BeginRenderPass(ref PassDesc, GBuffers.FrameBuffers.mCoreObject);
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();
        }
        public unsafe void TickRender()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public unsafe void TickSync()
        {
            BasePass.SwapBuffer();
        }
    }
}
