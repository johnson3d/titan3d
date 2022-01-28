using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileFSPolicy : URenderPolicy
    {
        #region Feature On/Off
        public override bool DisableShadow
        {
            get => mDisableShadow;
            set
            {
                mDisableShadow = value;
                BasePassNode.mOpaqueShading.DisableShadow.SetValue(value);
                BasePassNode.mOpaqueShading.UpdatePermutation();
            }
        }
        public override bool DisableAO
        {
            get => mDisableAO;
            set
            {
                mDisableAO = value;
                BasePassNode.mOpaqueShading.DisableAO.SetValue(value);
                BasePassNode.mOpaqueShading.UpdatePermutation();
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
                shading?.DisablePointLights.SetValue(value);
                shading.UpdatePermutation();
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
            return this.AttachmentCache.FindAttachement(BasePassNode.GBuffers.RenderTargets[0].Attachement.AttachmentName).Srv;
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
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom)
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
