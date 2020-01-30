using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileAoBlurVSE : CGfxShadingEnv
    {
        public CShaderResourceView mSRV_Src;
        private CTextureBindInfo mTexBindInfo_Src = new CTextureBindInfo();
        private CSamplerBindInfo mSampBindInfo_Src = new CSamplerBindInfo();
        private CSamplerStateDesc mSampStateDesc_Src = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileAoBlurV.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            //AddSRV("gSrcTex");
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;

            if (Shader.FindTextureBindInfo(null, "gSrcTex", ref mTexBindInfo_Src))
            {
                TexBinder.PSBindTexture(mTexBindInfo_Src.PSBindPoint, mSRV_Src);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gSrcTex", ref mSampBindInfo_Src) == true)
            {
                mSampStateDesc_Src.SetDefault();
                mSampStateDesc_Src.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSampStateDesc_Src.AddressU = EAddressMode.ADM_CLAMP;
                mSampStateDesc_Src.AddressV = EAddressMode.ADM_CLAMP;
                mSampStateDesc_Src.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSampStateDesc_Src);
                SamplerBinder.PSBindSampler(mSampBindInfo_Src.PSBindPoint, SamplerStat);
            }

        }
    }
}
