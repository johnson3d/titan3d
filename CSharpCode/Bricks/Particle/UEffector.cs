using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TtEffector
    {
        public virtual string Name
        {
            get { return "NullEffector"; }
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            DoEffect(emitter, elapsed, ref *(FParticleBase*)particle);
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, ref FParticleBase particle)
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
        public virtual void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {

        }
        public virtual TtEffector CloneEffector()
        {
            return null;
        }
    }

    public class TtAcceleratedEffector : TtEffector
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FAcceleratedEffector
        {
            public Vector3 Acceleration;
            public uint FAcceleratedEffector_Pad0;
        };
        public override TtEffector CloneEffector()
        {
            var result = new TtAcceleratedEffector();
            result.mAcceleratedEffector = mAcceleratedEffector;
            return result;
        }
        public override string Name
        {
            get { return "Accelerated"; }
        }
        FAcceleratedEffector mAcceleratedEffector;
        public Vector3 Acceleration
        {
            get => mAcceleratedEffector.Acceleration;
            set => mAcceleratedEffector.Acceleration = value;
        }        
        public override string GetParametersDefine()
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
        public override string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{RName.GetRName("Shaders\\Bricks\\Particle\\Effectors.compute", RName.ERNameType.Engine).Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            return sourceCode;
        }
        public override unsafe void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticleBase*)particle;
            cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
        }
        public override void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue($"EffectorParameters{index}", in mAcceleratedEffector);
        }
    }
}
