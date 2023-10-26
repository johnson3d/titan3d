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
    }

    public class UParticleMdfQueue<FParticle, FParticleSystem> : Graphics.Pipeline.Shader.TtMdfQueue1<TtParticleModifier>
        where FParticle : unmanaged
        where FParticleSystem : unmanaged
    {
        public override void CopyFrom(Graphics.Pipeline.Shader.TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            Emitter = (mdf as UParticleMdfQueue<FParticle, FParticleSystem>).Emitter;
        }
        public UEmitter<FParticle, FParticleSystem> Emitter;
        public override unsafe void OnDrawCall(NxRHI.ICommandList cmd, URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, URenderPolicy policy, UMesh mesh, int atom)
        {
            base.OnDrawCall(cmd, shadingType, drawcall, policy, mesh, atom);

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
        public override Hash160 GetHash()
        {
            string CodeString = IO.TtFileManager.ReadAllText(RName.GetRName("shaders/Bricks/Particle/NebulaParticle.cginc", RName.ERNameType.Engine).Address);
            mMdfQueueHash = Hash160.CreateHash160(CodeString);
            return mMdfQueueHash;
        }
    }
}
