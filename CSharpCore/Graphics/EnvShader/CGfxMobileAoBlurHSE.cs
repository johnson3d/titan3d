using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileAoBlurHSE : CGfxShadingEnv
    {
        public CShaderResourceView mSRV_AoMask;
        private CTextureBindInfo mTexBindInfo_AoMask = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_AoMask = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_AoMask = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileAoBlurH.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            //AddSRV("gAoMask");
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;

            if (Shader.FindTextureBindInfo(null, "gAoMask", ref mTexBindInfo_AoMask))
            {
                TexBinder.PSBindTexture(mTexBindInfo_AoMask.PSBindPoint, mSRV_AoMask);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gAoMask", ref mSamplerBindInfo_AoMask) == true)
            {
                mSamplerStateDesc_AoMask.SetDefault();
                mSamplerStateDesc_AoMask.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_AoMask.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_AoMask.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_AoMask.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_AoMask);

                SamplerBinder.PSBindSampler(mSamplerBindInfo_AoMask.PSBindPoint, SamplerStat);
            }

        }
    }
}
