using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public interface IEffector
    {
        string Name { get; }
        unsafe void DoEffect(IParticleEmitter emitter, float elapsed, void* particle);
        string GetParametersDefine();
        string GetHLSL();
        void SetCBuffer(uint index, RHI.CConstantBuffer CBuffer);
        IEffector CloneEffector();
    }
    public class UEffector<FParticle> : IEffector where FParticle : unmanaged
    {
        public virtual string Name
        {
            get { return "NullEffector"; }
        }
        public unsafe void DoEffect(IParticleEmitter emitter, float elapsed, void* particle)
        {
            DoEffect(emitter, elapsed, ref *(FParticle*)particle);
        }
        public unsafe virtual void DoEffect(IParticleEmitter emitter, float elapsed, ref FParticle particle)
        {

        }
        public virtual string GetParametersDefine()
        {
            return "";
        }
        public virtual string GetHLSL()
        {
            return "";
        }
        public virtual void SetCBuffer(uint index, RHI.CConstantBuffer CBuffer)
        {

        }
        public virtual IEffector CloneEffector()
        {
            return null;
        }
    }

    public class UAcceleratedEffector : IEffector
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FAcceleratedEffector
        {
            public Vector3 Acceleration;
            public uint FAcceleratedEffector_Pad0;
        };
        public virtual IEffector CloneEffector()
        {
            var result = new UAcceleratedEffector();
            result.mAcceleratedEffector = mAcceleratedEffector;
            return result;
        }
        public string Name
        {
            get { return "Accelerated"; }
        }
        FAcceleratedEffector mAcceleratedEffector;
        public Vector3 Acceleration
        {
            get => mAcceleratedEffector.Acceleration;
            set => mAcceleratedEffector.Acceleration = value;
        }        
        public virtual string GetParametersDefine()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine($"struct {Name}_EffectorParameters", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                codeBuilder.AddLine("float3 Acceleration;", ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);
            sourceCode += ";";

            return sourceCode;
        }
        public string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.FileManager.ReadAllText($"{RName.GetRName("Shaders\\Bricks\\Particle\\Effectors.compute", RName.ERNameType.Engine).Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            return sourceCode;
        }
        public unsafe void DoEffect(IParticleEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticleBase*)particle;
            cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
        }
        public void SetCBuffer(uint index, RHI.CConstantBuffer CBuffer)
        {
            CBuffer.SetValue($"EffectorParameters{index}", in mAcceleratedEffector);
        }
    }
}
