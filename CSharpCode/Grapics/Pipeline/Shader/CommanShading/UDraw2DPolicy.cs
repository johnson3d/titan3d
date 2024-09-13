using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : TtRenderPolicy
    {
        public TtGraphicsShadingEnv mBasePassShading;
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, atom);
            mBasePassShading.OnDrawCall(cmd, drawcall, this, atom);
        }
    }
}
