//This is a ShaderAsset

void DoUpdateSystem(uint3 id, uint3 GroupId, uint3 GroupThreadId, uint GroupIndex)
{

}
#define USER_PARTICLE_UPDATESYS

void OnInitParticle(uint3 id, inout FParticle particle)
{
}
#define USER_PARTICLE_INITIALIZE

void OnDeadParticle(uint3 id, uint idxInPool, inout FParticle particle)
{
}
#define USER_PARTICLE_FINALIZE

void DoOnTimer(uint3 id, uint3 GroupId, uint3 GroupThreadId, uint GroupIndex, float second)
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
