using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public interface IShaderCodeProvider
    {
        IO.CMemStreamWriter DefineCode { get; }
        IO.CMemStreamWriter SourceCode { get; }
    }
}
