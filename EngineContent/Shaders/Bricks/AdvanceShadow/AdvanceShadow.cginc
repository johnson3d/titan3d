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

    FAdvShadowLayerData LayerData[32];
};

int GetPageNode(float2 pos)
{
    if (any(pos < BoxMin) || any(pos >= BoxMax))
        return -1;
    
    float dist = length(pos - CameraPosition.xz);
    if (dist > MaxShadowDistance)
        return -1;
    int level = (int) ((MaxShadowDistance - dist) * MaxDeepLevel / MaxShadowDistance);
    float2 gridSize = LayerData[level].LayerGridSize;
    
    int2 sigment = (int2) ((pos - BoxMin) / gridSize);
    int2 layer = LayerData[level].LayerStartAndSide;
    int index = layer.x + (sigment.y * layer.y + sigment.x);
    if (index >= NodeCount)
        return -1;
    
    return index;
}

float GetESMValue(float linearDepth, float far, float c)
{
     // 方法 1：非线性压缩
    //float compressedDepth = log(linearDepth + 1.0);
    //float safeInput = min(compressedDepth * c, 80.0);
    
    // 方法 2：动态参数调整    
    float safeC = 80.0 / far;
    float safeInput = c * safeC * linearDepth;
    float esmValue = exp(safeInput);
    return esmValue;
}

#endif//_AdvanceShadow_cginc_