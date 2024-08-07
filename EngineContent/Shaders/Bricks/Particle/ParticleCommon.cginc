#ifndef _PARTICLE_COMMON_H_
#define _PARTICLE_COMMON_H_
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/GpuSceneCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/Random.cginc"

bool IsParticleEmitShape(in FParticle cur)
{
    return (cur.Flags & EParticleFlags_EmitShape) != 0;
}

bool IsParticleEmitIndex(in FParticle cur)
{
    return (cur.Flags & EParticleFlags_EmitIndex) != 0;
}

struct UShapeBox
{
	float3		Center;
	float		Thinness;

	float3		HalfExtent;
	uint		FEmitShapeBox_Pad0;
};

struct UShapeSphere
{
	float3		Center;
	float		Radius;

	uint3		FEmitShapeSphere_Pad0;
	float		Thinness;
};

cbuffer cbParticleDesc DX_AUTOBIND
{
	float		ParticleElapsedTime;
	uint		ParticleRandomSeed;
	uint		ParticleRandomPoolSize;
	uint		ParticleMaxSize;
    
	uint		Draw_IndexCountPerInstance;
	uint		Draw_StartIndexLocation;
	uint		Draw_BaseVertexLocation;
	uint		Draw_StartInstanceLocation;

    uint AllocatorCapacity;
    uint CurAliveCapacity;
    uint BackendAliveCapacity;
    uint ParticleCapacity;
	
    float2 ParticleStartSecond_Pad;
    float ParticleStartSecond;
    uint OnTimerState;
	
    FParticleEmitter EmitterData;
#include "ParticleCBufferVar"
}

StructuredBuffer<float4>			bfRandomPool;
RWStructuredBuffer<FParticleEmitter> bfEmitterData;


#endif//#ifndef _PARTICLE_COMMON_H_