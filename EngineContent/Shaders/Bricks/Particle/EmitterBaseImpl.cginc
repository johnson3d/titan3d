#ifndef EMITTER_BASE_IMPL_CGINC
#define EMITTER_BASE_IMPL_CGINC

uint TtEmitter::Spawn(uint num, uint flags, float life)
{
    TtRawRingBuffer Allocator = GetAllocator();
    TtRawArray Alives = GetBackendAlives();
    
    uint numOfOK = 0;
    for (uint i = 0; i < num; i++)
    {
        int index = Allocator.Pop();
        if (index < 0)
            continue;
        Alives.Push(index);
        numOfOK++;
        
        bfParticles[index].Life = life;
        bfParticles[index].Flags = flags;
        bfParticles[index].Scale = 1.0f;
		
        if (IsParticleEmitShape(bfParticles[index]))
        {
            uint shapeIndex = GetParticleData(bfParticles[index].Flags); // % (uint)EmitterShapes.Count;
            DoParticleEmitShape(this, bfParticles[index], shapeIndex);
        }
        else
        {
            bfParticles[index].Location = float3(0, 0, 0);
        }
        bfParticles[index].Location += EmitterData.Location;
        OnInitParticle(this, bfParticles[index]);
    }
    return numOfOK;
};

uint TtEmitter::HasFlags(FParticle particle, uint flags)
{
    return (particle.Flags & (uint)flags);
}

uint TtEmitter::GetParticleData(uint flags)
{
    return (flags & (uint)(~EParticleFlags_FlagMask));
}

uint TtEmitter::SetParticleFlags(uint flags, uint data)
{
    return (uint)flags | (data & (~EParticleFlags_FlagMask));
}

FParticle TtEmitter::GetParticle(uint index)
{
    return bfParticles[index];
}

int TtEmitter::RandomNext(inout FParticle cur)
{
    uint index = cur.RandomSeed;
    cur.RandomSeed = rand_lcg(cur.RandomSeed);
    return index;
}

float TtEmitter::RandomUnit(inout FParticle cur)
{
    uint index = (cur.RandomSeed) % 65535;
    cur.RandomSeed = rand_lcg(cur.RandomSeed);
    return ((float) index / 65535.0f);
}

float TtEmitter::RandomSignedUnit(inout FParticle cur)//[-1,1]
{
    float result = RandomUnit(cur);
    return (result - 0.5f) * 2.0f;
}

float3 TtEmitter::RandomVector3(inout FParticle cur, bool normalized)
{
    float3 result;
    result.x = RandomSignedUnit(cur);
    result.y = RandomSignedUnit(cur);
    result.z = RandomSignedUnit(cur);
    if (normalized)
    {
        result = normalize(result);
    }
    return result;
}

float4 TtEmitter::RandomVector4(inout FParticle cur)
{
    float4 result;
    result.x = RandomSignedUnit(cur);
    result.y = RandomSignedUnit(cur);
    result.z = RandomSignedUnit(cur);
    result.w = RandomSignedUnit(cur);
    return result;
}

#endif//EMITTER_BASE_IMPL_CGINC