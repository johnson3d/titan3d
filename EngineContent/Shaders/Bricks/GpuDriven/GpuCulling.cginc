#ifndef	_GPU_CULLING_H_
#define _GPU_CULLING_H_

#include "../../Inc/VertexLayout.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/BaseStructure/Frustum.cginc"
#include "../../Inc/BaseStructure/Quat.cginc"

cbuffer cbGPUCulling DX_AUTOBIND
{
    float3 BoundCenter;
    uint IndirectArgsOffset;
    float3 BoundExtent;
    uint MaxInstance;
    
    uint NumOfIndirectDraw;
    
    //uint Draw_IndexCountPerInstance;
    //uint Draw_StartIndexLocation;
    //uint Draw_BaseVertexLocation;
    //uint Draw_StartInstanceLocation;
};

StructuredBuffer<VSInstanceData> InstanceDataArray DX_AUTOBIND;
RWStructuredBuffer<VSInstanceData> CullInstanceDataArray DX_AUTOBIND;
RWBuffer<uint> IndirectArgsBuffer DX_AUTOBIND;

void PushInstance(VSInstanceData instance)
{
    uint index;
    InterlockedAdd(IndirectArgsBuffer[IndirectArgsOffset + 1], 1, index);
    //if (index >= MaxInstance)
    //    return;
    CullInstanceDataArray[index] = instance;
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_GPUCullingSetup(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    //IndirectArgsBuffer[0] = Draw_IndexCountPerInstance;
    IndirectArgsBuffer[IndirectArgsOffset + 1] = 0;
    //IndirectArgsBuffer[2] = Draw_StartIndexLocation;
    //IndirectArgsBuffer[3] = Draw_BaseVertexLocation;
    //IndirectArgsBuffer[4] = Draw_StartInstanceLocation;
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_GPUCullingFlush(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    if (DispatchThreadId.x >= NumOfIndirectDraw)
        return;
    IndirectArgsBuffer[IndirectArgsOffset + 5 * (DispatchThreadId.x + 1) + 1] = IndirectArgsBuffer[IndirectArgsOffset * 0 + 1];
}

float3 QuatRotateVec(in float3 inPos, in float4 inQuat)
{
    float3 uv = cross(inQuat.xyz, inPos);
    float3 uuv = cross(inQuat.xyz, uv);
    uv = uv * (2.0f * inQuat.w);
    uuv *= 2.0f;
	
    return inPos + uv + uuv;
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_GPUCullingMain(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    if (DispatchThreadId.x >= MaxInstance)
        return;
    TtFrustum frustum = TtFrustum::CreateByCamera();    
    VSInstanceData instance = InstanceDataArray[DispatchThreadId.x];
    TtQuat quat = TtQuat::CreateQuat(instance.Quat);
    //float3 extent = TtQuat::TransformedBoxAABB(BoundExtent, quat);
    float3 extent = max(max(BoundExtent.x, BoundExtent.y), BoundExtent.z);
    //float3 extent = abs(QuatRotateVec(BoundExtent, instance.Quat));
    float3 center = BoundCenter + instance.Position;
    if (frustum.IsOverlap6(center, extent) == false)
    {
        return;
    }
    PushInstance(instance);
}

#endif//_GPU_CULLING_H_