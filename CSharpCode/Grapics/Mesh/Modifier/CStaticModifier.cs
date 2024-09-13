using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class TtStaticModifier : AuxPtrType<IStaticModifier>, Pipeline.Shader.IMeshModifier
    {
        public TtStaticModifier()
        {
            mCoreObject = IStaticModifier.CreateInstance();
        }
        public string ModifierNameVS { get => null; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return null;
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return null;
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
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return "";
        }
    }
}
