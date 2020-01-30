using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileBloomBlurVSE : CGfxShadingEnv
    {
        public CShaderResourceView mSrcTex;
        private CTextureBindInfo mTexBindInfo_SrcTex = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_SrcTex = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_SrcTex = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileBloomBlurV.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            AddSRV("gSrcTex");
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;
            if (Shader.FindTextureBindInfo(null, "gSrcTex", ref mTexBindInfo_SrcTex))
            {
                TexBinder.PSBindTexture(mTexBindInfo_SrcTex.PSBindPoint, mSrcTex);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gSrcTex", ref mSamplerBindInfo_SrcTex) == true)
            {
                mSamplerStateDesc_SrcTex.SetDefault();
                mSamplerStateDesc_SrcTex.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_SrcTex.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SrcTex.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SrcTex.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_SrcTex);
                SamplerBinder.PSBindSampler(mSamplerBindInfo_SrcTex.PSBindPoint, SamplerStat);
            }

        }
    }
}
