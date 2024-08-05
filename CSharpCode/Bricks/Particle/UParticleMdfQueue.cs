using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TtParticleModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public TtParticleModifier()
        {
            if (UniqueText == null)
            {
                UniqueText = "";
                {
                //    var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
                //    string sourceCode = "";
                //    //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

                //    codeBuilder.AddLine($"struct FParticle", ref sourceCode);
                //    codeBuilder.PushSegment(ref sourceCode);
                //    {
                //        var members = typeof(FParticle).GetFields();
                //        foreach (var i in members)
                //        {
                //            codeBuilder.AddLine($"{TtEmitter.ToHLSLTypeString(i.FieldType)} {i.Name.Substring(1)};", ref sourceCode);
                //        }
                //    }
                //    codeBuilder.PopSegment(ref sourceCode);
                //    sourceCode += ";";
                //    ParticleVarCode.TextCode = sourceCode;
                //    UniqueText += sourceCode;
                //}
                //{
                //    var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
                //    string sourceCode = "";
                //    //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

                //    codeBuilder.AddLine($"struct FParticleEmitter", ref sourceCode);
                //    codeBuilder.PushSegment(ref sourceCode);
                //    {
                //        var members = typeof(FParticleEmitter).GetFields();
                //        foreach (var i in members)
                //        {
                //            codeBuilder.AddLine($"{TtEmitter.ToHLSLTypeString(i.FieldType)} {i.Name.Substring(1)};", ref sourceCode);
                //        }
                //    }
                //    codeBuilder.PopSegment(ref sourceCode);
                //    sourceCode += ";";
                //    ParticleEmitterCode.TextCode = sourceCode;

                //    UniqueText += sourceCode;
                }

                UniqueText = Hash160.CreateHash160(UniqueText).ToString();

                UEngine.Instance.RegFinalCleanupAction(() =>
                {
                    ParticleVarCode = null;
                    ParticleEmitterCode = null;
                    UniqueText = null;
                });
            }
        }
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
        static NxRHI.UShaderCode ParticleVarCode = new NxRHI.UShaderCode();
        static NxRHI.UShaderCode ParticleEmitterCode = new NxRHI.UShaderCode();
        static string UniqueText = null;
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            if (includeName.EndsWith("/ParticleVar"))
            {
                return ParticleVarCode.mCoreObject;
            }
            else if(includeName.EndsWith("/ParticleSystemVar"))
            {
                return ParticleEmitterCode.mCoreObject;
            }
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return UniqueText;
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return new Graphics.Pipeline.Shader.EPixelShaderInput[] {
                Graphics.Pipeline.Shader.EPixelShaderInput.PST_Color,
            };
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
                drawcall.BindSRV(drawcall.FindBinder("sbParticleInstance"), Emitter.GpuResources.ParticlesBuffer.Srv);
                drawcall.BindSRV(drawcall.FindBinder("sbAlives"), Emitter.GpuResources.CurAlivesBuffer.Srv);
                drawcall.BindCBuffer(drawcall.FindBinder("cbParticleDesc"), Emitter.CurrentQueue.CBuffer);

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
