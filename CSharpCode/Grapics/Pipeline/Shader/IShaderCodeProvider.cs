using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public interface IShaderCodeProvider
    {
        unsafe NxRHI.UShaderCode DefineCode { get; }
        unsafe NxRHI.UShaderCode SourceCode { get; }
    }
}
