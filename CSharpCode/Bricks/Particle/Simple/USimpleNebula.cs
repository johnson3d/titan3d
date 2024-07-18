using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Particle.Simple
{
    public class USimpleEmitter : Bricks.Particle.TtEmitter
    {
        public override unsafe void InitEmitter(NxRHI.UGpuDevice rc, Graphics.Mesh.TtMesh mesh, uint maxParticle)
        {
            SystemData.Flags = 0;
            base.InitEmitter(rc, mesh, maxParticle);
        }
        public override void DoUpdateSystem()
        {
            if (SystemData.Flags == 0)
            {
                mCoreObject.Spawn(512, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
                SystemData.Flags = 1;
            }
        }
        public unsafe override void OnInitParticle(FParticleBase* pParticles, ref FParticleBase particle)
        {
            if (HasFlags(in particle, EParticleFlags.EmitIndex) != 0)
            {
                var particleIndex = GetParticleData(particle.Flags);
                particle.Location = pParticles[particleIndex].Location;
                particle.Location.Y += 2.0f;
                //particle.mLocation.Z = RandomUnit() * 10.0f;
            }
            particle.Life += RandomUnit() * 0.5f;
            
            particle.Scale = 0.5f - RandomUnit() * 0.2f;
        }
        public override unsafe void OnDeadParticle(uint index, ref FParticleBase particle)
        {
            if (HasFlags(in particle, EParticleFlags.EmitShape) != 0)
            {
                uint shapeIndex = GetParticleData(particle.Flags);
                if (shapeIndex == 0)
                    mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 1), 5.0f);
                else
                    mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitShape, 0), 3.0f);
            }

            //mCoreObject.Spawn(1, SetParticleFlags(EParticleFlags.EmitIndex, index), 3.0f);
        }
    }
}
