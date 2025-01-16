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
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {

        }
    }
}
