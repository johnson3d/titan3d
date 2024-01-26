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
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {

        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {

        }
    }
}
