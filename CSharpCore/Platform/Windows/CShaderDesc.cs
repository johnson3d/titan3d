using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    partial class CShaderDesc
    {
#if Use_GLSLOptimizer
        partial void OptimizeGLES300_Windows(CShaderDesc desc)
        {
            switch (desc.ShaderType)
            {
                case EShaderType.EST_VertexShader:
                    {
                        using (var optimizer = new GLSLOptimizerSharp.GLSLOptimizer(GLSLOptimizerSharp.Target.OpenGLES30))
                        {
                            var result = optimizer.Optimize(GLSLOptimizerSharp.ShaderType.Vertex, desc.GLCode, GLSLOptimizerSharp.OptimizationOptions.None);
                            desc.SetGLCode(result.OutputCode);
                        }
                        break;
                    }
                case EShaderType.EST_PixelShader:
                    {
                        using (var optimizer = new GLSLOptimizerSharp.GLSLOptimizer(GLSLOptimizerSharp.Target.OpenGLES30))
                        {
                            var result = optimizer.Optimize(GLSLOptimizerSharp.ShaderType.Fragment, desc.GLCode, GLSLOptimizerSharp.OptimizationOptions.None);
                            desc.SetGLCode(result.OutputCode);
                        }
                        break;
                    }
            }
        }
#else
        
#endif

    }
}
