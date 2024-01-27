using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : URenderPolicy
    {
        public UGraphicsShadingEnv mBasePassShading;
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, atom);
            mBasePassShading.OnDrawCall(cmd, drawcall, this, atom);
        }
    }
}
