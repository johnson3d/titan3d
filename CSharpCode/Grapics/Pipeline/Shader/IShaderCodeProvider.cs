using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public interface IShaderCodeProvider
    {
        unsafe NxRHI.TtShaderCode DefineCode { get; }
        unsafe NxRHI.TtShaderCode SourceCode { get; }
    }
}
