using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxUFOMobileOutlineNprSE : CGfxShadingEnv
    {
        //public CShaderResourceView mEnvMap;
        
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/UFOMobileOutlineNprSE.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            //AddSRV("gEnvMap");
             
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            //CTextureBindInfo info = new CTextureBindInfo(); //this is a struct,so you can use new as convenient;
            //if (Shader.FindTextureBindInfo(null, "gEnvMap", ref info))
            //{
            //    TexBinder.PSBindTexture(info.PSBindPoint, mEnvMap);
            //}

            //var SamplerBindInfo = new CSamplerBindInfo();
            //if (Shader.FindSamplerBindInfo(null, "Samp_gEnvMap", ref SamplerBindInfo) == true)
            //{
            //    var SamplerStatDesc = new CSamplerStateDesc();
            //    SamplerStatDesc.SetDefault();
            //    SamplerStatDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
            //    var SamplerStat = CEngine.Instance.SamplerStateManager.GetSamplerState(CEngine.Instance.RenderContext, SamplerStatDesc);

            //    SamplerBinder.PSBindSampler(SamplerBindInfo.PSBindPoint, SamplerStat);
            //}
            
        }
    }
}
