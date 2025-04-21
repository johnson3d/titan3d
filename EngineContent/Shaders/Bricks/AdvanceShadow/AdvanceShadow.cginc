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
    
    float EsmConstant; // 指数系数（越大阴影越“硬”）
    float MaxExp;
    float GaussSigma;

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

//https://blog.csdn.net/Jaihk662/article/details/127259797
float GetESMValue(float sampleDepth, float near, float far)
{
    float linearDepth = LinearFromDepth(sampleDepth, near, far);
    ///linearDepth = (linearDepth - near) / (far - near);
    
    //linearDepth = 1 - sampleDepth;
     // 方法 1：非线性压缩
    //float compressedDepth = log(linearDepth + 1.0);
    //float safeInput = min(compressedDepth * c, 80.0);
    
    // 方法 2：动态参数调整    
    //float safeC = 11.0 / far;
    float safeC = 1.0f;
    float safeInput = min(EsmConstant * safeC * linearDepth, MaxExp);
    //float safeInput = EsmConstant * linearDepth;
    float esmValue = exp(safeInput);
    
    return esmValue;
}

#endif//_AdvanceShadow_cginc_