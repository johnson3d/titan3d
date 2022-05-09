using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using EngineNS.RHI;
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
        public override EVertexStreamType[] GetNeedStreams()
        {
            return new EVertexStreamType[] { EVertexStreamType.VST_Position,
                EVertexStreamType.VST_Normal,
                EVertexStreamType.VST_UV,};
        }
        public override void CopyFrom(Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            Emitter = (mdf as UParticleMdfQueue<FParticle, FParticleSystem>).Emitter;
        }
        public UEmitter<FParticle, FParticleSystem> Emitter;
        public override unsafe void OnDrawCall(URenderPolicy.EShadingType shadingType, CDrawCall drawcall, URenderPolicy policy, UMesh mesh)
        {
            if (Emitter != null)
            {
                drawcall.BindSrv("sbParticleInstance", Emitter.GpuResources.ParticlesSrv);
                drawcall.BindSrv("sbAlives", Emitter.GpuResources.CurAlivesSrv);
                
                if (Emitter.IsGpuDriven)
                {
                    drawcall.SetIndirectDraw(Emitter.GpuResources.DrawArgBuffer, 0);
                }
                else
                {
                    drawcall.SetInstanceNumber((int)Emitter.mCoreObject.GetLiveNumber());
                }
            }
            else
            {
                drawcall.SetInstanceNumber(1);
            }
        }
        public override Hash160 GetHash()
        {
            string CodeString = IO.FileManager.ReadAllText(RName.GetRName("shaders/Bricks/Particle/NebulaParticle.cginc", RName.ERNameType.Engine).Address);
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
            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(sourceCode);
        }
    }
}
