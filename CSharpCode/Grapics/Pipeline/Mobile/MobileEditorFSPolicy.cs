using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class TtMobileEditorFSPolicy : TtMobileFSPolicy
    {
        public TtMobileEditorFSPolicy()
        {
            RegRenderNode2("BasePassNode", BasePassNode);
            RegRenderNode2("ShadowMapNode", mShadowMapNode);
            RegRenderNode2("HitproxyNode", HitproxyNode);
            RegRenderNode2("PickedNode", PickedNode);
            RegRenderNode2("PickBlurNode", PickBlurNode);
            RegRenderNode2("PickHollowNode", PickHollowNode);
            RegRenderNode2("GpuSceneNode", GpuSceneNode);
            RegRenderNode2("ScreenTilingNode", ScreenTilingNode);
            RegRenderNode2("VoxelsNode", VoxelsNode);
            RegRenderNode2("FinalCopyNode", FinalCopyNode);
            RegRenderNode2("HzbNode", HzbNode);
            RegRenderNode2("TranslucentNode", TranslucentNode);
        }
        public override Common.TtGpuSceneNode GetGpuSceneNode()
        {
            return GpuSceneNode;
        }
        public override NxRHI.TtSrView GetFinalShowRSV()
        {
            return this.AttachmentCache.FindAttachement(FinalCopyNode.GBuffers.RenderTargets[0].Attachement.AttachmentName).Srv;
        }
        public override bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                var finalShading = FinalCopyNode.GetPassShading() as TtFinalCopyShading;
                if (finalShading != null)
                {
                    finalShading.SetDisableAO(value);
                }
            }
        }
        #region GetHitproxy
        public override IProxiable GetHitproxy(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitproxy(MouseX, MouseY);
        }

        public UInt32 GetHitProxyID(UInt32 MouseX, UInt32 MouseY)
        {
            return HitproxyNode.GetHitProxyID(MouseX, MouseY);
        }
        #endregion

        public TtMobileTranslucentNode TranslucentNode = new TtMobileTranslucentNode();

        public TtFinalCopyNode FinalCopyNode = new TtFinalCopyNode();

        public Common.TtHitproxyNode HitproxyNode = new Common.TtHitproxyNode();

        public Common.TtPickedNode PickedNode = new Common.TtPickedNode();
        public Common.TtPickBlurNode PickBlurNode = new Common.TtPickBlurNode();
        public Common.TtPickHollowNode PickHollowNode = new Common.TtPickHollowNode();

        public Common.TtGpuSceneNode GpuSceneNode = new Common.TtGpuSceneNode();

        public Common.TtScreenTilingNode ScreenTilingNode = new Common.TtScreenTilingNode();

        public Common.TtHzbNode HzbNode = new Common.TtHzbNode();

        public Common.TtImageAssetNode EnvMapNode { get; set; } = new Common.TtImageAssetNode();
        public Common.TtImageAssetNode VignetteNode { get; set; } = new Common.TtImageAssetNode();
        //for test
        public Bricks.VXGI.UVoxelsNode VoxelsNode = new Bricks.VXGI.UVoxelsNode();

        public override async Thread.Async.TtTask Initialize(TtCamera camera)
        {
            await base.Initialize(camera);

            EnvMapNode.ImageName = RName.GetRName("texture/hdri_epic_courtyard_daylight.srv", RName.ERNameType.Engine);
            VignetteNode.ImageName = RName.GetRName("texture/default_vignette.srv", RName.ERNameType.Engine);

            //await BasePassNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Pipeline.Mobile.UBasePassOpaque>(),
            //    EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "BasePass");

            //await TranslucentNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Pipeline.Mobile.UBasePassOpaque>(),
            //    EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "BasePass");

            //await HitproxyNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Common.UHitproxyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Hitproxy");

            //await PickedNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Common.UPickSetupShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "PickedNode");

            //await PickBlurNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickBlur");

            //await PickHollowNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickHollow");

            //await FinalCopyNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<UFinalCopyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "EditorFinal");

            //await mShadowMapNode.Initialize(this, Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<Shadow.UShadowShading>(), EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "ShadowDepth");

            //await VoxelsNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "VoxelsNode");

            //await HzbNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "HzbNode");

            //await ScreenTilingNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "ScreenTilingNode");            

            //await GpuSceneNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "GpuScene");
        }
        protected override void OnBuildGraph()
        {
            //AddLinker(mShadowMapNode.DepthPinOut, FinalCopyNode.);
            //AddLinker(BasePassNode.ColorPinOut, BasePassNode.);

            {
                AddLinker(PickedNode.PickedPinOut, PickBlurNode.PickedPinIn);
            }
            AddLinker(PickedNode.PickedPinOut, PickHollowNode.PickedPinIn);
            AddLinker(PickBlurNode.ResultPinOut, PickHollowNode.BlurPinIn);

            {
                AddLinker(GpuSceneNode.PointLightsPinOut, ScreenTilingNode.PointLightsPinIn);
                AddLinker(BasePassNode.DepthPinOut, ScreenTilingNode.DepthPinIn);
            }

            AddLinker(TranslucentNode.AlbedoPinInOut, FinalCopyNode.ColorPinIn);
            AddLinker(PickHollowNode.ResultPinOut, FinalCopyNode.PickPinIn);
            AddLinker(VignetteNode.ImagePinOut, FinalCopyNode.VignettePinIn);

            System.Diagnostics.Debug.Assert(false);
            //RootNode = FinalCopyNode;
        }
        public override void OnResize(float x, float y)
        {
            BasePassNode.OnResize(this, x, y);

            TranslucentNode?.OnResize(this, x, y);

            ScreenTilingNode.OnResize(this, x, y);

            HzbNode.OnResize(this, x, y);

            HitproxyNode?.OnResize(this, x, y);

            PickedNode?.OnResize(this, x, y);

            PickBlurNode?.OnResize(this, x, y);

            PickHollowNode?.OnResize(this, x, y);

            FinalCopyNode?.OnResize(this, x, y);

            VoxelsNode?.OnResize(this, x, y);

            GpuSceneNode?.OnResize(this, x, y);
        }
        public unsafe override void Dispose()
        {
            mShadowMapNode?.Dispose();
            mShadowMapNode = null;

            ScreenTilingNode?.Dispose();
            ScreenTilingNode = null;

            TranslucentNode?.Dispose();
            TranslucentNode = null;

            PickedNode?.Dispose();
            PickedNode = null;

            PickBlurNode?.Dispose();
            PickBlurNode = null;

            PickHollowNode?.Dispose();
            PickHollowNode = null;

            FinalCopyNode?.Dispose();
            FinalCopyNode = null;

            HitproxyNode?.Dispose();
            HitproxyNode = null;

            VoxelsNode?.Dispose();
            VoxelsNode = null;

            HzbNode?.Dispose();
            HzbNode = null;

            GpuSceneNode?.Dispose();
            GpuSceneNode = null;

            base.Dispose();
        }
        //渲染DrawCall的时候调用，如果产生了对应的ShadingType的Drawcall，则会callback到这里设置一些这个shading的特殊参数
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Mesh.TtRenderMesh.TtAtom atom)
        {
            atom.MdfQueue.OnDrawCall(cmd, drawcall, this, atom);
            //drawcall.Effect.ShadingEnv
            switch (atom.Material.RenderLayer)
            {
                case ERenderLayer.RL_Translucent:
                    TranslucentNode.mTranslucentShading.OnDrawCall(cmd, drawcall, this, atom);
                    return;
                default:
                    BasePassNode.mOpaqueShading.OnDrawCall(cmd, drawcall, this, atom);
                    return;
            }
        }
        public unsafe override void Tick(GamePlay.TtWorld world, Action<TtRenderGraphNode, TtRenderGraphPin, TtAttachBuffer> onRemove)
        {
            base.Tick(world, onRemove);

            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            mShadowMapNode?.Tick(world, this, cmdlist, true);

            GpuSceneNode?.Tick(world, this, cmdlist, true);

            BasePassNode?.Tick(world, this, cmdlist, true);

            ScreenTilingNode?.Tick(world, this, cmdlist, false);

            HzbNode?.Tick(world, this, cmdlist, false);

            VoxelsNode?.Tick(world, this, cmdlist, true);

            TranslucentNode?.Tick(world, this, cmdlist, true);

            HitproxyNode?.Tick(world, this, cmdlist, true);

            PickedNode?.Tick(world, this, cmdlist, true);

            PickBlurNode?.Tick(world, this, cmdlist, true);

            PickHollowNode?.Tick(world, this, cmdlist, true);

            FinalCopyNode?.Tick(world, this, cmdlist, true);

            this.CommitCommandList(cmdlist, "Frame");
        }
        public unsafe override void TickSync()
        {
            mShadowMapNode?.TickSync(this);

            GpuSceneNode?.TickSync(this);

            BasePassNode?.TickSync(this);

            ScreenTilingNode?.TickSync(this);

            HzbNode?.TickSync(this);

            VoxelsNode?.TickSync(this);

            TranslucentNode?.TickSync(this);

            HitproxyNode?.TickSync(this);

            PickedNode?.TickSync(this);

            PickBlurNode?.TickSync(this);

            PickHollowNode?.TickSync(this);

            FinalCopyNode?.TickSync(this);
        }
    }
}
