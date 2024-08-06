//This is a ShaderAsset

void DoUpdateSystem(FComputeEnv env)
{

}
#define USER_PARTICLE_UPDATESYS

void OnInitParticle(FComputeEnv env, inout FParticle particle)
{
}
#define USER_PARTICLE_INITIALIZE

void OnDeadParticle(FComputeEnv env, uint idxInPool, inout FParticle particle)
{
}
#define USER_PARTICLE_FINALIZE

void DoParticleTick(FComputeEnv env, float elapsed, inout FParticle particle)
{
    
}
#define USER_PARTICLE_DOTICK

void DoOnTimer(FComputeEnv env, float second)
{
    TtReadonlyRawArray CurAlives = GetCurrentAlives();
    uint CountOfAlive = CurAlives.GetCount();
    if (id.x < CountOfAlive)
    {
        uint index = CurAlives.GetValue(id.x);
        FParticle particle = bfParticles[index];
    }
}
#define USER_PARTICLE_ONTIMER
