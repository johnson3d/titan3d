using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh.Modifier
{
    public class CStaticModifier : AuxPtrType<IStaticModifier>, Pipeline.Shader.IMeshModifier
    {
        public CStaticModifier()
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
    }
}
