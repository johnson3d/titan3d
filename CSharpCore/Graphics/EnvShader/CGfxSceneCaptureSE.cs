using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Graphics.Mesh;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxSceneCaptureSE : CGfxShadingEnv
    {
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/SceneCapture.shadingenv");
            }
        }
        protected override void OnCreated()
        {
        }
        public override void BindResources(CGfxMesh mesh, CPass pass)
        {
        }
    }
}
