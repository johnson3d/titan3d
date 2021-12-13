using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class UDeferredPolicy : IRenderPolicy
    {
        public RHI.CShaderResourceView EnvMapSRV;
        public RHI.CShaderResourceView VignetteSRV;
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
        public Common.UHzbNode HzbNode { get; set; } = new Common.UHzbNode();

        public Common.UAvgBrightnessNode AvgBrightnessNode { get; set; } = new Common.UAvgBrightnessNode();
        public Common.UHdrNode HdrNode { get; set; } = new Common.UHdrNode();
        //for test
        public Bricks.VXGI.UVoxelsNode VoxelsNode { get; set; } = new Bricks.VXGI.UVoxelsNode();

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

        public override Common.UBasePassNode GetBasePassNode()
        {
            return BasePassNode;
        }
        public override Common.UGpuSceneNode GetGpuSceneNode()
        {
            return GpuSceneNode;
        }
        public override Common.URenderGraphNode QueryNode(string name)
        {
            switch (name)
            {
                case "BasePassNode":
                    return GetBasePassNode();
                case "ShadowMapNode":
                    return mShadowMapNode;
                case "DirLightingNode":
                    return DirLightingNode;
                case "ForwordNode":
                    return ForwordNode;
                case "ParticleNode":
                    return ParticleNode;
                case "HitproxyNode":
                    return HitproxyNode;
                case "PickedNode":
                    return PickedNode;
                case "PickBlurNode":
                    return PickBlurNode;
                case "PickHollowNode":
                    return PickHollowNode;
                case "VoxelsNode":
                    return VoxelsNode;
                case "HzbNode":
                    return HzbNode;
                case "GpuSceneNode":
                    return GpuSceneNode;
                case "ScreenTilingNode":
                    return ScreenTilingNode;
                case "AvgBrightnessNode":
                    return AvgBrightnessNode;
                case "HdrNode":
                    return HdrNode;
            }
            return null;
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
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            if (mDisableHDR)
            {
                return DirLightingNode.GBuffers.GetGBufferSRV(0);
            }
            else
            {
                if (HdrNode.GBuffers != null)
                {
                    var srv = HdrNode.GBuffers.GetGBufferSRV(0);
                    if (srv != null)
                        return srv;
                }
                return DirLightingNode.GBuffers.GetGBufferSRV(0);
            }
        }
        public override RHI.CShaderResourceView QuerySRV(string name)
        {
            switch (name)
            {
                case "FinalRT":
                    return DirLightingNode.GBuffers.GetGBufferSRV(0);
                case "LightRT":
                    return DirLightingNode.GBuffers.GetGBufferSRV(0);
                default:
                    return null;
            }
        }
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
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
        public override async System.Threading.Tasks.Task Initialize(CCamera camera, float x, float y)
        {
            await base.Initialize(camera, x, y);

            EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("texture/default_envmap.srv", RName.ERNameType.Engine));
            VignetteSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("texture/default_vignette.srv", RName.ERNameType.Engine));

            await GpuSceneNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "GpuScene");

            await BasePassNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredOpaque>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y);

            await mShadowMapNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Shadow.UShadowShading>(), EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "ShadowDepth");

            await DirLightingNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>(), EPixelFormat.PXF_R10G10B10A2_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "DirLighting");

            await ForwordNode.Initialize(this, null, this.DirLightingNode.GBuffers.GetGBufferSRV(0).mCoreObject.GetFormat(), EPixelFormat.PXF_D16_UNORM, x, y, "Forward");

            await ParticleNode.Initialize(this, null, this.DirLightingNode.GBuffers.GetGBufferSRV(0).mCoreObject.GetFormat(), EPixelFormat.PXF_D16_UNORM, x, y, "Particle"); 

            await HitproxyNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHitproxyShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "Hitproxy");

            await PickedNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickSetupShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "PickedNode");

            await PickBlurNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickBlurShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickBlur");

            await PickHollowNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UPickHollowShading>(), EPixelFormat.PXF_R16G16_FLOAT, EPixelFormat.PXF_UNKNOWN, x, y, "PickHollow");

            await VoxelsNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_D16_UNORM, x, y, "VoxelsNode");

            await HzbNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "HzbNode");

            await ScreenTilingNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "ScreenTilingNode");            

            await AvgBrightnessNode.Initialize(this, null, EPixelFormat.PXF_UNKNOWN, EPixelFormat.PXF_UNKNOWN, x, y, "Brightness");

            await HdrNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Common.UHdrShading>(), EPixelFormat.PXF_R8G8B8A8_UNORM, EPixelFormat.PXF_UNKNOWN, x, y, "Hdr");

            DisableHDR = false;
        }
        public override void OnResize(float x, float y)
        {
            BasePassNode.OnResize(this, x, y);

            ScreenTilingNode.OnResize(this, x, y);

            HzbNode.OnResize(this, x, y);

            DirLightingNode?.OnResize(this, x, y);

            ForwordNode?.OnResize(this, x, y);

            HitproxyNode?.OnResize(this, x, y);

            PickedNode?.OnResize(this, x, y);

            PickBlurNode?.OnResize(this, x, y);

            PickHollowNode?.OnResize(this, x, y);

            VoxelsNode?.OnResize(this, x, y);

            GpuSceneNode?.OnResize(this, x, y);

            AvgBrightnessNode?.OnResize(this, x, y);

            HdrNode?.OnResize(this, x, y);
        }
        public unsafe override void Cleanup()
        {
            BasePassNode.Cleanup();

            ScreenTilingNode?.Cleanup();
            ScreenTilingNode = null;

            mShadowMapNode?.Cleanup();
            mShadowMapNode = null;

            DirLightingNode?.Cleanup();
            DirLightingNode = null;

            ForwordNode?.Cleanup();
            ForwordNode = null;

            HitproxyNode?.Cleanup();
            HitproxyNode = null;

            PickedNode?.Cleanup();
            PickedNode = null;

            PickBlurNode?.Cleanup();
            PickBlurNode = null;

            PickHollowNode?.Cleanup();
            PickHollowNode = null;

            VoxelsNode?.Cleanup();
            VoxelsNode = null;

            HzbNode?.Cleanup();
            HzbNode = null;

            GpuSceneNode?.Cleanup();
            GpuSceneNode = null;

            AvgBrightnessNode?.Cleanup();
            AvgBrightnessNode = null;

            HdrNode?.Cleanup();
            HdrNode = null;
            
            base.Cleanup();
        }
        public unsafe void BeginTickLogic(GamePlay.UWorld world)
        {
            ParticleNode?.BeginTickLogic(world, this, true);
        }
        public unsafe void EndTickLogic(GamePlay.UWorld world)
        {
            ParticleNode?.EndTickLogic(world, this, true);
        }
        public unsafe override void TickLogic(GamePlay.UWorld world)
        {
            BeginTickLogic(world);

            if (DisableShadow == false)
                mShadowMapNode?.TickLogic(world, this, true);

            BasePassNode?.TickLogic(world, this, true);

            GpuSceneNode?.TickLogic(world, this, true);

            ScreenTilingNode?.TickLogic(world, this, false);

            HzbNode?.TickLogic(world, this, false);

            VoxelsNode?.TickLogic(world, this, true);

            HitproxyNode?.TickLogic(world, this, true);

            PickedNode?.TickLogic(world, this, true);

            PickBlurNode?.TickLogic(world, this, true);

            PickHollowNode?.TickLogic(world, this, true);

            DirLightingNode?.TickLogic(world, this, true);


            if (mDisableHDR == false)
            {
                AvgBrightnessNode?.TickLogic(world, this, true);
            }

            ForwordNode?.TickLogic(world, this, true);
            ParticleNode?.TickLogic(world, this, true);

            if (mDisableHDR == false)
            {
                //AvgBrightnessNode?.TickLogic(world, this, true);
                HdrNode?.TickLogic(world, this, true);
            }

            EndTickLogic(world);
        }
        public unsafe override void TickRender()
        {
            if (DisableShadow == false)
                mShadowMapNode?.TickRender(this);

            BasePassNode?.TickRender(this);

            GpuSceneNode?.TickRender(this);

            ScreenTilingNode?.TickRender(this);

            HzbNode?.TickRender(this);

            VoxelsNode?.TickRender(this);

            HitproxyNode?.TickRender(this);

            PickedNode?.TickRender(this);

            PickBlurNode?.TickRender(this);

            PickHollowNode?.TickRender(this);

            DirLightingNode?.TickRender(this);

            if (mDisableHDR == false)
            {
                AvgBrightnessNode?.TickRender(this);
            }

            ForwordNode?.TickRender(this);
            ParticleNode?.TickRender(this);

            if (mDisableHDR == false)
            {
                //AvgBrightnessNode?.TickRender(this);
                HdrNode?.TickRender(this);
            }
        }
        public unsafe override void TickSync()
        {
            if (DisableShadow == false)
                mShadowMapNode?.TickSync(this);

            BasePassNode?.TickSync(this);

            GpuSceneNode?.TickSync(this);

            ScreenTilingNode?.TickSync(this);

            HzbNode?.TickSync(this);

            VoxelsNode?.TickSync(this);

            HitproxyNode?.TickSync(this);

            PickedNode?.TickSync(this);

            PickBlurNode?.TickSync(this);

            PickHollowNode?.TickSync(this);

            DirLightingNode?.TickSync(this);

            ForwordNode?.TickSync(this);
            ParticleNode?.TickSync(this);

            if (mDisableHDR == false)
            {
                AvgBrightnessNode?.TickSync(this);

                HdrNode?.TickSync(this);
            }
        }
    }
}
