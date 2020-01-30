using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    

    public class CGfxPickedHollowSE : CGfxShadingEnv
    {
        public CShaderResourceView mSRV_PickedSetUp;
        private CTextureBindInfo mTBI_PickedSetUp = new CTextureBindInfo();
        private CSamplerBindInfo mSBI_PickedSetUp = new CSamplerBindInfo();
        private CSamplerStateDesc mSSD_PickedSetUp = new CSamplerStateDesc();

        public CShaderResourceView mSRV_PickedBlur;
        private CTextureBindInfo mTBI_PickedBlur = new CTextureBindInfo();
        private CSamplerBindInfo mSBI_PickedBlur = new CSamplerBindInfo();
        private CSamplerStateDesc mSSD_PickedBlur = new CSamplerStateDesc();
        
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/PickedHollow.shadingenv");
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

            if (Shader.FindTextureBindInfo(null, "gPickedSetUpTex", ref mTBI_PickedSetUp))
            {
                TexBinder.PSBindTexture(mTBI_PickedSetUp.PSBindPoint, mSRV_PickedSetUp);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gPickedSetUp", ref mSBI_PickedSetUp) == true)
            {
                mSSD_PickedSetUp.SetDefault();
                mSSD_PickedSetUp.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                mSSD_PickedSetUp.AddressU = EAddressMode.ADM_CLAMP;
                mSSD_PickedSetUp.AddressV = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSSD_PickedSetUp);

                SamplerBinder.PSBindSampler(mSBI_PickedSetUp.PSBindPoint, SamplerStat);
            }

            //
            if (Shader.FindTextureBindInfo(null, "gPickedBlurTex", ref mTBI_PickedBlur))
            {
                TexBinder.PSBindTexture(mTBI_PickedBlur.PSBindPoint, mSRV_PickedBlur);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gPickedBlur", ref mSBI_PickedBlur) == true)
            {
                mSSD_PickedBlur.SetDefault();
                mSSD_PickedBlur.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                mSSD_PickedBlur.AddressU = EAddressMode.ADM_CLAMP;
                mSSD_PickedBlur.AddressV = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSSD_PickedBlur);

                SamplerBinder.PSBindSampler(mSBI_PickedBlur.PSBindPoint, SamplerStat);
            }
        }
    }
}
