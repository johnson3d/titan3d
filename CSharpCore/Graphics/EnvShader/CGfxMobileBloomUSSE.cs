using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileBloomUSSE : CGfxShadingEnv
    {
        public CShaderResourceView mSrcTexUp;
        private CTextureBindInfo mTexBindInfo_SrcTexUp = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_SrcTexUp = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_SrcTexUp = new CSamplerStateDesc();

        public CShaderResourceView mSrcTexDown;
        private CTextureBindInfo mTexBindInfo_SrcTexDown = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_SrcTexDown = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_SrcTexDown = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileBloomUS.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            AddSRV("gSrcTexUp");
            AddSRV("gSrcTexDown");
        }
        
        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
            //, CConstantBuffer cbPerInstance, CShaderResources TexBinder, CShaderSamplers SamplerBinder, CShaderProgram Shader)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;
            //up
            {
                if (Shader.FindTextureBindInfo(null, "gSrcTexUp", ref mTexBindInfo_SrcTexUp))
                {
                    TexBinder.PSBindTexture(mTexBindInfo_SrcTexUp.PSBindPoint, mSrcTexUp);
                }

                if (Shader.FindSamplerBindInfo(null, "Samp_gSrcTexUp", ref mSamplerBindInfo_SrcTexUp) == true)
                {
                    mSamplerStateDesc_SrcTexUp.SetDefault();
                    mSamplerStateDesc_SrcTexUp.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    mSamplerStateDesc_SrcTexUp.AddressU = EAddressMode.ADM_CLAMP;
                    mSamplerStateDesc_SrcTexUp.AddressV = EAddressMode.ADM_CLAMP;

                    var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_SrcTexUp);

                    SamplerBinder.PSBindSampler(mSamplerBindInfo_SrcTexUp.PSBindPoint, SamplerStat);
                }
            }

            //down
            {
                if (Shader.FindTextureBindInfo(null, "gSrcTexDown", ref mTexBindInfo_SrcTexDown))
                {
                    TexBinder.PSBindTexture(mTexBindInfo_SrcTexDown.PSBindPoint, mSrcTexDown);
                }

                if (Shader.FindSamplerBindInfo(null, "Samp_gSrcTexDown", ref mSamplerBindInfo_SrcTexDown) == true)
                {
                    mSamplerStateDesc_SrcTexDown.SetDefault();
                    mSamplerStateDesc_SrcTexDown.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                    mSamplerStateDesc_SrcTexDown.AddressU = EAddressMode.ADM_CLAMP;
                    mSamplerStateDesc_SrcTexDown.AddressV = EAddressMode.ADM_CLAMP;

                    var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_SrcTexDown);

                    SamplerBinder.PSBindSampler(mSamplerBindInfo_SrcTexDown.PSBindPoint, SamplerStat);
                }
            }
        }
    }
}
