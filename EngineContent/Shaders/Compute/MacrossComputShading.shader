#ifndef _MACROSS_COMPUTE_SHADING_H_
#define _MACROSS_COMPUTE_SHADING_H_
#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"
#include "../Inc/Math.cginc"
#include "../Inc/Random.cginc"
#include "MacrossComputeShadingEnv.cginc"

#include "@MacrossShader"

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_Main(uint3 Id : SV_DispatchThreadID,
	uint3 GroupId : SV_GroupID,
	uint3 GroupThreadId : SV_GroupThreadID,
	uint GroupIndex : SV_GroupIndex)
{
    TtMacrossComputeShadingEnv emt;
    emt.ComputeEnv.Id = Id;
    emt.ComputeEnv.GroupId = GroupId;
    emt.ComputeEnv.GroupThreadId = GroupThreadId;
    emt.ComputeEnv.GroupIndex = GroupIndex;
    
    CSMacrossShaderMain(emt);
}

#include "MacrossComputeShadingEnvImpl.cginc"

#endif//#ifndef _MACROSS_COMPUTE_SHADING_H_