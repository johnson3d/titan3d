using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle.Simple
{
    public class TtSimpleEmitter : Bricks.Particle.TtEmitter
    {
        public override RName GetEmitterShader()
        {
            return RName.GetRName("Shaders/Bricks/Particle/SimpleEmitter/Emitter.compute", RName.ERNameType.Engine);
        }
        public override unsafe void InitEmitter(NxRHI.TtGpuDevice rc, Graphics.Mesh.TtMesh mesh, uint maxParticle)
        {
            EmitterData.Flags = 0;
            base.InitEmitter(rc, mesh, maxParticle);
        }
        public override void DoUpdateSystem()
        {
            if (EmitterData.Flags == 0)
            {
                Spawn(512, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
                EmitterData.Flags = 1;
            }
        }
        public unsafe override void OnInitParticle(ref FParticle particle)
        {
            if (HasFlags(in particle, EParticleFlags.EmitIndex) != 0)
            {
                var particleIndex = GetParticleData(particle.Flags);
                particle.Location = GetParticle(particleIndex).Location;
                particle.mLocation.Y += 2.0f;
                //particle.mLocation.Z = RandomUnit() * 10.0f;
            }
            particle.Life += RandomUnit(ref particle) * 0.5f;
            particle.Velocity = Velocity;
            particle.Scale = 0.5f - RandomUnit(ref particle) * 0.2f;
            particle.Color = ((uint)RandomNext(ref particle) | 0xff000000);
        }
        public override unsafe void OnDeadParticle(uint index, ref FParticle particle)
        {
            if (HasFlags(in particle, EParticleFlags.EmitShape) != 0)
            {
                uint shapeIndex = GetParticleData(particle.Flags);
                if (shapeIndex == 0)
                    Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 1), 5.0f);
                else
                    Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
            }

            //mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitIndex, index), 3.0f);
        }
        public override void OnParticleTick(TtEmitter emitter, float elapsed, ref FParticle particle)
        {
            //base.OnParticleTick(emitter, elapsed, ref particle);
            var rf = particle.Rotatorf;
            rf.Y += 1.14f * elapsed;
            particle.Rotatorf = rf;
        }
    }

    [Bricks.CodeBuilder.ContextMenu(filterStrings: "SimpleEmitter", "Emitter\\SimpleEmitter", Editor.TtParticleGraph.NebulaEditorKeyword)]
    public class TtSimpleEmitterNode : Editor.TtEmitterNode
    {
        public TtSimpleEmitterNode()
        {
            Name = "SimpleEmitter";
        }
        public override TtTypeDesc CreateEmitterType()
        {
            return TtTypeDescGetter<TtSimpleEmitter>.TypeDesc;
        }
    }
}
