#ifndef _BaseStructure_Frustum_Compute_H_
#define _BaseStructure_Frustum_Compute_H_
#include "Box3.cginc"

struct TtFrustum
{
    float4 PlanesX;
    float4 PlanesY;
    float4 PlanesZ;
    float4 PlanesW;
    
    float4 NearPlane;
    float4 FarPlane;
    
    bool Cull(float3 BoundsCenter, float3 BoundsExtent)
    {
        float4 OrigX = BoundsCenter.xxxx;
        float4 OrigY = BoundsCenter.yyyy;
        float4 OrigZ = BoundsCenter.zzzz;

        float3 AbsExt = abs(BoundsExtent);
        float4 AbsExtentX = AbsExt.xxxx;
        float4 AbsExtentY = AbsExt.yyyy;
        float4 AbsExtentZ = AbsExt.zzzz;

    // Calculate the distance (x * x) + (y * y) + (z * z) - w
        float4 DistX = OrigX * PlanesX;
        float4 DistY = OrigY * PlanesY + DistX;
        float4 DistZ = OrigZ * PlanesZ + DistY;
        float4 Distance = DistZ - PlanesW;
		// Now do the push out FMath::Abs(x * x) + FMath::Abs(y * y) + FMath::Abs(z * z)
        float4 PushX = AbsExtentX * abs(PlanesX);
        float4 PushY = AbsExtentY * abs(PlanesY) + PushX;
        float4 PushOut = AbsExtentZ * abs(PlanesZ) + PushY;

		// Check for completely outside
        if (any(Distance > PushOut))
        {
            return false;
        }

        return true;
    }
};

#endif//_BaseStructure_Frustum_Compute_H_