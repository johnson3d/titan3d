using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxPickedBlurHSE : CGfxShadingEnv
    {
        public CShaderResourceView mSRV_PickedSetUp;
        private CTextureBindInfo mTBI_PickedSetUp = new CTextureBindInfo();
        private CSamplerBindInfo mSBI_PickedSetUp = new CSamplerBindInfo();
        private CSamplerStateDesc mSSD_PickedSetUp = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/PickedBlurH.shadingenv");
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
        }
    }
}
