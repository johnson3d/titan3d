#ifndef	_SOFT_RASTER_FRASTER_H_
#define _SOFT_RASTER_FRASTER_H_

#include "FRaster.cginc"

StructuredBuffer<int> SrcClusterBuffer;
RWStructuredBuffer<int> VisClusterBuffer;

bool IsVisible(int clusterIdx)
{
    //trasform ClusterBuffer[clusterIdx].BoundCenter to WorldCordinate
    
    //1.frustum culling aabb
    //2.hzb culling aabb
    
    return true;
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_ClusterCullingMain(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    //if (GroupId.x = 0)
    //{
    //    VisClusterBuffer[0] = 0;
    //}
    //GroupMemoryBarrierWithGroupSync();
    
    if (IsVisible(DispatchThreadId.x) == false)
    {
        return;
    }
    int index = 0;
    InterlockedAdd(VisClusterBuffer[0], 1, index);
    //VisClusterBuffer&DispatchThreadId [0] is the count of array
    VisClusterBuffer[1 + index] = SrcClusterBuffer[1 + DispatchThreadId.x];
}

#endif//_SOFT_RASTER_FRASTER_H_