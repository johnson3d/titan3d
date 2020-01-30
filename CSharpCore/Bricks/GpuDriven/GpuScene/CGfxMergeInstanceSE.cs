using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics;

namespace EngineNS.Bricks.GpuDriven.GpuScene
{
    public class CGfxMergeInstanceSE : EngineNS.Graphics.CGfxShadingEnv
    {
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/Compute/GpuDriven/MergeInstance.shadingenv", RName.enRNameType.Engine);
            }
        }
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            base.GetMacroDefines(defs);
            defs.Add(new CShaderDefinitions.MacroDefine("ENV_USEVTF", "0"));
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
                case "ENV_USEVTF":
                    {
                        var result = new List<string>();
                        result.Add("0");
                        result.Add("1");
                        return result;
                    }
            }
            return null;
        }
    }
}
