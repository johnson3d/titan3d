using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.TtStaticModifier>
    {
        
    }

    public class UMdfInstanceStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue2<Mesh.Modifier.TtStaticModifier, Modifier.TtInstanceModifier>
    {
        public UMdfInstanceStaticMesh()
        {
            UpdateShaderCode();
        }
        public Modifier.TtInstanceModifier InstanceModifier
        {
            get => this.Modifiers[1] as Modifier.TtInstanceModifier;
        }
    }
}
