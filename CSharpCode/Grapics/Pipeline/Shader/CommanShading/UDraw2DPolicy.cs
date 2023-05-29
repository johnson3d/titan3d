using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : URenderPolicy
    {
        public UGraphicsShadingEnv mBasePassShading;
        public override Shader.UGraphicsShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.UMesh mesh, int atom)
        {
            mBasePassShading.OnDrawCall(shadingType, drawcall, this, mesh);
        }
    }
}
