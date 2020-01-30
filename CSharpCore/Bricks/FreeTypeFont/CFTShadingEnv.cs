using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.FreeTypeFont
{
    public class CFTShadingEnv : EngineNS.Graphics.CGfxShadingEnv
    {
        public override void GetMacroDefines(List<CShaderDefinitions.MacroDefine> defs)
        {
            defs.Add(new CShaderDefinitions.MacroDefine("UserDef_ShadingEnv", "matrix RenderMatrix; " +
                                                                         "float TextOpacity = 1;" +
                                                                         "float4 TextColor = float4(1,1,1,1);"));
        }
        public override RName ShadingEnvName
        {
            get
            {
                return RName.GetRName("Shaders/font.shadingenv");
            }
        }
        protected override void OnCreated()
        {
            
        }
        public override void BindResources(Graphics.Mesh.CGfxMesh mesh, CPass pass)
        {
            var fmesh = mesh.Tag as CFontMesh; 
            if (fmesh != null)
            {
                if (fmesh.CBuffer == null)
                {
                    fmesh.CBuffer = CEngine.Instance.RenderContext.CreateConstantBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_ShadingEnv);
                    int TextMatrixId = fmesh.CBuffer.FindVar("RenderMatrix");
                    int TextOpacityId = fmesh.CBuffer.FindVar("TextOpacity");
                    int TextColorId = fmesh.CBuffer.FindVar("TextColor");

                    fmesh.CBuffer.SetValue(TextMatrixId, fmesh.RenderMatrix, 0);
                    fmesh.CBuffer.SetValue(TextOpacityId, fmesh.TextOpacity, 0);
                    fmesh.CBuffer.SetValue(TextColorId, fmesh.TextColor, 0);
                }
                
                pass.BindCBuffer(pass.GpuProgram, pass.Effect.CacheData.CBID_ShadingEnv, fmesh.CBuffer);
            }
        }
        public static CFTShadingEnv GetFTShadingEnv()
        {
            return CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv<CFTShadingEnv>();
        }
    }
}
