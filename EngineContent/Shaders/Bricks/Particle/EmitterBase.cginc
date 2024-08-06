#ifndef EMITTER_BASE_CGINC
#define EMITTER_BASE_CGINC

struct TtEmitter
{
    FComputeEnv ComputeEnv;
    
    //recomment num>0 && num<10
    void Spawn(uint num, uint flags, float life);
    uint HasFlags(FParticle particle, uint flags);
};

#endif//EMITTER_BASE_CGINC