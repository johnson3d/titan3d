using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.CStaticModifier>
    {
        
    }

    public class UMdfInstanceStaticMesh : Graphics.Pipeline.Shader.TtMdfQueue2<Modifier.UInstanceModifier, Mesh.Modifier.CStaticModifier>
    {
        public UMdfInstanceStaticMesh()
        {
            InstanceModifier.SetMode(true);
            UpdateShaderCode();
        }
        public Modifier.UInstanceModifier InstanceModifier
        {
            get => this.Modifiers[0] as Modifier.UInstanceModifier;
        }
        public void SetInstantMode(bool bSSBO = true)
        {
            InstanceModifier.SetMode(bSSBO);
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, UMesh mesh, int atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, mesh, atom);
            InstanceModifier?.OnDrawCall(cmd, shadingType, drawcall, policy, mesh);
        }
    }
}
