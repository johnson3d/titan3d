using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class USceenSpaceNode : URenderGraphNode
    {
        public Common.URenderGraphPin ResultPinOut = Common.URenderGraphPin.CreateOutput("Result", true, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public USceenSpaceNode()
        {
            Name = "USceenSpaceNode";
        }
        public override void InitNodePins()
        {
            AddOutput(ResultPinOut, EGpuBufferViewType.GBVT_Rtv | EGpuBufferViewType.GBVT_Srv);
        }
        public Graphics.Mesh.UMesh ScreenMesh;
        public Shader.CommanShading.UBasePassPolicy ScreenDrawPolicy;
        public UGraphicsBuffers GBuffers { get; protected set; } = new UGraphicsBuffers();
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RHI.CRenderPass RenderPass;
        public unsafe override void Cleanup()
        {
            GBuffers?.Cleanup();
            GBuffers = null;

            base.Cleanup();
        }
        public string DebugName;
        public override async System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            ScreenDrawPolicy = new Shader.CommanShading.UBasePassPolicy();
            await ScreenDrawPolicy.Initialize(null);
            //ScreenDrawPolicy.mBasePassShading = shading;
            ScreenDrawPolicy.TagObject = policy;

            var PassDesc = new IRenderPassDesc();
            unsafe
            {
                PassDesc.NumOfMRT = 1;
                PassDesc.AttachmentMRTs[0].Format = ResultPinOut.Attachement.Format;
                PassDesc.AttachmentMRTs[0].Samples = 1;
                PassDesc.AttachmentMRTs[0].LoadAction = FrameBufferLoadAction.LoadActionClear;
                PassDesc.AttachmentMRTs[0].StoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.m_AttachmentDepthStencil.Format = dsFmt;
                //PassDesc.m_AttachmentDepthStencil.Samples = 1;
                //PassDesc.m_AttachmentDepthStencil.LoadAction = FrameBufferLoadAction.LoadActionClear;
                //PassDesc.m_AttachmentDepthStencil.StoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.m_AttachmentDepthStencil.StencilLoadAction = FrameBufferLoadAction.LoadActionClear;
                //PassDesc.m_AttachmentDepthStencil.StencilStoreAction = FrameBufferStoreAction.StoreActionStore;
                //PassDesc.mFBClearColorRT0 = new Color4(0, 0, 0, 0);
                //PassDesc.mDepthClearValue = 1.0f;
                //PassDesc.mStencilClearValue = 0u;
            }

            RenderPass = UEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<IRenderPassDesc>(rc, in PassDesc); 

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, ResultPinOut);
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
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            if (GBuffers != null && RenderPass != null)
                GBuffers.OnResize(x, y);
        }
        public unsafe void ClearGBuffer(URenderPolicy policy)
        {
            var cmdlist = BasePass.DrawCmdList;
            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                if (cmdlist.BeginRenderPass(policy, GBuffers, in passClears, "ClearScreen"))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, URenderPolicy policy, bool bClear)
        {
            var cmdlist = BasePass.DrawCmdList;
            if (ScreenMesh != null)
            {
                cmdlist.ClearMeshDrawPassArray();
                cmdlist.SetViewport(GBuffers.ViewPort.mCoreObject);
                for (int j = 0; j < ScreenMesh.Atoms.Length; j++)
                {
                    var drawcall = ScreenMesh.GetDrawCall(GBuffers, j, ScreenDrawPolicy, Graphics.Pipeline.URenderPolicy.EShadingType.BasePass, this);
                    if (drawcall == null)
                        continue;
                    if (drawcall.Effect.ShaderIndexer.cbPerViewport != 0xFFFFFFFF)
                    {
                        drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.ShaderIndexer.cbPerViewport, GBuffers.PerViewportCBuffer.mCoreObject);
                    }
                    if (drawcall.Effect.ShaderIndexer.cbPerCamera != 0xFFFFFFFF)
                    {
                        drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.ShaderIndexer.cbPerCamera, policy.DefaultCamera.PerCameraCBuffer.mCoreObject);
                    }
                    cmdlist.PushDrawCall(drawcall.mCoreObject);
                }
            }
            if(cmdlist.BeginCommand())
            {
                var passClears = new IRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4(0, 0, 0, 0));
                if (cmdlist.BeginRenderPass(policy, GBuffers, in passClears, DebugName))
                {
                    cmdlist.BuildRenderPass(0);
                    cmdlist.EndRenderPass();
                }
                cmdlist.EndCommand();
            }
        }
        public override unsafe void TickRender(URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);
        }
        public override unsafe void TickSync(URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
