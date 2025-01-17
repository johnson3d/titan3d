using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class TtMeshletModifier : Pipeline.Shader.IMeshModifier
    {
        public TtMeshletModifier()
        {
            
        }
        public void Dispose()
        {

        }

        public string ModifierNameVS { get => "DoMeshletModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/MeshletModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return "";
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] {
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {

        }
        public void OnBuildDrawCall(Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall, Mesh.TtMesh.TtAtom atom)
        {
            Meshlets = atom.MeshPrimitives.Meshlets;
        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            if (Meshlets == null)
                return;
            var indexer = drawcall.Effect.GetTypedBindIndexer<TtMdfMeshletBinderIndexer>();
            var binder = indexer.MeshletsBuffer;
            if (binder != null)
            {
                drawcall.BindSRV(binder, Meshlets.MeshLetsBuffer.Srv);
            }
            binder = indexer.VerticesBuffer;
            if (binder != null)
            {
                drawcall.BindSRV(binder, Meshlets.VerticesBuffer.Srv);
            }
            binder = indexer.TrianglesBuffer;
            if (binder != null)
            {
                drawcall.BindSRV(binder, Meshlets.TrianglesBuffer.Srv);
            }
            //drawcall.DrawInstance = 37;
            //drawcall.BindIndirectDrawArgsBuffer()
        }
        public class TtMdfMeshletBinderIndexer : NxRHI.TtShader.AuxShaderBinderIndexer<TtMdfMeshletBinderIndexer>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder MeshletsBuffer;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder VerticesBuffer;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder TrianglesBuffer;
        }
        public Bricks.GpuDriven.TtMeshlets Meshlets;
    }

    public class TtMdfMeshlet : Graphics.Pipeline.Shader.TtMdfQueue1<TtMeshletModifier>
    {
        public TtMeshletModifier MeshletModifier
        {
            get
            {
                return this.Modifiers[0] as TtMeshletModifier;
            }
        }
    }
}
