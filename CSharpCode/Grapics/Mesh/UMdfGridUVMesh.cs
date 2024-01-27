using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Graphics.Mesh
{
    public class TtGridUVModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public void Dispose()
        {

        }
        public string ModifierNameVS { get => "DoGridUVModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/modifier/GridUVModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_LightMap};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return new Graphics.Pipeline.Shader.EPixelShaderInput[] {
                Graphics.Pipeline.Shader.EPixelShaderInput.PST_UV
            };
        }
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {

        }
        public unsafe void OnDrawCall(TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            UMdfGridUVMesh mdfQueue = mdfQueue1 as UMdfGridUVMesh;
            var binder = drawcall.FindBinder("cbGridUVMesh");
            if (binder.IsValidPointer == false)
            {
                return;
            }
            if (mdfQueue.PerGridUVMeshCBuffer == null)
            {
                mdfQueue.PerGridUVMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }
            drawcall.BindCBuffer(binder, mdfQueue.PerGridUVMeshCBuffer);
        }
    }

    public class UMdfGridUVMesh : Graphics.Pipeline.Shader.TtMdfQueue1<TtGridUVModifier>
    {
        public NxRHI.UCbView PerGridUVMeshCBuffer { get; set; }
        public void SetUVMinAndMax(in Vector2 min, in Vector2 max)
        {
            if (PerGridUVMeshCBuffer == null)
                return;
            PerGridUVMeshCBuffer.SetValue("UVMin", in min);
            PerGridUVMeshCBuffer.SetValue("UVMax", in max);
        }
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            PerGridUVMeshCBuffer = (mdf as UMdfGridUVMesh).PerGridUVMeshCBuffer;
        }
    }
}
