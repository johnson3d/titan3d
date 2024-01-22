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
        public override NxRHI.USrView GetFinalShowRSV()
        {
            return this.AttachmentCache.FindAttachement(BasePassNode.GBuffers.RenderTargets[0].Attachement.AttachmentName).Srv;
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

            BasePassNode.OnResize(this, x, y);
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref BasePassNode);
            base.Dispose();
        }
        public override Shader.UGraphicsShadingEnv GetPassShading(EShadingType type, Mesh.TtMesh.TtAtom atom, Pipeline.TtRenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return BasePassNode.mOpaqueShading;
            }
            return null;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, atom);
            if (shadingType == EShadingType.BasePass)
            {
                switch (atom.Material.RenderLayer)
                {
                    case ERenderLayer.RL_Translucent:
                        //BasePassNode.mTranslucentShading.OnDrawCall(shadingType, drawcall, this, mesh);
                        return;
                    default:
                        BasePassNode.mOpaqueShading.OnDrawCall(cmd, shadingType, drawcall, this, atom);
                        return;
                }
            }
        }
        public unsafe override void TickLogic(GamePlay.UWorld world)
        {
            BasePassNode.TickLogic(world, this, true);
        }
        public unsafe override void TickSync()
        {
            BasePassNode.TickSync(this);
        }
    }
}
