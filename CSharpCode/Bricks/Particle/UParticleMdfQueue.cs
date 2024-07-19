using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TtParticleModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public void Dispose()
        {

        }
        public string ModifierNameVS { get => "DoNebulaModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/Bricks/Particle/NebulaParticle.cginc", RName.ERNameType.Engine);
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {

        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, TtMesh.TtAtom atom)
        {

        }
    }

    public class TtParticleMdfQueue : Graphics.Pipeline.Shader.TtMdfQueue1<TtParticleModifier>
    {
        public override void CopyFrom(Graphics.Pipeline.Shader.TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            Emitter = (mdf as TtParticleMdfQueue).Emitter;
        }
        public TtEmitter Emitter;
        public override unsafe void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            if (Emitter != null)
            {
                drawcall.BindSRV(drawcall.FindBinder("sbParticleInstance"), Emitter.GpuResources.ParticlesSrv);
                drawcall.BindSRV(drawcall.FindBinder("sbAlives"), Emitter.GpuResources.CurAlivesSrv);
                var binder = drawcall.FindBinder("cbForMultiDraw");
                if (binder.IsValidPointer)
                {
                    if (Emitter.GpuResources.DrawIdBuffer == null)
                    {
                        Emitter.GpuResources.DrawIdBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    }
                    
                    drawcall.BindCBuffer(binder, Emitter.GpuResources.DrawIdBuffer);
                }   


                if (Emitter.IsGpuDriven)
                {
                    drawcall.BindIndirectDrawArgsBuffer(Emitter.GpuResources.DrawArgBuffer, 0);
                }
                else
                {
                    drawcall.DrawInstance = Emitter.mCoreObject.GetLiveNumber();
                }
            }
            else
            {
                drawcall.DrawInstance = 1;
            }
        }
    }
}
