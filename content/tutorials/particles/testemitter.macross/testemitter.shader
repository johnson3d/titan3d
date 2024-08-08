void DoUpdateSystem(TtEmitter emt)
{
    uint tmp_r_SetParticleFlags_4124938284 = 0;
    uint tmp_r_Spawn_318270620 = 0;
    uint v_oldValue_AtomicAdd_EmitterFlags_2788520947 = 0;
    emt.AtomicAdd_EmitterFlags(1,v_oldValue_AtomicAdd_EmitterFlags_2788520947);
    if ((v_oldValue_AtomicAdd_EmitterFlags_2788520947 < 512))
    {
        tmp_r_SetParticleFlags_4124938284 = emt.SetParticleFlags(EParticleFlags_EmitShape,0);
        tmp_r_Spawn_318270620 = emt.Spawn(1,tmp_r_SetParticleFlags_4124938284,3);
    }
    else
    {
    }
}
void OnDeadParticle(TtEmitter emt,uint index,inout FParticle particle)
{
    uint tmp_r_GetParticleData_2624031083 = 0;
    uint tmp_r_SetParticleFlags_43658510 = 0;
    uint tmp_r_Spawn_2821917696 = 0;
    uint tmp_r_SetParticleFlags_2645294717 = 0;
    uint tmp_r_Spawn_4269439374 = 0;
    tmp_r_GetParticleData_2624031083 = emt.GetParticleData(particle.Flags);
    if ((tmp_r_GetParticleData_2624031083 == 0))
    {
        tmp_r_SetParticleFlags_43658510 = emt.SetParticleFlags(EParticleFlags_EmitShape,1);
        tmp_r_Spawn_2821917696 = emt.Spawn(1,tmp_r_SetParticleFlags_43658510,5);
    }
    else
    {
        tmp_r_SetParticleFlags_2645294717 = emt.SetParticleFlags(EParticleFlags_EmitShape,0);
        tmp_r_Spawn_4269439374 = emt.Spawn(1,tmp_r_SetParticleFlags_2645294717,3);
    }
}
void OnInitParticle(TtEmitter emt,inout FParticle particle)
{
    float tmp_r_RandomUnit_3537506712 = 0;
    float tmp_r_RandomUnit_2843124104 = 0;
    particle.Velocity = emt.Velocity;
    tmp_r_RandomUnit_3537506712 = emt.RandomUnit(particle);
    particle.Life = (tmp_r_RandomUnit_3537506712 + particle.Life);
    tmp_r_RandomUnit_2843124104 = emt.RandomUnit(particle);
    particle.Color = (uint)tmp_r_RandomUnit_2843124104;
    particle.Scale = 1;
}
void OnParticleTick(TtEmitter emt,float elapsed,inout FParticle particle)
{
    float4 Angles = float4(0,0,0,0);
    float4 tmp_r_Uint2Color4f_3287025661 = (float4)0;
    float4 tmp_r_CreateColor4f_1754197827 = (float4)0;
    uint tmp_r_Color2Uint_1566520051 = 0;
    tmp_r_Uint2Color4f_3287025661 = emt.Uint2Color4f(particle.Rotator);
    tmp_r_CreateColor4f_1754197827 = CreateColor4f(0,(tmp_r_Uint2Color4f_3287025661.g + (elapsed * 1.14)),0,0);
    tmp_r_Color2Uint_1566520051 = emt.Color2Uint(tmp_r_CreateColor4f_1754197827);
    particle.Rotator = tmp_r_Color2Uint_1566520051;
}
void OnTimer(TtEmitter emt,float second)
{
}

#define USER_EMITTER
