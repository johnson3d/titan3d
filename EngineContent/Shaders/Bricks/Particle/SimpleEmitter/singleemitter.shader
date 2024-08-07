
void DoUpdateSystem(TtEmitter emt)
{
    //uint oldValue;
    //InterlockedCompareExchange(bfEmitterData[0].Flags, 0, 1, oldValue);
    //if (oldValue == 0)
    //{
    //    SpawnParticle((uint3) 0, 512, SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
    //}
    uint oldValue;
    InterlockedAdd(emt.EmitterDataRef[0].Flags, 1, oldValue);
    [branch]
    if (oldValue < 512)
    {
        emt.Spawn(1, emt.SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
    }
}

void OnInitParticle(TtEmitter emt, inout FParticle particle)
{
    float4 rdValue = emt.RandomVector4(particle); //RandomFloat4(id.x);
	particle.Life += (rdValue.w + 0.5f)* 0.5f ;

    uint index = emt.GetParticleData(particle.Flags);
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
        particle.Location = emt.GetParticle(index).Location;
    }
    //particle.Scale = 1.0f;
    particle.Velocity = emt.Velocity;
    particle.Color = emt.Color2Uint(rdValue);
}

void OnDeadParticle(TtEmitter emt, uint idxInPool, inout FParticle particle)
{
    if (emt.HasFlags(particle, EParticleFlags_EmitShape))
	{
        uint shapeIndex = emt.GetParticleData(particle.Flags);
		if (shapeIndex == 0)
		{
            emt.Spawn(1, emt.SetParticleFlags(EParticleFlags_EmitShape, 1), 5.0f);
        }
		else
		{
            emt.Spawn(1, emt.SetParticleFlags(EParticleFlags_EmitShape, 0), 3.0f);
        }
	}
    else
    {
        emt.Spawn(1, emt.SetParticleFlags(EParticleFlags_EmitShape, idxInPool), 3.0f);
    }
}

void OnParticleTick(TtEmitter emt, float elapsed, inout FParticle particle)
{
    half4 angles = emt.Uint2Color4f(particle.Rotator);
    angles.xz = float2(0, 0);
    angles.y += elapsed * 1.14f;
    particle.Rotator = emt.Color2Uint(angles);
}

void OnTimer(TtEmitter emt, float second)
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