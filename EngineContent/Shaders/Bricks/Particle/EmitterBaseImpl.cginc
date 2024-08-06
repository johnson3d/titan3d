#ifndef EMITTER_BASE_IMPL_CGINC
#define EMITTER_BASE_IMPL_CGINC

void TtEmitter::Spawn(uint num, uint flags, float life)
{
    TtRawRingBuffer Allocator = GetAllocator();
    TtRawArray Alives = GetBackendAlives();
    
    [unroll]
    for (uint i = 0; i < num; i++)
    {
        int index = Allocator.Pop();
        if (index < 0)
            continue;
        Alives.Push(index);
		
        bfParticles[index].Life = life;
        bfParticles[index].Flags = flags;
        bfParticles[index].Scale = 1.0f;
		
        if (IsParticleEmitShape(bfParticles[index]))
        {
            uint shapeIndex = GetParticleData(bfParticles[index].Flags); // % (uint)EmitterShapes.Count;
            DoParticleEmitShape(ComputeEnv, bfParticles[index], shapeIndex);
        }
        else
        {
            bfParticles[index].Location = float3(0, 0, 0);
        }
        bfParticles[index].Location += EmitterData.Location;
        OnInitParticle(this, bfParticles[index]);
    }
};

uint TtEmitter::HasFlags(FParticle particle, uint flags)
{
    return (particle.Flags & (uint)flags);
}
        

#endif//EMITTER_BASE_IMPL_CGINC