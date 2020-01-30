using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.EnvShader
{
    public class CGfxGizmosSE : CGfxShadingEnv
    {
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {

        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/Gizmos.shadingenv");
            }
        }
        protected override void OnCreated()
        {
             
        }

        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {

        }
    }
}
