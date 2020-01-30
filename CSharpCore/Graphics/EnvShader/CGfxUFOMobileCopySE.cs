using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxUFOMobileCopySE : CGfxShadingEnv
    {
        public CShaderResourceView mBaseSceneView;
        private CTextureBindInfo mTexBindInfo_BaseSceneView = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_BaseSceneView = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_BaseSceneView = new CSamplerStateDesc();

        public CShaderResourceView mBloomTex;
        private CTextureBindInfo mTexBindInfo_BloomTex = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_BloomTex = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_BloomTex = new CSamplerStateDesc();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/UFOMobileCopy.shadingenv");
            }
        }

        protected override void OnCreated()
        {
            AddSRV("gBaseSceneView");
            AddSRV("gBloomTex");
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
            //, CConstantBuffer cbPerInstance, CShaderResources TexBinder, CShaderSamplers SamplerBinder, CShaderProgram Shader)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.GpuProgram;

            //base scene view;
            if (Shader.FindTextureBindInfo(null, "gBaseSceneView", ref mTexBindInfo_BaseSceneView))
            {
                TexBinder.PSBindTexture(mTexBindInfo_BaseSceneView.PSBindPoint, mBaseSceneView);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gBaseSceneView", ref mSamplerBindInfo_BaseSceneView) == true)
            {
                mSamplerStateDesc_BaseSceneView.SetDefault();
                mSamplerStateDesc_BaseSceneView.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                mSamplerStateDesc_BaseSceneView.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BaseSceneView.AddressV = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_BaseSceneView);

                SamplerBinder.PSBindSampler(mSamplerBindInfo_BaseSceneView.PSBindPoint, SamplerStat);
            }

            //bloom;
            if (Shader.FindTextureBindInfo(null, "gBloomTex", ref mTexBindInfo_BloomTex))
            {
                TexBinder.PSBindTexture(mTexBindInfo_BloomTex.PSBindPoint, mBloomTex);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gBloomTex", ref mSamplerBindInfo_BloomTex) == true)
            {
                mSamplerStateDesc_BloomTex.SetDefault();
                mSamplerStateDesc_BloomTex.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                mSamplerStateDesc_BloomTex.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BloomTex.AddressV = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_BloomTex);

                SamplerBinder.PSBindSampler(mSamplerBindInfo_BloomTex.PSBindPoint, SamplerStat);
            }
        }
    }
}
