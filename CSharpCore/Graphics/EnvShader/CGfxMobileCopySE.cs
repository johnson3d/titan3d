using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileCopySE : CGfxShadingEnv
    {
        public CShaderResourceView mBaseSceneView;
        private CTextureBindInfo mTexBindInfo_BaseSceneView = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_BaseSceneView = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_BaseSceneView = new CSamplerStateDesc();

        public CShaderResourceView mBloomTex;
        private CTextureBindInfo mTexBindInfo_BloomTex = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_BloomTex = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_BloomTex = new CSamplerStateDesc();

        CShaderResourceView mVignetteTex;
        RName mVignetteTexName;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName VignetteTex
        {
            get { return mVignetteTexName; }
            set
            {
                mVignetteTexName = value;
                mVignetteTex = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value, true);
            }
        }
        private CTextureBindInfo mTexBindInfo_Vignette = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_Vignette = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_Vignette = new CSamplerStateDesc();

        public CShaderResourceView mSunShaftTex;
        private CTextureBindInfo mTexBindInfo_SunShaft = new CTextureBindInfo();
        private CSamplerBindInfo mSamplerBindInfo_SunShaft = new CSamplerBindInfo();
        private CSamplerStateDesc mSamplerStateDesc_SunShaft = new CSamplerStateDesc();

        //mobile ao;
        public CShaderResourceView mSRV_MobileAo;
        private CTextureBindInfo mTBI_MobileAo = new CTextureBindInfo();
        CSamplerState mSampStat_MobileAo;
        private CSamplerBindInfo mSBI_MobileAo = new CSamplerBindInfo();
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_FSAA", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_AO", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_SUNSHAFT", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_BLOOM", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_HDR", "0"));
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileCopy.shadingenv");
            }
        }
        protected override void OnShadingEnvInit(GfxEnvShaderCode code)
        {
            code.GetMacroValues = FGetMacroValues;
        }
        static List<string> FGetMacroValues(string name)
        {
            switch (name)
            {
                case "ENV_DISABLE_AO":
                    {
                        var result = new List<string>();
                        result.Add("0");
                        result.Add("1");
                        return result;
                    }
            }
            return null;
        }
        protected override void OnCreated()
        {
            //AddSRV("gBaseSceneView");
            //AddSRV("gBloomTex");
            //mobile ao sampler state;
            {
                var SamplerDesc_MobileAo = new CSamplerStateDesc();
                SamplerDesc_MobileAo.SetDefault();
                SamplerDesc_MobileAo.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                SamplerDesc_MobileAo.AddressU = EAddressMode.ADM_CLAMP;
                SamplerDesc_MobileAo.AddressV = EAddressMode.ADM_CLAMP;
                SamplerDesc_MobileAo.AddressW = EAddressMode.ADM_CLAMP;
                mSampStat_MobileAo = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, SamplerDesc_MobileAo);
            }
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
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
                mSamplerStateDesc_BaseSceneView.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_BaseSceneView.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BaseSceneView.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BaseSceneView.AddressW = EAddressMode.ADM_CLAMP;

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
                mSamplerStateDesc_BloomTex.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_BloomTex.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BloomTex.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_BloomTex.AddressW = EAddressMode.ADM_CLAMP;

                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_BloomTex);
                SamplerBinder.PSBindSampler(mSamplerBindInfo_BloomTex.PSBindPoint, SamplerStat);
            }

            //vignette;
            if (Shader.FindTextureBindInfo(null, "gVignette", ref mTexBindInfo_Vignette))
            {
                TexBinder.PSBindTexture(mTexBindInfo_Vignette.PSBindPoint, mVignetteTex);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gVignette", ref mSamplerBindInfo_Vignette) == true)
            {
                mSamplerStateDesc_Vignette.SetDefault();
                mSamplerStateDesc_Vignette.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_Vignette.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_Vignette.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_Vignette.AddressW = EAddressMode.ADM_CLAMP;
                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_Vignette);
                SamplerBinder.PSBindSampler(mSamplerBindInfo_Vignette.PSBindPoint, SamplerStat);
            }

            //sun shaft;
            if (Shader.FindTextureBindInfo(null, "gSunShaft", ref mTexBindInfo_SunShaft))
            {
                TexBinder.PSBindTexture(mTexBindInfo_SunShaft.PSBindPoint, mSunShaftTex);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gSunShaft", ref mSamplerBindInfo_SunShaft) == true)
            {
                mSamplerStateDesc_SunShaft.SetDefault();
                mSamplerStateDesc_SunShaft.Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                mSamplerStateDesc_SunShaft.AddressU = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SunShaft.AddressV = EAddressMode.ADM_CLAMP;
                mSamplerStateDesc_SunShaft.AddressW = EAddressMode.ADM_CLAMP;
                var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, mSamplerStateDesc_SunShaft);
                SamplerBinder.PSBindSampler(mSamplerBindInfo_SunShaft.PSBindPoint, SamplerStat);
            }
            
            //mobile ao srv binding;
            if (Shader.FindTextureBindInfo(null, "gMobileAoTex", ref mTBI_MobileAo))
            {
                TexBinder.PSBindTexture(mTBI_MobileAo.PSBindPoint, mSRV_MobileAo);
            }

            if (Shader.FindSamplerBindInfo(null, "Samp_gMobileAoTex", ref mSBI_MobileAo) == true)
            {
                SamplerBinder.PSBindSampler(mSBI_MobileAo.PSBindPoint, mSampStat_MobileAo);
            }
        }
    }
}
