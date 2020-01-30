using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileSunShaftBlurSE : CGfxShadingEnv
    {
        public CShaderResourceView mSunShaftMask;
        private CTextureBindInfo mTexBindInfo_SunShaftMask = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_SunShaftMask = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_SunShaftMask = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileSunShaftBlur.shadingenv");
            }
        }
        protected override void OnCreated()
        {
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;
            if (Shader.FindTextureBindInfo(null, "gSunShaftMask", ref mTexBindInfo_SunShaftMask))
            {
                TexBinder.PSBindTexture(mTexBindInfo_SunShaftMask.PSBindPoint, mSunShaftMask);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gSunShaftMask", ref mSamplerBindInfo_SunShaftMask) == true)
            {
                mSamplerStateDesc_SunShaftMask.SetDefault();
                mSamplerStateDesc_SunShaftMask.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_SunShaftMask.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SunShaftMask.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SunShaftMask.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_SunShaftMask);
                SamplerBinder.PSBindSampler(mSamplerBindInfo_SunShaftMask.PSBindPoint, SamplerStat);
            }

        }
    }
}
