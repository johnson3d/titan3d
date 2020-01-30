using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxUFOMobileTranslucentSE : CGfxShadingEnv
    {
        public CShaderResourceView mEnvMap;
        CSamplerState SamplerStat;
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/UFOMobileTranslucentSE.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            AddSRV("gEnvMap");

            var SamplerStatDesc = new CSamplerStateDesc();
            SamplerStatDesc.SetDefault();
            SamplerStatDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
            SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, SamplerStatDesc);
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var TexBinder = pass.ShaderResources;
            var SamplerBinder = pass.ShaderSamplerBinder;
            var Shader = pass.Effect;

            if (Shader.CacheData.EnvMapBindPoint != UInt32.MaxValue)
                TexBinder.PSBindTexture(Shader.CacheData.EnvMapBindPoint, mEnvMap);

            if (Shader.CacheData.SampEnvMapBindPoint != UInt32.MaxValue)
                SamplerBinder.PSBindSampler(Shader.CacheData.SampEnvMapBindPoint, SamplerStat);
        }
    }
}
