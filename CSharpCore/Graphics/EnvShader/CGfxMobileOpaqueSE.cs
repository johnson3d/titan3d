using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxMobileOpaqueSE : CGfxShadingEnv
    {
        RName mEnvMapName;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName EnvMapName
        {
            get { return mEnvMapName; }
            set
            {
                mEnvMapName = value;
                mEnvMap = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value, true);
            }
        }
        RName mEyeEnvMapName;
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Texture)]
        public RName EyeEnvMapName
        {
            get { return mEyeEnvMapName; }
            set
            {
                mEyeEnvMapName = value;
                mEyeEnvMap = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value, true);
            }
        }
        CShaderResourceView mEnvMap;
        public CShaderResourceView EnvMap
        {
            get { return mEnvMap; }
        }
        CShaderResourceView mEyeEnvMap;
        public CShaderResourceView EyeEnvMap
        {
            get { return mEyeEnvMap; }
        }
        CSamplerState SamplerStat;

        public CShaderResourceView mTex_ShadowMap;
        CSamplerState mSampler_ShadowMap;
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_SHADOW", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_AO", "0"));
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_DISABLE_POINTLIGHTS", "0"));
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/MobileOpaque.shadingenv");
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
            AddSRV("gEnvMap");
            AddSRV("gEyeEnvMap");
            AddSRV("gShadowMap");

            var SamplerStatDesc = new CSamplerStateDesc();
            SamplerStatDesc.SetDefault();
            SamplerStatDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
            SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, SamplerStatDesc);

            var SamplerDesc_ShadowMap = new CSamplerStateDesc();
            SamplerDesc_ShadowMap.SetDefault();
            SamplerDesc_ShadowMap.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
            SamplerDesc_ShadowMap.AddressU = EAddressMode.ADM_CLAMP;
            SamplerDesc_ShadowMap.AddressV = EAddressMode.ADM_CLAMP;
            SamplerDesc_ShadowMap.AddressW = EAddressMode.ADM_CLAMP;           
            //SamplerDesc_ShadowMap.BorderColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            mSampler_ShadowMap = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, SamplerDesc_ShadowMap);

        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
            //, CConstantBuffer cbPerInstance, CShaderResources TexBinder, CShaderSamplers SamplerBinder, CShaderProgram Shader)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var effect = pass.Effect;
            //env map;

            if (effect.CacheData.EnvMapBindPoint != UInt32.MaxValue)
            {
                TexBinder.PSBindTexture(effect.CacheData.EnvMapBindPoint, mEnvMap);
            }

            if (effect.CacheData.SampEnvMapBindPoint != UInt32.MaxValue)
            {
                SamplerBinder.PSBindSampler(effect.CacheData.SampEnvMapBindPoint, SamplerStat);
            }

            //eye env map;
            if (effect.CacheData.EyeEnvMapBindPoint != UInt32.MaxValue)
            {
                TexBinder.PSBindTexture(effect.CacheData.EyeEnvMapBindPoint, mEyeEnvMap);
            }

            if (effect.CacheData.SampEyeEnvMapBindPoint != UInt32.MaxValue)
            {
                SamplerBinder.PSBindSampler(effect.CacheData.SampEyeEnvMapBindPoint, SamplerStat);
            }

            //shadow map binding;
            if (effect.CacheData.BindingSlot_ShadowMap != UInt32.MaxValue)
            {
                TexBinder.PSBindTexture(effect.CacheData.BindingSlot_ShadowMap, mTex_ShadowMap);
            }

            if (effect.CacheData.BindingSlot_ShadowMapSampler != UInt32.MaxValue)
            {
                SamplerBinder.PSBindSampler(effect.CacheData.BindingSlot_ShadowMapSampler, mSampler_ShadowMap);
            }
        }
    }
}
