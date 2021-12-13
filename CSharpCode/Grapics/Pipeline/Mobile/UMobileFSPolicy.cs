using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileFSPolicy : IRenderPolicy
    {
        public override Common.UBasePassNode GetBasePassNode() 
        { 
            return BasePassNode; 
        }
        #region Feature On/Off
        public override bool DisableShadow
        {
            get => mDisableShadow;
            set
            {
                mDisableShadow = value;
                BasePassNode.mOpaqueShading.SetDisableShadow(value);
            }
        }
        public override bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                BasePassNode.mOpaqueShading.SetDisableAO(value);
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
                var shading = BasePassNode.mOpaqueShading;
                shading?.SetDisablePointLights(value);
            }
        }
        #endregion
        public UMobileOpaqueNode BasePassNode = new UMobileOpaqueNode();
        public Shadow.UShadowMapNode mShadowMapNode = new Shadow.UShadowMapNode();
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName EnvMap
        {
            get
            {
                if (EnvMapSRV == null)
                    return null;
                return EnvMapSRV.AssetName;
            }
            set
            {
                Action action = async () =>
                {
                    EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                action();
            }
        }
        public RHI.CShaderResourceView EnvMapSRV;
        public RHI.CShaderResourceView VignetteSRV;
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            return BasePassNode.GBuffers.GetGBufferSRV(0);
        }

        public override async System.Threading.Tasks.Task Initialize(CCamera camera, float x, float y)
        {
            await base.Initialize(camera, x, y);
            await BasePassNode.Initialize(this, UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassOpaque>(), EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y, "BasePass");
            
            EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("texture/default_envmap.srv", RName.ERNameType.Engine));
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

            BasePassNode.OnResize(this, x, y);
        }
        public override void Cleanup()
        {
            BasePassNode.Cleanup();
            base.Cleanup();
        }
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return BasePassNode.mOpaqueShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom)
        {
            base.OnDrawCall(shadingType, drawcall, mesh, atom);
            if (shadingType == EShadingType.BasePass)
            {
                switch (mesh.Atoms[atom].Material.RenderLayer)
                {
                    case ERenderLayer.RL_Translucent:
                        //BasePassNode.mTranslucentShading.OnDrawCall(shadingType, drawcall, this, mesh);
                        return;
                    default:
                        BasePassNode.mOpaqueShading.OnDrawCall(shadingType, drawcall, this, mesh);
                        return;
                }
            }
        }
        public unsafe override void TickLogic(GamePlay.UWorld world)
        {
            BasePassNode.TickLogic(world, this, true);
        }
        public unsafe override void TickRender()
        {
            BasePassNode.TickRender(this);
        }
        public unsafe override void TickSync()
        {
            BasePassNode.TickSync(this);
        }
    }
}
