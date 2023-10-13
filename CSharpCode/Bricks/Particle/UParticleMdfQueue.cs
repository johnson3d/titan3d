using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class UParticleMdfQueue<FParticle, FParticleSystem> : Graphics.Pipeline.Shader.UMdfQueue 
        where FParticle : unmanaged
        where FParticleSystem : unmanaged
    {
        public UParticleMdfQueue()
        {
            UpdateShaderCode();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override void CopyFrom(Graphics.Pipeline.Shader.UMdfQueue mdf)
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
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("#ifndef _UParticlMdfQueue_Nebula_INC_", ref sourceCode);
            codeBuilder.AddLine("#define _UParticlMdfQueue_Nebula_INC_", ref sourceCode);
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/Bricks/Particle/NebulaParticle.cginc", RName.ERNameType.Engine).Address}\"", ref sourceCode);

            codeBuilder.AddLine("#endif", ref sourceCode);
            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = sourceCode;
        }
    }
}
