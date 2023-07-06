#ifndef	_SOFT_RASTER_FRASTER_H_
#define _SOFT_RASTER_FRASTER_H_

#include "FRaster.cginc"

// TODO: ??
struct FClusterData
{
    float3 BoundCenter;
    int IndexStart;
    float3 BoundExtent;
    int IndexEnd;
    matrix WVPMatrix;
};

StructuredBuffer<FClusterData> ClusterBuffer;
RWByteAddressBuffer SrcClusterBuffer;
RWByteAddressBuffer VisClusterBuffer;

bool IsVisible(uint clusterIdx)
{
    //trasform ClusterBuffer[clusterIdx].BoundCenter to WorldCordinate
    
    //1.frustum culling aabb
    //2.hzb culling aabb
    
    return true;
}

groupshared uint MaxSrcCount;

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_ClusterCullingMain(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    if (LocalThreadId.x == 0)
    {
        MaxSrcCount = SrcClusterBuffer.Load(0);
    }
    GroupMemoryBarrierWithGroupSync();
    
    if (DispatchThreadId.x >= MaxSrcCount)
    {
        return;
    }
    
    uint clusterId = SrcClusterBuffer.Load((1 + DispatchThreadId.x) * 4);
    if (IsVisible(clusterId) == false)
    {
        return;
    }
    // TODO:
    int index = 0;
    VisClusterBuffer.InterlockedAdd(0, 1, index);
    //VisClusterBuffer&ClusterBuffer [0] is the count of array
    VisClusterBuffer.Store((1 + index) * 4, clusterId);
    //VisClusterBuffer.Store(4, 1);
}

#endif//_SOFT_RASTER_FRASTER_H_