
void DoUpdateSystem(TtEmitter emt)
{
    //uint oldValue;
    //InterlockedCompareExchange(bfSystemData[0].Flags, 0, 1, oldValue);
    //if (oldValue == 0)
    //{
    //    SpawnParticle((uint3) 0, 512, SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
    //}
    uint oldValue;
    InterlockedAdd(bfSystemData[0].Flags, 1, oldValue);
    [branch]
    if (oldValue < 512)
    {
        emt.Spawn(1, SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
    }
}

void OnInitParticle(TtEmitter emt, inout FParticle particle)
{
	float4 rdValue = RandomFloat4BySeed2(particle); //RandomFloat4(id.x);
	particle.Life += (rdValue.w + 0.5f)* 0.5f ;

    uint index = GetParticleData(particle.Flags);
	if (IsParticleEmitShape(particle))
    { //Index as EmitShap Type(0 or 1): SetParticleFlags(EParticleFlags_EmitShape, 1) SetParticleFlags(EParticleFlags_EmitShape, 0)
        if (index == 0)
		{
			particle.Scale = 0.5f - rdValue.z * 0.2f;
		}
		else
		{
			particle.Scale = 0.1f - rdValue.z * 0.2f;
		}
	}
	else
    { //Index as EmitIndex: SetParticleFlags(Nebula_EmitIndex, idxInPool)
		particle.Scale = 0.5f - rdValue.z * 0.2f;
        particle.Location = bfParticles[index].Location;
    }
    //particle.Scale = 1.0f;
    particle.Velocity = EmitterData.Velocity;
    particle.Color = ToColorUint(rdValue);

}

void OnDeadParticle(TtEmitter emt, uint idxInPool, inout FParticle particle)
{
    if (HasParticleFlags(particle, EParticleFlags_EmitShape))
	{
		uint shapeIndex = GetParticleData(particle.Flags);
		if (shapeIndex == 0)
		{
            emt.Spawn(1, SetParticleFlags(EParticleFlags_EmitShape, 1), 5.0f);
        }
		else
		{
            emt.Spawn(1, SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
        }
	}
    else
    {
        emt.Spawn(1, SetParticleFlags(EParticleFlags_EmitShape, idxInPool), 3.0f);
    }
}

void DoParticleTick(TtEmitter emt, float elapsed, inout FParticle particle)
{
    half4 angles = ToColor4f(particle.Rotator);
    angles.xz = float2(0, 0);
    angles.y += elapsed * 1.14f;
    particle.Rotator = ToColorUint(angles);
}

void DoOnTimer(TtEmitter emt, float second)
{
    TtReadonlyRawArray CurAlives = GetCurrentAlives();
    uint CountOfAlive = CurAlives.GetCount();
    if (emt.ComputeEnv.Id.x < CountOfAlive)
    {
        //uint index = GetParticleIndexByThreadID(id.x);
        uint index = CurAlives.GetValue(emt.ComputeEnv.Id.x);
        FParticle particle = bfParticles[index];
    }
}

#define USER_EMITTER