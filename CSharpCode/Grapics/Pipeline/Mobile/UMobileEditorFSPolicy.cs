using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileEditorFSPolicy : UMobileFSPolicy
    {
        public UMobileEditorFSPolicy()
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
                var finalShading = FinalCopyNode.GetPassShading() as UFinalCopyShading;
                if (finalShading != null)
                {
                    finalShading.SetDisableAO(value);
                }
            }
        }
        public override bool DisableHDR
        {
            get
            {
                return mDisableHDR;
            }
            set
            {
                mDisableHDR = value;
                var shading = FinalCopyNode.GetPassShading() as UFinalCopyShading;
                shading?.SetDisableHDR(value);
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

        public UMobileTranslucentNode TranslucentNode = new UMobileTranslucentNode();
        public UFinalCopyNode FinalCopyNode = new UFinalCopyNode();

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public Common.UHitproxyNode HitproxyNode = new Common.UHitproxyNode();
        public Common.UPickedNode PickedNode = new Common.UPickedNode();
在此之后:
        public Common.TtHitproxyNode HitproxyNode = new Common.UHitproxyNode();
        public Common.UPickedNode PickedNode = new Common.UPickedNode();
*/
        public Common.TtHitproxyNode HitproxyNode = new Common.TtHitproxyNode();
        public Common.UPickedNode PickedNode = new Common.UPickedNode();
        public Common.UPickBlurNode PickBlurNode = new Common.UPickBlurNode();
        public Common.UPickHollowNode PickHollowNode = new Common.UPickHollowNode();


/* 项目“Engine.Android”的未合并的更改
在此之前:
        public Common.UGpuSceneNode GpuSceneNode = new Common.UGpuSceneNode();
        public Common.UScreenTilingNode ScreenTilingNode = new Common.UScreenTilingNode();
在此之后:
        public Common.TtGpuSceneNode GpuSceneNode = new Common.UGpuSceneNode();
        public Common.UScreenTilingNode ScreenTilingNode = new Common.UScreenTilingNode();
*/
        public Common.TtGpuSceneNode GpuSceneNode = new Common.TtGpuSceneNode();

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public Common.UScreenTilingNode ScreenTilingNode = new Common.UScreenTilingNode();
在此之后:
        public Common.TtScreenTilingNode ScreenTilingNode = new Common.UScreenTilingNode();
*/
        public Common.TtScreenTilingNode ScreenTilingNode = new Common.TtScreenTilingNode();

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public Common.UHzbNode HzbNode = new Common.UHzbNode();
        public Common.UImageAssetNode EnvMapNode { get; set; } = new Common.UImageAssetNode();
在此之后:
        public Common.TtHzbNode HzbNode = new Common.UHzbNode();
        public Common.UImageAssetNode EnvMapNode { get; set; } = new Common.UImageAssetNode();
*/
        public Common.TtHzbNode HzbNode = new Common.TtHzbNode();

/* 项目“Engine.Android”的未合并的更改
在此之前:
        public Common.UImageAssetNode EnvMapNode { get; set; } = new Common.UImageAssetNode();
        public Common.UImageAssetNode VignetteNode { get; set; } = new Common.UImageAssetNode();
        //for test
在此之后:
        public Common.TtImageAssetNode EnvMapNode { get; set; } = new Common.UImageAssetNode();
        public Common.TtImageAssetNode VignetteNode { get; set; } = new Common.UImageAssetNode();
        //for test
*/
        public Common.TtImageAssetNode EnvMapNode { get; set; } = new Common.TtImageAssetNode();
        public Common.TtImageAssetNode VignetteNode { get; set; } = new Common.TtImageAssetNode();
        //for test
        public Bricks.VXGI.UVoxelsNode VoxelsNode = new Bricks.VXGI.UVoxelsNode();

        public override async System.Threading.Tasks.Task Initialize(TtCamera camera)
        {
            await base.Initialize(camera);

            EnvMapNode.ImageName = RName.GetRName("texture/hdri_epic_courtyard_daylight.srv", RName.ERNameType.Engine);
            VignetteNode.ImageName = RName.GetRName("texture/default_vignette.srv", RName.ERNameType.Engine);

            //await BasePassNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>(),
            //    EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "BasePass");

            //await TranslucentNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>(),
            //    EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "BasePass");

            //await HitproxyNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHitproxyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Hitproxy");

            //await PickedNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickSetupShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "PickedNode");

            //await PickBlurNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickBlur");

            //await PickHollowNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickHollow");

            //await FinalCopyNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<UFinalCopyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "EditorFinal");

            //await mShadowMapNode.Initialize(this, TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>(), EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "ShadowDepth");

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
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
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
        public unsafe override void TickLogic(GamePlay.TtWorld world, Action<TtRenderGraphNode, TtRenderGraphPin, TtAttachBuffer> onRemove)
        {
            base.TickLogic(world, onRemove);

            if (this.DisableShadow == false)
                mShadowMapNode?.TickLogic(world, this, true);

            GpuSceneNode?.TickLogic(world, this, true);

            BasePassNode?.TickLogic(world, this, true);

            ScreenTilingNode?.TickLogic(world, this, false);

            HzbNode?.TickLogic(world, this, false);

            VoxelsNode?.TickLogic(world, this, true);

            TranslucentNode?.TickLogic(world, this, true);

            HitproxyNode?.TickLogic(world, this, true);

            PickedNode?.TickLogic(world, this, true);

            PickBlurNode?.TickLogic(world, this, true);

            PickHollowNode?.TickLogic(world, this, true);

            FinalCopyNode?.TickLogic(world, this, true);
        }
        public unsafe override void TickSync()
        {
            if (this.DisableShadow == false)
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
