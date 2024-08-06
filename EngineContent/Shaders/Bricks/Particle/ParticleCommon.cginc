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
    uint OnTimer;
	
    FParticleEmitter EmitterData;
#include "ParticleCBufferVar"
}

StructuredBuffer<float4>			bfRandomPool;
RWStructuredBuffer<FParticleEmitter> bfSystemData;

float4 RandomFloat4(uint id)
{
	uint index = (ParticleRandomPoolSize + id) % ParticleRandomPoolSize;
	return bfRandomPool[index];
}

float RandomFloatBySeed(inout FParticle cur)
{
	uint index = (cur.RandomSeed) % ParticleRandomPoolSize;
	uint remain = cur.RandomSeed % 4;
	cur.RandomSeed = rand_lcg(cur.RandomSeed);
	return bfRandomPool[index][remain];
}

float RandomFloatBySeedSignedUnit(inout FParticle cur)
{
	uint index = (cur.RandomSeed) % 65535;
	cur.RandomSeed = rand_lcg(cur.RandomSeed);
	return ((float)index / 65535.0f);
}

float RandomFloatBySeed2(inout FParticle cur)
{
	uint index = (cur.RandomSeed) % 65535;
	cur.RandomSeed = rand_lcg(cur.RandomSeed);
	return ((float)index / 65535.0f - 0.5f) * 2.0f;
}

float4 RandomFloat4BySeed(inout FParticle cur)
{
	uint index = (cur.RandomSeed) % ParticleRandomPoolSize;
	cur.RandomSeed = rand_lcg(cur.RandomSeed);
	return bfRandomPool[index];
}

float4 RandomFloat4BySeed2(inout FParticle cur)
{
	float4 result;
	result.x = RandomFloatBySeed2(cur);
	result.y = RandomFloatBySeed2(cur);
	result.z = RandomFloatBySeed2(cur);
	result.w = RandomFloatBySeed2(cur);
	return result;
}

float4 RandomFloat4BySeed3(inout FParticle cur)
{
	float4 result;
	result.x = RandomFloatBySeed(cur);
	result.y = RandomFloatBySeed(cur);
	result.z = RandomFloatBySeed(cur);
	result.w = RandomFloatBySeed(cur);
	return result;
}

uint GetParticleData(uint flags)
{
    return (flags & (~EParticleFlags_FlagMask));
}
uint SetParticleFlags(uint flags, uint data)
{
    return (uint) flags | (data & (~EParticleFlags_FlagMask));
}
bool HasParticleFlags(FParticle particle, uint flags)
{
	return (particle.Flags & flags) == flags;
}

#endif//#ifndef _PARTICLE_COMMON_H_