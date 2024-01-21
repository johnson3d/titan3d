#ifndef	_GPU_CULLING_H_
#define _GPU_CULLING_H_

#include "../../Inc/VertexLayout.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/BaseStructure/Frustum.cginc"

cbuffer cbGPUCulling DX_AUTOBIND
{
    float3 BoundCenter;
    uint IndirectArgsOffset;
    float3 BoundExtent;
    uint MaxInstance;
    
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
void CS_GPUCullingMain(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    if (DispatchThreadId.x >= MaxInstance)
        return;
    TtFrustum frustum = (TtFrustum) 0;
    frustum.PlanesX = ClipPlanesX;
    frustum.PlanesY = ClipPlanesY;
    frustum.PlanesZ = ClipPlanesZ;
    frustum.PlanesW = ClipPlanesW;
    
    if (frustum.Cull(BoundCenter, BoundExtent) == false)
    {
        return;
    }
    PushInstance(InstanceDataArray[DispatchThreadId.x]);
}

#endif//_GPU_CULLING_H_