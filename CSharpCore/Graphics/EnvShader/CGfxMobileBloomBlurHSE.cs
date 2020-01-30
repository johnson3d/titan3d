using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileBloomBlurHSE : CGfxShadingEnv
    {
        public CShaderResourceView mBaseSceneView;
        private CTextureBindInfo mTexBindInfo_BaseSceneView = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_BaseSceneView = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_BaseSceneView = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileBloomBlurH.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            AddSRV("gBaseSceneView");
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;

            if (Shader.FindTextureBindInfo(null, "gBaseSceneView", ref mTexBindInfo_BaseSceneView))
            {
                TexBinder.PSBindTexture(mTexBindInfo_BaseSceneView.PSBindPoint, mBaseSceneView);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gBaseSceneView", ref mSamplerBindInfo_BaseSceneView) == true)
            {
                mSamplerStateDesc_BaseSceneView.SetDefault();
                mSamplerStateDesc_BaseSceneView.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_BaseSceneView.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BaseSceneView.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BaseSceneView.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_BaseSceneView);

                SamplerBinder.PSBindSampler(mSamplerBindInfo_BaseSceneView.PSBindPoint, SamplerStat);
            }

        }
    }
}
