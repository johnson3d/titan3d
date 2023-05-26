using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredPolicy : URenderPolicy
    {
        public UDeferredPolicy()
        {
            RegRenderNode("EnvMapNode", EnvMapNode);
            RegRenderNode("VignetteNode", VignetteNode);
            RegRenderNode("BasePassNode", BasePassNode);
            RegRenderNode("ShadowMapNode", mShadowMapNode);
            RegRenderNode("DirLightingNode", DirLightingNode);
            RegRenderNode("ForwordNode", ForwordNode);
            RegRenderNode("ParticleNode", ParticleNode);
            RegRenderNode("HitproxyNode", HitproxyNode);
            RegRenderNode("PickedNode", PickedNode);
            RegRenderNode("PickBlurNode", PickBlurNode);
            RegRenderNode("PickHollowNode", PickHollowNode);
            RegRenderNode("GpuSceneNode", GpuSceneNode);
            RegRenderNode("ScreenTilingNode", ScreenTilingNode);
            RegRenderNode("HdrNode", HdrNode);
            RegRenderNode("VoxelsNode", VoxelsNode);
            RegRenderNode("AvgBrightnessNode", AvgBrightnessNode);
            //RegRenderNode("HzbNode", HzbNode);
            RegRenderNode("Copy2SwapChainNode", Copy2SwapChainNode);

            //RegRenderNode("TempCopyNode", TempCopyNode); 
        }        
        public Common.UImageAssetNode EnvMapNode { get; set; } = new Common.UImageAssetNode();
        public Common.UImageAssetNode VignetteNode { get; set; } = new Common.UImageAssetNode();
        public UDeferredBasePassNode BasePassNode { get; set; } = new UDeferredBasePassNode();
        public Shadow.UShadowMapNode mShadowMapNode { get; set; } = new Shadow.UShadowMapNode();
        public UDeferredDirLightingNode DirLightingNode { get; set; } = new UDeferredDirLightingNode();
        public UForwordNode ForwordNode { get; set; } = new UForwordNode();
        public Bricks.Particle.UParticleGraphNode ParticleNode { get; set; } = new Bricks.Particle.UParticleGraphNode();

        public Common.UHitproxyNode HitproxyNode { get; set; } = new Common.UHitproxyNode();
        public Common.UPickedNode PickedNode { get; set; } = new Common.UPickedNode();
        public Common.UPickBlurNode PickBlurNode { get; set; } = new Common.UPickBlurNode();
        public Common.UPickHollowNode PickHollowNode { get; set; } = new Common.UPickHollowNode();

        public Common.UGpuSceneNode GpuSceneNode { get; set; } = new Common.UGpuSceneNode();
        public Common.UScreenTilingNode ScreenTilingNode { get; set; } = new Common.UScreenTilingNode();
        //public Common.UHzbNode HzbNode { get; set; } = new Common.UHzbNode();

        public Common.UAvgBrightnessNode AvgBrightnessNode { get; set; } = new Common.UAvgBrightnessNode();
        public Common.UHdrNode HdrNode { get; set; } = new Common.UHdrNode();
        //for test
        public Bricks.VXGI.UVoxelsNode VoxelsNode { get; set; } = new Bricks.VXGI.UVoxelsNode();

        public Common.UCopyNode TempCopyNode { get; set; } = new Common.UCopyNode();

        public Common.UCopy2SwapChainNode Copy2SwapChainNode { get; set; } = new Common.UCopy2SwapChainNode();

        #region Feature On/Off
        public override bool DisableShadow
        {
            get => mDisableShadow;
            set
            {
                mDisableShadow = value;
                var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                shading?.SetDisableShadow(value);
            }
        }
        public override bool DisablePointLight
        {
            get
            {
                return mDisablePointLight;
            }
            set
            {
                mDisablePointLight = value;
                var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                shading?.SetDisablePointLights(value);
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
                //var shading = DirLightingNode.ScreenDrawPolicy.mBasePassShading as UDeferredDirLightingShading;
                //shading?.SetDisableHDR(value);
            }
        }
        #endregion

        public override Common.UGpuSceneNode GetGpuSceneNode()
        {
            return GpuSceneNode;
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
        public override NxRHI.USrView GetFinalShowRSV()
        {
            var attachBuffer = Copy2SwapChainNode.FindAttachBuffer(Copy2SwapChainNode.ColorPinOut);
            if (attachBuffer == null)
                return null;
            return attachBuffer.Srv;
        }
        public override Shader.UGraphicsShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    {
                        if (node == BasePassNode)
                        {
                            return BasePassNode.mOpaqueShading;
                        }
                        else if (node == ForwordNode)
                        {
                            switch (mesh.Atoms[atom].Material.RenderLayer)
                            {
                                case ERenderLayer.RL_Translucent:
                                    return ForwordNode.mTranslucentShading;
                                case ERenderLayer.RL_Sky:
                                    return ForwordNode.mTranslucentShading;
                                default:
                                    return ForwordNode.mOpaqueShading;
                            }
                        }
                    }
                    break;
                case EShadingType.DepthPass:
                    return mShadowMapNode.mShadowShading;
                case EShadingType.HitproxyPass:
                    return HitproxyNode.mHitproxyShading;
                case EShadingType.Picked:
                    return PickedNode.PickedShading;
                default:
                    break;
            }
            return null;
        }
        public override async System.Threading.Tasks.Task Initialize(UCamera camera)
        {
            await base.Initialize(camera);

            EnvMapNode.ImageName = RName.GetRName("texture/default_envmap.srv", RName.ERNameType.Engine);
            VignetteNode.ImageName = RName.GetRName("texture/default_vignette.srv", RName.ERNameType.Engine);

            //await GpuSceneNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "GpuScene");

            //await BasePassNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredOpaque>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y);

            //await mShadowMapNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>(), EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "ShadowDepth");

            //await DirLightingNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>(), EPixelFormat.PXF_R10G10B10A2_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "DirLighting");

            //await ForwordNode.Initialize(this, null, EPixelFormat.PXF_R10G10B10A2_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Forward");

            //await ParticleNode.Initialize(this, null, EPixelFormat.PXF_R10G10B10A2_UNORM, EPixelFormat.PXF_D16_UNORM, x, y, "Particle"); 

            //await HitproxyNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHitproxyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Hitproxy");

            //await PickedNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickSetupShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "PickedNode");

            //await PickBlurNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickBlur");

            //await PickHollowNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickHollow");

            //await VoxelsNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "VoxelsNode");

            //await HzbNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "HzbNode");

            //await ScreenTilingNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "ScreenTilingNode");            

            //await AvgBrightnessNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "Brightness");

            //await HdrNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHdrShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "Hdr");

        }

        protected override void OnBuildGraph()
        {
            {
                AddLinker(EnvMapNode.ImagePinOut, DirLightingNode.EnvMapPinIn);
                AddLinker(VignetteNode.ImagePinOut, DirLightingNode.VignettePinIn);
                AddLinker(GpuSceneNode.PointLightsPinOut, DirLightingNode.PointLightsPinIn);
                //AddLinker(BasePassNode.Rt0PinOut, TempCopyNode.SrcPinIn);
                //AddLinker(TempCopyNode.DestPinOut, DirLightingNode.Rt0PinIn);

                AddLinker(BasePassNode.Rt0PinOut, DirLightingNode.Rt0PinIn);
                AddLinker(BasePassNode.Rt1PinOut, DirLightingNode.Rt1PinIn);
                AddLinker(BasePassNode.Rt2PinOut, DirLightingNode.Rt2PinIn);
                AddLinker(BasePassNode.DepthStencilPinOut, DirLightingNode.DepthStencilPinIn);
                AddLinker(mShadowMapNode.DepthPinOut, DirLightingNode.ShadowMapPinIn);
                {
                    {
                        AddLinker(PickedNode.PickedPinOut, PickBlurNode.PickedPinIn);
                    }
                    AddLinker(PickedNode.PickedPinOut, PickHollowNode.PickedPinIn);
                    AddLinker(PickBlurNode.ResultPinOut, PickHollowNode.BlurPinIn);
                }
                AddLinker(PickHollowNode.ResultPinOut, DirLightingNode.PickPinIn);
                {
                    AddLinker(GpuSceneNode.PointLightsPinOut, ScreenTilingNode.PointLightsPinIn);
                    AddLinker(BasePassNode.DepthStencilPinOut, ScreenTilingNode.DepthPinIn);
                }
                AddLinker(ScreenTilingNode.TilingPinOut, DirLightingNode.TileScreenPinIn);
                AddLinker(GpuSceneNode.GpuScenePinOut, DirLightingNode.GpuScenePinIn);
            }
            AddLinker(DirLightingNode.ResultPinOut, AvgBrightnessNode.ColorPinIn);
            //AddLinker(GpuSceneNode.GpuScenePinOut, AvgBrightnessNode.GpuScenePinInOut);

            AddLinker(DirLightingNode.ResultPinOut, ForwordNode.ColorPinInOut);
            AddLinker(BasePassNode.DepthStencilPinOut, ForwordNode.DepthPinInOut);

            AddLinker(ForwordNode.ColorPinInOut, ParticleNode.ColorPinInOut);
            AddLinker(ForwordNode.DepthPinInOut, ParticleNode.DepthPinInOut);

            AddLinker(ParticleNode.ColorPinInOut, HdrNode.ColorPinIn);            
            AddLinker(AvgBrightnessNode.GpuScenePinInOut, HdrNode.GpuScenePinIn);

            AddLinker(HdrNode.ResultPinOut, Copy2SwapChainNode.ColorPinOut);
            AddLinker(HitproxyNode.HitIdPinOut, Copy2SwapChainNode.HitIdPinIn);

            // vxgi
            AddLinker(GpuSceneNode.GpuScenePinOut, VoxelsNode.GpuScenePinInOut);
            AddLinker(BasePassNode.Rt0PinOut, VoxelsNode.AlbedoPinInOut);
            AddLinker(BasePassNode.DepthStencilPinOut, VoxelsNode.DepthPinInOut);
            
            AddLinker(VoxelsNode.GpuScenePinInOut, AvgBrightnessNode.GpuScenePinInOut);


            RootNode = Copy2SwapChainNode;
            DisableHDR = false;
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);
            //BasePassNode.OnResize(this, x, y);

            //ScreenTilingNode.OnResize(this, x, y);

            //HzbNode.OnResize(this, x, y);

            //DirLightingNode?.OnResize(this, x, y);

            //ForwordNode?.OnResize(this, x, y);

            //HitproxyNode?.OnResize(this, x, y);

            //PickedNode?.OnResize(this, x, y);

            //PickBlurNode?.OnResize(this, x, y);

            //PickHollowNode?.OnResize(this, x, y);

            //VoxelsNode?.OnResize(this, x, y);

            //GpuSceneNode?.OnResize(this, x, y);

            //AvgBrightnessNode?.OnResize(this, x, y);

            //HdrNode?.OnResize(this, x, y);
        }
        public override void Dispose()
        {
            //BasePassNode.Cleanup();

            //ScreenTilingNode?.Cleanup();
            //ScreenTilingNode = null;

            //mShadowMapNode?.Cleanup();
            //mShadowMapNode = null;

            //DirLightingNode?.Cleanup();
            //DirLightingNode = null;

            //ForwordNode?.Cleanup();
            //ForwordNode = null;

            //HitproxyNode?.Cleanup();
            //HitproxyNode = null;

            //PickedNode?.Cleanup();
            //PickedNode = null;

            //PickBlurNode?.Cleanup();
            //PickBlurNode = null;

            //PickHollowNode?.Cleanup();
            //PickHollowNode = null;

            //VoxelsNode?.Cleanup();
            //VoxelsNode = null;

            //HzbNode?.Cleanup();
            //HzbNode = null;

            //GpuSceneNode?.Cleanup();
            //GpuSceneNode = null;

            //AvgBrightnessNode?.Cleanup();
            //AvgBrightnessNode = null;

            //HdrNode?.Cleanup();
            //HdrNode = null;

            base.Dispose();
        }
        public override void BeginTickLogic(GamePlay.UWorld world)
        {
            ParticleNode?.BeginTickLogic(world, this, true);
        }
        public override void EndTickLogic(GamePlay.UWorld world)
        {
            ParticleNode?.EndTickLogic(world, this, true);
        }
    }
}
