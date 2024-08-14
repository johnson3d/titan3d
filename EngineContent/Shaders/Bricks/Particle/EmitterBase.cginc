#ifndef EMITTER_BASE_CGINC
#define EMITTER_BASE_CGINC

struct TtEmitter
{
    FComputeEnv ComputeEnv;
    float3 Location;
    float3 Velocity;
    RWStructuredBuffer<FParticleEmitter> EmitterDataRef;
    void InitMembers(FParticleEmitter data, RWStructuredBuffer<FParticleEmitter> bfEmitterData)
    {
        Location = data.Location;
        Velocity = data.Velocity;
        EmitterDataRef = bfEmitterData;
    }
    
    //recomment num>0 && num<10
    uint Spawn(uint num, uint flags, float life);
    uint HasFlags(FParticle particle, uint flags);
    uint GetParticleData(uint flags);
    uint SetParticleFlags(uint flags, uint data);
    int RandomNext(inout FParticle cur);
    float RandomUnit(inout FParticle cur); //[0,1]
    float RandomSignedUnit(inout FParticle cur); //[-1,1]
    float3 RandomVector3(inout FParticle cur, bool normalized);
    float4 RandomVector4(inout FParticle cur);
    FParticle GetParticle(uint index);
    
    uint Color2Uint(float4 color)
    {
        return ToColorUint(color);
    }
    float4 Uint2Color4f(uint value)
    {
        return ToColor4f(value);
    }
};

#endif//EMITTER_BASE_CGINC