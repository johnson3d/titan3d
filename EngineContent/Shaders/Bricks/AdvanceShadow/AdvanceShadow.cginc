#ifndef _AdvanceShadow_cginc_
#define _AdvanceShadow_cginc_
#include "../../Inc/GlobalDefine.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"

Texture2DArray GShadowMapArray DX_AUTOBIND;
StructuredBuffer<FAdvShadowNodeData> QTreeNodeBuffer DX_AUTOBIND;

cbuffer cbAdvanceShadow DX_AUTOBIND
{
    float2 BoxMin;
    float2 BoxMax;
    
    int NodeCount;
    int PageCount;
    float MaxShadowDistance;
    int MaxDeepLevel;
    
    int2 LayerStartAndSide[32];
    float2 LayerGridSize[32];
};

int GetPageNode(float2 pos)
{
    if (any(pos < BoxMin) || any(pos >= BoxMax))
        return -1;
    
    float dist = length(pos - CameraPosition.xz);
    if (dist > MaxShadowDistance)
        return -1;
    int level = (int) (dist * MaxDeepLevel / MaxShadowDistance);
    float2 gridSize = LayerGridSize[level];
    
    int2 sigment = (int2) ((pos - BoxMin) / gridSize);
    int2 layer = LayerStartAndSide[level];
    int index = layer.x + (sigment.y * layer.y + sigment.x);
    if (index >= NodeCount)
        return -1;
    
    return index;
}

#endif//_AdvanceShadow_cginc_