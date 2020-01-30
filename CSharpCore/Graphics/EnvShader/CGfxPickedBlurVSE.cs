using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxPickedBlurVSE : CGfxShadingEnv
    {
        public CShaderResourceView mSRV_PickedBlurH;
        private CTextureBindInfo mTBI_PickedBlurH = new CTextureBindInfo();
        private CSamplerBindInfo mSBI_PickedBlurH = new CSamplerBindInfo();
        private CSamplerStateDesc mSSD_PickedBlurH = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/PickedBlurV.shadingenv");
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

            if (Shader.FindTextureBindInfo(null, "gPickedBlurHTex", ref mTBI_PickedBlurH))
            {
                TexBinder.PSBindTexture(mTBI_PickedBlurH.PSBindPoint, mSRV_PickedBlurH);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gPickedBlurH", ref mSBI_PickedBlurH) == true)
            {
                mSSD_PickedBlurH.SetDefault();
                mSSD_PickedBlurH.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                mSSD_PickedBlurH.AddressU = EAddressMode.ADM_CLAMP;
                mSSD_PickedBlurH.AddressV = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSSD_PickedBlurH);

                SamplerBinder.PSBindSampler(mSBI_PickedBlurH.PSBindPoint, SamplerStat);
            }
        }
    }
}
