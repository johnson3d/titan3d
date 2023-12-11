using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : URenderPolicy
    {
        public UGraphicsShadingEnv mBasePassShading;
        public override Shader.UGraphicsShadingEnv GetPassShading(EShadingType type, Mesh.TtMesh.TtAtom atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, atom);
            mBasePassShading.OnDrawCall(cmd, shadingType, drawcall, this, atom);
        }
    }
}
