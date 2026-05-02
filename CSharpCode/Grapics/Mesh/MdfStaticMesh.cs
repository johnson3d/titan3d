using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMdfStaticMesh@EngineCore", "EngineNS.Graphics.Mesh.UMdfStaticMesh" })]
    public class TtMdfStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.TtStaticModifier>
    {
        
    }

    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMdfInstanceStaticMesh@EngineCore", "EngineNS.Graphics.Mesh.UMdfInstanceStaticMesh" })]
    public class TtMdfInstanceStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue2<Mesh.Modifier.TtStaticModifier, Modifier.TtInstanceModifier>
    {
        public TtMdfInstanceStaticMesh()
        {
            UpdateShaderCode();
        }
        public Modifier.TtInstanceModifier InstanceModifier
        {
            get => this.Modifiers[1] as Modifier.TtInstanceModifier;
        }
    }
}
