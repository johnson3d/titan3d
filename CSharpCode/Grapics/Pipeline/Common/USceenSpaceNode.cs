using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class USceenSpaceNode : URenderGraphNode
    {
        public Graphics.Mesh.UMesh ScreenMesh;
        public Shader.CommanShading.UBasePassPolicy ScreenDrawPolicy;
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public unsafe void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;
        }
        public string DebugName;
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            ScreenDrawPolicy = new Shader.CommanShading.UBasePassPolicy();
            await ScreenDrawPolicy.Initialize(null, x, y);
            ScreenDrawPolicy.mBasePassShading = shading;
            ScreenDrawPolicy.TagObject = policy;

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = rtFmt;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                PassDesc.m_AttachmentDepthStencil.Samples = 1;
                PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }

            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc); 

            GBuffers.Initialize(RenderPass, null, 1, dsFmt, (uint)x, (uint)y);
            GBuffers.CreateGBuffer(0, rtFmt, (uint)x, (uint)y);
            GBuffers.UpdateFrameBuffers(x, y);
            GBuffers.TargetViewIdentifier = new UGraphicsBuffers.UTargetViewIdentifier();

            BasePass.Initialize(rc, debugName);
            DebugName = debugName;

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = UEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial;
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
        public override void OnResize(IRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null && RenderPass != null)
                GBuffers.OnResize(x, y);
        }
        public unsafe void ClearGBuffer()
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                if (cmdlist.BeginRenderPass(GBuffers.FrameBuffers.mCoreObject, in passClears, "ClearScreen"))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            if (ScreenMesh != null)
            {
                cmdlist.ClearMeshDrawPassArray();
                cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
                for (int j = 0; j < ScreenMesh.Atoms.Length; j++)
                {
                    var drawcall = ScreenMesh.GetDrawCall(GBuffers, j, ScreenDrawPolicy, Graphics.Pipeline.IRenderPolicy.EShadingType.BasePass, this);
                    if (drawcall == null)
                        continue;
                    if (drawcall.Effect.CBPerViewportIndex != 0xFFFFFFFF)
                    {
                        drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerViewportIndex, GBuffers.PerViewportCBuffer.mCoreObject);
                    }
                    if (GBuffers.Camera != null && drawcall.Effect.CBPerCameraIndex != 0xFFFFFFFF)
                    {
                        drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerCameraIndex, GBuffers.Camera.PerCameraCBuffer.mCoreObject);
                    }
                    cmdlist.PushDrawCall(drawcall.mCoreObject);
                }
            }
            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                if (cmdlist.BeginRenderPass(GBuffers.FrameBuffers.mCoreObject, in passClears, DebugName))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public override unsafe void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public override unsafe void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
