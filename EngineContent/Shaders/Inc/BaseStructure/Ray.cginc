#ifndef _BaseStructure_Ray_Compute_H_
#define _BaseStructure_Ray_Compute_H_
#include "Box3.cginc"

struct TtRay
{
    float3 Pos;
    float3 Dir;
    
    // Box intersector adapted from https://www.shadertoy.com/view/ld23DV
    bool TestAABB(in float3 pos, in float3 extent)
    {
        float3 rayPos = Pos - pos;
        float3 n = rayPos / Dir;
        float3 k = extent / abs(Dir);
        float3 t1 = -k - n, t2 = k - n;
        float tN = max(max(t1.x, t1.y), t1.z);
        float tF = min(min(t2.x, t2.y), t2.z);
        return tN < tF && tF > 0.0;
    }
};

#endif//_BaseStructure_Ray_Compute_H_