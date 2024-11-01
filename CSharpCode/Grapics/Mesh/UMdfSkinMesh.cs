using EngineNS.Graphics.Pipeline.Shader;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfSkinMesh : Graphics.Pipeline.Shader.TtMdfQueue1<Mesh.Modifier.TtSkinModifier>
    {
        public Mesh.Modifier.TtSkinModifier SkinModifier
        {
            get
            {
                return this.Modifiers[0] as Mesh.Modifier.TtSkinModifier;
            }
        }
        public NxRHI.TtCbView PerSkinMeshCBuffer { get; set; } = null;
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            if(mdf is UMdfSkinMesh mdfSkin)
            {
                PerSkinMeshCBuffer = mdfSkin.PerSkinMeshCBuffer;
            }
        }
        public class TtSkinMeshBinderIndexer : NxRHI.TtShader.AuxShaderBinderIndexer<TtSkinMeshBinderIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbSkinMesh;
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Pipeline.TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var binder = drawcall.Effect.GetTypedBindIndexer<TtSkinMeshBinderIndexer>().cbSkinMesh;
            if (binder == null || PerSkinMeshCBuffer == null)
            {
                return;
            }
            drawcall.BindCBuffer(binder, PerSkinMeshCBuffer);
        }
    }
}
