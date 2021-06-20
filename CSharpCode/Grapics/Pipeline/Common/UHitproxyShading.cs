using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UHitproxyShading : Shader.UShadingEnv
    {
        public UHitproxyShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/HitProxy.cginc", RName.ERNameType.Engine);
        }
    }
    public class UPickSetupShading : Shader.UShadingEnv
    {
        public UPickSetupShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_setup.cginc", RName.ERNameType.Engine);
        }
    }
    public class UPickBlurShading : Shader.UShadingEnv
    {
        public UPickBlurShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_blur.cginc", RName.ERNameType.Engine);
        }
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
            //var cbIndex = drawcall.mCoreObject.FindCBufferIndex("cbPerShadingEnv");
            //if (cbIndex != 0xFFFFFFFF)
            //{
            //    if (PerShadingCBuffer == null)
            //    {
            //        PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(gpuProgram, cbIndex);
            //        PerShadingCBuffer.SetMatrix(0, ref Matrix.mIdentity);
            //        var RenderColor = new Color4(1, 1, 1, 1);
            //        PerShadingCBuffer.SetValue(1, ref RenderColor);
            //    }
            //    drawcall.mCoreObject.BindCBufferAll(cbIndex, PerShadingCBuffer.mCoreObject.Ptr);
            //}
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            UPickedProxiableManager Manager = policy.TagObject as UPickedProxiableManager;

            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.FindSRVIndex("SourceTexture");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickedBuffer.GBufferSRV[0].mCoreObject);
        }
    }
    public class UPickBlurProcessor : USceenSpaceProcessor
    {
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y);
        }
    }
    public class UPickHollowShading : Shader.UShadingEnv
    {
        public UPickHollowShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Sys/pick/pick_hollow.cginc", RName.ERNameType.Engine);
        }
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
        }
        public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, IRenderPolicy policy, Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);

            UPickedProxiableManager Manager = policy.TagObject as UPickedProxiableManager;
            var gpuProgram = drawcall.Effect.ShaderProgram;
            var index = drawcall.mCoreObject.FindSRVIndex("gPickedSetUpTex");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickedBuffer.GBufferSRV[0].mCoreObject);

            index = drawcall.mCoreObject.FindSRVIndex("gPickedBlurTex");
            drawcall.mCoreObject.BindSRVAll(index, Manager.PickBlurProcessor.GBuffers.GBufferSRV[0].mCoreObject);
        }
    }
    public class UPickHollowProcessor : USceenSpaceProcessor
    {
        public override async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat rtFmt, EPixelFormat dsFmt, float x, float y)
        {
            await base.Initialize(policy, shading, rtFmt, dsFmt, x, y);
        }
    }
    public class UPickedProxiableManager
    {
        public unsafe void Cleanup()
        {
            PickedBuffer?.Cleanup();
            PickedBuffer = null;

            PickBlurProcessor?.Cleanup();
            PickBlurProcessor = null;

            PickHollowProcessor?.Cleanup();
            PickHollowProcessor = null;
        }
        public List<IProxiable> PickedProxies = new List<IProxiable>();
        public void Selected(IProxiable obj)
        {
            if (PickedProxies.Contains(obj))
                return;
            PickedProxies.Add(obj);
        }
        public void Unselected(IProxiable obj)
        {
            PickedProxies.Remove(obj);
        }
        public void ClearSelected()
        {
            PickedProxies.Clear();
        }
        public UGraphicsBuffers PickedBuffer { get; protected set; } = new UGraphicsBuffers();
        public UPickSetupShading PickedShading = null;
        public UDrawBuffers BasePass = new UDrawBuffers();
        public RenderPassDesc PassDesc = new RenderPassDesc();

        public UPickBlurProcessor PickBlurProcessor = new UPickBlurProcessor();
        public UPickHollowProcessor PickHollowProcessor = new UPickHollowProcessor();
        public async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, float x, float y)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            BasePass.Initialize(rc);
            BasePass.SetDebugName("UPickedProxiableManager");

            PickedBuffer.SwapChainIndex = -1;
            PickedBuffer.Initialize(1, EPixelFormat.PXF_D24_UNORM_S8_UINT, (uint)x, (uint)y);
            PickedBuffer.CreateGBuffer(0, EPixelFormat.PXF_R16G16_FLOAT, (uint)x, (uint)y);            
            PickedBuffer.TargetViewIdentifier = policy.GBuffers.TargetViewIdentifier;
            PickedBuffer.Camera = policy.GBuffers.Camera;

            PassDesc.mFBLoadAction_Color = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Color = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mFBClearColorRT0 = new Color4(1, 0, 0, 0);
            PassDesc.mFBLoadAction_Depth = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Depth = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mDepthClearValue = 1.0f;
            PassDesc.mFBLoadAction_Stencil = FrameBufferLoadAction.LoadActionClear;
            PassDesc.mFBStoreAction_Stencil = FrameBufferStoreAction.StoreActionStore;
            PassDesc.mStencilClearValue = 0u;

            PickedShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickSetupShading>();

            await PickBlurProcessor.Initialize(policy, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y);

            await PickHollowProcessor.Initialize(policy, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y);
        }
        public void OnResize(float x, float y)
        {
            if (PickedBuffer != null)
                PickedBuffer.OnResize(x, y);

            if (PickBlurProcessor != null)
                PickBlurProcessor.OnResize(x, y);

            if (PickHollowProcessor != null)
                PickHollowProcessor.OnResize(x, y);
        }
        List<Mesh.UMesh> mPickedMeshes = new List<Mesh.UMesh>();
        public unsafe void TickLogic(IRenderPolicy policy)
        {
            var cmdlist = BasePass.DrawCmdList.mCoreObject;
            cmdlist.ClearMeshDrawPassArray();
            cmdlist.SetViewport(PickedBuffer.ViewPort.mCoreObject);

            mPickedMeshes.Clear();
            foreach (var i in PickedProxies)
            {
                i.GetDrawMesh(mPickedMeshes);
            }
            foreach (var mesh in mPickedMeshes)
            {
                if (mesh.Atoms == null)
                    continue;

                for (int j = 0; j < mesh.Atoms.Length; j++)
                {
                    var drawcall = mesh.GetDrawCall(PickedBuffer, j, policy, Graphics.Pipeline.IRenderPolicy.EShadingType.Picked);
                    if (drawcall != null)
                    {
                        PickedBuffer.SureCBuffer(drawcall.Effect, "UPickedProxiableManager");
                        if (PickedBuffer.PerViewportCBuffer != null)
                            drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerViewportIndex, PickedBuffer.PerViewportCBuffer.mCoreObject);
                        if (PickedBuffer.Camera.PerCameraCBuffer != null)
                            drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerCameraIndex, PickedBuffer.Camera.PerCameraCBuffer.mCoreObject);

                        cmdlist.PushDrawCall(drawcall.mCoreObject);
                    }
                }   
            }

            cmdlist.BeginCommand();
            cmdlist.BeginRenderPass(ref PassDesc, PickedBuffer.FrameBuffers.mCoreObject);
            cmdlist.BuildRenderPass(0);
            cmdlist.EndRenderPass();
            cmdlist.EndCommand();

            PickBlurProcessor.TickLogic();
            PickHollowProcessor.TickLogic();
        }
        public unsafe void TickRender(IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = BasePass.CommitCmdList.mCoreObject;
            cmdlist.Commit(rc.mCoreObject);

            PickBlurProcessor.TickRender();
            PickHollowProcessor.TickRender();
        }
        public unsafe void TickSync(IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
            PickBlurProcessor.TickSync();
            PickHollowProcessor.TickSync();
        }
    }
}
